using System;
using System.Collections.Generic;
using System.Linq;
using Game.Gameplay.Events;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

namespace Game.Data
{
    [Serializable]
    public class QuestChoice
    {
        public string ChoiceText;
        public List<Config> NextSteps;
        [CanBeNull, HideIf("@this.NextSteps.Count > 0")] public TileEvent ChoiceEvent;
        
        [CanBeNull]
        public QuestStep SelectNextStep()
        {
            if (NextSteps == null || NextSteps.Count == 0)
            {
                return null;
            }

            int totalChance = NextSteps.Select(stepConfig => stepConfig.Chance).Sum();
            int cumulativeChance = 0;
            int roll = Random.Range(0, totalChance);

            for (int i = 0; i < NextSteps.Count; i++)
            {
                cumulativeChance += NextSteps[i].Chance;
                if (roll < cumulativeChance)
                {
                    return NextSteps[i].NextStep;
                }
            }

            throw new Exception("Failed to select monster data based on roll");
        }

        [Serializable]
        public class Config
        {
            public int Chance = 1;
            public QuestStep NextStep;
        }
    }
}