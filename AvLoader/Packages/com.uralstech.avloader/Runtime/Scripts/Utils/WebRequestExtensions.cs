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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

#nullable enable
namespace Uralstech.AvLoader.Utils
{
    /// <summary>
    /// Utility extensions for web requests.
    /// </summary>
    /// <remarks>
    /// This class is <see langword="public"/> to allow package users to reuse these extensions if useful.
    /// However, it should not be considered stable and is not part of the supported public API.
    /// It exists solely as an internal utility and may change or be removed at any time.
    /// /// </remarks>
    public static class WebRequestExtensions
    {
        /// <summary>
        /// Wraps an <see cref="AsyncOperation"/> as a <see cref="Task"/>.
        /// </summary>
        public static async Task AsTask(this AsyncOperation current) => await current;

        /// <summary>
        /// Sends all <see cref="UnityWebRequest"/>s in the current list, and creates a <see cref="Task"/>[] for their async operations.
        /// </summary>
        public static Task[] SendAll(this List<UnityWebRequest> current)
        {
            int count = current.Count;
            Task[] tasks = new Task[count];

            for (int i = 0; i < count; i++)
                tasks[i] = current[i].SendWebRequest().AsTask();
            return tasks;
        }

        /// <summary>
        /// Enumerates the erred/failed <see cref="UnityWebRequest"/>s in the current list.
        /// </summary>
        public static IEnumerable<UnityWebRequest> GetErred(this List<UnityWebRequest> current)
        {
            foreach (UnityWebRequest request in current)
            {
                if (request.result is not UnityWebRequest.Result.InProgress and not UnityWebRequest.Result.Success)
                    yield return request;
            }
        }

        /// <summary>
        /// Aborts all <see cref="UnityWebRequest"/>s in the current list.
        /// </summary>
        public static void AbortAll(this List<UnityWebRequest> current)
        {
            int count = current.Count;
            for (int i = 0; i < count; i++)
                current[i].Abort();
        }

        /// <summary>
        /// Disposes all <see cref="UnityWebRequest"/>s in the current list.
        /// </summary>
        public static void DisposeAll(this List<UnityWebRequest> current)
        {
            int count = current.Count;
            for (int i = 0; i < count; i++)
                current[i].Dispose();
        }

        /// <summary>
        /// Awaits for the <see cref="DownloadHandlerTexture"/> of the current <see cref="UnityWebRequest"/> to finish processing its texture.
        /// </summary>
        public static async Awaitable AwaitTextureProcessing(this UnityWebRequest? request, CancellationToken token)
        {
            if (request?.downloadHandler is not DownloadHandlerTexture downloadHandler || downloadHandler.isDone)
                return;

            while (!downloadHandler.isDone)
                await Awaitable.NextFrameAsync(token);
        }
    }
}