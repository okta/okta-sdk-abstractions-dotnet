﻿// <copyright file="OktaClientConfigurationValidator.cs" company="Okta, Inc">
// Copyright (c) 2018 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Text.RegularExpressions;
using Okta.Sdk.Abstractions.Configuration;

namespace Okta.Sdk.Abstractions
{
    /// <summary>
    /// Helper validator class for OktaClient settings
    /// </summary>
    public class OktaClientConfigurationValidator
    {
        /// <summary>
        /// Validates the OktaClient configuration
        /// </summary>
        /// <param name="configuration">The configuration to be validated</param>
        public static void Validate(OktaClientConfiguration configuration)
        {
            if (string.IsNullOrEmpty(configuration.OktaDomain))
            {
                throw new ArgumentNullException(nameof(configuration.OktaDomain), "Your Okta URL is missing. You can copy your domain from the Okta Developer Console. Follow these instructions to find it: https://bit.ly/finding-okta-domain");
            }

            configuration.OktaDomain = UrlHelper.EnsureTrailingSlash(configuration.OktaDomain);

            if (!configuration.DisableHttpsCheck && !configuration.OktaDomain.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Your Okta URL must start with https. Current value: {configuration.OktaDomain}. You can copy your domain from the Okta Developer Console. Follow these instructions to find it: https://bit.ly/finding-okta-domain", nameof(configuration.OktaDomain));
            }

            if (configuration.OktaDomain.IndexOf("{yourOktaDomain}", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                throw new ArgumentNullException(nameof(configuration.OktaDomain), "Replace {yourOktaDomain} with your Okta domain. You can copy your domain from the Okta Developer Console. Follow these instructions to find it: https://bit.ly/finding-okta-domain");
            }

            if (configuration.OktaDomain.IndexOf("-admin.okta.com", StringComparison.OrdinalIgnoreCase) >= 0 ||
                configuration.OktaDomain.IndexOf("-admin.oktapreview.com", StringComparison.OrdinalIgnoreCase) >= 0 ||
                configuration.OktaDomain.IndexOf("-admin.okta-emea.com", StringComparison.OrdinalIgnoreCase) >= 0 ||
                configuration.OktaDomain.IndexOf("-admin.trex-govcloud.com", StringComparison.OrdinalIgnoreCase) >= 0 ||
                configuration.OktaDomain.IndexOf("-admin.okta-gov.com", StringComparison.OrdinalIgnoreCase) >= 0 ||
                configuration.OktaDomain.IndexOf("-admin.okta.mil", StringComparison.OrdinalIgnoreCase) >= 0 ||
                configuration.OktaDomain.IndexOf("-admin.okta-miltest.com", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                throw new ArgumentNullException(nameof(configuration.OktaDomain), $"Your Okta domain should not contain -admin. Current value: {configuration.OktaDomain}. You can copy your domain from the Okta Developer Console. Follow these instructions to find it: https://bit.ly/finding-okta-domain");
            }

            if (configuration.OktaDomain.IndexOf(".com.com", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                throw new ArgumentNullException(nameof(configuration.OktaDomain), $"It looks like there's a typo in your Okta domain. Current value: {configuration.OktaDomain}. You can copy your domain from the Okta Developer Console. Follow these instructions to find it: https://bit.ly/finding-okta-domain");
            }

            if (Regex.Matches(configuration.OktaDomain, "://").Count != 1)
            {
                throw new ArgumentNullException(nameof(configuration.OktaDomain), $"It looks like there's a typo in your Okta domain. Current value: {configuration.OktaDomain}. You can copy your domain from the Okta Developer Console. Follow these instructions to find it: https://bit.ly/finding-okta-domain");
            }
        }
    }
}
