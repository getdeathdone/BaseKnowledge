using UnityEngine;

namespace CreationalPatterns.Prototype
{
    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            var originalArcher = new Archer();
            originalArcher.ShowStats();

            var clonedArcher = (Archer)originalArcher.Clone();
            clonedArcher.ShowStats();
        }
    }
}