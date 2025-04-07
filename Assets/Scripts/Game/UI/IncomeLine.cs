using System;
using Game.Gameplay;
using Game.Gameplay.Events;
using Game.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class IncomeLine : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _countText;
        [SerializeField] private Image _resIcon;
        [SerializeField] private Color _positiveColor;
        [SerializeField] private Color _negativeColor;
        [SerializeField] private ResSpritesDictionary _resSpritesDictionary;

        public void Setup(ResourceEvent.ResourceChange resourceChange)
        {
            if (_resSpritesDictionary.TryGetValue(resourceChange.Type, out var resSprite))
            {
                _resIcon.sprite = resSprite;
            }
            _countText.text = resourceChange.Amount > 0 ? resourceChange.Amount.ToString() : $"-{resourceChange.Amount.ToString()}";
            _countText.color = resourceChange.Amount > 0 ? _positiveColor : _negativeColor;
        }
    }
    
    [Serializable] public class ResSpritesDictionary : UnitySerializedDictionary<PlayerResources, Sprite> {}
}
