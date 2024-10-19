using UnityEngine;

namespace BehavioralPatterns.Memento
{
    public class GameManager : MonoBehaviour
    {
        private Player player;
        private Memento savedState;

        private void Start()
        {
            player = new GameObject("Player").AddComponent<Player>();
            player.SetPosition(new Vector3(0, 0, 0));
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                savedState = player.Save();
                Debug.Log("Position saved!");
            }

            if (Input.GetKeyDown(KeyCode.R) && savedState != null)
            {
                player.Restore(savedState);
                Debug.Log("Position restored!");
            }

            if (Input.GetKeyDown(KeyCode.UpArrow)) player.SetPosition(player.transform.position + Vector3.up);
        }
    }
}