using CleanArchitecture.Application.Abstractions.Data;
using CleanArchitecture.Domain.Users;
using CleanArchitecture.Infrastructure.BackgroundJobs;
using CleanArchitecture.Infrastructure.Database;
using CleanArchitecture.Infrastructure.Interceptors;
using CleanArchitecture.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace CleanArchitecture.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new ArgumentNullException(nameof(configuration));

        services.AddSingleton<InsertOutboxMessagesInterceptor>();

        services.AddDbContext<ApplicationDbContext>(
            (sp, options) => options
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>()));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

        // 注册实用工具类
        services.AddSingleton<Application.Abstractions.Clock.IDateTimeProvider, Clock.DateTimeProvider>();

        // 注册对外部 API 的 Http 请求客户端
        services.AddHttpClient<Application.Abstractions.Services.IWeatherService, Services.WeatherService>();

        services.AddQuartz(configure =>
        {
            var jobKey = new JobKey(nameof(ProcessOutboxMessagesJob));

            configure.AddJob<ProcessOutboxMessagesJob>(jobKey, static (IJobConfigurator _) => { });
            configure.AddTrigger(trigger =>
                trigger.ForJob(jobKey)
                    .WithSimpleSchedule(schedule =>
                        schedule.WithIntervalInSeconds(10)
                            .RepeatForever()));
        });

        services.AddQuartzHostedService();

        return services;
    }
}
