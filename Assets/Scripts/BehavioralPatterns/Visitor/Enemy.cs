using UnityEngine;

namespace BehavioralPatterns.Visitor
{
    public abstract class Enemy : MonoBehaviour, IEnemyElement
    {
        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class Goblin : Enemy
    {
    }

    public class Orc : Enemy
    {
    }
}