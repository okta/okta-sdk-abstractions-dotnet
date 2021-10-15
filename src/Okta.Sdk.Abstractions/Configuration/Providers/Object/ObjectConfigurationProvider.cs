// <copyright file="ObjectConfigurationProvider.cs" company="Okta, Inc">
// Copyright (c) 2018 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using Microsoft.Extensions.Configuration.Json;

namespace Okta.Sdk.Abstractions.Configuration.Providers.Object
{
    /// <summary>
    /// Object configuration provider
    /// </summary>
    public class ObjectConfigurationProvider: JsonStreamConfigurationProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectConfigurationProvider"/> class.
        /// </summary>
        /// <param name="configurationSource">The Object configuration Source</param>
        public ObjectConfigurationProvider(ObjectConfigurationSource configurationSource)
            : base(configurationSource)
        {
        }
    }
}
