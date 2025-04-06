using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class LoadingOverlay : MonoBehaviour
    {
        [SerializeField] private Image _overlay;
        [SerializeField] private float _fullWidth = 1080f;

        public async UniTask HideOverlay()
        {
            await DOTween.To(() => _fullWidth, x =>
            {
                _overlay.rectTransform.sizeDelta = new Vector2(x, _overlay.rectTransform.sizeDelta.y);
            }, 0f, 0.5f).ToUniTask();
        }
        
        public async UniTask ShowOverlay()
        {
            await DOTween.To(() => 0f, x =>
            {
                _overlay.rectTransform.sizeDelta = new Vector2(x, _overlay.rectTransform.sizeDelta.y);
            }, _fullWidth, 0.5f).ToUniTask();
        }
    }
}
