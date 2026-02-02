using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Visonize.UsImaging.Domain.Tests")]

namespace Visonize.UsImaging.Domain.Service.Framework
{
    
    internal static class DispatcherProxyCreator
    {
        public static T CreateDispatcherProxy<T>(T target, IDispatcher dispatcher) where T : class
        {
            if (dispatcher == null) throw new ArgumentNullException("Dispatcher is null.");
            if (target == null) throw new ArgumentException("Target is null.");

            var proxy = DispatchProxy.Create<T, DispatcherProxy<T>>();
            (proxy as DispatcherProxy<T>).Target = target;
            (proxy as DispatcherProxy<T>).Dispatcher = dispatcher;
            return proxy;
        }
    }

    internal class DispatcherProxy<T> : DispatchProxy where T : class
    {
        private MethodInfo createDispatcherProxyMethodInfo;

        public T Target { get; set; }
        public IDispatcher Dispatcher { get; set; }

        public DispatcherProxy()
        {
            this.createDispatcherProxyMethodInfo = typeof(DispatcherProxyCreator).GetMethod("CreateDispatcherProxy", BindingFlags.Public | BindingFlags.Static);
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            // TODO: in case of same thread don't need to dispach

            //if (Dispatcher.CheckAccess())
            //{
            //    return targetMethod.Invoke(Target, args);
            //}
            //else
            //{

            var returnType = targetMethod.ReturnType;

            if (returnType == typeof(void))
            {
                Dispatcher.BeginInvoke(() => targetMethod.Invoke(Target, args));
                return null;
            }
            else if (returnType.IsInterface)
            {
                var returnObject = targetMethod.Invoke(Target, args);

                // Make the generic method
                MethodInfo constructed = this.createDispatcherProxyMethodInfo.MakeGenericMethod(returnType);

                // Call it
                object proxy = constructed.Invoke(null, new object[] { returnObject, Dispatcher });

                return proxy;
            }
            //else if (typeof(Task).IsAssignableFrom(returnType))
            //{
            //    return Dispatcher.InvokeAsync(() =>
            //        (Task)targetMethod.Invoke(Target, args)
            //    ).Task.Unwrap();
            //}
            //else
            //{
            //    return Dispatcher.Invoke(() => targetMethod.Invoke(Target, args));
            //}
            return null;

            //}
        }
    }

}
