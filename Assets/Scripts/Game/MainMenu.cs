using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Core;
using Game.UI;
using Reflex.Attributes;
using Reflex.Core;
using TMPro;
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
        [SerializeField] private CanvasGroup _introBg;
        [SerializeField] private List<TextMeshProUGUI> _introText;
        [SerializeField] private float _introTextReplicaDelay = 0.5f;
        [SerializeField] private float _typingSpeed = 0.05f;
        
        private SoundManager _soundManager;
        private UniTask _typingTask;
        private CancellationTokenSource _typingCancellationTokenSource;
        
        [Inject]  
        private void Inject(SoundManager soundManager)
        {
            _soundManager = soundManager;
        }

        private void Start()
        {
            _startGameButton.onClick.AddListener(() => OnStartGameButtonClicked().Forget());
            
            _soundManager.PlayMusicClip("gameplay");

            EntranceSequence().Forget();
        }

        private async UniTask EntranceSequence()
        {
            await _loadingOverlay.HideOverlay();
            
            _menuUI.gameObject.SetActive(true);
        }

        private async UniTask OnStartGameButtonClicked()
        {
            _startGameButton.interactable = false;
            
            foreach (var text in _introText)
            {
                text.maxVisibleCharacters = 0;
            }

            await _introBg.DOFade(1f, 0.5f).ToUniTask();

            _typingCancellationTokenSource = new CancellationTokenSource();
            _typingTask = TypingTask();
            try
            {
                await _typingTask;
            }
            catch (Exception e)
            {
                // ignored
            }
            
            foreach (var text in _introText)
            {
                text.maxVisibleCharacters = text.text.Length;
            }

            await UniTask.DelayFrame(1);

            while (!Input.anyKeyDown)
            {
                await UniTask.DelayFrame(1);
            }

            await _loadingOverlay.ShowOverlay();
            var scene = SceneManager.LoadScene(1, new LoadSceneParameters(LoadSceneMode.Single));
            ReflexSceneManager.PreInstallScene(scene, builder => {});
        }
        
        private async UniTask TypingTask()
        {
            foreach (var text in _introText)
            {
                for (int i = 0; i < text.text.Length; i++)
                {
                    text.maxVisibleCharacters++;
                    await UniTask.Delay(TimeSpan.FromSeconds(_typingSpeed), DelayType.UnscaledDeltaTime, cancellationToken: _typingCancellationTokenSource.Token);
                }
                
                await UniTask.Delay(TimeSpan.FromSeconds(_introTextReplicaDelay), DelayType.UnscaledDeltaTime, cancellationToken: _typingCancellationTokenSource.Token);
            }
        }

        private void Update()
        {
            if (_typingCancellationTokenSource != null && Input.anyKeyDown)
            {
                _typingCancellationTokenSource.Cancel();
            }
        }
    }
}
