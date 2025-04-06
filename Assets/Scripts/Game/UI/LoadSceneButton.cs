using Reflex.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI
{
    public class LoadSceneButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private int _sceneIndex;

        private void Start()
        {
            _button.onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            var scene = SceneManager.LoadScene(_sceneIndex, new LoadSceneParameters(LoadSceneMode.Single));
            ReflexSceneManager.PreInstallScene(scene, builder => {});
        }
    }
}
