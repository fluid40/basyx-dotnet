/*******************************************************************************
* Copyright (c) 2024 Bosch Rexroth AG
* Author: Constantin Ziesche (constantin.ziesche@bosch.com)
*
* This program and the accompanying materials are made available under the
* terms of the MIT License which is available at
* https://github.com/eclipse-basyx/basyx-dotnet/blob/main/LICENSE
*
* SPDX-License-Identifier: MIT
*******************************************************************************/
using System.Xml;
using System.Xml.Serialization;

namespace BaSyx.Utils.Settings
{
    public class AuthenticationConfiguration
    {
        [XmlElement]
        public bool Activated { get; set; }

        [XmlElement]
        public string Username { get; set; }

        [XmlElement]
        public string Password { get; set; }

        [XmlElement]
        public string BaseAddress { get; set; }

        [XmlElement]
        public string GetTokenRelativeUrl { get; set; }

        [XmlElement]
        public string ValidateTokenRelativeUrl { get; set; }

        [XmlElement]
        public string TokenType { get; set; }

        public AuthenticationConfiguration()
        {
        }
    }
}
