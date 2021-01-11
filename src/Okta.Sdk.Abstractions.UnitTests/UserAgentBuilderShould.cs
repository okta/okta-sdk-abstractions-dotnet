using FluentAssertions;
using Xunit;


namespace Okta.Sdk.Abstractions.UnitTests
{
    public class UserAgentBuilderShould
    {

        [Theory]
        [InlineData("Mono 5.11.0 ((HEAD/369243a66cf)", "Mono 5.11.0 --HEAD-369243a66cf-")]
        [InlineData("0/1:;2()", "0-1--2--")]
        public void ReturnSanitizedFrameworkDescriptionWhenContainsBrackets(string rawString, string sanitizedString)
        {
            var frameworkInfo = UserAgentBuilder.Sanitize(rawString);
            frameworkInfo.Should().Be(sanitizedString);
        }
    }
}
