using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Okta.Sdk.Abstractions.UnitTests
{
    public class UrlHelperShould
    {
        [Theory]
        [InlineData("https://devex-testing.oktapreview.com/oauth2/default", "https://devex-testing.oktapreview.com")]
        [InlineData("https://devex-testing.okta.com/oauth2/default", "https://devex-testing.okta.com")]
        [InlineData("http://devex-testing.okta.com/oauth2/default", "http://devex-testing.okta.com")]
        [InlineData("https://devex-testing.trex-govcloud.com/oauth2/default", "https://devex-testing.trex-govcloud.com")]
        [InlineData("https://devex-testing.okta-gov.com/oauth2/default", "https://devex-testing.okta-gov.com")]
        [InlineData("https://devex-testing.okta.mil/oauth2/default", "https://devex-testing.okta.mil")]
        [InlineData("https://devex-testing.okta-miltest.com/oauth2/default", "https://devex-testing.okta-miltest.com")]
        public void GetOktaDomain(string issuer, string expectedOktaDomain)
        {
            UrlHelper.GetOktaRootUrl(issuer).Should().Be(expectedOktaDomain);
        }
    }
}
