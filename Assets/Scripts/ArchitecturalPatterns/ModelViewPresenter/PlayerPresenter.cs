namespace ArchitecturalPatterns.ModelViewPresenter
{
    public class PlayerPresenter
    {
        private readonly PlayerModel playerModel;
        private readonly IPlayerView view;

        public PlayerPresenter(IPlayerView view)
        {
            this.view = view;
            playerModel = new PlayerModel { Health = 100, Score = 0 };
            UpdateView();
        }

        public void TakeDamage(int damage)
        {
            playerModel.Health -= damage;
            UpdateView();
        }

        public void AddScore(int points)
        {
            playerModel.Score += points;
            UpdateView();
        }

        private void UpdateView()
        {
            view.UpdateHealth(playerModel.Health);
            view.UpdateScore(playerModel.Score);
        }
    }
}