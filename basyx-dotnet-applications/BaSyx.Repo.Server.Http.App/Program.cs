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
using BaSyx.API.ServiceProvider;
using BaSyx.Common.UI;
using BaSyx.Common.UI.Swagger;
using BaSyx.Models.Connectivity;
using BaSyx.Servers.AdminShell.Http;
using BaSyx.Utils.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Web;
using Endpoint = BaSyx.Models.Connectivity.Endpoint;

namespace BaSyx.AASX.SM.Server.Http.App
{
    class Program
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();


        static void Main(string[] args)
        {
            Logger.Info("Starting Repo Http-Server...");
            
            Logger.Info("Starting Submodel Http-Server...");

            // AAS Repo Server
            var settings = ServerSettings.CreateSettings();
            settings.ServerConfig.Hosting.ContentPath = "Content";
            settings.ServerConfig.Hosting.Environment = "Development";
            settings.ServerConfig.Hosting.Urls.Add($"http://0.0.0.0:5043");

            var httpServer = new RepositoryHttpServer(settings);
            httpServer.WebHostBuilder.UseNLog();
            var smRepositoryService = new SubmodelRepositoryServiceProvider();
            var smEndpoints = settings.ServerConfig.Hosting.Urls.ConvertAll(url =>
            {
                Logger.Info("Using " + url + " as sm repository base endpoint url");
                return new Endpoint(url, InterfaceName.AssetAdministrationShellRepositoryInterface);
            });
            smRepositoryService.UseDefaultEndpointRegistration(smEndpoints);

            var aasRepositoryService = new AssetAdministrationShellRepositoryServiceProvider();
            var aasEndpoints = settings.ServerConfig.Hosting.Urls.ConvertAll(url =>
            {
                Logger.Info("Using " + url + " as aas repository base endpoint url");
                return new Endpoint(url, InterfaceName.AssetAdministrationShellRepositoryInterface);
            });
            aasRepositoryService.UseDefaultEndpointRegistration(aasEndpoints);

            httpServer.SetServiceProvider(aasRepositoryService, smRepositoryService);

            httpServer.AddBaSyxUI(PageNames.AssetAdministrationShellRepositoryServer);
            httpServer.AddSwagger(Interface.All);

            httpServer.Run();
        }
    }
}
