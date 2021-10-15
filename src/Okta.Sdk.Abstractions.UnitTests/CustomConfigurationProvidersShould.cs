// <copyright file="CustomConfigurationProvidersShould.cs" company="Okta, Inc">
// Copyright (c) 2018 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System.IO;
using System.Text;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using Okta.Sdk.Abstractions.UnitTests.Internal;
using Okta.Sdk.Abstractions.Configuration;
using Okta.Sdk.Abstractions.Configuration.Providers.Yaml;
using Microsoft.Extensions.Configuration;
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
fruits:
  - Mango
  - Orange
  - Apple
  - Pomegranate
  - Watermelon
  - Banana
  - Papaya
  - Guava
";

            byte[] bytes = Encoding.UTF8.GetBytes(yamlFile);
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                var yamlProvider = new TestableYamlConfigurationProvider(new YamlConfigurationSource());
                yamlProvider.Load(stream);

                yamlProvider.LoadedData.Should().HaveCount(14);
                yamlProvider.LoadedData.Should().Contain(new KeyValuePair<string, string>("okta:client:connectionTimeout", "99"));
                yamlProvider.LoadedData.Should().Contain(new KeyValuePair<string, string>("okta:client:orgUrl", "https://OrgUrl.okta.com"));
                yamlProvider.LoadedData.Should().Contain(new KeyValuePair<string, string>("okta:client:oktaDomain", "https://Domain.okta.com"));
                yamlProvider.LoadedData.Should().Contain(new KeyValuePair<string, string>("okta:client:token", "tokentokentokentokentokentokentoken"));
                yamlProvider.LoadedData.Should().Contain(new KeyValuePair<string, string>("okta:client:requestTimeout", "0"));
                yamlProvider.LoadedData.Should().Contain(new KeyValuePair<string, string>("okta:client:rateLimit:maxRetries", "4"));
                yamlProvider.LoadedData.Should().Contain(new KeyValuePair<string, string>("fruits:0", "Mango"));
                yamlProvider.LoadedData.Should().Contain(new KeyValuePair<string, string>("fruits:1", "Orange"));
                yamlProvider.LoadedData.Should().Contain(new KeyValuePair<string, string>("fruits:2", "Apple"));
                yamlProvider.LoadedData.Should().Contain(new KeyValuePair<string, string>("fruits:3", "Pomegranate"));
                yamlProvider.LoadedData.Should().Contain(new KeyValuePair<string, string>("fruits:4", "Watermelon"));
                yamlProvider.LoadedData.Should().Contain(new KeyValuePair<string, string>("fruits:5", "Banana"));
                yamlProvider.LoadedData.Should().Contain(new KeyValuePair<string, string>("fruits:6", "Papaya"));
                yamlProvider.LoadedData.Should().Contain(new KeyValuePair<string, string>("fruits:7", "Guava"));
            }
        }

        [Fact]
        public void ParseEnvironmentConfiguration()
        {
            var envProvider = new TestableCustomEnvironmentVariablesProvider(mustStartWith: "oktakey", separator: "_", "okta");

            envProvider.Load(new Dictionary<string, string> 
            {
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
            // anonymous type object
            var anonymousFilled = new
            {
                connectionTimeout = 45,
                disablehttpscheck = true,
                token = "tokentokentokentokentokentokentoken",
                oktaDomain = "https://oktadomain.okta.com",
                proxy = new
                {
                    port = 8080,
                    host = "https://proxyHost",
                    username = "user",
                    password = "pass",
                },
            };

            // strongly typed object with all properties set to defaults
            var typedEmpty = new OktaClientConfiguration();

            // strongly typed object with partially set properties for "okta:client" branch
            var typedPartiallySetClient = new OktaClientConfiguration
            {
                ConnectionTimeout = 120,
                Proxy = new ProxyConfiguration
                {
                    Port = 8000,
                    Host = "https://proxyHostClient",
                    Username = "user",
                    Password = "pass",
                },
            };

            // strongly typed object with partially set properties for "okta:testing" branch
            var typedPartiallySetTesting = new OktaClientConfiguration
            {
                ConnectionTimeout = 999,
                DisableHttpsCheck = false,
                Proxy = new ProxyConfiguration
                {
                    Port = 9999,
                    Host = "https://proxyHostTesting",
                    Username = "testuser",
                    Password = "testpass",
                },
            };

            var configuration = new ConfigurationBuilder()
              .AddObject(anonymousFilled, root: "okta:client")
              .AddObject(typedEmpty, root: "okta:client")
              .AddObject(typedPartiallySetClient, root: "okta:client")
              .AddObject(null, root: "okta:client")

              .AddObject(anonymousFilled, root: "okta:testing")
              .AddObject(typedEmpty, root: "okta:testing")
              .AddObject(typedPartiallySetTesting, root: "okta:testing")
              .AddObject(null, root: "okta:testing")

              .AddObject(null)
              .AddObject(typedEmpty)
              .AddObject(anonymousFilled)

              .Build();

            // properties for the okta:client branch
            OktaClientConfiguration compiledConfig = new OktaClientConfiguration();
            configuration.GetSection("okta").GetSection("client").Bind(compiledConfig);

            compiledConfig.ConnectionTimeout.Should().Be(120);
            compiledConfig.DisableHttpsCheck.Should().BeFalse();
            compiledConfig.Token.Should().Be("tokentokentokentokentokentokentoken");
            compiledConfig.OktaDomain.Should().Be("https://oktadomain.okta.com");
            compiledConfig.Proxy.Port.Should().Be(8000);
            compiledConfig.Proxy.Host.Should().Be("https://proxyHostClient");
            compiledConfig.Proxy.Username.Should().Be("user");
            compiledConfig.Proxy.Password.Should().Be("pass");

            // properties for the okta:testing branch
            compiledConfig = new OktaClientConfiguration();
            configuration.GetSection("okta").GetSection("testing").Bind(compiledConfig);

            compiledConfig.ConnectionTimeout.Should().Be(999);
            compiledConfig.DisableHttpsCheck.Should().BeFalse();
            compiledConfig.Token.Should().Be("tokentokentokentokentokentokentoken");
            compiledConfig.OktaDomain.Should().Be("https://oktadomain.okta.com");
            compiledConfig.Proxy.Port.Should().Be(9999);
            compiledConfig.Proxy.Host.Should().Be("https://proxyHostTesting");
            compiledConfig.Proxy.Username.Should().Be("testuser");
            compiledConfig.Proxy.Password.Should().Be("testpass");

            // properties for the okta:client (lowest priority), then okta:testing, then root (highest priority)
            compiledConfig = new OktaClientConfiguration { Token = "productiontoken", };
            configuration.Bind(compiledConfig);

            compiledConfig.ConnectionTimeout.Should().Be(45);
            compiledConfig.DisableHttpsCheck.Should().BeTrue();
            compiledConfig.Token.Should().Be("tokentokentokentokentokentokentoken");
            compiledConfig.OktaDomain.Should().Be("https://oktadomain.okta.com");
            compiledConfig.Proxy.Port.Should().Be(8080);
            compiledConfig.Proxy.Host.Should().Be("https://proxyHost");
            compiledConfig.Proxy.Username.Should().Be("user");
            compiledConfig.Proxy.Password.Should().Be("pass");

            configuration.GetSection("okta").GetSection("client").Bind(compiledConfig);
            configuration.GetSection("okta").GetSection("testing").Bind(compiledConfig);
            configuration.Bind(compiledConfig);

            compiledConfig.ConnectionTimeout.Should().Be(45);
            compiledConfig.DisableHttpsCheck.Should().BeTrue();
            compiledConfig.Token.Should().Be("tokentokentokentokentokentokentoken");
            compiledConfig.OktaDomain.Should().Be("https://oktadomain.okta.com");
            compiledConfig.Proxy.Port.Should().Be(8080);
            compiledConfig.Proxy.Host.Should().Be("https://proxyHost");
            compiledConfig.Proxy.Username.Should().Be("user");
            compiledConfig.Proxy.Password.Should().Be("pass");
        }
    }
}
