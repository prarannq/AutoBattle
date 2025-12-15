using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusBar : MonoBehaviour
{
    public Unit unit; // ユニットの参照
    public Vector3 offset = new Vector3(0, 1, 0); // ゲージの位置オフセット
    public GameObject canvas;

    public GameObject hpBarPrefab; // HPバー用Prefab
    public GameObject mpBarPrefab; // MPバー用Prefab
    private GameObject hpBarInstance; // HPバーのインスタンス
    private GameObject mpBarInstance; // MPバーのインスタンス
    private Transform hpFillTransform; // HPバーのFill部分
    private Transform mpFillTransform; // MPバーのFill部分

    void Start()
    {
        canvas = GameObject.Find("Canvas");

    }

    void Update()
    {
        if (unit != null)
        {
            if (hpFillTransform != null || mpFillTransform != null)
            {
                UpdateHpBar();
                UpdateMpBar();

                // HPゲージとMPゲージをユニットの位置に追従
                hpBarInstance.transform.position = transform.position + offset + new Vector3(-1f, 0, 0);
                mpBarInstance.transform.position = transform.position + offset + new Vector3(-1f, -0.1f, 0);
            }
        }
    }

    public void GetStatusBar(Unit owner)
    {
        unit = owner;
        // HPゲージのPrefabをインスタンス化
        hpBarInstance = Instantiate(hpBarPrefab, canvas.transform);
        hpBarInstance.transform.position = transform.position + offset;

        // MPゲージのPrefabをインスタンス化（HPの下に配置）
        mpBarInstance = Instantiate(mpBarPrefab, canvas.transform);
        mpBarInstance.transform.position = transform.position + offset + new Vector3(0, -0.2f, 0);

        // Fill部分を取得
        hpFillTransform = hpBarInstance.transform.Find("Fill");
        mpFillTransform = mpBarInstance.transform.Find("Fill");

        if (hpFillTransform == null || mpFillTransform == null)
        {
            Debug.LogError("Fill部分が見つかりません。Prefabを確認してください。");
        }

        if (unit == null || hpBarPrefab == null || mpBarPrefab == null)
        {
            Debug.LogError("UnitまたはゲージPrefabが設定されていません。");
            return;
        }
    }

    private void UpdateHpBar()
    {
        if (hpFillTransform != null)
        {
            float hpRatio = Mathf.Clamp01((float)unit.hp / unit.hpMax);
            Vector3 scale = hpFillTransform.localScale;
            scale.x = hpRatio;
            hpFillTransform.localScale = scale;
        }
    }

    private void UpdateMpBar()
    {
        if (mpFillTransform != null)
        {
            float mpRatio = Mathf.Clamp01((float)unit.mp / unit.mpMax);
            Vector3 scale = mpFillTransform.localScale;
            scale.x = mpRatio;
            mpFillTransform.localScale = scale;
        }
    }
}
