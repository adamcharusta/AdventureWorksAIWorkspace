using System.Reflection;
using NetArchTest.Rules;

namespace AdventureWorksAIWorkspace.Architecture.Tests;

public sealed class LayerDependencyTests
{
    private const string ApiNamespace = "AdventureWorksAIWorkspace.Api";
    private const string ApplicationNamespace = "AdventureWorksAIWorkspace.Application";
    private const string DomainNamespace = "AdventureWorksAIWorkspace.Domain";
    private const string InfrastructureNamespace = "AdventureWorksAIWorkspace.Infrastructure";

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
