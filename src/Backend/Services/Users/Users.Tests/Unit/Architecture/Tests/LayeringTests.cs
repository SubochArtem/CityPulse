using Xunit;

namespace Users.Tests.Unit.Architecture.Tests;

public sealed class LayeringTests
{
    private const string BusinessAssembly = "Users.Business";
    private const string DataAccessAssembly = "Users.DataAccess";
    private const string PresentationAssembly = "Users.Presentation";

    private static IEnumerable<string?> ReferencedAssemblyNames(Type anchor) =>
        anchor.Assembly.GetReferencedAssemblies().Select(assembly => assembly.Name);

    [Fact]
    public void Presentation_ShouldReferenceBusiness()
    {
        var referenced = ReferencedAssemblyNames(typeof(Program));

        Assert.Contains(BusinessAssembly, referenced);
    }

    [Fact]
    public void Presentation_ShouldNotReferenceDataAccess()
    {
        var referenced = ReferencedAssemblyNames(typeof(Program));

        Assert.DoesNotContain(DataAccessAssembly, referenced);
    }

    [Fact]
    public void Business_ShouldNotReferencePresentation()
    {
        var referenced = ReferencedAssemblyNames(typeof(Business.DependencyInjection));

        Assert.DoesNotContain(PresentationAssembly, referenced);
    }

    [Fact]
    public void DataAccess_ShouldNotReferenceBusinessOrPresentation()
    {
        var referenced = ReferencedAssemblyNames(typeof(DataAccess.DependencyInjection)).ToList();

        Assert.DoesNotContain(BusinessAssembly, referenced);
        Assert.DoesNotContain(PresentationAssembly, referenced);
    }
}
