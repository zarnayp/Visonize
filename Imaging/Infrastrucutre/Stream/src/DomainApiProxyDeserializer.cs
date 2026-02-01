using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Interfaces;
using DupploPulse.UsImaging.Infrastructure.Common;

namespace DupploPulse.UsImaging.Infrastructure.Stream
{
    public class DomainApiProxyDeserializer
    {
        private readonly object proxyiedObject;
        private readonly Type proxyObjectType;
        private readonly IGeneralLogger logger;

        public DomainApiProxyDeserializer(object proxyiedObject, IGeneralLogger logger)
        {
            this.proxyiedObject = proxyiedObject;
            this.proxyObjectType = proxyiedObject.GetType();
            this.logger = logger;
        }

        public void Deserialize(string message)
        {
            ApiCall apiCall = new ApiCall(message);

            var innerProxyObjectType = this.proxyObjectType;
            var innerProxyiedObject = this.proxyiedObject;

            for(int i = 0; i < apiCall.Methods.Count - 1; i++)
            {
                var innerObjectGetMethodInfo = innerProxyObjectType.GetMethod(apiCall.Methods[i].MethodName);

                var parametersOfInnerGetMethod = innerObjectGetMethodInfo.GetParameters();
                var getArgs = new object[parametersOfInnerGetMethod.Length];
                for (int j = 0; j < parametersOfInnerGetMethod.Length; j++)
                {
                    getArgs[j] = Convert.ChangeType(apiCall.Methods[i].Arguments[j], parametersOfInnerGetMethod[j].ParameterType);
                }

                innerProxyiedObject = innerObjectGetMethodInfo.Invoke(innerProxyiedObject, getArgs);
                innerProxyObjectType = innerObjectGetMethodInfo.ReturnType;
            }

            var method = apiCall.Methods[apiCall.Methods.Count - 1];
            var methodName = method.MethodName;

            var methodInfo = innerProxyObjectType.GetMethod(methodName);

            if (method == null)
            {
                logger.LogError($"Api call {apiCall} not processed. Method {methodName} not found.");
            }

            var parameters = methodInfo.GetParameters();

            var args = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                args[i] = Convert.ChangeType(method.Arguments[i], parameters[i].ParameterType);
            }

            methodInfo.Invoke(innerProxyiedObject, args);
        }
    }
}
