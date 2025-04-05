using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Gameplay.Events
{
    public class TileEvent : ScriptableObject
    {
        [SerializeField] private GameObject _eventMarkerPrefab;

        public GameObject MarkerPrefab => _eventMarkerPrefab;

        public virtual bool ValidateForTile(IGameController gameController, Vector2Int tile)
        {
            return true;
        }
        
        public async virtual UniTask HandleEvent(IGameController gameController) {}
    }
}
