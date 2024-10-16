using UnityEngine;

namespace StructuralPatterns.Bridge
{
    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            IAmmo bullet = new Bullet();
            IAmmo laser = new Laser();

            Weapon pistol = new Pistol(bullet);
            Weapon rifle = new Rifle(laser);

            pistol.Shoot();
            rifle.Shoot();
        }
    }
}