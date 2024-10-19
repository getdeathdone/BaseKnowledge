using UnityEngine;

namespace BehavioralPatterns.TemplateMethod
{
    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            Enemy goblin = new GameObject("Goblin").AddComponent<Goblin>();
            goblin.PerformAction();

            Enemy orc = new GameObject("Orc").AddComponent<Orc>();
            orc.PerformAction();
        }
    }
}