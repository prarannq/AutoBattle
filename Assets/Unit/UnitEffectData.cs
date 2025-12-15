using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TargetSelectionData
{
    public string type;   // 例: "self","frontEnemy","allAllies"など
    public int count;     // random系で必要な場合、"count":1など
    // 必要ならcriteria等の追加フィールドも定義可能
}

[System.Serializable]
public class ModifierData
{
    public string stat;    // "hp","ad","ap","ar","sp","mp"
    public string formula; // "hp+100","ap-(ap*0.2)"などの式
}

[System.Serializable]
public class StatusEffectData
{
    public string type;            // "stun","damageReduction"など
    public string durationFormula; // "ap*0.05"など
    public string amount;          // 状態異常に数値が必要な場合("0.3"等)
}

[System.Serializable]
public class EffectData
{
    public TargetSelectionData targetSelection; // 対象選択方法
    public List<ModifierData> modifiers;        // ステータス変更用データのリスト
    public List<StatusEffectData> statusEffects;// 状態異常付与用データのリスト
}

[System.Serializable]
public class UnitDefinition
{
    public string id;
    public int hp;
    public int mpMax;
    public int ad;
    public int ap;
    public int ar;
    public float sp;

    // 複数の効果を想定してList<UnitEffect>で定義
    public EffectData attackEffects; // 通常攻撃後に適用する効果一覧
    public EffectData skillEffects;  // スキル発動後に適用する効果一覧
}

[System.Serializable]
public class UnitDataCollection
{
    public UnitDefinition[] units;
}

[System.Serializable]
public class ItemDataCollection
{
    public ItemDefinition[] items;
}

[System.Serializable]
public class ItemDefinition
{
    public string id;
    public string name;
    public int cost;
    public string activationTiming;
    public EffectData effect; // または List<EffectData> effects;
}