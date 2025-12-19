using System.Linq;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using Gameplay.Map;

namespace Network
{
    /// <summary>
    /// Представление игрока в сети (NetworkBehaviour)
    /// Синхронизирует данные игрока между клиентами
    /// </summary>
    public class NetworkPlayer : NetworkBehaviour
    {
        private NetworkVariable<string> _playerName = new NetworkVariable<string>(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server);

        private NetworkVariable<ulong> _clientId = new NetworkVariable<ulong>(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server);

        private NetworkVariable<int> _playerColor = new NetworkVariable<int>(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server);

        public string PlayerName => _playerName.Value;
        public ulong ClientId => _clientId.Value;
        public Player PlayerColor => (Player)_playerColor.Value;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                // Устанавливаем имя игрока из Lobby
                string playerName = GetPlayerNameFromLobby();
                _playerName.Value = playerName;
                _clientId.Value = OwnerClientId;

                // Определяем цвет игрока: Host = Blue, Client = Red
                Player playerColor = Unity.Netcode.NetworkManager.Singleton.IsHost && OwnerClientId == Unity.Netcode.NetworkManager.Singleton.LocalClientId
                    ? Player.Blue
                    : Player.Red;
                
                _playerColor.Value = (int)playerColor;

                Debug.Log($"[NetworkPlayer] Spawned for client {OwnerClientId} with name: {playerName}, color: {playerColor}");
            }

            // Подписываемся на изменения
            _playerName.OnValueChanged += OnPlayerNameChanged;
            _playerColor.OnValueChanged += OnPlayerColorChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _playerName.OnValueChanged -= OnPlayerNameChanged;
            _playerColor.OnValueChanged -= OnPlayerColorChanged;
        }

        private void OnPlayerNameChanged(string oldName, string newName)
        {
            Debug.Log($"[NetworkPlayer] Player name changed from {oldName} to {newName}");
        }

        private void OnPlayerColorChanged(int oldColor, int newColor)
        {
            Debug.Log($"[NetworkPlayer] Player color changed from {(Player)oldColor} to {(Player)newColor}");
        }

        private string GetPlayerNameFromLobby()
        {
            var connectionManager = NetworkConnectionManager.Instance;
            if (connectionManager?.JoinedLobby == null)
            {
                return "Player";
            }

            try
            {
                var playerId = AuthenticationService.Instance.PlayerId;
                var player = connectionManager.JoinedLobby.Players.FirstOrDefault(p => p.Id == playerId);

                if (player != null && player.Data.ContainsKey("PlayerName"))
                {
                    return player.Data["PlayerName"].Value;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to get player name from lobby: {e.Message}");
            }

            return "Player";
        }
    }
}

