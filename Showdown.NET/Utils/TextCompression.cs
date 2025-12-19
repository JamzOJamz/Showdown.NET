using System.IO.Compression;
using System.Text;
using JetBrains.Annotations;

namespace Showdown.NET.Utils;

/// <summary>
/// Provides text compression utilities using Brotli compression.
/// </summary>
/// <remarks>
/// Small strings (less than 64 bytes) are not compressed for efficiency.
/// Larger strings are compressed using Brotli at the fastest compression level.
/// </remarks>
[PublicAPI]
public static class TextCompression
{
    private const int MinCompressSize = 64;
    
    /// <summary>
    /// Compresses a text string into a byte array.
    /// </summary>
    /// <param name="text">The text to compress.</param>
    /// <returns>
    /// A byte array containing the compressed data, or the original UTF-8 bytes
    /// if the text is smaller than the minimum compression size threshold.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="text"/> is null.
    /// </exception>
    public static byte[] Compress(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var bytes = Encoding.UTF8.GetBytes(text);
        return bytes.Length < MinCompressSize ? bytes :
            BrotliCompress(bytes);
    }

    /// <summary>
    /// Decompresses a byte array back into a text string.
    /// </summary>
    /// <param name="data">The compressed data to decompress.</param>
    /// <returns>
    /// The decompressed text string. If the data is smaller than the compression threshold,
    /// it's treated as uncompressed UTF-8 text.
    /// </returns>
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