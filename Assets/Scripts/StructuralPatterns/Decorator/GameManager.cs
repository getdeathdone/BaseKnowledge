using UnityEngine;

namespace StructuralPatterns.Decorator
{
    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            Weapon sword = new Sword();
            Debug.Log(sword.GetDescription());
            Debug.Log(sword.GetDamage());

            Weapon fireSword = new FireEnchantment(sword);
            Debug.Log(fireSword.GetDescription());
            Debug.Log(fireSword.GetDamage());
        }
    }
}