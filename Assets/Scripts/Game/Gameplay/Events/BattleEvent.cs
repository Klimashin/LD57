using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Gameplay.Events
{
    [CreateAssetMenu(menuName = "TileEvents/BattleEvent")]
    public class BattleEvent : TileEvent
    {
        public override async UniTask HandleEvent(IEventsProcessor eventsProcessor)
        {
            await base.HandleEvent(eventsProcessor);
            
            Debug.Log("Battle event is not implemented.");
        }
    }
}
