using Okta.Sdk.Abstractions.Configuration.Providers.Yaml;
using System;
using System.Collections.Generic;
using System.Text;

namespace Okta.Sdk.Abstractions.UnitTests.Internal
{
    public class TestableYamlConfigurationProvider : YamlConfigurationProvider
    {
        /// <summary>
        /// Gets the collection of loaded properties.
        /// </summary>
        public IDictionary<string, string> LoadedData => Data;

        public TestableYamlConfigurationProvider(YamlConfigurationSource source) : base(source)
        {
        }
    }
}
