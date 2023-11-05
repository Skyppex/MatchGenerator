// See https://aka.ms/new-console-template for more information

using MatchGenerator;

Console.WriteLine("Hello, World!");

PrintDirection(Direction.Up);
PrintDirection(Direction.Down);
PrintDirection(Direction.Left);
PrintDirection(Direction.Right);
    
static void PrintDirection(Direction direction) =>
    Console.WriteLine(direction.Match<string>(
        () => "Up",
        null,
        () => "Left",
        () => "Right"
    ));

[Match]
public enum Direction
{
    Up,
    Down,
    Left,
    Right
}
