using System;
using System.Reflection;
using System.Security;
using Autofac;
using Autofac.Core;
using Chill.Autofac;
using FakeItEasy;

namespace Chill.AutofacFakeItEasy
{
    /// <summary>
    /// An implementation of <see cref="IAutoMockingContainer"/> that uses Autofac and FakeItEasy to build objects
    /// with mocked dependencies.
    /// </summary>
    internal class AutofacFakeItEasyMockingContainer : AutofacChillContainer
    {
        private Faker _faker;

        public AutofacFakeItEasyMockingContainer()
            : base(CreateContainerBuilder())
        {
            _faker = new Faker();
            RegisterFakeBuilder(_faker.CreateFake);
        }

        private static ContainerBuilder CreateContainerBuilder()
        {
            var builder = new ContainerBuilder();
            return builder;
        }
    }

    public class Faker
    {
        /// <summary>
        /// Creates a fake object.
        /// </summary>
        /// <param name="typedService">The typed service.</param>
        /// <returns>A fake object.</returns>
        [SecuritySafeCritical]
        public object CreateFake(Type typedService)
        {
            MethodInfo method = typeof(Faker).GetMethod("CreateFakeGeneric");
            MethodInfo generic = method.MakeGenericMethod(typedService);
            return generic.Invoke(this, null);

            
        }
        [SecuritySafeCritical]
        public T CreateFakeGeneric<T>()
        {
            var fake = A.Fake<T>();

            return fake;
        }

    }

}