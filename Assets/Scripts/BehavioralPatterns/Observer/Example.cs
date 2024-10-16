using System.Collections.Generic;
using UnityEngine;

namespace BehavioralPatterns.Observer
{
    public class Subject : MonoBehaviour
    {
        private readonly List<IObserver> observers = new();

        public void AddObserver(IObserver observer)
        {
            observers.Add(observer);
        }

        public void RemoveObserver(IObserver observer)
        {
            observers.Remove(observer);
        }

        public void ChangeState()
        {
            NotifyObservers();
        }

        private void NotifyObservers()
        {
            foreach (var observer in observers) observer.Update();
        }
    }

    public interface IObserver
    {
        void Update();
    }

    public class ConcreteObserver : MonoBehaviour, IObserver
    {
        public void Update()
        {
            Debug.Log("Observer notified of state change!");
        }
    }
    
    public class ObserverExample : MonoBehaviour
    {
        private void Start()
        {
            var subject = new GameObject("Subject").AddComponent<Subject>();
            IObserver observer1 = new GameObject("Observer1").AddComponent<ConcreteObserver>();
            IObserver observer2 = new GameObject("Observer2").AddComponent<ConcreteObserver>();

            subject.AddObserver(observer1);
            subject.AddObserver(observer2);

            subject.ChangeState();
        }
    }
}