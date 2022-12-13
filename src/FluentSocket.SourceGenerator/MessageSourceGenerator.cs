using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;

namespace FluentSocket.SourceGenerator;

[Generator]
public class MessageSourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        var sb = new StringBuilder();
        var generatedMessages = new HashSet<INamedTypeSymbol>();

        foreach (var syntaxTree in context.Compilation.SyntaxTrees)
        {
            SemanticModel semanticModel = null;

            var root = syntaxTree.GetRoot();

            // Get all types with attribute FluentSocket.MessageAttribute
            foreach (var syntaxNode in root.DescendantNodes())
            {
                if (syntaxNode is not MemberDeclarationSyntax classDeclarationSyntax)
                {
                    continue;
                }

                if (!IsMessage(classDeclarationSyntax))
                {
                    continue;
                }

                semanticModel ??= context.Compilation.GetSemanticModel(syntaxTree);

                if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is INamedTypeSymbol classSymbol)
                {
                    RegisterMessage(context.Compilation, semanticModel, classSymbol, generatedMessages, sb);
                }
            }
        }

        context.AddSource("Message", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private void RegisterMessage(
        Compilation compilation,
        SemanticModel semanticModel,
        INamedTypeSymbol classSymbol,
        ISet<INamedTypeSymbol> generatedMessages,
        StringBuilder sb)
    {
        if (!generatedMessages.Add(classSymbol))
        {
            return;
        }

        if (IsStatic(classSymbol))
        {
            AddDerivedMessages(compilation, classSymbol, generatedMessages, sb);
            return;
        }

        var nestedTypes = new List<INamedTypeSymbol>();
        var properties = new List<(string method, IPropertySymbol propertySymbol, bool isStatic)>();

        foreach (var member in classSymbol.GetMembers())
        {
            if (member.Kind != SymbolKind.Property)
            {
                continue;
            }

            var propertySymbol = (IPropertySymbol)member;

            if (propertySymbol.SetMethod is null)
            {
                continue;
            }

            var propertyType = propertySymbol.Type;
            var method = GetMethod(propertyType);
            var isStatic = false;

            if (method is "Object" && propertyType is INamedTypeSymbol namedTypeSymbol)
            {
                nestedTypes.Add(namedTypeSymbol);
                isStatic = IsStatic(namedTypeSymbol);
            }

            properties.Add((method, propertySymbol, isStatic));
        }

        // Namespace
        sb.AppendLine($"namespace {classSymbol.ContainingNamespace.ToDisplayString()}");
        sb.AppendLine("{");

        // Class
        sb.AppendLine($"    public partial class {classSymbol.Name} : global::FluentSocket.IMessage");
        sb.AppendLine("    {");

        // Write properties
        sb.AppendLine("        public void Write(ref global::FluentSocket.MessageWriter writer)");
        sb.AppendLine("        {");

        foreach (var (method, propertyName, isStatic) in properties)
        {
            if (isStatic)
            {
                var typeName = propertyName.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                sb.AppendLine($"            {typeName}.Write(ref writer, {propertyName.Name});");
            }
            else
            {
                sb.AppendLine($"            writer.Write{method}(this.{propertyName.Name});");
            }

        }

        sb.AppendLine("        }");
        sb.AppendLine();

        // Read properties
        sb.AppendLine("        public void Read(ref global::FluentSocket.MessageReader reader)");
        sb.AppendLine("        {");

        foreach (var (method, propertyName, isStatic) in properties)
        {
            var typeName = propertyName.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            if (isStatic)
            {
                sb.AppendLine($"            {propertyName.Name} = {typeName}.Read(ref reader);");
            }
            else if (method == "Object")
            {
                sb.AppendLine($"            this.{propertyName.Name} = reader.ReadObject<{typeName}>();");
            }
            else
            {
                sb.AppendLine($"            this.{propertyName.Name} = reader.Read{method}();");
            }
        }

        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();

        foreach (var typeSymbol in nestedTypes)
        {
            RegisterMessage(compilation, semanticModel, typeSymbol, generatedMessages, sb);
        }
    }

    private static bool IsStatic(INamedTypeSymbol classSymbol)
    {
        return classSymbol.TypeKind == TypeKind.Interface ||
               classSymbol is { TypeKind: TypeKind.Class, IsAbstract: true};
    }

    private void AddDerivedMessages(
        Compilation compilation,
        INamedTypeSymbol classSymbol,
        ISet<INamedTypeSymbol> generatedMessages,
        StringBuilder sb)
    {
        var name = classSymbol.Name;
        var types = new List<(INamedTypeSymbol Type, SemanticModel Model)>();

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var root = syntaxTree.GetRoot();

            foreach (var node in root.DescendantNodes())
            {
                if (node is not ClassDeclarationSyntax { BaseList: {} baseList})
                {
                    continue;
                }

                var isImplementingType = false;

                foreach (var baseType in baseList.Types)
                {
                    if (baseType is not SimpleBaseTypeSyntax { Type: {} type })
                    {
                        continue;
                    }

                    switch (type)
                    {
                        case IdentifierNameSyntax identifier when identifier.Identifier.Text == name:
                            isImplementingType = true;
                            break;
                        case QualifiedNameSyntax qualifiedName when qualifiedName.Right.Identifier.Text == name:
                            isImplementingType = true;
                            break;
                        default:
                            continue;
                    }
                }

                if (!isImplementingType)
                {
                    continue;
                }

                var semanticModel = compilation.GetSemanticModel(syntaxTree);

                if (semanticModel.GetDeclaredSymbol(node) is INamedTypeSymbol typeSymbol)
                {
                    types.Add((typeSymbol, semanticModel));
                }
            }
        }

        if (types.Count == 0)
        {
            return;
        }

        types.Sort(static (x, y) => string.Compare(x.Type.Name, y.Type.Name, StringComparison.Ordinal));

        var typeName = classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var ns = classSymbol.ContainingNamespace.ToDisplayString();
        var classOrInterface = classSymbol.TypeKind == TypeKind.Interface ? "interface" : "class";

        sb.AppendLine($"namespace {ns}");
        sb.AppendLine("{");

        sb.AppendLine($"    partial {classOrInterface} {name} : global::FluentSocket.IUnionMessage<{typeName}>");
        sb.AppendLine("    {");

        sb.AppendLine("#if NET7_0_OR_GREATER");
        sb.AppendLine($"        static void global::FluentSocket.IUnionMessage<{typeName}>.Write(ref global::FluentSocket.MessageWriter writer, {typeName} value) => {name}Extensions.WriteUnion(ref writer, value);");
        sb.AppendLine($"        static {typeName} global::FluentSocket.IUnionMessage<{typeName}>.Read(ref global::FluentSocket.MessageReader reader) => {name}Extensions.ReadUnion<{typeName}>(ref reader);");
        sb.AppendLine("#endif");

        sb.AppendLine("    }");
        sb.AppendLine();

        var handlerName = classSymbol.TypeKind == TypeKind.Interface ? $"{name}Handler" : $"I{name}Handler";
        var asyncHandlerName = classSymbol.TypeKind == TypeKind.Interface ? $"IAsync{name.Substring(1)}Handler" : $"IAsync{name}Handler";

        // Start handler
        sb.AppendLine($"    public partial interface {handlerName}");
        sb.AppendLine("    {");

        foreach (var (type, _) in types)
        {
            sb.AppendLine($"        void Handle{type.Name}({type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} message);");
        }

        sb.AppendLine("    }");
        sb.AppendLine();
        // End handler

        // Start handler
        sb.AppendLine($"    public partial interface {asyncHandlerName}");
        sb.AppendLine("    {");

        foreach (var (type, _) in types)
        {
            sb.AppendLine($"        global::System.Threading.Tasks.ValueTask Handle{type.Name}Async({type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} message);");
        }

        sb.AppendLine("    }");
        // End handler

        sb.AppendLine("}");
        sb.AppendLine();

        sb.AppendLine("namespace FluentSocket");
        sb.AppendLine("{");

        sb.AppendLine($"    public static partial class {name}Extensions");
        sb.AppendLine("    {");

        sb.AppendLine($"        public static void WriteUnion(ref this global::FluentSocket.MessageWriter writer, {typeName} value)");
        sb.AppendLine("        {");

        sb.AppendLine("            switch (value)");
        sb.AppendLine("            {");

        var id = 0;

        foreach (var (typeSymbol, _) in types)
        {
            var inheritTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            sb.AppendLine($"                case {inheritTypeName} v:");
            sb.AppendLine($"                    writer.WriteByte({id++});");
            sb.AppendLine("                    writer.WriteObject(v);");
            sb.AppendLine("                    break;");
        }

        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();

        // Start Read
        sb.AppendLine($"        public static {typeName} ReadUnion<T>(ref this global::FluentSocket.MessageReader reader)");
        sb.AppendLine($"            where T : {typeName}");
        sb.AppendLine("        {");

        sb.AppendLine("            switch (reader.ReadByte())");
        sb.AppendLine("            {");

        id = 0;

        foreach (var (typeSymbol, _) in types)
        {
            var inheritTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            sb.AppendLine($"                case {id++}:");
            sb.AppendLine($"                    return reader.ReadObject<{inheritTypeName}>();");
        }

        sb.AppendLine("                default:");
        sb.AppendLine("                    throw new global::System.ArgumentException(\"Invalid union type.\");");


        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
        // End Read

        // Start Handle
        sb.AppendLine("        public static void Handle<THandler>(this THandler handler, ref global::FluentSocket.MessageReader reader)");
        sb.AppendLine($"            where THandler : global::{ns}.{handlerName}");
        sb.AppendLine("        {");

        sb.AppendLine("            switch (reader.ReadByte())");
        sb.AppendLine("            {");

        id = 0;

        foreach (var (typeSymbol, _) in types)
        {
            var inheritTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            sb.AppendLine($"                case {id++}:");
            sb.AppendLine($"                    handler.Handle{typeSymbol.Name}(reader.ReadObject<{inheritTypeName}>());");
            sb.AppendLine("                    break;");
        }

        sb.AppendLine("                default:");
        sb.AppendLine("                    throw new global::System.ArgumentException(\"Invalid union type.\");");


        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();

        sb.AppendLine($"        public static void Handle<THandler>(this THandler handler, {typeName} value)");
        sb.AppendLine($"            where THandler : global::{ns}.{handlerName}");
        sb.AppendLine("        {");

        sb.AppendLine("            switch (value)");
        sb.AppendLine("            {");

        foreach (var (typeSymbol, _) in types)
        {
            var inheritTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            sb.AppendLine($"                case {inheritTypeName} v:");
            sb.AppendLine($"                    handler.Handle{typeSymbol.Name}(v);");
            sb.AppendLine("                    break;");
        }

        sb.AppendLine("                default:");
        sb.AppendLine("                    throw new global::System.ArgumentException(\"Invalid union type.\");");


        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
        // End Handle

        // Start async handle
        sb.AppendLine($"        public static global::System.Threading.Tasks.ValueTask HandleAsync<THandler>(this THandler handler, {typeName} value)");
        sb.AppendLine($"            where THandler : global::{ns}.{asyncHandlerName}");
        sb.AppendLine("        {");

        sb.AppendLine("            switch (value)");
        sb.AppendLine("            {");

        foreach (var (typeSymbol, _) in types)
        {
            var inheritTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            sb.AppendLine($"                case {inheritTypeName} v:");
            sb.AppendLine($"                    return handler.Handle{typeSymbol.Name}Async(v);");
        }

        sb.AppendLine("                default:");
        sb.AppendLine("                    throw new global::System.ArgumentException(\"Invalid union type.\");");


        sb.AppendLine("            }");
        sb.AppendLine("        }");
        // End async handle

        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();

        foreach (var (typeSymbol, semanticModel) in types)
        {
            RegisterMessage(compilation, semanticModel, typeSymbol, generatedMessages, sb);
        }
    }

    private static string GetMethod(ITypeSymbol propertyType)
    {
        return propertyType switch
        {
            { ContainingNamespace.Name: "System", Name: "String" } => "String",
            { ContainingNamespace.Name: "System", Name: "Int16" } => "Int16",
            { ContainingNamespace.Name: "System", Name: "UInt16" } => "UInt16",
            { ContainingNamespace.Name: "System", Name: "Int32" } => "Int32",
            { ContainingNamespace.Name: "System", Name: "UInt32" } => "UInt32",
            { ContainingNamespace.Name: "System", Name: "Int64" } => "Int64",
            { ContainingNamespace.Name: "System", Name: "UInt64" } => "UInt64",
            { ContainingNamespace.Name: "System", Name: "Boolean" } => "Boolean",
            { ContainingNamespace.Name: "System", Name: "Byte" } => "Byte",
            { ContainingNamespace.Name: "System", Name: "Single" } => "Single",
            { ContainingNamespace.Name: "System", Name: "Double" } => "Double",
            _ => "Object"
        };
    }


    private static bool IsMessage(MemberDeclarationSyntax classDeclarationSyntax)
    {
        foreach (var attributeListSyntax in classDeclarationSyntax.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                switch (attributeSyntax.Name)
                {
                    case IdentifierNameSyntax identifierNameSyntax when
                        identifierNameSyntax.Identifier.Text.EndsWith("Message"):
                    case QualifiedNameSyntax qualifiedNameSyntax when
                        qualifiedNameSyntax.Right.Identifier.Text.EndsWith("Message"):
                        return true;
                }
            }
        }

        return false;
    }

    public void Initialize(GeneratorInitializationContext context) { }
}
