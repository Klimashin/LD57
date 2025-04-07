using Game.Gameplay.Events;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace Game.Gameplay
{
    public class FieldTileController : MonoBehaviour
    {
        [SerializeField] private int _cost = 1;
        [SerializeField] protected TextMeshPro _costText;
        [SerializeField] protected Transform _eventMarkerTransform;
        [SerializeField, TextArea] protected string _tileFlavorText;
        [SerializeField] protected Transform _characterPosition;

        public int Cost => _cost;
        public string FlavorText => _tileFlavorText;
        public Vector2Int Coords { get; private set; }
        public Vector3 CharacterPosition => _characterPosition.transform.position;
        [CanBeNull] public TileEvent TileEvent { get; private set; }

        private GameObject _eventMarker;

        public void Setup(Vector2Int coords)
        {
            Coords = coords;

            if (_cost > 0)
            {
                _costText.text = _cost.ToString();
            }
            else
            {
                _costText.enabled = false;
            }
        }

        public void BindTileEvent(TileEvent tileEvent)
        {
            TileEvent = tileEvent;
            _eventMarker = Instantiate(tileEvent.MarkerPrefab, _eventMarkerTransform);
        }

        public void ConsumeTileEvent()
        {
            if (TileEvent == null)
            {
                return;
            }

            TileEvent = null;
            Destroy(_eventMarker);
        }
    }
}
