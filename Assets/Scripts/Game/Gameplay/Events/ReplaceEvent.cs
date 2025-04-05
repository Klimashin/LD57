using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Gameplay.Events
{
    [CreateAssetMenu(menuName = "TileEvents/ReplaceEvent")]
    public class ReplaceEvent : TileEvent
    {
        public override async UniTask HandleEvent(IGameController gameController)
        {
            await base.HandleEvent(gameController);
            
            Debug.Log("ReplaceEvent is not implemented.");
        }
    }
}
