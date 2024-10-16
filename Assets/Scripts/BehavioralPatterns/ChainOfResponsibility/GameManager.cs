using UnityEngine;

namespace BehavioralPatterns.ChainOfResponsibility
{
    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            Handler handlerA = gameObject.AddComponent<ConcreteHandlerA>();
            Handler handlerB = gameObject.AddComponent<ConcreteHandlerB>();

            handlerA.SetNextHandler(handlerB);

            handlerA.HandleRequest("B"); // ConcreteHandlerB handled the request
        }
    }
}