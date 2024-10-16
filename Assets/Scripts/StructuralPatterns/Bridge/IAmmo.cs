using UnityEngine;

namespace StructuralPatterns.Bridge
{
    public interface IAmmo
    {
        void Fire();
    }

    public class Bullet : IAmmo
    {
        public void Fire()
        {
            Debug.Log("Firing a bullet");
        }
    }

    public class Laser : IAmmo
    {
        public void Fire()
        {
            Debug.Log("Firing a laser");
        }
    }
}