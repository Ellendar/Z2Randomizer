using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using CrossPlatformUI.Services;

namespace CrossPlatformUI.Browser;

public partial class BrowserFileService : IFileService
{
    [JSImport("globalThis.customFetchString")]
    [return: JSMarshalAs<JSType.Promise<JSType.String>>]
    private static partial Task<string> FetchString(string url);
    [JSImport("globalThis.customFetchBinary")]
    [return: JSMarshalAs<JSType.Promise<JSType.String>>]
    private static partial Task<string> FetchBinary(string url);
    
    public Task<string> OpenLocalFile(string filename)
    {
        return FetchString(filename);
    }

    public async Task<byte[]> OpenLocalBinaryFile(string filename)
    {
        var b64Str = await FetchBinary(filename);
        return Convert.FromBase64String(b64Str);
    }

    public Task SaveGeneratedBinaryFile(string filename, byte[] filedata, string? path = null)
    {
        throw new System.NotImplementedException();
    }
}