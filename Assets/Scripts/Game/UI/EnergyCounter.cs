using Game.Gameplay;
using Reflex.Attributes;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class EnergyCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _counterText;

        private GameController _gameController;
        
        [Inject]
        private void Inject(GameController gameController)
        {
            _gameController = gameController;
        }

        private void Update()
        {
            _counterText.text = _gameController.Energy.ToString();
        }
    }
}
