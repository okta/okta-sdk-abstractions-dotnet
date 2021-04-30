// <copyright file="IonApiError.cs" company="Okta, Inc">
// Copyright (c) 2018 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Okta.Sdk.Abstractions
{
    /// <inheritdoc/>
    public sealed class IonApiError : BaseResource, IIonApiError
    {
        /// <inheritdoc/>
        public string Version => GetStringProperty("version");

        /// <inheritdoc/>
        public string ErrorSummary => GetErrorSummary();

        private string GetErrorSummary()
        {
            var errorMessage = GetResponseErrorSummary();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return errorMessage;
            }

            return GetFormValidationErrorSummary();
        }

        // Extracts error message from the top level of response object
        private string GetResponseErrorSummary()
        {
            var sbErrorSumary = new StringBuilder();
            var messageObj = this.GetProperty<BaseResource>("messages");

            if (messageObj != null)
            {
                var messages = messageObj.GetArrayProperty<BaseResource>("value");

                if (messages != null)
                {
                    foreach (var message in messages)
                    {
                        sbErrorSumary.AppendLine(message.GetProperty<string>("message"));
                    }
                }
            }

            return sbErrorSumary.ToString();
        }

        /*
         Extracts form validation error messages like in this case:

         {
          "version": "1.0.0",
          "stateHandle": "028e-r0fW99Ud4dADthU5wogZhdxzLhvOUPnC6FXVx",
          "expiresAt": "2021-04-27T22:05:50.000Z",
          "intent": "LOGIN",
          "remediation": {
            "type": "array",
            "value": [
              {
                "rel": [
                  "create-form"
                ],
                "name": "reset-authenticator",
                "relatesTo": [
                  "$.currentAuthenticator"
                ],
                "href": "..................",
                "method": "POST",
                "produces": "application/ion+json; okta-version=1.0.0",
                "value": [
                  {
                    "name": "credentials",
                    "type": "object",
                    "form": {
                      "value": [
                        {
                          "name": "passcode",
                          "label": "New password",
                          "secret": true,
                          "messages": {
                            "type": "array",
                            "value": [
                              {
                                "message": "Password requirements were not met. Password requirements: at least 8 characters, a lowercase letter, an uppercase letter, a number, no parts of your username. Your password cannot be any of your last 4 passwords.",
                                "i18n": {
                                  "key": "password.passwordRequirementsNotMet",
                                  "params": [
                                    "Password requirements: at least 8 characters, a lowercase letter, an uppercase letter, a number, no parts of your username. Your password cannot be any of your last 4 passwords."
                                  ]
            },
                                "class": "ERROR"
                              }
                            ]
                          }
                        }
                      ]
                    },
                    "required": true
                  },
                  {
        ........
                }
                ],
                "accepts": "application/json; okta-version=1.0.0"
              }
            ]
          },
 .........
        }
        */
        private string GetFormValidationErrorSummary()
        {
            var jToken = JToken.Parse(GetRaw());
            var errorMessages = jToken
                                    .SelectTokens("$.remediation.value[*].value[*].form.value[*].messages.value[*].message")
                                    .Select(t => t.ToString());
            return string.Join(Environment.NewLine, errorMessages);
        }
    }
}
