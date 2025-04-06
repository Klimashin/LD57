using Game.Gameplay;
using Reflex.Attributes;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class TileInfoUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _moveCost;
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
                        _canvasGroup.alpha = 1f;
                        Setup(_gameController.CurrentHoveredTile);
                    }
                    else
                    {
                        _canvasGroup.alpha = 0f;
                    }
                    break;
                    
                default:
                    _canvasGroup.alpha = 0f;
                    break;
            }
        }

        private void Setup(FieldTileController tile)
        {
            _moveCost.text = tile.Config.Cost.ToString();
            _tileFlavorText.text = tile.FlavorText;
            _eventDescription.text = tile.TileEvent != null ? tile.TileEvent.EventHint : _noEventText;
        }
    }
}
