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

#nullable enable
namespace Uralstech.AvLoader.Utils
{
    // yeah this is a mess
#if UNITY_6000_5_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
#endif
    internal static class IOUtils
    {
        internal const string DefaultModelFile = "model";
        internal const string DefaultMetadataFile = "metadata.json";
        internal const string DefaultBustRenderFile = "bust";
        internal const string DefaultFullRenderFile = "full";

        internal static readonly KeyValuePair<string, AvModelFileExtension>[] s_gltfStringAndModelFileExtensionPairs = {
            new(".glb", AvModelFileExtension.GLB),
            new(".gltf", AvModelFileExtension.GLTF),
        };

        internal static readonly KeyValuePair<string, AvImageFileExtension>[] s_jpegStringAndImageFileExtensionPairs = {
            new(".jpeg", AvImageFileExtension.JPEG),
            new(".jpg", AvImageFileExtension.JPG),
        };

        internal static readonly KeyValuePair<string, AvModelFileExtension>[] s_modelStringAndModelFileExtensionPairs;
        internal static readonly KeyValuePair<string, AvImageFileExtension>[] s_imageStringAndImageFileExtensionPairs;
        internal static readonly Dictionary<string, AvModelFileExtension> s_stringToModelFileExtensionLookup;
        internal static readonly Dictionary<string, AvImageFileExtension> s_stringToImageFileExtensionLookup;
        internal static readonly Dictionary<AvModelFileExtension, string> s_modelFileExtensionToStringLookup;
        internal static readonly Dictionary<AvImageFileExtension, string> s_imageFileExtensionToStringLookup;

        static IOUtils()
        {
            KeyValuePair<string, AvImageFileExtension>[] completeImageExtensionPairs = new KeyValuePair<string, AvImageFileExtension>[s_jpegStringAndImageFileExtensionPairs.Length + 1];
            Array.Copy(s_jpegStringAndImageFileExtensionPairs, completeImageExtensionPairs, s_jpegStringAndImageFileExtensionPairs.Length);
            completeImageExtensionPairs[^1] = new KeyValuePair<string, AvImageFileExtension>(".png", AvImageFileExtension.PNG);
            s_imageStringAndImageFileExtensionPairs = completeImageExtensionPairs;

            KeyValuePair<string, AvModelFileExtension>[] completeModelExtensionPairs = new KeyValuePair<string, AvModelFileExtension>[s_gltfStringAndModelFileExtensionPairs.Length + 1];
            Array.Copy(s_gltfStringAndModelFileExtensionPairs, completeModelExtensionPairs, s_gltfStringAndModelFileExtensionPairs.Length);
            completeModelExtensionPairs[^1] = new KeyValuePair<string, AvModelFileExtension>(".vrm", AvModelFileExtension.VRM);
            s_modelStringAndModelFileExtensionPairs = completeModelExtensionPairs;

            s_stringToModelFileExtensionLookup = new Dictionary<string, AvModelFileExtension>(s_modelStringAndModelFileExtensionPairs, StringComparer.OrdinalIgnoreCase);
            s_stringToImageFileExtensionLookup = new Dictionary<string, AvImageFileExtension>(s_imageStringAndImageFileExtensionPairs, StringComparer.OrdinalIgnoreCase);
            s_modelFileExtensionToStringLookup = CreateReverseDictionary(s_modelStringAndModelFileExtensionPairs);
            s_imageFileExtensionToStringLookup = CreateReverseDictionary(s_imageStringAndImageFileExtensionPairs);
        }

        private static Dictionary<T2, T1> CreateReverseDictionary<T1, T2>(KeyValuePair<T1, T2>[] pairs)
        {
            int count = pairs.Length;
            Dictionary<T2, T1> result = new(count);

            for (int i = 0; i < count; i++)
            {
                KeyValuePair<T1, T2> pair = pairs[i];
                result.Add(pair.Value, pair.Key);
            }

            return result;
        }
    }
}