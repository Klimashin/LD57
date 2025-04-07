using DG.Tweening;
using Game.Gameplay;
using Reflex.Attributes;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class ResourceCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _counterText;
        [SerializeField] private PlayerResources _resType;

        private GameController _gameController;
        private int _cachedValue;
        
        [Inject]
        private void Inject(GameController gameController)
        {
            _gameController = gameController;
        }

        private void Update()
        {
            int currentRes = _gameController.GetResource(_resType);
            if (_cachedValue != currentRes)
            {
                int prevValue = _cachedValue;
                _cachedValue = currentRes;
                AnimateResourceCounter(prevValue, _cachedValue, 1.5f);
            }
        }
        
        private void AnimateResourceCounter(int fromValue, int toValue, float duration)
        {
            DOTween.To(() => (float)fromValue, x =>
            {
                _counterText.text = Mathf.RoundToInt(x).ToString();
            }, toValue, duration);
        }
    }
}
