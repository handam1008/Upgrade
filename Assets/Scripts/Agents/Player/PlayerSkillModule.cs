using System;
using System.Collections.Generic;
using System.Linq;
using DevLib.ModuleSystem;
using GameSystem;
using SkillSystem;
using UnityEngine;

namespace Agents.Player
{
    public class PlayerSkillModule : AbstractSkillModule, IAfterInitModule
    {
        [Serializable]
        public struct LoadoutEntry
        {
            public SkillSlot slot;
            public SkillDataSO skillData;
        }
        
        [Tooltip("시작 시 각 슬롯에 장착될 기본 스킬( 런타임에 장착도 가능하게 할 예정")]
        [SerializeField] private List<LoadoutEntry> _defaultLoadout = new List<LoadoutEntry>();
        
        //어떤 슬롯에 어떤 스킬이 장착되었는가?
        private readonly Dictionary<SkillSlot, int> _slotToSkillId = new Dictionary<SkillSlot, int>();

        //기본공격 관련
        private int _basicAttackId;
        private bool _hasBasicAttack;

        //대쉬 관련 (슬롯에 장착하지 않고 전용 키로 쓴다)
        private int _dashId;
        private bool _hasDash;
        
        //가장 최근에 요청된 스킬 ID, FSM에서 SkillState진입시에 이 스킬을 시전하게 된다.
        public int RequestedSkillId { get; private set; }

        //어떤 입력(슬롯)으로 시전했는지. 차징 스킬이 "그 키가 떼졌는지"를 판별하는 데 쓴다.
        public SkillSlot? RequestedInputSlot { get; private set; }

        public event Action<SkillSlot, SkillDataSO> OnSlotChanged; //UI가 구독해서 처리한다.
        
        
        // 스킬 딕셔너리가 다 채워지고 (base.Init에서) 그 뒤에 호출해서 정리작업을 수행한다.
        public void AfterInit()
        {
            CacheBasicAttack();
            CacheDash();
            BuildDefaultLoadout();
        }

        private void CacheBasicAttack()
        {
            ISkill basicSkill = _skillDict.Values
                .FirstOrDefault(s => s.SkillData.category == SkillCategory.BasicAttack);

            if (basicSkill == null)
            {
                Debug.LogWarning($"[PlayerSkillModule] 기본 공격이 누락되었습니다. : {gameObject}");
                return;
            }

            _basicAttackId = basicSkill.SkillData.skillIdHash;
            _hasBasicAttack = true; //기본 공격 소유중
        }

        private void CacheDash()
        {
            ISkill dashSkill = _skillDict.Values
                .FirstOrDefault(s => s.SkillData.category == SkillCategory.Dash);

            if (dashSkill == null)
            {
                Debug.LogWarning($"[PlayerSkillModule] 대쉬 스킬이 누락되었습니다. : {gameObject}");
                return;
            }

            _dashId = dashSkill.SkillData.skillIdHash;
            _hasDash = true;
        }

        private void BuildDefaultLoadout()
        {
            foreach (LoadoutEntry entry in _defaultLoadout)
            {
                if(entry.skillData == null) continue;
                EquipSkill(entry.slot, entry.skillData.skillIdHash);
            }
        }

        #region 키 입력 해석 및 시전요청 (FSM에서 요청)

        public bool TryResolveBasicAttack(out int skillId)
        {
            skillId = _basicAttackId;
            return _hasBasicAttack;
        }
        
        //스킬 슬롯에 장착된 슬롯을 돌려주는 함수, 슬롯을 주면 거기 장착된 id를 준다.
        public bool TryResolveSlot(SkillSlot slot, out int skillId)
        {
            return _slotToSkillId.TryGetValue(slot, out skillId);
        }
        
        //시전 가능여부 검사후 가능하다면 요청기록해주는 함수
        public bool TryRequestSkill(int skillId) => TryRequestSkill(skillId, null);

        public bool TryRequestSkill(int skillId, SkillSlot? inputSlot)
        {
            //캔슬 불가능한 스킬이 시전 중이면 새 요청을 막는다
            if (CurrentSkill is { IsUsing: true, CanInterrupt: false }) return false;

            if(!CanUseSkill(skillId)) return false;
            RequestedSkillId = skillId;
            RequestedInputSlot = inputSlot;
            return true;
        }

        public bool TryResolveDash(out int skillId)
        {
            skillId = _dashId;
            return _hasDash;
        }

        #endregion
        

        #region UI 연동용 API

        public void EquipSkill(SkillSlot entrySlot, int skillId)
        {
            if (!_skillDict.TryGetValue(skillId, out ISkill skill))
            {
                Debug.LogWarning($"[PlayerSkillModule] 장착하려는 스킬이 없습니다. : {skillId}");
                return;
            }
            
            _slotToSkillId[entrySlot] = skillId;
            OnSlotChanged?.Invoke(entrySlot, skill.SkillData);
        }

        public void UnEquipSkill(SkillSlot entrySlot)
        {
            if(_slotToSkillId.Remove(entrySlot))
                OnSlotChanged?.Invoke(entrySlot, null);
        }
        
        //스킬에 장착된 스킬 데이터를 가져오는 
        public SkillDataSO GetSlotData(SkillSlot slot)
        {
            return _slotToSkillId.TryGetValue(slot, out int skillId)
                && _skillDict.TryGetValue(skillId, out ISkill skill) 
                ? skill.SkillData 
                : null;
        }
        
        //UI관련 쿨다운 표기 로직만 없는 상태이다.
        public float GetSlotCooldown(SkillSlot slot)
        {
            return 0f;
        }

        #endregion


        public SkillDataSO GetSkillData(int skillId) 
            => _skillDict.TryGetValue(skillId, out ISkill skill)? skill.SkillData : null;

        public override float GetBaseDamage(SkillDataSO skillData)
        {
            //원래는 여기에 Stat도 들어가고, 장비파생데이터도, 버프데이터도 
            return skillData.baseSkillDamage * skillData.damageMultiplier;
        }

        
    }
}