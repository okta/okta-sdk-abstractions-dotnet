using System.IO;
using System.Text;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Okta.Sdk.Abstractions.Configuration;
using Okta.Sdk.Abstractions.UnitTests.Internal;
using Okta.Sdk.Abstractions.Configuration.Providers.Yaml;
using Okta.Sdk.Abstractions.Configuration.Providers.Object;

namespace Okta.Sdk.Abstractions.UnitTests
{
    public class CustomConfigurationProvidersShould
    {
        [Fact]
        public void ParseYamlConfigurationFile()
        {
            var yamlFile = @"okta:
  client:
    connectionTimeout: 99 # seconds
    orgUrl: https://OrgUrl.okta.com
    oktaDomain: https://Domain.okta.com
    token: tokentokentokentokentokentokentoken
    requestTimeout: 0 # seconds
    rateLimit:
      maxRetries: 4
";

            byte[] bytes = Encoding.UTF8.GetBytes(yamlFile);
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                var yamlProvider = new TestableYamlConfigurationProvider(new YamlConfigurationSource());
                yamlProvider.Load(stream);

                yamlProvider.LoadedData.Should().HaveCount(6);
                yamlProvider.LoadedData.Should().Contain(new KeyValuePair<string, string>("okta:client:connectionTimeout", "99"));
                yamlProvider.LoadedData.Should().Contain(new KeyValuePair<string, string>("okta:client:orgUrl", "https://OrgUrl.okta.com"));
                yamlProvider.LoadedData.Should().Contain(new KeyValuePair<string, string>("okta:client:oktaDomain", "https://Domain.okta.com"));
                yamlProvider.LoadedData.Should().Contain(new KeyValuePair<string, string>("okta:client:token", "tokentokentokentokentokentokentoken"));
                yamlProvider.LoadedData.Should().Contain(new KeyValuePair<string, string>("okta:client:requestTimeout", "0"));
                yamlProvider.LoadedData.Should().Contain(new KeyValuePair<string, string>("okta:client:rateLimit:maxRetries", "4"));
            }
        }

        [Fact]
        public void ParseEnvironmentConfiguration()
        {
            var envProvider = new TestableCustomEnvironmentVariablesProvider(mustStartWith: "oktakey", separator: "_", "okta");

            envProvider.Load(new Dictionary<string, string> {
                { "oktakey_key1", "value1" },
                { "oktakey_key2", "value2" },
                { "key_key3", "value3" },
                { "oktakey_key4_subkey1", "value5" },
            });

            envProvider.LoadedData.Should().HaveCount(3);
            envProvider.LoadedData.Should().Contain(new KeyValuePair<string, string>("okta:key1", "value1"));
            envProvider.LoadedData.Should().Contain(new KeyValuePair<string, string>("okta:key2", "value2"));
            envProvider.LoadedData.Should().Contain(new KeyValuePair<string, string>("okta:key4:subkey1", "value5"));
            envProvider.LoadedData.Should().NotContain(new KeyValuePair<string, string>("okta:key3", "value3"));
        }

        [Fact]
        public void ParseConfigurationObject()
        {
            var configurationObject = new
            {
                connectionTimeout = 99,
                disablehttpscheck = true,
                token = "tokentokentokentokentokentokentoken",
                oktaDomain = "https://oktadomain.okta.com",
                proxy = new
                {
                    port = 9999,
                    host = "https://proxyHost",
                    username = "user",
                    password = "pass",
                },
            };

            var configuration = new ConfigurationBuilder()
              .AddObject(configurationObject, root: "okta:client")
              .AddObject(configurationObject, root: "okta:testing")
              .AddObject(configurationObject)
              .Build();

            OktaClientConfiguration compiledConfig = new OktaClientConfiguration();
            configuration.GetSection("okta").GetSection("client").Bind(compiledConfig);

            compiledConfig.ConnectionTimeout.Should().Be(99);
            compiledConfig.DisableHttpsCheck.Should().Be(true);
            compiledConfig.Token.Should().Be("tokentokentokentokentokentokentoken");
            compiledConfig.OktaDomain.Should().Be("https://oktadomain.okta.com");
            compiledConfig.Proxy.Port.Should().Be(9999);
            compiledConfig.Proxy.Host.Should().Be("https://proxyHost");
            compiledConfig.Proxy.Username.Should().Be("user");
            compiledConfig.Proxy.Password.Should().Be("pass");
        }
    }
}
