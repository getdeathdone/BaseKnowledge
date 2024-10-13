namespace StructuralPatterns.Decorator
{
    public abstract class Weapon
    {
        public abstract string GetDescription();
        public abstract int GetDamage();
    }

    public class Sword : Weapon
    {
        public override string GetDescription()
        {
            return "Sword";
        }

        public override int GetDamage()
        {
            return 10;
        }
    }

    public abstract class WeaponDecorator : Weapon
    {
        protected Weapon _weapon;

        public WeaponDecorator(Weapon weapon)
        {
            _weapon = weapon;
        }

        public override string GetDescription()
        {
            return _weapon.GetDescription();
        }

        public override int GetDamage()
        {
            return _weapon.GetDamage();
        }
    }

    public class FireEnchantment : WeaponDecorator
    {
        public FireEnchantment(Weapon weapon) : base(weapon)
        {
        }

        public override string GetDescription()
        {
            return _weapon.GetDescription() + " with Fire";
        }

        public override int GetDamage()
        {
            return _weapon.GetDamage() + 5; // Добавляем 5 урона от огня
        }
    }
}