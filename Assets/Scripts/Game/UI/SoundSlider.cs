using Game.Core;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class SoundSlider : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
    
        private SoundManager _soundManager;
    
        [Inject]
        private void Inject(SoundManager soundSystem)
        {
            _soundManager = soundSystem;
        }

        private void Start()
        {
            _slider.value = _soundManager.GetVolume();
            _slider.onValueChanged.AddListener(UpdateVolume);
        }

        private void UpdateVolume(float sliderValue)
        {
            _soundManager.SetVolume(sliderValue);
        }
    }
}
