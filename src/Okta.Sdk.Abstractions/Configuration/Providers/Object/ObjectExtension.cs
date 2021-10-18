// <copyright file="ObjectExtension.cs" company="Okta, Inc">
// Copyright (c) 2018 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Okta.Sdk.Abstractions.Configuration.Providers.Object
{
    /// <summary>
    /// Extension methods for adding <see cref="ObjectConfigurationSource"/>.
    /// </summary>
    public static class ObjectExtension
    {
        /// <summary>
        /// Adds the configuration in <paramref name="object"/> to the <see cref="IConfigurationBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="object">The object to examine.</param>
        /// <param name="root">A root element to prepend to any discovered key.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddObject(
            this IConfigurationBuilder builder,
            object @object,
            string root = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(IConfigurationBuilder));
            }

            var configContainer = @object ?? new { };
            JObject jsonObject;

            if (string.IsNullOrWhiteSpace(root))
            {
                jsonObject = JObject.FromObject(
                    configContainer,
                    new JsonSerializer
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                    });
            }
            else
            {
                jsonObject = new JObject
                {
                    {
                        root,
                        JToken.FromObject(
                            configContainer,
                            new JsonSerializer
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                            })
                    },
                };
            }

            return builder.Add(new ObjectConfigurationSource(jsonObject));
        }
    }
}
