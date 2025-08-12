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
using BaSyx.API.Interfaces;
using BaSyx.Models.AdminShell;
using BaSyx.Models.Extensions;
using BaSyx.Utils.ResultHandling;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleAssetAdministrationShell;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaSyx.Clients.AdminShell.Http;
using BaSyx.Models.Connectivity;
using BaSyx.Utils.ResultHandling.ResultTypes;

namespace RepoClientServerTests
{
    [TestClass]
    public class MainTest : ISubmodelRepositoryClient
    {
        private static readonly Submodel Submodel;
        private static readonly AssetAdministrationShell AdminShell;

        private static readonly AssetAdministrationShellRepositoryHttpClient AasRepoClient;
        private static readonly SubmodelRepositoryHttpClient SubmodelRepoClient;

        public IEndpoint Endpoint => ((IClient)SubmodelRepoClient).Endpoint;

        static MainTest()
        {
            Server.Run();

            AdminShell = TestAssetAdministrationShell.GetAssetAdministrationShell("MainAdminShell");
            Submodel = TestSubmodel.GetSubmodel("MainSubmodel");
            AdminShell.Submodels.Add(Submodel);

            AasRepoClient = new AssetAdministrationShellRepositoryHttpClient(new Uri(Server.ServerUrl));
            SubmodelRepoClient = new SubmodelRepositoryHttpClient(new Uri(Server.ServerUrl));
        }


        #region AAS Repository Client

        [TestMethod]
        public void Test001_PostShells()
        {
            var result = PostShells(AdminShell);

            result.Success.Should().BeTrue();
            result.Entity.Should().NotBeNull();
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
            result.Entity.Should().NotBeNull();
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
            result.Entity.Should().NotBeNull();
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
            result.Entity.Should().NotBeNull();
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
        [DataRow("", true)]
        [DataRow("PutShell", false)]
        public void Test005_PutShellsById(string idShort, bool putSuccessful)
        {
            if (string.IsNullOrEmpty(idShort))
                idShort = AdminShell.IdShort;

            var putShell = new AssetAdministrationShell(idShort, new BaSyxShellIdentifier(idShort, "1.0.0"))
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

            if (!putSuccessful)
            {
                result.Success.Should().BeFalse();
                var falseResult = GetShellsById(putShell.Id);
                falseResult.Success.Should().BeFalse();
                return;
            }

            result.Success.Should().BeTrue();
            
            var getResult = GetShellsById(putShell.Id);
            getResult.Success.Should().BeTrue();

            var trueResult = GetShellsById(AdminShell.Id);
            trueResult.Success.Should().BeTrue();
            result.Entity.Should().NotBeNull();
            var aas = trueResult.Entity;
            aas.SubmodelReferences.Count.Should().Be(1);
            aas.Submodels.Count.Should().Be(0);

            aas.Id.Should().BeEquivalentTo(AdminShell.Id);
            aas.IdShort.Should().Be(putShell.IdShort);
            aas.Description.Should().BeEquivalentTo(putShell.Description);
            aas.Administration.Should().BeEquivalentTo(AdminShell.Administration);
        }

        [TestMethod]
        public void Test006_DeleteShellsById()
        {
            var result = DeleteShellsById(AdminShell.Id);

            result.Success.Should().BeTrue();
            
            var getResult = GetShellsById(AdminShell.Id);
            getResult.Success.Should().BeFalse();

            if (!getResult.Success)
                Test001_PostShells(); // Recreate the shell for further tests
        }

        [TestMethod]
        public void Test007_GetShellsReferenceById()
        {
            var result = GetShellsReferenceById(AdminShell.Id);

            result.Success.Should().BeTrue();
            result.Entity.Should().NotBeNull();
            result.Entity.First.Value.Should().BeEquivalentTo(AdminShell.Id);

            var falseResult = GetShellsReferenceById("NonExistingId");
            falseResult.Success.Should().BeFalse();
            falseResult.Entity.Should().BeNull();
        }

        [TestMethod]
        public void Test008_GetShellsAssetInformationById()
        {
            var result = GetShellsAssetInformationById(AdminShell.Id);

            result.Success.Should().BeTrue();
            result.Entity.Should().NotBeNull();

            result.Entity.Should().BeEquivalentTo(AdminShell.AssetInformation);

            var falseResult = GetShellsReferenceById("NonExistingId");
            falseResult.Success.Should().BeFalse();
            falseResult.Entity.Should().BeNull();
        }

        #endregion

        #region AAS Repository Client

        public IResult<IAssetAdministrationShell> PostShells(IAssetAdministrationShell aas)
        {
            return ((IAssetAdministrationShellRepositoryClient)AasRepoClient).CreateAssetAdministrationShellAsync(aas).GetAwaiter().GetResult();
        }

        public IResult<PagedResult<IElementContainer<IAssetAdministrationShell>>> GetShells()
        {
            return ((IAssetAdministrationShellRepositoryClient)AasRepoClient).RetrieveAssetAdministrationShellsAsync().GetAwaiter().GetResult();
        }

        public IResult<PagedResult<IEnumerable<IReference<IAssetAdministrationShell>>>> GetShellsReference()
        {
            return ((IAssetAdministrationShellRepositoryClient)AasRepoClient).RetrieveAssetAdministrationShellsReferenceAsync().GetAwaiter().GetResult();
        }

        public IResult<IReference<IAssetAdministrationShell>> GetShellsReferenceById(string id)
        {
            return ((IAssetAdministrationShellRepositoryClient)AasRepoClient).RetrieveAssetAdministrationShellReferenceAsync(id).GetAwaiter().GetResult();
        }

        public IResult<IAssetAdministrationShell> GetShellsById(string id)
        {
            return ((IAssetAdministrationShellRepositoryClient)AasRepoClient).RetrieveAssetAdministrationShellAsync(id).GetAwaiter().GetResult();
        }

        public IResult PutShellsById(string id, IAssetAdministrationShell aas)
        {
            return ((IAssetAdministrationShellRepositoryClient)AasRepoClient).UpdateAssetAdministrationShellAsync(id, aas).GetAwaiter().GetResult();
        }

        public IResult DeleteShellsById(string id)
        {
            return ((IAssetAdministrationShellRepositoryClient)AasRepoClient).DeleteAssetAdministrationShellAsync(id).GetAwaiter().GetResult();
        }

        public IResult DeleteShellsReferenceById(string id)
        {
            return ((IAssetAdministrationShellRepositoryClient)AasRepoClient).DeleteAssetAdministrationShellAsync(id).GetAwaiter().GetResult();
        }

        public IResult GetShellsAssetInformationById(string id)
        {
            return ((IAssetAdministrationShellRepositoryClient)AasRepoClient).RetrieveAssetAdministrationShellAssetInformation(id).GetAwaiter().GetResult();
        }

        #endregion

        #region Submodel Repository Client

        public Task<IResult<ISubmodel>> CreateSubmodelAsync(ISubmodel submodel)
        {
            return ((ISubmodelRepositoryClient)SubmodelRepoClient).CreateSubmodelAsync(submodel);
        }

        public Task<IResult<ISubmodel>> RetrieveSubmodelAsync(Identifier id)
        {
            return ((ISubmodelRepositoryClient)SubmodelRepoClient).RetrieveSubmodelAsync(id);
        }

        public Task<IResult<PagedResult<IElementContainer<ISubmodel>>>> RetrieveSubmodelsAsync(int limit = 100, string cursor = "", string semanticId = "", string idShort = "")
        {
            return ((ISubmodelRepositoryClient)SubmodelRepoClient).RetrieveSubmodelsAsync(limit, cursor, semanticId, idShort);
        }

        public Task<IResult> UpdateSubmodelAsync(Identifier id, ISubmodel submodel)
        {
            return ((ISubmodelRepositoryClient)SubmodelRepoClient).UpdateSubmodelAsync(id, submodel);
        }

        public Task<IResult> ReplaceSubmodelAsync(Identifier id, ISubmodel submodel)
        {
            throw new NotImplementedException();
        }

        public Task<IResult> DeleteSubmodelAsync(Identifier id)
        {
            return ((ISubmodelRepositoryClient)SubmodelRepoClient).DeleteSubmodelAsync(id);
        }

        public IResult<ISubmodel> CreateSubmodel(ISubmodel submodel)
        {
            return ((ISubmodelRepositoryInterface)SubmodelRepoClient).CreateSubmodel(submodel);
        }

        public IResult<ISubmodel> RetrieveSubmodel(Identifier id)
        {
            return ((ISubmodelRepositoryInterface)SubmodelRepoClient).RetrieveSubmodel(id);
        }

        public IResult<PagedResult<IElementContainer<ISubmodel>>> RetrieveSubmodels(int limit = 100, string cursor = "", string semanticId = "", string idShort = "")
        {
            return ((ISubmodelRepositoryInterface)SubmodelRepoClient).RetrieveSubmodels(limit, cursor, semanticId, idShort);
        }

        public IResult UpdateSubmodel(Identifier id, ISubmodel submodel)
        {
            return ((ISubmodelRepositoryInterface)SubmodelRepoClient).UpdateSubmodel(id, submodel);
        }

        public IResult ReplaceSubmodel(Identifier id, ISubmodel submodel)
        {
            throw new NotImplementedException();
        }

        public IResult DeleteSubmodel(Identifier id)
        {
            return ((ISubmodelRepositoryInterface)SubmodelRepoClient).DeleteSubmodel(id);
        }

        #endregion
    }
}