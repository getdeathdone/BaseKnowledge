using Jigsaw.Scripts.Core;
using UnityEngine;

namespace Jigsaw.Scripts.Game
{
    public class GameJigsawPuzzle : JigsawPuzzle
    {
        private float _Color;
        private float _LightUp;

        public bool solved { get; private set; }

        public int moves { get; private set; }

        public float time { get; private set; }

        private void Start()
        {
        }

        private new void Update()
        {
            base.Update();
        }

        protected override void PuzzleStart()
        {
            solved = false;
        }

        protected override void ActivatePiece(GameObject piece)
        {
            _LightUp = piece.GetComponent<Renderer>().material.GetFloat("_LightUp");
            _Color = piece.GetComponent<Renderer>().material.GetFloat("_Color");
            piece.GetComponent<Renderer>().material.SetFloat("_LightUp", 0.01f);
            piece.GetComponent<Renderer>().material.SetFloat("_Color", 1);
        }

        protected override void DeactivatePiece(GameObject piece)
        {
            piece.GetComponent<Renderer>().material.SetFloat("_LightUp", _LightUp);
            piece.GetComponent<Renderer>().material.SetFloat("_Color", _Color);
        }

        protected override void PiecePlaced(GameObject piece)
        {
            piece.GetComponent<Renderer>().material.SetFloat("_LightUp", 0.01f);
            piece.GetComponent<Renderer>().material.SetFloat("_Color", 1f);
        }

        protected override void PuzzleSolved(int moves, float time)
        {
            solved = true;
            this.moves = moves;
            this.time = time;
        }

        protected override GameObject ScatterPiece(GameObject piece)
        {
            return null;
        }
    }
}