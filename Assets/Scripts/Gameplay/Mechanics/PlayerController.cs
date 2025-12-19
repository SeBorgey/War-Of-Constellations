using Unity.Netcode;
using UnityEngine;
using System;
using Gameplay.Map;
using Network;

namespace Gameplay.Mechanics
{
    public class PlayerController : NetworkBehaviour
    {
        [Header("Player Data")]
        [SerializeField] private Player _playerColor;
        
        // Синхронизированное золото через NetworkVariable
        private NetworkVariable<int> _gold = new NetworkVariable<int>(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server);
        
        [SerializeField] private int _clickPower = 1;

        [Header("Gold Generation")]
        [SerializeField] private float _goldGenerationInterval = 5f; // Генерация каждые 5 секунд
        [SerializeField] private int _goldPerStar = 1; // Золото за каждый узел

        public Player PlayerColor => _playerColor;
        public int Gold => _gold.Value;

        public event Action<int> OnGoldChanged;

        private float _lastGoldGenerationTime;
        private GameMap _gameMap;
        private BonusSystem _bonusSystem;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            // Автоматически определяем цвет игрока на основе роли в сети
            InitializePlayerColor();
            
            // Подписываемся на изменения золота
            _gold.OnValueChanged += OnGoldValueChanged;
            
            // Инициализируем золото на сервере
            if (IsServer)
            {
                _gold.Value = 0;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _gold.OnValueChanged -= OnGoldValueChanged;
        }

        private void Start()
        {
            if (_gameMap == null)
            {
                _gameMap = FindObjectOfType<GameMap>();
            }

            if (_bonusSystem == null)
            {
                _bonusSystem = GetComponent<BonusSystem>();
            }
        }

        private void Update()
        {
            // Генерация золота происходит только на сервере
            if (IsServer && Time.time - _lastGoldGenerationTime >= _goldGenerationInterval)
            {
                _lastGoldGenerationTime = Time.time;
                GenerateGold();
            }
        }

        private void InitializePlayerColor()
        {
            var networkManager = Unity.Netcode.NetworkManager.Singleton;
            if (networkManager != null && networkManager.IsClient)
            {
                // Пытаемся найти NetworkPlayer для определения цвета
                NetworkPlayer[] networkPlayers = FindObjectsOfType<NetworkPlayer>();
                foreach (var np in networkPlayers)
                {
                    if (np.IsOwner)
                    {
                        _playerColor = np.PlayerColor;
                        Debug.Log($"[PlayerController] Initialized with color: {_playerColor}");
                        return;
                    }
                }

                // Если NetworkPlayer не найден, определяем по роли
                _playerColor = networkManager.IsHost ? Player.Blue : Player.Red;
                Debug.Log($"[PlayerController] Initialized with color based on role: {_playerColor}");
            }
            else
            {
                // Если нет сети, используем значение по умолчанию
                if (_playerColor == 0) // Если не установлен в Inspector
                {
                    _playerColor = Player.Blue;
                }
            }
        }

        public void Initialize(Player color)
        {
            _playerColor = color;
            if (IsServer)
            {
                _gold.Value = 0;
            }
        }

        private void GenerateGold()
        {
            if (_gameMap == null)
            {
                _gameMap = FindObjectOfType<GameMap>();
            }

            if (_gameMap == null)
            {
                return;
            }

            // Подсчитываем количество узлов игрока
            int starCount = 0;
            StarState targetState = _playerColor == Player.Blue ? StarState.Blue : StarState.Red;

            var constellations = _gameMap.GetConstellations();
            foreach (var constellation in constellations)
            {
                var stars = constellation.GetStars();
                foreach (var star in stars)
                {
                    if (star.State == targetState)
                    {
                        starCount++;
                    }
                }
            }

            // Генерируем золото на основе количества узлов
            int goldToAdd = starCount * _goldPerStar;
            
            // Применяем множитель от бонусов
            if (_bonusSystem != null)
            {
                float multiplier = _bonusSystem.GetGoldGenerationMultiplier();
                goldToAdd = Mathf.RoundToInt(goldToAdd * multiplier);
            }
            
            if (goldToAdd > 0)
            {
                AddGold(goldToAdd);
                Debug.Log($"[PlayerController] Generated {goldToAdd} gold for {_playerColor} (stars: {starCount})");
            }
        }

        public void AddGold(int amount)
        {
            if (IsServer)
            {
                _gold.Value += amount;
            }
            else
            {
                Debug.LogWarning("[PlayerController] AddGold can only be called on server!");
            }
        }

        private void OnGoldValueChanged(int oldValue, int newValue)
        {
            OnGoldChanged?.Invoke(newValue);
        }

        public int GetClickPower()
        {
            int basePower = _clickPower;
            
            // Применяем бонусы
            if (_bonusSystem != null)
            {
                basePower = _bonusSystem.GetTotalClickPower(basePower);
            }
            
            return basePower;
        }
    }
}
