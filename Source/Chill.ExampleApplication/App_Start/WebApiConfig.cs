using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Chill.ExampleApplication.Dal;
using Module = Autofac.Module;

namespace Chill.ExampleApplication
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config, IContainer container)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new {id = RouteParameter.Optional}
                );

            // Create the depenedency resolver.
            var resolver = new AutofacWebApiDependencyResolver(container);

            // Configure Web API with the dependency resolver.
            config.DependencyResolver = resolver;
        }
    }

    public static class ContainerConfiguration
    {
        public static IContainer GetContainer()
        {
            // Create the container builder.
            var builder = new ContainerBuilder();

            builder.RegisterModule<StudentAutofacModule>();

            return builder.Build();
        }

        public  class StudentAutofacModule : Module

        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
                builder.RegisterType<StudentContext>()
                    .InstancePerRequest();
                base.Load(builder);
            }
        }
    }


}
