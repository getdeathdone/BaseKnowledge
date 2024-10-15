using UnityEngine;

namespace CreationalPatterns.Builder
{
    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            HouseBuilder builder = new StandardHouseBuilder();
            var director = new HouseDirector(builder);

            director.ConstructHouse();
            var house = director.GetHouse();
            house.Show();
        }
    }
}