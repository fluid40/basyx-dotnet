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
using BaSyx.API.Interfaces;
using BaSyx.Models.AdminShell;
using BaSyx.Utils.ResultHandling;
using BaSyx.Utils.ResultHandling.ResultTypes;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace BaSyx.API.Clients
{
    public interface IAssetAdministrationShellRepositoryClient : IAssetAdministrationShellRepositoryInterface, IClient
    {
        Task<IResult<IAssetAdministrationShell>> CreateAssetAdministrationShellAsync(IAssetAdministrationShell aas);

        Task<IResult<IAssetAdministrationShell>> RetrieveAssetAdministrationShellAsync(Identifier id);

        Task<IResult<PagedResult<IElementContainer<IAssetAdministrationShell>>>> RetrieveAssetAdministrationShellsAsync(int limit = 100, string cursor = "", string assetIds = "", string idShort = "");

        Task<IResult> UpdateAssetAdministrationShellAsync(Identifier id, IAssetAdministrationShell aas);

        Task<IResult> DeleteAssetAdministrationShellAsync(Identifier id);

        Task<IResult<PagedResult<IEnumerable<IReference<IAssetAdministrationShell>>>>>
            RetrieveAssetAdministrationShellsReferenceAsync(int limit = 100, string cursor = "");

        Task<IResult<IReference<IAssetAdministrationShell>>> RetrieveAssetAdministrationShellReferenceAsync(Identifier id);

        Task<IResult<IAssetInformation>> RetrieveAssetAdministrationShellAssetInformation(Identifier id);
    }
}
