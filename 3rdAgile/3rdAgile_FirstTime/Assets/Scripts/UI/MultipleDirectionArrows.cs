// -----------------------------------------------------------------------------------
// 指定したタグを持つ複数のオブジェクトを検索し、その方向を示す矢印UIを制御するクラス
// MultipleDirectionArrows.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using UnityEngine;
using System.Collections.Generic;

public class MultipleDirectionArrows : MonoBehaviour
{
    // 矢印を非表示にする対象までの距離
    private readonly float DISTANCE_TO_HIDE_ARROW = 2.5f;

    // 矢印のプレイヤーまでの距離
    private readonly float ARROW_ORWARD_DISTANCE = 1.75f;
    // 矢印と地面の距離
    private readonly float ARROW_HEIGHT = 0.2f;

    [Header("追跡する対象タグ")]
    [SerializeField] private string targetTag = "";

    [Header("矢印UIのRectTransform")]
    [SerializeField] private Transform[] arrows = null;

    // この矢印UIの基準となる、自分が操作しているプレイヤー
    private Transform player = null;

    // アイテムに使用するとき
    public bool isItem = false;

    /// <summary>
    /// 矢印の基準となるプレイヤーを外部から設定する
    /// </summary>
    public void SetPlayer(Transform targetPlayer)
    {
        player = targetPlayer;
    }

    /// <summary>
    /// 毎フレーム矢印UIの表示と向きを更新する
    /// </summary>
    private void Update()
    {
        UpdateArrow();
    }

    /// <summary>
    /// 矢印UIの表示位置と向きを更新する
    /// </summary>
    private void UpdateArrow()
    {
        // プレイヤーまたは矢印UIが未設定なら表示しない
        if (player == null || arrows == null || arrows.Length == 0)
        {
            SetArrowVisible(false);
            return;
        }

        // 現在向くべき対象を探す
        Transform[] currentTarget = FindTarget();
        if (currentTarget == null || currentTarget.Length == 0)
        {
            SetArrowVisible(false);
            return;
        }

        // 複数人で運ぶアイテムの時
        if (isItem)
        {
            // 複数人で運ぶアイテムを誰かが持っていた時に入れとく
            List<Transform> targetList = new();

            for (int i = 0; i < currentTarget.Length; i++)
            {
                // 参照用
                ItemInteractable item = currentTarget[i].GetComponent<ItemInteractable>();

                // 存在チェック
                if (item == null)
                    continue;
                if (item.Object == null)
                    continue;
                if (!item.Object.IsValid)
                    continue;

                // 複数人アイテムを誰かが持っているとき
                if (item.IsHelpPeople)
                {
                    targetList.Add(currentTarget[i]);
                }
            }

            // 入れなおす
            currentTarget = targetList.ToArray();
        }


        // 矢印UIの数と対象の数を比較して、少ない方の数を取得
        int count = Mathf.Min(currentTarget.Length, arrows.Length);
        if (currentTarget.Length == 0 && count == 0)
        {
            SetArrowVisible(false);
            return;
        }

        for (int i = 0; i < count; i++)
        {
            // 対象が見つからなければ矢印を非表示
            if (currentTarget[i] == null)
            {
                SpecificSetArrowVisible(false, i);
                continue;
            }

            // プレイヤーから対象までの方向ベクトルを取得
            Vector3 direction = currentTarget[i].position - player.position;
            direction.y = 0.0f;

            // 対象に一定距離近づいたとき矢印を非表示にする
            if (direction.magnitude <= DISTANCE_TO_HIDE_ARROW)
            {
                SpecificSetArrowVisible(false, i);
                continue;
            }

            // 長さを1に正規化して向きだけ取り出す
            direction.Normalize();

            // プレイヤーの少し前に配置
            arrows[i].position = player.position + direction * ARROW_ORWARD_DISTANCE + Vector3.up * ARROW_HEIGHT;

            // 対象方向を向く
            arrows[i].rotation = Quaternion.LookRotation(direction);

            // 対象があるので矢印を表示
            SpecificSetArrowVisible(true, i);
        }
    }

    /// <summary>
    /// 指定タグを持つ対象の中から、矢印が向くべき対象を探す
    /// </summary>
    private Transform[] FindTarget()
    {
        // タグが設定されていなければ対象を探さない
        if (string.IsNullOrEmpty(targetTag)) return null;

        // 指定タグを持つオブジェクトを取得
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);

        // 対象が見つからなければ終了
        if (targets.Length == 0) return null;

        // 見つかったオブジェクトのTransformを配列に格納
        Transform[] transforms = new Transform[targets.Length];
        for (int i = 0; i < targets.Length; i++)
        {
            transforms[i] = targets[i].transform;
        }

        // 見つかったオブジェクトのTransformを返す
        return transforms;
    }

    /// <summary>
    /// 矢印UIの表示・非表示を切り替える
    /// </summary>
    private void SetArrowVisible(bool visible)
    {
        foreach (Transform rect in arrows)
        {
            // 矢印UIの参照が設定されていない場合は処理を行わない
            if (rect == null) continue;

            // 現在の表示状態と違う場合のみ切り替える
            if (rect.gameObject.activeSelf != visible)
            {
                rect.gameObject.SetActive(visible);
            }
        }
    }

    /// <summary>
    /// 特定の矢印の表示・非表示を切り替える
    /// </summary>
    private void SpecificSetArrowVisible(bool visible, int index)
    {
        // インデックスが範囲外なら処理を行わない
        if (index < 0 || index >= arrows.Length) return;
        // 矢印UIの参照が設定されていない場合は処理を行わない
        if (arrows[index] == null) return;

        // 現在の表示状態と違う場合のみ切り替える
        if (arrows[index].gameObject.activeSelf != visible)
        {
            arrows[index].gameObject.SetActive(visible);
        }
    }
}
