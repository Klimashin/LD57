using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Gameplay.Events
{
    [CreateAssetMenu(menuName = "TileEvents/ResourceEvent")]
    public class ResourceEvent : TileEvent
    {
        [SerializeField, TextArea] private string _eventDescription;
        [SerializeField] private List<ResourceConfig> _config;

        public override async UniTask HandleEvent(IGameController gameController)
        {
            await base.HandleEvent(gameController);


            List<ResourceChange> resourceChanges = new();
            foreach (var resourceConfig in _config)
            {
                int resourceAmount = Random.Range(resourceConfig.Amount.x, resourceConfig.Amount.y + 1);
                resourceChanges.Add(new ResourceChange() { Type = resourceConfig.Type, Amount = resourceAmount });
                await gameController.HandleResourceChange(resourceChanges, _eventDescription);
            }
        }

        [Serializable]
        public class ResourceConfig
        {
            public PlayerResources Type;
            public Vector2Int Amount = new (1, 1);
        }

        public struct ResourceChange
        {
            public PlayerResources Type;
            public int Amount;
        }
    }
}
