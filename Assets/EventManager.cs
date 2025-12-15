using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public event Action<Unit, Unit> OnAttack;
    public event Action<Unit, int> OnDamaged;
    public event Action<Unit, SkillBase> OnSkill;

    public void Initialize()
    {
        // 必要なら初期化
    }

    public void InvokeOnAttack(Unit attacker, Unit defender)
    {
        OnAttack?.Invoke(attacker, defender);
    }

    public void InvokeOnDamaged(Unit target, int damage)
    {
        OnDamaged?.Invoke(target, damage);
    }

    public void InvokeOnSkill(Unit caster, SkillBase skill)
    {
        OnSkill?.Invoke(caster, skill);
    }

    /// <summary>
    /// 全てのイベント購読をリセットする
    /// </summary>
    public void ResetAllListeners()
    {
        OnAttack = null;
        OnDamaged = null;
        OnSkill = null;
    }
}
