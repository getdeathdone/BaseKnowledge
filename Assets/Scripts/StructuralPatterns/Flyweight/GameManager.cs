using UnityEngine;

namespace StructuralPatterns.Flyweight
{
    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            var bulletFactory = new BulletFactory();

            var bullet1 = bulletFactory.GetBullet("9mm");
            bullet1.Fire();

            var bullet2 = bulletFactory.GetBullet("9mm");
            bullet2.Fire();

            var bullet3 = bulletFactory.GetBullet("5.56mm");
            bullet3.Fire();
        }
    }
}