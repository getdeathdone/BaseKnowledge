namespace CreationalPatterns.Builder
{
    public abstract class HouseBuilder
    {
        protected House house = new();

        public abstract void BuildWindows();
        public abstract void BuildDoors();
        public abstract void BuildGarage();
        public abstract void BuildGarden();

        public House GetHouse()
        {
            return house;
        }
    }
}