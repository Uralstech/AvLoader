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
    public class CacheModelData : IAvAsyncPostProcessor
    {
        public readonly string Directory;
        public readonly AvImageFileExtension FullRenderImageFormat;
        public readonly AvImageFileExtension BustRenderImageFormat;

        public CacheModelData(string baseDirectory, string avatarDirectoryName,
            AvImageFileExtension fullRenderImageFormat = AvImageFileExtension.JPG, AvImageFileExtension bustRenderImageFormat = AvImageFileExtension.JPG)
        {
            Directory = Path.Join(baseDirectory, avatarDirectoryName);
            FullRenderImageFormat = fullRenderImageFormat;
            BustRenderImageFormat = bustRenderImageFormat;

            if (fullRenderImageFormat is AvImageFileExtension.JPEGAny or AvImageFileExtension.None)
                throw new ArgumentException($"Unsupported file extension for caching avatar renders: {fullRenderImageFormat}", nameof(fullRenderImageFormat));

            if (bustRenderImageFormat is AvImageFileExtension.JPEGAny or AvImageFileExtension.None)
                throw new ArgumentException($"Unsupported file extension for caching avatar renders: {bustRenderImageFormat}", nameof(bustRenderImageFormat));
        }

        public async Awaitable PostProcessAsync(ILoadedAv avatar, CancellationToken token = default)
        {
            if (avatar.RawData.DataLoaderType == typeof(FileAvDataLoader))
                return;

            if (!IOUtils.s_modelFileExtensionToStringLookup.TryGetValue(avatar.RawData.ModelFormat, out string extension))
            {
                Debug.LogError($"{nameof(CacheModelData)}: Could not save avatar due to unrecognized model format/extension: '{avatar.RawData.ModelFormat}'.");
                return;
            }

            try
            {
                if (!System.IO.Directory.Exists(Directory))
                    System.IO.Directory.CreateDirectory(Directory);

                await File.WriteAllBytesAsync(Path.Join(Directory, $"{IOUtils.DefaultModelFile}{extension}"), avatar.RawData.Model, token);
                await File.WriteAllTextAsync(Path.Join(Directory, IOUtils.DefaultMetadataFile), JsonConvert.SerializeObject(avatar.RawData.Metadata), token);

                if (avatar.FullRender != null)
                {
                    await File.WriteAllBytesAsync(
                        Path.Join(Directory, $"{IOUtils.DefaultFullRenderFile}{IOUtils.s_imageFileExtensionToStringLookup[FullRenderImageFormat]}"),
                        EncodeToFormat(avatar.FullRender, FullRenderImageFormat), token);
                }

                if (avatar.BustRender != null)
                {
                    await File.WriteAllBytesAsync(
                        Path.Join(Directory, $"{IOUtils.DefaultBustRenderFile}{IOUtils.s_imageFileExtensionToStringLookup[BustRenderImageFormat]}"),
                        EncodeToFormat(avatar.BustRender, BustRenderImageFormat), token);
                }

                Debug.Log("SAVED");
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

        private static byte[] EncodeToFormat(Texture2D image, AvImageFileExtension format)
        {
            return format switch
            {
                AvImageFileExtension.JPEG or AvImageFileExtension.JPG => image.EncodeToJPG(),
                AvImageFileExtension.PNG => image.EncodeToPNG(),
                _ => throw new NotImplementedException()
            };
        }
    }
}