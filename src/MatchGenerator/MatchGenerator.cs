using System.Collections.Immutable;
using System.Text;
using MatchGeneratorStuff;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator]
public class EnumGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add the marker attribute
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "MatchAttribute.g.cs", 
            SourceText.From(SourceGenerationHelper.MATCH_ATTRIBUTE, Encoding.UTF8)));

        // Do a simple filter for enums
        IncrementalValuesProvider<EnumDeclarationSyntax> enumDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s), // select enums with attributes
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)) // sect the enum with the [EnumExtensions] attribute
            .Where(static m => m is not null)!; // filter out attributed enums that we don't care about

        // Combine the selected enums with the `Compilation`
        IncrementalValueProvider<(Compilation, ImmutableArray<EnumDeclarationSyntax>)> compilationAndEnums
            = context.CompilationProvider.Combine(enumDeclarations.Collect());

        // Generate the source using the compilation and enums
        context.RegisterSourceOutput(compilationAndEnums,
            static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        => node is EnumDeclarationSyntax { AttributeLists.Count: > 0 };

    private static EnumDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        // we know the node is a EnumDeclarationSyntax thanks to IsSyntaxTargetForGeneration
        var enumDeclarationSyntax = (EnumDeclarationSyntax)context.Node;

        // loop through all the attributes on the method
        foreach (AttributeListSyntax attributeListSyntax in enumDeclarationSyntax.AttributeLists)
        {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                if (ModelExtensions.GetSymbolInfo(context.SemanticModel, attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                {
                    // weird, we couldn't get the symbol, ignore it
                    continue;
                }

                INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                string fullName = attributeContainingTypeSymbol.ToDisplayString();

                // Is the attribute the [EnumExtensions] attribute?
                if (fullName == "MatchGenerator.MatchAttribute")
                {
                    // return the enum
                    return enumDeclarationSyntax;
                }
            }
        }

        // we didn't find the attribute we were looking for
        return null;
    }   
    
    private static void Execute(Compilation compilation, ImmutableArray<EnumDeclarationSyntax> enums, SourceProductionContext context)
    {
        if (enums.IsDefaultOrEmpty)
        {
            // nothing to do yet
            return;
        }

        // I'm not sure if this is actually necessary, but `[LoggerMessage]` does it, so seems like a good idea!
        IEnumerable<EnumDeclarationSyntax> distinctEnums = enums.Distinct();

        // Convert each EnumDeclarationSyntax to an EnumToGenerate
        List<EnumToGenerate> enumsToGenerate = GetTypesToGenerate(compilation, distinctEnums, context.CancellationToken);

        // If there were errors in the EnumDeclarationSyntax, we won't create an
        // EnumToGenerate for it, so make sure we have something to generate
        if (enumsToGenerate.Count > 0)
        {
            // generate the source code and add it to the output
            string result = SourceGenerationHelper.GenerateExtensionClass(enumsToGenerate);
            context.AddSource("MatchExtensions.g.cs", SourceText.From(result, Encoding.UTF8));
        }
    }
    
    private static List<EnumToGenerate> GetTypesToGenerate(Compilation compilation, IEnumerable<EnumDeclarationSyntax> enums, CancellationToken ct)
    {
        // Create a list to hold our output
        var enumsToGenerate = new List<EnumToGenerate>();
        // Get the semantic representation of our marker attribute 
        INamedTypeSymbol? enumAttribute = compilation.GetTypeByMetadataName("MatchGenerator.MatchAttribute");

        if (enumAttribute == null)
        {
            // If this is null, the compilation couldn't find the marker attribute type
            // which suggests there's something very wrong! Bail out..
            return enumsToGenerate;
        }

        foreach (EnumDeclarationSyntax enumDeclarationSyntax in enums)
        {
            // stop if we're asked to
            ct.ThrowIfCancellationRequested();

            string? accessLevel = GetAccessLevel(enumDeclarationSyntax);
            
            if (accessLevel is null)
                continue;
            
            // Get the semantic representation of the enum syntax
            SemanticModel semanticModel = compilation.GetSemanticModel(enumDeclarationSyntax.SyntaxTree);
            if (ModelExtensions.GetDeclaredSymbol(semanticModel, enumDeclarationSyntax) is not INamedTypeSymbol enumSymbol)
            {
                // something went wrong, bail out
                continue;
            }

            // Get the full type name of the enum e.g. Colour, 
            // or OuterClass<T>.Colour if it was nested in a generic type (for example)
            string enumName = enumSymbol.ToString();

            // Get all the members in the enum
            ImmutableArray<ISymbol> enumMembers = enumSymbol.GetMembers();
            var members = new List<string>(enumMembers.Length);

            // Get all the fields from the enum, and add their name to the list
            foreach (ISymbol member in enumMembers)
            {
                if (member is IFieldSymbol { ConstantValue: not null })
                {
                    members.Add(member.Name);
                }
            }

            // Create an EnumToGenerate for use in the generation phase
            enumsToGenerate.Add(new EnumToGenerate(accessLevel, enumName, members));
        }

        return enumsToGenerate;
    }
    
    private static string? GetAccessLevel(EnumDeclarationSyntax enumDeclarationSyntax)
    {
        SyntaxTokenList modifiers = enumDeclarationSyntax.Modifiers;

        if (modifiers.Any(SyntaxKind.ProtectedKeyword) && modifiers.Any(SyntaxKind.InternalKeyword))
            return "protected internal";

        if (modifiers.Any(SyntaxKind.PrivateKeyword) && modifiers.Any(SyntaxKind.ProtectedKeyword))
            return "private protected";
        
        if (modifiers.Any(SyntaxKind.PublicKeyword))
            return "public";
        
        if (modifiers.Any(SyntaxKind.InternalKeyword))
            return "internal";
        
        return null;
    }
}
