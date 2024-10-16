using System.Collections.Generic;
using UnityEngine;

namespace StructuralPatterns.Flyweight
{
    public class Bullet
    {
        public string Type { get; set; }

        public void Fire()
        {
            Debug.Log("Firing bullet of type: " + Type);
        }
    }

    public class BulletFactory
    {
        private readonly Dictionary<string, Bullet> _bullets = new();

        public Bullet GetBullet(string type)
        {
            if (!_bullets.ContainsKey(type))
            {
                _bullets[type] = new Bullet { Type = type };
                Debug.Log("Creating new bullet of type: " + type);
            }

            return _bullets[type];
        }
    }
}