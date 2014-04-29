﻿using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Exceptionless.Core;
using Exceptionless.Core.Extensions;
using Exceptionless.Core.Jobs;
using Exceptionless.Core.Utility;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security.OAuth;
using Owin;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;

namespace Exceptionless.Api {
    public static class AppBuilder {
        public static void Build(IAppBuilder app) {
            BuildWithContainer(app, CreateContainer());
        }

        public static void BuildWithContainer(IAppBuilder app, Container container) {
            if (container == null)
                throw new ArgumentNullException("container");

            var config = new HttpConfiguration();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.MapHttpAttributeRoutes();

            container.RegisterWebApiFilterProvider(config);
            try {
                container.Verify();
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
                throw;
            }
            config.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);

            // sample middleware that would be how we would auth an api token
            // maybe we should be using custom OAuthBearerAuthenticationProvider's
            // http://leastprivilege.com/2013/10/31/retrieving-bearer-tokens-from-alternative-locations-in-katanaowin/
            app.Use((context, next) => {
                var token = context.Request.Query.Get("access_token");
                if (String.IsNullOrEmpty(token)) {
                    var authHeader = context.Request.Headers.Get("Authorization");
                    if (!String.IsNullOrEmpty(authHeader)) {
                        var authHeaderVal = AuthenticationHeaderValue.Parse(authHeader);
                        if (authHeaderVal.Scheme.Equals("token", StringComparison.OrdinalIgnoreCase)
                            || authHeaderVal.Scheme.Equals("bearer", StringComparison.OrdinalIgnoreCase))
                            token = authHeaderVal.Parameter;
                    }
                }

                var projectRepository = container.GetInstance<IProjectRepository>();
                var project = projectRepository.GetByApiKey(token);
                if (project == null)
                    return next.Invoke();

                context.Request.User = PrincipalUtility.CreateClientUser(project.Id);
                return next.Invoke();
            });
            app.UseStageMarker(PipelineStage.Authenticate);

            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
            app.UseWebApi(config);

            AutoMapperConfig.CreateMappings();

            // TODO: Remove this as it's only for testing.
            Task.Factory.StartNew(() => container.GetInstance<ProcessEventPostsJob>().Run());
        }

        public static Container CreateContainer() {
            var container = new Container();
            container.Options.AllowOverridingRegistrations = true;
            container.Options.PropertySelectionBehavior = new InjectAttributePropertySelectionBehavior();

            container.RegisterPackage<Bootstrapper>();

            return container;
        }
    }
}