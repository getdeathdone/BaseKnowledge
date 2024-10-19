using UnityEngine;

namespace BehavioralPatterns.Visitor
{
    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            var goblin = new GameObject("Goblin").AddComponent<Goblin>();
            var orc = new GameObject("Orc").AddComponent<Orc>();

            var damageVisitor = new DamageVisitor();

            goblin.Accept(damageVisitor);
            orc.Accept(damageVisitor);
        }
    }
}