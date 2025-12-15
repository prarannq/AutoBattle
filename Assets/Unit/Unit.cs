using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Unit Identification")]
    public string unitId; // JSONデータで定義したユニットIDをInspectorや生成処理で設定

    [Header("Runtime Stats")]
    public int hp;
    public int hpMax;
    public int mp;
    public int mpMax;
    public int ad;
    public int ap;
    public int ar;
    public float sp;

    // attackEffects と skillEffects が持つデータ
    public EffectData effectData;
    public TargetSelectionData targetSelection;
    public string type;

    public int field;
    public bool attacking = false;

    public UnitAnim unitAnim;
    public EventManager eventManager;
    public EffectManager effectManager;
    public TargetingManager targetingManager;
    public UnitDataManager unitDataManager;
    public StatusBar statusBar;

    public List<GameObject> equipList = new List<GameObject>(9);
    public List<Item> itemList = new List<Item>(9);

    // JSONから取得するデータ
    private UnitDefinition unitDef;

    // ---------- ツールチップ用に保持する文字列 -----------
    private string staticTooltipAttack = ""; // Attack関連の固定文字列
    private string staticTooltipSkill = "";  // Skill関連の固定文字列

    void Start()
    {
        unitAnim = GetComponentInChildren<UnitAnim>();
        statusBar = GetComponent<StatusBar>();

        FieldController fieldController = GetComponentInParent<FieldController>();
        unitDataManager = fieldController.unitDataManager;

        eventManager = GetComponentInParent<EventManager>();
        effectManager = GetComponentInParent<EffectManager>();
        targetingManager = GetComponentInParent<TargetingManager>();

        // Unitの初期化はバトル開始時に行うことを想定
        if (!string.IsNullOrEmpty(unitId))
        {
            InitializeFromData();
        }

        // 攻撃・スキルの固定文字列をまとめて作る
        SetString();
    }

    /// <summary>
    /// attackEffect / skillEffect の情報をまとめ、固定文字列にしておく
    /// </summary>
    private void SetString()
    {
        if (unitDef == null)
        {
            // データがない場合はエラー的な文字を入れておく
            staticTooltipAttack = "(No UnitDefinition)";
            staticTooltipSkill = "";
            return;
        }

        // =============== Attack ===============
        if (unitDef.attackEffects != null)
        {
            // Target
            var atkTS = unitDef.attackEffects.targetSelection;
            int atkCount = atkTS.count;
            string atkCountText = (atkCount > 0) ? atkCount.ToString() : "";
            // Modifiers(先頭のみ or 複数？)
            List<ModifierData> atkMods = unitDef.attackEffects.modifiers;

            // 攻撃TargetとModifiersまとめ
            staticTooltipAttack = "Attack\n";
            staticTooltipAttack += $"Target: {atkTS.type}{atkCountText}\n";

            if (atkMods != null && atkMods.Count > 0)
            {
                // とりあえず先頭だけ表示する例
                // 拡張したければループで全表示も可
                staticTooltipAttack += $"Status: {atkMods[0].stat}\n";
                staticTooltipAttack += $"Amount: {atkMods[0].formula}\n";
            }
            else
            {
                // Modifiersが無ければ空
                staticTooltipAttack += "(No Attack Modifier)\n";
            }
        }
        else
        {
            staticTooltipAttack = "Attack\n(No Attack Effects)\n";
        }

        // =============== Skill ===============
        if (unitDef.skillEffects != null)
        {
            var sklTS = unitDef.skillEffects.targetSelection;
            int sklCount = sklTS.count;
            string sklCountText = (sklCount > 0) ? sklCount.ToString() : "";
            List<ModifierData> sklMods = unitDef.skillEffects.modifiers;

            staticTooltipSkill = "\nSkill\n";
            staticTooltipSkill += $"Target: {sklTS.type}{sklCountText}\n";

            if (sklMods != null && sklMods.Count > 0)
            {
                // 複数modifierをすべて表示する例
                for (int i = 0; i < sklMods.Count; i++)
                {
                    staticTooltipSkill += $"Status: {sklMods[i].stat}\n";
                    staticTooltipSkill += $"Amount: {sklMods[i].formula}\n";
                }
            }
            else
            {
                staticTooltipSkill += "(No Skill Modifier)\n";
            }
        }
        else
        {
            staticTooltipSkill = "\nSkill\n(No Skill Effects)\n";
        }
    }

    public void SetItem()
    {
        Item[] items = GetComponentsInChildren<Item>();
        foreach (Item item in items)
        {
            if (item.cost > 0 && item.cost < 10)
            {
                itemList[item.cost - 1] = item;
            }
        }
    }

    public Item GetItem(int itemCost)
    {
        return itemList[itemCost - 1];
    }

    // SetItemを呼ぶたび、イベントがセットされてしまうため、応急処置として分離した
    public void EventSetItem()
    {
        Item[] items = GetComponentsInChildren<Item>();
        foreach (Item item in items)
        {
            if (item.cost > 0 && item.cost < 10)
            {
                itemList[item.cost - 1] = item;
                item.RegisterEvents(eventManager, this);
            }
        }
        
    }

    public GameObject SearchSlot(int cost)
    {
        int index = cost - 1;
        if (index >= 0 && index < equipList.Count)
        {
            return equipList[index];
        }
        return null;
    }

    public void SetFieldNum(int fieldNum)
    {
        field = fieldNum;
    }

    public void InitializeBattle(EventManager em, EffectManager efm, TargetingManager tm)
    {
        EventSetItem();
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i] != null)
            {
                itemList[i].BattleStart();
            }
            else
            {
                break;
            }
        }
        statusBar.GetStatusBar(this);

        // 戦闘開始時に攻撃を開始
        attacking = true;
        StartCoroutine(AttackRoutine());
    }
    public void BattleEnd()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i] != null)
            {
                itemList[i].BattleEnd();
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// JSONデータからUnitDefinitionを取得してステータスを反映
    /// </summary>
    public void InitializeFromData()
    {
        unitDef = unitDataManager.GetUnitDefinition(unitId);
        if (unitDef == null)
        {
            Debug.LogError("UnitDefinition not found for unitId:" + unitId);
            return;
        }

        hpMax = unitDef.hp;
        hp = hpMax;
        mpMax = unitDef.mpMax;
        mp = 0;
        ad = unitDef.ad;
        ap = unitDef.ap;
        ar = unitDef.ar;
        sp = unitDef.sp;

        effectData = unitDef.attackEffects;
        targetSelection = unitDef.attackEffects.targetSelection;
        type = unitDef.attackEffects.targetSelection.type;
    }

    IEnumerator AttackRoutine()
    {
       
        while (attacking)
        {
            float attackInterval = 100f / Mathf.Max(sp, 10f);
            yield return new WaitForSeconds(attackInterval);
            Attack();
        }
    }

    /// <summary>
    /// 通常攻撃処理
    /// </summary>
    public void Attack()
    {
        unitAnim.StartAttack();
        // ターゲット取得
        Unit targetUnit = targetingManager.FrontUnit(field);
        if (targetUnit != null)
        {
            Debug.Log(targetUnit);
            // ダメージ計算(基本はad)
            int damage = ad;
            targetUnit.Damaged(damage);
            eventManager.InvokeOnAttack(this, targetUnit);
        }

        // MPチャージ
        mp += 10;
        if (mp >= mpMax)
        {
            Skill();
            mp = 0;
        }

        // 攻撃後効果発動
        if (unitDef.attackEffects != null)
        {
            effectManager.ApplyEffect(unitDef.attackEffects, field, unitDef.id);
        }
    }

    /// <summary>
    /// スキル発動処理
    /// </summary>
    public void Skill()
    {
        unitAnim.StartSkill();
        // スキル効果適用
        if (unitDef.skillEffects != null)
        {
            effectManager.ApplyEffect(unitDef.skillEffects, field, unitDef.id);
        }

        eventManager.InvokeOnSkill(this, null);
    }

    public void Damaged(int damage)
    {
        int actualDamage = Mathf.Max(damage - ar, 0);
        hp -= actualDamage;
        if (ar > 0) ar--;
        if (hp <= 0)
        {
            Die();
        }
        else
        {
            eventManager.InvokeOnDamaged(this, actualDamage);
        }
    }

    public void MagicDamaged(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            Die();
        }
        else
        {
            eventManager.InvokeOnDamaged(this, damage);
        }
    }

    public void Die()
    {
        // バトルコントローラに死亡通知
        BattleController bc = FindObjectOfType<BattleController>();
        targetingManager.DeleteUnit(field);
        bc.DieUnit(field);
        Invoke("DeactivateObject", 0.3f);
    }

    void DeactivateObject()
    {
        gameObject.SetActive(false);
    }

    public void AnimIdle()
    {
        unitAnim.StartIdle();
    }

    public void StopAllActions()
    {
        attacking = false;
        StopAllCoroutines();
        AnimIdle();
    }

    // ---------- ツールチップ関連ここから ----------
    private void OnMouseEnter()
    {
        // HPやMPなど、現在値が変わるものはここで取り込む
        // それ以外(攻撃/スキル効果)はstaticTooltipAttack / staticTooltipSkillにすでに格納済み

        string dynamicStats =
            $"{unitId}\n" +
            $"HP: {hp}\n" +
            $"AD: {ad}\n" +
            $"AP: {ap}\n" +
            $"AR: {ar}\n" +
            $"SP: {sp}\n\n";

        string message = dynamicStats + staticTooltipAttack + staticTooltipSkill;

        // ワールド座標→スクリーン座標
        Vector3 screenPos = Input.mousePosition;

        TooltipController.Instance.ShowTooltip(message, screenPos);
    }

    private void OnMouseExit()
    {
        TooltipController.Instance.HideTooltip();
    }
    // ---------- ツールチップ関連ここまで ----------
}
