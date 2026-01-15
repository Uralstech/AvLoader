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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;

#nullable enable
namespace Uralstech.AvLoader
{
    /// <summary>
    /// Loads avatars from on-device files.
    /// </summary>
    public class FileAvDataLoader : IAvDataLoader
    {
        private readonly string _modelFilePath, _metadataFilePath;
        private readonly AvModelFileExtension _modelFormat;

        private readonly string? _bustRenderFilePath, _fullRenderFilePath;
        private readonly AvImageFileExtension _bustRenderFormat, _fullRenderFormat;
        private readonly bool _searchForFiles = false;

        /// <summary>
        /// Creates a loader configured to load avatar data from the directory at <paramref name="basePath"/>.
        /// </summary>
        /// <remarks>
        /// <paramref name="basePath"/> <b>must</b> contain the avatar and associated data in the following structure:
        /// <code>
        /// basePath/
        /// ├─ model.[supported model extension]      # The avatar's model
        /// ├─ metadata.json                          # Metadata
        /// ├─ full.[supported image extension]       # Full render of the avatar (optional)
        /// └─ bust.[supported image extension]       # Bust render of the avatar (optional)
        /// </code>
        /// 
        /// If you want to use a different file structure, call <see cref="FileAvDataLoader(string, string, string?, string?)"/> instead.
        /// </remarks>
        /// <param name="basePath">The base directory containing the avatar and its metadata.</param>
        /// <param name="modelFormat">The expected file extension(s) of the model, leave as <see cref="AvModelFileExtension.None"/> to auto detect of all supported formats.</param>
        /// <param name="fullRenderFormat">The expected file extension(s) of the model's full render, leave as <see cref="AvImageFileExtension.None"/> to auto detect of all supported formats.</param>
        /// <param name="bustRenderFormat">The expected file extension(s) of the model's bust render, leave as <see cref="AvImageFileExtension.None"/> to auto detect of all supported formats.</param>
        /// <param name="shouldLoadFullRender">Should the full render be loaded?</param>
        /// <param name="shouldLoadBustRender">Should the bust render be loaded?</param>
        public FileAvDataLoader(string basePath, AvModelFileExtension modelFormat = AvModelFileExtension.None,
            AvImageFileExtension fullRenderFormat = AvImageFileExtension.None, AvImageFileExtension bustRenderFormat = AvImageFileExtension.None,
            bool shouldLoadFullRender = true, bool shouldLoadBustRender = true)
        {
            _modelFormat = modelFormat;
            _fullRenderFormat = fullRenderFormat;
            _bustRenderFormat = bustRenderFormat;

            _modelFilePath = basePath;
            _metadataFilePath = Path.Join(basePath, IOUtils.DefaultMetadataFile);

            if (shouldLoadFullRender) _fullRenderFilePath = basePath;
            if (shouldLoadBustRender) _bustRenderFilePath = basePath;
            _searchForFiles = true;
        }

        /// <summary>
        /// Creates a loader configured to load avatar data from paths to each file.
        /// </summary>
        /// <param name="modelFilePath">Path to the model.</param>
        /// <param name="metadataFilePath">Path to the avatar metadata.</param>
        /// <param name="fullRenderFilePath">Path to the full render of the avatar (optional).</param>
        /// <param name="bustRenderFilePath">Path to the bust render of the avatar (optional).</param>
        /// <exception cref="NotSupportedException">Thrown if the file extension of <paramref name="modelFilePath"/>, <paramref name="fullRenderFilePath"/> or <paramref name="bustRenderFilePath"/> is not supported.</exception>
        public FileAvDataLoader(string modelFilePath, string metadataFilePath, string? fullRenderFilePath = null, string? bustRenderFilePath = null)
        {
            _modelFilePath = modelFilePath;
            _metadataFilePath = metadataFilePath;
            _fullRenderFilePath = fullRenderFilePath;
            _bustRenderFilePath = bustRenderFilePath;

            string modelExtension = Path.GetExtension(_modelFilePath);
            if (!IOUtils.s_stringToModelFileExtensionLookup.TryGetValue(modelExtension, out _modelFormat))
                throw new NotSupportedException($"Model format with file extension '{modelExtension}' is not supported.");

            if (!string.IsNullOrEmpty(_fullRenderFilePath))
            {
                string fullRenderExtension = Path.GetExtension(_fullRenderFilePath);
                if (!IOUtils.s_stringToImageFileExtensionLookup.TryGetValue(fullRenderExtension, out _fullRenderFormat))
                    throw new NotSupportedException($"Image format with file extension '{fullRenderExtension}' is not supported.");
            }

            if (!string.IsNullOrEmpty(_bustRenderFilePath))
            {
                string bustRenderExtension = Path.GetExtension(_bustRenderFilePath);
                if (!IOUtils.s_stringToImageFileExtensionLookup.TryGetValue(bustRenderExtension, out _bustRenderFormat))
                    throw new NotSupportedException($"Image format with file extension '{bustRenderExtension}' is not supported.");
            }
        }

        /// <inheritdoc/>
        public async Awaitable<AvDataContainer?> LoadAvatarAsync(bool throwOnFail, CancellationToken token = default)
        {
            string? modelFilePath = _modelFilePath;
            AvModelFileExtension modelFormat = _modelFormat;

            string? fullRenderPath = _fullRenderFilePath;
            AvImageFileExtension fullRenderFormat = _fullRenderFormat;

            string? bustRenderPath = _bustRenderFilePath;
            AvImageFileExtension bustRenderFormat = _bustRenderFormat;

            if (_searchForFiles)
            {
                if (!TrySearchForModelFile(_modelFilePath, IOUtils.DefaultModelFile, _modelFormat, throwOnFail, out modelFilePath, out modelFormat))
                    return null;

                if (!string.IsNullOrEmpty(_fullRenderFilePath)
                    && !TrySearchForImageFile(_fullRenderFilePath, IOUtils.DefaultFullRenderFile, _fullRenderFormat, throwOnFail, out fullRenderPath, out fullRenderFormat))
                    return null;

                if (!string.IsNullOrEmpty(_bustRenderFilePath)
                    && !TrySearchForImageFile(_bustRenderFilePath, IOUtils.DefaultBustRenderFile, _bustRenderFormat, throwOnFail, out bustRenderPath, out bustRenderFormat))
                    return null;
            }

            try
            {
                string metadataJson = await File.ReadAllTextAsync(_metadataFilePath, token);
                if (!TryDecodeMetadata(metadataJson, throwOnFail, out AvMetadata? metadata))
                    return null;

                byte[] modelData = await File.ReadAllBytesAsync(modelFilePath, token);
                Texture2D? fullRender = null, bustRender = null;

                byte[]? fullRenderData = !string.IsNullOrEmpty(_fullRenderFilePath)
                    ? await File.ReadAllBytesAsync(fullRenderPath, token) : null;

                if (fullRenderData is not null && !TryDecodeImage(fullRenderData, nameof(AvDataContainer.FullRender), throwOnFail, out fullRender))
                    return null;

                byte[]? bustRenderData = !string.IsNullOrEmpty(_bustRenderFilePath)
                    ? await File.ReadAllBytesAsync(bustRenderPath, token) : null;

#pragma warning disable IDE0046 // Convert to conditional expression
                if (bustRenderData is not null && !TryDecodeImage(bustRenderData, nameof(AvDataContainer.BustRender), throwOnFail, out bustRender))
                    return null;
#pragma warning restore IDE0046 // Convert to conditional expression

                return new AvDataContainer(modelData, modelFormat, modelFilePath, metadata.Value, typeof(FileAvDataLoader), fullRender, bustRender);
            }
            catch (SystemException ex)
            {
                if (throwOnFail) throw;
                Debug.LogWarning($"{nameof(FileAvDataLoader)}: Could not load avatar data due to exception:\n{ex}");
                return null;
            }
        }

        private static bool TrySearchForModelFile(string basePath, string fileName, AvModelFileExtension baseExt, bool throwOnFail,
            [NotNullWhen(true)] out string? resultPath, out AvModelFileExtension resultExtension)
        {
            resultPath = null; resultExtension = baseExt;
            if (IOUtils.s_modelFileExtensionToStringLookup.TryGetValue(baseExt, out string? modelExtension))
            {
                resultPath = Path.Join(basePath, $"{fileName}{modelExtension}");
                return true;
            }

            KeyValuePair<string, AvModelFileExtension>[] validExtensions = baseExt is AvModelFileExtension.GLTFAny
                ? IOUtils.s_gltfStringAndModelFileExtensionPairs : IOUtils.s_modelStringAndModelFileExtensionPairs;

            foreach (KeyValuePair<string, AvModelFileExtension> pair in validExtensions)
            {
                string path = Path.Join(basePath, $"{fileName}{pair.Key}");
                if (!File.Exists(path))
                    continue;

                resultPath = path;
                resultExtension = pair.Value;
                return true;
            }

            string errorMessage = $"File '{fileName}' with matching extensions for '{baseExt}' were not found at directory '{basePath}'.";
            if (throwOnFail) throw new FileNotFoundException(errorMessage);

            Debug.LogWarning($"{nameof(FileAvDataLoader)}: {errorMessage}");
            return false;
        }

        private static bool TrySearchForImageFile(string basePath, string fileName, AvImageFileExtension baseExt, bool throwOnFail,
            [NotNullWhen(true)] out string? resultPath, out AvImageFileExtension resultExtension)
        {
            resultPath = null; resultExtension = baseExt;
            if (IOUtils.s_imageFileExtensionToStringLookup.TryGetValue(baseExt, out string? modelExtension))
            {
                resultPath = Path.Join(basePath, $"{fileName}{modelExtension}");
                return true;
            }

            KeyValuePair<string, AvImageFileExtension>[] validExtensions = baseExt is AvImageFileExtension.JPEGAny
                ? IOUtils.s_jpegStringAndImageFileExtensionPairs : IOUtils.s_imageStringAndImageFileExtensionPairs;
            
            foreach (KeyValuePair<string, AvImageFileExtension> pair in validExtensions)
            {
                string path = Path.Join(basePath, $"{fileName}{pair.Key}");
                if (!File.Exists(path))
                    continue;

                resultPath = path;
                resultExtension = pair.Value;
                return true;
            }

            string errorMessage = $"File '{fileName}' with matching extensions for '{baseExt}' were not found at directory '{basePath}'.";
            if (throwOnFail) throw new FileNotFoundException(errorMessage);

            Debug.LogWarning($"{nameof(FileAvDataLoader)}: {errorMessage}");
            return false;
        }

        private static bool TryDecodeMetadata(string json, bool throwOnFail, [NotNullWhen(true)] out AvMetadata? result)
        {
            try
            {
                result = JsonConvert.DeserializeObject<AvMetadata>(json);
                return result.HasValue;
            }
            catch (JsonException ex)
            {
                if (throwOnFail) throw;
                Debug.LogWarning($"{nameof(FileAvDataLoader)}: Could not load {nameof(AvMetadata)} due to JSON exception:\n{ex}");

                result = null;
                return false;
            }
        }

        private static bool TryDecodeImage(byte[]? data, string name, bool throwOnFail, [NotNullWhen(true)] out Texture2D? result)
        {
            result = null;
            if (data is null) return false;

            Texture2D tex = new(2,2) { name = name };
            if (tex.LoadImage(data))
            {
                result = tex;
                return true;
            }
            
            if (throwOnFail) throw new InvalidDataException($"Could not load {name} as Texture2D.");
            Debug.LogWarning($"{nameof(FileAvDataLoader)}: Could not load {name} as Texture2D.");
            
            UnityEngine.Object.Destroy(tex);
            return false;
        }
    }
}