using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Gameplay.Events
{
    [CreateAssetMenu(menuName = "TileEvents/ShuffleEvent")]
    public class ShuffleEvent : TileEvent
    {
        public override async UniTask HandleEvent(IEventsProcessor eventsProcessor)
        {
            await base.HandleEvent(eventsProcessor);

            await eventsProcessor.ShuffleTiles();
        }
    }
}
