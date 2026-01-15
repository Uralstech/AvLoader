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
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;

#nullable enable
namespace Uralstech.AvLoader
{
    /// <summary>
    /// Post-processing step that saves the loaded avatar's data to a local directory
    /// for future use. Skips execution if the avatar was originally loaded using
    /// <see cref="FileAvDataLoader"/>, unless overridden via <see cref="IgnoreOriginalLoaderType"/>.
    /// </summary>
    /// <remarks>
    /// This only works for formats where the model does not depend on resources
    /// stored separately from the source file (<see cref="AvDataContainer.Model"/>).
    /// For example, avatars loaded from OBJ files with external texture files
    /// will not work.
    /// </remarks>
    public class CacheModelData : IAsyncAvPostProcessor
    {
        /// <summary>
        /// The base directory to save the avatar to.
        /// </summary>
        /// <remarks>
        /// If <see cref="UseAvatarIdAsDirName"/> is <see langword="true"/> (default),
        /// the avatar will be stored in a new directory at "<see cref="BaseDirectory"/>/<see cref="AvMetadata.Id"/>".
        /// </remarks>
        public string BaseDirectory;
        
        private AvImageFileExtension _fullRenderImageFormat = AvImageFileExtension.JPG;
        
        /// <summary>The file extension and format for saving the avatar's full render. Cannot be presumptive, like <see cref="AvImageFileExtension.JPEGAny"/>.</summary>
        public AvImageFileExtension FullRenderImageFormat
        {
            get => _fullRenderImageFormat;
            set
            {
                if (value is AvImageFileExtension.JPEGAny or AvImageFileExtension.None)
                    throw new ArgumentException($"Unsupported file extension for caching avatar renders: {value}", nameof(value));

                _fullRenderImageFormat = value;
            }
        }
        
        private AvImageFileExtension _bustRenderImageFormat = AvImageFileExtension.JPG;

        /// <summary>The file extension and format for saving the avatar's bust render. Cannot be presumptive, like <see cref="AvImageFileExtension.JPEGAny"/>.</summary>
        public AvImageFileExtension BustRenderImageFormat
        {
            get => _bustRenderImageFormat;
            set
            {
                if (value is AvImageFileExtension.JPEGAny or AvImageFileExtension.None)
                    throw new ArgumentException($"Unsupported file extension for caching avatar renders: {value}", nameof(value));

                _bustRenderImageFormat = value;
            }
        }

        /// <summary>The quality to encode the avatar's full render, if saving as JPEG.</summary>
        public int FullRenderJPEGQuality = 100;

        /// <summary>The quality to encode the avatar's bust render, if saving as JPEG.</summary>
        public int BustRenderJPEGQuality = 100;

        /// <summary>Should the avatar be saved even if it was loaded from the local filesystem?</summary>
        public bool IgnoreOriginalLoaderType;

        /// <summary>
        /// If <see langword="true"/> (default), the avatar will be stored in a new directory at "<see cref="BaseDirectory"/>/<see cref="AvMetadata.Id"/>".
        /// Otherwise, <see cref="BaseDirectory"/> is used as-is.
        /// </summary>
        public bool UseAvatarIdAsDirName;

        public CacheModelData(string baseDirectory, bool useAvatarIdAsChildDirName = true)
        {
            BaseDirectory = baseDirectory;
            UseAvatarIdAsDirName = useAvatarIdAsChildDirName;
        }

        /// <inheritdoc/>
        public async Awaitable PostProcessAsync(LoadedAv avatar, AvDataContainer rawData, CancellationToken token = default)
        {
            if (!IgnoreOriginalLoaderType && rawData.DataLoaderType == typeof(FileAvDataLoader))
                return;

            if (!IOUtils.s_modelFileExtensionToStringLookup.TryGetValue(rawData.ModelFormat, out string extension))
            {
                Debug.LogError($"{nameof(CacheModelData)}: Could not save avatar due to unrecognized model format/extension: '{rawData.ModelFormat}'.");
                return;
            }

            try
            {
                string directory = UseAvatarIdAsDirName ? Path.Join(BaseDirectory, avatar.Metadata.Id) : BaseDirectory;
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                await File.WriteAllBytesAsync(Path.Join(directory, $"{IOUtils.DefaultModelFile}{extension}"), rawData.Model, token);
                await File.WriteAllTextAsync(Path.Join(directory, IOUtils.DefaultMetadataFile), JsonConvert.SerializeObject(rawData.Metadata), token);

                if (avatar.FullRender != null)
                {
                    await File.WriteAllBytesAsync(
                        Path.Join(directory, $"{IOUtils.DefaultFullRenderFile}{IOUtils.s_imageFileExtensionToStringLookup[FullRenderImageFormat]}"),
                        EncodeToFormat(avatar.FullRender, FullRenderImageFormat, FullRenderJPEGQuality), token);
                }

                if (avatar.BustRender != null)
                {
                    await File.WriteAllBytesAsync(
                        Path.Join(directory, $"{IOUtils.DefaultBustRenderFile}{IOUtils.s_imageFileExtensionToStringLookup[BustRenderImageFormat]}"),
                        EncodeToFormat(avatar.BustRender, BustRenderImageFormat, BustRenderJPEGQuality), token);
                }
            }
            catch (JsonException ex)
            {
                Debug.LogError($"{nameof(CacheModelData)}: Could not save avatar due to JSON exception:\n'{ex}'.");
            }
            catch (SystemException ex)
            {
                Debug.LogError($"{nameof(CacheModelData)}: Could not save avatar due to system exception:\n'{ex}'.");
            }
        }

        private static byte[] EncodeToFormat(Texture2D image, AvImageFileExtension format, int quality)
        {
            return format switch
            {
                AvImageFileExtension.JPEG or AvImageFileExtension.JPG => image.EncodeToJPG(quality),
                AvImageFileExtension.PNG => image.EncodeToPNG(),
                _ => throw new NotImplementedException()
            };
        }
    }
}