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
using System.Threading;
using UnityEngine;

#nullable enable
namespace Uralstech.AvLoader.PostProcessors
{
    /// <summary>
    /// QoL post-processor that runs an <see cref="Action"/>.
    /// </summary>
    public class ActionPostProcessor : IAvPostProcessor
    {
        /// <summary>The action to execute.</summary>
        public Action<LoadedAv, AvDataContainer> Action;

        public ActionPostProcessor(Action<LoadedAv, AvDataContainer> action)
        {
            Action = action;
        }

        /// <inheritdoc/>
        public void PostProcess(LoadedAv avatar, AvDataContainer rawData) => Action(avatar, rawData);
    }

    /// <summary>
    /// QoL post-processor that runs an async <see cref="Func"/>.
    /// </summary>
    public class AsyncFuncPostProcessor : IAsyncAvPostProcessor
    {
        /// <summary>The <see cref="Func{T1, T2, T3, TResult}"/> to execute.</summary>
        public Func<LoadedAv, AvDataContainer, CancellationToken, Awaitable> Func;

        public AsyncFuncPostProcessor(Func<LoadedAv, AvDataContainer, CancellationToken, Awaitable> func)
        {
            Func = func;
        }

        /// <inheritdoc/>
        public Awaitable PostProcessAsync(LoadedAv avatar, AvDataContainer rawData, CancellationToken token = default) => Func(avatar, rawData, token);
    }
}
