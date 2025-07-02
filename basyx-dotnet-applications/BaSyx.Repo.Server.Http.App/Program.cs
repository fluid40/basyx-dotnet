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
using BaSyx.Common.UI;
using BaSyx.Common.UI.Swagger;
using BaSyx.Models.Connectivity;
using BaSyx.Servers.AdminShell.Http;
using BaSyx.Utils.Settings;
using CommandLine;
using NLog;
using NLog.Web;
using Endpoint = BaSyx.Models.Connectivity.Endpoint;

namespace BaSyx.AASX.SM.Server.Http.App
{
    class Program
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public class Options
        {
            [Option('s', "settings", Required = false, HelpText = "Path to the ServerSettings.xml")]
            public string SettingsFilePath { get; set; }
        }

        static void Main(string[] args)
        {
            Logger.Info("Starting Repo Http-Server...");

            // AAS Repo Server

            var settings = GetSettings(args);
            var endpoints = settings.ServerConfig.Hosting.Urls.ConvertAll(url =>
            {
                var address = url.Replace("+", "127.0.0.1");
                Logger.Info("Using " + address + " as base endpoint url");
                return new Endpoint(address, InterfaceName.AssetAdministrationShellRepositoryInterface);
            });

            var httpServer = new RepositoryHttpServer(settings);
            httpServer.WebHostBuilder.UseNLog();

            var smRepositoryService = new SubmodelRepositoryServiceProvider();
            smRepositoryService.UseDefaultEndpointRegistration(endpoints);
            var aasRepositoryService = new AssetAdministrationShellRepositoryServiceProvider();
            aasRepositoryService.UseDefaultEndpointRegistration(endpoints);

            httpServer.SetServiceProvider(aasRepositoryService, smRepositoryService);

            httpServer.AddBaSyxUI(PageNames.AssetAdministrationShellRepositoryServer);
            httpServer.AddSwagger(Interface.All);

            httpServer.Run();
        }

        private static ServerSettings CreateDefaultSetting()
        {
            var settings = ServerSettings.CreateSettings();
            settings.ServerConfig.Hosting.ContentPath = "Content";
            settings.ServerConfig.Hosting.Environment = "Development";
            settings.ServerConfig.Hosting.Urls.Add($"http://127.0.0.1:5043");
            return settings;
        }

        private static ServerSettings GetSettings(string[] args)
        {
            var options = new Options();
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed(o => options = o);

            var settingsFilePath = "./ServerSettings.xml";
            if (!string.IsNullOrEmpty(options.SettingsFilePath))
                settingsFilePath = options.SettingsFilePath;

            Logger.Info("Try loading settings from: " + settingsFilePath);

            return ServerSettings.LoadSettingsFromFile(settingsFilePath) ?? CreateDefaultSetting();
        }
    }
}
