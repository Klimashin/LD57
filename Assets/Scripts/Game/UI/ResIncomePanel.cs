using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Gameplay.Events;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class ResIncomePanel : MonoBehaviour
    {
        [SerializeField] private RectTransform _panelRect;
        [SerializeField] private TextMeshProUGUI _flavorText;
        [SerializeField] private IncomeLine _incomeLinePrefab;
        [SerializeField] private Transform _incomeTransform;

        private CancellationTokenSource _cancellationTokenSource;
        
        public async UniTask Show(List<ResourceEvent.ResourceChange> resourceChanges, string description)
        {
            Cleanup();

            _flavorText.text = description;
            foreach (var resourceChange in resourceChanges)
            {
                var incomeLine = Instantiate(_incomeLinePrefab, _incomeTransform);
                incomeLine.Setup(resourceChange);
            }

            gameObject.SetActive(true);

            _cancellationTokenSource = new();

            try
            {
                _panelRect.localScale = Vector3.zero;
                await _panelRect.DOScale(Vector3.one, 0.5f).ToUniTask(cancellationToken: _cancellationTokenSource.Token);

                while (!Input.anyKeyDown)
                {
                    await UniTask.DelayFrame(1, cancellationToken: _cancellationTokenSource.Token);
                }
            }
            catch (Exception e)
            {
                //ignore
            }

            await _panelRect.DOScale(Vector3.zero, 0.5f).ToUniTask();

            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (Input.anyKeyDown && _cancellationTokenSource is {IsCancellationRequested: false})
            {
                _cancellationTokenSource?.Cancel();
            }
        }

        private void Cleanup()
        {
            _flavorText.text = string.Empty;
            
            int count = _incomeTransform.childCount;
            for (int i = count - 1; i >= 0; i--)
            {
                Destroy(_incomeTransform.GetChild(i).gameObject);
            }
        }
    }
}
