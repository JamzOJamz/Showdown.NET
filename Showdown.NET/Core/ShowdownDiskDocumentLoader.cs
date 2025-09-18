using Microsoft.ClearScript;

namespace Showdown.NET.Core;

internal class ShowdownDiskDocumentLoader : ShowdownDocumentLoader
{
    public override async Task<Document> LoadDocumentAsync(
        DocumentSettings settings,
        DocumentInfo? sourceInfo,
        string specifier,
        DocumentCategory category,
        DocumentContextCallback contextCallback)
    {
        // Try base implementation first
        var result = base.LoadDocumentAsync(settings, sourceInfo, specifier, category, contextCallback);
        if (result != null) return await result;

        var baseUri = sourceInfo?.Uri;

        // Delegate to default loader if no base URI
        if (baseUri is null)
            return await Default.LoadDocumentAsync(settings, sourceInfo, specifier, category, contextCallback);

        // Resolve the file path
        var resolvedPath = ResolveFilePath(baseUri, specifier);
        var resolvedUri = new Uri(resolvedPath, UriKind.Absolute);

        // Return cached document if available
        var cachedDocument = GetCachedDocument(resolvedUri);
        if (cachedDocument != null)
            return cachedDocument;

        // Override category to Json if the URI ends with .json
        if (resolvedUri.LocalPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            category = DocumentCategory.Json;

        // Load and cache the document
        var document = await CreateDocumentFromFile(resolvedUri, category, contextCallback);
        CacheDocument(document, false);

        return document;
    }

    private static string ResolveFilePath(Uri baseUri, string specifier)
    {
        var baseDir = Path.GetDirectoryName(baseUri.LocalPath)!;
        var resolvedPath = Path.GetFullPath(Path.Combine(baseDir, specifier));

        // Try exact path first
        if (File.Exists(resolvedPath))
            return resolvedPath;

        // Try with .js extension
        var jsFile = resolvedPath + ".js";
        if (File.Exists(jsFile))
            return jsFile;

        // Try index.js in directory
        var indexFile = Path.Combine(resolvedPath, "index.js");
        return File.Exists(indexFile)
            ? indexFile
            :
            // File not found - return original path for error handling
            resolvedPath;
    }

    private static async Task<Document> CreateDocumentFromFile(
        Uri resolvedUri,
        DocumentCategory category,
        DocumentContextCallback contextCallback)
    {
        var localPath = resolvedUri.LocalPath;

        if (!File.Exists(localPath)) return CreateModuleNotFoundDocument(resolvedUri, category, contextCallback);

        var localDir = Path.GetDirectoryName(localPath)
                       ?? throw new InvalidOperationException("Resolved path has no directory.");

        var code = await File.ReadAllTextAsync(localPath);
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
}