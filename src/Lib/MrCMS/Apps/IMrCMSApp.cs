﻿using System;
using System.Collections.Generic;
using System.Reflection;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using MrCMS.Website.CMS;
using NHibernate.Cfg;

namespace MrCMS.Apps
{
    public interface IMrCMSApp
    {
        string Name { get; }
        string Version { get; }
        string ContentPrefix { get; set; }
        string ViewPrefix { get; set; }

        Assembly Assembly { get; }
        IEnumerable<Type> Conventions { get; }
        IEnumerable<Type> BaseTypes { get; }
        IDictionary<Type, string> SignalRHubs { get; }
        IEnumerable<RegistrationInfo> Registrations { get; }

        IServiceCollection RegisterServices(IServiceCollection serviceCollection);

        IRouteBuilder MapRoutes(IRouteBuilder routeBuilder);
        void SetupMvcOptions(MvcOptions options);
        void ConfigureAutomapper(IMapperConfigurationExpression expression);
        void AppendConfiguration(Configuration configuration);
        void ConfigureAuthorization(AuthorizationOptions options);
    }
}