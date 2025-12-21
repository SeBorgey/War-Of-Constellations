using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.IO;

namespace Gameplay.Map
{
    public enum StarState
    {
        White,  // Neutral
        Blue,   // Controlled by Blue player
        Red     // Controlled by Red player
    }

    public enum Player
    {
        Blue,
        Red
    }

    public class Star : NetworkBehaviour
    {
        [Header("Identification")]
        [SerializeField] private int _id;

        // Синхронизированные через NetworkVariable
        private NetworkVariable<int> _constellationId = new NetworkVariable<int>(
            -1,
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server);

        [Header("Star Data")]
        // Синхронизированные через NetworkVariable
        private NetworkVariable<Vector2> _coordinates = new NetworkVariable<Vector2>(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server);

        private NetworkVariable<int> _size = new NetworkVariable<int>(
            1,
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server);
        
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI _hpText; // Текст для отображения HP
        
        // Синхронизированные через NetworkVariable
        private NetworkVariable<int> _hp = new NetworkVariable<int>(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server);

        [Header("Combat State")]
        // Синхронизированные через NetworkVariable
        private NetworkVariable<int> _blueDamage = new NetworkVariable<int>(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server);
        
        private NetworkVariable<int> _redDamage = new NetworkVariable<int>(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server);

        // Getters
        public int Id => _id;
        public int ConstellationId => _constellationId.Value;
        public Vector2 Coordinates => _coordinates.Value;
        public int Size => _size.Value;
        public int HP => _hp.Value;
        public int BlueDamage => _blueDamage.Value;
        public int RedDamage => _redDamage.Value;

        public StarState State
        {
            get
            {
                if (_blueDamage.Value >= _hp.Value)
                    return StarState.Blue;
                if (_redDamage.Value >= _hp.Value)
                    return StarState.Red;
                return StarState.White;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // #region agent log
            try { File.AppendAllText(LOG_PATH, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"K\",\"location\":\"Star.cs:OnNetworkSpawn\",\"message\":\"Star spawned\",\"data\":{{\"starId\":{_id},\"isServer\":{IsServer.ToString().ToLower()},\"isClient\":{IsClient.ToString().ToLower()},\"isHost\":{IsHost.ToString().ToLower()}}},\"timestamp\":{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion

            // Находим TextMeshProUGUI если не установлен
            if (_hpText == null)
            {
                _hpText = GetComponentInChildren<TextMeshProUGUI>();
                if (_hpText == null)
                {
                    Debug.LogWarning($"[Star] TextMeshProUGUI not found for Star {_id}!");
                }
            }

            // Подписываемся на изменения HP и урона для обновления текста
            _hp.OnValueChanged += OnHPChanged;
            _blueDamage.OnValueChanged += OnDamageChanged;
            _redDamage.OnValueChanged += OnDamageChanged;

            // Подписываемся на изменения координат и размера для обновления визуала
            _coordinates.OnValueChanged += OnCoordinatesChanged;
            _size.OnValueChanged += OnSizeChanged;

            // Применяем текущие значения (важно для клиента, который получает уже установленные значения)
            ApplySyncedValues();

            // Обновляем текст при спавне
            UpdateHPText();
        }

        private void OnCoordinatesChanged(Vector2 oldValue, Vector2 newValue)
        {
            transform.position = new Vector3(newValue.x, newValue.y, 0);
        }

        private void OnSizeChanged(int oldValue, int newValue)
        {
            UpdateScale();
        }

        private void ApplySyncedValues()
        {
            // Применяем синхронизированные координаты
            if (_coordinates.Value != Vector2.zero)
            {
                transform.position = new Vector3(_coordinates.Value.x, _coordinates.Value.y, 0);
            }

            // Применяем размер
            UpdateScale();

            Debug.Log($"[Star] Applied synced values: coords={_coordinates.Value}, size={_size.Value}, hp={_hp.Value}");
        }

        public override void OnNetworkDespawn()
        {
            // Отписываемся от событий
            _hp.OnValueChanged -= OnHPChanged;
            _blueDamage.OnValueChanged -= OnDamageChanged;
            _redDamage.OnValueChanged -= OnDamageChanged;
            _coordinates.OnValueChanged -= OnCoordinatesChanged;
            _size.OnValueChanged -= OnSizeChanged;

            base.OnNetworkDespawn();
        }

        private void OnHPChanged(int oldValue, int newValue)
        {
            UpdateHPText();
        }

        private void OnDamageChanged(int oldValue, int newValue)
        {
            UpdateHPText();
        }

        private void UpdateHPText()
        {
            if (_hpText != null)
            {
                // Показываем текущее HP и прогресс захвата
                int currentHP = _hp.Value;
                int blueProgress = _blueDamage.Value;
                int redProgress = _redDamage.Value;
                
                // Если звезда захвачена, показываем только HP
                if (blueProgress >= currentHP)
                {
                    _hpText.text = currentHP.ToString();
                    _hpText.color = Color.cyan; // Синий цвет для Blue игрока
                }
                else if (redProgress >= currentHP)
                {
                    _hpText.text = currentHP.ToString();
                    _hpText.color = Color.red; // Красный цвет для Red игрока
                }
                else
                {
                    // Показываем HP и прогресс
                    _hpText.text = $"{currentHP}\nB:{blueProgress} R:{redProgress}";
                    _hpText.color = Color.white; // Белый цвет для нейтральной звезды
                }
            }
        }

        // Setters
        public void SetId(int id)
        {
            _id = id;
        }

        public void SetConstellationId(int constellationId)
        {
            if (IsServer)
            {
                _constellationId.Value = constellationId;
            }
        }

        public void SetCoordinates(Vector2 coordinates)
        {
            if (IsServer)
            {
                _coordinates.Value = coordinates;
            }
            transform.position = new Vector3(coordinates.x, coordinates.y, 0);
        }

        public void SetSize(int size)
        {
            if (IsServer)
            {
                _size.Value = Mathf.Clamp(size, 1, 5);
            }
            UpdateScale();
        }

        public void SetHP(int hp)
        {
            if (IsServer)
            {
                _hp.Value = Mathf.Clamp(hp, 1, 20);
            }
            else
            {
                Debug.LogWarning("[Star] SetHP can only be called on server!");
            }
        }

        public void Initialize(int id, Vector2 coordinates, int size, int hp)
        {
            SetId(id);
            SetCoordinates(coordinates);
            SetSize(size);
            
            // Инициализация NetworkVariable только на сервере
            if (IsServer)
            {
                _hp.Value = Mathf.Clamp(hp, 1, 20);
                _blueDamage.Value = 0;
                _redDamage.Value = 0;
            }
            
            _constellationId = new NetworkVariable<int>(
            -1,
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server);
            
            // Находим TextMeshProUGUI если не установлен (для случаев, когда Initialize вызывается до OnNetworkSpawn)
            if (_hpText == null)
            {
                _hpText = GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        /// <summary>
        /// Apply damage to the star from a specific player.
        /// If the enemy has accumulated damage, it decreases their counter instead.
        /// This method should only be called on the server.
        /// </summary>
        public void ApplyDamage(Player player, int amount)
        {
            // #region agent log
            try { File.AppendAllText(LOG_PATH, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"H\",\"location\":\"Star.cs:ApplyDamage\",\"message\":\"ApplyDamage called\",\"data\":{{\"starId\":{_id},\"player\":\"{player}\",\"amount\":{amount},\"isServer\":{IsServer.ToString().ToLower()},\"hpBefore\":{_hp.Value},\"blueDamageBefore\":{_blueDamage.Value},\"redDamageBefore\":{_redDamage.Value}}},\"timestamp\":{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion
            if (!IsServer)
            {
                // #region agent log
                try { File.AppendAllText(LOG_PATH, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"H\",\"location\":\"Star.cs:ApplyDamage\",\"message\":\"Not server, aborting\",\"data\":{{}},\"timestamp\":{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
                // #endregion
                Debug.LogWarning("[Star] ApplyDamage can only be called on server!");
                return;
            }

            if (amount <= 0) return;

            if (player == Player.Blue)
            {
                // If Red has damage, decrease it first
                if (_redDamage.Value > 0)
                {
                    _redDamage.Value = Mathf.Max(0, _redDamage.Value - amount);
                }
                else
                {
                    _blueDamage.Value += amount;
                }
            }
            else // Player.Red
            {
                // If Blue has damage, decrease it first
                if (_blueDamage.Value > 0)
                {
                    _blueDamage.Value = Mathf.Max(0, _blueDamage.Value - amount);
                }
                else
                {
                    _redDamage.Value += amount;
                }
            }
            // #region agent log
            try { File.AppendAllText(LOG_PATH, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"H\",\"location\":\"Star.cs:ApplyDamage\",\"message\":\"Damage applied\",\"data\":{{\"starId\":{_id},\"blueDamageAfter\":{_blueDamage.Value},\"redDamageAfter\":{_redDamage.Value}}},\"timestamp\":{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion

        }

        private const string LOG_PATH = "/home/ivan/Desktop/unity/War-Of-Constellations/.cursor/debug.log";

        /// <summary>
        /// ServerRpc для обработки клика по узлу
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ClickStarServerRpc(Player player, int power)
        {
            // #region agent log
            try { File.AppendAllText(LOG_PATH, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"G\",\"location\":\"Star.cs:ClickStarServerRpc\",\"message\":\"ServerRpc received\",\"data\":{{\"starId\":{_id},\"player\":\"{player}\",\"power\":{power},\"isServer\":{IsServer.ToString().ToLower()}}},\"timestamp\":{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion
            if (power <= 0)
            {
                // #region agent log
                try { File.AppendAllText(LOG_PATH, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"G\",\"location\":\"Star.cs:ClickStarServerRpc\",\"message\":\"Invalid power\",\"data\":{{\"power\":{power}}},\"timestamp\":{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
                // #endregion
                Debug.LogWarning($"[Star] Invalid click power: {power}");
                return;
            }

            Debug.Log($"[Star] Server received click on Star {_id} from {player} with power {power}");
            ApplyDamage(player, power);
        }

        private void UpdateScale()
        {
            // Scale the star based on its size (1-5)
            float scale = 0.5f + (_size.Value * 0.2f); // Size 1 = 0.7, Size 5 = 1.5
            transform.localScale = Vector3.one * scale;
        }
    }
}

