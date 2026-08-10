namespace Cangjie.TypeSharp.System;

public class FormData : IDisposable
{
    internal MultipartFormDataContent? _multipartFormDataContent = new();

    public void append(string name, string value)
    {
        _multipartFormDataContent?.Add(new StringContent(value), name);
    }

    public void Dispose()
    {
        _multipartFormDataContent?.Dispose();
        _multipartFormDataContent = null;
    }
}