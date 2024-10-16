using UnityEngine;

namespace StructuralPatterns.Composite
{
    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            var soldier1 = new Soldier();
            var soldier2 = new Soldier();
            var squad = new Squad();
            squad.AddUnit(soldier1);
            squad.AddUnit(soldier2);

            squad.Attack();
        }
    }
}