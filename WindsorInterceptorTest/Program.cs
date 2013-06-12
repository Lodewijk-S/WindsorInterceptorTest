using Castle.Core;
using Castle.DynamicProxy;
using Castle.MicroKernel.Proxy;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindsorInterceptorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var container = new WindsorContainer())
            {
                container.Register(
                    Component.For<ScopeInterceptor>(), 
                    Component.For<IEngine>().ImplementedBy<Engine>());

                container.Kernel.ProxyFactory.AddInterceptorSelector(new ScopeSelector());

                var engine = container.Resolve<IEngine>();

                engine.Handle(r => Console.WriteLine(r));
            }

            Console.ReadLine();
        }
    }

    public interface IEngine
    {
        string Handle(string input);
        void Handle(Action<string> oncomplete);
    }

    public class Engine : IEngine
    {
        public string Handle(string input) 
        {
            return input;
        }

        public void Handle(Action<string> oncomplete)
        {
           ThreadPool.QueueUserWorkItem(x => {
                oncomplete(Handle("test"));
            });
        }
    }

    public class ScopeSelector : IModelInterceptorsSelector
    {
        public bool HasInterceptors(Castle.Core.ComponentModel model)
        {
            return model.Implementation == typeof(Engine);
        }

        public Castle.Core.InterceptorReference[] SelectInterceptors(Castle.Core.ComponentModel model, Castle.Core.InterceptorReference[] interceptors)
        {
            return new[] { InterceptorReference.ForType<ScopeInterceptor>() };
        }
    }

    public class ScopeInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            Console.WriteLine("Handled Handle with return type " + invocation.Method.ReturnType);
            invocation.Proceed();
        }
    }
}
