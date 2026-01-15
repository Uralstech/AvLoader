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
namespace Uralstech.AvLoader
{
    public static class WebRequestExtensions
    {
        public static async Task AsTask(this AsyncOperation current) => await current;

        public static Task[] SendAll(this List<UnityWebRequest> current)
        {
            int count = current.Count;
            Task[] tasks = new Task[count];

            for (int i = 0; i < count; i++)
                tasks[i] = current[i].SendWebRequest().AsTask();
            return tasks;
        }

        public static IEnumerable<UnityWebRequest> GetErred(this List<UnityWebRequest> current)
        {
            foreach (UnityWebRequest request in current)
            {
                if (request.result is not UnityWebRequest.Result.InProgress and not UnityWebRequest.Result.Success)
                    yield return request;
            }
        }

        public static void AbortAll(this List<UnityWebRequest> current)
        {
            int count = current.Count;
            for (int i = 0; i < count; i++)
                current[i].Abort();
        }

        public static void DisposeAll(this List<UnityWebRequest> current)
        {
            int count = current.Count;
            for (int i = 0; i < count; i++)
                current[i].Dispose();
        }

        public static async Awaitable AwaitTextureProcessing(this UnityWebRequest? request, CancellationToken token)
        {
            if (request?.downloadHandler is not DownloadHandlerTexture downloadHandler || downloadHandler.isDone)
                return;

            while (!downloadHandler.isDone)
                await Awaitable.NextFrameAsync(token);
        }
    }
}