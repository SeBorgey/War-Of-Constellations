namespace Core.Interfaces
{
    /// <summary>
    /// Интерфейс для синхронизации состояния игры между клиентами
    /// </summary>
    public interface IGameStateSync
    {
        /// <summary>
        /// Синхронизирует текущее состояние игры с другими клиентами
        /// </summary>
        void SyncGameState();

        /// <summary>
        /// Вызывается при получении состояния игры от сервера/другого клиента
        /// </summary>
        /// <param name="data">Данные состояния игры</param>
        void OnGameStateReceived(GameStateData data);
    }
}

