using Game.Gameplay;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class TileInfoUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _moveCost;
        [SerializeField] private Image _moveCostIcon;
        [SerializeField] private TextMeshProUGUI _tileFlavorText;
        [SerializeField] private TextMeshProUGUI _eventDescription;
        [SerializeField] private string _noEventText = "No signs of life or danger.";

        private GameController _gameController;
        
        [Inject]
        private void Inject(GameController gameController)
        {
            _gameController = gameController;
        }

        private void Update()
        {
            switch (_gameController.State)
            {
                case GameState.AwaitingInput:
                    if (_gameController.CurrentHoveredTile != null)
                    {
                        Setup(_gameController.CurrentHoveredTile);
                    }
                    else
                    {
                        Reset();
                    }
                    break;
                    
                default:
                    Reset();
                    break;
            }
        }

        private void Setup(FieldTileController tile)
        {
            _moveCostIcon.enabled = true;
            _moveCost.text = $"Movement cost: {tile.Cost.ToString()}";
            _tileFlavorText.text = tile.FlavorText;
            _eventDescription.text = tile.TileEvent != null ? tile.TileEvent.EventHint : _noEventText;
        }

        private void Reset()
        {
            _moveCost.text = string.Empty;
            _tileFlavorText.text = string.Empty;
            _eventDescription.text = string.Empty;
            _moveCostIcon.enabled = false;
        }
    }
}
