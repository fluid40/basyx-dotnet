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
using BaSyx.API.ServiceProvider;
using BaSyx.Components.Common;
using BaSyx.Models.Connectivity;
using BaSyx.Utils.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BaSyx.Servers.AdminShell.Http
{
    public class RepositoryHttpServer : ServerApplication
    {
        public RepositoryHttpServer() : this(null, null)
        { }

        public RepositoryHttpServer(ServerSettings serverSettings) : this(serverSettings, null)
        { }

        public RepositoryHttpServer(ServerSettings serverSettings, string[] webHostBuilderArgs)
            : base(serverSettings, webHostBuilderArgs)
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            WebHostBuilder.UseSetting(WebHostDefaults.ApplicationKey, entryAssembly.FullName);
        }

        public void SetServiceProvider(IAssetAdministrationShellRepositoryServiceProvider assRepositoryServiceProvider, ISubmodelRepositoryServiceProvider smRepoServiceProvider)
        {
            WebHostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<IAssetAdministrationShellRepositoryServiceProvider>(assRepositoryServiceProvider);
                services.AddSingleton<ISubmodelRepositoryServiceProvider>(smRepoServiceProvider);
                services.AddSingleton<IServiceProvider>(assRepositoryServiceProvider);
                services.AddSingleton<IServiceProvider>(smRepoServiceProvider);
                services.AddMvc((options) =>
                {
                    options.Conventions.Add(new ControllerConvention(this)
                        .Include(typeof(RepositoryController)));
                });
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                    {
                        Title = "BaSyx Repository HTTP Server",
                        Version = "v1",
                        Description = "A BaSyx HTTP server for Asset Administration Shell and Submodel repositories."
                    });
                });
            });
            this.Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BaSyx Repository HTTP Server V1");
                });
            });
        }       
    }
}
