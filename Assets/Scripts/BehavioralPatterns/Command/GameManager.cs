using UnityEngine;

namespace BehavioralPatterns.Command
{
    public class GameManager : MonoBehaviour
    {
        private ICommand moveCommand;

        private void Start()
        {
            moveCommand = new MoveCommand(gameObject, new Vector3(1, 0, 0));
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space)) moveCommand.Execute();
        }
    }
}