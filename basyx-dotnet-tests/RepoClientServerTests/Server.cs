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
using BaSyx.Servers.AdminShell.Http;
using BaSyx.Utils.Settings;
using NLog.Web;
using SimpleAssetAdministrationShell;
using System.Collections.Generic;

namespace RepoClientServerTests
{
    class Server
    {
        public static string ServerUrl = "http://localhost:5999";
        public static void Run()
        {
            var settings = new ServerSettings()
            {
               ServerConfig = new ServerConfiguration()
               {
                   Hosting = new HostingConfiguration()
                   {
                       Urls = [ServerUrl]
                   }
               }
            };

            var httpServer = new RepositoryHttpServer(settings);

            var smRepositoryService = new SubmodelRepositoryServiceProvider();
            smRepositoryService.UseAutoEndpointRegistration(settings.ServerConfig);
            var aasRepositoryService = new AssetAdministrationShellRepositoryServiceProvider();
            aasRepositoryService.UseAutoEndpointRegistration(settings.ServerConfig);
            httpServer.SetServiceProvider(aasRepositoryService, smRepositoryService);

            _ = httpServer.RunAsync();
        }
    }
}
