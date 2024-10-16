using UnityEngine;

namespace StructuralPatterns.Composite
{
    public abstract class Unit
    {
        public abstract void Attack();
    }

    public class Soldier : Unit
    {
        public override void Attack()
        {
            Debug.Log("Soldier attacking");
        }
    }
}