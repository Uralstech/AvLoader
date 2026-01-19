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
using UnityEngine;
using Uralstech.AvLoader.Utils;

#nullable enable
namespace Uralstech.AvLoader.DataLoaders
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
            bool shouldLoadFullRender = false, bool shouldLoadBustRender = false)
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
        public async Awaitable<AvSourceData?> LoadAvatarAsync(bool throwOnFail, CancellationToken token = default)
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

            if (!AvMetadata.TryCreateFromFile(_metadataFilePath, out AvMetadata? metadata, throwOnFail))
                return null;

            try
            {
                byte[] modelData = await File.ReadAllBytesAsync(modelFilePath, token);
                Texture2D? fullRender = null, bustRender = null;

                byte[]? fullRenderData = !string.IsNullOrEmpty(_fullRenderFilePath)
                    ? await File.ReadAllBytesAsync(fullRenderPath, token) : null;

                if (fullRenderData is not null && !fullRenderData.TryDecodeImage(out fullRender, nameof(AvSourceData.FullRender), throwOnFail))
                    return null;

                byte[]? bustRenderData = !string.IsNullOrEmpty(_bustRenderFilePath)
                    ? await File.ReadAllBytesAsync(bustRenderPath, token) : null;

#pragma warning disable IDE0046 // Convert to conditional expression
                if (bustRenderData is not null && !bustRenderData.TryDecodeImage(out bustRender, nameof(AvSourceData.BustRender), throwOnFail))
                    return null;
#pragma warning restore IDE0046 // Convert to conditional expression

                return new AvSourceData(modelData, modelFormat, modelFilePath, metadata.Value, typeof(FileAvDataLoader), fullRender, bustRender);
            }
            catch (SystemException ex)
            {
                if (throwOnFail) throw;
                Debug.LogWarning($"{nameof(FileAvDataLoader)}: Could not load avatar data due to exception:\n{ex}");
                return null;
            }
        }

        /// <summary>Searches for a supported avatar model file inside <paramref name="directory"/> using the default model file name.</summary>
        /// <param name="directory">The directory to search in.</param>
        /// <param name="searchFilter">The expected model file extension(s). Use <see cref="AvModelFileExtension.None"/> or grouped values to search across all supported formats.</param>
        /// <param name="foundPath">When this method returns <see langword="true"/>, contains the full path to the found model file.</param>
        /// <param name="foundExtension">When this method returns <see langword="true"/>, contains the detected model file extension.</param>
        /// <returns><see langword="true"/> if a valid model file was found; otherwise, <see langword="false"/>.</returns>
        public static bool TrySearchForModelFile(string directory, AvModelFileExtension searchFilter,
            [NotNullWhen(true)] out string? foundPath, out AvModelFileExtension foundExtension) =>
            TrySearchForModelFile(directory, IOUtils.DefaultModelFile, searchFilter, false, out foundPath, out foundExtension);

        /// <summary>Searches for a supported avatar model file inside <paramref name="directory"/> using a custom base file name.</summary>
        /// <param name="fileNameWithoutExtensino">The model file name without an extension.</param>
        /// <inheritdoc cref="TrySearchForModelFile(string, AvModelFileExtension, out string?, out AvModelFileExtension)"/>
        public static bool TrySearchForModelFile(string directory, string fileNameWithoutExtensino, AvModelFileExtension searchFilter,
            [NotNullWhen(true)] out string? foundPath, out AvModelFileExtension foundExtension) =>
            TrySearchForModelFile(directory, fileNameWithoutExtensino, searchFilter, false, out foundPath, out foundExtension);

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

            string errorMessage = $"A valid model file with name '{fileName}' was not found at directory '{basePath}', provided extension for search: '{baseExt}'.";
            if (throwOnFail) throw new FileNotFoundException(errorMessage);

            Debug.LogWarning($"{nameof(FileAvDataLoader)}: {errorMessage}");
            return false;
        }

        /// <summary>Searches for a supported full-render image file inside <paramref name="directory"/> using the default full-render file name.</summary>
        /// <param name="directory">The directory to search in.</param>
        /// <param name="searchFilter">The expected image file extension(s). Use <see cref="AvImageFileExtension.None"/> or grouped values to search across all supported formats.</param>
        /// <param name="foundPath">When this method returns <see langword="true"/>, contains the full path to the found image file.</param>
        /// <param name="foundExtension">When this method returns <see langword="true"/>, contains the detected image file extension.</param>
        /// <returns><see langword="true"/> if a valid image file was found; otherwise, <see langword="false"/>.</returns>
        public static bool TrySearchForFullRender(string directory, AvImageFileExtension searchFilter,
            [NotNullWhen(true)] out string? foundPath, out AvImageFileExtension foundExtension) =>
            TrySearchForImageFile(directory, IOUtils.DefaultFullRenderFile, searchFilter, false, out foundPath, out foundExtension);

        /// <summary>Searches for a supported bust-render image file inside <paramref name="directory"/> using the default bust-render file name.</summary>
        /// <inheritdoc cref="TrySearchForFullRender(string, AvImageFileExtension, out string?, out AvImageFileExtension)"/>
        public static bool TrySearchForBustRender(string directory, AvImageFileExtension searchFilter,
            [NotNullWhen(true)] out string? foundPath, out AvImageFileExtension foundExtension) =>
            TrySearchForImageFile(directory, IOUtils.DefaultBustRenderFile, searchFilter, false, out foundPath, out foundExtension);

        /// <summary>Searches for a supported image file inside <paramref name="directory"/> using a custom base file name.</summary>
        /// <param name="fileNameWithoutExtensino">The image file name without an extension.</param>
        /// <inheritdoc cref="TrySearchForFullRender(string, AvImageFileExtension, out string?, out AvImageFileExtension)"/>
        public static bool TrySearchForImageFile(string directory, string fileNameWithoutExtensino, AvImageFileExtension searchFilter,
            [NotNullWhen(true)] out string? foundPath, out AvImageFileExtension foundExtension) =>
            TrySearchForImageFile(directory, fileNameWithoutExtensino, searchFilter, false, out foundPath, out foundExtension);

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

            string errorMessage = $"A valid image file with name '{fileName}' was not found at directory '{basePath}', provided extension for search: '{baseExt}'.";
            if (throwOnFail) throw new FileNotFoundException(errorMessage);

            Debug.LogWarning($"{nameof(FileAvDataLoader)}: {errorMessage}");
            return false;
        }
    }
}