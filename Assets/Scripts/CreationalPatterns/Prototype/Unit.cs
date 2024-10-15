using System;
using UnityEngine;

namespace CreationalPatterns.Prototype
{
    public abstract class Unit : ICloneable
    {
        public int Health { get; set; }
        public int AttackPower { get; set; }

        public object Clone()
        {
            return DeepClone();
        }

        public abstract Unit DeepClone();

        public virtual void ShowStats()
        {
            Debug.Log($"Health: {Health}, Attack Power: {AttackPower}");
        }
    }

    public class Archer : Unit
    {
        public Archer()
        {
            Health = 100;
            AttackPower = 20;
        }

        public override Unit DeepClone()
        {
            return (Unit)MemberwiseClone();
        }
    }
}