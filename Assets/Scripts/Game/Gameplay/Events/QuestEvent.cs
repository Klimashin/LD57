using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Data;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Gameplay.Events
{
    [CreateAssetMenu(menuName = "TileEvents/QuestEvent")]
    public class QuestEvent : TileEvent
    {
        [SerializeField] private List<Config> _quests;
        
        public override async UniTask HandleEvent(IGameController gameController)
        {
            await base.HandleEvent(gameController);

            var quest = SelectQuest();

            await gameController.Quest(quest);
        }
        
        private Quest SelectQuest()
        {
            int totalChance = _quests.Select(config => config.Chance).Sum();
            int cumulativeChance = 0;
            int roll = Random.Range(0, totalChance);

            for (int i = 0; i < _quests.Count; i++)
            {
                cumulativeChance += _quests[i].Chance;
                if (roll < cumulativeChance)
                {
                    return _quests[i].Quest;
                }
            }

            throw new Exception("Failed to select monster data based on roll");
        }
        
        [Serializable]
        public class Config
        {
            public int Chance = 1;
            public Quest Quest;
        }
    }
}
