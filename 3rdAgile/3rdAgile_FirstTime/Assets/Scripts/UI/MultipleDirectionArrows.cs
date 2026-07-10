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

    [Header("追跡する対象タグ")]
    [SerializeField] private string targetTag = "";

    [Header("矢印UIのRectTransform")]
    [SerializeField] private RectTransform[] arrowRect = null;

    [Header("矢印を中心からどれだけ離して表示するか")]
    [SerializeField] private float radius = 120.0f;

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
        if (player == null || arrowRect == null || arrowRect.Length == 0)
        {
            SetArrowVisible(false);
            return;
        }

        // 現在向くべき対象を探す
        Transform[] currentTarget = FindTarget();

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
        int count = Mathf.Min(currentTarget.Length, arrowRect.Length);
        if (count == 0) return;

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

            // 高さ方向は無視して、XZ平面上の方向だけを見る
            direction.y = 0.0f;

            // 対象に一定距離近づいたとき矢印を非表示にする
            if (direction.sqrMagnitude <= DISTANCE_TO_HIDE_ARROW)
            {
                SpecificSetArrowVisible(false, i);
                continue;
            }

            // 長さを1に正規化して向きだけ取り出す
            direction.Normalize();

            // 3D空間のXZ方向を、UI用の2Dベクトルに変換
            Vector2 dir2D = new Vector2(direction.x, direction.z);

            // 中心から指定半径だけ離した位置に矢印を配置する
            arrowRect[i].anchoredPosition = dir2D * radius;

            // 矢印画像の向きを対象方向に合わせる
            float angle = Mathf.Atan2(dir2D.y, dir2D.x) * Mathf.Rad2Deg;
            arrowRect[i].localRotation = Quaternion.Euler(0f, 0f, angle - 90f);

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
        foreach (RectTransform rect in arrowRect)
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
        if (index < 0 || index >= arrowRect.Length) return;
        // 矢印UIの参照が設定されていない場合は処理を行わない
        if (arrowRect[index] == null) return;

        // 現在の表示状態と違う場合のみ切り替える
        if (arrowRect[index].gameObject.activeSelf != visible)
        {
            arrowRect[index].gameObject.SetActive(visible);
        }
    }
}
