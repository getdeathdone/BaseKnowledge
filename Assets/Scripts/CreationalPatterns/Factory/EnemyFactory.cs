using System;
using UnityEngine;

namespace CreationalPatterns.Factory
{
    public abstract class Enemy
    {
        public abstract void Attack();
    }

    public class Zombie : Enemy
    {
        public override void Attack()
        {
            Debug.Log("Zombie attacks!");
        }
    }

    public class Vampire : Enemy
    {
        public override void Attack()
        {
            Debug.Log("Vampire attacks!");
        }
    }

    public class EnemyFactory
    {
        public static Enemy CreateEnemy(string type)
        {
            switch (type)
            {
                case "Zombie":
                    return new Zombie();
                case "Vampire":
                    return new Vampire();
                default:
                    throw new Exception("Unknown enemy type");
            }
        }
    }
}