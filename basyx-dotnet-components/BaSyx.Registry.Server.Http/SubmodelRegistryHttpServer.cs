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
using BaSyx.API.Http.Controllers;
using BaSyx.API.Interfaces;
using BaSyx.Components.Common;
using BaSyx.Utils.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BaSyx.Registry.Server.Http
{
    public class SubmodelRegistryHttpServer : ServerApplication
    {
        public SubmodelRegistryHttpServer() : this(null, null)
        { }

        public SubmodelRegistryHttpServer(ServerSettings serverSettings) : this(serverSettings, null)
        { }

        public SubmodelRegistryHttpServer(ServerSettings serverSettings, string[] webHostBuilderArgs)
            : base(serverSettings, webHostBuilderArgs)
        {
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            WebHostBuilder.UseSetting(WebHostDefaults.ApplicationKey, entryAssembly.FullName);
        }

        public void SetRegistryProvider(ISubmodelRegistryInterface smRegistryProvider)
        {
            WebHostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<ISubmodelRegistryInterface>(smRegistryProvider);
                services.AddMvc((options) =>
                {
                options.Conventions.Add(new ControllerConvention(this)
                    .Include(typeof(SubmodelRegistryController))
                    .Include(typeof(DescriptionController)));
                });
            });
        }
    }
}
