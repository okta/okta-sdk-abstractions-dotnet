// <copyright file="YamlConfigurationProvider.cs" company="Okta, Inc">
// Copyright (c) 2018 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using YamlDotNet.Serialization;

namespace Okta.Sdk.Abstractions.Configuration.Providers.Yaml
{
    /// <summary>
    /// Yaml file configuration provider
    /// </summary>
    public class YamlConfigurationProvider : FileConfigurationProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="YamlConfigurationProvider"/> class.
        /// </summary>
        /// <param name="source">The source settings</param>
        public YamlConfigurationProvider(YamlConfigurationSource source)
            : base(source)
        {
        }

        /// <inheritdoc/>
        public override void Load(Stream stream)
        {
            Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var deserializer = new DeserializerBuilder().Build();
                using (var reader = new StreamReader(stream))
                {
                    var document = (Dictionary<object, object>)deserializer.Deserialize(reader);

                    foreach (var entry in document)
                    {
                        AddConfigurationValue(string.Empty, entry);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Cannot process the yaml file \"{Source.Path}\"", e);
            }
        }

        private void AddConfigurationValue(string parentPrefix, KeyValuePair<object, object> entry)
        {
            string prefix = (string)entry.Key;
            if (!string.IsNullOrEmpty(parentPrefix))
            {
                prefix = $"{parentPrefix}:{prefix}";
            }

            switch (entry.Value)
            {
                case string text:
                    {
                        Data.Add(prefix, text);

                        break;
                    }

                case List<object> list:
                    {
                        int listItemNo = 0;
                        foreach (var item in list)
                        {
                            Data.Add($"{prefix}:{listItemNo++}", (string)item);
                        }

                        break;
                    }

                case Dictionary<object, object> dictionary:
                    {
                        foreach (var pair in dictionary)
                        {
                            AddConfigurationValue(prefix, pair);
                        }

                        break;
                    }
            }
        }
    }
}
