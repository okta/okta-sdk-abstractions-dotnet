// <copyright file="CustomEnvironmentVariablesProvider.cs" company="Okta, Inc">
// Copyright (c) 2018 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Okta.Sdk.Abstractions.Configuration.Providers.EnvironmentVariables
{
    /// <summary>
    /// Configuration provider for environment variables.
    /// Partially taken from FlexibleConfiguration library by Nate Barbettini
    /// </summary>
    public class CustomEnvironmentVariablesProvider : ConfigurationProvider
    {
        private readonly string mustStartWith;
        private readonly string separator;
        private readonly string root;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomEnvironmentVariablesProvider"/> class.
        /// </summary>
        /// <param name="mustStartWith">Variable name prefix.</param>
        /// <param name="separator">Separator symbol.</param>
        /// <param name="root">Root branch for a configuration tree.</param>
        public CustomEnvironmentVariablesProvider(string mustStartWith, string separator, string root)
        {
            this.mustStartWith = mustStartWith;
            this.separator = separator;
            this.root = root;
        }

        /// <inheritdoc/>
        public override void Load()
        {
            Load(Environment.GetEnvironmentVariables());
        }

        /// <summary>
        /// Loads configuration properties using the Environment Variables Provider.
        /// </summary>
        /// <param name="variables">The Environment variables to filter.</param>
        public void Load(IDictionary variables)
        {
            var data = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var enumerator = new EnvironmentVariablesEnumerator(mustStartWith, separator);
            foreach (var item in enumerator.GetItems(variables))
            {
                var key = item.Key;

                if (!string.IsNullOrEmpty(root))
                {
                    key = ConfigurationPath.Combine(root, key);
                }

                if (data.ContainsKey(key))
                {
                    throw new FormatException($"The key '{key}' is duplicated.");
                }

                data[key] = item.Value;
            }

            Data = data;
        }
    }
}
