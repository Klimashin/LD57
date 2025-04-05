using Cysharp.Threading.Tasks;
using Game.Core;
using Reflex.Attributes;
using UnityEngine;

namespace Game
{
    public class BootSequence : MonoBehaviour
    {
        private SceneManager _sceneManager;
        private SoundManager _soundManager;
        
        [Inject]  
        private void Inject(SceneManager sceneManager, SoundManager soundManager)
        {
            _sceneManager = sceneManager;
            _soundManager = soundManager;
        }

        private void Start()
        {
            HandleBoot().Forget();
        }

        private async UniTaskVoid HandleBoot()
        {
            await _sceneManager.LoadScene("Menu");
            
            _soundManager.PlayMusicClip("test");
        }
    }
}
