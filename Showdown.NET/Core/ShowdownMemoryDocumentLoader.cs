using System.Text;
using Microsoft.ClearScript;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;

namespace Showdown.NET.Core;

internal class ShowdownMemoryDocumentLoader(ZipArchive archive) : ShowdownDocumentLoader
{
    private const string VirtualRoot = "vfs://archive/";

    public override void DiscardCachedDocuments()
    {
        base.DiscardCachedDocuments();
        archive.Dispose();
    }

    public string[] GetVirtualFileSystemEntries(string path)
    {
        var trimmedPath = GetTrimmedDirectoryPath(path);
        var results = new HashSet<string>();

        foreach (var entry in archive.Entries.Where(e => e.Key != null))
        {
            var entryPath = entry.Key!;

            if (!IsEntryInDirectory(entryPath, trimmedPath))
                continue;

            var relativePath = GetRelativePath(entryPath, trimmedPath);
            var childName = GetImmediateChild(relativePath);

            if (!string.IsNullOrEmpty(childName))
                results.Add(childName);
        }

        return results.ToArray();
    }

    public override async Task<Document> LoadDocumentAsync(DocumentSettings settings, DocumentInfo? sourceInfo,
        string specifier, DocumentCategory category,
        DocumentContextCallback contextCallback)
    {
        // Try base implementation first
        var result = base.LoadDocumentAsync(settings, sourceInfo, specifier, category, contextCallback);
        if (result != null) return await result;

        var fileUri = ResolveFileUri(sourceInfo, specifier);
        var entry = FindArchiveEntry(fileUri);

        if (entry.entry == null)
            return CreateModuleNotFoundDocument(fileUri, category, contextCallback);

        var resolvedUri = new Uri($"{VirtualRoot}{entry.matchedKey}");

        // Return cached document if available
        var cachedDocument = GetCachedDocument(resolvedUri);
        if (cachedDocument != null)
            return cachedDocument;

        // Load and cache the document
        var document = await CreateDocumentFromEntry(entry.entry, resolvedUri, category, contextCallback);
        CacheDocument(document, false);

        return document;
    }

    private static string GetTrimmedDirectoryPath(string path)
    {
        var asUri = new Uri(path);
        var trimmed = asUri.LocalPath.TrimStart('/');

        // Ensure directory path ends with separator
        if (!string.IsNullOrEmpty(trimmed) && !trimmed.EndsWith('/'))
            trimmed += "/";

        return trimmed;
    }

    private static bool IsEntryInDirectory(string entryPath, string directoryPath)
    {
        return string.IsNullOrEmpty(directoryPath) || entryPath.StartsWith(directoryPath);
    }

    private static string GetRelativePath(string entryPath, string directoryPath)
    {
        return string.IsNullOrEmpty(directoryPath) ? entryPath : entryPath[directoryPath.Length..];
    }

    private static string GetImmediateChild(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return string.Empty;

        var separatorIndex = relativePath.IndexOf('/');
        return separatorIndex >= 0
            ? relativePath[..separatorIndex] // Directory name
            : relativePath; // File name
    }

    private Uri ResolveFileUri(DocumentInfo? sourceInfo, string specifier)
    {
        var baseUri = sourceInfo?.Uri ?? new Uri(VirtualRoot);
        var normalizedSpecifier = specifier.Replace("\\", "/");

        if (normalizedSpecifier.StartsWith("./"))
            normalizedSpecifier = normalizedSpecifier[2..];

        return new Uri(baseUri, normalizedSpecifier);
    }

    private (ZipArchiveEntry? entry, string? matchedKey) FindArchiveEntry(Uri fileUri)
    {
        var entryKey = fileUri.ToString().StartsWith(VirtualRoot)
            ? fileUri.ToString()[VirtualRoot.Length..]
            : fileUri.ToString();

        var candidateKeys = new[]
        {
            entryKey,
            entryKey + ".js",
            entryKey.TrimEnd('/') + "/index.js"
        };

        foreach (var key in candidateKeys)
        {
            var entry = archive.Entries.FirstOrDefault(e => e.Key == key);
            if (entry != null)
                return (entry, key);
        }

        return (null, null);
    }

    private static async Task<Document> CreateDocumentFromEntry(ZipArchiveEntry entry, Uri resolvedUri,
        DocumentCategory category, DocumentContextCallback contextCallback)
    {
        // Adjust category for JSON files
        if (resolvedUri.LocalPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            category = DocumentCategory.Json;

        var code = await ReadEntryContentAsync(entry);
        var localPath = "vfs://archive" + resolvedUri.LocalPath;
        var localDir = new Uri(new Uri(localPath), ".").ToString().TrimEnd('/');

        var finalCode = category == DocumentCategory.Json
            ? code
            : WrapCodeWithNodejsGlobals(code, localPath, localDir);

        return new StringDocument(
            new DocumentInfo(resolvedUri)
            {
                Category = category,
                ContextCallback = contextCallback
            },
            finalCode);
    }

    private static async Task<string> ReadEntryContentAsync(ZipArchiveEntry entry)
    {
        using var memoryStream = new MemoryStream();
        entry.WriteTo(memoryStream);
        memoryStream.Position = 0;

        using var reader = new StreamReader(memoryStream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}