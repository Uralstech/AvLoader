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

using System.IO;
using UnityEngine.Networking;

#nullable enable
namespace Uralstech.AvLoader.Utils
{
    /// <summary>
    /// Exception thrown when a <see cref="UnityWebRequest"/> fails, providing detailed request information.
    /// </summary>
    public sealed class UnityWebRequestException : IOException
    {
        /// <summary>The URL that failed to load.</summary>
        public readonly string Url;

        /// <summary>The HTTP response code (if any).</summary>
        public readonly long ResponseCode;
        
        /// <summary>The error message reported by the UnityWebRequest.</summary>
        public readonly string Error;

        /// <summary>The error message reported by the DownloadHandler.</summary>
        public readonly string DownloadHandlerError;

        public UnityWebRequestException(UnityWebRequest request)
            : base($"{request.method} failed for '{request.url}' ({request.responseCode}): {request.error}, DownloadHandler: {request.downloadHandler.error}")
        {
            Url = request.url;
            ResponseCode = request.responseCode;
            Error = request.error;
            DownloadHandlerError = request.downloadHandler.error;
        }
    }
}