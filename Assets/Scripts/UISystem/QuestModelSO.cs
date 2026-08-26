using GameModule.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace UISystem
{
    [CreateAssetMenu(fileName = "Quest View Model", menuName = "System/Quest")]
    public class QuestModelSo : ScriptableObject
    {
        public string questTitle;
        public string questDescription;
        public string reward;

        public void SetQuest(string title, string Des, string reward)
        {
            questTitle = title;
            questDescription = Des;
            this.reward = reward;
        }

        public static QuestModelSo CreateInstanceFromOriginal(QuestModelSo original)
        {
            QuestModelSo healthModelso = CreateInstance<QuestModelSo>();
            healthModelso.SetQuest(original.questTitle,original.questDescription,original.reward);
            return healthModelso;
        }
    }
}
