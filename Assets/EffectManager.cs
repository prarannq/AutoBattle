using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public  TargetingManager targetingManager;
    private int fieldNum;
    public List<Unit> targetUnitList;
    public GameObject floatingTextPrefab;
    public GameObject canvas;
    public string Id;

    public void Initialize(TargetingManager tm)
    {
        // EffectManagerがTargetingManagerを使うには渡しておく
        targetingManager = tm;
    }

    public void ApplyEffect(EffectData effectData, int field, string id)
    {
        Id = id;


        fieldNum = field;
        
        if (effectData == null) return;

        List<Unit> targetUnitList = new List<Unit>();
        // targetSelectionのtypeを参照
        switch (effectData.targetSelection.type)
        {
            case "self":
                {
                    Unit selfUnit = targetingManager.GetUnit(field);
                    if (selfUnit != null) targetUnitList.Add(selfUnit);
                    break;
                }
            case "frontEnemy":
                {
                    Unit frontEnemy = targetingManager.FrontUnit(field);
                    if (frontEnemy != null) targetUnitList.Add(frontEnemy);
                    break;
                }
            case "frontlineEnemies":
                {
                    targetUnitList = targetingManager.GetFrontlineEnemies(field);
                    break;
                }
            case "frontAlly":
                {
                    targetUnitList = targetingManager.GetFrontAlly(field);
                    break;
                }
            case "allAllies":
                {
                    targetUnitList = targetingManager.GetAllAllies(field);
                    break;
                }
            case "allEnemies":
                {
                    targetUnitList = targetingManager.GetAllEnemies(field);
                    break;
                }
            case "randomAlly":
                {
                    targetUnitList = targetingManager.GetRandomAllies(field, effectData.targetSelection.count);
                    break;
                }
            case "randomEnemy":
                {
                    targetUnitList = targetingManager.GetRandomEnemies(field, effectData.targetSelection.count);
                    break;
                }
            case "allyBehind":
                {
                    targetUnitList = targetingManager.GetAllyBehind(field);
                    
                    break;
                }
            case "enemyBehindTarget":
                {
                    targetUnitList = targetingManager.GetEnemyBehindTarget(field);
                    break;
                }
            default:
                Debug.LogWarning("Unknown targetSelection type: " + effectData.targetSelection.type);
                break;
        }

        
        // ターゲットがいなければ何もしない
        if (targetUnitList == null || targetUnitList.Count == 0) return;

        // modifiers処理
        if (effectData.modifiers != null)
        {
            foreach (var mod in effectData.modifiers)
            {
                float value = EvaluateFormula(mod.formula);
                if (value == 0) Debug.Log("\n" + id + "\n" + effectData.targetSelection.type + "\n" + mod.stat + "\n" + value + "\n\n");
                string sign = (value >= 0) ? "+" : "";
                int intVal = Mathf.FloorToInt(value);
                //Debug.Log("\n" + id + "\n" + effectData.targetSelection.type + "\n" + mod.stat + "\n" + value + "\n\n");
                foreach (var t in targetUnitList)
                {
                    if(t != null)
                    {
                        switch (mod.stat)
                        {
                            case "hp":
                                t.hp = (int)(t.hp + value);
                                ShowFloatingText(t, $"{sign}{intVal} HP");

                                if (t.hp > t.hpMax) t.hp = t.hpMax;
                                if (t.hp < 1)
                                {
                                    t.Die();

                                }
                                break;
                            case "mp":
                                t.mp = (int)(t.mp + value);
                                ShowFloatingText(t, $"{sign}{intVal} MP");
                                if (t.mp > t.mpMax) t.mp = t.mpMax;
                                break;
                            case "ad":
                                t.ad = (int)(t.ad + value);
                                ShowFloatingText(t, $"{sign}{intVal} AD");
                                if (0 > t.ad) t.ad = 0;
                                break;
                            case "ap":
                                t.ap = (int)(t.ap + value);
                                ShowFloatingText(t, $"{sign}{intVal} AP");
                                if (0 > t.ap) t.ap = 0;
                                break;

                            case "ar":
                                t.ar = (int)(t.ar + value);
                                ShowFloatingText(t, $"{sign}{intVal} AR");
                                if (0 > t.ar) t.ar = 0;
                                break;

                            case "sp":
                                t.sp = t.sp + value;
                                ShowFloatingText(t, $"{sign}{Mathf.FloorToInt(value)} SP");
                                if (0 > t.sp) t.sp = 0;
                                break;
                            default:
                                Debug.LogWarning("Unknown stat: " + mod.stat);
                                break;
                        }
                    }
                    
                }
            }
        }

        // statusEffects処理 (必要に応じて)
        if (effectData.statusEffects != null)
        {
            foreach (var se in effectData.statusEffects)
            {
                float duration = EvaluateFormula(se.durationFormula);
                float amount = 0f;
                if (!string.IsNullOrEmpty(se.amount))
                {
                    float.TryParse(se.amount, out amount);
                }

                foreach (var t in targetUnitList)
                {
                    // TODO: 状態異常適用
                    // t.ApplyStatusEffect(se.type, duration, amount);
                }
            }
        }
    }

    private string formula;
    private int pos;

    public float EvaluateFormula(string formulaStr)
    {
        // 初期化
        formula = formulaStr;
        pos = 0;
        // 空白除去
        formula = formula.Replace(" ", "");

        // 先頭が'-'の場合を簡易処理
        if (formula.StartsWith("-"))
        {
            // 例: -(ap*0.3) => 0-(ap*0.3) として扱う
            formula = "0" + formula;
        }

        // 実際に構文解析開始
        float result = ParseExpression();

        // posが最後まで行ってない場合、何か不正な文字が残っている
        // ここではエラー処理省略 (本来はチェックしても良い)
        return result;
    }

    /// <summary>
    /// Expression := Term (('+' | '-') Term)*
    /// </summary>
    private float ParseExpression()
    {
        float value = ParseTerm();

        while (pos < formula.Length)
        {
            char c = formula[pos];
            if (c == '+' || c == '-')
            {
                pos++; // 演算子を読み飛ばす
                float right = ParseTerm();
                if (c == '+')
                {
                    value += right;
                }
                else
                {
                    value -= right;
                }
            }
            else
            {
                // +, - 以外なら式終端
                break;
            }
        }

        return value;
    }

    /// <summary>
    /// Term := Factor (('*') Factor)*
    /// </summary>
    private float ParseTerm()
    {
        float value = ParseFactor();

        while (pos < formula.Length)
        {
            char c = formula[pos];
            if (c == '*')
            {
                pos++; // '*'を読み飛ばす
                float right = ParseFactor();
                value *= right;
            }
            else
            {
                // * 以外ならterm終端
                break;
            }
        }

        return value;
    }

    /// <summary>
    /// Factor := '(' Expression ')' | StatNameOrNumber
    /// </summary>
    private float ParseFactor()
    {
        SkipSpaces();

        if (pos < formula.Length && formula[pos] == '(')
        {
            // 括弧付き式
            pos++; // '('を飛ばす
            float insideValue = ParseExpression();
            SkipSpaces();
            if (pos < formula.Length && formula[pos] == ')')
            {
                pos++; // ')'を飛ばす
            }
            // 本来は、')'がない場合のエラー処理などを挟む
            return insideValue;
        }
        else
        {
            // 括弧でなければステータス名 or 数値
            return ParseStatNameOrNumber();
        }
    }

    /// <summary>
    /// ステータス名("ap", "hp"など) or 数値("3.14"など)をパース
    /// 例:  "ap" -> 取得 / "123" -> float.Parse
    /// </summary>
    private float ParseStatNameOrNumber()
    {
        SkipSpaces();

        int start = pos;
        // ステータス or 数値トークンを最後まで読む
        // 例: ap, hp, 123, 0.3, など
        while (pos < formula.Length)
        {
            char c = formula[pos];
            // 半角英数字 or '.' は対象
            if (char.IsLetterOrDigit(c) || c == '.' || c == '_')
            {
                pos++;
            }
            else
            {
                // 演算子や括弧に当たったら終わり
                break;
            }
        }
        // 切り出し
        string token = formula.Substring(start, pos - start);

        // ParseStatを呼んでステータス or 数値変換
        float v = ParseStat(token);

        return v;
    }

    /// <summary>
    /// 既存実装にあった "ap", "ad", ... or 数値を返すメソッド
    /// </summary>
    private float ParseStat(string statName)
    {
        // 既存のUnit取得
        Unit unit = targetingManager.GetUnit(fieldNum);
        if (unit == null) return 0f;

        statName = statName.Trim();
        switch (statName)
        {
            case "hp": return unit.hp;
            case "hpMax": return unit.hpMax;
            case "mp": return unit.mp;
            case "mpMax": return unit.mpMax;
            case "ad": return unit.ad;
            case "ap": return unit.ap;
            case "ar": return unit.ar;
            case "sp": return unit.sp;
            default:
                // 数値としてTryParse
                if (float.TryParse(statName, out float val))
                    return val;
                // 不明なトークンは0
                return 0f;
        }
    }

    private void SkipSpaces()
    {
        while (pos < formula.Length && formula[pos] == ' ')
        {
            pos++;
        }
    }

    private void ShowFloatingText(Unit unit, string text)
    {
        if (floatingTextPrefab == null) return;

        // Unitの位置の少し上に表示する例
        // 例: unit.transform.position + Vector3.up * 2
        float rndX = Random.Range(0.1f, 1.5f);
        float rndY = Random.Range(0.1f, 2.0f);
        Vector3 spawnPos = unit.transform.position + new Vector3(0.1f + rndX, 0.1f+rndY, 0); ;

        // Canvas上に配置するなら、WorldToScreenPoint等で座標変換が必要
        // 今回簡単にワールド座標に直接Prefabを置く例：
        GameObject floatObj = GameObject.Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity,canvas.transform);
        // テキスト設定
        FloatingTextController ft = floatObj.GetComponent<FloatingTextController>();
        if (ft != null)
        {
            ft.SetText(text);
        }
    }
}
