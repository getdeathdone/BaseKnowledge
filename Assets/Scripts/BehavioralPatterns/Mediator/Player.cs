using UnityEngine;

namespace BehavioralPatterns.Mediator
{
    public class Player : MonoBehaviour
    {
        private Mediator _mediator;

        private void Start()
        {
            _mediator = FindObjectOfType<Mediator>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space)) _mediator.Notify(gameObject, "PlayerHit");
        }
    }
}