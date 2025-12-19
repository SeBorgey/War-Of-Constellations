using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

namespace Gameplay.Mechanics
{
    /// <summary>
    /// Система управления бонусами игрока
    /// </summary>
    public class BonusSystem : NetworkBehaviour
    {
        [Header("Bonuses")]
        [SerializeField] private ClickPowerBonus _clickPowerBonus = new ClickPowerBonus();
        [SerializeField] private GoldGenerationBonus _goldGenerationBonus = new GoldGenerationBonus();

        private PlayerController _playerController;

        public ClickPowerBonus ClickPowerBonus => _clickPowerBonus;
        public GoldGenerationBonus GoldGenerationBonus => _goldGenerationBonus;

        private void Start()
        {
            if (_playerController == null)
            {
                _playerController = GetComponent<PlayerController>();
            }
        }

        /// <summary>
        /// Покупка бонуса (вызывается через ServerRpc)
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void PurchaseBonusServerRpc(int bonusType)
        {
            if (_playerController == null)
            {
                Debug.LogWarning("[BonusSystem] PlayerController is null!");
                return;
            }

            Bonus bonus = null;
            switch (bonusType)
            {
                case 0: // ClickPower
                    bonus = _clickPowerBonus;
                    break;
                case 1: // GoldGeneration
                    bonus = _goldGenerationBonus;
                    break;
                default:
                    Debug.LogWarning($"[BonusSystem] Unknown bonus type: {bonusType}");
                    return;
            }

            if (bonus == null)
            {
                return;
            }

            int cost = bonus.GetCostForNextLevel();
            if (_playerController.Gold < cost)
            {
                Debug.LogWarning($"[BonusSystem] Not enough gold! Need {cost}, have {_playerController.Gold}");
                return;
            }

            if (!bonus.CanPurchase(_playerController.Gold))
            {
                Debug.LogWarning($"[BonusSystem] Cannot purchase bonus {bonus.name}!");
                return;
            }

            // Покупаем бонус
            _playerController.AddGold(-cost);
            bonus.currentLevel++;
            
            Debug.Log($"[BonusSystem] Purchased {bonus.name} level {bonus.currentLevel} for {cost} gold");
            
            // Уведомляем клиентов об обновлении
            UpdateBonusesClientRpc(bonusType, bonus.currentLevel);
        }

        [ClientRpc]
        private void UpdateBonusesClientRpc(int bonusType, int newLevel)
        {
            Bonus bonus = null;
            switch (bonusType)
            {
                case 0:
                    bonus = _clickPowerBonus;
                    break;
                case 1:
                    bonus = _goldGenerationBonus;
                    break;
            }

            if (bonus != null)
            {
                bonus.currentLevel = newLevel;
            }
        }

        /// <summary>
        /// Получить общую силу клика с учетом бонусов
        /// </summary>
        public int GetTotalClickPower(int basePower)
        {
            int totalPower = basePower;
            
            if (_clickPowerBonus != null)
            {
                totalPower += _clickPowerBonus.GetTotalPowerIncrease();
            }

            return totalPower;
        }

        /// <summary>
        /// Получить множитель генерации золота с учетом бонусов
        /// </summary>
        public float GetGoldGenerationMultiplier()
        {
            if (_goldGenerationBonus != null)
            {
                return _goldGenerationBonus.GetTotalGoldMultiplier();
            }

            return 1f;
        }

        /// <summary>
        /// Публичный метод для покупки бонуса (вызывается с клиента)
        /// </summary>
        public void PurchaseBonus(BonusType type)
        {
            if (!IsClient)
            {
                Debug.LogWarning("[BonusSystem] PurchaseBonus can only be called from client!");
                return;
            }

            PurchaseBonusServerRpc((int)type);
        }

        public enum BonusType
        {
            ClickPower = 0,
            GoldGeneration = 1
        }
    }
}

