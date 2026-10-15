using FluentAssertions;
using Okta.Sdk.Abstractions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Okta.Sdk.Abstractions.UnitTests.Internal
{
    public class DefaultRequestExecutorShould
    {
        private const string OktaDomain = "https://test.okta.com";

        [Fact]
        public void SetHttpClientDefaultRequestAuthorizationHeader()
        {
            var testToken = "abcd1234-fake-token";
            var httpClient = new HttpClient();
            var oktaConfig = new OktaClientConfiguration { Token = testToken, OktaDomain = OktaDomain };
            _ = new DefaultRequestExecutor(oktaConfig, httpClient, null);

            httpClient.DefaultRequestHeaders.Should().NotBeNull();
            httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
            httpClient.DefaultRequestHeaders.Authorization.Scheme.Should().Be("SSWS");
            httpClient.DefaultRequestHeaders.Authorization.Parameter.Should().Be(testToken);
        }

        [Fact]
        public async Task SetScopedAuthorizationOnRequestWithoutMutatingHttpClientDefaults()
        {
            var handler = new CapturingHandler();
            var httpClient = new HttpClient(handler);
            var executor = new DefaultRequestExecutor(new OktaClientConfiguration { Token = "admin-token", OktaDomain = OktaDomain }, httpClient, NullLogger.Instance);

            await executor.GetAsync("/api/v1/users/me", ScopedHeaders("user-token-a"), CancellationToken.None);

            httpClient.DefaultRequestHeaders.Authorization.Scheme.Should().Be("SSWS");
            httpClient.DefaultRequestHeaders.Authorization.Parameter.Should().Be("admin-token");

            var sent = handler.Requests.Single();
            sent.Authorization.Should().Be("SSWS user-token-a");
        }

        [Fact]
        public async Task NotLeakScopedAuthorizationIntoSubsequentUnscopedRequests()
        {
            var handler = new CapturingHandler();
            var httpClient = new HttpClient(handler);
            var executor = new DefaultRequestExecutor(new OktaClientConfiguration { Token = "admin-token", OktaDomain = OktaDomain }, httpClient, NullLogger.Instance);

            await executor.GetAsync("/api/v1/users/me", ScopedHeaders("user-token-a"), CancellationToken.None);
            await executor.GetAsync("/api/v1/users", new Dictionary<string, string>(), CancellationToken.None);
            await executor.PostAsync("/api/v1/users", null, "{}", CancellationToken.None);

            handler.Requests.Select(r => r.Authorization).Should().Equal(
                "SSWS user-token-a",
                "SSWS admin-token",
                "SSWS admin-token");
        }

        [Fact]
        public async Task NotForwardRawAuthorizationValueHeader()
        {
            var handler = new CapturingHandler();
            var httpClient = new HttpClient(handler);
            var executor = new DefaultRequestExecutor(new OktaClientConfiguration { Token = "admin-token", OktaDomain = OktaDomain }, httpClient, NullLogger.Instance);

            await executor.PostAsync("/api/v1/users", ScopedHeaders("user-token-a"), "{}", CancellationToken.None);

            var sent = handler.Requests.Single();
            sent.HeaderNames.Should().NotContain(h => string.Equals(h, "Authorization-Value", StringComparison.OrdinalIgnoreCase));
            sent.Authorization.Should().Be("SSWS user-token-a");
        }

        [Fact]
        public async Task KeepConcurrentScopedAuthorizationIsolated()
        {
            var handler = new CapturingHandler(delay: TimeSpan.FromMilliseconds(5));
            var httpClient = new HttpClient(handler);
            var executor = new DefaultRequestExecutor(new OktaClientConfiguration { Token = "admin-token", OktaDomain = OktaDomain }, httpClient, NullLogger.Instance);

            using (var gate = new ManualResetEventSlim(false))
            {
                var tasks = Enumerable.Range(0, 200).SelectMany(i => new[]
                {
                    Task.Run(() => { gate.Wait(); return executor.GetAsync("/api/v1/users/me?u=a", ScopedHeaders("user-token-a"), CancellationToken.None); }),
                    Task.Run(() => { gate.Wait(); return executor.GetAsync("/api/v1/users/me?u=b", ScopedHeaders("user-token-b"), CancellationToken.None); }),
                }).ToList();
                gate.Set();
                await Task.WhenAll(tasks);
            }

            handler.Requests.Should().HaveCount(400);
            foreach (var request in handler.Requests)
            {
                var expected = request.Query.EndsWith("u=a") ? "SSWS user-token-a" : "SSWS user-token-b";
                request.Authorization.Should().Be(expected);
            }
        }

        private static IEnumerable<KeyValuePair<string, string>> ScopedHeaders(string token)
            => new Dictionary<string, string>
            {
                ["Authorization-Scheme"] = "SSWS",
                ["Authorization-Value"] = token,
            };

        private sealed class CapturedRequest
        {
            public string Query { get; set; }

            public string Authorization { get; set; }

            public IList<string> HeaderNames { get; set; }
        }

        // Records the request exactly as HttpClient sends it (i.e. with DefaultRequestHeaders merged in).
        private sealed class CapturingHandler : HttpMessageHandler
        {
            private readonly TimeSpan _delay;

            public CapturingHandler(TimeSpan delay = default)
            {
                _delay = delay;
            }

            public ConcurrentQueue<CapturedRequest> Requests { get; } = new ConcurrentQueue<CapturedRequest>();

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (_delay > TimeSpan.Zero)
                {
                    await Task.Delay(_delay, cancellationToken);
                }

                Requests.Enqueue(new CapturedRequest
                {
                    Query = request.RequestUri.Query,
                    Authorization = request.Headers.Authorization?.ToString(),
                    HeaderNames = request.Headers.Select(h => h.Key).ToList(),
                });

                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}", Encoding.UTF8, "application/json") };
            }
        }

        [Theory]
        [InlineData("https://test.okta.com//attacker.example.com/api/v1/users")]
        [InlineData("https://test.okta.com///attacker.example.com/api/v1/users")]
        [InlineData("\\/attacker.example.com/api/v1/users")]
        [InlineData("/\\/attacker.example.com/api/v1/users")]
        [InlineData(" //attacker.example.com/api/v1/users")]
        [InlineData("\t//attacker.example.com/api/v1/users")]
        [InlineData("https://attacker.example.com/api/v1/users")]
        [InlineData("http:\\\\attacker.example.com/api/v1/users")]
        public async Task RejectRequestUrisThatResolveToAnotherOrigin(string href)
        {
            var handler = new RecordingHandler();
            var executor = CreateExecutor(handler);

            Func<Task> get = () => executor.GetAsync(href, null, CancellationToken.None);
            Func<Task> post = () => executor.PostAsync(href, null, "{}", CancellationToken.None);
            Func<Task> put = () => executor.PutAsync(href, null, "{}", CancellationToken.None);
            Func<Task> delete = () => executor.DeleteAsync(href, null, CancellationToken.None);

            await get.Should().ThrowAsync<InvalidOperationException>();
            await post.Should().ThrowAsync<InvalidOperationException>();
            await put.Should().ThrowAsync<InvalidOperationException>();
            await delete.Should().ThrowAsync<InvalidOperationException>();
            handler.Requests.Should().BeEmpty();
        }

        [Theory]
        [InlineData("/api/v1/users", "https://test.okta.com/api/v1/users")]
        [InlineData("api/v1/users", "https://test.okta.com/api/v1/users")]
        [InlineData("//api/v1/users", "https://test.okta.com/api/v1/users")]
        [InlineData("https://test.okta.com/api/v1/users?after=abc&limit=2", "https://test.okta.com/api/v1/users?after=abc&limit=2")]
        [InlineData("https://TEST.okta.com/api/v1/users", "https://test.okta.com/api/v1/users")]
        [InlineData("https://test.okta.com.attacker.example.com/api/v1/users", "https://test.okta.com/.attacker.example.com/api/v1/users")]
        public async Task SendSameOriginRequestUrisToTheOktaDomain(string href, string expectedUri)
        {
            var handler = new RecordingHandler();
            var executor = CreateExecutor(handler);

            await executor.GetAsync(href, null, CancellationToken.None);

            handler.Requests.Should().ContainSingle();
            handler.Requests[0].RequestUri.Should().Be(new Uri(expectedUri));
        }

        private static DefaultRequestExecutor CreateExecutor(HttpMessageHandler handler)
        {
            var oktaConfig = new OktaClientConfiguration { Token = "abcd1234-fake-token", OktaDomain = OktaDomain };
            return new DefaultRequestExecutor(oktaConfig, new HttpClient(handler), NullLogger.Instance);
        }

        private sealed class RecordingHandler : HttpMessageHandler
        {
            public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Requests.Add(request);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });
            }
        }
    }
}
