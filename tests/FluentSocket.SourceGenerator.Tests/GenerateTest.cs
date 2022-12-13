using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace FluentSocket.SourceGenerator.Tests;

[UsesVerify]
public class GenerateTest
{
    [Fact]
    public Task GenerateDesigner() => Test(
        """
        namespace Tests
        {
            [FluentSocket.Message]
            public partial class EchoMessage
            {
                public string Text { get; set; }
            }
        }
        """);

    [Fact]
    public Task GenerateInterfaceDesigner() => Test(
        """
        namespace Tests
        {
            [FluentSocket.Message]
            public partial class IncomingMessage
            {
                public int Id { get; set; }

                public IIncomingMessage Message { get; set; }
            }
        }
        """,

        """
        namespace Tests
        {
            [FluentSocket.Message]
            public partial interface IIncomingMessage
            {
            }
        }
        """,

        """
        namespace Tests
        {
            [FluentSocket.Message]
            public partial class EchoMessage : IIncomingMessage
            {
                public string Text { get; set; }
            }
        }
        """);

    private static Task Test(params string[] source)
    {
        VerifySourceGenerators.Enable();

        var generator = new MessageSourceGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(IMessage).Assembly.Location)
        };

        var syntaxTrees = source.Select(x => CSharpSyntaxTree.ParseText(x)).ToArray();

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: syntaxTrees,
            references: references);

        driver = driver.RunGenerators(compilation);

        return Verify(driver);
    }
}
