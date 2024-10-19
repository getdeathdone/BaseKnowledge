using UnityEngine;

namespace BehavioralPatterns.Visitor
{
    public interface IVisitor
    {
        void Visit(Enemy goblin);
    }

    public class DamageVisitor : IVisitor
    {
        public void Visit(Enemy enemy)
        {
            if (enemy is Goblin) Debug.Log("Goblin takes damage.");

            if (enemy is Orc) Debug.Log("Goblin takes damage.");
        }
    }
}