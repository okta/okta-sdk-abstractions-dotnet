// <copyright file="CustomEnvironmentVariablesSource.cs" company="Okta, Inc">
// Copyright (c) 2018 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using Microsoft.Extensions.Configuration;

namespace Okta.Sdk.Abstractions.Configuration.Providers.EnvironmentVariables
{
    /// <summary>
    /// Configuration source for Environment Variables
    /// </summary>
    public class CustomEnvironmentVariablesSource : IConfigurationSource
    {
        /// <summary>
        /// Gets or sets the prefix to filter environment variables.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or sets the separator symbol.
        /// </summary>
        public string Separator { get; set; }

        /// <summary>
        /// Gets or sets the root branch of configuration tree.
        /// </summary>
        public string Root { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomEnvironmentVariablesSource"/> class.
        /// </summary>
        /// <param name="mustStartWith">Variable name prefix</param>
        /// <param name="separator">The separator</param>
        /// <param name="root">The root</param>
        public CustomEnvironmentVariablesSource(string mustStartWith, string separator, string root)
            : base()
        {
            Prefix = mustStartWith;
            Separator = separator;
            Root = root;
        }

        /// <summary>
        /// Builds the <see cref="CustomEnvironmentVariablesProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The configuration builder</param>
        /// <returns><see cref="CustomEnvironmentVariablesProvider"/></returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder) =>
            new CustomEnvironmentVariablesProvider(Prefix, Separator, Root);
    }
}
