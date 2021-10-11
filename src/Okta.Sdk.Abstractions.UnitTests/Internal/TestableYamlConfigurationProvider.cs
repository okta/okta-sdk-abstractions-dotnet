// <copyright file="TestableYamlConfigurationProvider.cs" company="Okta, Inc">
// Copyright (c) 2018 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using Okta.Sdk.Abstractions.Configuration.Providers.Yaml;
using System.Collections.Generic;

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
