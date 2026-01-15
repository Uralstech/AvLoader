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
namespace Uralstech.AvLoader
{
    /// <summary>
    /// Post-processing step that runs based on the given condition.
    /// </summary>
    public class OnCondition : IAvPostProcessor, IAsyncAvPostProcessor
    {
        /// <summary>Condition callback.</summary>
        public Func<LoadedAv, AvDataContainer, bool> Condition;

        private object _postProcessor;

        /// <summary>
        /// Post-processor to run if <see cref="Condition"/> is met, can be of any type
        /// inheriting from <see cref="IAvPostProcessor"/> or <see cref="IAsyncAvPostProcessor"/>.
        /// </summary>
        public object PostProcessor
        {
            get => _postProcessor;
            set
            {
                if (value is not IAvPostProcessor and not IAsyncAvPostProcessor)
                    throw new ArgumentException($"Expected object inheriting from {nameof(IAvPostProcessor)} or {nameof(IAsyncAvPostProcessor)}, but got object of type: {value.GetType()}.", nameof(value));
                
                _postProcessor = value;
            }
        }

        public OnCondition(Func<LoadedAv, AvDataContainer, bool> condition, IAvPostProcessor postProcessor)
        {
            Condition = condition;
            _postProcessor = postProcessor;
        }

        public OnCondition(Func<LoadedAv, AvDataContainer, bool> condition, IAsyncAvPostProcessor asyncPostProcessor)
        {
            Condition = condition;
            _postProcessor = asyncPostProcessor;
        }

        /// <inheritdoc/>
        public void PostProcess(LoadedAv avatar, AvDataContainer rawData)
        {
            if (PostProcessor is IAvPostProcessor postProcessor && Condition(avatar, rawData))
                postProcessor.PostProcess(avatar, rawData);
        }

        /// <inheritdoc/>
        public async Awaitable PostProcessAsync(LoadedAv avatar, AvDataContainer rawData, CancellationToken token = default)
        {
            if (PostProcessor is IAsyncAvPostProcessor asyncPostProcessor && Condition(avatar, rawData))
                await asyncPostProcessor.PostProcessAsync(avatar, rawData, token);
        }
    }
}
