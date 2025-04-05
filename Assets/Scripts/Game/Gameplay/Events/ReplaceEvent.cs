using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Gameplay.Events
{
    [CreateAssetMenu(menuName = "TileEvents/ReplaceEvent")]
    public class ReplaceEvent : TileEvent
    {
        public override async UniTask HandleEvent(IEventsProcessor eventsProcessor)
        {
            await base.HandleEvent(eventsProcessor);
            
            Debug.Log("ReplaceEvent is not implemented.");
        }
    }
}
