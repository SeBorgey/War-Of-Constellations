using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public class Bootstrapper : MonoBehaviour
    {
        [SerializeField] private string _nextSceneName = "MainMenu";

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            InitializeServices();
            LoadNextScene();
        }

        private void InitializeServices()
        {
            Debug.Log("Initializing Services...");
            // TODO: Initialize generic services (Audio, Input, etc.)
        }

        private void LoadNextScene()
        {
            Debug.Log($"Loading Scene: {_nextSceneName}");
            // SceneManager.LoadScene(_nextSceneName); // Commented out to prevent errors if scene doesn't exist yet
        }
    }
}
