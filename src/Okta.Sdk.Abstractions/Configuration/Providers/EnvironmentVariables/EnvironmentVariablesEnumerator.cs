// <copyright file="EnvironmentVariablesEnumerator.cs" company="Okta, Inc">
// Copyright (c) 2018 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;

namespace Okta.Sdk.Abstractions.Configuration.Providers.EnvironmentVariables
{
    /// <summary>
    /// Environment enumeration functions
    /// Taken from FlexibleConfiguration library by Nate Barbettini
    /// </summary>
    public class EnvironmentVariablesEnumerator
    {
        private const string KeyDelimiter = ":";
        private readonly string prefix;
        private readonly string separator;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentVariablesEnumerator"/> class.
        /// </summary>
        /// <param name="prefix">The prefix to filter environment variables.</param>
        /// <param name="separator">Separator symbol.</param>
        public EnvironmentVariablesEnumerator(string prefix, string separator)
        {
            this.prefix = prefix;
            this.separator = separator;
        }

        /// <summary>
        /// Enumerator function.
        /// </summary>
        /// <param name="environmentVariables">The dictionary containing system environment variables.</param>
        /// <returns>Filtered variables.</returns>
        public IEnumerable<KeyValuePair<string, string>> GetItems(IDictionary environmentVariables)
        {
            foreach (DictionaryEntry item in environmentVariables)
            {
                var key = item.Key
                    .ToString()
                    .ToLower();

                if (!string.IsNullOrEmpty(prefix))
                {
                    if (!key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Trim prefix
                    key = key.Substring(prefix.Length);

                    // Trim stray beginning separator, if necessary
                    if (key.StartsWith(separator, StringComparison.OrdinalIgnoreCase))
                    {
                        key = key.Substring(separator.Length);
                    }
                }

                key = key.Replace(separator.ToLower(), KeyDelimiter);

                yield return new KeyValuePair<string, string>(key, item.Value.ToString());
            }
        }
    }
}
