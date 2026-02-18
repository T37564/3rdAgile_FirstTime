using System;
using UnityEngine;

/// <summary>
/// ランダム生成時にアイテムが地面に配置されていない場合、再配置するクラス
/// </summary>
public class RegenerationCallOut : MonoBehaviour
{
    [Header("アイテムを配置する際のレイヤーマスク")]
    [SerializeField] private LayerMask layerMask;

    [Header("rayの長さ")]
    [SerializeField] private float rayLength = 0.0f;

    //アイテムが地面に配置されていない場合、再配置するためのフラグ
    public bool isGenerateRequest = false;

    //誰かに知らせるためのイベント
    public Action<RegenerationCallOut> OnNeedRegenerate;

    private void Update()
    {
        if(transform.position.y <= -10 && !IsGround())
        {
            isGenerateRequest = true;
            Debug.LogWarning("アイテムが地面に配置されていないので再配置するよう要請する");

            //RegenerationCallOutクラスにイベントを通知する
            OnNeedRegenerate.Invoke(this);
        }
    }

    private bool IsGround()
    {
        return Physics.Raycast(transform.position, Vector3.down, rayLength, layerMask);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * rayLength);
    }
}
