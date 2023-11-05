using System.Text;

namespace MatchGeneratorStuff;

internal static class SourceGenerationHelper
{
    public const string MATCH_ATTRIBUTE =
        """
        namespace MatchGenerator
        {
            [System.AttributeUsage(System.AttributeTargets.Enum, AllowMultiple = false,  Inherited = false)]
            public class MatchAttribute : System.Attribute
            {
            }
        }
        """;

    public static string GenerateExtensionClass(List<EnumToGenerate> enumsToGenerate)
    {
        int indentLevel = 0;
        var sb = new StringBuilder();
        sb.AppendLine(
            """
            #nullable enable
            namespace MatchGenerator
            {
                public static class MatchExtensions
                {
            """);
    
        indentLevel++;
        indentLevel++;
    
        foreach (EnumToGenerate enumToGenerate in enumsToGenerate)
        {
            string fullEnumName = enumToGenerate.FullName;
            string enumName = enumToGenerate.Name;
            List<string> discriminants = enumToGenerate.Discriminants;
            
            AppendMatchWithoutReturn(sb, ref indentLevel, enumToGenerate.AccessLevel, fullEnumName, enumName, discriminants);
            sb.AppendLine();
            AppendMatchWithReturnUsingFunc(sb, ref indentLevel, enumToGenerate.AccessLevel, fullEnumName, enumName, discriminants);
            sb.AppendLine();
            AppendMatchWithReturnUsingT(sb, ref indentLevel, enumToGenerate.AccessLevel, fullEnumName, enumName, discriminants);
            
            if (enumToGenerate != enumsToGenerate.Last())
                sb.AppendLine();
        }
    
        indentLevel--;
        sb.AppendLine(Indent.By(indentLevel) + "}");
        indentLevel--;
        sb.AppendLine(Indent.By(indentLevel) + "}");
    
        return sb.ToString();
    }
    
    private static void AppendMatchWithoutReturn(
        StringBuilder sb,
        ref int indentLevel,
        string accessLevel,
        string fullEnumName,
        string enumName,
        List<string> discriminants)
    {
        List<string> parameters = new();
    
        foreach (string discriminant in discriminants)
            parameters.Add($"Action {discriminant}");
        
        string enumVariableName = $"{enumName}Value";
        sb.Append(Indent.By(indentLevel) + $"{accessLevel} static void Match(this {fullEnumName} {enumVariableName}, ");
    
        sb.Append(string.Join(", ", parameters));
        sb.AppendLine(")");
        sb.AppendLine(Indent.By(indentLevel) + "{");
        indentLevel++;

        foreach (string? discriminant in discriminants)
            sb.AppendLine(Indent.By(indentLevel) + $"{discriminant} = {discriminant} ?? throw new ArgumentNullException(nameof({discriminant}));");
        
        sb.AppendLine();

        sb.AppendLine(Indent.By(indentLevel) + $"switch ({enumVariableName})");
        sb.AppendLine(Indent.By(indentLevel) + "{");
        indentLevel++;
    
        // Cases
        foreach (string discriminant in discriminants)
        {
            sb.AppendLine(Indent.By(indentLevel) + $"case {fullEnumName}.{discriminant}:");
            indentLevel++;
            sb.AppendLine(Indent.By(indentLevel) + $"{discriminant}();");
            sb.AppendLine(Indent.By(indentLevel) + "break;");
            indentLevel--;
        }
        
        // Default
        sb.AppendLine(Indent.By(indentLevel) + "default:");
        indentLevel++;
        sb.AppendLine(Indent.By(indentLevel) + $"throw new System.ArgumentOutOfRangeException(nameof({enumVariableName}), \"Enum value not a valid discriminant\");");
        indentLevel--;
        indentLevel--;
        sb.AppendLine(Indent.By(indentLevel) + "}");
        indentLevel--;
        sb.AppendLine(Indent.By(indentLevel) + "}");
    }
    
    private static void AppendMatchWithReturnUsingFunc(
        StringBuilder sb,
        ref int indentLevel,
        string accessLevel,
        string fullEnumName,
        string enumName,
        List<string> discriminants)
    {
        List<string> parameters = new();
    
        foreach (string discriminant in discriminants)
            parameters.Add($"Func<T> {discriminant}");
    
        string enumVariableName = $"{enumName}Value";
        sb.Append(Indent.By(indentLevel) + $"{accessLevel} static T Match<T>(this {fullEnumName} {enumVariableName}, ");
    
        sb.Append(string.Join(", ", parameters));
        sb.AppendLine(")");
        sb.AppendLine(Indent.By(indentLevel) + "{");
        indentLevel++;
        
        foreach (string? discriminant in discriminants)
            sb.AppendLine(Indent.By(indentLevel) + $"{discriminant} = {discriminant} ?? throw new System.ArgumentNullException(nameof({discriminant}));");

        sb.AppendLine();
        
        sb.AppendLine(Indent.By(indentLevel) + $"return {enumVariableName} switch");
        sb.AppendLine(Indent.By(indentLevel) + "{");
        indentLevel++;
    
        // Cases
        foreach (string discriminant in discriminants)
            sb.AppendLine(Indent.By(indentLevel) + $"{fullEnumName}.{discriminant} => {discriminant}(),");
        
        // Default
        sb.AppendLine(Indent.By(indentLevel) + $"_ => throw new System.ArgumentOutOfRangeException(nameof({enumVariableName}), \"Enum value not a valid discriminant\"),");
        indentLevel--;
        sb.AppendLine(Indent.By(indentLevel) + "};");
        indentLevel--;
        sb.AppendLine(Indent.By(indentLevel) + "}");
    }
    
    private static void AppendMatchWithReturnUsingT(
        StringBuilder sb,
        ref int indentLevel,
        string accessLevel,
        string fullEnumName,
        string enumName,
        List<string> discriminants)
    {
        List<string> parameters = new();
    
        foreach (string discriminant in discriminants)
            parameters.Add($"T {discriminant}");
    
        string enumVariableName = $"{enumName}Value";
        sb.Append(Indent.By(indentLevel) + $"{accessLevel} static T Match<T>(this {fullEnumName} {enumVariableName}, ");
    
        sb.Append(string.Join(", ", parameters));
        sb.AppendLine(")");
        sb.AppendLine(Indent.By(indentLevel) + "{");
        indentLevel++;
        
        sb.AppendLine(Indent.By(indentLevel) + $"return {enumVariableName} switch");
        sb.AppendLine(Indent.By(indentLevel) + "{");
        indentLevel++;
    
        // Cases
        foreach (string discriminant in discriminants)
            sb.AppendLine(Indent.By(indentLevel) + $"{fullEnumName}.{discriminant} => {discriminant},");
        
        // Default
        sb.AppendLine(Indent.By(indentLevel) + $"_ => throw new System.ArgumentOutOfRangeException(nameof({enumVariableName}), \"Enum value not a valid discriminant\"),");
        indentLevel--;
        sb.AppendLine(Indent.By(indentLevel) + "};");
        indentLevel--;
        sb.AppendLine(Indent.By(indentLevel) + "}");
    }
}

internal static class Indent
{
    public static string By(int indentLevel) => new('\t', indentLevel);
}