using System;
using Game.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class QuestChoiceUI : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _choiceText;

        private QuestChoice _choice;
        private Action<QuestChoice> _onChoiceClicked;
        private bool _clickFired;

        private void Start()
        {
            _button.onClick.AddListener(() =>
            {
                if (_choice != null && _onChoiceClicked != null && !_clickFired)
                {
                    _clickFired = true;
                    _onChoiceClicked.Invoke(_choice);
                }
            });
        }

        public void Setup(QuestChoice choice, Action<QuestChoice> onChoiceClicked)
        {
            _choice = choice;
            _onChoiceClicked = onChoiceClicked;
            _choiceText.text = _choice.ChoiceText;
        }
    }
}
