using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Chill.Autofac;
using NSubstitute;

namespace Chill.AutofacNSubstitute
{
    /// <summary>
    /// Automocking container that uses NSubstitute to create mocks and Autofac as the container. 
    /// </summary>
    internal class AutofacNSubstituteChillContainer : AutofacChillContainer
    {

        public AutofacNSubstituteChillContainer()
            : base(CreateBuilder())
        {
            RegisterFakeBuilder((t) => Substitute.For(new Type[]{t}, null));
        }

        static ContainerBuilder CreateBuilder()
        {
            var containerBuilder = new ContainerBuilder();
            return containerBuilder;
        }

      
    }
}