namespace CreationalPatterns.Builder
{
    public class StandardHouseBuilder : HouseBuilder
    {
        public override void BuildWindows()
        {
            house.Windows = 4;
        }

        public override void BuildDoors()
        {
            house.Doors = 2;
        }

        public override void BuildGarage()
        {
            house.HasGarage = true;
        }

        public override void BuildGarden()
        {
            house.HasGarden = false;
        }
    }
}