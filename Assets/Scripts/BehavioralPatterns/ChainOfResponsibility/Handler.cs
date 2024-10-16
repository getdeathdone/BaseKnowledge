using UnityEngine;

namespace BehavioralPatterns.ChainOfResponsibility
{
    public abstract class Handler : MonoBehaviour
    {
        protected Handler nextHandler;

        public void SetNextHandler(Handler handler)
        {
            nextHandler = handler;
        }

        public void HandleRequest(string request)
        {
            if (CanHandle(request))
                Debug.Log($"{GetType().Name} handled the request: {request}");
            else if (nextHandler != null)
                nextHandler.HandleRequest(request);
            else
                Debug.Log("No handler could handle the request.");
        }

        protected abstract bool CanHandle(string request);
    }

    public class ConcreteHandlerA : Handler
    {
        protected override bool CanHandle(string request)
        {
            return request == "A";
        }
    }

    public class ConcreteHandlerB : Handler
    {
        protected override bool CanHandle(string request)
        {
            return request == "B";
        }
    }
}