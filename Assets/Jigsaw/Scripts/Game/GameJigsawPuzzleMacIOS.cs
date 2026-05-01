using UnityEngine;

namespace Jigsaw.Scripts.Game
{
    public class GameJigsawPuzzleMacIOS : GameJigsawPuzzle
    {
        protected override void ActivatePiece(GameObject piece)
        {
            piece.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f);
        }

        protected override void DeactivatePiece(GameObject piece)
        {
            piece.GetComponent<Renderer>().material.color = new Color(0.8f, 0.8f, 0.8f);
        }

        protected override void PiecePlaced(GameObject piece)
        {
            piece.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f);
        }
    }
}