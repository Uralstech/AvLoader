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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Uralstech.AvLoader.Utils;

#nullable enable
namespace Uralstech.AvLoader.DataLoaders
{
    /// <summary>
    /// Loads avatar data from remote URIs via <see cref="UnityWebRequest.Get(Uri)"/>.
    /// Downloads are performed in parallel.
    /// </summary>
    public class URIAvDataLoader : IAvDataLoader
    {
        /// <summary>
        /// Called each time a new <see cref="UnityWebRequest"/> is created, e.g. for auth configuration.
        /// </summary>
        public Func<UnityWebRequest, Awaitable>? WebRequestConfigurationCallback;

        private readonly Uri _modelURI, _metadataURI;
        private readonly Uri? _fullRenderURI, _bustRenderURI;
        private readonly AvModelFileExtension _modelFormat;
        private readonly Encoding _metadataEncoding;

        /// <inheritdoc cref="URIAvDataLoader(Uri, Uri, AvModelFileExtension, Uri?, Uri?, Encoding?)"/>
        public URIAvDataLoader(string modelURI, string metadataURI, AvModelFileExtension modelFormat, string? fullRenderURI = null, string? bustRenderURI = null, Encoding? metadataEncoding = null)
            : this(new Uri(modelURI), new Uri(metadataURI), modelFormat,
                !string.IsNullOrEmpty(fullRenderURI) ? new Uri(fullRenderURI) : null,
                !string.IsNullOrEmpty(bustRenderURI) ? new Uri(bustRenderURI) : null,
                metadataEncoding)
        { }

        /// <summary>
        /// Creates a loader configured to download avatar data from explicit URIs.
        /// </summary>
        /// <param name="modelURI">Direct URI to the avatar model file.</param>
        /// <param name="metadataURI">Direct URI to the metadata JSON file.</param>
        /// <param name="modelFormat">The file format/extension of the model. Must be explicitly specified (no auto-detection like with <see cref="FileAvDataLoader"/>).</param>
        /// <param name="fullRenderURI">Optional URI to the full-body render image.</param>
        /// <param name="bustRenderURI">Optional URI to the bust/half-body render image.</param>
        /// <param name="metadataEncoding">Text encoding for the metadata JSON. Defaults to UTF-8.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="modelFormat"/> is <see cref="AvModelFileExtension.None"/>, as URI-based loading requires an explicit format.</exception>
        public URIAvDataLoader(Uri modelURI, Uri metadataURI, AvModelFileExtension modelFormat, Uri? fullRenderURI = null, Uri? bustRenderURI = null, Encoding? metadataEncoding = null)
        {
            _modelURI = modelURI;
            _metadataURI = metadataURI;
            _fullRenderURI = fullRenderURI;
            _bustRenderURI = bustRenderURI;
            _modelFormat = modelFormat;
            _metadataEncoding = metadataEncoding ?? Encoding.UTF8;

            if (modelFormat is AvModelFileExtension.None)
                throw new ArgumentException($"Cannot use model format auto-detection for URI-based loader (format received: {modelFormat})", nameof(modelFormat));
        }

        /// <inheritdoc/>
        public async Awaitable<AvSourceData?> LoadAvatarAsync(bool throwOnFail, CancellationToken token = default)
        {
            UnityWebRequest? modelDownload = await CreateAndConfigureGetRequest(_modelURI, UnityWebRequest.Get, throwOnFail),
                             metadataDownload = await CreateAndConfigureGetRequest(_metadataURI, UnityWebRequest.Get, throwOnFail);

            if (modelDownload is null || metadataDownload is null)
            {
                modelDownload?.Dispose();
                metadataDownload?.Dispose();
                return null;
            }

            List<UnityWebRequest> requests = new(2) { modelDownload, metadataDownload };
            UnityWebRequest? fullRenderDownload = null, bustRenderDownload = null;

            if (_fullRenderURI is not null)
            {
                fullRenderDownload = await CreateAndConfigureGetRequest(_fullRenderURI, UnityWebRequestTexture.GetTexture, throwOnFail);
                if (fullRenderDownload is null)
                {
                    requests.DisposeAll();
                    return null;
                }

                requests.Add(fullRenderDownload);
            }

            if (_bustRenderURI is not null)
            {
                bustRenderDownload = await CreateAndConfigureGetRequest(_bustRenderURI, UnityWebRequestTexture.GetTexture, throwOnFail);
                if (bustRenderDownload is null)
                {
                    requests.DisposeAll();
                    return null;
                }

                requests.Add(bustRenderDownload);
            }

            try
            {
                using (CancellationTokenRegistration _ = token.Register(requests.AbortAll))
                    await Task.WhenAll(requests.SendAll());

                token.ThrowIfCancellationRequested();

                bool isSuccess = true;
                List<Exception>? failExceptions = throwOnFail ? new() : null;
                foreach (UnityWebRequest failed in requests.GetErred())
                {
                    isSuccess = false;
                    if (throwOnFail)
                    {
                        failExceptions!.Add(new UnityWebRequestException(failed));
                        continue;
                    }

                    Debug.LogWarning($"{nameof(URIAvDataLoader)}: Could not load data at '{failed.url}' due to error: ({failed.responseCode}) {failed.error}");
                }

                if (!isSuccess)
                    return throwOnFail ? throw new AggregateException("One or more GET requests failed.", failExceptions) : null;

                if (!AvMetadata.TryCreateFromBytes(metadataDownload.downloadHandler.nativeData, out AvMetadata? metadata, _metadataEncoding, throwOnFail))
                    return null;

                Texture2D? fullRender = await fullRenderDownload.TryGetTextureAsync(throwOnFail, nameof(AvSourceData.FullRender), token);
                Texture2D? bustRender = await bustRenderDownload.TryGetTextureAsync(throwOnFail, nameof(AvSourceData.BustRender), token);

#pragma warning disable IDE0046 // Convert to conditional expression
                if ((fullRenderDownload is not null && fullRender == null)
                    || (bustRenderDownload is not null && bustRender == null))
                    return null;
#pragma warning restore IDE0046 // Convert to conditional expression

                return new AvSourceData(
                    modelDownload.downloadHandler.data, _modelFormat, _modelURI.AbsoluteUri,
                    metadata, typeof(URIAvDataLoader), fullRender, bustRender
                );
            }
            finally
            {
                requests.DisposeAll();
            }
        }

        private async Awaitable<UnityWebRequest?> CreateAndConfigureGetRequest(Uri uri, Func<Uri, UnityWebRequest> createMethod, bool throwOnFail)
        {
            UnityWebRequest request = createMethod(uri);
            if (WebRequestConfigurationCallback is null)
                return request;
            
            try
            {
                await WebRequestConfigurationCallback(request);
                return request;
            }
            catch (Exception ex)
            {
                if (throwOnFail) throw new AggregateException("Could not configure request due to exception from user code.", ex);
                Debug.LogWarning($"{nameof(URIAvDataLoader)}: Could not configure request due to exception from user code:\n{ex}");
                return null;
            }
        }
    }
}