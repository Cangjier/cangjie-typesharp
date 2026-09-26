namespace Cangjie.TypeSharp.System;
public class Error(string message) : Exception(message)
{
    public string message { get; } = message;

    public string? cause { get; set; } = null;
}
