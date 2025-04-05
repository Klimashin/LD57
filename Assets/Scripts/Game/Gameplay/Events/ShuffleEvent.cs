using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Gameplay.Events
{
    [CreateAssetMenu(menuName = "TileEvents/ShuffleEvent")]
    public class ShuffleEvent : TileEvent
    {
        public override async UniTask HandleEvent(IGameController gameController)
        {
            await base.HandleEvent(gameController);

            await gameController.ShuffleTiles();
        }
    }
}
