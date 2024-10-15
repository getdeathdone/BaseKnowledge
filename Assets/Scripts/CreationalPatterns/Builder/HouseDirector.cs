namespace CreationalPatterns.Builder
{
    public class HouseDirector
    {
        private readonly HouseBuilder builder;

        public HouseDirector(HouseBuilder builder)
        {
            this.builder = builder;
        }

        public void ConstructHouse()
        {
            builder.BuildWindows();
            builder.BuildDoors();
            builder.BuildGarage();
            builder.BuildGarden();
        }

        public House GetHouse()
        {
            return builder.GetHouse();
        }
    }
}