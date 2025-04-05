using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Quests/QuestStep")]
    public class QuestStep : ScriptableObject
    {
        [TextArea] public string StepDescription;
        
        [InfoBox("If at least 1 next step is set for a choice - it will be selected. Otherwise - event will be triggered (if set) and quest will end.")]
        public List<QuestChoice> Choices;
    }
}
