using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace CleanArchitecture.ArchitectureTests;

public class LayerDependencyTests
{
    [Fact]
    public void Domain_ShouldNotDependOnOtherLayers()
    {
        var result = Types.InAssembly(typeof(CleanArchitecture.Domain.Users.User).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "CleanArchitecture.Application",
                "CleanArchitecture.Infrastructure",
                "CleanArchitecture.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_ShouldNotDependOnInfrastructureOrApi()
    {
        var result = Types.InAssembly(typeof(CleanArchitecture.Application.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "CleanArchitecture.Infrastructure",
                "CleanArchitecture.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOnApi()
    {
        var result = Types.InAssembly(typeof(CleanArchitecture.Infrastructure.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("CleanArchitecture.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
