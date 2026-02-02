using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using Visonize.UsImaging.Domain.Service.Framework;
using Org.BouncyCastle.Asn1.X509;
using static System.Runtime.InteropServices.JavaScript.JSType;

[assembly: InternalsVisibleTo("Visonize.UsImaging.Infrastructure.Stream.Tests")]
namespace Visonize.UsImaging.Infrastructure.Stream
{

    internal class ApiCall
    {
        public class Method
        {
            public Method(string method, List<string> args)
            {
                this.MethodName = method;
                this.Arguments = args;
            }

            public string MethodName { get; set; }
            public List<string> Arguments { get; set; }
        }

        public List<Method> Methods { get; set; } = new List<Method>();

        public string Serialize()
        {
            return TinyJson.JSONWriter.ToJson(Methods);
        }

        public void Clear()
        {
            Methods.Clear();
        }

        public ApiCall()
        {
        }

        public ApiCall(string serialized)
        {
            Methods = TinyJson.JSONParser.FromJson<List<Method>>(serialized);
        }


    }
 


    public static class DomainApiProxyCreator
    {
        public static T CreateDispatcherProxy<T>(Action<string> sendMessage) where T : class
        {
            if (sendMessage == null) throw new ArgumentNullException("SendMessage is null.");

            var proxy = DispatchProxy.Create<T, DomainApiProxySerializer<T>>();
            (proxy as DomainApiProxySerializer<T>).ApiCallAction = sendMessage;
            (proxy as DomainApiProxySerializer<T>).ApiCall = new ApiCall();
            return proxy;
        }

        internal static T CreateDispatcherProxyForInner<T>(Action<string> sendMessage, ApiCall apiCall) where T : class
        {
            if (sendMessage == null) throw new ArgumentNullException("SendMessage is null.");

            var proxy = DispatchProxy.Create<T, DomainApiProxySerializer<T>>();
            (proxy as DomainApiProxySerializer<T>).ApiCallAction = sendMessage;
            (proxy as DomainApiProxySerializer<T>).ApiCall = apiCall;
            return proxy;
        }
    }

    internal class DomainApiProxySerializer<T> : DispatchProxy where T : class
    {
        private MethodInfo createDispatcherProxyMethodInfo;
        private MethodInfo createDispatcherProxyMethodInfoForInner;
        private Dictionary<string,object> cachedInnerObjects = new Dictionary<string,object>();  // TODO: try to cache returning object to speed up

        public Action<string> ApiCallAction { get; set; }

        public ApiCall ApiCall { get; set; }

        public DomainApiProxySerializer()
        {
            this.createDispatcherProxyMethodInfo = typeof(DomainApiProxyCreator).GetMethod("CreateDispatcherProxy", BindingFlags.Public | BindingFlags.Static);
            this.createDispatcherProxyMethodInfoForInner = typeof(DomainApiProxyCreator).GetMethod("CreateDispatcherProxyForInner", BindingFlags.NonPublic | BindingFlags.Static);

        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            // Serialize method call into a simple string format
            //var command = targetMethod.Name + "|" + string.Join("|", args);
            //ApiCall?.Invoke(command);

            //string message;

            //if (ParentMessage != string.Empty)
            //{
            //    message = $"{ParentMessage}_{command}";
            //}
            //else
            //{
            //    message = command;
            //}

            List<string> list = args == null ? new List<string>() : args.Select(o => o.ToString()).ToList();

            ApiCall.Methods.Add(new (targetMethod.Name, list));

            // For void return type, return null; for value type return default
            if (targetMethod.ReturnType == typeof(void))
            {
                var message = ApiCall.Serialize();
                ApiCall.Clear();
                ApiCallAction?.Invoke(message);

                return null;
            }
            else if (targetMethod.ReturnType.IsInterface)
            {
                // Make the generic method
                MethodInfo constructed = this.createDispatcherProxyMethodInfoForInner.MakeGenericMethod(targetMethod.ReturnType);

                // Call it
                object proxy = constructed.Invoke(null, new object[] { ApiCallAction, ApiCall });

                return proxy;

            }

            throw new NotSupportedException($"Only interfaaces are allowed to be called. {targetMethod.Name} is not interface");
        }
    }
}
