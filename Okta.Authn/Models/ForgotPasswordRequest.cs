﻿using System;
using System.Collections.Generic;
using System.Text;
using Okta.Authn.Abstractions;

namespace Okta.Authn.Models
{
    public class ForgotPasswordRequest : Resource
    {
        public string Username
        {
            get => GetStringProperty("username");
            set => this["username"] = value;
        }

        public string RelayState
        {
            get => GetStringProperty("relayState");
            set => this["relayState"] = value;
        }

        public string FactorType
        {
            get => GetStringProperty("factorType");
            set => this["factorType"] = value;
        }
    }
}
