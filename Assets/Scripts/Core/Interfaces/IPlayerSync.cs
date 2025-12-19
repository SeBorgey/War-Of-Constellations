namespace Core.Interfaces
{
    /// <summary>
    /// Интерфейс для синхронизации данных игрока между клиентами
    /// </summary>
    public interface IPlayerSync
    {
        /// <summary>
        /// Синхронизирует данные игрока с другими клиентами
        /// </summary>
        /// <param name="clientId">ID клиента</param>
        /// <param name="data">Данные игрока</param>
        void SyncPlayerData(ulong clientId, PlayerData data);

        /// <summary>
        /// Вызывается при получении данных игрока от сервера/другого клиента
        /// </summary>
        /// <param name="clientId">ID клиента</param>
        /// <param name="data">Данные игрока</param>
        void OnPlayerDataReceived(ulong clientId, PlayerData data);
    }
}

