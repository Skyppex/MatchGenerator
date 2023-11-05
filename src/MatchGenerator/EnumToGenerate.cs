namespace MatchGeneratorStuff;

public readonly struct EnumToGenerate : IEquatable<EnumToGenerate>
{
    public bool Equals(EnumToGenerate other) => AccessLevel == other.AccessLevel && FullName == other.FullName && Discriminants.Equals(other.Discriminants);

    public override bool Equals(object? obj) => obj is EnumToGenerate other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = AccessLevel.GetHashCode();
            hashCode = (hashCode * 397) ^ FullName.GetHashCode();
            hashCode = (hashCode * 397) ^ Discriminants.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(EnumToGenerate left, EnumToGenerate right) => left.Equals(right);

    public static bool operator !=(EnumToGenerate left, EnumToGenerate right) => !left.Equals(right);

    public readonly string AccessLevel;
    public readonly string FullName;
    public readonly List<string> Discriminants;
    
    public EnumToGenerate(string accessLevel, string fullName, List<string> discriminants)
    {
        FullName = fullName;
        Discriminants = discriminants;
        AccessLevel = accessLevel;
    }

    public string Name => FullName.Split('.').Last();
}
