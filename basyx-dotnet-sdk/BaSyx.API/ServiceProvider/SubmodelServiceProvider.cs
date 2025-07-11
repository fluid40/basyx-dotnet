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
using BaSyx.Models.AdminShell;
using BaSyx.Utils.ResultHandling;
using BaSyx.Utils.Client;
using System.Collections.Generic;
using System;
using BaSyx.Models.Connectivity;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using BaSyx.Models.Extensions;
using System.Text.Json;
using BaSyx.Utils.ResultHandling.ResultTypes;
using BaSyx.Utils.ResultHandling.http;
using System.Linq;
using BaSyx.Utils.DependencyInjection;
using Range = BaSyx.Models.AdminShell.Range;
using Microsoft.Extensions.DependencyInjection;

namespace BaSyx.API.ServiceProvider
{
    /// <summary>
    /// Reference implementation of ISubmodelServiceProvider interface
    /// </summary>
    public class SubmodelServiceProvider : ISubmodelServiceProvider
    {
        private static readonly ILogger logger = LoggingExtentions.CreateLogger<SubmodelServiceProvider>();

        private ISubmodel _submodel;

        public const int DEFAULT_TIMEOUT = 60000;
        public ISubmodelDescriptor ServiceDescriptor { get; internal set; }

        private readonly Dictionary<string, Action<ValueScope>> updateFunctions;
        private readonly Dictionary<string, EventDelegate> eventDelegates;
        private readonly Dictionary<string, InvocationResponse> invocationResults;

        private IMessageClient messageClient;
        private JsonSerializerOptions _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        /// <summary>
        /// Constructor for SubmodelServiceProvider
        /// </summary>
        public SubmodelServiceProvider()
        {
            updateFunctions = new Dictionary<string, Action<ValueScope>>();
            eventDelegates = new Dictionary<string, EventDelegate>();
            invocationResults = new Dictionary<string, InvocationResponse>();
        }
        /// <summary>
        /// Contructor for SubmodelServiceProvider with a Submodel object to bind to
        /// </summary>
        /// <param name="submodel">Submodel object</param>
        public SubmodelServiceProvider(ISubmodel submodel) : this()
        {
            BindTo(submodel);
        }
        /// <summary>
        /// Contructor for SubmodelServiceProvider with a Submodel object to bind to and a SubmodelDescriptor as ServiceDescriptor
        /// </summary>
        /// <param name="submodel">Submodel object</param>
        /// <param name="submodelDescriptor">SubmodelDescriptor object</param>
        public SubmodelServiceProvider(ISubmodel submodel, ISubmodelDescriptor submodelDescriptor) : this()
        {
            _submodel = submodel;
            ServiceDescriptor = submodelDescriptor;
        }

        /// <summary>
        /// Bind this SubmodelServiceProvider to a specific Submodel
        /// </summary>
        /// <param name="submodel">Submodel object</param>
        public void BindTo(ISubmodel submodel)
        {
            _submodel = submodel;
            ServiceDescriptor = new SubmodelDescriptor(submodel, null);
        }

        /// <summary>
        /// Returns the model binding of this SubmodelServiceProvider
        /// </summary>
        /// <returns>Submodel object</returns>
        public ISubmodel GetBinding()
        {
            return _submodel;
        }

        public void UseInMemorySubmodelElementHandler()
        {
            UseInMemorySubmodelElementHandlerInternal(_submodel.SubmodelElements);
        }
        private void UseInMemorySubmodelElementHandlerInternal(IElementContainer<ISubmodelElement> submodelElements)
        {
            if (submodelElements.HasChildren())
            {
                foreach (var child in submodelElements.Children)
                {
                    UseInMemorySubmodelElementHandlerInternal(child);
                }
            }
            if(submodelElements.Value != null)
            {
                if (submodelElements.Value.ModelType != ModelType.Operation)
                    RegisterSubmodelElementHandler(submodelElements.Path, new SubmodelElementHandler(submodelElements.Value.Get, submodelElements.Value.Set));                
            }
        }

        /// <summary>
        /// Use as specific SubmodelElementHandler for all SubmodelElements
        /// </summary>
        /// <param name="elementHandler">SubmodelElementHandler</param>
        public void UseSubmodelElementHandler(SubmodelElementHandler elementHandler)
        {
            UseSubmodelElementHandlerInternal(_submodel.SubmodelElements, elementHandler, null);
        }
        /// <summary>
        /// Use a specific SubmodelElementHandler for all SubmodelElements of a specific ModelType (e.g. Property) except Operations
        /// </summary>
        /// <param name="elementHandler">SubmodelElementHandler</param>
        /// <param name="modelType">ModelType</param>
        public void UseSubmodelElementHandlerForModelType(SubmodelElementHandler elementHandler, ModelType modelType)
        {
            UseSubmodelElementHandlerInternal(_submodel.SubmodelElements, elementHandler, modelType);
        }

        private void UseSubmodelElementHandlerInternal(IElementContainer<ISubmodelElement> submodelElements, SubmodelElementHandler elementHandler, ModelType modelType = null)
        {
            if (submodelElements.HasChildren())
            {
                foreach (var child in submodelElements.Children)
                {
                    UseSubmodelElementHandlerInternal(child, elementHandler, modelType);
                }
            }
            if (submodelElements.Value != null)
            {
                if (modelType == null)
                    RegisterSubmodelElementHandler(submodelElements.Path, elementHandler);
                else if (submodelElements.Value.ModelType == modelType)
                    RegisterSubmodelElementHandler(submodelElements.Path, elementHandler);
                else
                    return;
            }
        }
        /// <summary>
        /// Use a specific MethodCalledHandler for all Operations
        /// </summary>
        /// <param name="methodCalledHandler"></param>
        public void UseOperationHandler(MethodCalledHandler methodCalledHandler)
        {
            UseOperationHandlerInternal(_submodel.SubmodelElements, methodCalledHandler);
        }

        private void UseOperationHandlerInternal(IElementContainer<ISubmodelElement> submodelElements, MethodCalledHandler methodCalledHandler)
        {
            if (submodelElements.HasChildren())
            {
                foreach (var child in submodelElements.Children)
                {
                    UseOperationHandlerInternal(child, methodCalledHandler);
                }
            }
            else
            {
                if (submodelElements.Value is IOperation)
                    RegisterMethodCalledHandler(submodelElements.Path, methodCalledHandler);
                else
                    return;
            }
        }
              

        public void RegisterSubmodelElementHandler(string idShortPath, SubmodelElementHandler elementHandler)
        {
            if (_submodel == null)
                throw new Exception(new Result(false, new NotFoundMessage("Submodel")).ToString());

            var submodelElement = _submodel.SubmodelElements.Retrieve<SubmodelElement>(idShortPath);
            if (!submodelElement.Success || submodelElement.Entity == null)
                throw new Exception(new Result(false, new NotFoundMessage($"SubmodelElement {idShortPath}")).ToString());

            submodelElement.Entity.Get = elementHandler.GetValueHandler;
            submodelElement.Entity.Set = elementHandler.SetValueHandler;
        }

        public void RegisterMethodCalledHandler(string pathToOperation, MethodCalledHandler handler)
        {
            if (_submodel == null)
                throw new Exception(new Result(false, new NotFoundMessage("Submodel")).ToString());

            var submodelElement = _submodel.SubmodelElements.Retrieve<SubmodelElement>(pathToOperation);
            if (!submodelElement.Success || submodelElement.Entity == null)
                throw new Exception(new Result(false, new NotFoundMessage($"SubmodelElement {pathToOperation}")).ToString());

            submodelElement.Entity.Cast<Operation>().OnMethodCalled = handler;
        }

        public void RegisterEventDelegate(string pathToEvent, EventDelegate eventDelegate)
        {
            if (!eventDelegates.ContainsKey(pathToEvent))
                eventDelegates.Add(pathToEvent, eventDelegate);
            else
                eventDelegates[pathToEvent] = eventDelegate;
        }

        public IResult<InvocationResponse> InvokeOperation(string pathToOperation, InvocationRequest invocationRequest, bool async)
        {
           if(async)
                return InvokeOperationServerSide(pathToOperation, invocationRequest);
           else
                return InvokeOperation(pathToOperation, invocationRequest);
        }

        public IResult<InvocationResponse> InvokeOperation(string pathToOperation, InvocationRequest invocationRequest)
        {
            if (_submodel == null)
                return new Result<InvocationResponse>(false, new NotFoundMessage("Submodel"));

            var operation_Retrieved = _submodel.SubmodelElements.Retrieve<IOperation>(pathToOperation);
            if (operation_Retrieved.Success && operation_Retrieved.Entity != null)
            {
                MethodCalledHandler methodHandler;
                if (operation_Retrieved.Entity.OnMethodCalled != null)
                    methodHandler = operation_Retrieved.Entity.OnMethodCalled;
                else
                    return new Result<InvocationResponse>(false, new NotFoundMessage($"MethodHandler for {pathToOperation}"));

                IResult<IOperationVariableSet> checkedInputArguments = CheckInputArguments(operation_Retrieved.Entity, invocationRequest.InputArguments);
                if(!checkedInputArguments.Success)
                    return new Result<InvocationResponse>(checkedInputArguments);

                InvocationResponse invocationResponse = new InvocationResponse(invocationRequest.RequestId, false);
                invocationResponse.InOutputArguments = invocationRequest.InOutputArguments;
                invocationResponse.OutputArguments = CreateOutputArguments(operation_Retrieved.Entity.OutputVariables);

                int timeout = DEFAULT_TIMEOUT;
                if (invocationRequest.Timeout.HasValue)
                    timeout = invocationRequest.Timeout.Value;

                using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
                {
                    Task<OperationResult> runner = Task.Run(async () =>
                    {
                        try
                        {
                            invocationResponse.ExecutionState = ExecutionState.Running;
                            return await methodHandler
                            .Invoke(operation_Retrieved.Entity, checkedInputArguments.Entity, invocationResponse.InOutputArguments, invocationResponse.OutputArguments, cancellationTokenSource.Token)
                            .ConfigureAwait(false);                                                        
                        }
                        catch (Exception e)
                        {
                            invocationResponse.ExecutionState = ExecutionState.Failed;
                            return new OperationResult(e);
                        }

                    }, cancellationTokenSource.Token);

                    if (Task.WhenAny(runner, Task.Delay(timeout, cancellationTokenSource.Token)).Result == runner)
                    {
                        cancellationTokenSource.Cancel();

                        invocationResponse.Success = runner.Result.Success;
                        invocationResponse.Messages = runner.Result.Messages;
                        if (invocationResponse.ExecutionState != ExecutionState.Failed)
                            invocationResponse.ExecutionState = ExecutionState.Completed;

                        return new Result<InvocationResponse>(true, invocationResponse);
                    }
                    else
                    {
                        cancellationTokenSource.Cancel();
                        invocationResponse.Success = false;
                        invocationResponse.Messages.Add(new TimeoutMessage());                        
                        invocationResponse.ExecutionState = ExecutionState.Timeout;
                        return new Result<InvocationResponse>(true, invocationResponse);
                    }
                }
            }
            return new Result<InvocationResponse>(operation_Retrieved);
        }

        private IResult<IOperationVariableSet> CheckInputArguments(IOperation operation, IOperationVariableSet inputArguments)
        {
            try
            {
                if(inputArguments == null)
                {
                    if(operation.InputVariables.Count == 0)
                        return new Result<IOperationVariableSet>(true, new OperationVariableSet());
                    else
                        return new Result<IOperationVariableSet>(false, new ErrorMessage("InputArgument are null but there are expected input variables"));
                }

                IOperationVariableSet checkedInputArguments = new OperationVariableSet();
                foreach (var inputArgument in inputArguments)
                {
                    //Check whether input argument exists in the operation input variable definition
                    var inputVariable = operation.InputVariables.Get(inputArgument.Value.IdShort);
                    if (inputVariable == null)
                        return new Result<IOperationVariableSet>(false, new NotFoundMessage($"InputVariable {inputArgument.Value.IdShort}"));

                    //Check whether modeltype matches
                    if(inputArgument.Value.ModelType != inputVariable.ModelType)
                        return new Result<IOperationVariableSet>(false, new ErrorMessage($"ModelType of InputArgument {inputArgument.Value.ModelType} does not match with InputVariable definition {inputVariable.ModelType}"));

                    //Special handling for properties
                    if(inputArgument.Value is IProperty inArgProperty && inputVariable is IProperty inVariableProperty)
                    {
                        //Check if value types match, if not try to convert incoming value to expected value
                        if (inArgProperty.ValueType == inVariableProperty.ValueType)
                        {
                            checkedInputArguments.Add(inArgProperty);
                        }                            
                        else
                        {
                            object value = inArgProperty.Value.Value.Value;
                            Property newInArg = new Property(inVariableProperty.IdShort, inVariableProperty.ValueType, value);
                            checkedInputArguments.Add(newInArg);
                        }
                    }
                    else
                    {
                        checkedInputArguments.Add(inputArgument);
                    }
                }
                return new Result<IOperationVariableSet>(true, checkedInputArguments);
            }
            catch (Exception e)
            {
                return new Result<IOperationVariableSet>(e);
            }            
        }

        private IOperationVariableSet CreateOutputArguments(IOperationVariableSet outputVariables)
        {
            if (outputVariables == null)
                return null;

            OperationVariableSet variables = new OperationVariableSet();
            if(outputVariables.Count > 0)
            {
                foreach (var outputVariable in outputVariables)
                {
                    DataType dataType;
                    if (outputVariable.Value is IProperty property)
                        dataType = property.ValueType;
                    else if (outputVariable.Value is IRange range)
                        dataType = range.ValueType;
                    else
                        dataType = null;

                    var se = SubmodelElementFactory.CreateSubmodelElement(outputVariable.Value.IdShort, outputVariable.Value.ModelType, dataType);
                    se.Description = outputVariable.Value.Description;
                    se.DisplayName = outputVariable.Value.DisplayName;
                    se.SemanticId = outputVariable.Value.SemanticId;
                    se.SupplementalSemanticIds = outputVariable.Value.SupplementalSemanticIds;
                    se.Category = outputVariable.Value.Category;
                    se.Kind = outputVariable.Value.Kind;
                    se.EmbeddedDataSpecifications = outputVariable.Value.EmbeddedDataSpecifications;
                    se.Qualifiers = outputVariable.Value.Qualifiers;
                    variables.Add(se);
                }
            }
            return variables;
        }

        public IResult<InvocationResponse> InvokeOperationServerSide(string idShortPath, InvocationRequest invocationRequest)
        {
            if (_submodel == null)
                return new Result<InvocationResponse>(false, new NotFoundMessage("Submodel"));
            if (invocationRequest == null)
                return new Result<InvocationResponse>(new ArgumentNullException(nameof(invocationRequest)));

            var operation_Retrieved = _submodel.SubmodelElements.Retrieve<IOperation>(idShortPath);
            if (operation_Retrieved.Success && operation_Retrieved.Entity != null)
            {
                MethodCalledHandler methodHandler;
                if (operation_Retrieved.Entity.OnMethodCalled != null)
                    methodHandler = operation_Retrieved.Entity.OnMethodCalled;
                else
                    return new Result<InvocationResponse>(false, new NotFoundMessage($"MethodHandler for {idShortPath}"));

                IResult<IOperationVariableSet> checkedInputArguments = CheckInputArguments(operation_Retrieved.Entity, invocationRequest.InputArguments);
                if (!checkedInputArguments.Success)
                    return new Result<InvocationResponse>(checkedInputArguments);

                Task invocationTask = Task.Run(async() =>
                {
                    InvocationResponse invocationResponse = new InvocationResponse(invocationRequest.RequestId, false);
                    invocationResponse.InOutputArguments = invocationRequest.InOutputArguments;
                    invocationResponse.OutputArguments = CreateOutputArguments(operation_Retrieved.Entity.OutputVariables);
                    SetInvocationResult(idShortPath, invocationRequest.RequestId, ref invocationResponse);

                    int timeout = DEFAULT_TIMEOUT;
                    if (invocationRequest.Timeout.HasValue)
                        timeout = invocationRequest.Timeout.Value;

                    using (var cancellationTokenSource = new CancellationTokenSource())
                    {
                        Task<OperationResult> runner = Task.Run(async () =>
                        {
                            try
                            {
                                invocationResponse.ExecutionState = ExecutionState.Running;
                                return await methodHandler
                                .Invoke(operation_Retrieved.Entity, checkedInputArguments.Entity, invocationResponse.InOutputArguments, invocationResponse.OutputArguments, cancellationTokenSource.Token)
                                .ConfigureAwait(false);                                                              
                            }
                            catch (Exception e)
                            {
                                invocationResponse.ExecutionState = ExecutionState.Failed;
                                return new OperationResult(e);
                            }
                        }, cancellationTokenSource.Token);

                        if (await Task.WhenAny(runner, Task.Delay(timeout, cancellationTokenSource.Token)).ConfigureAwait(false) == runner)
                        {
                            cancellationTokenSource.Cancel();
                            invocationResponse.Success = runner.Result.Success;
                            invocationResponse.Messages = runner.Result.Messages;
                            if (invocationResponse.ExecutionState != ExecutionState.Failed)
                                invocationResponse.ExecutionState = ExecutionState.Completed;
                        }
                        else
                        {
                            cancellationTokenSource.Cancel();
                            invocationResponse.Success = false;
                            invocationResponse.Messages.Add(new TimeoutMessage());
                            invocationResponse.ExecutionState = ExecutionState.Timeout;
                        }
                    }
                });

                InvocationResponse callbackResponse = new InvocationResponse(invocationRequest.RequestId, true);
                return new Result<InvocationResponse>(true, callbackResponse);
            }
            return new Result<InvocationResponse>(operation_Retrieved);
        }

        private void SetInvocationResult(string operationId, string requestId, ref InvocationResponse invocationResponse)
        {
            string key = string.Join("_", operationId, requestId);
            if (invocationResults.ContainsKey(key))
            {
                invocationResults[key] = invocationResponse;
            }
            else
            {
                invocationResults.Add(key, invocationResponse);
            }
        }
      
        public IResult<InvocationResponse> GetInvocationResult(string pathToOperation, string requestId)
        {
            string key = string.Join("_", pathToOperation, requestId);
            if (invocationResults.ContainsKey(key))
            {
                return new Result<InvocationResponse>(true, invocationResults[key]);
            }
            else
            {
               return new Result<InvocationResponse>(false, new NotFoundMessage($"Request with id {requestId}"));
            }
        }

        public async Task<IResult> PublishEventAsync(IEventPayload eventMessage)
        {
            if (messageClient == null || !messageClient.IsConnected)
                return new Result(false, new Message(MessageType.Warning, "MessageClient is not initialized or not connected"));

            if (eventMessage == null)
                return new Result(new ArgumentNullException(nameof(eventMessage)));

            try
            {
                string message = JsonSerializer.Serialize(eventMessage, _jsonOptions);
                return await messageClient.PublishAsync(eventMessage.Topic, message).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error sending event message");
                return new Result(e);
            }
        }

        public IResult PublishEvent(IEventPayload eventMessage) => PublishEventAsync(eventMessage).Result;        
       
        public virtual void ConfigureEventHandler(IMessageClient messageClient)
        {
            this.messageClient = messageClient;
        }

        public IResult<ISubmodelElement> CreateSubmodelElement(string idShortPath, ISubmodelElement submodelElement)
        {
            if (_submodel == null)
                return new Result<ISubmodelElement>(false, new NotFoundMessage("Submodel"));

            var created = _submodel.SubmodelElements.Create(idShortPath, submodelElement);
            return created;
        }

        public IResult UpdateSubmodelElement(string idShortPath, ISubmodelElement submodelElement)
        {
            if (_submodel == null)
                return new Result(false, new NotFoundMessage("Submodel"));

            if (submodelElement == null)
                return new Result(false, new NotFoundMessage("submodelElement"));

            var updated = _submodel.SubmodelElements.Update(idShortPath, submodelElement);
            return updated;
        }

        public IResult UpdateSubmodelElementByPath(string idShortPath, ISubmodelElement submodelElement)
        {
            if (_submodel == null)
                return new Result(false, new NotFoundMessage("Submodel"));

            if (submodelElement == null)
                return new Result(false, new NotFoundMessage("submodelElement"));

            var retrievedSme = RetrieveSubmodelElement(idShortPath);
            if (!retrievedSme.Success || retrievedSme.Entity == null)
                return new Result(false, new NotFoundMessage($"SubmodelElement {idShortPath}"));

            var sme = retrievedSme.Entity as SubmodelElement;

            // update value if set
            if (submodelElement.Value != null)
            {
                var valueResult = UpdateSubmodelElementValue(sme, submodelElement.Value);
                if (!valueResult.Success)
                    return valueResult;
            }

            // update metadata
            var metadataResult = UpdateSubmodelElementMetadata(sme, submodelElement);
            return metadataResult;
        }

        public IResult UpdateSubmodelElementMetadata(string idShortPath, ISubmodelElement submodelElement)
        {
            if (_submodel == null)
                return new Result(false, new NotFoundMessage("Submodel"));

            if (submodelElement == null)
                return new Result(false, new NotFoundMessage("submodelElement"));

            var retrievedSme = RetrieveSubmodelElement(idShortPath);
            if (!retrievedSme.Success || retrievedSme.Entity == null)
                return new Result(false, new NotFoundMessage($"SubmodelElement {idShortPath}"));

            var sme = retrievedSme.Entity as SubmodelElement;

            return UpdateSubmodelElementMetadata(sme, submodelElement);
        }

        public IResult UpdateSubmodelElementMetadata(SubmodelElement sme, ISubmodelElement submodelElement)
        {
            if (sme.ModelType != submodelElement.ModelType)
                return new Result(false, new ErrorMessage("Model type not matching for found submodel element and request body"));

            sme.Category = submodelElement.Category ?? sme.Category;

            // default init as empty list
            if (submodelElement.Description.Count > 0)
                sme.Description = submodelElement.Description;

            // default init as empty list
            if (submodelElement.DisplayName.Count > 0)
                sme.DisplayName = submodelElement.DisplayName;
            
            sme.SemanticId = submodelElement.SemanticId ?? sme.SemanticId;
            
            // default init as empty list
            if (submodelElement.SupplementalSemanticIds.Any())
                sme.SupplementalSemanticIds = submodelElement.SupplementalSemanticIds;

            // default init as empty list
            if (submodelElement.Qualifiers.Any())
                sme.Qualifiers = submodelElement.Qualifiers;

            switch (sme.ModelType.Type)
            {
                case ModelTypes.Property:
                    var smeProperty = (Property)sme;
                    var submodelElementProperty = (Property)submodelElement;

                    smeProperty.ValueId = submodelElementProperty.ValueId ?? smeProperty.ValueId;
                    smeProperty.ValueType = submodelElementProperty.ValueType ?? smeProperty.ValueType;
                    break;

                case ModelTypes.BasicEventElement:
                    var smeBasicEvent = (BasicEventElement)sme;
                    var submodelElementBasicEvent = (BasicEventElement)submodelElement;
                    smeBasicEvent.ObservableReference = submodelElementBasicEvent.ObservableReference ?? smeBasicEvent.ObservableReference;

                    if (submodelElementBasicEvent.Direction != EventDirection.None)
                        smeBasicEvent.Direction = submodelElementBasicEvent.Direction;

                    if (submodelElementBasicEvent.State != EventState.None)
                        smeBasicEvent.State = submodelElementBasicEvent.State;

                    smeBasicEvent.MessageTopic = submodelElementBasicEvent.MessageTopic ?? smeBasicEvent.MessageTopic;
                    smeBasicEvent.MessageBroker = submodelElementBasicEvent.MessageBroker ?? smeBasicEvent.MessageBroker;
                    smeBasicEvent.LastUpdate = submodelElementBasicEvent.LastUpdate ?? smeBasicEvent.LastUpdate;
                    smeBasicEvent.MinInterval = submodelElementBasicEvent.MinInterval ?? smeBasicEvent.MinInterval;
                    smeBasicEvent.MaxInterval = submodelElementBasicEvent.MaxInterval ?? smeBasicEvent.MaxInterval;
                    
                    break;

                case ModelTypes.Entity:
                    var smeEntity = (Entity)sme;
                    var submodelElementEntity = (Entity)submodelElement;

                    if (submodelElementEntity.EntityType != EntityType.None)
                        smeEntity.EntityType = submodelElementEntity.EntityType;

                    break;

                case ModelTypes.MultiLanguageProperty:
                    var smeMultiLang = (MultiLanguageProperty)sme;
                    var submodelElementMultiLang = (MultiLanguageProperty)submodelElement;

                    smeMultiLang.ValueId = submodelElementMultiLang.ValueId ?? smeMultiLang.ValueId;

                    break;

                case ModelTypes.Operation:
                    var smeOperation = (Operation)sme;
                    var submodelElementOperation = (Operation)submodelElement;

                    smeOperation.InputVariables = submodelElementOperation.InputVariables ?? smeOperation.InputVariables;
                    smeOperation.InOutputVariables = submodelElementOperation.InOutputVariables ?? smeOperation.InOutputVariables;
                    smeOperation.OutputVariables = submodelElementOperation.OutputVariables ?? smeOperation.OutputVariables;

                    break;

                case ModelTypes.Range:
                    var smeRange = (Range)sme;
                    var submodelElementRange = (Range)submodelElement;

                    smeRange.ValueId = submodelElementRange.ValueId ?? smeRange.ValueId;
                    smeRange.ValueType = submodelElementRange.ValueType ?? smeRange.ValueType;

                    break;

                case ModelTypes.SubmodelElementList:
                    var smeList = (SubmodelElementList)sme;
                    var submodelElementList = (SubmodelElementList)submodelElement;

                    smeList.SemanticIdListElement =
                        submodelElementList.SemanticIdListElement ?? smeList.SemanticIdListElement;
                    smeList.TypeValueListElement = submodelElementList.TypeValueListElement ?? smeList.TypeValueListElement;

                    break;
            }


            return new Result(true);
        }

        public IResult<ISubmodelElement> CreateOrUpdateSubmodelElement(string idShortPath, ISubmodelElement submodelElement, SubmodelElementHandler submodelElementHandler)
        {
            if (_submodel == null)
                return new Result<ISubmodelElement>(false, new NotFoundMessage("Submodel"));

            var created = _submodel.SubmodelElements.CreateOrUpdate(idShortPath, submodelElement);
            return created;
        }

        public IResult<PagedResult<IElementContainer<ISubmodelElement>>> RetrieveSubmodelElements(int limit = 100, string cursor = "")
        {
            if (_submodel == null)
                return new Result<PagedResult<IElementContainer<ISubmodelElement>>>(false, new NotFoundMessage("Submodel"));

            if (_submodel.SubmodelElements == null)
                return new Result<PagedResult<IElementContainer<ISubmodelElement>>>(false, new NotFoundMessage("SubmodelElements"));

            var retrieved = _submodel.SubmodelElements.RetrieveAll();
            if (!retrieved.Success || retrieved.Entity == null)
                return new Result<PagedResult<IElementContainer<ISubmodelElement>>>(retrieved.Success, null, retrieved.Messages);

            var smeDict = retrieved.Entity.ToDictionary(sme => sme.IdShort, sme => sme);

            // create the paged data
            var paginationHelper = new PaginationHelper<ISubmodelElement>(smeDict, elem => elem.IdShort);
            var pagingMetadata = new PagingMetadata(cursor);
            var pagedResult = paginationHelper.GetPaged(limit, pagingMetadata);

            var smcPaged = new ElementContainer<ISubmodelElement>();
            smcPaged.AddRange(pagedResult.Result as IEnumerable<ISubmodelElement>);
            var paginatedSmc = new PagedResult<IElementContainer<ISubmodelElement>>(smcPaged, pagedResult.PagingMetadata);

            return new Result<PagedResult<IElementContainer<ISubmodelElement>>>(retrieved.Success, paginatedSmc, retrieved.Messages); 
        }

        public IResult<ISubmodelElement> RetrieveSubmodelElement(string submodelElementId)
        {
            if (_submodel == null)
                return new Result<ISubmodelElement>(false, new NotFoundMessage("Submodel"));

            if (_submodel.SubmodelElements == null)
                return new Result<ISubmodelElement>(false, new NotFoundMessage(submodelElementId));

            return _submodel.SubmodelElements.Retrieve(submodelElementId);
        }

        public IResult<ValueScope> RetrieveSubmodelElementValue(string submodelElementId)
        {
            if (_submodel == null)
                return new Result<ValueScope>(false, new NotFoundMessage("Submodel"));

            var submodelElement = _submodel.SubmodelElements.Retrieve(submodelElementId)?.Entity;
            if (submodelElement == null)
                return new Result<ValueScope>(false, new NotFoundMessage($"SubmodelElement {submodelElementId}"));

            if (submodelElement.Get != null)
                return new Result<ValueScope>(true, submodelElement.Get.Invoke(submodelElement).Result);
            else
                return new Result<ValueScope>(false, new NotFoundMessage($"SubmodelElementHandler for {submodelElementId}"));

            //var submodelElement = _submodel.SubmodelElements.Retrieve<ISubmodelElement>(submodelElementId);
            //if (!submodelElement.Success || submodelElement.Entity == null)
            //    return new Result<ValueScope>(false, new NotFoundMessage($"SubmodelElement {submodelElementId}"));

            //if (submodelElement.Entity.Get != null)
            //    return new Result<ValueScope>(true, submodelElement.Entity.Get.Invoke(submodelElement.Entity).Result);
            //else
            //    return new Result<ValueScope>(false, new NotFoundMessage($"SubmodelElementHandler for {submodelElementId}"));
        }

        public IResult<IReference> RetrieveSubmodelElementReference(string idShortPath)
        {
            if (_submodel == null)
                return new Result<IReference>(false, new NotFoundMessage("Submodel"));

            var submodelElement = _submodel.SubmodelElements.GetChild(idShortPath);
            if (submodelElement == null)
                return new Result<IReference>(false, new NotFoundMessage($"SubmodelElement {idShortPath}"));

            var keys = new List<IKey>()
            {
                //add submodel reference key
                Reference.CreateReferenceKey(_submodel)
            };
            //add submodel element keys (recursive with parent containers)
            keys.AddRange(submodelElement.RetrieveReferenceKeys());

            var reference = new Reference(keys)
            {
                Type = ReferenceType.ModelReference
            };
            return new Result<IReference>(true, reference);
        }

        public IResult<PagedResult<IReference>> RetrieveSubmodelElementsReference(int limit = 100,
            string cursor = "")
        {
            if (_submodel == null)
                return new Result<PagedResult<IReference>>(false, new NotFoundMessage("Submodel"));

            var result = RetrieveSubmodelElements(limit, cursor);
            if (!result.Success || result.Entity == null || result.Entity.Result == null)
                return new Result<PagedResult<IReference>>(false, new NotFoundMessage("SubmodelElements"));

            var keys = new List<IKey>()
            {
                //add submodel reference key
                Reference.CreateReferenceKey(_submodel)
            };

            foreach (var sme in result.Entity.Result)
                //add submodel element key
                keys.Add(Reference.CreateReferenceKey(sme));

            var reference = new Reference(keys);
            reference.Type = ReferenceType.ModelReference;
            return new Result<PagedResult<IReference>>(true, new PagedResult<IReference>(reference, result.Entity.PagingMetadata));
        }

        public IResult UpdateSubmodelElementValue(string submodelElementId, ValueScope value)
        {
            if (_submodel == null)
                return new Result(false, new NotFoundMessage("Submodel"));

            if (value == null)
                return new Result(false, new NotFoundMessage("value"));

            var submodelElement = _submodel.SubmodelElements.Retrieve<ISubmodelElement>(submodelElementId);
            if (!submodelElement.Success || submodelElement.Entity == null)
                return new Result(false, new NotFoundMessage($"SubmodelElement {submodelElementId}"));

            return UpdateSubmodelElementValue(submodelElement.Entity, value);
        }

        public IResult UpdateSubmodelElementValue(ISubmodelElement submodelElement, ValueScope value)
        {
            if (submodelElement.Set != null)
            {
                submodelElement.Set.Invoke(submodelElement, value);
                return new Result(true);
            }
            else
                return new Result(false, new NotFoundMessage($"SubmodelElementHandler for {submodelElement.IdShort}"));
        }

        public IResult DeleteSubmodelElement(string submodelElementId)
        {
            if (_submodel == null)
                return new Result(false, new NotFoundMessage("Submodel"));

            if (_submodel.SubmodelElements == null)
                return new Result(false, new NotFoundMessage(submodelElementId));

            var deleted = _submodel.SubmodelElements.Delete(submodelElementId);
            return deleted;
        }

        public IResult<ISubmodel> RetrieveSubmodel()
        {
            if (_submodel == null)
                return new Result<ISubmodel>(false, new ErrorMessage("The service provider's inner Submodel object is null"));

            return new Result<ISubmodel>(true, _submodel);
        }

        public IResult UpdateSubmodel(ISubmodel submodel)
        {
            if (_submodel == null)
                return new Result(false, new NotFoundMessage("Submodel"));

            // update the metadata
            var result = UpdateSubmodelMetadata(submodel);
            if (!result.Success)
                return result;

            // update submodel elements
            // default init as empty list
            if (submodel.SubmodelElements.Count > 0)
            {
                _submodel.SubmodelElements.Clear();
                _submodel.SubmodelElements.AddRange(submodel.SubmodelElements);
            }

            return new Result(true);
        }

        public IResult UpdateSubmodelMetadata(ISubmodel submodel)
        {
            if (_submodel == null)
                return new Result(false, new ErrorMessage("The service provider's inner Submodel object is null"));
            
            string idShort = submodel.IdShort ?? _submodel.IdShort;
            var updatedSubmodel = new Submodel(idShort, _submodel.Id)
            {
                Administration = submodel.Administration ?? _submodel.Administration,
                Category = submodel.Category ?? _submodel.Category,
                SemanticId = submodel.SemanticId ?? _submodel.SemanticId,
                ConceptDescription = submodel.ConceptDescription ?? _submodel.ConceptDescription,
                SubmodelElements = _submodel.SubmodelElements,
            };

            // default init as empty list
            updatedSubmodel.SupplementalSemanticIds = submodel.SupplementalSemanticIds.Any() ? submodel.SupplementalSemanticIds : _submodel.SupplementalSemanticIds;
            // default init as empty list
            updatedSubmodel.Qualifiers = submodel.Qualifiers.Any() ? submodel.Qualifiers : _submodel.Qualifiers;
            // default init as empty list
            updatedSubmodel.Description = submodel.Description.Any() ? submodel.Description : _submodel.Description;
            // default init as empty list
            updatedSubmodel.DisplayName = submodel.DisplayName.Any() ? submodel.DisplayName : _submodel.DisplayName;
            // default init as empty list
            updatedSubmodel.EmbeddedDataSpecifications = submodel.EmbeddedDataSpecifications.Any() ? submodel.EmbeddedDataSpecifications : _submodel.EmbeddedDataSpecifications;

            _submodel = updatedSubmodel;
            // Update the parent reference of the SubmodelElements collection to point to the new Submodel instance
            _submodel.SubmodelElements.Parent = _submodel;
            return new Result(true);
        }

        public IResult ReplaceSubmodel(ISubmodel submodel)
        {
            _submodel = submodel;
            // Update the parent reference of the SubmodelElements collection to point to the new Submodel instance
            _submodel.SubmodelElements.Parent = _submodel;
            return new Result(true);
        }
    }
}
