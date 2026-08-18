using System;
using UnityEngine;

namespace SkillSystem
{
    public interface ISkill
    {
        event Action<ISkill> OnSkillEnd;
        SkillDataSO SkillData { get; }
        float NormalizedCoolTime { get; } //0~1로 표기되도록
        bool IsUsing { get; }
        bool CanInterrupt { get; } //캔슬가능한 스킬이냐?
        
        void InitalizeSkill(ISkillModule skillModule);
        bool CanUseSkill(GameObject target = null);
        void UseSkill(GameObject target = null);

        void OnUpdateSkill(); //시전중 매프레임 호출(차징이나 누적 타이머
        void ReleaseInput(); //시전 시작하고 입력이 떼졌을 때 수행
        void StopSkill(); //스킬 강제 종료
        void CleanUpSkillData();    //스킬에서 구독한 모든 정보들 클린업
    }
}