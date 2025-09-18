using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using SharpCompress.Archives.Zip;
using Showdown.NET.Core;

namespace Showdown.NET;

[PublicAPI]
public static class ShowdownHost
{
    private static bool _initialized;
    private static readonly object InitLock = new();
    internal static ShowdownEngine? Engine;

    static ShowdownHost()
    {
        NativeLibrary.SetDllImportResolver(
            typeof(V8Runtime).Assembly, (name, _, _) =>
            {
                if (NativeLibrary.TryLoad(name, out var existingHandle)) return existingHandle;

                var searchDirs = HostSettings.AuxiliarySearchPath.Split(';');
                foreach (var searchDir in searchDirs)
                {
                    var libraryPath = Path.Combine(searchDir, name);
                    if (!File.Exists(libraryPath)) continue;
                    try
                    {
                        var libPtr = NativeLibrary.Load(libraryPath);
                        return libPtr;
                    }
                    catch
                    {
                        // Ignore load failures and continue searching other directories
                    }
                }

                return IntPtr.Zero;
            });
    }

    public static void Init(string showdownDistPath = @".\pokemon-showdown\dist", string? v8RuntimeSearchPath = null)
    {
        var absolutePath = Path.GetFullPath(showdownDistPath);
        if (!Directory.Exists(absolutePath))
            throw new DirectoryNotFoundException($"Path does not exist: '{absolutePath}'");

        lock (InitLock)
        {
            if (_initialized) return;
            SetV8Path(v8RuntimeSearchPath);
            Engine = new ShowdownEngine(absolutePath);
            _initialized = true;
        }
    }

    public static void InitFromArchive(Stream showdownArchiveStream, string? v8RuntimeSearchPath = null)
    {
        ArgumentNullException.ThrowIfNull(showdownArchiveStream);

        lock (InitLock)
        {
            if (_initialized) return;
            SetV8Path(v8RuntimeSearchPath);
            var archive = ZipArchive.Open(showdownArchiveStream);
            Engine = new ShowdownEngine(archive);
            _initialized = true;
        }
    }
    
    public static void Unload()
    {
        lock (InitLock)
        {
            if (!_initialized) return;
            
            Engine?.Dispose();
            Engine = null;
            _initialized = false;
        }
    }

    private static void SetV8Path(string? v8RuntimeSearchPath)
    {
        if (v8RuntimeSearchPath != null)
            HostSettings.AuxiliarySearchPath = v8RuntimeSearchPath;
    }

    internal static void EnsureInitialized()
    {
        lock (InitLock)
        {
            if (!_initialized)
                throw new InvalidOperationException(
                    "Showdown has not been initialized. Did you forget to call Showdown.Init(pathToDist) first?"
                );
        }
    }
}