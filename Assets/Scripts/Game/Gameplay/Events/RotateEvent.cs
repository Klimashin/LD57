using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Gameplay.Events
{
    [CreateAssetMenu(menuName = "TileEvents/RotateEvent")]
    public class RotateEvent : TileEvent
    {
        [SerializeField] private bool _clockwise;
        
        public override bool ValidateForTile(IGameController gameController, Vector2Int tile)
        {
            return tile.x > 0 && tile.y > 0 && tile.x < gameController.FieldSize.x - 1 && tile.y < gameController.FieldSize.y - 1;
        }
        
        public override async UniTask HandleEvent(IGameController gameController)
        {
            await base.HandleEvent(gameController);
            
            await gameController.RotateTiles(gameController.CharacterPosition, _clockwise);
        }
    }
}
