using UnityEngine;

namespace StructuralPatterns.Bridge
{
    public abstract class Weapon
    {
        protected IAmmo _ammo;

        public Weapon(IAmmo ammo)
        {
            _ammo = ammo;
        }

        public abstract void Shoot();
    }

    public class Rifle : Weapon
    {
        public Rifle(IAmmo ammo) : base(ammo)
        {
        }

        public override void Shoot()
        {
            Debug.Log("Rifle shooting...");
            _ammo.Fire();
        }
    }

    public class Pistol : Weapon
    {
        public Pistol(IAmmo ammo) : base(ammo)
        {
        }

        public override void Shoot()
        {
            Debug.Log("Pistol shooting...");
            _ammo.Fire();
        }
    }
}