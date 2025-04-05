using Game.Core;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class RestartButton : MonoBehaviour
    {
        [SerializeField] private Button _button;

        private SceneManager _sceneManager;
        
        [Inject]
        private void Inject(SceneManager sceneManager)
        {
            _sceneManager = sceneManager;
        }

        private void Start()
        {
            _button.onClick.AddListener(ReloadCurrentScene);
        }

        private void ReloadCurrentScene()
        {
            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene(); 
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene.name);
        }
    }
}
