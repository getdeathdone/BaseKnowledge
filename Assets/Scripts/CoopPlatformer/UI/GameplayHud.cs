using System.Linq;
using System.Text;
using CoopPlatformer.Gameplay.Space;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace CoopPlatformer.UI
{
    public class GameplayHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text _playersLabel;
        [SerializeField] private TMP_Text _healthLabel;

        private void Update()
        {
            var networkManager = NetworkManager.Singleton;
            if (networkManager == null || !networkManager.IsListening)
            {
                SetText("Players: 0", string.Empty);
                return;
            }

            var ships = FindObjectsByType<NetworkShipController>(FindObjectsSortMode.None)
                .Where(ship => ship != null && ship.IsSpawned && ship.NetworkObject != null && ship.NetworkObject.IsPlayerObject)
                .OrderBy(ship => ship.OwnerClientId)
                .ToArray();

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
            if (_playersLabel != null)
            {
                _playersLabel.text = value;
            }
        }

        private void SetHealthLabel(string value)
        {
            if (_healthLabel != null)
            {
                _healthLabel.text = value;
            }
        }
    }
}
