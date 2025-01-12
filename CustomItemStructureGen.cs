using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Threading;

namespace EIV.Generator;

[Generator]
public class CustomItemStructureGen : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName("ExtractIntoVoid.Generator.CustomItemStructure", static (s, _) => true, GetSourceStuff).Where(static x=>x is not null);
        context.RegisterSourceOutput(provider, AddToSource);
    }

    private void AddToSource(SourceProductionContext context, CustomItemGen? source)
    {
        if (!source.HasValue) 
            return;
        string src = (string)CustomItemStructure_Source.Source.Clone();
        src = src.Replace(CustomItemStructure_Source.ReplaceNewType, source.Value.NewClassName);
        src = src.Replace(CustomItemStructure_Source.ReplaceBaseType, source.Value.BaseName);
        src = src.Replace(CustomItemStructure_Source.AddNameSpace, source.Value.NameSpace);
        context.AddSource($"{source.Value.BaseName}.g.cs", src);
    }

    public static CustomItemGen? GetSourceStuff(GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        if (context.SemanticModel.GetDeclaredSymbol(context.TargetNode) is not INamedTypeSymbol classSymbol)
        {
            return null;
        }
        var name = classSymbol.Name;
        CustomItemGen customItemGen = new();
        customItemGen.NewClassName = (string)context.Attributes[0].ConstructorArguments[0].Value;
        customItemGen.BaseName = name;
        if (string.IsNullOrEmpty(customItemGen.NewClassName))
            customItemGen.NewClassName = name;
        customItemGen.NameSpace = GetNamespace(context.TargetNode as BaseTypeDeclarationSyntax);
        return customItemGen;
    }

    public static DiagnosticDescriptor TestDiag { get; } = new(
    id: "EG0001",
    title: "Testing this",
    messageFormat: "'{0}'.",
    category: "EIV_GEN",
    DiagnosticSeverity.Warning,
    isEnabledByDefault: true);

    static string GetNamespace(BaseTypeDeclarationSyntax syntax)
    {
        // If we don't have a namespace at all we'll return an empty string
        // This accounts for the "default namespace" case
        string nameSpace = string.Empty;

        // Get the containing syntax node for the type declaration
        // (could be a nested type, for example)
        SyntaxNode potentialNamespaceParent = syntax.Parent;

        // Keep moving "out" of nested classes etc until we get to a namespace
        // or until we run out of parents
        while (potentialNamespaceParent != null &&
                potentialNamespaceParent is not NamespaceDeclarationSyntax
                && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        // Build up the final namespace by looping until we no longer have a namespace declaration
        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            // We have a namespace. Use that as the type
            nameSpace = namespaceParent.Name.ToString();

            // Keep moving "out" of the namespace declarations until we 
            // run out of nested namespace declarations
            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }

                // Add the outer namespace as a prefix to the final namespace
                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                namespaceParent = parent;
            }
        }

        // return the final namespace
        return nameSpace;
    }
}

public struct CustomItemGen
{
    public string BaseName;
    public string NameSpace;
    public string NewClassName;
}

