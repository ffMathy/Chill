using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using Chill.ExampleApplication;
using Chill.ExampleApplication.Dal;
using FluentAssertions;
using RestSharp;
using Xunit;

namespace Chill.Examples.Tests.ExampleApplication
{
    public class For_StudentController
    {
        public class When_getting_student : GivenWhenThen
        {
            [Fact]
            public void FactMethodName2()
            {
                var port = "8083";
                var config = new HttpSelfHostConfiguration(string.Format("http://localhost:{0}", port));

                var container = ContainerConfiguration.GetContainer();
                WebApiConfig.Register(config, container);
                using (CrossProcessLockFactory.CreateCrossProcessLock())
                using (HttpSelfHostServer server = new HttpSelfHostServer(config))
                {
                    server.OpenAsync().Wait();

                    var request = new RestRequest("/Students");
                    RestClient c = new RestClient("http://localhost:" + port);
                    var result = c.Get<List<StudentEntity>>(request);

                    result.Should().NotBeNull();
                    server.CloseAsync().Wait();
                }
            }

            [Fact]
            public void FactMethodName()
            {
                var port = "8083";
                var config = new HttpSelfHostConfiguration(string.Format("http://localhost:{0}", port));

                var container = ContainerConfiguration.GetContainer();
                WebApiConfig.Register(config, container);
                using (CrossProcessLockFactory.CreateCrossProcessLock())
                using (HttpSelfHostServer server = new HttpSelfHostServer(config))
                {
                    server.OpenAsync().Wait();

                    var request = new RestRequest("/Students");
                    RestClient c = new RestClient("http://localhost:" + port);
                    var result = c.Get<List<StudentEntity>>(request);

                    result.Should().NotBeNull();
                    server.CloseAsync().Wait();
                }
            }
        }
    }

    public class CrossProcessLockFactory
    {
        private static int DefaultTimoutInMinutes = 2;
        public static IDisposable CreateCrossProcessLock()
        {
            return new ProcessLock(TimeSpan.FromMinutes(DefaultTimoutInMinutes));
        }

        public static IDisposable CreateCrossProcessLock(TimeSpan timespan)
        {
            return new ProcessLock(timespan);
        }
    }

    public class ProcessLock : IDisposable
    {
        // the name of the global mutex;
        private const string MutexName = "FAA9569-7DFE-4D6D-874D-19123FB16CBC-8739827-[SystemSpecicString]";

        private Mutex _globalMutex;

        private bool _owned = false;


        public ProcessLock(TimeSpan timeToWait)
        {
            try
            {
                _globalMutex = new Mutex(true, MutexName, out _owned);
                if (_owned == false)
                {
                    // did not get the mutex, wait for it.
                    _owned = _globalMutex.WaitOne(timeToWait);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                throw;
            }
        }

        public void Dispose()
        {
            if (_owned)
            {
                _globalMutex.ReleaseMutex();
            }
            _globalMutex = null;
        }
    }
}
