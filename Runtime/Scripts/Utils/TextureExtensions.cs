// Copyright 2026 URAV ADVANCED LEARNING SYSTEMS PRIVATE LIMITED
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

#nullable enable
namespace Uralstech.AvLoader.Utils
{
    /// <summary>
    /// Utility extensions for texture-related operations.
    /// </summary>
    /// <remarks>
    /// This class is <see langword="public"/> to allow package users to reuse these extensions if useful.
    /// However, it should not be considered stable and is not part of the supported public API.
    /// It exists solely as an internal utility and may change or be removed at any time.
    /// </remarks>
    public static class TextureExtensions
    {
        /// <summary>
        /// Awaits for the <see cref="DownloadHandlerTexture"/> of the current <see cref="UnityWebRequest"/> to finish processing its texture.
        /// </summary>
        /// <param name="name">Name for error logs.</param>
        /// <param name="throwOnFail">Should the method throw if it couldn't load the image data?</param>
        public static async Awaitable<Texture2D?> TryGetTextureAsync(this UnityWebRequest? request, bool throwOnFail = false, string? name = null, CancellationToken token = default)
        {
            if (request?.downloadHandler is not DownloadHandlerTexture downloadHandler)
            {
                if (throwOnFail) throw new ArgumentException($"Download handler is not of type {nameof(DownloadHandlerTexture)} (name: {name}).");
                Debug.LogWarning($"{nameof(TextureExtensions)}: Download handler is not of type {nameof(DownloadHandlerTexture)} (name: {name}).");
                return null;
            }

            while (!downloadHandler.isDone)
                await Awaitable.NextFrameAsync(token);

            Texture2D? texture = downloadHandler.texture;
            if (texture != null) return texture;

            if (throwOnFail) throw new System.IO.InvalidDataException($"Could not get texture from download handler (name: {name}).");
            Debug.LogWarning($"{nameof(TextureExtensions)}: Could not get texture from download handler (name: {name}).");
            return null;
        }
        
        /// <summary>
        /// Tries decoding a <see cref="Texture2D"/> from binary data using <see cref="ImageConversion.LoadImage(Texture2D, ReadOnlySpan{byte})"/>.
        /// </summary>
        /// <param name="name">Name to give the created texture, and for error logs.</param>
        /// <param name="throwOnFail">Should the method throw if it couldn't load the image data?</param>
        public static bool TryDecodeImage(this ReadOnlySpan<byte> current, [NotNullWhen(true)] out Texture2D? result, string? name = null, bool throwOnFail = false)
        {
            result = null;
            if (current.IsEmpty)
                return false;

            Texture2D tex = new(2,2) { name = name };
            if (tex.LoadImage(current))
            {
                result = tex;
                return true;
            }
            
            UnityEngine.Object.Destroy(tex);

            if (throwOnFail) throw new System.IO.InvalidDataException($"Could not load binary data as Texture2D (name: {name}).");
            Debug.LogWarning($"{nameof(TextureExtensions)}: Could not load binary data as Texture2D (name: {name}).");
            return false;
        }

        /// <inheritdoc cref="TryDecodeImage(ReadOnlySpan{byte}, out Texture2D?, string?, bool)"/>
        public static bool TryDecodeImage(this byte[]? current, [NotNullWhen(true)] out Texture2D? result, string? name = null, bool throwOnFail = false) =>
            TryDecodeImage((ReadOnlySpan<byte>)current, out result, name, throwOnFail);
    }
}