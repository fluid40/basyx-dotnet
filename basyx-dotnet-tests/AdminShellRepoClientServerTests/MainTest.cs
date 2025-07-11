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
using BaSyx.Clients.AdminShell.Http;
using BaSyx.API.Clients;
using BaSyx.API.Http;
using BaSyx.API.Interfaces;
using BaSyx.Models.AdminShell;
using BaSyx.Models.Extensions;
using BaSyx.Models.Connectivity;
using BaSyx.Utils.ResultHandling;
using BaSyx.Utils.Extensions;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleAssetAdministrationShell;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BaSyx.Utils.ResultHandling.ResultTypes;
using System.Text.Json;

namespace AdminShellRepoClientServerTests
{
    [TestClass]
    public class MainTest : IAssetAdministrationShellRepositoryClient
    {
        private static Submodel MainSubmodel;
        private static Submodel TestSubmodel;
        private static AssetAdministrationShell AdminShell;

        private static AssetAdministrationShellHttpClient Client;
        private static AssetAdministrationShellRepositoryHttpClient RepoClient;

        public IEndpoint Endpoint => ((IClient)RepoClient).Endpoint;

        static MainTest()
        {
            Server.Run();
            TestSubmodel = SimpleAssetAdministrationShell.TestSubmodel.GetSubmodel("TestSubmodel");
            AdminShell = TestAssetAdministrationShell.GetAssetAdministrationShell("MainAdminShell");
            MainSubmodel = SimpleAssetAdministrationShell.TestSubmodel.GetSubmodel("MainSubmodel");
            AdminShell.Submodels.Add(MainSubmodel);
            string basePath = AssetAdministrationShellRepositoryRoutes.SHELLS_AAS.Replace("{aasIdentifier}", StringOperations.Base64UrlEncode(AdminShell.Id));
            Client = new AssetAdministrationShellHttpClient(new Uri(Server.ServerUrl + basePath), false);
            RepoClient = new AssetAdministrationShellRepositoryHttpClient(new Uri(Server.ServerUrl));
        }

        [TestMethod]
        public void Test0000_CreateAssetAdministrationShell()
        {
            var testShell = TestAssetAdministrationShell.GetAssetAdministrationShell("TestAdminShell");
            var testSubmodel = SimpleAssetAdministrationShell.TestSubmodel.GetSubmodel("TestSubmodel");
            testShell.Submodels.Add(testSubmodel);

            var result = CreateAssetAdministrationShell(testShell);
            result.Success.Should().BeTrue();
            result.Entity.Should().BeEquivalentTo(testShell, options =>
            {
                options
                .Excluding(p => p.Submodels);
                return options;
            });
        }

        [TestMethod]
        public void Test0001_RetrieveAssetAdministrationShell()
        {
            var testShell = TestAssetAdministrationShell.GetAssetAdministrationShell("TestAdminShell");
            var testSubmodel = SimpleAssetAdministrationShell.TestSubmodel.GetSubmodel("TestSubmodel");
            testShell.Submodels.Add(testSubmodel);

            var result = RetrieveAssetAdministrationShell(testShell.Id);
            result.Success.Should().BeTrue();
            result.Entity.Should().BeEquivalentTo(testShell, options =>
            {
                options
                .Excluding(p => p.Submodels);
                return options;
            });
        }

        [TestMethod]
        public void Test0002_UpdateAssetAdministrationShell()
        { 
            var testShell = TestAssetAdministrationShell.GetAssetAdministrationShell("TestAdminShell");
            var testSubmodel = SimpleAssetAdministrationShell.TestSubmodel.GetSubmodel("TestSubmodel");
            testShell.Submodels.Add(testSubmodel);

            LangStringSet newDescription = new LangStringSet()
            {
                new LangString("de", "Meine neue Beschreibung"),
                new LangString("en", "My new description")
            };
            testShell.Description = newDescription;
            var updated = UpdateAssetAdministrationShell(testShell.Id, testShell);
            updated.Success.Should().BeTrue();

            var result = RetrieveAssetAdministrationShell(testShell.Id);
            result.Success.Should().BeTrue();
            result.Entity.Should().BeEquivalentTo(testShell, options =>
            {
                options
                .Excluding(p => p.Submodels);
                return options;
            });
        }

        [TestMethod]
        public void Test0003_RetrieveAllAssetAdministrationShells()
        {
            var result = RetrieveAssetAdministrationShells();
            result.Success.Should().BeTrue();
            result.Entity.Result.Should().HaveCount(2);
            result.Entity.Result.Should().ContainEquivalentOf(AdminShell, options =>
            {
                options
                .Excluding(p => p.Submodels);
                return options;
            });
        }

        [TestMethod]
        public void Test0004_DeleteAssetAdministrationShell()
        {
            var testShell = TestAssetAdministrationShell.GetAssetAdministrationShell("TestAdminShell");
            var testSubmodel = SimpleAssetAdministrationShell.TestSubmodel.GetSubmodel("TestSubmodel");
            testShell.Submodels.Add(testSubmodel);

            var result = DeleteAssetAdministrationShell(testShell.Id);
            result.Success.Should().BeTrue();
        }

        [TestMethod]
        public void Test000_RetrieveAssetAdministrationShell()
        {
            var result = RetrieveAssetAdministrationShell(AdminShell.Id);
            result.Success.Should().BeTrue();
            result.Entity.Should().BeEquivalentTo(AdminShell, options =>
            {
                options
                .Excluding(p => p.Submodels);
                return options;
            });
        }

        [TestMethod]
        public void Test001_UpdateAssetAdministrationShell()
        {
            LangStringSet newDescription = new LangStringSet()
            {
                new LangString("de", "Meine neue Beschreibung"),
                new LangString("en", "My new description")
            };
            AdminShell.Description = newDescription;

            var result1 = RetrieveAssetAdministrationShell(AdminShell.Id);
            var updated = UpdateAssetAdministrationShell(AdminShell.Id, AdminShell);
            updated.Success.Should().BeTrue();

            var result = RetrieveAssetAdministrationShell(AdminShell.Id);
            result.Success.Should().BeTrue();
            result.Entity.Should().BeEquivalentTo(AdminShell, options =>
            {
                options
                .Excluding(p => p.Submodels);
                return options;
            });
        }

        [TestMethod]
        public void Test002_UpdateAssetInformation()
        {
            AssetInformation assetInfo = new AssetInformation()
            {
                AssetKind = AssetKind.Instance,
                DefaultThumbnail = new Resource() { ContentType = "image/jpg", Path = "/test/path/to/thumbnail.jpg"},
                GlobalAssetId = AdminShell.AssetInformation.GlobalAssetId,
                SpecificAssetIds = new List<SpecificAssetId>()
                {
                    new SpecificAssetId() { Name = "MyInventoryNumber", Value = "123" }
                }
            };
            AdminShell.AssetInformation = assetInfo;
            var updated = UpdateAssetInformation(assetInfo);
            updated.Success.Should().BeTrue();

            var result = RetrieveAssetAdministrationShellByAasClient();
            result.Success.Should().BeTrue();
            result.Entity.Should().BeEquivalentTo(AdminShell, options =>
            {
                options
                .Excluding(p => p.Submodels);
                return options;
            });
        }

        [TestMethod]
        public void Test003_RetrieveAssetInformation()
        {
            var result = RetrieveAssetInformation();
            result.Success.Should().BeTrue();
            result.Entity.Should().BeEquivalentTo(AdminShell.AssetInformation, options =>
            {
                return options;
            });
        }

        [TestMethod]
        public void Test010_CreateSubmodelReference()
        {
            var reference = TestSubmodel.CreateReference();
            var result = CreateSubmodelReference(reference);
            result.Success.Should().BeTrue();
            result.Entity.Should().BeEquivalentTo(reference);
        }

        [TestMethod]
        public void Test011_RetrieveAllSubmodelReferences()
        {
            var reference = TestSubmodel.CreateReference();
            var result = RetrieveAllSubmodelReferences();
            result.Success.Should().BeTrue();
            result.Entity.Result.Should().HaveCount(2);
            result.Entity.Result.Should().ContainEquivalentOf(reference);
        }


        [TestMethod]
        public void Test100_UpdateSubmodel()
        {
            LangStringSet newDescription = new LangStringSet()
            {
                new LangString("de", "Meine neue Beschreibung"),
                new LangString("en", "My new description")
            };
            TestSubmodel.Description = newDescription;
            var updated = UpdateSubmodel(TestSubmodel);
            updated.Success.Should().BeTrue();
        }

        [TestMethod]
        public void Test101_RetrieveSubmodel()
        {
            var result = RetrieveSubmodel();

            result.Success.Should().BeTrue();
            result.Entity.IdShort.Should().BeEquivalentTo(TestSubmodel.IdShort);
            result.Entity.Id.Should().BeEquivalentTo(TestSubmodel.Id);
            result.Entity.Description.Should().BeEquivalentTo(TestSubmodel.Description);
            result.Entity.DisplayName.Should().BeEquivalentTo(TestSubmodel.DisplayName);
            result.Entity.SemanticId.Should().BeEquivalentTo(TestSubmodel.SemanticId);
            result.Entity.Kind.Should().Be(TestSubmodel.Kind);
        }

        [TestMethod]
        public void Test102_CreateSubmodelElement()
        {
            Property<string> property = new Property<string>("MyTestProperty", "MyTestValue");

            var result = CreateSubmodelElement(".", property);

            result.Success.Should().BeTrue();
            result.Entity.Should().BeEquivalentTo(property, options =>
            {
                options
                .Excluding(p => p.EmbeddedDataSpecifications)
                .Excluding(p => p.Parent)
                .Excluding(p => p.Get)
                .Excluding(p => p.Set);
                return options;
            });
        }

        [TestMethod]
        public void Test103_CreateSubmodelElementHierarchy()
        {
            SubmodelElementCollection coll = new SubmodelElementCollection("MyCollection")
            {
               Value =
                {
                      Value =
                       {
                           new Property<string>("MySubString", "MySubStringValue"),
                           new Property<int>("MySubInt", 5),
                           new Property<double>("MySubDouble", 4.5d),
                           new Property<float>("MySubFloat", 2.3f),
                           new SubmodelElementCollection("MySubCollection")
                           {
                               Value =
                               {
                                   Value =
                                   {
                                       new Property<string>("MySubSubString", "MySubSubStringValue"),
                                       new Property<int>("MySubSubInt", 6),
                                       new Property<double>("MySubSubDouble", 5.5d),
                                       new Property<float>("MySubSubFloat", 3.3f),
                                   }
                               }
                           }
                       }
                }
            };
            TestSubmodel.SubmodelElements.Add(coll);
            var created = CreateSubmodelElement(".", coll);
            created.Success.Should().BeTrue();
        }

        [TestMethod]
        public void Test104_RetrieveSubmodelElements()
        {
            var result = RetrieveSubmodelElements();
            result.Entity.Result.Should().ContainEquivalentOf(TestSubmodel.SubmodelElements["MyCollection"],
                options =>
                {
                    options
                      .Excluding(p => p.EmbeddedDataSpecifications)
                      .Excluding(p => p.Parent)
                      .Excluding(p => p.Get)
                      .Excluding(p => p.Set);
                    return options;
                });
        }

        [TestMethod]
        public void Test105_RetrieveSubmodelElement()
        {
            var result = RetrieveSubmodelElement("MyCollection.MySubCollection.MySubSubFloat");
            result.Entity.GetValue<float>().Should().Be(3.3f);
        }

        [TestMethod]
        public void Test106_UpdateSubmodelElement()
        {
            var mySubFloat = TestSubmodel.SubmodelElements["MyCollection.MySubCollection.MySubSubFloat"].Cast<Property<float>>();
            mySubFloat.Description = new LangStringSet()
            {
                new LangString("de", "Meine float Property Beschreibung"),
                new LangString("en", "My float Property description")
            };
            var updated = UpdateSubmodelElement("MyCollection.MySubCollection.MySubSubFloat", mySubFloat);
            updated.Success.Should().BeTrue();
            var retrieved = RetrieveSubmodelElement("MyCollection.MySubCollection.MySubSubFloat");
            retrieved.Success.Should().BeTrue();
            retrieved.Entity.Description.Should().BeEquivalentTo(mySubFloat.Description);
        }

        [TestMethod]
        public void Test107_RetrieveSubmodelElementHierarchy()
        {
            var result = RetrieveSubmodelElement("MyCollection.MySubCollection");
            result.Success.Should().BeTrue();
            result.Entity.Cast<ISubmodelElementCollection>().Value.Value["MySubSubInt"].GetValue<int>().Should().Be(6);
        }

        [TestMethod]
        public void Test108_UpdateSubmodelElementValue()
        {
            var result = UpdateSubmodelElementValue("MyCollection.MySubCollection.MySubSubDouble", new PropertyValue(new ElementValue(1.8d)));
            result.Success.Should().BeTrue();
        }

        [TestMethod]
        public void Test109_RetrieveSubmodelElementValue()
        {
            var result = RetrieveSubmodelElementValue("MyCollection.MySubCollection.MySubSubDouble");
            result.Success.Should().BeTrue();
            result.Entity.GetValue<double>().Should().Be(1.8d);
        }

        [TestMethod]
        public void Test110_InvokeOperation()
        {
            InvocationRequest request = new InvocationRequest(Guid.NewGuid().ToString())
            {
                InputArguments =
                {
                    new Property<string>("Expression", "3*8"),
                    new Property<int>("ComputingTime", 100)
                }
            };

            var result = InvokeOperation("Calculate", request, false);
            result.Success.Should().BeTrue();
            result.Entity.OutputArguments["Result"].GetValue<double>().Should().Be(24);

        }

        [TestMethod]
        public void Test111_InvokeOperationAsync()
        {
            InvocationRequest request = new InvocationRequest(Guid.NewGuid().ToString())
            {
                InputArguments =
                {
                    new Property<string>("Expression", "3*8"),
                    new Property<int>("ComputingTime", 2000)
                }
            };

            var result = InvokeOperation("Calculate", request, true);
            result.Success.Should().BeTrue();
            result.Entity.ExecutionState.Should().Be(ExecutionState.Initiated);

            Thread.Sleep(2500);

            var handleResult = GetInvocationResult("Calculate", request.RequestId);
            handleResult.Success.Should().BeTrue();
            handleResult.Entity.OutputArguments["Result"].GetValue<double>().Should().Be(24);
        }

        [TestMethod]
        public void Test112_DeleteSubmodelElement()
        {
            var deleted = DeleteSubmodelElement("MyCollection");
            deleted.Success.Should().BeTrue();

            var retrieved = RetrieveSubmodelElements();
            retrieved.Success.Should().BeTrue();
            retrieved.Entity.Result.Should().NotContainEquivalentOf(TestSubmodel.SubmodelElements["MyCollection"]);
        }

        [TestMethod]
        public void Test112_DeleteSubmodelReference()
        {
            var result = DeleteSubmodelReference(TestSubmodel.Id);
            result.Success.Should().BeTrue();
        }

        #region Asset Administration Shell Submodel Client

        public IResult<ISubmodel> RetrieveSubmodel(RequestLevel level = RequestLevel.Deep, RequestExtent extent = RequestExtent.WithoutBlobValue)
        {
            return Client.RetrieveSubmodel(TestSubmodel.Id, level, extent);
        }

        public IResult UpdateSubmodel(ISubmodel submodel)
        {
            return Client.UpdateSubmodel(TestSubmodel.Id, submodel);
        }

        public IResult<ISubmodelElement> CreateSubmodelElement(string rootIdShortPath, ISubmodelElement submodelElement)
        {
            return Client.CreateSubmodelElement(TestSubmodel.Id, rootIdShortPath, submodelElement);
        }

        public IResult DeleteSubmodelElement(string idShortPath)
        {
            return Client.DeleteSubmodelElement(TestSubmodel.Id, idShortPath);
        }

        public IResult<InvocationResponse> GetInvocationResult(string idShortPath, string requestId)
        {
            return Client.GetInvocationResult(AdminShell.Submodels["MainSubmodel"].Id, idShortPath, requestId);
        }

        public IResult<InvocationResponse> InvokeOperation(string idShortPath, InvocationRequest invocationRequest, bool async)
        {
            return Client.InvokeOperation(AdminShell.Submodels["MainSubmodel"].Id, idShortPath, invocationRequest, async);
        }

        public IResult<ISubmodelElement> RetrieveSubmodelElement(string idShortPath)
        {
            return Client.RetrieveSubmodelElement(TestSubmodel.Id, idShortPath);
        }

        public IResult<PagedResult<IElementContainer<ISubmodelElement>>> RetrieveSubmodelElements()
        {
            return Client.RetrieveSubmodelElements(TestSubmodel.Id);
        }

        public IResult<ValueScope> RetrieveSubmodelElementValue(string idShortPath)
        {
            return Client.RetrieveSubmodelElementValue(TestSubmodel.Id, idShortPath);
        }

        public IResult UpdateSubmodelElement(string rootIdShortPath, ISubmodelElement submodelElement)
        {
            return Client.UpdateSubmodelElement(TestSubmodel.Id, rootIdShortPath, submodelElement);
        }

        public IResult UpdateSubmodelElementValue(string idShortPath, ValueScope value)
        {
            return Client.UpdateSubmodelElementValue(TestSubmodel.Id, idShortPath, value);
        }

        public Task<IResult<ISubmodel>> RetrieveSubmodelAsync(RequestLevel level = RequestLevel.Deep, RequestExtent extent = RequestExtent.WithoutBlobValue)
        {
            return Client.RetrieveSubmodelAsync(TestSubmodel.Id, level, extent);
        }

        public Task<IResult> UpdateSubmodelAsync(ISubmodel submodel)
        {
            return Client.UpdateSubmodelAsync(TestSubmodel.Id, submodel);
        }

        public Task<IResult<ISubmodelElement>> CreateSubmodelElementAsync(string rootIdShortPath, ISubmodelElement submodelElement)
        {
            return Client.CreateSubmodelElementAsync(TestSubmodel.Id, rootIdShortPath, submodelElement);
        }

        public Task<IResult> UpdateSubmodelElementAsync(string rootIdShortPath, ISubmodelElement submodelElement)
        {
            return Client.UpdateSubmodelElementAsync(TestSubmodel.Id, rootIdShortPath, submodelElement);
        }

        public Task<IResult<PagedResult<IElementContainer<ISubmodelElement>>>> RetrieveSubmodelElementsAsync()
        {
            return Client.RetrieveSubmodelElementsAsync(TestSubmodel.Id);
        }

        public Task<IResult<ISubmodelElement>> RetrieveSubmodelElementAsync(string idShortPath)
        {
            return Client.RetrieveSubmodelElementAsync(TestSubmodel.Id, idShortPath);
        }

        public Task<IResult<ValueScope>> RetrieveSubmodelElementValueAsync(string idShortPath)
        {
            return Client.RetrieveSubmodelElementValueAsync(TestSubmodel.Id, idShortPath);
        }

        public Task<IResult> UpdateSubmodelElementValueAsync(string idShortPath, ValueScope value)
        {
            return Client.UpdateSubmodelElementValueAsync(TestSubmodel.Id, idShortPath, value);
        }

        public Task<IResult> DeleteSubmodelElementAsync(string idShortPath)
        {
            return Client.DeleteSubmodelElementAsync(TestSubmodel.Id, idShortPath);
        }

        public Task<IResult<InvocationResponse>> InvokeOperationAsync(string idShortPath, InvocationRequest invocationRequest, bool async = false)
        {
            return Client.InvokeOperationAsync(TestSubmodel.Id, idShortPath, invocationRequest, async);
        }

        public Task<IResult<InvocationResponse>> GetInvocationResultAsync(string idShortPath, string requestId)
        {
            return Client.GetInvocationResultAsync(TestSubmodel.Id, idShortPath, requestId);
        }

        #endregion

        #region Asset Administration Shell Client


        public Task<IResult<IAssetAdministrationShell>> RetrieveAssetAdministrationShellAsync()
        {
            return ((IAssetAdministrationShellClient)Client).RetrieveAssetAdministrationShellAsync();
        }

        public Task<IResult> UpdateAssetAdministrationShellAsync(IAssetAdministrationShell aas)
        {
            return ((IAssetAdministrationShellClient)Client).UpdateAssetAdministrationShellAsync(aas);
        }

        public Task<IResult<IAssetInformation>> RetrieveAssetInformationAsync()
        {
            return ((IAssetAdministrationShellClient)Client).RetrieveAssetInformationAsync();
        }

        public Task<IResult> UpdateAssetInformationAsync(IAssetInformation assetInformation)
        {
            return ((IAssetAdministrationShellClient)Client).UpdateAssetInformationAsync(assetInformation);
        }

        public Task<IResult<PagedResult<IEnumerable<IReference<ISubmodel>>>>> RetrieveAllSubmodelReferencesAsync()
        {
            return ((IAssetAdministrationShellClient)Client).RetrieveAllSubmodelReferencesAsync();
        }

        public Task<IResult<IReference>> CreateSubmodelReferenceAsync(IReference submodelRef)
        {
            return ((IAssetAdministrationShellClient)Client).CreateSubmodelReferenceAsync(submodelRef);
        }

        public Task<IResult> DeleteSubmodelReferenceAsync(string submodelIdentifier)
        {
            return ((IAssetAdministrationShellClient)Client).DeleteSubmodelReferenceAsync(submodelIdentifier);
        }

        public IResult<IAssetAdministrationShell> RetrieveAssetAdministrationShellByAasClient()
        {
            return ((IAssetAdministrationShellInterface)Client).RetrieveAssetAdministrationShell();
        }

        public IResult UpdateAssetAdministrationShellByAasClient(IAssetAdministrationShell aas)
        {
            return ((IAssetAdministrationShellInterface)Client).UpdateAssetAdministrationShell(aas);
        }

        public IResult<IAssetInformation> RetrieveAssetInformation()
        {
            return ((IAssetAdministrationShellInterface)Client).RetrieveAssetInformation();
        }

        public IResult UpdateAssetInformation(IAssetInformation assetInformation)
        {
            return ((IAssetAdministrationShellInterface)Client).UpdateAssetInformation(assetInformation);
        }

        public IResult<PagedResult<IEnumerable<IReference<ISubmodel>>>> RetrieveAllSubmodelReferences()
        {
            return ((IAssetAdministrationShellInterface)Client).RetrieveAllSubmodelReferences();
        }

        public IResult<IReference> CreateSubmodelReference(IReference submodelRef)
        {
            return ((IAssetAdministrationShellInterface)Client).CreateSubmodelReference(submodelRef);
        }

        public IResult DeleteSubmodelReference(Identifier id)
        {
            return ((IAssetAdministrationShellInterface)Client).DeleteSubmodelReference(id);
        }

        #endregion

        #region Asset Administration Shell Repository Client

        public Task<IResult<IAssetAdministrationShell>> CreateAssetAdministrationShellAsync(IAssetAdministrationShell aas)
        {
            return ((IAssetAdministrationShellRepositoryClient)RepoClient).CreateAssetAdministrationShellAsync(aas);
        }

        public Task<IResult<IAssetAdministrationShell>> RetrieveAssetAdministrationShellAsync(Identifier id)
        {
            return ((IAssetAdministrationShellRepositoryClient)RepoClient).RetrieveAssetAdministrationShellAsync(id);
        }

        public Task<IResult<PagedResult<IElementContainer<IAssetAdministrationShell>>>> RetrieveAssetAdministrationShellsAsync(int limit = 100, string cursor = "", string assetIds = "", string idShort = "")
        {
            return ((IAssetAdministrationShellRepositoryClient)RepoClient).RetrieveAssetAdministrationShellsAsync(limit, cursor, assetIds, idShort);
        }

        public Task<IResult> UpdateAssetAdministrationShellAsync(Identifier id, IAssetAdministrationShell aas)
        {
            return ((IAssetAdministrationShellRepositoryClient)RepoClient).UpdateAssetAdministrationShellAsync(id, aas);
        }

        public Task<IResult> DeleteAssetAdministrationShellAsync(Identifier id)
        {
            return ((IAssetAdministrationShellRepositoryClient)RepoClient).DeleteAssetAdministrationShellAsync(id);
        }

        public IResult<IAssetAdministrationShell> CreateAssetAdministrationShell(IAssetAdministrationShell aas)
        {
            return ((IAssetAdministrationShellRepositoryInterface)RepoClient).CreateAssetAdministrationShell(aas);
        }

        public IResult<IAssetAdministrationShell> RetrieveAssetAdministrationShell(Identifier id)
        {
            return ((IAssetAdministrationShellRepositoryInterface)RepoClient).RetrieveAssetAdministrationShell(id);
        }

        public IResult<PagedResult<IElementContainer<IAssetAdministrationShell>>> RetrieveAssetAdministrationShells(int limit = 100, string cursor = "", string assetIds = "", string idShort = "")
        {
            return ((IAssetAdministrationShellRepositoryInterface)RepoClient).RetrieveAssetAdministrationShells(limit, cursor, assetIds, idShort);
        }

        public IResult<PagedResult<IEnumerable<IReference<IAssetAdministrationShell>>>> RetrieveAssetAdministrationShellsReference(int limit = 100, string cursor = "", string assetIds = "", string idShort = "")
        {
            return((IAssetAdministrationShellRepositoryInterface)RepoClient).RetrieveAssetAdministrationShellsReference(limit, cursor);
        }

        public IResult UpdateAssetAdministrationShell(Identifier id, IAssetAdministrationShell aas)
        {
            return ((IAssetAdministrationShellRepositoryInterface)RepoClient).UpdateAssetAdministrationShell(id, aas);
        }

        public IResult DeleteAssetAdministrationShell(Identifier id)
        {
            return ((IAssetAdministrationShellRepositoryInterface)RepoClient).DeleteAssetAdministrationShell(id);
        }

        #endregion
    }
}
