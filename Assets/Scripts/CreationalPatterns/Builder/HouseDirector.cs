namespace CreationalPatterns.Builder
{
    public class HouseDirector
    {
        private readonly HouseBuilder builder;

        public HouseDirector(HouseBuilder builder)
        {
            this.builder = builder;
        }

        public House ConstructHouse()
        {
            builder.BuildWindows();
            builder.BuildDoors();
            builder.BuildGarage();
            builder.BuildGarden();

            return builder.GetHouse();
        }
    }
}