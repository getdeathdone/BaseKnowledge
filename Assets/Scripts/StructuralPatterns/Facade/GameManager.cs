using UnityEngine;

namespace StructuralPatterns.Facade
{
    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            var gameFacade = new GameFacade();
            gameFacade.StartGame();
        }
    }
}