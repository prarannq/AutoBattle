using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Item : MonoBehaviour
{
    public GameController gameController;
    public string itemId;
    public ItemDefinition itemDef;
    public  Unit owner;
    
    public EventManager eventManager;
    public EffectManager effectManager;
    public int cost;
    public ItemDataManager itemDataManager;

    private ServerController serverController;
    // D&Dロジックは省略（もともとのコード参考）
    // D&D関連
    protected GameObject draggedObject;
    private Vector3 startPosition;
    public Vector3 mousePosition;
    public Box box;
    protected FieldController fieldController;

    private string tooltipMessage = "";
    private bool bought = false;

    void Start()
    {
        gameController = GetComponentInParent<GameController>();
        serverController = GetComponentInParent<ServerController>();
        itemDataManager = GetComponentInParent<ItemDataManager>();
        itemDef = itemDataManager.GetItemDefinition(itemId);
        box = GetComponentInParent<Box>();
        fieldController = GetComponentInParent<FieldController>();
        cost = itemDef.cost;
        effectManager = GetComponentInParent<EffectManager>();
        SetString();
    }

    private void SetString()
    {
        if (itemDef == null)
        {
            tooltipMessage = "(No itemDef)";
            return;
        }

        // 1) 基本情報
        // id, name, cost, activationTiming
        tooltipMessage = $"{itemDef.name}\n (Cost {cost})\n";
        tooltipMessage += $"Timing: {itemDef.activationTiming}\n";

        // 2) effectData (targetSelection + modifiers)
        if (itemDef.effect != null)
        {
            // ターゲット指定
            TargetSelectionData ts = itemDef.effect.targetSelection;
            // countが0の場合は表示しない
            string countText = (ts.count > 0) ? ts.count.ToString() : "";

            tooltipMessage += $"Target: {ts.type}{countText}\n";

            // Modifiers
            List<ModifierData> mods = itemDef.effect.modifiers;
            if (mods != null && mods.Count > 0)
            {
                // 複数のmodifierをループで表示
                for (int i = 0; i < mods.Count; i++)
                {
                    ModifierData md = mods[i];
                    // 例: " => HP +10" とか " => AP -(ap*0.3)"
                    // 今回は単純に stat と formula を列挙
                    tooltipMessage += $"Effect{i + 1}: stat={md.stat}, formula={md.formula}\n";
                }
            }
            else
            {
                tooltipMessage += "(No modifiers)\n";
            }
        }
        else
        {
            tooltipMessage += "(No effect)\n";
        }
    }

    void Update()
    {
        if (draggedObject != null)
        {
            mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            draggedObject.transform.position = new Vector3(mousePosition.x, mousePosition.y, startPosition.z);
        }
    }


    public void OnMouseDown()
    {
        startPosition = transform.position;

        if (this.CompareTag("ShopItem"))
        {
            BuyItem();
        }
        else
        {
            fieldController.SwitchCollider(true);
            draggedObject = gameObject;
        }
    }

    public void OnMouseUp()
    {
        Collider2D[] hit = Physics2D.OverlapPointAll(mousePosition);
        if (hit != null)
        {
            bool equipped = false;
            foreach (Collider2D collider in hit)
            {
                GameObject targetObject = collider.gameObject;
                if (targetObject.CompareTag("UnitField"))
                {
                    Unit unit = targetObject.GetComponentInChildren<Unit>();
                    if (unit != null)
                    {
                        GameObject slot = unit.SearchSlot(cost);
                        if (slot != null)
                        {
                            if (slot.transform.childCount > 0)
                            {
                                SwapItems(slot.transform.GetChild(0).gameObject, slot);
                                /*
                                Unit parentUnit = GetComponentInParent<Unit>();
                                Debug.Log(parentUnit);
                                if(parentUnit == null)
                                {
                                    // Slotに既にアイテムあり→スワップ
                                    SwapItems(slot.transform.GetChild(0).gameObject, slot);
                                }
                                else if(parentUnit.GetItem(cost + 1) == null)
                                {
                                    Debug.Log(parentUnit.GetItem(cost));
                                    // Slotに既にアイテムあり→スワップ
                                    SwapItems(slot.transform.GetChild(0).gameObject, slot);
                                }
                                else
                                {
                                    Debug.Log(parentUnit.GetItem(cost + 1));
                                    break;
                                }
                                */
                            }
                            else
                            {
                                Unit parentUnit = GetComponentInParent<Unit>();
                                Debug.Log(parentUnit);
                                if(parentUnit != null)
                                {
                                    GameObject parentSlot = parentUnit.SearchSlot(cost + 1);
                                    if(cost < 9)
                                    {
                                        if (parentSlot.transform.childCount > 0)
                                        {
                                            break;
                                        }
                                    }
                                    
                                }
                                
                                // 1コストアイテムなしで2コスト以上を装備できないロジック
                                if (cost == 1)
                                {
                                    AttachToSlot(slot);
                                }
                                else
                                {
                                    // 一つ下のコストアイテムが装備されているかチェック
                                    GameObject preSlot = unit.SearchSlot(cost - 1);
                                    if (preSlot != null && preSlot.transform.childCount > 0)
                                    {
                                        AttachToSlot(slot);
                                    }
                                    else
                                    {
                                        // 条件満たさず装備不可
                                        break;
                                    }
                                }
                                
                                
                            }
                            equipped = true;
                            break;
                        }
                    }
                }
                else if (targetObject.CompareTag("Sell"))
                {
                    SellItem();
                    equipped = true;
                    break;
                }
            }

            if (!equipped)
            {
                // 装備失敗で元位置へ
                transform.position = startPosition;
            }
        }
        else
        {
            // 当たり判定なし、元位置へ
            transform.position = startPosition;
        }

        draggedObject = null;
        fieldController.SwitchCollider(false);
    }

    public void SellItem()
    {
        gameController.ChangeGold(cost - cost / 2);
        serverController.SellItemLog(itemId);
        Destroy(gameObject);
    }

    public void AttachToSlot(GameObject slot)
    {
        transform.SetParent(slot.transform);
        transform.position = slot.transform.position;

        // Log
        Unit unit = slot.GetComponentInParent<Unit>();
        serverController.EquipItemLog(itemId, unit.unitId);
        
    }

    void SwapItems(GameObject existingItem, GameObject target)
    {
        // スワップ処理
        existingItem.transform.SetParent(transform.parent);
        existingItem.transform.position = startPosition;
        transform.SetParent(target.transform);
        transform.position = target.transform.position;

        //Log
        Unit unit = target.GetComponentInParent<Unit>();
        Item item = existingItem.GetComponent<Item>();
        serverController.SwapItemLog(itemId, item.itemId, unit.unitId);
        
    }

    // 戦闘中効果はイベント購読で処理

    public void BattleStart()
    {
        GetComponent<Collider2D>().enabled = false;
        if (itemDef.activationTiming == "battleStart")
        {
            Use();
            //Debug.Log("battleStart");
        }
        if (itemDef.activationTiming == "loop")
        {
            StartCoroutine(LoopUse());
        }
    }
    public void BattleEnd()
    {
        GetComponent<Collider2D>().enabled = true;
    }

    public void RegisterEvents(EventManager em, Unit u)
    {
        owner = u;
        eventManager = em;
        // 必要なイベントを購読
        if (itemDef.activationTiming == "attack")
        {
            eventManager.OnAttack += OnAttackEvent;
        }
        if (itemDef.activationTiming == "damageTaken")
        {
            eventManager.OnDamaged += OnDamagedEvent;
        }
        if (itemDef.activationTiming == "skill")
        {
            eventManager.OnSkill += OnSkillEvent;
        }
    }

    // イベントハンドラ
    protected void OnAttackEvent(Unit attacker, Unit defender)
    {
        if (attacker == owner)
        {
            // ownerが攻撃者の場合の処理
            Use();
        }
    }

    protected void OnDamagedEvent(Unit target, int damage)
    {
        if (target == owner)
        {
            // ownerが被ダメ時処理
            Use();
        }
    }

    protected void OnSkillEvent(Unit caster, SkillBase skill)
    {
        if (caster == owner)
        {
            // ownerがスキル発動時
            Use();

        }
    }

    IEnumerator LoopUse()
    {
        while (owner.attacking)
        {
            Use();
            yield return new WaitForSeconds(5.0f);
        }
    }

    public void Use()
    {
        // userのfieldを取得し、effectManagerを通じてApplyEffectを呼ぶ
        // itemDef.effectがnullでない前提
        // userのfieldを引数に渡し、effectManagerが対象を特定
        // BattleControllerなどから取得済みとする
        effectManager.ApplyEffect(itemDef.effect, owner.field, itemDef.id);
    }

    private void OnMouseEnter()
    {
        // すでに SetString() で tooltipMessage を生成済み。
        // ここでは tooltipMessage を TooltipController に渡すだけ
        Vector3 screenPos = Input.mousePosition;
        TooltipController.Instance.ShowTooltip(tooltipMessage, screenPos);
    }

    private void OnMouseExit()
    {
        TooltipController.Instance.HideTooltip();
    }

    public void BuyItem()
    {
        Shop shop = GetComponentInParent<Shop>();
        if (shop != null && shop.Buy(cost))
        {
            GameObject newParent = box.SearchSpace();
            if (newParent != null)
            {
                transform.SetParent(newParent.transform);
                transform.position = newParent.transform.position;
                startPosition = transform.position;
                this.tag = "BoxItem";
                serverController.BuyItemLog(itemId);
            }
        }
    }
}
