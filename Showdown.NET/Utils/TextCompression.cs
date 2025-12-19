using System.IO.Compression;
using System.Text;
using JetBrains.Annotations;

namespace Showdown.NET.Utils;

[PublicAPI]
public static class TextCompression
{
    private const int MinCompressSize = 64;
    
    public static byte[] Compress(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var bytes = Encoding.UTF8.GetBytes(text);
        return bytes.Length < MinCompressSize ? bytes :
            BrotliCompress(bytes);
    }

    public static string Decompress(ReadOnlySpan<byte> data)
    {
        // Optional: could mark compressed vs raw with a prefix byte
        return data.Length < MinCompressSize ? Encoding.UTF8.GetString(data) : BrotliDecompress(data);
    }
    
    private static byte[] BrotliCompress(byte[] input)
    {
        using var output = new MemoryStream();
        using (var brotli = new BrotliStream(output, CompressionLevel.Fastest, leaveOpen: true))
            brotli.Write(input, 0, input.Length);
        return output.ToArray();
    }

    private static string BrotliDecompress(ReadOnlySpan<byte> data)
    {
        using var input = new MemoryStream(data.ToArray());
        using var brotli = new BrotliStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        brotli.CopyTo(output);
        return Encoding.UTF8.GetString(output.ToArray());
    }
}