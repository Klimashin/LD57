using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Data;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Gameplay.Events
{
    [CreateAssetMenu(menuName = "TileEvents/BattleEvent")]
    public class BattleEvent : TileEvent
    {
        [SerializeField] private List<MonsterConfig> _config;
        [SerializeField] private TileEvent _battleWonEvent;
        
        public override async UniTask HandleEvent(IGameController gameController)
        {
            await base.HandleEvent(gameController);

            var monster = SelectMonster();
            
            Debug.Log($"Selected monster: {monster.name}");
            
            await gameController.Battle(SelectMonster());
        }

        private MonsterData SelectMonster()
        {
            int totalChance = _config.Select(monsterConfig => monsterConfig.Chance).Sum();
            int cumulativeChance = 0;
            int roll = Random.Range(0, totalChance);

            for (int i = 0; i < _config.Count; i++)
            {
                cumulativeChance += _config[i].Chance;
                if (roll < cumulativeChance)
                {
                    return _config[i].MonsterData;
                }
            }

            throw new Exception("Failed to select monster data based on roll");
        }

        [Serializable]
        public class MonsterConfig
        {
            public int Chance = 1;
            public MonsterData MonsterData;
        }
    }
}
