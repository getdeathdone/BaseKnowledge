using System.Collections;
using UnityEngine;

// The JigsawMain Class is used to access the (blender - fbx) imported piece prototypes and 
// to access materials for lines, sample image, scattered pieces and piece bases.
//
// You need to attach this script to an object in your scene. easiest way is to add the JigsawMain prefab
// to you scene.
//
namespace Jigsaw.Scripts.Core
{
    public class JigsawMain : MonoBehaviour
    {
        // ---------------------------------------------------------------------------------------------------------
        // public attributes
        // ---------------------------------------------------------------------------------------------------------
        public GameObject jigsaw; // 	will contain the FBX imported blender file with all (25*9) puzzle peaces
        public Material linesHorizontal; //	material for horizontal lines
        public Material linesVertical; //	material for vertical ines
        public Material sampleImage; //	material for sample image
        public Material scatteredPieces; //	material for scattered pieces
        public Material pieceBase; //	material for piece bases
        public int layerMask = 31; //	default layer mask for quick RayCasting


        // ---------------------------------------------------------------------------------------------------------
        // private attributes
        // ---------------------------------------------------------------------------------------------------------
        private readonly Hashtable basePieceTransforms = new();

        // is true if this class has been initialized correctly
        public bool isValid { get; private set; }


        // ---------------------------------------------------------------------------------------------------------
        // methods
        // ---------------------------------------------------------------------------------------------------------

        // Use this for initialization
        private void Start()
        {
            // load references to all puzzle piece Transform objects into HashTable
            GetBasePieces();
        }

        // get base piece prototype
        //获得基本块原型
        public Transform GetBase(string ident)
        {
            return jigsaw.transform.Find(ident);
        }

        // get specific piece prototype
        //获得特殊块原型
        public Transform GetPiece(string ident, string piece)
        {
            var basePiece = basePieceTransforms[ident] as Transform;
            if (basePiece != null)
                return basePiece.Find(ident + piece.ToUpper());
            return null;
        }

        // Load all 25 base puzzle pieces into HashTable
        //加载25个拼图方块
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