using UnityEngine;

namespace CreationalPatterns.Builder
{
    public class House
    {
        public int Windows { get; set; }
        public int Doors { get; set; }
        public bool HasGarage { get; set; }
        public bool HasGarden { get; set; }

        public void Show()
        {
            Debug.Log($"House with {Windows} windows, {Doors} doors, garage: {HasGarage}, garden: {HasGarden}");
        }
    }
}