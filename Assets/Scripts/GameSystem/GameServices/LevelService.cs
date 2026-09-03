using System;
using DevLib.ServiceLocator;
using UnityEngine;

namespace GameSystem.GameServices
{
    public class LevelService : MonoBehaviour, ILevelService
    {

        [field: SerializeField] public int Level { get; private set; } = 1;
       [field:SerializeField] public float HaveExp { get; private set; }
       [field:SerializeField ] public float[] needExp { get; private set; } = {15, 35, 60, 85, 115, 145, 175, 210, 245, 280, 315, 355, 395, 435, 475, 515, 555, 600, 645};
       
       
       public event Action<float> OnLevelChanged;
       public event Action<int> OnLevelUp;


       private void Awake()
        {
            ServiceLocator.Register<ILevelService>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.UnRegister<ILevelService>();
        }
        

        public void GetExp(int amount) 
        {
            HaveExp += amount;


            while (!(Level > needExp.Length) && HaveExp >= needExp[Level - 1])
            {
                HaveExp -= needExp[Level - 1];
                Level++;
                OnLevelUp?.Invoke(Level);
            }

            if (Level > needExp.Length) return;
            
            OnLevelChanged?.Invoke(HaveExp / needExp[Level-1]);
            
           
        }

       
    }
}