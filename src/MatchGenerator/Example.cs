namespace MatchGeneratorStuff;

[AttributeUsage(AttributeTargets.Enum)]
public class MatchAttribute : System.Attribute
{
    
}

public enum ExampleEnum
{
    A,
    B,
    C
}

public class Example
{
    public void Test()
    {
        var exampleEnumValue = ExampleEnum.A;

        exampleEnumValue.Match(() => { }, () => { }, () => { });
        int val = exampleEnumValue.Match(() => 1, () => 1, () => 1);
    }
}

public static class ExampleEnumExtensions
{
    public static void Match(this ExampleEnum exampleEnumValue, Action A, Action B, Action C)
    {
        switch (exampleEnumValue)
        {
            case ExampleEnum.A:
                A();
                break;
            
            case ExampleEnum.B:
                B();
                break;
            
            case ExampleEnum.C:
                C();
                break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(exampleEnumValue), "Enum value not a valid discriminant");
        }
    }
    
    public static T Match<T>(this ExampleEnum exampleEnumValue, Func<T> A, Func<T> B, Func<T> C)
    {
        A = A ?? throw new ArgumentNullException(nameof(A));
        B = B ?? throw new ArgumentNullException(nameof(B));
        C = C ?? throw new ArgumentNullException(nameof(C));
        
        return exampleEnumValue switch
        {
            ExampleEnum.A => A(),
            ExampleEnum.B => B(),
            ExampleEnum.C => C(),
            _ => throw new ArgumentOutOfRangeException(nameof(exampleEnumValue), "Enum value not a valid discriminant"),
        };
    }
    
    public static T Match<T>(this ExampleEnum exampleEnumValue, T A, T B, T C)
    {
        return exampleEnumValue switch
        {
            ExampleEnum.A => A,
            ExampleEnum.B => B,
            ExampleEnum.C => C,
            _ => throw new ArgumentOutOfRangeException(nameof(exampleEnumValue), "Enum value not a valid discriminant"),
        };
    }
}