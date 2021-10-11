// <copyright file="ObjectConfigurationSource.cs" company="Okta, Inc">
// Copyright (c) 2018 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Newtonsoft.Json.Linq;

namespace Okta.Sdk.Abstractions.Configuration.Providers.Object
{
    /// <summary>
    /// JObject configuration source.
    /// </summary>
    public class ObjectConfigurationSource : IConfigurationSource
    {
        /// <summary>
        /// Gets configuration object.
        /// </summary>
        public JObject JsonObject { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectConfigurationSource"/> class.
        /// </summary>
        /// <param name="jsonObject">The configuration object.</param>
        public ObjectConfigurationSource(JObject jsonObject)
        {
            JsonObject = jsonObject;
        }

        /// <summary>
        ///  Builds the <see cref="JsonStreamConfigurationProvider"/> for the source configuration object.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <returns>The <see cref="IConfigurationProvider"/>.</returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.Write(JsonObject.ToString());
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);

            var configurationSource = new JsonStreamConfigurationSource
            {
                Stream = stream,
            };

            return new JsonStreamConfigurationProvider(configurationSource);
        }
    }
}
