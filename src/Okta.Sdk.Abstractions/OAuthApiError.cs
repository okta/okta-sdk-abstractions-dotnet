﻿// <copyright file="OAuthApiError.cs" company="Okta, Inc">
// Copyright (c) 2018 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text;

namespace Okta.Sdk.Abstractions
{
    /// <inheritdoc/>
    public class OAuthApiError : BaseResource, IOAuthApiError
    {
        /// <inheritdoc/>
        public string Error => GetStringProperty("error");

        /// <inheritdoc/>
        public string ErrorDescription => GetStringProperty("error_description");
    }

    
}
