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
using System.Collections.Generic;
using System.Runtime.Serialization;
using BaSyx.Models.Extensions;
using System.Linq;
using System.Text.Json.Serialization;

namespace BaSyx.Models.AdminShell
{
    [DataContract]
    public class AssetAdministrationShell : Identifiable, IAssetAdministrationShell
    {
        public IAssetInformation AssetInformation { get; set; }

        [JsonIgnore]
        public IElementContainer<ISubmodel> Submodels { get; set; }

        private List<IReference<ISubmodel>> _submodelRefs = [];

        [JsonPropertyName("submodels")]
        public IList<IReference<ISubmodel>> SubmodelReferences 
        { 
            get => _submodelRefs;
            set => _submodelRefs = value.ToList();
        }
        public IReference<IAssetAdministrationShell> DerivedFrom { get; set; }     
        public ModelType ModelType => ModelType.AssetAdministrationShell;
        public IEnumerable<IEmbeddedDataSpecification> EmbeddedDataSpecifications { get; set; }
        public IConceptDescription ConceptDescription { get; set; }
     
        public AssetAdministrationShell(string idShort, Identifier id) : base(idShort, id)
        {
            Submodels = new ElementContainer<ISubmodel>(this);
            MetaData = new Dictionary<string, string>();
            EmbeddedDataSpecifications = new List<IEmbeddedDataSpecification>();
        }
    }
}
