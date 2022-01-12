// <copyright file="YamlConfigurationExtensions.cs" company="Okta, Inc">
// Copyright (c) 2018 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;
using Microsoft.Extensions.Configuration;

namespace Okta.Sdk.Abstractions.Configuration.Providers.Yaml
{
    /// <summary>
    /// Extension methods for adding <see cref="YamlConfigurationSource"/>.
    /// </summary>
    public static class YamlConfigurationExtensions
    {
        /// <summary>
        /// Adds the Yaml configuration provider at path to builder.
        /// </summary>
        /// <param name="builder">The builder to add to.</param>
        /// <param name="path">The file path and name</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddYamlFile(this IConfigurationBuilder builder, string path)
        {
            return builder.AddYamlFile(path, optional: false);
        }

        /// <summary>
        /// Adds the Yaml configuration provider at path to builder.
        /// </summary>
        /// <param name="builder">The builder to add to.</param>
        /// <param name="path">The file path and name</param>
        /// <param name="optional">Whether the file is optional.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddYamlFile(this IConfigurationBuilder builder, string path, bool optional)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var yamlConfigurationSource = new YamlConfigurationSource
            {
                Path = path,
                Optional = optional,
            };
            yamlConfigurationSource.ResolveFileProvider();

            return builder.Add(yamlConfigurationSource);
        }
    }
}
