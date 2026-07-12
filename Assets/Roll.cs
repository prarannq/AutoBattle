using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roll : MonoBehaviour
{
    private bool isDragging;
    private Vector3 startPosition;

    void Update()
    {
        // マウスの位置を取得
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // z座標は変更しない

        if (isDragging)
        {
            // ドラッグ中はオブジェクトをマウスの位置に追従させる
            transform.position = mousePosition;
        }

        // マウスボタンを押したときの処理
        if (Input.GetMouseButtonDown(0))
        {
            Collider2D collider = Physics2D.OverlapPoint(mousePosition);
            if (collider != null && collider.transform == transform)
            {
                // オブジェクトをドラッグ開始
                isDragging = true;
                startPosition = transform.position;
            }
        }

        // マウスボタンを離したときの処理
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            Collider2D[] colliders = Physics2D.OverlapPointAll(mousePosition);
            foreach (Collider2D collider in colliders)
            {
                if (collider != null && collider.transform != transform && collider.CompareTag("Roll"))
                {
                    Debug.Log("Hello, Unity Console!");
                    // 別のオブジェクトと位置を交換
                    Vector3 otherPosition = collider.transform.position;
                    collider.transform.position = startPosition;
                    transform.position = otherPosition;
                }
                else
                {
                    Debug.Log(collider.transform);
                    // 別のオブジェクトがなければ元の位置に戻す
                    transform.position = startPosition;
                }
            }
        }
    }
}
