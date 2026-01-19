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
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.Networking;
using Uralstech.AvLoader.Utils;

#nullable enable
namespace Uralstech.AvLoader
{
    /// <summary>
    /// Information regarding a loaded model.
    /// </summary>
    [JsonObject]
    public struct AvMetadata
    {
        /// <summary>
        /// Unique ID of the avatar.
        /// </summary>
        [JsonProperty("id")]
        public string Id;

        /// <summary>
        /// The type of the model.
        /// </summary>
        [JsonProperty("bodyType"), JsonConverter(typeof(StringEnumConverter))]
        public AvType Type;

        /// <summary>
        /// The gender of the model or outfit it is wearing.
        /// </summary>
        [JsonProperty("outfitGender"), JsonConverter(typeof(StringEnumConverter))]
        public AvGender Gender;

        /// <summary>
        /// The creation timestamp of the model.
        /// </summary>
        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt;

        /// <summary>
        /// The last time this model was updated.
        /// </summary>
        [JsonProperty("updatedAt")]
        public DateTimeOffset UpdatedAt;

        /// <summary>
        /// The skin tone of the model stored as a hex string (including the #).
        /// </summary>
        [JsonProperty("skinTone")]
        public string? SkinTone;

        /// <summary>
        /// Tries creating a <see cref="AvMetadata"/> struct from raw bytes.
        /// </summary>
        /// <param name="data">The data to read from.</param>
        /// <param name="metadata">The decoded struct, <see langword="null"/> if decoding fails.</param>
        /// <param name="encoding">The encoding of the data. Assumes <see cref="Encoding.UTF8"/> if <see langword="null"/>.</param>
        /// <param name="throwOnFail">Should the method throw if it couldn't create the <see cref="AvMetadata"/>?</param>
        /// <returns><see langword="true"/> if successful; <see langword="false"/> otherwise.</returns>
        public static bool TryCreateFromBytes(ReadOnlySpan<byte> data, [NotNullWhen(true)] out AvMetadata? metadata, Encoding? encoding = null, bool throwOnFail = false)
        {
            metadata = null;
            encoding ??= Encoding.UTF8;
            
            try
            {
                string text = encoding.GetString(data);
                metadata = JsonConvert.DeserializeObject<AvMetadata>(text);

                return metadata.HasValue;
            }
            catch (Exception ex)
            {
                if (throwOnFail) throw;
                Debug.LogWarning($"{nameof(AvMetadata)}: Could not create metadata from bytes:\n{ex}");
                return false;
            }
        }

        /// <summary>
        /// Tries creating a <see cref="AvMetadata"/> struct from a file.
        /// </summary>
        /// <param name="path">The path to read from.</param>
        /// <param name="metadata">The decoded struct, <see langword="null"/> if decoding fails.</param>
        /// <param name="throwOnFail">Should the method throw if it couldn't create the <see cref="AvMetadata"/>?</param>
        /// <returns><see langword="true"/> if successful; <see langword="false"/> otherwise.</returns>
        public static bool TryCreateFromFile(string path, [NotNullWhen(true)] out AvMetadata? metadata, bool throwOnFail = false)
        {
            metadata = null;
            
            try
            {
                string text = File.ReadAllText(path);
                metadata = JsonConvert.DeserializeObject<AvMetadata>(text);

                return metadata.HasValue;
            }
            catch (Exception ex)
            {
                if (throwOnFail) throw;
                Debug.LogWarning($"{nameof(AvMetadata)}: Could not create metadata from file:\n{ex}");
                return false;
            }
        }

        /// <summary>
        /// Tries creating a <see cref="AvMetadata"/> struct from data at the given URI.
        /// </summary>
        /// <param name="uri">The URI to load from.</param>
        /// <param name="encoding">The encoding of the data. Assumes <see cref="Encoding.UTF8"/> if <see langword="null"/>.</param>
        /// <param name="throwOnFail">Should the method throw if it couldn't create the <see cref="AvMetadata"/>?</param>
        /// <returns>The decoded <see cref="AvMetadata"/> struct if successfull; <see langword="null"/> otherwise.</returns>
        public static async Awaitable<AvMetadata?> TryCreateFromUriAsync(Uri uri, CancellationToken? token = null, Encoding? encoding = null, bool throwOnFail = true)
        {
            encoding ??= Encoding.UTF8;
            using UnityWebRequest request = UnityWebRequest.Get(uri);
            
            using (CancellationTokenRegistration? _ = token?.Register(request.Abort))
                await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (throwOnFail) throw new UnityWebRequestException(request);
                Debug.LogWarning($"{nameof(AvMetadata)}: Could not load metadata from URI due to a web request error: ({request.responseCode}) {request.error}");
                return null;
            }

            try
            {
                string text = encoding.GetString(request.downloadHandler.nativeData);
                return JsonConvert.DeserializeObject<AvMetadata>(text);
            }
            catch (Exception ex)
            {
                if (throwOnFail) throw;
                Debug.LogWarning($"{nameof(AvMetadata)}: Could not create metadata from downloaded data:\n{ex}");
                return null;
            }
        }
    }
}