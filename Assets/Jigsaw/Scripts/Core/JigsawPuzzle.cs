using System;
using System.Collections;
using System.Collections.Generic;
using Jigsaw.Scripts.Networking;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Jigsaw.Scripts.Core
{
    [Serializable]
    public class JigsawPuzzle : MonoBehaviour
    {
        public Texture image; 

        public Vector2 size = new(5, 5); 

        public string topLeftPiece = "11"; 

        public bool outsideSnapping = true; 
        public bool showImage = true; 
        public bool showLines = true; 

        public int placePrecision = 12; 

        public bool scatterPieces = true; 

        public float spacing = 0.01f; 
        private JigsawNetworkSync _networkSync;

        private GameObject activePiece;
        private Vector3 activePoint;
        private Vector2 checkContainerSize;
        private bool checkedOnce;

        private Vector2 checkSize;
        private string checkTopLeftPiece = "";

        private Dictionary<GameObject, List<GameObject>> connectedPieces = new();
        private bool dragging;
        private Touch dragTouch;
        private GameObject linesH;
        private GameObject linesV;

        private JigsawMain main;
        private GameObject[,] neighborGrid;
        private Dictionary<GameObject, Vector2> neighborGridLookup = new();
        private GameObject pieceCache;
        private Hashtable piecePositions = new();

        private List<GameObject> pieces = new();
        private GameObject piecesContainer;

        private Hashtable piecesLookup = new();
        private GameObject puzzleContainer;
        private int puzzleMode;
        private int puzzleMoves;
        private GameObject puzzlePlane;
        private int puzzleTicks;
        private bool restarting;

        private bool rotatePieces = false; 
        private GameObject sampleImage;

        protected int pieceCount => (int)(size.x * size.y);

        protected int piecesPlaced => puzzleContainer.transform.childCount;

        protected int piecesScattered => piecesContainer.transform.childCount;

        protected float puzzleProgress => piecesPlaced / pieceCount * 100;

        protected void Update()
        {
            if (main == null)
            {
                if (!checkedOnce)
                {
                    var go = GameObject.Find("JigsawMain");
                    if (go == null)
                    {
                        Debug.LogError("JigsawMain (prefab) GameObject not added to scene!");
                        checkedOnce = true;
                        return;
                    }

                    main = go.GetComponent<JigsawMain>();
                    if (main != null)
                        if (!main.isValid)
                        {
                            Debug.LogError("JigsawMain (prefab) GameObject is not valid - Base puzzle pieces could not be found!");
                            main = null;
                            checkedOnce = true;
                            return;
                        }

                    if (main.GetBase(topLeftPiece) == null)
                        topLeftPiece = "11";

                    SetLines();
                    SetSample();
                    SetPieces(false);
                    SetPlane();
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (!Equals(size, checkSize) || topLeftPiece != checkTopLeftPiece || restarting)
                {
                    if (activePiece)
                    {
                        DeactivatePiece(activePiece);
                        activePiece = null;
                    }

                    SetLines();
                    SetSample();
                    SetPieces(true);
                    puzzleMode = 0;
                }

                if (linesH.activeSelf != showLines) linesH.SetActive(showLines);
                if (linesV.activeSelf != showLines) linesV.SetActive(showLines);
                if (sampleImage.activeSelf != showImage) sampleImage.SetActive(showImage);

                if (_networkSync == null) _networkSync = GetComponent<JigsawNetworkSync>();

                switch (puzzleMode)
                {
                    case 0: 
                        if (pieceCount == 0) return;

                        if (scatterPieces)
                            ScatterPieces();

                        if (rotatePieces)
                            RotatePieces();

                        puzzleMoves = 0;
                        puzzleTicks = Environment.TickCount;
                        restarting = false;
                        PuzzleStart();
                        puzzleMode++;
                        break;

                    case 1: 

                        if (Input.GetMouseButton(0))
                        {
                            if (Input.touchCount > 0 && !dragging)
                                dragTouch = Input.touches[0];

                            if (Input.touchCount == 0 ||
                                (Input.touchCount > 0 && dragTouch.fingerId == Input.touches[0].fingerId))
                            {
                                var position = Input.mousePosition;
                                if (Input.touchCount > 0)
                                    position = Input.touches[0].position;
                                if (activePiece != null)
                                {
                                    RaycastHit hit;
                                    if (puzzlePlane.GetComponent<Collider>().Raycast(Camera.main.ScreenPointToRay(position),
                                            out hit,
                                            Vector3.Distance(Camera.main.transform.position, transform.position) * 2))
                                    {
                                        var d = hit.point - activePoint;
                                        activePiece.transform.position += d;

                                        if (_networkSync != null)
                                            _networkSync.UpdatePiecePositionServerRpc(activePiece.name,
                                                activePiece.transform.position);

                                        if (connectedPieces.ContainsKey(activePiece))
                                            foreach (var piece in connectedPieces[activePiece])
                                                piece.transform.position += d;

                                        activePoint = hit.point;
                                    }
                                }
                                else
                                {
                                    var hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(position),
                                        Vector3.Distance(Camera.main.transform.position, transform.position) * 2,
                                        1 << main.layerMask);
                                    if (hits.Length > 0)
                                    {
                                        for (var h = 0; h < hits.Length; h++)
                                            if (hits[h].collider.gameObject == puzzlePlane)
                                                activePoint = hits[h].point;
                                            else if (hits[h].collider.gameObject.transform.parent.gameObject ==
                                                     piecesContainer)
                                                if ((activePiece != null && activePiece.transform.position.z >
                                                        hits[h].collider.gameObject.transform.position.z) ||
                                                    activePiece == null)
                                                {
                                                    var candidate = hits[h].collider.gameObject;
                                                    if (_networkSync != null && _networkSync.IsPieceLocked(candidate.name))
                                                        continue;
                                                    activePiece = candidate;
                                                }

                                        if (activePiece != null)
                                        {
                                            if (_networkSync != null) _networkSync.RequestDragServerRpc(activePiece.name);
                                            ActivatePiece(activePiece);
                                        }

                                        if (activePiece != null)
                                            if (connectedPieces.ContainsKey(activePiece))
                                                foreach (var piece in connectedPieces[activePiece])
                                                    ActivatePiece(piece);

                                        dragging = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            dragging = false;
                            if (activePiece != null)
                            {
                                DeactivatePiece(activePiece);

                                if (connectedPieces.ContainsKey(activePiece))
                                    foreach (var piece in connectedPieces[activePiece])
                                        DeactivatePiece(piece);

                                puzzleMoves++;

                                if (PieceInPlace())
                                {
                                    var isPlaced = true;
                                    activePiece.transform.position =
                                        PiecePosition((Vector2)piecePositions[activePiece.name]);
                                    activePiece.transform.parent = puzzleContainer.transform;

                                    if (_networkSync != null)
                                        _networkSync.ReleaseDragServerRpc(activePiece.name, activePiece.transform.position,
                                            isPlaced);

                                    PiecePlaced(activePiece);


                                    if (connectedPieces.ContainsKey(activePiece))
                                    {
                                        foreach (var piece in connectedPieces[activePiece])
                                        {
                                            piece.transform.position = PiecePosition((Vector2)piecePositions[piece.name]);
                                            piece.transform.parent = puzzleContainer.transform;
                                            PiecePlaced(piece);
                                            connectedPieces.Remove(piece);
                                        }

                                        connectedPieces.Remove(activePiece);
                                    }


                                    if (puzzleProgress == 100)
                                    {
                                        PuzzleSolved(puzzleMoves, (Environment.TickCount - puzzleTicks) / 1000);
                                        puzzleMode++;
                                    }
                                }
                                else
                                {
                                    var neighborChain = CheckNeighbors();
                                    if (neighborChain != null && outsideSnapping)
                                    {
                                        foreach (var go in neighborChain) Debug.Log("n: " + go.name);
                                        neighborChain.Add(activePiece);
                                        if (connectedPieces.ContainsKey(activePiece))
                                            neighborChain.AddRange(connectedPieces[activePiece]);

                                        foreach (var piece in neighborChain)
                                        {
                                            var chain = new List<GameObject>();
                                            chain.AddRange(neighborChain);
                                            chain.Remove(piece);
                                            if (connectedPieces.ContainsKey(piece))
                                                connectedPieces[piece] = chain;
                                            else
                                                connectedPieces.Add(piece, chain);
                                            chain = new List<GameObject>(); 
                                            chain.AddRange(connectedPieces[piece]);
                                            chain.Add(piece);
                                        }

                                        neighborChain.Remove(activePiece);
                                        foreach (var piece in neighborChain)
                                        {
                                            var diffDist = PiecePosition((Vector2)piecePositions[piece.name]) -
                                                           PiecePosition((Vector2)piecePositions[activePiece.name]);
                                            piece.transform.position = activePiece.transform.position + diffDist;
                                        }

                                        SnappedOutside(activePiece, neighborChain.ToArray());
                                    }

                                    if (_networkSync != null)
                                        _networkSync.ReleaseDragServerRpc(activePiece.name, activePiece.transform.position,
                                            false);
                                }

                                activePiece = null;
                            }
                        }

                        break;
                    case 2: 
                        break;
                }
            }
        }

        protected virtual void PuzzleStart()
        {
        }


        protected virtual void ActivatePiece(GameObject piece)
        {
        }


        protected virtual void DeactivatePiece(GameObject piece)
        {
        }


        protected virtual void PiecePlaced(GameObject piece)
        {
        }


        protected virtual void PuzzleSolved(int moves, float time)
        {
        }

        protected virtual void SnappedOutside(GameObject piece, GameObject[] chain)
        {
        }


        protected virtual GameObject ScatterPiece(GameObject piece)
        {
            return null;
        }

        public void Restart()
        {
            restarting = true;
        }

        private bool PieceInPlace()
        {
            var dX = transform.localScale.x / size.x;
            var dY = transform.localScale.y / size.y;
            var dV = PiecePosition((Vector2)piecePositions[activePiece.name]) - activePiece.transform.position;
            var cV = new Vector3(dX, dY, 0);
            return Vector3.Distance(Vector3.zero, dV) / Vector3.Distance(Vector3.zero, cV) * 100 < placePrecision;
        }


        private List<GameObject> CheckNeighbors()
        {
            var neighbors = new List<GameObject>();
            var piecePos = neighborGridLookup[activePiece];

            var alreadyConnected = new List<GameObject>();
            if (connectedPieces.ContainsKey(activePiece)) alreadyConnected = connectedPieces[activePiece];

            if (piecePos.x > 1)
            {
                var neighbor = neighborGrid[(int)piecePos.x - 1, (int)piecePos.y];
                if (!alreadyConnected.Contains(neighbor)) neighbors.Add(neighbor);
            }

            if (piecePos.x < size.x)
            {
                var neighbor = neighborGrid[(int)piecePos.x + 1, (int)piecePos.y];
                if (!alreadyConnected.Contains(neighbor)) neighbors.Add(neighbor);
            }

            if (piecePos.y > 1)
            {
                var neighbor = neighborGrid[(int)piecePos.x, (int)piecePos.y - 1];
                if (!alreadyConnected.Contains(neighbor)) neighbors.Add(neighbor);
            }

            if (piecePos.y < size.y)
            {
                var neighbor = neighborGrid[(int)piecePos.x, (int)piecePos.y + 1];
                if (!alreadyConnected.Contains(neighbor)) neighbors.Add(neighbor);
            }


            if (neighbors.Count == 0)
            {
                Debug.Log("no neighbors");
                return null;
            }

            var closest = neighbors[0];
            var dist = Vector3.Distance(neighbors[0].transform.position, activePiece.transform.position);
            if (neighbors.Count > 1)
                for (var cnt = 1; cnt < neighbors.Count; cnt++)
                {
                    var d = Vector3.Distance(neighbors[cnt].transform.position, activePiece.transform.position);
                    if (d < dist)
                    {
                        dist = d;
                        closest = neighbors[cnt];
                    }
                }

            var dX = transform.localScale.x / size.x;
            var dY = transform.localScale.y / size.y;
            var dV = closest.transform.position - activePiece.transform.position;
            var dV2 = PiecePosition((Vector2)piecePositions[closest.name]) -
                      PiecePosition((Vector2)piecePositions[activePiece.name]);
            var cV = new Vector3(dX, dY, 0);

            var validDist = Vector3.Distance(Vector3.zero, dV - dV2) / Vector3.Distance(Vector3.zero, cV) * 100;
            Debug.Log(validDist + " : " + placePrecision);
            var valid = validDist < placePrecision;

            if (!valid) return null;

            neighbors.Clear();
            neighbors.Add(closest);
            if (connectedPieces.ContainsKey(closest)) neighbors.AddRange(connectedPieces[closest]);
            return neighbors;
        }

        private void SetLinesHorizontal()
        {
            if (topLeftPiece.Length != 2) return;
            var tpX = Convert.ToInt32(topLeftPiece.Substring(1, 1));
            var tpY = Convert.ToInt32(topLeftPiece.Substring(0, 1));
            if (linesH != null) Destroy(linesH);
            linesH = GameObject.CreatePrimitive(PrimitiveType.Cube);
            linesH.name = "lines-horizontal";
            linesH.transform.parent = gameObject.transform;
            linesH.GetComponent<Renderer>().material = main.linesHorizontal;
            linesH.transform.localScale = new Vector3(-1, -1 * (1 / size.y) * (size.y - 1), 0.0001F);
            linesH.transform.rotation = transform.rotation;
            linesH.transform.position = transform.position +
                                        transform.forward * (transform.localScale.z / 2 + 0.001F);
            linesH.GetComponent<Renderer>().material.mainTextureScale = new Vector2(-0.2F * size.x, -0.2F * (size.y - 1));
            linesH.GetComponent<Renderer>().material.mainTextureOffset =
                new Vector2((5 - size.x) * -0.2F + (tpX - 1) * 0.2F, 0.005F + (tpY - 1) * -0.2F);
            linesH.SetActive(false);
        }

        private void SetLinesVertical()
        {
            if (topLeftPiece.Length != 2) return;
            var tpX = Convert.ToInt32(topLeftPiece.Substring(1, 1));
            var tpY = Convert.ToInt32(topLeftPiece.Substring(0, 1));
            if (linesV != null) Destroy(linesV);
            linesV = GameObject.CreatePrimitive(PrimitiveType.Cube);
            linesV.name = "lines-vertical";
            linesV.transform.parent = gameObject.transform;
            linesV.GetComponent<Renderer>().material = main.linesVertical;
            linesV.transform.localScale = new Vector3(-1 * (1 / size.x) * (size.x - 1), -1, 0.0001F);
            linesV.transform.rotation = transform.rotation;
            linesV.transform.position = transform.position +
                                        transform.forward * (transform.localScale.z / 2 + 0.001F);
            linesV.GetComponent<Renderer>().material.mainTextureScale = new Vector2(-0.2F * (size.x - 1), -0.2F * size.y);
            linesV.GetComponent<Renderer>().material.mainTextureOffset =
                new Vector2(-0.2F * (5 - size.x + 1) + (tpX - 1) * 0.2F, 0 + +((tpY - 1) * -0.2F));
            linesV.SetActive(false);
        }

        private void SetLines()
        {
            SetLinesHorizontal();
            SetLinesVertical();
            checkSize = size;
            checkTopLeftPiece = topLeftPiece;
        }

        private void RotatePieces()
        {
            var s = transform.localScale;
            var dX = s.x / size.x;
            var dY = s.y / size.y;
            for (var p = 0; p < pieces.Count; p++)
            {
                var piece = pieces[p];
                piece.transform.parent = null;
                piece.transform.RotateAround(
                    piece.transform.position + transform.right * -1 * (dX / 2) + transform.up * -1 * (dY / 2),
                    transform.forward, Random.value * 360);
            }
        }

        private void ScatterPieces()
        {
            var piecesToScatter = new ArrayList(pieces);
            while (piecesToScatter.Count > 0)
            {
                var piece = piecesToScatter[(int)Mathf.Floor(Random.value * piecesToScatter.Count)] as GameObject;
                piecesToScatter.Remove(piece);

                if (ScatterPiece(piece) != null)
                    continue;

                var r = new Rect();

                var p = transform.position;
                var s = transform.localScale;
                var dX = s.x / size.x;
                var dY = s.y / size.y;

                var si = s.x / 4;
                if (s.y / 4 < si) si = s.y / 4;

                switch ((int)Mathf.Floor(Random.value * 4) + 1)
                {
                    case 1: 
                        r = new Rect(p.x - s.x / 2 - dX / 2 + s.x * 0.1f, p.y + s.y / 2 + dY + si - dY / 2, s.x * 0.8f,
                            -1 * (si - dY));
                        break;
                    case 2: 
                        r = new Rect(p.x + s.x / 2 + dX - dX / 2, p.y + s.y / 2 - s.y * 0.1f, si - dX, -1 * s.y * 0.8f);
                        break;
                    case 3: 
                        r = new Rect(p.x - s.x / 2 - dX / 2 + s.x * 0.1f, p.y - s.y / 2 - dY / 2, s.x * 0.8f,
                            -1 * (si - dY));
                        break;
                    case 4: 
                        r = new Rect(p.x - s.x / 2 - dX - si + dX / 2, p.y + s.y / 2 - s.y * 0.1f, si - dX,
                            -1 * s.y * 0.8f);
                        break;
                }

                piece.transform.parent = piecesContainer.transform;

                piece.transform.position =
                    transform.position +
                    transform.right * -1 * r.xMin +
                    transform.right * -1 * Random.value * r.width +
                    transform.up * r.yMin +
                    transform.up * Random.value * r.height +
                    transform.forward * (transform.localScale.z / 2 + 0.001f) +
                    transform.forward * (0.004f + 0.001F * Random.value * 20);

                if (transform.parent != null)
                {
                    Vector2 vp = transform.parent.localToWorldMatrix.MultiplyPoint3x4(transform.localPosition);
                    piece.transform.position -= (Vector3)vp;
                }
            }
        }

        private void SetPlane()
        {
            puzzlePlane = GameObject.CreatePrimitive(PrimitiveType.Cube);
            puzzlePlane.name = "puzzlePlane";
            puzzlePlane.transform.parent = transform;
            puzzlePlane.transform.rotation = transform.rotation;
            puzzlePlane.transform.localScale = new Vector3(10, 10, 0.0001F);
            puzzlePlane.transform.position = transform.position +
                                             transform.forward * (transform.localScale.z / 2 + 0.0004F);
            puzzlePlane.layer = main.layerMask;
            Destroy(puzzlePlane.GetComponent("MeshRenderer"));
        }

        private void SetSample()
        {
            if (sampleImage != null) Destroy(sampleImage);
            sampleImage = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sampleImage.name = "sampleImage";
            sampleImage.transform.parent = gameObject.transform;
            sampleImage.transform.localScale = new Vector3(1, 1, 0.0001F);
            sampleImage.transform.rotation = transform.rotation;
            sampleImage.transform.position = transform.position +
                                             transform.forward * (transform.localScale.z / 2 + 0.0005F);
            sampleImage.GetComponent<Renderer>().material = main.sampleImage;
            sampleImage.GetComponent<Renderer>().material.mainTexture = image;
            sampleImage.GetComponent<Renderer>().material.mainTextureOffset = Vector2.zero;
            sampleImage.SetActive(false);
        }

        private void CreateContainers()
        {
            if (piecesContainer != null) Destroy(piecesContainer);
            if (puzzleContainer != null) Destroy(puzzleContainer);
            if (pieceCache != null) Destroy(pieceCache);

            piecesContainer = new GameObject("piecesContainer");
            piecesContainer.transform.parent = gameObject.transform;
            piecesContainer.transform.rotation = transform.rotation;
            piecesContainer.transform.localScale = transform.localScale;
            piecesContainer.transform.position = transform.position;

            puzzleContainer = new GameObject("puzzleContainer");
            puzzleContainer.transform.parent = gameObject.transform;
            puzzleContainer.transform.rotation = transform.rotation;
            puzzleContainer.transform.localScale = transform.localScale;
            puzzleContainer.transform.position = transform.position;

            pieceCache = new GameObject("pieceCache");
            pieceCache.transform.parent = gameObject.transform;
            pieceCache.transform.rotation = transform.rotation;
            pieceCache.transform.localScale = transform.localScale;
            pieceCache.transform.position = transform.position;
        }

        private string GetType(Vector2 pos)
        {
            var x = pos.x;
            var y = pos.y;

            var pt = "C";
            if (y == 1)
            {
                if (x == 1) pt = "TL";
                else if (x == size.x) pt = "TR";
                else
                    pt = "T";
            }
            else if (y == size.y)
            {
                if (x == 1) pt = "BL";
                else if (x == size.x) pt = "BR";
                else
                    pt = "B";
            }
            else if (x == 1)
            {
                pt = "L";
            }
            else if (x == size.x)
            {
                pt = "R";
            }

            return pt;
        }

        private Vector3 PiecePosition(Vector3 pos)
        {
            var dX = transform.localScale.x / size.x;
            var dY = transform.localScale.y / size.y;

            var positionVector =
                (transform.localScale.x / 2 * -1 + dX * (pos.x - 1) + dX * (spacing / 2)) * transform.right * -1 +
                (transform.localScale.y / 2 - dY * (pos.y - 1) - dY * (spacing / 2)) * transform.up;

            return transform.position +
                   transform.forward * (transform.localScale.z / 2 + 0.001f) +
                   positionVector;
        }

        private void InitPiece(GameObject puzzlePiece, Vector2 pos)
        {
            var scale5x5 = Vector3.Scale(new Vector3(11.45f, 11.45f, 36.14547f), transform.localScale);
            var CxR = new Vector3(1 / (0.2F * size.x), 1 / (0.2F * size.y), 25 / (size.x * size.y));
            puzzlePiece.transform.parent = null;
            puzzlePiece.transform.localScale = Vector3.Scale(scale5x5 * (1 - spacing), CxR);
            puzzlePiece.transform.rotation = transform.rotation;
            puzzlePiece.transform.parent = piecesContainer.transform;
            puzzlePiece.transform.position = PiecePosition(pos);
            piecePositions.Add(puzzlePiece.name, pos);
            var scaleX = 1 / (0.2F * size.x);
            var scaleY = 1 / (0.2F * size.y);
            puzzlePiece.GetComponent<Renderer>().material = main.scatteredPieces;
            puzzlePiece.GetComponent<Renderer>().materials[1] = main.pieceBase;
            puzzlePiece.GetComponent<Renderer>().material.mainTexture = image;
            puzzlePiece.GetComponent<Renderer>().material.mainTextureScale = new Vector2(scaleX, scaleY);
            puzzlePiece.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.2F * scaleX * (pos.x - 1),
                -0.2F * scaleY * (pos.y - 1 + (5 - size.y)));
        }

        private GameObject CreateNewPiece(Vector2 piece, Vector2 pos, string pType)
        {
            GameObject puzzlePiece = null;
            var basePiece = main.GetPiece("" + piece.y + "" + piece.x, pType);
            if (basePiece != null)
            {
                puzzlePiece = Instantiate(basePiece.gameObject, new Vector3(pos.x * 2F, pos.y * -2F, 0),
                    Quaternion.Euler(new Vector3(0, 180, 0)));
                puzzlePiece.AddComponent<BoxCollider>();
            }

            puzzlePiece.layer = main.layerMask;
            return puzzlePiece;
        }

        private void SetPieces(bool recreate)
        {
            if (topLeftPiece.Length != 2) return;
            if (size.x <= 1 || size.y <= 1) return;

            if (!recreate)
                CreateContainers();
            else
                while (pieces.Count > 0)
                {
                    var p = pieces[0];
                    pieces.Remove(p);
                    piecePositions.Remove(p.name);
                    p.SetActive(false);
                    p.transform.parent = pieceCache.transform;
                }

            var tpX = Convert.ToInt32(topLeftPiece.Substring(1, 1));
            var tpY = Convert.ToInt32(topLeftPiece.Substring(0, 1));
            var bX = tpX;

            var idX = 1;
            var idY = 1;

            neighborGrid = new GameObject[(int)size.x + 1, (int)size.y + 1]; 
            neighborGridLookup = new Dictionary<GameObject, Vector2>();

            for (var y = 1; y <= size.y; y++)
            {
                for (var x = 1; x <= size.x; x++)
                {
                    var pType = GetType(new Vector2(x, y));
                    var puzzlePiece = piecesLookup["" + tpY + tpX + pType + "-" + idX] as GameObject;
                    if (puzzlePiece != null)
                        while (puzzlePiece != null && puzzlePiece.activeSelf)
                        {
                            idX++;
                            puzzlePiece = piecesLookup["" + tpY + tpX + pType + "-" + idX] as GameObject;
                        }

                    if (puzzlePiece != null)
                    {
                        puzzlePiece.name = "" + tpY + tpX + pType + "-" + idX;
                        InitPiece(puzzlePiece, new Vector2(x, y));
                        pieces.Add(puzzlePiece);
                        puzzlePiece.SetActive(true);
                    }
                    else
                    {
                        puzzlePiece = CreateNewPiece(new Vector2(tpX, tpY), new Vector2(x, y), pType);
                        puzzlePiece.name = "" + tpY + tpX + pType + "-" + idX;
                        if (puzzlePiece != null)
                        {
                            InitPiece(puzzlePiece, new Vector2(x, y));
                            piecesLookup.Add(puzzlePiece.name, puzzlePiece);
                            pieces.Add(puzzlePiece);
                        }
                    }

                    tpX++;
                    if (tpX == bX + size.x || tpX == 6)
                    {
                        if (tpX == 6)
                        {
                            tpX = 1;
                            idX++;
                        }
                        else
                        {
                            tpX = bX;
                        }
                    }

                    neighborGrid[x, y] = puzzlePiece;
                    neighborGridLookup.Add(puzzlePiece, new Vector2(x, y));
                }

                tpX = bX;
                idX = 1;
                tpY++;
                if (tpY == 6)
                {
                    tpY = 1;
                    idY++;
                }
            }
        }
    }
}