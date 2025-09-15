using Microsoft.ClearScript;

namespace Showdown.NET;

internal class ShowdownDocumentLoader : DocumentLoader
{
    private static readonly Dictionary<string, string> Polyfills = new(StringComparer.OrdinalIgnoreCase)
    {
        {
            "fs",
            @"
function readdirSync(path) {
    return hostFs.readdirSync(path);
}

module.exports = {
    readdirSync
};
"
        },
        {
            "path",
            @"
function isWindowsAbsolute(path) {
    // e.g. C:/... or C:\...
    return /^[a-zA-Z]:[\\/]/.test(path);
}

function isPosixAbsolute(path) {
    // e.g. /usr/bin
    return path.startsWith('/');
}

function isAbsolute(path) {
    return isPosixAbsolute(path) || isWindowsAbsolute(path);
}

function join(...args) {
    // Filter out empty/invalid segments
    const filtered = args.filter(arg => arg && typeof arg === 'string');
    
    // Concatenate with slash
    const concatenated = filtered.join('/');
    
    // Normalize using resolve
    const normalized = resolve(concatenated);
    
    // If the first segment wasn’t absolute, strip any leading slash
    if (filtered.length && !isAbsolute(filtered[0])) {
        return normalized.replace(/^\/+/, '');
    }

    return normalized;
}

function resolve(...args) {
    let resolvedPath = '';
    let absoluteFound = false;
    let sawWindowsDrive = false;

    // Iterate backwards through args
    for (let i = args.length - 1; i >= 0 && !absoluteFound; i--) {
        let segment = args[i];
        if (!segment) continue;

        resolvedPath = segment + '/' + resolvedPath;

        if (isAbsolute(segment)) {
            absoluteFound = true;
            if (isWindowsAbsolute(segment)) {
                sawWindowsDrive = true;
            }
        }
    }

    // If no absolute path was found, fall back to POSIX root `/`
    if (!absoluteFound) {
        resolvedPath = '/' + resolvedPath;
    }

    const parts = resolvedPath.split(/[\\/]+/);
    const finalParts = [];

    for (const part of parts) {
        if (part === '' || part === '.') continue;
        if (part === '..') {
            if (finalParts.length && finalParts[finalParts.length - 1] !== '..') {
                finalParts.pop();
            } else if (!absoluteFound) {
                finalParts.push('..');
            }
        } else {
            finalParts.push(part);
        }
    }

    let result;

    if (sawWindowsDrive) {
        // Keep Windows drive letter as-is
        result = finalParts.join('/');
    } else if (absoluteFound) {
        // POSIX absolute path
        result = '/' + finalParts.join('/');
    } else {
        // Relative path fallback
        result = finalParts.join('/') || '.';
    }

    return result;
}

module.exports = {
    join,
    resolve
};
"
        },
        {
            "node:util",
            @"
var envHasBigInt64Array = typeof BigInt64Array !== 'undefined';

function equal(a, b) {
  if (a === b) return true;

  if (a && b && typeof a == 'object' && typeof b == 'object') {
    if (a.constructor !== b.constructor) return false;

    var length, i, keys;
    if (Array.isArray(a)) {
      length = a.length;
      if (length != b.length) return false;
      for (i = length; i-- !== 0;)
        if (!equal(a[i], b[i])) return false;
      return true;
    }

    if ((a instanceof Map) && (b instanceof Map)) {
      if (a.size !== b.size) return false;
      for (i of a.entries())
        if (!b.has(i[0])) return false;
      for (i of a.entries())
        if (!equal(i[1], b.get(i[0]))) return false;
      return true;
    }

    if ((a instanceof Set) && (b instanceof Set)) {
      if (a.size !== b.size) return false;
      for (i of a.entries())
        if (!b.has(i[0])) return false;
      return true;
    }

    if (ArrayBuffer.isView(a) && ArrayBuffer.isView(b)) {
      length = a.length;
      if (length != b.length) return false;
      for (i = length; i-- !== 0;)
        if (a[i] !== b[i]) return false;
      return true;
    }

    if (a.constructor === RegExp) return a.source === b.source && a.flags === b.flags;
    if (a.valueOf !== Object.prototype.valueOf) return a.valueOf() === b.valueOf();
    if (a.toString !== Object.prototype.toString) return a.toString() === b.toString();

    keys = Object.keys(a);
    length = keys.length;
    if (length !== Object.keys(b).length) return false;

    for (i = length; i-- !== 0;)
      if (!Object.prototype.hasOwnProperty.call(b, keys[i])) return false;

    for (i = length; i-- !== 0;) {
      var key = keys[i];
      if (!equal(a[key], b[key])) return false;
    }

    return true;
  }

  // true if both NaN, false otherwise
  return a !== a && b !== b;
}

function isDeepStrictEqual(a, b) {
  return equal(a, b);
}

module.exports = {
    isDeepStrictEqual
};
"
        },
        {
            "ts-chacha20",
            @"
""use strict"";
/*
 * Copyright (c) 2017, Bubelich Mykola
 * https://www.bubelich.com
 *
 * (｡◕‿‿◕｡)
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met, 0x
 *
 * Redistributions of source code must retain the above copyright notice,
 * this list of conditions and the following disclaimer.
 *
 * Redistributions in binary form must reproduce the above copyright notice,
 * this list of conditions and the following disclaimer in the documentation
 * and/or other materials provided with the distribution.
 *
 * Neither the name of the copyright holder nor the names of its contributors
 * may be used to endorse or promote products derived from this software without
 * specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER AND CONTRIBUTORS ""AS IS""
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *
 * ChaCha20 is a stream cipher designed by D. J. Bernstein.
 * It is a refinement of the Salsa20 algorithm, and it uses a 256-bit key.
 *
 * ChaCha20 successively calls the ChaCha20 block function, with the same key and nonce, and with successively increasing block counter parameters.
 * ChaCha20 then serializes the resulting state by writing the numbers in little-endian order, creating a keystream block.
 *
 * Concatenating the keystream blocks from the successive blocks forms a keystream.
 * The ChaCha20 function then performs an XOR of this keystream with the plaintext.
 * Alternatively, each keystream block can be XORed with a plaintext block before proceeding to create the next block, saving some memory.
 * There is no requirement for the plaintext to be an integral multiple of 512 bits.  If there is extra keystream from the last block, it is discarded.
 *
 * The inputs to ChaCha20 are
 * - 256-bit key
 * - 32-bit initial counter
 * - 96-bit nonce.  In some protocols, this is known as the Initialization Vector
 * - Arbitrary-length plaintext
 *
 * Implementation derived from chacha-ref.c version 20080118
 * See for details, 0x http, 0x//cr.yp.to/chacha/chacha-20080128.pdf
 */
/**
 *
 * @param {Uint8Array} key
 * @param {Uint8Array} nonce
 * @param {number} counter
 * @throws {Error}
 *
 * @constructor
 */
var Chacha20 = /** @class */ (function () {
    function Chacha20(key, nonce, counter) {
        this.key = key;
        this.nonce = nonce;
        this.counter = counter;
        this._rounds = 20;
        // Constants
        this._sigma = [0x61707865, 0x3320646e, 0x79622d32, 0x6b206574];
        // internal byte counter //
        this._byteCounter = 0;
        if (!(key instanceof Uint8Array) || key.length !== 32) {
            throw new Error('Key should be 32 byte array!');
        }
        if (!(nonce instanceof Uint8Array) || nonce.length !== 12) {
            throw new Error('Nonce should be 12 byte array!');
        }
        if (!counter) {
            this.counter = 0;
        }
        // param construction
        this._param = [
            this._sigma[0],
            this._sigma[1],
            this._sigma[2],
            this._sigma[3],
            // key
            this._get32(key, 0),
            this._get32(key, 4),
            this._get32(key, 8),
            this._get32(key, 12),
            this._get32(key, 16),
            this._get32(key, 20),
            this._get32(key, 24),
            this._get32(key, 28),
            // counter
            this.counter,
            // nonce
            this._get32(nonce, 0),
            this._get32(nonce, 4),
            this._get32(nonce, 8)
        ];
        // init 64 byte keystream block //
        this._keystream = [
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        ];
    }
    Chacha20.prototype._chacha = function () {
        var mix = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        var i = 0;
        var b = 0;
        // copy param array to mix //
        for (i = 0; i < 16; i++) {
            mix[i] = this._param[i];
        }
        // mix rounds //
        for (i = 0; i < this._rounds; i += 2) {
            this._quarterround(mix, 0, 4, 8, 12);
            this._quarterround(mix, 1, 5, 9, 13);
            this._quarterround(mix, 2, 6, 10, 14);
            this._quarterround(mix, 3, 7, 11, 15);
            this._quarterround(mix, 0, 5, 10, 15);
            this._quarterround(mix, 1, 6, 11, 12);
            this._quarterround(mix, 2, 7, 8, 13);
            this._quarterround(mix, 3, 4, 9, 14);
        }
        for (i = 0; i < 16; i++) {
            // add
            mix[i] += this._param[i];
            // store keystream
            this._keystream[b++] = mix[i] & 0xFF;
            this._keystream[b++] = (mix[i] >>> 8) & 0xFF;
            this._keystream[b++] = (mix[i] >>> 16) & 0xFF;
            this._keystream[b++] = (mix[i] >>> 24) & 0xFF;
        }
    };
    /**
     * The basic operation of the ChaCha algorithm is the quarter round.
     * It operates on four 32-bit unsigned integers, denoted a, b, c, and d.
     *
     * @param {Array} output
     * @param {number} a
     * @param {number} b
     * @param {number} c
     * @param {number} d
     * @private
     */
    Chacha20.prototype._quarterround = function (output, a, b, c, d) {
        output[d] = this._rotl(output[d] ^ (output[a] += output[b]), 16);
        output[b] = this._rotl(output[b] ^ (output[c] += output[d]), 12);
        output[d] = this._rotl(output[d] ^ (output[a] += output[b]), 8);
        output[b] = this._rotl(output[b] ^ (output[c] += output[d]), 7);
        // JavaScript hack to make UINT32 :) //
        output[a] >>>= 0;
        output[b] >>>= 0;
        output[c] >>>= 0;
        output[d] >>>= 0;
    };
    /**
     * Little-endian to uint 32 bytes
     *
     * @param {Uint8Array|[number]} data
     * @param {number} index
     * @return {number}
     * @private
     */
    Chacha20.prototype._get32 = function (data, index) {
        return data[index++] ^ (data[index++] << 8) ^ (data[index++] << 16) ^ (data[index] << 24);
    };
    /**
     * Cyclic left rotation
     *
     * @param {number} data
     * @param {number} shift
     * @return {number}
     * @private
     */
    Chacha20.prototype._rotl = function (data, shift) {
        return ((data << shift) | (data >>> (32 - shift)));
    };
    /**
     *  Encrypt data with key and nonce
     *
     * @param {Uint8Array} data
     * @return {Uint8Array}
     */
    Chacha20.prototype.encrypt = function (data) {
        return this._update(data);
    };
    /**
     *  Decrypt data with key and nonce
     *
     * @param {Uint8Array} data
     * @return {Uint8Array}
     */
    Chacha20.prototype.decrypt = function (data) {
        return this._update(data);
    };
    /**
     *  Encrypt or Decrypt data with key and nonce
     *
     * @param {Uint8Array} data
     * @return {Uint8Array}
     * @private
     */
    Chacha20.prototype._update = function (data) {
        if (!(data instanceof Uint8Array) || data.length === 0) {
            throw new Error('Data should be type of bytes (Uint8Array) and not empty!');
        }
        var output = new Uint8Array(data.length);
        // core function, build block and xor with input data //
        for (var i = 0; i < data.length; i++) {
            if (this._byteCounter === 0 || this._byteCounter === 64) {
                // generate new block //
                this._chacha();
                // counter increment //
                this._param[12]++;
                // reset internal counter //
                this._byteCounter = 0;
            }
            output[i] = data[i] ^ this._keystream[this._byteCounter++];
        }
        return output;
    };
    return Chacha20;
}());

module.exports = {
    Chacha20
};
"
        },
        {
            "node:crypto",
            @"
function getRandomValues(array) {
    return hostCrypto.getRandomValues(array);
}

module.exports = {
    getRandomValues
};
"
        }
    };
    
    private readonly Dictionary<Uri, Document> _documentCache = new();

    public override async Task<Document> LoadDocumentAsync(
        DocumentSettings settings,
        DocumentInfo? sourceInfo,
        string specifier,
        DocumentCategory category,
        DocumentContextCallback contextCallback)
    {
        // Check for polyfill first
        if (TryLoadPolyfill(specifier, category, contextCallback, out var polyfillDocument))
            return polyfillDocument;

        var baseUri = sourceInfo?.Uri;

        // Delegate to default loader if no base URI
        if (baseUri is null)
            return await Default.LoadDocumentAsync(settings, sourceInfo, specifier, category, contextCallback);

        // Resolve the file path
        var resolvedPath = ResolveFilePath(baseUri, specifier);
        var resolvedUri = new Uri(resolvedPath, UriKind.Absolute);

        // Return cached document if available
        if (_documentCache.TryGetValue(resolvedUri, out var cachedDocument))
            return cachedDocument;

        // Override category to Json if the URI ends with .json
        if (resolvedUri.LocalPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            category = DocumentCategory.Json;

        // Load and cache the document
        var document = await CreateDocumentFromFile(resolvedUri, category, contextCallback);
        _documentCache[resolvedUri] = document;

        return document;
    }

    private static bool TryLoadPolyfill(
        string specifier,
        DocumentCategory category,
        DocumentContextCallback contextCallback,
        out Document document)
    {
        if (Polyfills.TryGetValue(specifier, out var polyfillCode))
        {
            var documentInfo = new DocumentInfo(specifier)
            {
                Category = category,
                ContextCallback = contextCallback
            };
            document = new StringDocument(documentInfo, polyfillCode);
            return true;
        }

        document = null!;
        return false;
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

    private static StringDocument CreateModuleNotFoundDocument(
        Uri resolvedUri,
        DocumentCategory category,
        DocumentContextCallback contextCallback)
    {
        var documentInfo = new DocumentInfo(resolvedUri)
        {
            Category = category,
            ContextCallback = contextCallback
        };

        return new StringDocument(documentInfo, "hostThrow('MODULE_NOT_FOUND');");
    }

    private static string WrapCodeWithNodejsGlobals(string code, string localPath, string localDir)
    {
        var escapedPath = EscapeForJsString(localPath);
        var escapedDir = EscapeForJsString(localDir);

        return $"""
                const __filename = "{escapedPath}";
                const __dirname = "{escapedDir}";
                {code}
                """;
    }

    private static string EscapeForJsString(string path)
    {
        return path.Replace("\\", @"\\").Replace("\"", "\\\"");
    }
}