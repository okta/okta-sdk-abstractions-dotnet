// <copyright file="TestableCustomEnvironmentVariablesProvider.cs" company="Okta, Inc">
// Copyright (c) 2018 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using Okta.Sdk.Abstractions.Configuration.Providers.EnvironmentVariables;
using System.Collections.Generic;

namespace Okta.Sdk.Abstractions.UnitTests.Internal
{
    public class TestableCustomEnvironmentVariablesProvider : CustomEnvironmentVariablesProvider
    {
        /// <summary>
        /// Gets the collection of loaded properties.
        /// </summary>
        public IDictionary<string, string> LoadedData => Data;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestableCustomEnvironmentVariablesProvider"/> class.
        /// </summary>
        /// <param name="mustStartWith">Variable name prefix.</param>
        /// <param name="separator">Separator symbol.</param>
        /// <param name="root">Root branch for a configuration tree.</param>
        public TestableCustomEnvironmentVariablesProvider(string mustStartWith, string separator, string root)
            : base(mustStartWith, separator, root)
        { 
        }
    }
}
