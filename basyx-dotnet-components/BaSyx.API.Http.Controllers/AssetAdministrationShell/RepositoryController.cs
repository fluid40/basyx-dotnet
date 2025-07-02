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

using BaSyx.API.ServiceProvider;
using BaSyx.Models.AdminShell;
using BaSyx.Models.Connectivity;
using BaSyx.Utils.ResultHandling;
using BaSyx.Utils.ResultHandling.ResultTypes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BaSyx.API.Http.Controllers
{
    /// <summary>
    /// The Asset Administration Shell Repository Controller
    /// </summary>
    [ApiController]
    public class RepositoryController : Controller
    {
        private readonly IAssetAdministrationShellRepositoryServiceProvider _aasServiceProvider;
        private readonly AssetAdministrationShellRepositoryController _aasController;
        private readonly SubmodelRepositoryController _smController;


        /// <summary>
        /// The constructor for the Asset Administration Shell Repository Controller
        /// </summary>
        /// <param name="assetAdministrationShellRepositoryAasServiceProvider"></param>
        /// <param name="submodelRepositoryServiceProvider"></param>
        /// <param name="environment">The Hosting Environment provided by the dependency injection</param>
        public RepositoryController(IAssetAdministrationShellRepositoryServiceProvider assetAdministrationShellRepositoryAasServiceProvider, ISubmodelRepositoryServiceProvider submodelRepositoryServiceProvider, IWebHostEnvironment environment)
        {
            _aasServiceProvider = assetAdministrationShellRepositoryAasServiceProvider;
            _aasController = new AssetAdministrationShellRepositoryController(assetAdministrationShellRepositoryAasServiceProvider, environment);
            _smController = new SubmodelRepositoryController(submodelRepositoryServiceProvider, environment);
        }

        /// <summary>
        /// Returns all Asset Administration Shells
        /// </summary>
        /// <param name="limit">The maximum number of elements in the response array</param>
        /// <param name="cursor">A server-generated identifier retrieved from pagingMetadata that specifies from which position the result listing should continue (BASE64-URL-encoded)</param>
        /// <param name="assetIds">A list of specific Asset identifiers. Each Asset identifier is a base64-url-encoded</param>
        /// <param name="idShort">The Asset Administration Shell’s IdShort</param>
        /// <returns></returns>
        /// <response code="200">Requested Asset Administration Shells</response>
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS, Name = "GetAllAssetAdministrationShells")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagedResult<List<AssetAdministrationShell>>), 200)]
        public IActionResult GetAllAssetAdministrationShells([FromQuery] int limit = 100, [FromQuery] string cursor = "", [FromQuery] string assetIds = "", [FromQuery] string idShort = "")
        {
            return _aasController.GetAllAssetAdministrationShells(limit, cursor, assetIds, idShort);
        }

        /// <summary>
        /// Creates a new Asset Administration Shell
        /// </summary>
        /// <param name="aas">Asset Administration Shell object</param>
        /// <returns></returns>
        /// <response code="201">Asset Administration Shell created successfully</response>
        /// <response code="400">Bad Request</response>             
        [HttpPost(AssetAdministrationShellRepositoryRoutes.SHELLS, Name = "PostAssetAdministrationShell")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(AssetAdministrationShell), 201)]
        public IActionResult PostAssetAdministrationShell([FromBody] IAssetAdministrationShell aas)
        {
            return _aasController.PostAssetAdministrationShell(aas);
        }

        /// <summary>
        /// Returns References to all Asset Administration Shells
        /// </summary>
        /// <param name="limit">The maximum number of elements in the response array</param>
        /// <param name="cursor">A server-generated identifier retrieved from pagingMetadata that specifies from which position the result listing should continue (BASE64-URL-encoded)</param>
        /// <param name="assetIds">A list of specific Asset identifiers. Each Asset identifier is a base64-url-encoded</param>
        /// <param name="idShort">The Asset Administration Shell’s IdShort</param>
        /// <returns></returns>
        /// <response code="200">Requested Asset Administration Shells</response>
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS + OutputModifier.REFERENCE, Name = "GetAllAssetAdministrationShells-Reference")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagedResult<List<AssetAdministrationShell>>), 200)]
        public IActionResult GetAllAssetAdministrationShellsReference([FromQuery] int limit = 100, [FromQuery] string cursor = "", [FromQuery] string assetIds = "", [FromQuery] string idShort = "")
        {
            return _aasController.GetAllAssetAdministrationShellsReference(limit, cursor, assetIds, idShort);
        }

        /// <summary>
        /// Returns a specific Asset Administration Shell
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <returns></returns>
        /// <response code="200">Returns the requested Asset Administration Shell</response>
        /// <response code="404">No Asset Administration Shell found</response>           
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS, Name = "GetAssetAdministrationShellById")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(AssetAdministrationShell), 200)]
        public IActionResult GetAssetAdministrationShellById(string aasIdentifier)
        {
            return _aasController.GetAssetAdministrationShellById(aasIdentifier);
        }

        /// <summary>
        /// Returns a specific Asset Administration Shell Descriptor
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <returns></returns>
        /// <response code="200">Returns the requested Asset Administration Shell Descriptor</response>
        /// <response code="404">No Asset Administration Shell Descriptor found</response>           
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + DescriptionRoutes.DESCRIPTOR, Name = "GetAssetAdministrationShellDescriptorFromRepoById")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(AssetAdministrationShellDescriptor), 200)]
        public IActionResult GetAssetAdministrationShellDescriptorFromRepoById(string aasIdentifier)
        {
            return _aasController.GetAssetAdministrationShellDescriptorFromRepoById(aasIdentifier);
        }

        /// <summary>
        /// Updates an existing Asset Administration Shell
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="aas">Asset Administration Shell object</param>
        /// <returns></returns>
        /// <response code="204">Asset Administration Shell updated successfully</response>          
        [HttpPut(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS, Name = "PutAssetAdministrationShellById")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(204)]
        public IActionResult PutAssetAdministrationShellById(string aasIdentifier, [FromBody] IAssetAdministrationShell aas)
        {
            return _aasController.PutAssetAdministrationShellById(aasIdentifier, aas);
        }


        /// <summary>
        /// Deletes an Asset Administration Shell
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <returns></returns>
        /// <response code="200">Asset Administration Shell deleted successfully</response>
        [HttpDelete(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS, Name = "DeleteAssetAdministrationShellById")]
        [Produces("application/json")]
        [ProducesResponseType(204)]
        public IActionResult DeleteAssetAdministrationShellById(string aasIdentifier)
        {
            return _aasController.DeleteAssetAdministrationShellById(aasIdentifier);
        }

        #region Asset Adminstration Shell Interface

        /// <summary>
        /// Returns a specific Asset Administration Shell as a Reference
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <returns></returns>
        /// <response code="200">Requested Asset Administration Shel</response>
        /// <inheritdoc cref="AssetAdministrationShellController.GetAssetAdministrationShellReference"/>
        [HttpDelete(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + OutputModifier.REFERENCE, Name = "ShellRepo_GetAssetAdministrationShellsReference")]
        [ProducesResponseType(typeof(Reference), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 500)]
        [Produces("application/json")]
        public IActionResult ShellRepo_GetAssetAdministrationShellsReference(string aasIdentifier)
        {
            return _aasController.ShellRepo_GetAssetAdministrationShellsReference(aasIdentifier);
        }

        /// <summary>
        /// Returns the Asset Information
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <returns></returns>
        /// <response code="200">Requested Asset Information</response>
        /// <inheritdoc cref="AssetAdministrationShellController.GetAssetInformation"/>
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_ASSET_INFORMATION, Name = "ShellRepo_GetAssetInformation")]
        [ProducesResponseType(200)]
        [Produces("application/json")]
        public IActionResult ShellRepo_GetAssetInformation(string aasIdentifier)
        {
            return _aasController.ShellRepo_GetAssetInformation(aasIdentifier);
        }

        /// <summary>
        /// Updates the Asset Information
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="assetInformation">Asset Information object</param>
        /// <returns></returns>
        /// <response code="204">Asset Information updated successfully</response>
        /// <inheritdoc cref="AssetAdministrationShellController.PutAssetInformation(IAssetInformation)"/>
        [HttpPut(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_ASSET_INFORMATION, Name = "ShellRepo_PutAssetInformation")]
        [ProducesResponseType(204)]
        [Consumes("application/json")]
        [Produces("application/json")]
        public IActionResult ShellRepo_PutAssetInformation(string aasIdentifier, [FromBody] IAssetInformation assetInformation)
        {
            return _aasController.ShellRepo_PutAssetInformation(aasIdentifier, assetInformation);
        }

        /// <summary>
        /// The thumbnail of the Asset Information.
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <returns></returns>
        /// <response code="200">The thumbnail of the Asset Information</response>
        /// <inheritdoc cref="AssetAdministrationShellController.GetThumbnail()"/>
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_ASSET_INFORMATION_THUMBNAIL,
            Name = "ShellRepo_GetThumbnail")]
        [ProducesResponseType(typeof(AssetInformation), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult ShellRepo_GetThumbnail(string aasIdentifier)
        {
            return _aasController.ShellRepo_GetThumbnail(aasIdentifier);
        }

        /// <summary>
        /// Updates the thumbnail of the Asset Information.
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="file">Thumbnail to upload</param>
        /// <returns></returns>
        /// <response code="204">Thumbnail updated successfully</response>
        /// <inheritdoc cref="AssetAdministrationShellController.PutThumbnail(IFormFile)"/>
        [HttpPut(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_ASSET_INFORMATION_THUMBNAIL,
            Name = "ShellRepo_PutThumbnail")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 500)]
        public async Task<IActionResult> ShellRepo_PutThumbnail(string aasIdentifier, IFormFile file)
        {
            return await _aasController.ShellRepo_PutThumbnail(aasIdentifier, file);
        }

        /// <summary>
        /// Delete the thumbnail of the Asset Information.
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <returns></returns>
        /// <response code="200">Thumbnail deletion successful</response>
        [HttpDelete(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_ASSET_INFORMATION_THUMBNAIL, Name = "DeleteThumbnail")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 500)]
        public async Task<IActionResult> ShellRepo_DeleteThumbnail(string aasIdentifier)
        {
            return await _aasController.ShellRepo_DeleteThumbnail(aasIdentifier);
        }

        /// <summary>
        /// Returns all submodel references
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="limit">The maximum number of elements in the response array</param>
        /// <param name="cursor">A server-generated identifier retrieved from pagingMetadata that specifies from which position the result listing should continue (BASE64-URL-encoded)</param>
        /// <returns></returns>
        /// <response code="200">Requested submodel references</response> 
        /// <inheritdoc cref="AssetAdministrationShellController.GetAllSubmodelReferences"/>
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODEL_REFS, Name = "ShellRepo_GetAllSubmodelReferences")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagedResult<Reference[]>), 200)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult ShellRepo_GetAllSubmodelReferences(string aasIdentifier, [FromQuery] int limit = 100, [FromQuery] string cursor = "")
        {
            return _aasController.ShellRepo_GetAllSubmodelReferences(aasIdentifier, limit, cursor);
        }

        /// <summary>
        /// Creates a submodel reference at the Asset Administration Shell
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelReference">Reference to the Submodel</param>
        /// <returns></returns>
        /// <response code="201">Submodel reference created successfully</response>
        /// <response code="400">Bad Request</response>       
        /// <inheritdoc cref="AssetAdministrationShellController.PostSubmodelReference(IReference)"/>          
        [HttpPost(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODEL_REFS, Name = "ShellRepo_PostSubmodelReference")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Reference), 201)]
        [ProducesResponseType(typeof(Result), 400)]
        public IActionResult ShellRepo_PostSubmodelReference(string aasIdentifier, [FromBody] IReference submodelReference)
        {
            return _aasController.ShellRepo_PostSubmodelReference(aasIdentifier, submodelReference);
        }

        /// <summary>
        /// Deletes the submodel reference from the Asset Administration Shell. Does not delete the submodel itself!
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <returns></returns>
        /// <response code="204">Submodel deleted successfully</response>
        /// <response code="400">Bad Request</response>    
        /// <inheritdoc cref="AssetAdministrationShellController.DeleteSubmodelReferenceById(string)"/>          
        [HttpDelete(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODEL_REFS_BYID, Name = "ShellRepo_DeleteSubmodelReferenceById")]
        [Produces("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Result), 400)]
        public IActionResult ShellRepo_DeleteSubmodelReferenceById(string aasIdentifier, string submodelIdentifier)
        {
            return _aasController.ShellRepo_DeleteSubmodelReferenceById(aasIdentifier, submodelIdentifier);
        }

        #endregion

        #region Submodel Interface

        /// <summary>
        /// Returns the Submodel
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <param name="extent">Determines to which extent the resource is being serialized</param>
        /// <returns></returns>
        /// <response code="200">Requested submodel references</response> 
        /// <response code="200">Submodel in Path notation</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_GetSubmodel(string, RequestLevel, RequestExtent)"/>
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID, Name = "ShellRepo_GetSubmodel")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Submodel), 200)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult ShellRepo_GetSubmodel(string aasIdentifier, string submodelIdentifier, [FromQuery] RequestLevel level = default, [FromQuery] RequestExtent extent = default)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.GetSubmodelById(submodelIdentifier, level, extent);
        }
        
        /// <summary>
        /// Replaces the Submodel
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodel">Submodel object</param>
        /// <returns></returns>
        /// <response code="204">Submodel updated successfully</response>     
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_PutSubmodel(string, ISubmodel)"/>
        [HttpPut(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID, Name = "ShellRepo_PutSubmodel")]
        [Produces("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult ShellRepo_PutSubmodel(string aasIdentifier, string submodelIdentifier, [FromBody] ISubmodel submodel)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.PutSubmodelById(submodelIdentifier, submodel);
        }

        /// <summary>
        /// Updates the Submodel
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodel">Submodel object</param>
        /// <returns></returns>
        /// <response code="204">Submodel updated successfully</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_PatchSubmodel(string, ISubmodel)"/>
        [HttpPatch(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID, Name = "ShellRepo_PatchSubmodel")]
        [Produces("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult ShellRepo_PatchSubmodel(string aasIdentifier, string submodelIdentifier, [FromBody] ISubmodel submodel)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.PatchSubmodel(submodelIdentifier, submodel);
        }

        /// <summary>
        /// Deletes the submodel from the Asset Administration Shell and the Repository.
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <returns></returns>
        /// <response code="204">Submodel deleted successfully</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_DeleteSubmodel(string)"/>
        [HttpDelete(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID, Name = "ShellRepo_DeleteSubmodel")]
        [Produces("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult ShellRepo_DeleteSubmodel(string aasIdentifier, string submodelIdentifier)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.DeleteSubmodelById(submodelIdentifier);
        }

        /// <summary>
        /// Returns the metadata attributes of a specific Submodel
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <returns></returns>
        /// <response code="200">Requested Submodel</response>
        /// <response code="404">Submodel not found</response>  
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_GetSubmodelMetadata(string)"/>
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + OutputModifier.METADATA, Name = "ShellRepo_GetSubmodelMetadata")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Submodel), 200)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult ShellRepo_GetSubmodelMetadata(string aasIdentifier, string submodelIdentifier)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.DeleteSubmodelById(submodelIdentifier);
        }

        /// <summary>
        /// Updates the metadata attributes of an existing Submodel
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodel">The metadata attributes of the Submodel object</param>
        /// <returns></returns>
        /// <response code="200">Requested Submodel</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_PatchSubmodelMetadata(string, ISubmodel)"/> 
        [HttpPatch(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + OutputModifier.METADATA, Name = "ShellRepo_PatchSubmodelMetadata")]
        [Produces("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult ShellRepo_PatchSubmodelMetadata(string aasIdentifier, string submodelIdentifier, [FromBody] ISubmodel submodel)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_PatchSubmodelMetadata(submodelIdentifier, submodel);
        }

        /// <summary>
        /// Returns a specific Submodel in the ValueOnly representation
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <param name="extent">Determines to which extent the resource is being serialized</param>
        /// <returns></returns>
        /// <response code="200">ValueOnly representation of the Submodel</response>   
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_GetSubmodelValue(string, RequestLevel, RequestExtent)"/>
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + OutputModifier.VALUE, Name = "ShellRepo_GetSubmodelValue")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Submodel), 200)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult ShellRepo_GetSubmodelValue(string aasIdentifier, string submodelIdentifier, [FromQuery] RequestLevel level = default, [FromQuery] RequestExtent extent = default)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_GetSubmodelValue(submodelIdentifier, level, extent);
        }

        /// <summary>
        /// Updates the values of an existing Submodel
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="requestBody">Requested submodel element</param>
        /// <returns></returns>
        /// <response code="204">Submodel object in its ValueOnly representation</response>  
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_PatchSubmodelValueOnly(string, JsonDocument)"/>
        [HttpPatch(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + OutputModifier.VALUE, Name = "ShellRepo_PatchSubmodelValueOnly")]
        [Produces("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult ShellRepo_PatchSubmodelValueOnly(string aasIdentifier, string submodelIdentifier, [FromBody] JsonDocument requestBody)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_PatchSubmodelValueOnly(submodelIdentifier, requestBody);
        }

        /// <summary>
        /// Returns the Reference of a specific Submodel
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <returns></returns>
        /// <response code="200">ValueOnly representation of the Submodel</response> 
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_GetSubmodelReference(string)"/>    
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + OutputModifier.REFERENCE, Name = "ShellRepo_GetSubmodelReference")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Reference), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult ShellRepo_GetSubmodelReference(string aasIdentifier, string submodelIdentifier)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.GetAllSubmodelsReference(submodelIdentifier);
        }

        /// <summary>
        /// Returns a specific Submodel in the Path notation
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <returns></returns>
        /// <response code="200">ValueOnly representation of the Submodel</response> 
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_GetSubmodelPath(string, RequestLevel)"/>    
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + OutputModifier.PATH, Name = "ShellRepo_GetSubmodelPath")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Reference), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult ShellRepo_GetSubmodelPath(string aasIdentifier, string submodelIdentifier, [FromQuery] RequestLevel level = default)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_GetSubmodelPath(submodelIdentifier, level);
        }

        /// <summary>
        /// Returns all submodel elements including their hierarchy
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <param name="extent">Determines to which extent the resource is being serialized</param>
        /// <param name="limit">The maximum number of elements in the response array</param>
        /// <param name="cursor">A server-generated identifier retrieved from pagingMetadata that specifies from which position the result listing should continue</param>
        /// <returns></returns>
        /// <response code="200">List of found submodel elements</response>
        /// <response code="404">Submodel not found</response>   
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_GetAllSubmodelElements(string, int, string, RequestLevel, RequestExtent)"/>
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS, Name = "ShellRepo_GetAllSubmodelElements")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SubmodelElement[]), 200)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult ShellRepo_GetAllSubmodelElements(string aasIdentifier, string submodelIdentifier, [FromQuery] int limit = 100, [FromQuery] string cursor = "", [FromQuery] RequestLevel level = default, [FromQuery] RequestExtent extent = default)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_GetAllSubmodelElements(submodelIdentifier, limit, cursor, level, extent);
        }

        /// <summary>
        /// Creates a new submodel element
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelElement">Requested submodel element</param>
        /// <returns></returns>
        /// <response code="201">Submodel element created successfully</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_PostSubmodelElement(string, ISubmodelElement)"/>
        [HttpPost(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS, Name = "ShellRepo_PostSubmodelElement")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(SubmodelElement), 201)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult ShellRepo_PostSubmodelElement(string aasIdentifier, string submodelIdentifier, [FromBody] ISubmodelElement submodelElement)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_PostSubmodelElement(submodelIdentifier, submodelElement);
        }

        /// <summary>
        /// Returns the metadata attributes of all submodel elements including their hierarchy
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <param name="limit">The maximum number of elements in the response array</param>
        /// <param name="cursor">A server-generated identifier retrieved from pagingMetadata that specifies from which position the result listing should continue</param>
        /// <returns></returns>
        /// <response code="200">List of found submodel elements</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_GetAllSubmodelElementsMetadata(string, int, string, RequestLevel)"/>   
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS + OutputModifier.METADATA, Name = "ShellRepo_GetAllSubmodelElementsMetadata")]
		[Produces("application/json")]
		[ProducesResponseType(typeof(Submodel), 200)]
		[ProducesResponseType(typeof(Result), 400)]
		[ProducesResponseType(typeof(Result), 403)]
		[ProducesResponseType(typeof(Result), 500)]
		public IActionResult ShellRepo_GetAllSubmodelElementsMetadata(string aasIdentifier, string submodelIdentifier, [FromQuery] int limit = 100, [FromQuery] string cursor = "", [FromQuery] RequestLevel level = default)
		{
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_GetAllSubmodelElementsMetadata(submodelIdentifier, limit, cursor, level);
        }

        /// <summary>
        /// Returns all submodel elements including their hierarchy in the ValueOnly representation
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="limit">The maximum number of elements in the response array</param>
        /// <param name="cursor">A server-generated identifier retrieved from pagingMetadata that specifies from which position the result listing should continue</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <param name="extent">Determines to which extent the resource is being serialized</param>
        /// <returns></returns>
        /// <response code="200">List of found submodel elements</response>  
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_GetAllSubmodelElementsValueOnly(string, int, string, RequestLevel, RequestExtent)"/>   
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS + OutputModifier.VALUE, Name = "ShellRepo_GetAllSubmodelElementsValueOnly")]
		[Produces("application/json")]
		[ProducesResponseType(typeof(Submodel), 200)]
		[ProducesResponseType(typeof(Result), 400)]
		[ProducesResponseType(typeof(Result), 403)]
		[ProducesResponseType(typeof(Result), 500)]
		public IActionResult ShellRepo_GetAllSubmodelElementsValueOnly(string aasIdentifier, string submodelIdentifier, [FromQuery] int limit = 100, [FromQuery] string cursor = "", [FromQuery] RequestLevel level = default, [FromQuery] RequestExtent extent = default)
		{
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_GetAllSubmodelElementsValueOnly(submodelIdentifier, limit, cursor, level);
        }

        /// <summary>
        /// Returns the References of all submodel elements
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="limit">The maximum number of elements in the response array</param>
        /// <param name="cursor">A server-generated identifier retrieved from pagingMetadata that specifies from which position the result listing should continue</param>
        /// <returns></returns>
        /// <response code="200">List of found submodel elements</response>  
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_GetAllSubmodelElementsReference(string, int, string)"/>   
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS + OutputModifier.REFERENCE, Name = "ShellRepo_GetAllSubmodelElementsReference")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Submodel), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult ShellRepo_GetAllSubmodelElementsReference(string aasIdentifier, string submodelIdentifier, [FromQuery] int limit = 100, [FromQuery] string cursor = "")
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_GetAllSubmodelElementsReference(submodelIdentifier, limit, cursor);
        }

        /// <summary>
        /// Returns the References of all submodel elements
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="limit">The maximum number of elements in the response array</param>
        /// <param name="cursor">A server-generated identifier retrieved from pagingMetadata that specifies from which position the result listing should continue</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <returns></returns>
        /// <response code="200">List of found submodel elements</response>  
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_GetAllSubmodelElementsPath(string, int, string, RequestLevel)"/>   
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS + OutputModifier.PATH, Name = "ShellRepo_GetAllSubmodelElementsPath")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Submodel), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult ShellRepo_GetAllSubmodelElementsPath(string aasIdentifier, string submodelIdentifier, [FromQuery] int limit = 100, [FromQuery] string cursor = "", [FromQuery] RequestLevel level = default)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_GetAllSubmodelElementsPath(submodelIdentifier, limit, cursor);
        }

        /// <summary>
        /// Returns a specific submodel element from the Submodel at a specified path
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <param name="extent">Determines to which extent the resource is being serialized</param>
        /// <returns></returns>
        /// <response code="200">Requested submodel element</response>  
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_GetSubmodelElementByPath(string, string, RequestLevel, RequestExtent)"/>
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH, Name = "ShellRepo_GetSubmodelElementByPath")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SubmodelElement), 200)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult ShellRepo_GetSubmodelElementByPath(string aasIdentifier, string submodelIdentifier, string idShortPath, [FromQuery] RequestLevel level = default, [FromQuery] RequestExtent extent = default)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_GetSubmodelElementByPath(submodelIdentifier, idShortPath, level, extent);
        }

        /// <summary>
        /// Creates a new submodel element at a specified path within submodel elements hierarchy
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated)</param>
        /// <param name="submodelElement">Requested submodel element</param>
        /// <returns></returns>
        /// <response code="201">Submodel element created successfully</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_PostSubmodelElementByPath(string, string, ISubmodelElement)"/>
        [HttpPost(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH, Name = "ShellRepo_PostSubmodelElementByPath")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(SubmodelElement), 201)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult ShellRepo_PostSubmodelElementByPath(string aasIdentifier, string submodelIdentifier, string idShortPath, [FromBody] ISubmodelElement submodelElement)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_PostSubmodelElementByPath(submodelIdentifier, idShortPath, submodelElement);
        }

        /// <summary>
        /// Replaces an existing submodel element at a specified path within the submodel element hierarchy
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated)</param>
        /// <param name="requestBody">Requested submodel element</param>
        /// <returns></returns>
        /// <response code="204">Submodel element updated successfully</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_PutSubmodelElementByPath(string, string, ISubmodelElement)"/>
        [HttpPut(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH, Name = "ShellRepo_PutSubmodelElementByPath")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(SubmodelElement), 201)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult ShellRepo_PutSubmodelElementByPath(string aasIdentifier, string submodelIdentifier, string idShortPath, [FromBody] ISubmodelElement requestBody)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_PutSubmodelElementByPath(submodelIdentifier, idShortPath, requestBody);
        }

        /// <summary>
        /// Updates an existing SubmodelElement
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated)</param>
        /// <param name="requestBody">Requested submodel element</param>
        /// <returns></returns>
        /// <response code="204">Submodel element updated successfully</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_PatchSubmodelElementByPath(string, string, ISubmodelElement)"/>
        [HttpPatch(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH,
            Name = "ShellRepo_PatchSubmodelElementByPath")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult ShellRepo_PatchSubmodelElementByPath(string aasIdentifier, string submodelIdentifier, string idShortPath, [FromBody] ISubmodelElement requestBody)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_PatchSubmodelElementByPath(submodelIdentifier, idShortPath, requestBody);
        }

        /// <summary>
        /// Deletes a submodel element at a specified path within the submodel elements hierarchy
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated)</param>
        /// <returns></returns>
        /// <response code="204">Submodel element deleted successfully</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_DeleteSubmodelElementByPath(string, string)"/>
        [HttpDelete(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH, Name = "ShellRepo_DeleteSubmodelElementByPath")]
        [Produces("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult ShellRepo_DeleteSubmodelElementByPath(string aasIdentifier, string submodelIdentifier, string idShortPath)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_DeleteSubmodelElementByPath(submodelIdentifier, idShortPath);
        }

        /// <summary>
        /// Returns the metadata attributes of a specific submodel element from the Submodel at a specified
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <returns></returns>
        /// <response code="200">Requested submodel element in its ValueOnly representation</response>
        /// <response code="404">Submodel Element not found</response>     
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_GetSubmodelElementByPathMetadata(string, string, RequestLevel)"/>
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH + OutputModifier.METADATA, Name = "ShellRepo_GetSubmodelElementByPathMetadata")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SubmodelElement), 200)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult ShellRepo_GetSubmodelElementByPathMetadata(string aasIdentifier, string submodelIdentifier, string idShortPath, [FromQuery] RequestLevel level = default)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_GetSubmodelElementByPathMetadata(submodelIdentifier, idShortPath, level);
        }

        /// <summary>
        /// Updates the metadata attributes an existing SubmodelElement
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated)</param>
        /// <param name="submodelElement">Metadata attributes of the SubmodelElement</param>
        /// <returns></returns>
        /// <response code="200">Requested submodel element in its ValueOnly representation</response> 
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_PatchSubmodelElementByPathMetadata(string, string, ISubmodelElement)"/> 
        [HttpPatch(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH + OutputModifier.METADATA,
            Name = "ShellRepo_PatchSubmodelElementByPathMetadata")]
        [Produces("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult ShellRepo_PatchSubmodelElementByPathMetadata(string aasIdentifier, string submodelIdentifier, string idShortPath, [FromBody] ISubmodelElement submodelElement)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_PatchSubmodelElementByPathMetadata(submodelIdentifier, idShortPath, submodelElement);
        }

        /// <summary>
        /// Returns a specific submodel element from the Submodel at a specified path in the ValueOnly representation
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <param name="extent">Determines to which extent the resource is being serialized</param>
        /// <returns></returns>
        /// <response code="200">Requested submodel element in its ValueOnly representation</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_GetSubmodelElementByPathValueOnly(string, string, RequestLevel, RequestExtent)"/>
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH + OutputModifier.VALUE, Name = "ShellRepo_GetSubmodelElementByPathValueOnly")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SubmodelElement), 200)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult ShellRepo_GetSubmodelElementByPathValueOnly(string aasIdentifier, string submodelIdentifier, string idShortPath, [FromQuery] RequestLevel level = default, [FromQuery] RequestExtent extent = default)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_GetSubmodelElementByPathValueOnly(submodelIdentifier, idShortPath, level, extent);
        }

        /// <summary>
        /// Returns the Reference of a specific submodel element from the Submodel at a specified path
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated)</param>
        /// <returns></returns>
        /// <response code="200">Requested submodel element in its ValueOnly representation</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_GetSubmodelElementByPathReference(string, string)"/>
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH + OutputModifier.REFERENCE,
            Name = "ShellRepo_GetSubmodelElementByPathReference")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Reference), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult ShellRepo_GetSubmodelElementByPathReference(string aasIdentifier, string submodelIdentifier, string idShortPath)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_GetSubmodelElementByPathReference(submodelIdentifier, idShortPath);
        }

        /// <summary>
        /// Returns the Reference of a specific submodel element from the Submodel at a specified path
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <returns></returns>
        /// <response code="200">Requested submodel element in its ValueOnly representation</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_GetSubmodelElementByPathPath(string, string, RequestLevel)"/>
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH + OutputModifier.PATH,
            Name = "ShellRepo_GetSubmodelElementByPathPath")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Reference), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult ShellRepo_GetSubmodelElementByPathPath(string aasIdentifier, string submodelIdentifier, string idShortPath, [FromQuery] RequestLevel level = default)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_GetSubmodelElementByPathPath(submodelIdentifier, idShortPath, level);
        }

        /// <summary>
        /// Downloads file content from a specific submodel element from the Submodel at a specified path
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated), in this case a file</param>
        /// <returns></returns>
        /// <response code="200">Requested file</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_GetFileByPath(string, string)"/>
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH_ATTACHMENT, Name = "ShellRepo_GetFileByPath")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 405)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult ShellRepo_GetFileByPath(string aasIdentifier, string submodelIdentifier, string idShortPath)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_GetFileByPath(submodelIdentifier, idShortPath);
        }

        /// <summary>
        /// Uploads file content to an existing submodel element at a specified path within submodel elements hierarchy
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated), in this case a file</param>
        /// <param name="file">Content to upload</param>
        /// <returns></returns>
        /// <response code="200">Content uploaded successfully</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">File not found</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_PutFileByPath(string, string, IFormFile)"/>
        [HttpPut(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH_ATTACHMENT, Name = "ShellRepo_PutFileByPath")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 405)]
        [ProducesResponseType(typeof(Result), 500)]
        public async Task<IActionResult> ShellRepo_PutFileByPath(string aasIdentifier, string submodelIdentifier, string idShortPath, IFormFile file)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return await _smController.SubmodelRepo_PutFileByPath(submodelIdentifier, idShortPath, file);
        }

        /// <summary>
        /// Deletes file content of an existing submodel element at a specified path within submodel elements hierarchy
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated), in this case a file</param>
        /// <returns></returns>
        /// <response code="200">Content uploaded successfully</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">File not found</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_DeleteFileByPath(string, string)"/>
        [HttpDelete(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH_ATTACHMENT, Name = "ShellRepo_DeleteFileByPath")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 405)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult ShellRepo_DeleteFileByPath(string aasIdentifier, string submodelIdentifier, string idShortPath)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_DeleteFileByPath(submodelIdentifier, idShortPath);
        }

        /// <summary>
        /// Updates the value of an existing submodel element value at a specified path within submodel elements hierarchy
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated)</param>
        /// <param name="requestBody">Requested submodel element</param>
        /// <returns></returns>
        /// <response code="204">Submodel element updated successfully</response>
        /// <response code="400">Bad Request</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_PatchSubmodelElementValueByPathValueOnly(string, string, JsonDocument)"/>
        [HttpPatch(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH + OutputModifier.VALUE, Name = "ShellRepo_PatchSubmodelElementValueByPathValueOnly")]
        [Produces("application/json")]
        [ProducesResponseType(204)]
        public IActionResult ShellRepo_PatchSubmodelElementValueByPathValueOnly(string aasIdentifier, string submodelIdentifier, string idShortPath, [FromBody] JsonDocument requestBody)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_PatchSubmodelElementValueByPathValueOnly(submodelIdentifier, idShortPath, requestBody);
        }

        /// <summary>
        /// Synchronously invokes an Operation at a specified path
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated), in this case an operation</param>
        /// <param name="operationRequest">Operation request object</param>
        /// <returns></returns>
        /// <response code="200">Operation invoked successfully</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_InvokeOperationSync(string, string, InvocationRequest)"/>
        [HttpPost(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH_INVOKE, Name = "ShellRepo_InvokeOperationSync")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult ShellRepo_InvokeOperationSync(string aasIdentifier, string submodelIdentifier, string idShortPath, [FromBody] InvocationRequest operationRequest)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_InvokeOperationSync(submodelIdentifier, idShortPath, operationRequest);
        }

        /// <summary>
        /// Synchronously invokes an Operation at a specified path
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated), in this case an operation</param>
        /// <param name="operationRequest">Operation request object</param>
        /// <returns></returns>
        /// <response code="200">Operation invoked successfully</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_InvokeOperationSyncValueOnly(string, string, InvocationRequest)"/>
        [HttpPost(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH_INVOKE + OutputModifier.VALUE, Name = "ShellRepo_InvokeOperationSyncValueOnly")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(InvocationResponse), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 405)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult ShellRepo_InvokeOperationSyncValueOnly(string aasIdentifier, string submodelIdentifier, string idShortPath, [FromBody] InvocationRequest operationRequest)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_InvokeOperationSyncValueOnly(submodelIdentifier, idShortPath, operationRequest);
        }

        /// <summary>
        /// Asynchronously invokes an Operation at a specified path
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated), in this case an operation</param>
        /// <param name="operationRequest">Operation request object</param>
        /// <returns></returns>
        /// <response code="200">Operation invoked successfully</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_InvokeOperationAsync(string, string, InvocationRequest)"/>
        [HttpPost(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH_INVOKE_ASYNC, Name = "ShellRepo_InvokeOperationAsync")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult ShellRepo_InvokeOperationAsync(string aasIdentifier, string submodelIdentifier, string idShortPath, [FromBody] InvocationRequest operationRequest)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_InvokeOperationAsync(submodelIdentifier, idShortPath, operationRequest);
        }

        /// <summary>
        /// Asynchronously invokes an Operation at a specified path
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated), in this case an operation</param>
        /// <param name="operationRequest">Operation request object</param>
        /// <returns></returns>
        /// <response code="200">Operation invoked successfully</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_InvokeOperationAsyncValueOnly(string, string, InvocationRequest)"/>
        [HttpPost(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH_INVOKE_ASYNC + OutputModifier.VALUE, Name = "ShellRepo_InvokeOperationAsyncValueOnly")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(InvocationResponse), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 405)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult ShellRepo_InvokeOperationAsyncValueOnly(string aasIdentifier, string submodelIdentifier, string idShortPath, [FromBody] InvocationRequest operationRequest)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_InvokeOperationAsyncValueOnly(submodelIdentifier, idShortPath, operationRequest);
        }

        /// <summary>
        /// Returns the Operation status of an asynchronous invoked Operation
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated), in this case an operation</param>
        /// <param name="handleId">The returned handle id of an operation’s asynchronous invocation used to request the current state of the operation’s execution (BASE64-URL-encoded)</param>
        /// <returns></returns>
        /// <response code="200">Operation result object</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_GetOperationAsyncStatus(string, string, string)"/>
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH_OPERATION_STATUS, Name = "ShellRepo_GetOperationAsyncStatus")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Result), 302)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult ShellRepo_GetOperationAsyncStatus(string aasIdentifier, string submodelIdentifier, string idShortPath, string handleId)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_GetOperationAsyncStatus(submodelIdentifier, idShortPath, handleId);
        }

        /// <summary>
        /// Returns the Operation result of an asynchronous invoked Operation
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated), in this case an operation</param>
        /// <param name="handleId">The returned handle id of an operation’s asynchronous invocation used to request the current state of the operation’s execution (BASE64-URL-encoded)</param>
        /// <returns></returns>
        /// <response code="200">Operation result object</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_GetOperationAsyncResult(string, string, string)"/>
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH_OPERATION_RESULTS, Name = "ShellRepo_GetOperationAsyncResult")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(InvocationResponse), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult ShellRepo_GetOperationAsyncResult(string aasIdentifier, string submodelIdentifier, string idShortPath, string handleId)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_GetOperationAsyncResult(submodelIdentifier, idShortPath, handleId);
        }

        /// <summary>
        /// Returns the Operation result of an asynchronous invoked Operation
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated), in this case an operation</param>
        /// <param name="handleId">The returned handle id of an operation’s asynchronous invocation used to request the current state of the operation’s execution (BASE64-URL-encoded)</param>
        /// <returns></returns>
        /// <response code="200">Operation result object</response>
        /// <inheritdoc cref="AssetAdministrationShellController.Shell_GetOperationAsyncResultValueOnly(string, string, string)"/>
        [HttpGet(AssetAdministrationShellRepositoryRoutes.SHELLS_AAS + AssetAdministrationShellRoutes.AAS_SUBMODELS_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH_OPERATION_RESULTS + OutputModifier.VALUE, Name = "ShellRepo_GetOperationAsyncResultValueOnly")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(InvocationResponse), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult ShellRepo_GetOperationAsyncResultValueOnly(string aasIdentifier, string submodelIdentifier, string idShortPath, string handleId)
        {
            var decodedSubmodelIdentifier = ResultHandling.Base64UrlDecode(submodelIdentifier);

            // check if aas exists
            if (_aasServiceProvider.IsNullOrNotFound(aasIdentifier, out IActionResult aasResult, out IAssetAdministrationShellServiceProvider aasProvider))
                return aasResult;

            var retrievedAas = aasProvider.RetrieveAssetAdministrationShell();

            if (!retrievedAas.Success || retrievedAas.Entity == null)
                return retrievedAas.CreateActionResult(CrudOperation.Retrieve);

            // check if ass contains sm
            if (retrievedAas.Entity.SubmodelReferences.All(e => e.First.Value != decodedSubmodelIdentifier))
                return new NotFoundObjectResult(new Result(false, new NotFoundMessage($"Asset administration shell does not contain a submodel with ID '{decodedSubmodelIdentifier}'")));

            return _smController.SubmodelRepo_GetOperationAsyncResultValueOnly(submodelIdentifier, idShortPath, handleId);
        }

        #endregion

        /// <summary>
        /// Returns all Submodels
        /// </summary>
        /// <param name="semanticId">The value of the semantic id reference (BASE64-URL-encoded)</param>
        /// <param name="idShort">The Asset Administration Shell’s IdShort</param>
        /// <param name="limit">The maximum number of elements in the response array</param>
        /// <param name="cursor">A server-generated identifier retrieved from pagingMetadata that specifies from which position the result listing should continue (BASE64-URL-encoded)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <param name="extent">Determines to which extent the resource is being serialized</param>
        /// <returns>Requested Submodels</returns>
        [HttpGet(SubmodelRepositoryRoutes.SUBMODELS, Name = "GetAllSubmodels")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagedResult<List<Submodel>>), 200)]
        public IActionResult GetAllSubmodels([FromQuery] string semanticId = "", [FromQuery] string idShort = "", [FromQuery] int limit = 100, [FromQuery] string cursor = "", [FromQuery] RequestLevel level = default, [FromQuery] RequestExtent extent = default)
        {
            return _smController.GetAllSubmodels(semanticId, idShort, limit, cursor, level, extent);
        }

        /// <summary>
        /// Returns the metadata attributes of all Submodels
        /// </summary>
        /// <param name="semanticId">The value of the semantic id reference (BASE64-URL-encoded)</param>
        /// <param name="idShort">The Asset Administration Shell’s IdShort</param>
        /// <param name="limit">The maximum number of elements in the response array</param>
        /// <param name="cursor">A server-generated identifier retrieved from pagingMetadata that specifies from which position the result listing should continue (BASE64-URL-encoded)</param>
        /// <returns>Requested Submodels</returns>
        [HttpGet(SubmodelRepositoryRoutes.SUBMODELS + OutputModifier.METADATA, Name = "GetAllSubmodels-Metadata")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagedResult<List<Submodel>>), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 401)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult GetAllSubmodelsMetadata([FromQuery] string semanticId = "", [FromQuery] string idShort = "", [FromQuery] int limit = 100, [FromQuery] string cursor = "")
        {
            return _smController.GetAllSubmodelsMetadata(semanticId, idShort, limit, cursor);
        }

        /// <summary>
        /// Returns all Submodels in their ValueOnly representation
        /// </summary>
        /// <param name="semanticId">The value of the semantic id reference (BASE64-URL-encoded)</param>
        /// <param name="idShort">The Asset Administration Shell’s IdShort</param>
        /// <param name="limit">The maximum number of elements in the response array</param>
        /// <param name="cursor">A server-generated identifier retrieved from pagingMetadata that specifies from which position the result listing should continue (BASE64-URL-encoded)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <param name="extent">Determines to which extent the resource is being serialized</param>
        /// <returns></returns>
        /// <response code="200">Requested Submodels in their ValueOnly representation</response>  
        [HttpGet(SubmodelRepositoryRoutes.SUBMODELS + OutputModifier.VALUE, Name = "GetAllSubmodels-ValueOnly")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagedResult), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 401)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult GetAllSubmodelsValueOnly([FromQuery] string semanticId = "", [FromQuery] string idShort = "", [FromQuery] int limit = 100, [FromQuery] string cursor = "", [FromQuery] RequestLevel level = default, [FromQuery] RequestExtent extent = default)
        {
            return _smController.GetAllSubmodelsValueOnly(semanticId, idShort, limit, cursor, level, extent);
        }

        /// <summary>
        /// Returns the References for all Submodels
        /// </summary>
        /// <param name="semanticId">The value of the semantic id reference (BASE64-URL-encoded)</param>
        /// <param name="idShort">The Asset Administration Shell’s IdShort</param>
        /// <param name="limit">The maximum number of elements in the response array</param>
        /// <param name="cursor">A server-generated identifier retrieved from pagingMetadata that specifies from which position the result listing should continue (BASE64-URL-encoded)</param>
        /// <returns></returns>
        /// <response code="200">References of the requested Submodels</response>     
        [HttpGet(SubmodelRepositoryRoutes.SUBMODELS + OutputModifier.REFERENCE, Name = "GetAllSubmodels-Reference")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Reference), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 401)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult GetAllSubmodelsReference([FromQuery] string semanticId = "", [FromQuery] string idShort = "", [FromQuery] int limit = 100, [FromQuery] string cursor = "")
        {
            return _smController.GetAllSubmodelsReference(semanticId, idShort, limit, cursor);
        }


        /// <summary>
        /// Returns the Paths for all Submodels
        /// </summary>
        /// <param name="semanticId">The value of the semantic id reference (BASE64-URL-encoded)</param>
        /// <param name="idShort">The Asset Administration Shell’s IdShort</param>
        /// <param name="limit">The maximum number of elements in the response array</param>
        /// <param name="cursor">A server-generated identifier retrieved from pagingMetadata that specifies from which position the result listing should continue (BASE64-URL-encoded)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <returns></returns>
        /// <response code="200">References of the requested Submodels</response>     
        [HttpGet(SubmodelRepositoryRoutes.SUBMODELS + OutputModifier.PATH, Name = "GetAllSubmodelsPath-Path")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Reference), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 401)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult GetAllSubmodelsPath([FromQuery] string semanticId = "", [FromQuery] string idShort = "", [FromQuery] int limit = 100, [FromQuery] string cursor = "", [FromQuery] RequestLevel level = default)
        {
            return _smController.GetAllSubmodelsPath(semanticId, idShort, limit, cursor, level);
        }

        /// <summary>
        /// Creates a new Submodel
        /// </summary>
        /// <param name="submodel">Submodel object</param>
        /// <returns></returns>
        /// <response code="201">Submodel created successfully</response>
        /// <response code="400">Bad Request</response>             
        [HttpPost(SubmodelRepositoryRoutes.SUBMODELS, Name = "PostSubmodel")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Submodel), 201)]
        public IActionResult PostSubmodel([FromBody] ISubmodel submodel)
        {
            return _smController.PostSubmodel(submodel);
        }

        /// <summary>
        /// Returns a specific Submodel
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="level"></param>
        /// <param name="extent"></param>
        /// <returns></returns>
        /// <response code="200">Requested Submodel</response>
        /// <response code="404">No Submodel found</response>
        /// <inheritdoc cref="SubmodelController.GetSubmodel"/> 
        [HttpGet(SubmodelRepositoryRoutes.SUBMODEL_BYID, Name = "GetSubmodelById")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Submodel), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult GetSubmodelById(string submodelIdentifier, [FromQuery] RequestLevel level = default, [FromQuery] RequestExtent extent = default)
        {
            return _smController.GetSubmodelById(submodelIdentifier, level, extent);
        }

        /// <summary>
        /// Replace Replace an existing Submodel
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodel">Submodel object</param>
        /// <returns></returns>
        /// <response code="201">Submodel updated successfully</response>
        /// <response code="400">Bad Request</response>             
        [HttpPut(SubmodelRepositoryRoutes.SUBMODEL_BYID, Name = "PutSubmodelById")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Submodel), 201)]
        public IActionResult PutSubmodelById(string submodelIdentifier, [FromBody] ISubmodel submodel)
        {
            return _smController.PutSubmodelById(submodelIdentifier, submodel);
        }

        /// <summary>
        /// Updates an existing Submodel
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodel">Submodel object</param>
        /// <returns></returns>
        /// <response code="204">Submodel updated successfully</response>
        /// <inheritdoc cref="SubmodelController.PatchSubmodel(ISubmodel)"/>
        [HttpPatch(SubmodelRepositoryRoutes.SUBMODEL_BYID, Name = "PatchSubmodel")]
        [Produces("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult PatchSubmodel(string submodelIdentifier, [FromBody] ISubmodel submodel)
        {
            return _smController.PatchSubmodel(submodelIdentifier, submodel);
        }


        /// <summary>
        /// Deletes an existing Submodel
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <returns></returns>
        /// <response code="200">Submodel deleted successfully</response>
        [HttpDelete(SubmodelRepositoryRoutes.SUBMODEL_BYID, Name = "DeleteSubmodelById")]
        [Produces("application/json")]
        [ProducesResponseType(204)]
        public IActionResult DeleteSubmodelById(string submodelIdentifier)
        {
            return _smController.DeleteSubmodelById(submodelIdentifier);
        }

        #region Submodel Interface

        /// <summary>
        /// Returns the metadata attributes of a specific Submodel
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (UTF8-BASE64-URL-encoded)</param>
        /// <returns></returns>
        /// <response code="200">Requested Submodel in the metadata representation</response>
        /// <inheritdoc cref="SubmodelController.GetSubmodelMetadata()"/>
        [HttpGet(SubmodelRepositoryRoutes.SUBMODEL_BYID + OutputModifier.METADATA, Name = "GetSubmodelById-Metadata")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Submodel), 200)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult SubmodelRepo_GetSubmodelMetadata(string submodelIdentifier)
        {
            return _smController.SubmodelRepo_GetSubmodelMetadata(submodelIdentifier);
        }

        /// <summary>
        /// Updates the metadata attributes of the Submodel
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodel">The metadata attributes of the Submodel object</param>
        /// <returns></returns>
        /// <response code="200">Requested Submodel</response>
        /// <inheritdoc cref="SubmodelController.PatchSubmodelMetadata(ISubmodel)"/> 
        [HttpPatch(SubmodelRepositoryRoutes.SUBMODEL_BYID + OutputModifier.METADATA, Name = "SubmodelRepo_PatchSubmodelMetadata")]
        [Produces("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult SubmodelRepo_PatchSubmodelMetadata(string submodelIdentifier, [FromBody] ISubmodel submodel)
        {
            return _smController.SubmodelRepo_PatchSubmodelMetadata(submodelIdentifier, submodel);
        }

        /// <summary>
        /// Returns a specific Submodel in the ValueOnly representation
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (UTF8-BASE64-URL-encoded)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <param name="extent">Determines to which extent the resource is being serialized</param>
        /// <returns></returns>
        /// <response code="200">ValueOnly representation of the requested Submodel</response>    
        /// <inheritdoc cref="SubmodelController.GetSubmodelValueOnly(RequestLevel, RequestExtent)"/>
        [HttpGet(SubmodelRepositoryRoutes.SUBMODEL_BYID + OutputModifier.VALUE, Name = "GetSubmodelById-ValueOnly")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Submodel), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 401)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult SubmodelRepo_GetSubmodelValue(string submodelIdentifier, [FromQuery] RequestLevel level = default, [FromQuery] RequestExtent extent = default)
        {
            return _smController.SubmodelRepo_GetSubmodelValue(submodelIdentifier, level, extent);
        }

        /// <summary>
        /// Updates the values of the Submodel
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="requestBody">Requested submodel element</param>
        /// <returns></returns>
        /// <response code="204">Submodel object in its ValueOnly representation</response>     
        /// <inheritdoc cref="SubmodelController.PatchSubmodelValueOnly(JsonDocument)"/>
        [HttpPatch(SubmodelRepositoryRoutes.SUBMODEL_BYID + OutputModifier.VALUE, Name = "SubmodelRepo_PatchSubmodelValueOnly")]
        [Produces("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult SubmodelRepo_PatchSubmodelValueOnly(string submodelIdentifier, [FromBody] JsonDocument requestBody)
        {
            return _smController.SubmodelRepo_PatchSubmodelValueOnly(submodelIdentifier, requestBody);
        }

        /// <summary>
        /// Returns the Reference of a specific Submodel
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (UTF8-BASE64-URL-encoded)</param>
        /// <returns></returns>
        /// <response code="200">Requested Submodel</response>    
        /// <inheritdoc cref="SubmodelController.GetSubmodelReference()"/>
        [HttpGet(SubmodelRepositoryRoutes.SUBMODEL_BYID + OutputModifier.REFERENCE, Name = "GetSubmodelById-Reference")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Submodel), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 401)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult SubmodelRepo_GetSubmodelReference(string submodelIdentifier)
        {
            return _smController.SubmodelRepo_GetSubmodelReference(submodelIdentifier);
        }

        /// <summary>
        /// Returns a specific Submodel in the Path notation
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (UTF8-BASE64-URL-encoded)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <returns></returns>
        /// <response code="200">Requested Submodel</response>
        /// <inheritdoc cref="SubmodelController.GetSubmodelPath(RequestLevel)"/>
        [HttpGet(SubmodelRepositoryRoutes.SUBMODEL_BYID + OutputModifier.PATH, Name = "GetSubmodelById-Path")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Submodel), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 401)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult SubmodelRepo_GetSubmodelPath(string submodelIdentifier, [FromQuery] RequestLevel level = default)
        {
            return _smController.SubmodelRepo_GetSubmodelPath(submodelIdentifier, level);
        }

        /// <summary>
        /// Returns all submodel elements including their hierarchy
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (UTF8-BASE64-URL-encoded)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <param name="extent">Determines to which extent the resource is being serialized</param>
        /// <param name="limit">The maximum number of elements in the response array</param>
        /// <param name="cursor">A server-generated identifier retrieved from pagingMetadata that specifies from which position the result listing should continue</param>
        /// <returns></returns>
        /// <response code="200">List of found submodel elements</response>
        /// <response code="404">Submodel not found</response>
        /// <inheritdoc cref="SubmodelController.GetAllSubmodelElements(int, string, RequestLevel, RequestExtent)"/>
        [HttpGet(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS, Name = "GetAllSubmodelElements_SubmodelRepository")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SubmodelElement[]), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult SubmodelRepo_GetAllSubmodelElements(string submodelIdentifier, [FromQuery] int limit = 100, [FromQuery] string cursor = "", [FromQuery] RequestLevel level = default, [FromQuery] RequestExtent extent = default)
        {
            return _smController.SubmodelRepo_GetAllSubmodelElements(submodelIdentifier, limit, cursor, level, extent);
        }

        /// <summary>
        /// Returns the metadata attributes of all submodel elements including their hierarchy
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (UTF8-BASE64-URL-encoded)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <param name="limit">The maximum number of elements in the response array</param>
        /// <param name="cursor">A server-generated identifier retrieved from pagingMetadata that specifies from which position the result listing should continue</param>
        /// <returns></returns>
        /// <response code="200">List of found submodel elements in the metadata representation</response>
        /// <inheritdoc cref="SubmodelController.GetAllSubmodelElementsMetadata(int, string, RequestLevel)"/>
        [HttpGet(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS + OutputModifier.METADATA, Name = "GetAllSubmodelElements-Metadata_SubmodelRepository")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SubmodelElement[]), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult SubmodelRepo_GetAllSubmodelElementsMetadata(string submodelIdentifier, [FromQuery] int limit = 100, [FromQuery] string cursor = "", [FromQuery] RequestLevel level = default)
        {
            return _smController.SubmodelRepo_GetAllSubmodelElementsMetadata(submodelIdentifier, limit, cursor, level);
        }

        /// <summary>
        /// Returns all submodel elements including their hierarchy in the Path notation
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (UTF8-BASE64-URL-encoded)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <param name="limit">The maximum number of elements in the response array</param>
        /// <param name="cursor">A server-generated identifier retrieved from pagingMetadata that specifies from which position the result listing should continue</param>
        /// <returns></returns>
        /// <response code="200">List of found submodel elements in the Path notation</response>
        /// <inheritdoc cref="SubmodelController.GetAllSubmodelElementsPath(int, string, RequestLevel)"/>
        [HttpGet(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS + OutputModifier.PATH, Name = "GetAllSubmodelElements-Path_SubmodelRepo")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SubmodelElement[]), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult SubmodelRepo_GetAllSubmodelElementsPath(string submodelIdentifier, [FromQuery] int limit = 100, [FromQuery] string cursor = "", [FromQuery] RequestLevel level = default)
        {
            return _smController.SubmodelRepo_GetAllSubmodelElementsPath(submodelIdentifier, limit, cursor, level);
        }

        /// <summary>
        /// Returns all submodel elements including their hierarchy in the ValueOnly representation
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (UTF8-BASE64-URL-encoded)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <param name="extent">Determines to which extent the resource is being serialized</param>
        /// <param name="limit">The maximum number of elements in the response array</param>
        /// <param name="cursor">A server-generated identifier retrieved from pagingMetadata that specifies from which position the result listing should continue</param>
        /// <returns></returns>
        /// <response code="200">List of found submodel elements in the ValueOnly representation</response>
        /// <inheritdoc cref="SubmodelController.GetAllSubmodelElementsValueOnly(int, string, RequestLevel, RequestExtent)"/>
        [HttpGet(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS + OutputModifier.VALUE, Name = "GetAllSubmodelElements-ValueOnly_SubmodelRepo")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagedResult), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult SubmodelRepo_GetAllSubmodelElementsValueOnly(string submodelIdentifier, [FromQuery] int limit = 100, [FromQuery] string cursor = "", [FromQuery] RequestLevel level = default, [FromQuery] RequestExtent extent = default)
        {
            return _smController.SubmodelRepo_GetAllSubmodelElementsValueOnly(submodelIdentifier, limit, cursor, level, extent);
        }

        /// <summary>
        /// Returns the References of all submodel elements
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (UTF8-BASE64-URL-encoded)</param>
        /// <param name="limit">The maximum number of elements in the response array</param>
        /// <param name="cursor">A server-generated identifier retrieved from pagingMetadata that specifies from which position the result listing should continue</param>
        /// <returns></returns>
        /// <response code="200">List of found submodel elements</response>
        /// <inheritdoc cref="SubmodelController.GetAllSubmodelElementsReference(int, string)"/>
        [HttpGet(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS + OutputModifier.REFERENCE, Name = "GetAllSubmodelElements-Reference_SubmodelRepo")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagedResult), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult SubmodelRepo_GetAllSubmodelElementsReference(string submodelIdentifier, [FromQuery] int limit = 100, [FromQuery] string cursor = "")
        {
            return _smController.SubmodelRepo_GetAllSubmodelElementsReference(submodelIdentifier, limit, cursor);
        }

        /// <summary>
        /// Creates a new submodel element
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="submodelElement">Requested submodel element</param>
        /// <returns></returns>
        /// <response code="201">Submodel element created successfully</response>
        /// <inheritdoc cref="SubmodelController.PostSubmodelElement(ISubmodelElement)"/>
        [HttpPost(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS, Name = "SubmodelRepo_PostSubmodelElement")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(SubmodelElement), 201)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult SubmodelRepo_PostSubmodelElement(string submodelIdentifier, [FromBody] ISubmodelElement submodelElement)
        {
            return _smController.SubmodelRepo_PostSubmodelElement(submodelIdentifier, submodelElement);
        }

        /// <summary>
        /// Returns a specific submodel element from the Submodel at a specified path
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (UTF8-BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <param name="extent">Determines to which extent the resource is being serialized</param>
        /// <returns></returns>
        /// <response code="200">Requested submodel element</response>  
        /// <inheritdoc cref="SubmodelController.GetSubmodelElementByPath(string, RequestLevel, RequestExtent)"/>
        [HttpGet(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH, Name = "GetSubmodelElementByPath_SubmodelRepo")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SubmodelElement), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 401)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult SubmodelRepo_GetSubmodelElementByPath(string submodelIdentifier, string idShortPath, [FromQuery] RequestLevel level = default, [FromQuery] RequestExtent extent = default)
        {
            return _smController.SubmodelRepo_GetSubmodelElementByPath(submodelIdentifier, idShortPath, level, extent);
        }

        /// <summary>
        /// Returns the metadata attributes of a specific submodel element from the Submodel at a specified path
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (UTF8-BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <returns></returns>
        /// <response code="200">Metadata attributes of the requested submodel element</response>
        /// <inheritdoc cref="SubmodelController.GetSubmodelElementByPathMetadata(string, RequestLevel)"/>
        [HttpGet(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH + OutputModifier.METADATA, Name = "GetSubmodelElementByPath-Metadata_SubmodelRepo")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SubmodelElement), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 401)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult SubmodelRepo_GetSubmodelElementByPathMetadata(string submodelIdentifier, string idShortPath, [FromQuery] RequestLevel level = default)
        {
            return _smController.SubmodelRepo_GetSubmodelElementByPathMetadata(submodelIdentifier, idShortPath, level);
        }

        /// <summary>
        /// Updates the metadata attributes an existing SubmodelElement
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated)</param>
        /// <param name="submodelElement">Metadata attributes of the SubmodelElement</param>
        /// <returns></returns>
        /// <response code="200">Requested submodel element in its ValueOnly representation</response> 
        /// <inheritdoc cref="SubmodelController.PatchSubmodelElementByPathMetadata(string, ISubmodelElement)"/> 
        [HttpPatch(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH + OutputModifier.METADATA,
            Name = "SubmodelRepo_PatchSubmodelElementByPathMetadata")]
        [Produces("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult SubmodelRepo_PatchSubmodelElementByPathMetadata(string submodelIdentifier, string idShortPath, [FromBody] ISubmodelElement submodelElement)
        {
            return _smController.SubmodelRepo_PatchSubmodelElementByPathMetadata(submodelIdentifier, idShortPath, submodelElement);
        }

        /// <summary>
        /// Returns a specific submodel element from the Submodel at a specified path in the ValueOnly representation
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (UTF8-BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <param name="extent">Determines to which extent the resource is being serialized</param>
        /// <returns></returns>
        /// <response code="200">Requested submodel element in its ValueOnly representation</response>
        /// <inheritdoc cref="SubmodelController.GetSubmodelElementByPathValueOnly(string, RequestLevel, RequestExtent)"/>
        [HttpGet(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH + OutputModifier.VALUE, Name = "GetSubmodelElementByPath-ValueOnly_SubmodelRepo")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SubmodelElement), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 401)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult SubmodelRepo_GetSubmodelElementByPathValueOnly(string submodelIdentifier, string idShortPath, [FromQuery] RequestLevel level = default, [FromQuery] RequestExtent extent = default)
        {
            return _smController.SubmodelRepo_GetSubmodelElementByPathValueOnly(submodelIdentifier, idShortPath, level, extent);
        }

        /// <summary>
        /// Returns a specific submodel element from the Submodel at a specified path in the Path notation
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (UTF8-BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated)</param>
        /// <param name="level">Determines the structural depth of the respective resource content</param>
        /// <returns></returns>
        /// <response code="200">Requested submodel element in the Path notation</response>
        /// <inheritdoc cref="SubmodelController.GetSubmodelElementByPathPath(string, RequestLevel)"/>
        [HttpGet(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH + OutputModifier.PATH, Name = "GetSubmodelElementByPath-Path_SubmodelRepo")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Reference), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 401)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult SubmodelRepo_GetSubmodelElementByPathPath(string submodelIdentifier, string idShortPath, [FromQuery] RequestLevel level = default)
        {
            return _smController.SubmodelRepo_GetSubmodelElementByPathPath(submodelIdentifier, idShortPath, level);
        }

        /// <summary>
        /// Returns the Reference of a specific submodel element from the Submodel at a specified path
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (UTF8-BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated)</param>
        /// <returns></returns>
        /// <response code="200">A Reference of the requested submodel element</response>
        /// /// <inheritdoc cref="SubmodelController.GetSubmodelElementByPathReference(string)"/>
        [HttpGet(SubmodelRoutes.SUBMODEL + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH + OutputModifier.REFERENCE, Name = "GetSubmodelElementByPath-Reference_SubmodelRepo")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Reference), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult SubmodelRepo_GetSubmodelElementByPathReference(string submodelIdentifier, string idShortPath)
        {
            return _smController.SubmodelRepo_GetSubmodelElementByPathReference(submodelIdentifier, idShortPath);
        }

        /// <summary>
        /// Creates a new submodel element at a specified path within submodel elements hierarchy
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated)</param>
        /// <param name="submodelElement">Requested submodel element</param>
        /// <returns></returns>
        /// <response code="201">Submodel element created successfully</response>
        /// <inheritdoc cref="SubmodelController.PostSubmodelElementByPath(string, ISubmodelElement)"/>
        [HttpPost(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH, Name = "SubmodelRepo_PostSubmodelElementByPath")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(SubmodelElement), 201)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult SubmodelRepo_PostSubmodelElementByPath(string submodelIdentifier, string idShortPath, [FromBody] ISubmodelElement submodelElement)
        {
            return _smController.SubmodelRepo_PostSubmodelElementByPath(submodelIdentifier, idShortPath, submodelElement);
        }

        /// <summary>
        /// Replaces an existing submodel element at a specified path within the submodel element hierarchy
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated)</param>
        /// <param name="requestBody">Requested submodel element</param>
        /// <returns></returns>
        /// <response code="204">Submodel element updated successfully</response>
        /// <inheritdoc cref="SubmodelController.PutSubmodelElementByPath(string, ISubmodelElement)"/>
        [HttpPut(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH, Name = "SubmodelRepo_PutSubmodelElementByPath")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(SubmodelElement), 201)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult SubmodelRepo_PutSubmodelElementByPath(string submodelIdentifier, string idShortPath, [FromBody] ISubmodelElement requestBody)
        {
            return _smController.SubmodelRepo_PutSubmodelElementByPath(submodelIdentifier, idShortPath, requestBody);
        }

        /// <summary>
        /// Updates an existing SubmodelElement
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated)</param>
        /// <param name="submodelElement">Requested submodel element</param>
        /// <returns></returns>
        /// <response code="204">Submodel element updated successfully</response>
        /// <inheritdoc cref="SubmodelController.PatchSubmodelElementByPath(string, ISubmodelElement)"/>
        [HttpPatch(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH,
            Name = "SubmodelRepo_PatchSubmodelElementByPath")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult SubmodelRepo_PatchSubmodelElementByPath(string submodelIdentifier, string idShortPath, [FromBody] ISubmodelElement submodelElement)
        {
            return _smController.SubmodelRepo_PatchSubmodelElementByPath(submodelIdentifier, idShortPath, submodelElement);
        }

        /// <summary>
        /// Deletes a submodel element at a specified path within the submodel elements hierarchy
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated)</param>
        /// <returns></returns>
        /// <response code="204">Submodel element deleted successfully</response>
        /// <inheritdoc cref="SubmodelController.DeleteSubmodelElementByPath(string)"/>
        [HttpDelete(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH, Name = "SubmodelRepo_DeleteSubmodelElementByPath")]
        [Produces("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult SubmodelRepo_DeleteSubmodelElementByPath(string submodelIdentifier, string idShortPath)
        {
            return _smController.SubmodelRepo_DeleteSubmodelElementByPath(submodelIdentifier, idShortPath);
        }

        /// <summary>
        /// Updates the value of an existing submodel element value at a specified path within submodel elements hierarchy
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated)</param>
        /// <param name="requestBody">Requested submodel element</param>
        /// <returns></returns>
        /// <response code="204">Submodel element updated successfully</response>
        /// <response code="400">Bad Request</response>
        /// <inheritdoc cref="SubmodelController.PatchSubmodelElementValueByPathValueOnly(string, JsonDocument)"/>
        [HttpPatch(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH + OutputModifier.VALUE, Name = "SubmodelRepo_PatchSubmodelElementValueByPathValueOnly")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(SubmodelElement), 201)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult SubmodelRepo_PatchSubmodelElementValueByPathValueOnly(string submodelIdentifier, string idShortPath, [FromBody] JsonDocument requestBody)
        {
            return _smController.SubmodelRepo_PatchSubmodelElementValueByPathValueOnly(submodelIdentifier, idShortPath, requestBody);
        }

        /// <summary>
        /// Downloads file content from a specific submodel element from the Submodel at a specified path
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated), in this case a file</param>
        /// <returns></returns>
        /// <response code="200">Requested file</response>
        /// <inheritdoc cref="SubmodelController.GetFileByPath(string)"/>
        [HttpGet(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH_ATTACHMENT, Name = "SubmodelRepo_GetFileByPath")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 405)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult SubmodelRepo_GetFileByPath(string submodelIdentifier, string idShortPath)
        {
            return _smController.SubmodelRepo_GetFileByPath(submodelIdentifier, idShortPath);
        }

        /// <inheritdoc cref="SubmodelController.PutFileByPath(string, IFormFile)"/>
        [HttpPost(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH_ATTACHMENT, Name = "SubmodelRepo_UploadFileContentByIdShort")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 404)]
        public async Task<IActionResult> SubmodelRepo_UploadFileContentByIdShort(string submodelIdentifier, string idShortPath, IFormFile file)
        {
            return await _smController.SubmodelRepo_UploadFileContentByIdShort(submodelIdentifier, idShortPath, file);
        }

        /// <summary>
        /// Uploads file content to an existing submodel element at a specified path within submodel elements hierarchy
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated), in this case a file</param>
        /// <param name="file">Content to upload</param>
        /// <returns></returns>
        /// <response code="200">Content uploaded successfully</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">File not found</response>
        /// <inheritdoc cref="SubmodelController.PutFileByPath(string, IFormFile)"/>
        [HttpPut(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH_ATTACHMENT, Name = "SubmodelRepo_PutFileByPath")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 404)]
        public async Task<IActionResult> SubmodelRepo_PutFileByPath(string submodelIdentifier, string idShortPath, IFormFile file)
        {
            return await _smController.SubmodelRepo_PutFileByPath(submodelIdentifier, idShortPath, file);
        }

        /// <summary>
        /// Deletes file content of an existing submodel element at a specified path within submodel elements hierarchy
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated), in this case a file</param>
        /// <returns></returns>
        /// <response code="200">File deleted successfully</response>
        /// <inheritdoc cref="SubmodelController.DeleteFileByPath(string)"/>
        [HttpDelete(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH_ATTACHMENT,
            Name = "SubmodelRepo_DeleteFileByPath")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult SubmodelRepo_DeleteFileByPath(string submodelIdentifier, string idShortPath)
        {
            return _smController.SubmodelRepo_DeleteFileByPath(submodelIdentifier, idShortPath);
        }

        /// <summary>
        /// Synchronously invokes an Operation at a specified path
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated), in this case an operation</param>
        /// <param name="operationRequest">Operation request object</param>
        /// <returns></returns>
        /// <response code="200">Operation invoked successfully</response>
        /// <inheritdoc cref="SubmodelController.InvokeOperationSync(string, InvocationRequest)"/>
        [HttpPost(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH_INVOKE, Name = "SubmodelRepo_InvokeOperationSync")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult SubmodelRepo_InvokeOperationSync(string submodelIdentifier, string idShortPath, [FromBody] InvocationRequest operationRequest)
        {
            return _smController.SubmodelRepo_InvokeOperationSync(submodelIdentifier, idShortPath, operationRequest);
        }

        /// <summary>
        /// Asynchronously invokes an Operation at a specified path
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated), in this case an operation</param>
        /// <param name="operationRequest">Operation request object</param>
        /// <returns></returns>
        /// <response code="200">Operation invoked successfully</response>
        /// <inheritdoc cref="SubmodelController.InvokeOperationAsync(string, InvocationRequest)"/>
        [HttpPost(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH_INVOKE_ASYNC, Name = "SubmodelRepo_InvokeOperationAsync")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult SubmodelRepo_InvokeOperationAsync(string submodelIdentifier, string idShortPath, [FromBody] InvocationRequest operationRequest)
        {
            return _smController.SubmodelRepo_InvokeOperationAsync(submodelIdentifier, idShortPath, operationRequest);
        }

        /// <summary>
        /// Synchronously invokes an Operation at a specified path
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated), in this case an operation</param>
        /// <param name="operationRequest">Operation request object</param>
        /// <returns></returns>
        /// <response code="200">Operation invoked successfully</response>
        /// <inheritdoc cref="SubmodelController.InvokeOperationSyncValueOnly(string, InvocationRequest)"/>
        [HttpPost(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH_INVOKE + OutputModifier.VALUE, Name = "SubmodelRepo_InvokeOperationSyncValueOnly")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(InvocationResponse), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 405)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult SubmodelRepo_InvokeOperationSyncValueOnly(string submodelIdentifier, string idShortPath, [FromBody] InvocationRequest operationRequest)
        {
            return _smController.SubmodelRepo_InvokeOperationSyncValueOnly(submodelIdentifier, idShortPath, operationRequest);
        }

        /// <summary>
        /// Asynchronously invokes an Operation at a specified path
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated), in this case an operation</param>
        /// <param name="operationRequest">Operation request object</param>
        /// <returns></returns>
        /// <response code="200">Operation invoked successfully</response>
        /// <inheritdoc cref="SubmodelController.InvokeOperationAsyncValueOnly(string, InvocationRequest)"/>
        [HttpPost(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH_INVOKE_ASYNC + OutputModifier.VALUE, Name = "SubmodelRepo_InvokeOperationAsyncValueOnly")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(InvocationResponse), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 405)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult SubmodelRepo_InvokeOperationAsyncValueOnly(string submodelIdentifier, string idShortPath, [FromBody] InvocationRequest operationRequest)
        {
            return _smController.SubmodelRepo_InvokeOperationAsyncValueOnly(submodelIdentifier, idShortPath, operationRequest);
        }

        /// <summary>
        /// Returns the Operation status of an asynchronous invoked Operation
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated), in this case an operation</param>
        /// <param name="handleId">The returned handle id of an operation’s asynchronous invocation used to request the current state of the operation’s execution (BASE64-URL-encoded)</param>
        /// <returns></returns>
        /// <response code="200">Operation result object</response>
        /// <inheritdoc cref="SubmodelController.GetOperationAsyncStatus(string, string)"/>
        [HttpGet(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH_OPERATION_STATUS, Name = "SubmodelRepo_GetOperationAsyncStatus")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Result), 302)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult SubmodelRepo_GetOperationAsyncStatus(string submodelIdentifier, string idShortPath, string handleId)
        {
            return _smController.SubmodelRepo_GetOperationAsyncStatus(submodelIdentifier, idShortPath, handleId);
        }

        /// <summary>
        /// Returns the Operation result of an asynchronous invoked Operation
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated), in this case an operation</param>
        /// <param name="handleId">The returned handle id of an operation’s asynchronous invocation used to request the current state of the operation’s execution (BASE64-URL-encoded)</param>
        /// <returns></returns>
        /// <response code="200">Operation result object</response>
        /// <inheritdoc cref="SubmodelController.GetOperationAsyncResult(string, string)"/>
        [HttpGet(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH_OPERATION_RESULTS, Name = "SubmodelRepo_GetOperationAsyncResult")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(InvocationResponse), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 404)]
        public IActionResult SubmodelRepo_GetOperationAsyncResult(string submodelIdentifier, string idShortPath, string handleId)
        {
            return _smController.SubmodelRepo_GetOperationAsyncResult(submodelIdentifier, idShortPath, handleId);
        }

        /// <summary>
        /// Returns the Operation result of an asynchronous invoked Operation
        /// </summary>
        /// <param name="submodelIdentifier">The Submodel’s unique id (BASE64-URL-encoded)</param>
        /// <param name="idShortPath">IdShort path to the submodel element (dot-separated), in this case an operation</param>
        /// <param name="handleId">The returned handle id of an operation’s asynchronous invocation used to request the current state of the operation’s execution (BASE64-URL-encoded)</param>
        /// <returns></returns>
        /// <response code="200">Operation result object</response>
        /// <inheritdoc cref="SubmodelController.GetOperationAsyncResultValueOnly(string, string)"/>
        [HttpGet(SubmodelRepositoryRoutes.SUBMODEL_BYID + SubmodelRoutes.SUBMODEL_ELEMENTS_IDSHORTPATH_OPERATION_RESULTS + OutputModifier.VALUE, Name = "SubmodelRepo_GetOperationAsyncResultValueOnly")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(InvocationResponse), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 403)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult SubmodelRepo_GetOperationAsyncResultValueOnly(string submodelIdentifier, string idShortPath, string handleId)
        {
            return _smController.SubmodelRepo_GetOperationAsyncResultValueOnly(submodelIdentifier, idShortPath, handleId);
        }


        /// <summary>
        /// Returns root Messages of the repository server
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Operation result object</response>
        [HttpGet("/", Name = "GetRoot")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(InvocationResponse), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        public IActionResult GetRoot(string submodelIdentifier, string idShortPath, string handleId)
        {
            var version = "1.0";
            return Json(new { 
                message = $"BaSyx dotNet Repo Server v{version}",
                status_code = 200,
            });
        }

        #endregion     
    }
}
