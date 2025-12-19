namespace Showdown.NET.Definitions;

public record struct Side(int Player, string Username)
{
    public static Side Parse(string input)
    {
        int side = input[1] - '0';
        string username = input[4..];
        return new Side(side, username);
    }
}
