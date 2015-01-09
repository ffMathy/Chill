using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Chill;
using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;

namespace Chill.Autofac
{
    internal class AutofacChillContainer : IChillContainer
    {
        private ILifetimeScope _container;
        private ContainerBuilder _containerBuilder;

        public AutofacChillContainer()
            : this(new ContainerBuilder())
        {
        }

        public AutofacChillContainer(ILifetimeScope container)
        {
            _container = container;
        }

        public AutofacChillContainer(ContainerBuilder containerBuilder)
        {
            _containerBuilder = containerBuilder;
        }

        public void RegisterFakeBuilder(Func<Type, Object> fakeBuilder)
        {
            if (_container != null)
                throw new InvalidOperationException("Container already built");
            _containerBuilder.RegisterSource(new FakeRegistrationHandler(fakeBuilder));
        }

        protected ILifetimeScope Container
        {
            get
            {
                if (_container == null)
                    _container = _containerBuilder.Build();
                return _container;
            }
        }

        public void Dispose()
        {
            Container.Dispose();
        }


        public void RegisterType<T>()
        {
            Container.ComponentRegistry.Register(RegistrationBuilder.ForType<T>().InstancePerLifetimeScope().CreateRegistration());
        }

        public T Get<T>(string key = null) where T : class
        {
            if (key == null)
            {
                return Container.Resolve<T>();
            }
            else
            {
                return Container.ResolveKeyed<T>(key);
            }
        }

        public T Set<T>(T valueToSet, string key = null) where T : class
        {
            if (key == null)
            {
                Container.ComponentRegistry
                    .Register(RegistrationBuilder.ForDelegate((c, p) => valueToSet)
                        .InstancePerLifetimeScope().CreateRegistration());

            }
            else
            {
                Container.ComponentRegistry
                    .Register(RegistrationBuilder.ForDelegate((c, p) => valueToSet)
                        .As(new KeyedService(key, typeof(T)))
                        .InstancePerLifetimeScope().CreateRegistration());
            }
            return Get<T>();
        }


        public bool IsRegistered<T>()
        {
            return IsRegistered(typeof(T));
        }

        public bool IsRegistered(System.Type type)
        {
            return Container.IsRegistered(type);
        }


        /// <summary> Resolves unknown interfaces and Mocks using the <see cref="Substitute"/>. </summary>
        internal class FakeRegistrationHandler : IRegistrationSource
        {
            private readonly Func<Type, object> _fakeBuilder;

            public FakeRegistrationHandler(Func<Type, Object> fakeBuilder)
            {
                _fakeBuilder = fakeBuilder;
            }

            /// <summary>
            /// Retrieve a registration for an unregistered service, to be used
            /// by the container.
            /// </summary>
            /// <param name="service">The service that was requested.</param>
            /// <param name="registrationAccessor"></param>
            /// <returns>
            /// Registrations for the service.
            /// </returns>
            public IEnumerable<IComponentRegistration> RegistrationsFor
                (Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
            {
                if (service == null)
                    throw new ArgumentNullException("service");

                var typedService = service as IServiceWithType;
                if (typedService == null ||
                    !typedService.ServiceType.IsInterface ||
                    typedService.ServiceType.IsGenericType &&
                    typedService.ServiceType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                    typedService.ServiceType.IsArray ||
                    typeof(IStartable).IsAssignableFrom(typedService.ServiceType))
                    return Enumerable.Empty<IComponentRegistration>();

                var rb = RegistrationBuilder.ForDelegate((c, p) => _fakeBuilder(typedService.ServiceType))
                    .As(service)
                    .InstancePerLifetimeScope();

                return new[] { rb.CreateRegistration() };
            }

            public bool IsAdapterForIndividualComponents
            {
                get { return false; }
            }
        }

    }

    public static class TestBaseExtensions
    {
        /// <summary>
        /// Explicitly register a type so that it will be created from the chill container from now on. 
        /// 
        /// This is handy if you wish to create a concrete type from a container that typically doesn't allow
        /// you to do so. (such as autofac)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void RegisterConcreteType<T>(this TestBase testBase)
        {
            testBase.Container.RegisterType<T>();
        }
    }
}