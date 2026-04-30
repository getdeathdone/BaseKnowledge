using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoopPlatformer.Gameplay.Space;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoopPlatformer.UI
{
    public class GameplayHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text _playersLabel;
        [SerializeField] private TMP_Text _healthLabel;
        [SerializeField] private GameObject _fireButtonRoot;
        [SerializeField] private Button _fireButton;

        private NetworkShipController _ownerShip;

        private void Awake()
        {
            if (_fireButton != null) ConfigureFireButtonEvents(_fireButton);

            SetFireButtonVisible(false);
        }

        private void Update()
        {
            var networkManager = NetworkManager.Singleton;
            if (networkManager == null || !networkManager.IsListening)
            {
                _ownerShip = null;
                SetFireButtonVisible(false);
                SetText("Players: 0", string.Empty);
                return;
            }

            var ships = NetworkShipController.Ships
                .Where(ship =>
                    ship != null && ship.IsSpawned && ship.NetworkObject != null && ship.NetworkObject.IsPlayerObject)
                .OrderBy(ship => ship.OwnerClientId)
                .ToArray();

            _ownerShip = ships.FirstOrDefault(ship => ship.IsOwner);
            SetFireButtonVisible(_ownerShip != null);

            SetPlayersText(networkManager, ships.Length);
            SetHealthText(ships);
        }

        private void SetPlayersText(NetworkManager networkManager, int shipCount)
        {
            var connectedPlayers = networkManager.ConnectedClientsIds.Count;
            var activePlayers = Mathf.Max(connectedPlayers, shipCount);
            SetPlayersLabel($"Players: {activePlayers}/4");
        }

        private void SetHealthText(NetworkShipController[] ships)
        {
            if (ships.Length == 0)
            {
                SetHealthLabel("Waiting for players...");
                return;
            }

            var builder = new StringBuilder();
            foreach (var ship in ships)
            {
                var label = ship.IsOwner ? "You" : $"Player {ship.OwnerClientId + 1}";
                builder.Append(label)
                    .Append(": ")
                    .Append(ship.CurrentHealth)
                    .Append('/')
                    .Append(ship.MaxHealth)
                    .AppendLine();
            }

            SetHealthLabel(builder.ToString().TrimEnd());
        }

        private void SetText(string playersText, string healthText)
        {
            SetPlayersLabel(playersText);
            SetHealthLabel(healthText);
        }

        private void SetPlayersLabel(string value)
        {
            if (_playersLabel != null) _playersLabel.text = value;
        }

        private void SetHealthLabel(string value)
        {
            if (_healthLabel != null) _healthLabel.text = value;
        }

        private void OnFireButtonPointerDown(BaseEventData eventData)
        {
            if (_ownerShip != null)
            {
                var pointerId = eventData is PointerEventData pointerEventData
                    ? pointerEventData.pointerId
                    : int.MinValue;
                _ownerShip.SetFireButtonPressed(true, pointerId);
                _ownerShip.QueueFireButtonShot();
            }
        }

        private void OnFireButtonPointerUp(BaseEventData eventData)
        {
            if (_ownerShip != null) _ownerShip.SetFireButtonPressed(false);
        }

        private void SetFireButtonVisible(bool isVisible)
        {
            if (_fireButtonRoot != null && _fireButtonRoot.activeSelf != isVisible)
                _fireButtonRoot.SetActive(isVisible);
        }

        private void ConfigureFireButtonEvents(Button fireButton)
        {
            var trigger = fireButton.GetComponent<EventTrigger>();
            if (trigger == null) trigger = fireButton.gameObject.AddComponent<EventTrigger>();

            trigger.triggers ??= new List<EventTrigger.Entry>();
            trigger.triggers.Clear();

            AddEventTrigger(trigger, EventTriggerType.PointerDown, OnFireButtonPointerDown);
            AddEventTrigger(trigger, EventTriggerType.PointerUp, OnFireButtonPointerUp);
            AddEventTrigger(trigger, EventTriggerType.PointerExit, OnFireButtonPointerUp);
        }

        private static void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType,
            UnityAction<BaseEventData> callback)
        {
            var entry = new EventTrigger.Entry { eventID = eventType };
            entry.callback.AddListener(callback);
            trigger.triggers.Add(entry);
        }
    }
}