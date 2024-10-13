namespace ArchitecturalPatterns.ModelViewPresenter
{
    public interface IPlayerView
    {
        void UpdateHealth(int health);
        void UpdateScore(int score);
    }
}