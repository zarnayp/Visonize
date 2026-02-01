
using System;

namespace DupploPulse.UsImaging.Domain.Service.Framework
{
    public class Dispatcher : IDispatcher
    {
        private object lockObject = new object();

        private List<Action> operationsList = new List<Action>();

        public void Dispatch()
        {
            List<Action> imediateOperationsList;

            lock (lockObject)
            {
                imediateOperationsList = new List<Action>(operationsList);
                operationsList.Clear();
            }

            foreach (var operation in imediateOperationsList)
            {
                operation.Invoke();
            }
        }

        public void BeginInvoke(Action action)
        {
            lock (lockObject)
            {
                operationsList.Add(action);
            }
        }
    }
}