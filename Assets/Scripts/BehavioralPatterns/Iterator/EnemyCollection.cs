using System.Collections.Generic;
using UnityEngine;

namespace BehavioralPatterns.Iterator
{
    public class EnemyCollection : MonoBehaviour
    {
        private readonly List<Enemy> enemies = new();

        private void Start()
        {
            AddEnemy(new Enemy("Goblin"));
            AddEnemy(new Enemy("Orc"));
            AddEnemy(new Enemy("Dragon"));

            foreach (var enemy in this) Debug.Log("Enemy: " + enemy.Name);
        }

        public void AddEnemy(Enemy enemy)
        {
            enemies.Add(enemy);
        }

        public IEnumerator<Enemy> GetEnumerator()
        {
            foreach (var enemy in enemies) yield return enemy;
        }
    }
}