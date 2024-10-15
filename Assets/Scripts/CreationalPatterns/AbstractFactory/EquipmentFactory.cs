using UnityEngine;

namespace CreationalPatterns.AbstractFactory
{
    public abstract class Weapon
    {
        public abstract void Use();
    }

    public abstract class Armor
    {
        public abstract void Equip();
    }

    public class ElfWeapon : Weapon
    {
        public override void Use()
        {
            Debug.Log("Using Elf weapon.");
        }
    }

    public class ElfArmor : Armor
    {
        public override void Equip()
        {
            Debug.Log("Equipping Elf armor.");
        }
    }

    public class OrcWeapon : Weapon
    {
        public override void Use()
        {
            Debug.Log("Using Orc weapon.");
        }
    }

    public class OrcArmor : Armor
    {
        public override void Equip()
        {
            Debug.Log("Equipping Orc armor.");
        }
    }


    public abstract class EquipmentFactory
    {
        public abstract Weapon CreateWeapon();
        public abstract Armor CreateArmor();
    }

    public class ElfEquipmentFactory : EquipmentFactory
    {
        public override Weapon CreateWeapon()
        {
            return new ElfWeapon();
        }

        public override Armor CreateArmor()
        {
            return new ElfArmor();
        }
    }

    public class OrcEquipmentFactory : EquipmentFactory
    {
        public override Weapon CreateWeapon()
        {
            return new OrcWeapon();
        }

        public override Armor CreateArmor()
        {
            return new OrcArmor();
        }
    }
}