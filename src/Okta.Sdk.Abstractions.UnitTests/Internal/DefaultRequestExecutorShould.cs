using FluentAssertions;
using Okta.Sdk.Abstractions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Xunit;

namespace Okta.Sdk.Abstractions.UnitTests.Internal
{
    public class DefaultRequestExecutorShould
    {
        [Fact]
        public void SetHttpClientDefaultRequestAuthorizationHeader()
        {
            var testToken = "abcd1234-fake-token";
            var httpClient = new HttpClient();
            var oktaConfig = new OktaClientConfiguration { Token = testToken, OktaDomain = "https://test.okta.com" };
            _ = new DefaultRequestExecutor(oktaConfig, httpClient, null);

            httpClient.DefaultRequestHeaders.Should().NotBeNull();
            httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
            httpClient.DefaultRequestHeaders.Authorization.Scheme.Should().Be("SSWS");
            httpClient.DefaultRequestHeaders.Authorization.Parameter.Should().Be(testToken);
        }
    }
}
