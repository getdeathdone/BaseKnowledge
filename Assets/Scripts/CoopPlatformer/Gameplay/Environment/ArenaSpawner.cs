using Unity.Netcode;
using UnityEngine;

namespace CoopPlatformer.Gameplay.Environment
{
    [RequireComponent(typeof(NetworkObject))]
    public class ArenaSpawner : NetworkBehaviour
    {
        [Header("Arena Settings")] [SerializeField]
        private float _defaultRadius = 36f;

        [SerializeField] private int _starCount = 200;
        [SerializeField] private int _boundaryCount = 64;

        [Header("Prefabs & Sync")] [SerializeField]
        private GameObject _starPrefab;

        [SerializeField] private int _defaultSeed = 42;
        private readonly NetworkVariable<float> _activeRadius = new(36f);

        private readonly NetworkVariable<int> _activeSeed = new(42);

        private MaterialPropertyBlock _propBlock;
        private Mesh _quadMesh;

        public float CurrentRadius => _activeRadius.Value;

        private void Awake()
        {
            _propBlock = new MaterialPropertyBlock();
            _quadMesh = CreateQuadMesh();
        }

        private Mesh CreateQuadMesh()
        {
            var mesh = new Mesh();
            mesh.vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0), new Vector3(-0.5f, 0.5f, 0),
                new Vector3(0.5f, 0.5f, 0)
            };
            mesh.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
            mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            mesh.RecalculateNormals();
            return mesh;
        }

        private GameObject CreateVisualQuad(string name, Transform parent, Material material = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var filter = go.AddComponent<MeshFilter>();
            filter.mesh = _quadMesh;
            var renderer = go.AddComponent<MeshRenderer>();


            if (material == null)
            {
                material = Canvas.GetDefaultCanvasMaterial();
                if (material == null) material = new Material(Shader.Find("Sprites/Default"));
            }

            renderer.sharedMaterial = material;
            return go;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _activeSeed.Value = _defaultSeed != 0 ? _defaultSeed : Random.Range(1, 9999);
                _activeRadius.Value = _defaultRadius;
            }

            GenerateArena(_activeSeed.Value, _activeRadius.Value);
        }

        private void GenerateArena(int seed, float radius)
        {
            Random.InitState(seed);

            var existing = transform.Find("ArenaRoot");
            if (existing != null)
            {
                if (Application.isPlaying) Destroy(existing.gameObject);
                else DestroyImmediate(existing.gameObject);
            }

            var arenaRoot = new GameObject("ArenaRoot");
            arenaRoot.transform.SetParent(transform);

            SetupStars(arenaRoot.transform, radius);
            SetupBoundaries(arenaRoot.transform, radius);

            Debug.Log($"[ArenaSpawner] Created arena. Radius: {radius}, Seed: {seed}");
        }

        private void SetupStars(Transform parent, float radius)
        {
            for (var i = 0; i < _starCount; i++)
            {
                GameObject star;
                if (_starPrefab != null)
                    star = Instantiate(_starPrefab, parent);
                else
                    star = CreateVisualQuad($"Star_{i:000}", parent);

                star.transform.position = new Vector3(
                    Random.Range(-radius, radius),
                    Random.Range(-radius, radius),
                    5f);

                var scale = Random.Range(0.08f, 0.22f);
                star.transform.localScale = new Vector3(scale, scale, 1f);


                var rnd = star.GetComponentInChildren<Renderer>();
                if (rnd != null)
                {
                    var color = Color.yellow;

                    rnd.GetPropertyBlock(_propBlock);
                    _propBlock.SetColor("_Color", color);
                    _propBlock.SetColor("_BaseColor", color);
                    _propBlock.SetColor("_MainColor", color);
                    rnd.SetPropertyBlock(_propBlock);
                }
            }
        }

        private void SetupBoundaries(Transform parent, float radius)
        {
            for (var i = 0; i < _boundaryCount; i++)
            {
                var marker = CreateVisualQuad($"Boundary_{i:00}", parent);

                var angle = i / (float)_boundaryCount * Mathf.PI * 2f;
                var position = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;

                marker.transform.position = position;
                marker.transform.rotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
                marker.transform.localScale = new Vector3(0.5f, 2f, 1f);

                var rnd = marker.GetComponentInChildren<Renderer>();
                if (rnd != null)
                {
                    var color = new Color(0.2f, 0.9f, 1f, 1f);
                    rnd.GetPropertyBlock(_propBlock);
                    _propBlock.SetColor("_Color", color);
                    _propBlock.SetColor("_BaseColor", color);
                    _propBlock.SetColor("_MainColor", color);
                    rnd.SetPropertyBlock(_propBlock);
                }
            }
        }
    }
}