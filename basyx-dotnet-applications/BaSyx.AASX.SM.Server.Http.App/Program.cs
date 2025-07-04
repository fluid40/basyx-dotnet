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
using BaSyx.Registry.ReferenceImpl.InMemory;
using BaSyx.Registry.Server.Http;
using BaSyx.Servers.AdminShell.Http;
using BaSyx.Utils.Settings;
using CommandLine;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Web;
using System;
using System.Collections.Generic;
using BaSyx.Discovery.mDNS;
using Endpoint = BaSyx.Models.Connectivity.Endpoint;

namespace BaSyx.AASX.SM.Server.Http.App
{
    class Program
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public class Options
        {
            [Option('u', "url", Required = false, HelpText = "base url of the reverse proxy in format 'http://127.0.0.1:5044")]
            public string Url { get; set; }
        }

        public static UriBuilder GetUrl(string[] args)
        {
            var options = new Options();
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o => options = o);

            return options.Url != null ? new UriBuilder(options.Url) : new UriBuilder("http://127.0.0.1:5044");
        }

        static void Main(string[] args)
        {
            var reverseProxyUrl = GetUrl(args);
            var submodelRepoPort = reverseProxyUrl.Port + 1;
            var aasRepoPort = reverseProxyUrl.Port + 2;
            var registryPort = reverseProxyUrl.Port + 3;

            // Build the Submodel Repository server
            Logger.Info("Starting Submodel Repository Http-Server...");

            var smRepoSettings = ServerSettings.CreateSettings();
            smRepoSettings.ServerConfig.Hosting.ContentPath = "SubmodelContent";
            smRepoSettings.ServerConfig.Hosting.Environment = "Development";
            smRepoSettings.ServerConfig.Hosting.Urls.Add($"http://0.0.0.0:{submodelRepoPort}");

            var smRepositoryService = new SubmodelRepositoryServiceProvider();
            smRepositoryService.UseDefaultEndpointRegistration(new List<IEndpoint> { new Endpoint($"http://0.0.0.0:{submodelRepoPort}", InterfaceName.SubmodelRepositoryInterface) });
            
            var smRepositoryServer = new SubmodelRepositoryHttpServer(smRepoSettings);
            smRepositoryServer.WebHostBuilder.UseNLog();
            smRepositoryServer.SetServiceProvider(smRepositoryService);
            smRepositoryServer.AddBaSyxUI(PageNames.SubmodelRepositoryServer);
            smRepositoryServer.AddSwagger(Interface.SubmodelRepository);

            _ = smRepositoryServer.RunAsync();

            // Build the AAS Repository server
            Logger.Info("Starting AAS Repository Http-Server...");

            var aasRepoSettings = ServerSettings.CreateSettings();
            aasRepoSettings.ServerConfig.Hosting.ContentPath = "AASContent";
            aasRepoSettings.ServerConfig.Hosting.Environment = "Development";
            aasRepoSettings.ServerConfig.Hosting.Urls.Add($"http://0.0.0.0:{aasRepoPort}");

            var aasRepositoryService = new AssetAdministrationShellRepositoryServiceProvider();
            aasRepositoryService.UseDefaultEndpointRegistration(new List<IEndpoint> { new Endpoint($"http://0.0.0.0:{aasRepoPort}", InterfaceName.AssetAdministrationShellRepositoryInterface) });
            
            var aasRepositoryServer = new AssetAdministrationShellRepositoryHttpServer(aasRepoSettings);
            aasRepositoryServer.WebHostBuilder.UseNLog();
            aasRepositoryServer.SetServiceProvider(aasRepositoryService);
            aasRepositoryServer.AddBaSyxUI(PageNames.AssetAdministrationShellRepositoryServer);
            aasRepositoryServer.AddSwagger(Interface.AssetAdministrationShellRepository);
            
            _ = aasRepositoryServer.RunAsync();

            // Build the AAS registry Server
            Logger.Info("Starting registry Http-Server...");

            var registrySettings = ServerSettings.CreateSettings();
            registrySettings.ServerConfig.Hosting.ContentPath = "SubmodelContent";
            registrySettings.ServerConfig.Hosting.Environment = "Development";
            registrySettings.ServerConfig.Hosting.Urls.Add($"http://0.0.0.0:{registryPort}");

            var registryImpl = new InMemoryRegistry();
            var registryServer = new RegistryHttpServer(registrySettings);
            registryServer.WebHostBuilder.UseNLog();
            registryServer.AddBaSyxUI(PageNames.AssetAdministrationShellRegistryServer);
            registryServer.AddSwagger(Interface.AssetAdministrationShellRegistry);
            registryServer.SetRegistryProvider(registryImpl);

            //Start mDNS Discovery ability when the server successfully booted up
            registryServer.ApplicationStarted = () =>
            {
                registryImpl.StartDiscovery();
            };

            //Start mDNS Discovery when the server is shutting down
            registryServer.ApplicationStopping = () =>
            {
                registryImpl.StopDiscovery();
            };

            _ = registryServer.RunAsync();

            // Start YARP reverse proxy on public port (e.g. 5043)
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseUrls(reverseProxyUrl.ToString());
            Logger.Info($"Reverse Proxy URL: {reverseProxyUrl}");

            builder.Services.AddReverseProxy()
                .LoadFromMemory(
                    [
                        new Yarp.ReverseProxy.Configuration.RouteConfig
                        {
                            RouteId = "submodels",
                            Match = new Yarp.ReverseProxy.Configuration.RouteMatch { Path = "/submodels/{**catch-all}" },
                            ClusterId = "submodelCluster"
                        },
                        new Yarp.ReverseProxy.Configuration.RouteConfig
                        {
                            RouteId = "shells",
                            Match = new Yarp.ReverseProxy.Configuration.RouteMatch { Path = "/shells/{**catch-all}" },
                            ClusterId = "aasCluster"
                        },
                        new Yarp.ReverseProxy.Configuration.RouteConfig
                        {
                            RouteId = "shell-descriptors",
                            Match = new Yarp.ReverseProxy.Configuration.RouteMatch { Path = "/shell-descriptors/{**catch-all}" },
                            ClusterId = "registryCluster"
                        }
                    ],
                    [
                        new Yarp.ReverseProxy.Configuration.ClusterConfig
                        {
                            ClusterId = "submodelCluster",
                            Destinations = new Dictionary<string, Yarp.ReverseProxy.Configuration.DestinationConfig>
                            {
                                { "submodels", new Yarp.ReverseProxy.Configuration.DestinationConfig { Address = $"http://127.0.0.1:{submodelRepoPort}/" }}
                            }
                        },
                        new Yarp.ReverseProxy.Configuration.ClusterConfig
                        {
                            ClusterId = "aasCluster",
                            Destinations = new Dictionary<string, Yarp.ReverseProxy.Configuration.DestinationConfig>
                            {
                                { "shells", new Yarp.ReverseProxy.Configuration.DestinationConfig { Address = $"http://127.0.0.1:{aasRepoPort}/" }}
                            }
                        },
                        new Yarp.ReverseProxy.Configuration.ClusterConfig
                        {
                            ClusterId = "registryCluster",
                            Destinations = new Dictionary<string, Yarp.ReverseProxy.Configuration.DestinationConfig>
                            {
                                { "shell-descriptors", new Yarp.ReverseProxy.Configuration.DestinationConfig { Address = $"http://127.0.0.1:{registryPort}/" }}
                            }
                        }
                    ]
                );

            var app = builder.Build();
            app.MapReverseProxy();
            app.MapGet("/", () => Results.Json(new
            {
                message = "Welcome to Fluid4.0 AAS / Submodel Server",
                status_code = 200,
                submodel_repository_server_url = $"http://127.0.0.1:{submodelRepoPort}/",
                aas_repository_server_url = $"http://127.0.0.1:{aasRepoPort}/",
                register_server_url = $"http://127.0.0.1:{registryPort}/"
            }));
            app.Run();
        }
    }
}
