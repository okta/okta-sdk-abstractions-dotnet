// <copyright file="EnvironmentConfigurationExtensions.cs" company="Okta, Inc">
// Copyright (c) 2018 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using Microsoft.Extensions.Configuration;

namespace Okta.Sdk.Abstractions.Configuration.Providers.EnvironmentVariables
{
    /// <summary>
    /// Extension methods for adding <see cref="EnvironmentConfigurationExtensions"/>.
    /// </summary>
    public static class EnvironmentConfigurationExtensions
    {
        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from environment variables
        /// with a specified prefix.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="mustStartWith">The prefix that environment variable names must start with.</param>
        /// <param name="separator">The separator character or string between key and value names.</param>
        /// <param name="root">The configuration tree root.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddEnvironmentVariables(
            this IConfigurationBuilder builder,
            string mustStartWith,
            string separator,
            string root)
        {
            return builder.Add(new CustomEnvironmentVariablesSource(mustStartWith, separator, root));
        }
    }
}
