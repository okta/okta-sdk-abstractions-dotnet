// <copyright file="DefaultRequestExecutor.cs" company="Okta, Inc">
// Copyright (c) 2018 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Okta.Sdk.Abstractions.Configuration;

namespace Okta.Sdk.Abstractions
{
    /// <summary>
    /// The default implementation of <see cref="IRequestExecutor"/> that uses <c>System.Net.Http</c>.
    /// </summary>
    public sealed class DefaultRequestExecutor : IRequestExecutor
    {
        private const string AuthorizationSchemeHeader = "Authorization-Scheme";
        private const string AuthorizationValueHeader = "Authorization-Value";

        private readonly string _oktaDomain;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRequestExecutor"/> class.
        /// </summary>
        /// <param name="configuration">The client configuration.</param>
        /// <param name="httpClient">The HTTP client to use, if any.</param>
        /// <param name="logger">The logging interface.</param>
        public DefaultRequestExecutor(OktaClientConfiguration configuration, HttpClient httpClient, ILogger logger)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _oktaDomain = configuration.OktaDomain;
            _logger = logger;
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            ApplyDefaultClientSettings(_httpClient, _oktaDomain, configuration);
        }

        private static void ApplyDefaultClientSettings(HttpClient client, string oktaDomain, OktaClientConfiguration configuration)
        {
            client.BaseAddress = new Uri(oktaDomain, UriKind.Absolute);
            if (!string.IsNullOrEmpty(configuration.Token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("SSWS", configuration.Token);
            }
        }

        private string EnsureRelativeUrl(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentException("The request URI was empty.");
            }

            string path;

            if (uri.StartsWith(_oktaDomain, StringComparison.OrdinalIgnoreCase))
            {
                path = uri.Substring(_oktaDomain.Length);
            }
            else if (uri.Contains("://"))
            {
                throw new InvalidOperationException($"The client is configured to connect to {_oktaDomain}, but this request URI does not match: {uri}");
            }
            else
            {
                path = uri.TrimStart('/');
            }

            // Network-path references (e.g. "//host/path", "\\/host/path") are relative URIs that inherit only
            // the scheme of the base address, so they would send the request (and the SSWS token) to another host.
            // Resolve the path exactly as HttpClient will and make sure it stays on the configured Okta origin.
            if (!Uri.TryCreate(path, UriKind.Relative, out var relativeUri)
                || !Uri.TryCreate(_httpClient.BaseAddress, relativeUri, out var resolvedUri)
                || !IsSameOrigin(_httpClient.BaseAddress, resolvedUri))
            {
                throw new InvalidOperationException($"The client is configured to connect to {_oktaDomain}, but this request URI does not match: {uri}");
            }

            return path;
        }

        private static bool IsSameOrigin(Uri baseAddress, Uri resolvedUri)
            => Uri.Compare(
                baseAddress,
                resolvedUri,
                UriComponents.SchemeAndServer,
                UriFormat.Unescaped,
                StringComparison.OrdinalIgnoreCase) == 0;

        private string GetContentType(IEnumerable<KeyValuePair<string, string>> headers)
        {
            var contentType = HttpRequestContentBuilder.ContentTypeJson;

            if (headers != null)
            {
                var header = headers.FirstOrDefault(x => string.Equals(x.Key, "Content-Type", StringComparison.OrdinalIgnoreCase));

                if (!header.Equals(default(KeyValuePair<string, string>)))
                {
                    contentType = string.IsNullOrEmpty(header.Value) ? HttpRequestContentBuilder.ContentTypeJson : header.Value;
                }
            }

            return contentType;
        }

        private async Task<HttpResponse<string>> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            _logger.LogTrace($"{request.Method} {request.RequestUri}");

            using (var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
            {
                _logger.LogTrace($"{(int)response.StatusCode} {request.RequestUri.PathAndQuery}");

                string stringContent = null;
                if (response.Content != null)
                {
                    stringContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }

                return new HttpResponse<string>
                {
                    Headers = ExtractHeaders(response),
                    StatusCode = (int)response.StatusCode,
                    Payload = stringContent,
                };
            }
        }

        private void ApplyHeadersToRequest(HttpRequestMessage request, IEnumerable<KeyValuePair<string, string>> headers)
        {
            if (headers == null || !headers.Any())
            {
                return;
            }

            foreach (var header in headers)
            {
                // Content-Type is set during the content creation. This is a limitation of .NET, that doesn't allow to set Content-Type as a request header.
                if (string.Equals(header.Key, "Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // The raw credential is never forwarded as a header of its own; it's only used to build the Authorization header below.
                if (string.Equals(header.Key, AuthorizationValueHeader, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Authorization is set on this request only. Never mutate HttpClient.DefaultRequestHeaders here,
                // since the HttpClient is shared by the parent client and every scoped client.
                if (string.Equals(header.Key, AuthorizationSchemeHeader, StringComparison.OrdinalIgnoreCase))
                {
                    var authorizationHeaderValue = headers.FirstOrDefault(x => string.Equals(x.Key, AuthorizationValueHeader, StringComparison.OrdinalIgnoreCase)).Value;
                    request.Headers.Authorization = new AuthenticationHeaderValue(header.Value, authorizationHeaderValue);
                    continue;
                }

                request.Headers.Add(header.Key, header.Value);
            }
        }

        private static IEnumerable<KeyValuePair<string, IEnumerable<string>>> ExtractHeaders(HttpResponseMessage response)
            => response.Headers.Concat(response.Content.Headers);

        /// <inheritdoc/>
        public Task<HttpResponse<string>> GetAsync(string href, IEnumerable<KeyValuePair<string, string>> headers, CancellationToken cancellationToken)
        {
            var path = EnsureRelativeUrl(href);

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(path, UriKind.Relative));
            ApplyHeadersToRequest(request, headers);

            return SendAsync(request, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<HttpResponse<string>> PostAsync(string href, IEnumerable<KeyValuePair<string, string>> headers, string body, CancellationToken cancellationToken)
        {
            var path = EnsureRelativeUrl(href);
            var contentType = GetContentType(headers);

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(path, UriKind.Relative));
            ApplyHeadersToRequest(request, headers);

            request.Content = string.IsNullOrEmpty(body)
                ? null
                : HttpRequestContentBuilder.GetRequestContent(contentType, body);

            return SendAsync(request, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<HttpResponse<string>> PutAsync(string href, IEnumerable<KeyValuePair<string, string>> headers, string body, CancellationToken cancellationToken)
        {
            var path = EnsureRelativeUrl(href);
            var contentType = GetContentType(headers);

            var request = new HttpRequestMessage(HttpMethod.Put, new Uri(path, UriKind.Relative));
            ApplyHeadersToRequest(request, headers);

            request.Content = string.IsNullOrEmpty(body)
                ? null
                : HttpRequestContentBuilder.GetRequestContent(contentType, body);

            return SendAsync(request, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<HttpResponse<string>> DeleteAsync(string href, IEnumerable<KeyValuePair<string, string>> headers, CancellationToken cancellationToken)
        {
            var path = EnsureRelativeUrl(href);

            var request = new HttpRequestMessage(HttpMethod.Delete, new Uri(path, UriKind.Relative));
            ApplyHeadersToRequest(request, headers);

            return SendAsync(request, cancellationToken);
        }
    }
}
