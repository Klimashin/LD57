using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Gameplay.Events
{
    [CreateAssetMenu(menuName = "TileEvents/TeleportEvent")]
    public class TeleportEvent : TileEvent
    {
        public override async UniTask HandleEvent(IGameController gameController)
        {
            await base.HandleEvent(gameController);
            
            Debug.Log("TeleportEvent is not implemented.");
        }
    }
}
