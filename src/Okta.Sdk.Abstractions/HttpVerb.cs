﻿// <copyright file="HttpVerb.cs" company="Okta, Inc">
// Copyright (c) 2018 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text;

namespace Okta.Sdk.Abstractions
{
    /// <summary>
    /// HttpVerb enum.
    /// </summary>
    public enum HttpVerb
    {
        /// <summary>
        /// Represents the GET method of the HTTP protocol.
        /// </summary>
        Get,

        /// <summary>
        /// Represents the POST method of the HTTP protocol.
        /// </summary>
        Post,

        /// <summary>
        /// Represents the PUT method of the HTTP protocol.
        /// </summary>
        Put,

        /// <summary>
        /// Represents the DELETE method of the HTTP protocol.
        /// </summary>
        Delete,
    }
}
