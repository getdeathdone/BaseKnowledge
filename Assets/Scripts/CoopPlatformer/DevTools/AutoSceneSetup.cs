using CoopPlatformer.Core;
using CoopPlatformer.Gameplay.Space;
using CoopPlatformer.UI;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoopPlatformer.DevTools
{
    public class AutoSceneSetup : MonoBehaviour
    {
        private const float ArenaRadius = 18f;
        private static readonly Vector3 PrefabStoragePosition = new Vector3(1000f, 1000f, 0f);

        private NetworkManager _networkManager;
        private NetworkShipController _shipPrefab;
        private NetworkProjectile _projectilePrefab;
        private AsteroidController _asteroidPrefab;

        [ContextMenu("Build Scene")]
        public void BuildScene()
        {
            Cleanup();
            SetupCamera();
            SetupArena();
            SetupNetworkLayer();
            SetupUI();

            Debug.Log("[AutoSetup] Space shooter scene constructed successfully.");
        }

        private void Cleanup()
        {
            string[] objectNames =
            {
                "NetworkManager",
                "ArenaRoot",
                "GeneratedPrefabs",
                "AsteroidSpawner",
                "LobbyCanvas",
                "EventSystem"
            };

            foreach (var objectName in objectNames)
            {
                var sceneObject = GameObject.Find(objectName);
                if (sceneObject != null)
                {
                    DestroyImmediate(sceneObject);
                }
            }
        }

        private void SetupCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }

            camera.orthographic = true;
            camera.orthographicSize = 12f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.backgroundColor = new Color(0.03f, 0.04f, 0.1f);
            camera.clearFlags = CameraClearFlags.SolidColor;
        }

        private void SetupArena()
        {
            var arenaRoot = new GameObject("ArenaRoot");

            for (var i = 0; i < 90; i++)
            {
                var star = GameObject.CreatePrimitive(PrimitiveType.Quad);
                star.name = $"Star_{i:00}";
                DestroyImmediate(star.GetComponent<Collider>());
                star.transform.SetParent(arenaRoot.transform);
                star.transform.position = new Vector3(
                    Random.Range(-ArenaRadius, ArenaRadius),
                    Random.Range(-ArenaRadius, ArenaRadius),
                    5f);

                var scale = Random.Range(0.08f, 0.22f);
                star.transform.localScale = new Vector3(scale, scale, 1f);
                star.GetComponent<MeshRenderer>().sharedMaterial.color = Color.Lerp(Color.white, new Color(0.45f, 0.7f, 1f), Random.value * 0.5f);
            }

            for (var i = 0; i < 32; i++)
            {
                var marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
                marker.name = $"Boundary_{i:00}";
                DestroyImmediate(marker.GetComponent<Collider>());
                marker.transform.SetParent(arenaRoot.transform);

                var angle = i / 32f * Mathf.PI * 2f;
                var position = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * ArenaRadius;
                marker.transform.position = position;
                marker.transform.rotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
                marker.transform.localScale = new Vector3(0.3f, 1.2f, 1f);
                marker.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(0.2f, 0.9f, 1f, 0.85f);
            }
        }

        private void SetupNetworkLayer()
        {
            var generatedPrefabs = new GameObject("GeneratedPrefabs");
            generatedPrefabs.transform.position = PrefabStoragePosition;

            var networkManagerObject = new GameObject("NetworkManager");
            _networkManager = networkManagerObject.AddComponent<NetworkManager>();
            var transport = networkManagerObject.AddComponent<UnityTransport>();
            networkManagerObject.AddComponent<NetworkBootstrap>();

            _projectilePrefab = CreateProjectilePrefab(generatedPrefabs.transform);
            _asteroidPrefab = CreateAsteroidPrefab(generatedPrefabs.transform);
            _shipPrefab = CreateShipPrefab(generatedPrefabs.transform, _projectilePrefab);

            var config = new NetworkConfig
            {
                NetworkTransport = transport,
                PlayerPrefab = _shipPrefab.gameObject
            };
            config.Prefabs.Add(new NetworkPrefab { Prefab = _shipPrefab.gameObject });
            config.Prefabs.Add(new NetworkPrefab { Prefab = _projectilePrefab.gameObject });
            config.Prefabs.Add(new NetworkPrefab { Prefab = _asteroidPrefab.gameObject });
            _networkManager.NetworkConfig = config;

            var spawnerObject = new GameObject("AsteroidSpawner");
            spawnerObject.transform.position = Vector3.zero;
            spawnerObject.AddComponent<NetworkObject>();
            var spawner = spawnerObject.AddComponent<AsteroidSpawner>();
            SetPrivateField(spawner, "_asteroidPrefab", _asteroidPrefab);
        }

        private NetworkShipController CreateShipPrefab(Transform parent, NetworkProjectile projectilePrefab)
        {
            var shipObject = new GameObject("ShipPrefab");
            shipObject.transform.SetParent(parent);
            shipObject.AddComponent<NetworkObject>();
            shipObject.AddComponent<NetworkTransform>();

            var rigidbody2D = shipObject.AddComponent<Rigidbody2D>();
            rigidbody2D.gravityScale = 0f;

            var collider2D = shipObject.AddComponent<CircleCollider2D>();
            collider2D.isTrigger = true;
            collider2D.radius = 0.45f;

            var shipController = shipObject.AddComponent<NetworkShipController>();

            var hull = GameObject.CreatePrimitive(PrimitiveType.Quad);
            hull.name = "Hull";
            DestroyImmediate(hull.GetComponent<Collider>());
            hull.transform.SetParent(shipObject.transform);
            hull.transform.localPosition = Vector3.zero;
            hull.transform.localScale = new Vector3(0.6f, 1.1f, 1f);
            hull.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(0.2f, 0.95f, 1f);

            var wingLeft = GameObject.CreatePrimitive(PrimitiveType.Quad);
            wingLeft.name = "WingLeft";
            DestroyImmediate(wingLeft.GetComponent<Collider>());
            wingLeft.transform.SetParent(shipObject.transform);
            wingLeft.transform.localPosition = new Vector3(-0.45f, -0.1f, 0f);
            wingLeft.transform.localRotation = Quaternion.Euler(0f, 0f, 35f);
            wingLeft.transform.localScale = new Vector3(0.18f, 0.7f, 1f);
            wingLeft.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(0.95f, 0.4f, 0.2f);

            var wingRight = GameObject.CreatePrimitive(PrimitiveType.Quad);
            wingRight.name = "WingRight";
            DestroyImmediate(wingRight.GetComponent<Collider>());
            wingRight.transform.SetParent(shipObject.transform);
            wingRight.transform.localPosition = new Vector3(0.45f, -0.1f, 0f);
            wingRight.transform.localRotation = Quaternion.Euler(0f, 0f, -35f);
            wingRight.transform.localScale = new Vector3(0.18f, 0.7f, 1f);
            wingRight.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(0.95f, 0.4f, 0.2f);

            var engine = GameObject.CreatePrimitive(PrimitiveType.Quad);
            engine.name = "Engine";
            DestroyImmediate(engine.GetComponent<Collider>());
            engine.transform.SetParent(shipObject.transform);
            engine.transform.localPosition = new Vector3(0f, -0.72f, 0f);
            engine.transform.localScale = new Vector3(0.24f, 0.35f, 1f);
            engine.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(1f, 0.75f, 0.15f);

            var muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(shipObject.transform);
            muzzle.transform.localPosition = new Vector3(0f, 0.8f, 0f);

            SetPrivateField(shipController, "_projectilePrefab", projectilePrefab);
            SetPrivateField(shipController, "_muzzle", muzzle.transform);
            SetRenderersEnabled(shipObject, false);
            return shipController;
        }

        private NetworkProjectile CreateProjectilePrefab(Transform parent)
        {
            var projectileObject = new GameObject("ProjectilePrefab");
            projectileObject.transform.SetParent(parent);
            projectileObject.AddComponent<NetworkObject>();
            projectileObject.AddComponent<NetworkTransform>();

            var rigidbody2D = projectileObject.AddComponent<Rigidbody2D>();
            rigidbody2D.gravityScale = 0f;

            var collider2D = projectileObject.AddComponent<CircleCollider2D>();
            collider2D.isTrigger = true;
            collider2D.radius = 0.14f;

            var projectile = projectileObject.AddComponent<NetworkProjectile>();

            var bolt = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bolt.name = "Bolt";
            DestroyImmediate(bolt.GetComponent<Collider>());
            bolt.transform.SetParent(projectileObject.transform);
            bolt.transform.localPosition = Vector3.zero;
            bolt.transform.localScale = new Vector3(0.18f, 0.55f, 1f);
            bolt.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(1f, 0.95f, 0.35f);

            SetRenderersEnabled(projectileObject, false);
            return projectile;
        }

        private AsteroidController CreateAsteroidPrefab(Transform parent)
        {
            var asteroidObject = new GameObject("AsteroidPrefab");
            asteroidObject.transform.SetParent(parent);
            asteroidObject.AddComponent<NetworkObject>();
            asteroidObject.AddComponent<NetworkTransform>();

            var rigidbody2D = asteroidObject.AddComponent<Rigidbody2D>();
            rigidbody2D.gravityScale = 0f;

            var collider2D = asteroidObject.AddComponent<CircleCollider2D>();
            collider2D.isTrigger = true;
            collider2D.radius = 0.55f;

            var asteroid = asteroidObject.AddComponent<AsteroidController>();

            var rock = GameObject.CreatePrimitive(PrimitiveType.Quad);
            rock.name = "Rock";
            DestroyImmediate(rock.GetComponent<Collider>());
            rock.transform.SetParent(asteroidObject.transform);
            rock.transform.localPosition = Vector3.zero;
            rock.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
            rock.transform.localRotation = Quaternion.Euler(0f, 0f, 22f);
            rock.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(0.55f, 0.58f, 0.67f);

            SetRenderersEnabled(asteroidObject, false);
            return asteroid;
        }

        private void SetupUI()
        {
            var canvasObject = new GameObject("LobbyCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasObject.AddComponent<GraphicRaycaster>();

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();

            var panel = new GameObject("LobbyUI");
            panel.transform.SetParent(canvasObject.transform, false);
            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(0f, 0f);
            panelRect.pivot = new Vector2(0f, 0f);
            panelRect.anchoredPosition = new Vector2(30f, 30f);
            panelRect.sizeDelta = new Vector2(420f, 290f);
            panel.AddComponent<Image>().color = new Color(0.04f, 0.07f, 0.12f, 0.9f);

            var lobbyUI = panel.AddComponent<LobbyUI>();
            SetPrivateField(lobbyUI, "_bootstrap", _networkManager.GetComponent<NetworkBootstrap>());

            var title = CreateLabel("Title", "SPACE SHOOTER", 26, TextAlignmentOptions.TopLeft, panel.transform);
            ConfigureRect(title.rectTransform, new Vector2(20f, -18f), new Vector2(380f, 36f), new Vector2(0f, 1f));
            title.color = new Color(0.84f, 0.96f, 1f);

            var subtitle = CreateLabel("Subtitle", "PC hosts. Android joins with Lobby Code.", 16, TextAlignmentOptions.TopLeft, panel.transform);
            ConfigureRect(subtitle.rectTransform, new Vector2(20f, -54f), new Vector2(380f, 26f), new Vector2(0f, 1f));
            subtitle.color = new Color(0.63f, 0.72f, 0.82f);

            var status = CreateLabel("Status", "Host creates a room. Join uses Lobby Code.", 18, TextAlignmentOptions.TopLeft, panel.transform);
            ConfigureRect(status.rectTransform, new Vector2(20f, -92f), new Vector2(380f, 54f), new Vector2(0f, 1f));
            status.color = new Color(0.9f, 0.95f, 1f);
            SetPrivateField(lobbyUI, "_statusLabel", status);

            var inputLabel = CreateLabel("InputLabel", "Lobby Code", 16, TextAlignmentOptions.TopLeft, panel.transform);
            ConfigureRect(inputLabel.rectTransform, new Vector2(20f, -160f), new Vector2(180f, 24f), new Vector2(0f, 1f));
            inputLabel.color = new Color(0.63f, 0.72f, 0.82f);

            var inputField = CreateInputField(panel.transform);
            SetPrivateField(lobbyUI, "_joinCodeInput", inputField);

            var hostButton = CreateButton("HostButton", "Host", new Color(0.12f, 0.64f, 0.95f), new Vector2(20f, 28f), panel.transform);
            var clientButton = CreateButton("ClientButton", "Client", new Color(0.95f, 0.45f, 0.22f), new Vector2(150f, 28f), panel.transform);
            var hostClientButton = CreateButton("HostClientButton", "Host+Client", new Color(0.48f, 0.36f, 0.92f), new Vector2(280f, 28f), panel.transform);
            var refreshButton = CreateButton("RefreshButton", "Refresh Rooms", new Color(0.3f, 0.58f, 0.92f), new Vector2(20f, 88f), panel.transform);
            SetPrivateField(lobbyUI, "_hostButton", hostButton);
            SetPrivateField(lobbyUI, "_clientButton", clientButton);
            SetPrivateField(lobbyUI, "_hostClientButton", hostClientButton);
            SetPrivateField(lobbyUI, "_refreshButton", refreshButton);

            var roomListRoot = new GameObject("RoomListRoot");
            roomListRoot.transform.SetParent(panel.transform, false);
            var roomListRect = roomListRoot.AddComponent<RectTransform>();
            roomListRect.anchorMin = new Vector2(0f, 1f);
            roomListRect.anchorMax = new Vector2(0f, 1f);
            roomListRect.pivot = new Vector2(0f, 1f);
            roomListRect.anchoredPosition = new Vector2(20f, -250f);
            roomListRect.sizeDelta = new Vector2(380f, 150f);

            var roomListLayout = roomListRoot.AddComponent<VerticalLayoutGroup>();
            roomListLayout.childAlignment = TextAnchor.UpperLeft;
            roomListLayout.childControlWidth = true;
            roomListLayout.childControlHeight = true;
            roomListLayout.childForceExpandHeight = false;
            roomListLayout.spacing = 8f;

            var roomListEmpty = CreateLabel("RoomListEmpty", "No rooms loaded.", 16, TextAlignmentOptions.TopLeft, roomListRoot.transform);
            roomListEmpty.color = new Color(0.72f, 0.78f, 0.86f);
            var emptyLayout = roomListEmpty.gameObject.AddComponent<LayoutElement>();
            emptyLayout.preferredHeight = 26f;

            SetPrivateField(lobbyUI, "_roomListRoot", roomListRect);
            SetPrivateField(lobbyUI, "_roomListEmptyLabel", roomListEmpty);

            var overlay = new GameObject("LoadingOverlay");
            overlay.transform.SetParent(panel.transform, false);
            var overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            overlay.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);

            var overlayLabel = CreateLabel("LoadingText", "Working...", 22, TextAlignmentOptions.Center, overlay.transform);
            overlayLabel.rectTransform.anchorMin = Vector2.zero;
            overlayLabel.rectTransform.anchorMax = Vector2.one;
            overlayLabel.rectTransform.offsetMin = Vector2.zero;
            overlayLabel.rectTransform.offsetMax = Vector2.zero;
            overlayLabel.color = Color.white;

            overlay.SetActive(false);
            SetPrivateField(lobbyUI, "_loadingOverlay", overlay);

            SetupGameplayHud(canvasObject.transform);
        }

        private void SetupGameplayHud(Transform canvasTransform)
        {
            var hudRoot = new GameObject("GameplayHud");
            hudRoot.transform.SetParent(canvasTransform, false);
            var hudRect = hudRoot.AddComponent<RectTransform>();
            hudRect.anchorMin = new Vector2(1f, 1f);
            hudRect.anchorMax = new Vector2(1f, 1f);
            hudRect.pivot = new Vector2(1f, 1f);
            hudRect.anchoredPosition = new Vector2(-24f, -24f);
            hudRect.sizeDelta = new Vector2(300f, 180f);
            hudRoot.AddComponent<Image>().color = new Color(0.04f, 0.07f, 0.12f, 0.82f);

            var hud = hudRoot.AddComponent<GameplayHud>();

            var playersLabel = CreateLabel("Players", "Players: 0/4", 22, TextAlignmentOptions.TopLeft, hudRoot.transform);
            ConfigureRect(playersLabel.rectTransform, new Vector2(-20f, -18f), new Vector2(260f, 32f), new Vector2(1f, 1f));
            playersLabel.color = new Color(0.84f, 0.96f, 1f);

            var healthLabel = CreateLabel("Health", "Waiting for players...", 18, TextAlignmentOptions.TopLeft, hudRoot.transform);
            ConfigureRect(healthLabel.rectTransform, new Vector2(-20f, -56f), new Vector2(260f, 110f), new Vector2(1f, 1f));
            healthLabel.color = Color.white;

            SetPrivateField(hud, "_playersLabel", playersLabel);
            SetPrivateField(hud, "_healthLabel", healthLabel);
        }

        private TMP_InputField CreateInputField(Transform parent)
        {
            var inputRoot = new GameObject("JoinInput");
            inputRoot.transform.SetParent(parent, false);
            var rectTransform = inputRoot.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = new Vector2(20f, -188f);
            rectTransform.sizeDelta = new Vector2(380f, 48f);

            var background = inputRoot.AddComponent<Image>();
            background.color = new Color(0.1f, 0.14f, 0.2f, 1f);

            var inputField = inputRoot.AddComponent<TMP_InputField>();
            inputField.targetGraphic = background;
            inputField.characterLimit = 8;
            inputField.contentType = TMP_InputField.ContentType.Alphanumeric;

            var textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputRoot.transform, false);
            var textAreaRect = textArea.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(14f, 8f);
            textAreaRect.offsetMax = new Vector2(-14f, -8f);
            inputField.textViewport = textAreaRect;

            var placeholder = CreateLabel("Placeholder", "Enter lobby code", 18, TextAlignmentOptions.Left, textArea.transform);
            placeholder.rectTransform.anchorMin = Vector2.zero;
            placeholder.rectTransform.anchorMax = Vector2.one;
            placeholder.rectTransform.offsetMin = Vector2.zero;
            placeholder.rectTransform.offsetMax = Vector2.zero;
            placeholder.color = new Color(0.45f, 0.5f, 0.6f);

            var text = CreateLabel("Text", string.Empty, 18, TextAlignmentOptions.Left, textArea.transform);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            text.color = Color.white;

            inputField.placeholder = placeholder;
            inputField.textComponent = text;

            return inputField;
        }

        private Button CreateButton(string objectName, string label, Color color, Vector2 position, Transform parent)
        {
            var buttonObject = new GameObject(objectName);
            buttonObject.transform.SetParent(parent, false);

            var rectTransform = buttonObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(0f, 0f);
            rectTransform.pivot = new Vector2(0f, 0f);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = new Vector2(180f, 52f);

            var image = buttonObject.AddComponent<Image>();
            image.color = color;

            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;

            var buttonLabel = CreateLabel("Label", label, 18, TextAlignmentOptions.Center, buttonObject.transform);
            buttonLabel.rectTransform.anchorMin = Vector2.zero;
            buttonLabel.rectTransform.anchorMax = Vector2.one;
            buttonLabel.rectTransform.offsetMin = Vector2.zero;
            buttonLabel.rectTransform.offsetMax = Vector2.zero;
            buttonLabel.color = Color.white;

            return button;
        }

        private TextMeshProUGUI CreateLabel(string objectName, string text, float fontSize, TextAlignmentOptions alignment, Transform parent)
        {
            var labelObject = new GameObject(objectName);
            labelObject.transform.SetParent(parent, false);
            var label = labelObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.enableWordWrapping = true;
            return label;
        }

        private void ConfigureRect(RectTransform rectTransform, Vector2 anchoredPosition, Vector2 sizeDelta, Vector2 anchorMin)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMin;
            rectTransform.pivot = new Vector2(anchorMin.x, anchorMin.y);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = sizeDelta;
        }

        private void SetPrivateField(object target, string fieldName, object value)
        {
            target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(target, value);
        }

        private void SetRenderersEnabled(GameObject root, bool enabled)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                renderer.enabled = enabled;
            }
        }
    }
}
