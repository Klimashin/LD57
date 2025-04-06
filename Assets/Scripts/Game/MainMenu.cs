using Cysharp.Threading.Tasks;
using Game.Core;
using Game.UI;
using Reflex.Attributes;
using Reflex.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private LoadingOverlay _loadingOverlay;
        [SerializeField] private Button _startGameButton;
        [SerializeField] private GameObject _menuUI;
        
        private SoundManager _soundManager;
        
        [Inject]  
        private void Inject(SoundManager soundManager)
        {
            _soundManager = soundManager;
        }

        private void Start()
        {
            _startGameButton.onClick.AddListener(OnStartGameButtonClicked);
            
            _soundManager.PlayMusicClip("gameplay");

            EntranceSequence().Forget();
        }

        private async UniTask EntranceSequence()
        {
            await _loadingOverlay.HideOverlay();
            
            _menuUI.gameObject.SetActive(true);
        }

        private void OnStartGameButtonClicked()
        {
            var scene = SceneManager.LoadScene(1, new LoadSceneParameters(LoadSceneMode.Single));
            ReflexSceneManager.PreInstallScene(scene, builder => {});
        }
    }
}
