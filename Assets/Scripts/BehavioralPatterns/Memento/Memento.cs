using UnityEngine;

namespace BehavioralPatterns.Memento
{
    public class Memento
    {
        public Memento(Vector3 position)
        {
            Position = position;
        }

        public Vector3 Position { get; }
    }

    public class Player : MonoBehaviour
    {
        private Vector3 _position;

        public void SetPosition(Vector3 position)
        {
            _position = position;
            transform.position = _position;
        }

        public Memento Save()
        {
            return new Memento(_position);
        }

        public void Restore(Memento memento)
        {
            SetPosition(memento.Position);
        }
    }
}