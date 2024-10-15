using System;
using UnityEngine;

namespace CreationalPatterns.Factory
{
    public abstract class Enemy
    {
        public abstract void Attack();
    }

    public class Vampire : Enemy
    {
        public override void Attack()
        {
            Debug.Log("Vampire attacks!");
        }
    }

    public class Zombie : Enemy
    {
        public override void Attack()
        {
            Debug.Log("Zombie attacks!");
        }
    }

    public abstract class EnemyFactory
    {
        public abstract Enemy CreateEnemy();

        public static Enemy CreateEnemy(string type)
        {
            switch (type)
            {
                case "Vampire":
                    return new Vampire();
                case "Zombie":
                    return new Zombie();
                default:
                    throw new Exception("Unknown enemy type");
            }
        }
    }

    public class VampireFactory : EnemyFactory
    {
        public override Enemy CreateEnemy()
        {
            return new Vampire();
        }
    }

    public class ZombieFactory : EnemyFactory
    {
        public override Enemy CreateEnemy()
        {
            return new Zombie();
        }
    }
}