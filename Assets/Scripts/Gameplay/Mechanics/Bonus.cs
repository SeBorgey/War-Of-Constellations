using UnityEngine;

namespace Gameplay.Mechanics
{
    /// <summary>
    /// Базовый класс для бонусов
    /// </summary>
    [System.Serializable]
    public class Bonus
    {
        [Header("Bonus Info")]
        public string name;
        public string description;
        public int cost;
        public int maxLevel = 5;

        [Header("Current State")]
        public int currentLevel = 0;

        public virtual void Apply(PlayerController playerController)
        {
            // Переопределяется в наследниках
        }

        public virtual void Remove(PlayerController playerController)
        {
            // Переопределяется в наследниках
        }

        public bool CanPurchase(int playerGold)
        {
            return currentLevel < maxLevel && playerGold >= GetCostForNextLevel();
        }

        public int GetCostForNextLevel()
        {
            // Базовая стоимость увеличивается с каждым уровнем
            return cost * (currentLevel + 1);
        }
    }

    /// <summary>
    /// Бонус увеличения силы клика
    /// </summary>
    [System.Serializable]
    public class ClickPowerBonus : Bonus
    {
        public int powerIncreasePerLevel = 1;

        public ClickPowerBonus()
        {
            name = "Click Power";
            description = "Увеличивает силу клика";
            cost = 10;
        }

        public override void Apply(PlayerController playerController)
        {
            // Применяется через BonusSystem
        }

        public int GetTotalPowerIncrease()
        {
            return powerIncreasePerLevel * currentLevel;
        }
    }

    /// <summary>
    /// Бонус увеличения генерации золота
    /// </summary>
    [System.Serializable]
    public class GoldGenerationBonus : Bonus
    {
        public float goldMultiplierPerLevel = 0.2f; // +20% за уровень

        public GoldGenerationBonus()
        {
            name = "Gold Generation";
            description = "Увеличивает генерацию золота";
            cost = 15;
        }

        public override void Apply(PlayerController playerController)
        {
            // Применяется через BonusSystem
        }

        public float GetTotalGoldMultiplier()
        {
            return 1f + (goldMultiplierPerLevel * currentLevel);
        }
    }
}

