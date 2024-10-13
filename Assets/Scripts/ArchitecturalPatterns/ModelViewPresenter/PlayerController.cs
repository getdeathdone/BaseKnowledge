using UnityEngine;

namespace ArchitecturalPatterns.ModelViewPresenter
{
    public class PlayerController : MonoBehaviour
    {
        private PlayerPresenter presenter;

        private void Start()
        {
            var view = GetComponent<PlayerView>();
            presenter = new PlayerPresenter(view);
        }

        public void TakeDamage()
        {
            presenter.TakeDamage(10);
        }

        public void AddScore()
        {
            presenter.AddScore(5);
        }
    }
}