using System.Reflection;
using NetArchTest.Rules;

namespace AdventureWorksAIWorkspaceAPI.Architecture.Tests;

public sealed class LayerDependencyTests
{
    private const string ApiNamespace = "AdventureWorksAIWorkspaceAPI.Api";
    private const string ApplicationNamespace = "AdventureWorksAIWorkspaceAPI.Application";
    private const string DomainNamespace = "AdventureWorksAIWorkspaceAPI.Domain";
    private const string InfrastructureNamespace = "AdventureWorksAIWorkspaceAPI.Infrastructure";

    [Theory]
    [InlineData(DomainNamespace, ApplicationNamespace)]
    [InlineData(DomainNamespace, InfrastructureNamespace)]
    [InlineData(DomainNamespace, ApiNamespace)]
    [InlineData(ApplicationNamespace, InfrastructureNamespace)]
    [InlineData(ApplicationNamespace, ApiNamespace)]
    [InlineData(InfrastructureNamespace, ApiNamespace)]
    public void Layer_Should_NotDependOnForbiddenLayer(string assemblyName, string forbiddenNamespace)
    {
        var result = Types
            .InAssembly(LoadAssembly(assemblyName))
            .ShouldNot()
            .HaveDependencyOn(forbiddenNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    private static Assembly LoadAssembly(string assemblyName)
    {
        return Assembly.Load(new AssemblyName(assemblyName));
    }
}
