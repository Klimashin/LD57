using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Reflex.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Game.UI
{
    public class FinaleSequence : MonoBehaviour
    {
        [SerializeField] private List<RectTransform> _cogs;
        [SerializeField] private Vector2 _speedRange;
        [SerializeField] private RectTransform _characterPlatform;
        [SerializeField] private RectTransform _authors;
        [SerializeField] private LoadingOverlay _loadingOverlay;
        [SerializeField] private AudioSource _audio;

        private CancellationTokenSource _cancellationTokenSource;

        private void Start()
        {
            AnimationSequence().Forget();
        }

        private async UniTask AnimationSequence()
        {
            await _loadingOverlay.HideOverlay();

            _audio.volume = 0f;
            _audio.Play();
            _audio.DOFade(1f, 5f);

            await _characterPlatform.DOMoveY(300f, 5f).ToUniTask();

            foreach (var rectTransform in _cogs)
            {
                var speed = Random.Range(_speedRange.x, _speedRange.y);
                rectTransform.DORotate(new Vector3(0, 0, 360), speed, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1);
            }
            
            _characterPlatform.DOMoveY(-500f, 7f);

            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await _authors.DOMoveY(-1000f, 30f).ToUniTask(cancellationToken: _cancellationTokenSource.Token);
            }
            catch (Exception e)
            {
                //ignore
            }

            await _loadingOverlay.ShowOverlay();
            var scene = SceneManager.LoadScene(0, new LoadSceneParameters(LoadSceneMode.Single));
            ReflexSceneManager.PreInstallScene(scene, builder => {});
        }

        private void Update()
        {
            if (_cancellationTokenSource is {IsCancellationRequested: false} && Input.anyKeyDown)
            {
                _cancellationTokenSource.Cancel();
            }
        }
    }
}
