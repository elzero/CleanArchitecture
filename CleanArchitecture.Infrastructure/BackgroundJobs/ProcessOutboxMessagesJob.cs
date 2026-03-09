using CleanArchitecture.Application.Abstractions.Data;
using CleanArchitecture.Domain.Abstractions;
using Dapper;
using MediatR;
using Newtonsoft.Json;
using Quartz;

namespace CleanArchitecture.Infrastructure.BackgroundJobs;

// DisallowConcurrentExecution prevents the same job from running multiple times concurrently.
[DisallowConcurrentExecution]
public sealed class ProcessOutboxMessagesJob : IJob
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IPublisher _publisher;

    public ProcessOutboxMessagesJob(
        ISqlConnectionFactory sqlConnectionFactory,
        IPublisher publisher)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _publisher = publisher;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        // 1. Fetch unprocessed messages with FOR UPDATE SKIP LOCKED
        // FOR UPDATE enforces row-level locks, so another instance looking for the same rows will wait
        // SKIP LOCKED tells other instances to skip locked rows and grab the next 20
        const string sql = """
            SELECT
                Id,
                Content
            FROM OutboxMessages
            WHERE ProcessedOnUtc IS NULL
            ORDER BY OccurredOnUtc
            LIMIT 20
            FOR UPDATE SKIP LOCKED
            """;

        var messages = await connection.QueryAsync<OutboxMessageResponse>(
            sql, 
            transaction: transaction);

        var outboxMessageResponses = messages.ToList();
        if (outboxMessageResponses.Count == 0)
        {
            transaction.Commit();
            return;
        }

        // 2. Publish domain events via MediatR
        foreach (var outboxMessage in outboxMessageResponses)
        {
            var domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
                outboxMessage.Content,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });

            if (domainEvent is null)
            {
                continue;
            }

            try
            {
                await _publisher.Publish(domainEvent, context.CancellationToken);
                
                // 3. Mark as processed
                const string updateSql = """
                    UPDATE OutboxMessages
                    SET ProcessedOnUtc = @ProcessedOnUtc
                    WHERE Id = @Id
                    """;
                await connection.ExecuteAsync(
                    updateSql, 
                    new { ProcessedOnUtc = DateTime.UtcNow, outboxMessage.Id }, 
                    transaction: transaction);
            }
            catch (Exception ex)
            {
                // In a production system we'd log this and update the Error column instead.
                const string updateErrorSql = """
                    UPDATE OutboxMessages
                    SET Error = @Error
                    WHERE Id = @Id
                    """;
                await connection.ExecuteAsync(
                    updateErrorSql, 
                    new { Error = ex.ToString(), outboxMessage.Id },
                    transaction: transaction);
            }
        }

        // Commit all processing and updates. If process dies midway, transaction rolls back
        // and unlocks rows for other instances to pick up.
        transaction.Commit();
    }

    private sealed record OutboxMessageResponse(Guid Id, string Content);
}
