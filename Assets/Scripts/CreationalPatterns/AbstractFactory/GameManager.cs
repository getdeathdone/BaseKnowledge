using UnityEngine;

namespace CreationalPatterns.AbstractFactory
{
    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            EquipmentFactory elfFactory = new ElfEquipmentFactory();
            var elfWeapon = elfFactory.CreateWeapon();
            var elfArmor = elfFactory.CreateArmor();

            elfWeapon.Use();
            elfArmor.Equip();

            EquipmentFactory orcFactory = new OrcEquipmentFactory();
            var orcWeapon = orcFactory.CreateWeapon();
            var orcArmor = orcFactory.CreateArmor();

            orcWeapon.Use();
            orcArmor.Equip();
        }
    }
}