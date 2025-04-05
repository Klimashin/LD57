using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Gameplay.Events
{
    [CreateAssetMenu(menuName = "TileEvents/TeleportEvent")]
    public class TeleportEvent : TileEvent
    {
        public override async UniTask HandleEvent(IEventsProcessor eventsProcessor)
        {
            await base.HandleEvent(eventsProcessor);
            
            Debug.Log("TeleportEvent is not implemented.");
        }
    }
}
