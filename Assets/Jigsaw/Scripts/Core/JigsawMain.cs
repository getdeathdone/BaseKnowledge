using System.Collections;
using UnityEngine;

namespace Jigsaw.Scripts.Core
{
    public class JigsawMain : MonoBehaviour
    {
        public GameObject jigsaw; 
        public Material linesHorizontal; 
        public Material linesVertical; 
        public Material sampleImage; 
        public Material scatteredPieces; 
        public Material pieceBase; 
        public int layerMask = 31; 


        private readonly Hashtable basePieceTransforms = new();

        public bool isValid { get; private set; }


        private void Start()
        {
            GetBasePieces();
        }

        public Transform GetBase(string ident)
        {
            return jigsaw.transform.Find(ident);
        }

        public Transform GetPiece(string ident, string piece)
        {
            var basePiece = basePieceTransforms[ident] as Transform;
            if (basePiece != null)
                return basePiece.Find(ident + piece.ToUpper());
            return null;
        }

        private void GetBasePieces()
        {
            var aPieceNotFound = false;
            for (var px = 1; px <= 5; px++)
            for (var py = 1; py <= 5; py++)
            {
                var ident = "" + px + "" + py;
                var t = GetBase(ident);
                if (t == null)
                    aPieceNotFound = true;
                basePieceTransforms.Add(ident, t);
            }

            isValid = !aPieceNotFound;
        }
    }
}