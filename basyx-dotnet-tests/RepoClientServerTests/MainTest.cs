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
using BaSyx.API.Clients;
using BaSyx.API.Http;
using BaSyx.API.Interfaces;
using BaSyx.Models.AdminShell;
using BaSyx.Models.Extensions;
using BaSyx.Utils.ResultHandling;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleAssetAdministrationShell;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BaSyx.Utils.Extensions;
using BaSyx.Clients.AdminShell.Http;
using BaSyx.Models.Connectivity;
using BaSyx.Utils.ResultHandling.ResultTypes;
using Newtonsoft.Json.Linq;

namespace RepoClientServerTests
{
    [TestClass]
    public class MainTest : ISubmodelRepositoryClient
    {
        private static Submodel Submodel;
        private static Submodel TestingSubmodel;
        private static AssetAdministrationShell AdminShell;

        private static AssetAdministrationShellRepositoryHttpClient aasRepoClient;
        private static SubmodelRepositoryHttpClient submodelRepoClient;

        public IEndpoint Endpoint => ((IClient)submodelRepoClient).Endpoint;

        static MainTest()
        {
            Server.Run();

            AdminShell = TestAssetAdministrationShell.GetAssetAdministrationShell("MainAdminShell");
            Submodel = TestSubmodel.GetSubmodel("MainSubmodel");
            AdminShell.Submodels.Add(Submodel);

            aasRepoClient = new AssetAdministrationShellRepositoryHttpClient(new Uri(Server.ServerUrl));
            submodelRepoClient = new SubmodelRepositoryHttpClient(new Uri(Server.ServerUrl));
        }


        #region AAS Repository Client

        [TestMethod]
        public void Test001_PostShells()
        {
            var result = PostShells(AdminShell);

            result.Success.Should().BeTrue();
            result.Entity.SubmodelReferences.Count.Should().Be(1);
            result.Entity.Submodels.Count.Should().Be(0);
            result.Entity.Should().BeEquivalentTo(AdminShell, options =>
            {
                options
                    .Excluding(p => p.Submodels);
                return options;
            });
        }

        [TestMethod]
        public void Test002_GetShells()
        {
            var result = GetShells();

            result.Success.Should().BeTrue();
            result.Entity.Result.Count.Should().Be(1);
            var aas = result.Entity.Result[0];
            aas.SubmodelReferences.Count.Should().Be(1);
            aas.Submodels.Count.Should().Be(0);
            aas.Should().BeEquivalentTo(AdminShell, options =>
            {
                options
                    .Excluding(p => p.Submodels);
                return options;
            });
        }

        [TestMethod]
        public void Test003_GetShellsReference()
        {
            var result = GetShellsReference();

            result.Success.Should().BeTrue();
            var referenceList = result.Entity.Result as IList<IReference<IAssetAdministrationShell>>;
            referenceList.Count.Should().Be(1);

            var expectedRef = AdminShell.CreateReference();
            expectedRef.Should().BeEquivalentTo(referenceList[0]);
        }

        [TestMethod]
        public void Test004_GetShellsById()
        {
            var result = GetShellsById(AdminShell.Id);

            result.Success.Should().BeTrue();
            var aas = result.Entity;
            aas.SubmodelReferences.Count.Should().Be(1);
            aas.Submodels.Count.Should().Be(0);
            aas.Should().BeEquivalentTo(AdminShell, options =>
            {
                options
                    .Excluding(p => p.Submodels);
                return options;
            });
        }

        [TestMethod]
        public void Test005_PutShellsById()
        {
            var id = "PutShell";
            var putShell = new AssetAdministrationShell(id, new BaSyxShellIdentifier(id, "1.0.0"))
            {
                Description = new LangStringSet()
                {
                    new LangString("de-DE", "Put VWS"),
                    new LangString("en-US", "Put AAS")
                },
                AssetInformation = new AssetInformation()
                {
                    AssetKind = AssetKind.Instance,
                    GlobalAssetId = new BaSyxAssetIdentifier("PutShell", "2.0.0"),
                }
            };

            var result = PutShellsById(AdminShell.Id, putShell);

            result.Success.Should().BeTrue();
            
            var falseResult = GetShellsById(putShell.Id);
            falseResult.Success.Should().BeFalse();

            var trueResult = GetShellsById(AdminShell.Id);
            trueResult.Success.Should().BeTrue();
            var aas = trueResult.Entity;
            aas.SubmodelReferences.Count.Should().Be(1);
            aas.Submodels.Count.Should().Be(0);

            aas.Id.Should().BeEquivalentTo(AdminShell.Id);
            aas.IdShort.Should().Be(putShell.IdShort);
            aas.Description.Should().BeEquivalentTo(putShell.Description);
            aas.Administration.Should().BeEquivalentTo(AdminShell.Administration);
        }

        #endregion

        #region AAS Repository Client

        public IResult<IAssetAdministrationShell> PostShells(IAssetAdministrationShell aas)
        {
            return ((IAssetAdministrationShellRepositoryClient)aasRepoClient).CreateAssetAdministrationShellAsync(aas).GetAwaiter().GetResult();
        }

        public IResult<PagedResult<IElementContainer<IAssetAdministrationShell>>> GetShells()
        {
            return ((IAssetAdministrationShellRepositoryClient)aasRepoClient).RetrieveAssetAdministrationShellsAsync().GetAwaiter().GetResult();
        }

        public IResult<PagedResult<IEnumerable<IReference<IAssetAdministrationShell>>>> GetShellsReference()
        {
            return ((IAssetAdministrationShellRepositoryClient)aasRepoClient).RetrieveAssetAdministrationShellsReferenceAsync().GetAwaiter().GetResult();
        }

        public IResult<IAssetAdministrationShell> GetShellsById(string id)
        {
            return ((IAssetAdministrationShellRepositoryClient)aasRepoClient).RetrieveAssetAdministrationShellAsync(id).GetAwaiter().GetResult();
        }

        public IResult PutShellsById(string id, IAssetAdministrationShell aas)
        {
            return ((IAssetAdministrationShellRepositoryClient)aasRepoClient).UpdateAssetAdministrationShellAsync(id, aas).GetAwaiter().GetResult();
        }

        #endregion

        #region Submodel Repository Client

        public Task<IResult<ISubmodel>> CreateSubmodelAsync(ISubmodel submodel)
        {
            return ((ISubmodelRepositoryClient)submodelRepoClient).CreateSubmodelAsync(submodel);
        }

        public Task<IResult<ISubmodel>> RetrieveSubmodelAsync(Identifier id)
        {
            return ((ISubmodelRepositoryClient)submodelRepoClient).RetrieveSubmodelAsync(id);
        }

        public Task<IResult<PagedResult<IElementContainer<ISubmodel>>>> RetrieveSubmodelsAsync(int limit = 100, string cursor = "", string semanticId = "", string idShort = "")
        {
            return ((ISubmodelRepositoryClient)submodelRepoClient).RetrieveSubmodelsAsync(limit, cursor, semanticId, idShort);
        }

        public Task<IResult> UpdateSubmodelAsync(Identifier id, ISubmodel submodel)
        {
            return ((ISubmodelRepositoryClient)submodelRepoClient).UpdateSubmodelAsync(id, submodel);
        }

        public Task<IResult> DeleteSubmodelAsync(Identifier id)
        {
            return ((ISubmodelRepositoryClient)submodelRepoClient).DeleteSubmodelAsync(id);
        }

        public IResult<ISubmodel> CreateSubmodel(ISubmodel submodel)
        {
            return ((ISubmodelRepositoryInterface)submodelRepoClient).CreateSubmodel(submodel);
        }

        public IResult<ISubmodel> RetrieveSubmodel(Identifier id)
        {
            return ((ISubmodelRepositoryInterface)submodelRepoClient).RetrieveSubmodel(id);
        }

        public IResult<PagedResult<IElementContainer<ISubmodel>>> RetrieveSubmodels(int limit = 100, string cursor = "", string semanticId = "", string idShort = "")
        {
            return ((ISubmodelRepositoryInterface)submodelRepoClient).RetrieveSubmodels(limit, cursor, semanticId, idShort);
        }

        public IResult UpdateSubmodel(Identifier id, ISubmodel submodel)
        {
            return ((ISubmodelRepositoryInterface)submodelRepoClient).UpdateSubmodel(id, submodel);
        }

        public IResult DeleteSubmodel(Identifier id)
        {
            return ((ISubmodelRepositoryInterface)submodelRepoClient).DeleteSubmodel(id);
        }

        #endregion
    }
}
