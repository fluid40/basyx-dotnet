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
using BaSyx.Models.Connectivity;
using BaSyx.Utils.ResultHandling;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using BaSyx.Utils.ResultHandling.ResultTypes;
using System.Collections.Concurrent;

namespace BaSyx.Registry.ReferenceImpl.InMemory
{
    public class SubmodelInMemoryRegistry : ISubmodelRegistryInterface
    {
        private static readonly ILogger logger = LoggingExtentions.CreateLogger<InMemoryRegistry>();

        private readonly ConcurrentDictionary<string, ISubmodelDescriptor> _descriptors;

        public SubmodelInMemoryRegistry()
        {
            _descriptors = new ConcurrentDictionary<string, ISubmodelDescriptor>();
        }

        public IResult<ISubmodelDescriptor> CreateSubmodelRegistration(ISubmodelDescriptor submodelDescriptor)
        {
            if (submodelDescriptor == null)
                return new Result<ISubmodelDescriptor>(new ArgumentNullException(nameof(submodelDescriptor)));
            if (submodelDescriptor.Id?.Id == null)
                return new Result<ISubmodelDescriptor>(new ArgumentNullException(nameof(submodelDescriptor.Id)));

            bool success = _descriptors.TryAdd(submodelDescriptor.Id.Id, submodelDescriptor);
            if (success)
                return new Result<ISubmodelDescriptor>(true, submodelDescriptor);
            else
                return new Result<ISubmodelDescriptor>(false, new ConflictMessage($"Descriptor with {submodelDescriptor.Id.Id}"));
        }

        public IResult DeleteSubmodelRegistration(string submodelIdentifier)
        {
            if (string.IsNullOrEmpty(submodelIdentifier))
                return new Result(new ArgumentNullException(nameof(submodelIdentifier)));

            if (_descriptors.TryGetValue(submodelIdentifier, out _))
            {
                bool success = _descriptors.TryRemove(submodelIdentifier, out _);
                if (success)
                    return new Result(true);
                else
                    return new Result(false, new ErrorMessage($"Unable to delete descriptor with {submodelIdentifier}"));
            }
            else
                return new Result(false, new NotFoundMessage($"Descriptor with {submodelIdentifier}"));
        }



        public IResult<PagedResult<IEnumerable<ISubmodelDescriptor>>> RetrieveAllSubmodelRegistrations()
        {
            var smDescriptors = _descriptors.Values.ToList();
            return new Result<PagedResult<IEnumerable<ISubmodelDescriptor>>>(true, smDescriptors);
        }

        public IResult<PagedResult<IEnumerable<ISubmodelDescriptor>>> RetrieveAllSubmodelRegistrations(Predicate<ISubmodelDescriptor> predicate)
        {
            var allDescriptors = RetrieveAllSubmodelRegistrations();
            return new Result<PagedResult<IEnumerable<ISubmodelDescriptor>>>(allDescriptors.Success,
                new PagedResult<IEnumerable<ISubmodelDescriptor>>(allDescriptors.Entity.Result.Where(ConvertToFunc(predicate))));
        }


        public IResult<ISubmodelDescriptor> RetrieveSubmodelRegistration(string submodelIdentifier)
        {
            if (string.IsNullOrEmpty(submodelIdentifier))
                return new Result<ISubmodelDescriptor>(new ArgumentNullException(nameof(submodelIdentifier)));

            if (_descriptors.TryGetValue(submodelIdentifier, out var descriptor))
                return new Result<ISubmodelDescriptor>(true, descriptor);
            else
                return new Result<ISubmodelDescriptor>(false, new NotFoundMessage($"Descriptor with {submodelIdentifier}"));
        }


        public IResult<ISubmodelDescriptor> UpdateSubmodelRegistration(string submodelIdentifier, ISubmodelDescriptor submodelDescriptor)
        {
            if (string.IsNullOrEmpty(submodelIdentifier))
                return new Result<ISubmodelDescriptor>(new ArgumentNullException(nameof(submodelIdentifier)));
            if (submodelDescriptor == null)
                return new Result<ISubmodelDescriptor>(new ArgumentNullException(nameof(submodelDescriptor)));
            if (submodelDescriptor.Id?.Id == null)
                return new Result<ISubmodelDescriptor>(new ArgumentNullException(nameof(submodelDescriptor.Id)));

            if (_descriptors.TryGetValue(submodelIdentifier, out var oldDescriptor))
            {
                bool success = _descriptors.TryUpdate(submodelIdentifier, submodelDescriptor, oldDescriptor);
                if (success)
                    return new Result<ISubmodelDescriptor>(true);
                else
                    return new Result<ISubmodelDescriptor>(false, new ErrorMessage($"Unable to update descriptor with {submodelDescriptor.Id.Id}"));
            }
            else
                return new Result<ISubmodelDescriptor>(false, new NotFoundMessage($"Descriptor with {submodelDescriptor.Id.Id}"));
        }

        private Func<T, bool> ConvertToFunc<T>(Predicate<T> predicate)
        {
            return new Func<T, bool>(predicate);
        }
    }
}