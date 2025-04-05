using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Gameplay.Events
{
    [CreateAssetMenu(menuName = "TileEvents/RotateEvent")]
    public class RotateEvent : TileEvent
    {
        [SerializeField] private bool _clockwise;
        
        public override bool ValidateForTile(IEventsProcessor eventsProcessor, Vector2Int tile)
        {
            return tile.x > 0 && tile.y > 0 && tile.x < eventsProcessor.FieldSize.x - 1 && tile.y < eventsProcessor.FieldSize.y - 1;
        }
        
        public override async UniTask HandleEvent(IEventsProcessor eventsProcessor)
        {
            await base.HandleEvent(eventsProcessor);
            
            await eventsProcessor.RotateTiles(eventsProcessor.CharacterPosition, _clockwise);
        }
    }
}
