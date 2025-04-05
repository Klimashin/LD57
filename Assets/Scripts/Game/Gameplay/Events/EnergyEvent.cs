using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Gameplay.Events
{
    [CreateAssetMenu(menuName = "TileEvents/EnergyEvent")]
    public class EnergyEvent : TileEvent
    {
        [SerializeField] private Vector2Int _energyRangeToAdd;
        
        public override async UniTask HandleEvent(IEventsProcessor eventsProcessor)
        {
            await base.HandleEvent(eventsProcessor);

            var energy = Random.Range(_energyRangeToAdd.x, _energyRangeToAdd.y + 1);
            eventsProcessor.AddEnergy(energy);
        }
    }
}
