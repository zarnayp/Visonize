using System.Reflection;
using Visonize.UsImaging.Domain.Service.Framework;

namespace Visonize.UsImaging.Domain.Tests.Service.Framework
{

    internal interface ITargetMockInterface
    {
        void Update(int i);

        ITargetMockInnerInterface InnerObject { get; }
    }

    internal interface ITargetMockInnerInterface
    {
        ITargetMockInnerInnerInterface InnerInner {  get; }

        void UpdateInner(int i);
    }

    internal interface ITargetMockInnerInnerInterface
    {
        void UpdateInnerInner(int i);
    }

    internal class TargetMockClass : ITargetMockInterface
    {
        public TargetMockClass(TargetMockInnerClass innerObject)
        {
            InnerObject = innerObject;
        }

        public ITargetMockInnerInterface InnerObject { get; set; }

        public void Update(int i)
        {
            
        }
    }

    internal class TargetMockInnerClass : ITargetMockInnerInterface
    {
        public TargetMockInnerClass(ITargetMockInnerInnerInterface innerinner)
        {
            InnerInner = innerinner;
        }

        public ITargetMockInnerInnerInterface InnerInner { get; set; }

        public void UpdateInner(int i)
        {
        }
    }

    internal class TargetMockInnerInnerClass : ITargetMockInnerInnerInterface
    {
        public void UpdateInnerInner(int i)
        {
        }
    }

    internal class MockDispatcher : IDispatcher
    {
        public void BeginInvoke(Action action)
        {
            var type = action.Target.GetType();
            var field = type.GetField("targetMethod");
            var method = field.GetValue(action.Target);

            LastCalledMethodName = ((MethodInfo)method).Name;
            action.Invoke();
        }

        public string LastCalledMethodName { get; set; }
    }

    public class DispatcherProxyTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void DispatcherProxyTests_CreateDispatcherProxyAndCallMethod()
        {
            TargetMockInnerInnerClass mockInnerInnerClass = new TargetMockInnerInnerClass();
            TargetMockInnerClass mockInnerObject = new(mockInnerInnerClass);
            TargetMockClass mockObject = new(mockInnerObject);
            var mockDispatcher = new MockDispatcher();
            var proxy = DispatcherProxyCreator.CreateDispatcherProxy<ITargetMockInterface>(mockObject, mockDispatcher);

            proxy.Update(1);

            Assert.AreEqual("Update", mockDispatcher.LastCalledMethodName);
        }

        [Test]
        public void DispatcherProxyTests_CreateDispatcherProxyAndCallMethodOfInnerObject()
        {
            TargetMockInnerInnerClass mockInnerInnerClass = new TargetMockInnerInnerClass();
            TargetMockInnerClass mockInnerObject = new(mockInnerInnerClass);
            TargetMockClass mockObject = new(mockInnerObject);
            var mockDispatcher = new MockDispatcher();
            var proxy = DispatcherProxyCreator.CreateDispatcherProxy<ITargetMockInterface>(mockObject, mockDispatcher);

            proxy.InnerObject.UpdateInner(1);

            Assert.AreEqual("UpdateInner", mockDispatcher.LastCalledMethodName);
        }

        [Test]
        public void DispatcherProxyTests_CreateDispatcherProxyAndCallMethodOfInnerInnerObject()
        {
            TargetMockInnerInnerClass mockInnerInnerClass = new TargetMockInnerInnerClass();
            TargetMockInnerClass mockInnerObject = new(mockInnerInnerClass);
            TargetMockClass mockObject = new(mockInnerObject);
            var mockDispatcher = new MockDispatcher();
            var proxy = DispatcherProxyCreator.CreateDispatcherProxy<ITargetMockInterface>(mockObject, mockDispatcher);

            proxy.InnerObject.InnerInner.UpdateInnerInner(1);

            Assert.AreEqual("UpdateInnerInner", mockDispatcher.LastCalledMethodName);
        }
    }
}