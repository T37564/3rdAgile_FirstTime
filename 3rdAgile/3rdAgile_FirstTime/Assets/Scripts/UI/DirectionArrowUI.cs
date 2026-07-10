// -----------------------------------------------------------------------------------
// 指定したタグを持つオブジェクトを検索し、その方向を示す矢印UIを制御するクラス
// DirectionArrowUI.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using UnityEngine;

public class DirectionArrowUI : MonoBehaviour
{
    // 矢印を非表示にする対象までの距離
    private readonly float DISTANCE_TO_HIDE_ARROW = 2.0f;

    // 矢印のプレイヤーまでの距離
    private readonly float ARROW_ORWARD_DISTANCE = 1.75f;
    // 矢印と地面の距離
    private readonly float ARROW_HEIGHT = 0.2f;

    [Header("追跡する対象タグ")]
    [SerializeField] private string targetTag = "";

    [Header("矢印UIのRectTransform")]
    [SerializeField] private Transform arrow = null;

    // この矢印UIの基準となる、自分が操作しているプレイヤー
    private Transform player = null;

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
        if (player == null || arrow == null)
        {
            SetArrowVisible(false);
            return;
        }

        // 現在向くべき対象を探す
        Transform currentTarget = FindTarget();

        // 対象が見つからなければ矢印を非表示
        if (currentTarget == null)
        {
            SetArrowVisible(false);
            return;
        }

        // プレイヤーから対象への方向
        Vector3 direction = currentTarget.position - player.position;
        direction.y = 0.0f;

        // 距離判定
        if (direction.magnitude <= DISTANCE_TO_HIDE_ARROW)
        {
            SetArrowVisible(false);
            return;
        }

        // 長さを1に正規化して向きだけ取り出す
        direction.Normalize();

        // プレイヤーの少し前に配置
        arrow.position = player.position + direction * ARROW_ORWARD_DISTANCE + Vector3.up * ARROW_HEIGHT;

        // 対象方向を向く
        arrow.rotation = Quaternion.LookRotation(direction);

        // 対象があるので矢印を表示
        SetArrowVisible(true);
    }

    /// <summary>
    /// 指定タグを持つ対象の中から、矢印が向くべき対象を探す
    /// </summary>
    private Transform FindTarget()
    {
        // タグが設定されていなければ対象を探さない
        if (string.IsNullOrEmpty(targetTag)) return null;

        // 指定タグを持つオブジェクトを取得
        GameObject targets = GameObject.FindGameObjectWithTag(targetTag);

        // 対象が見つからなければ終了
        if (targets == null) return null;

        // 見つかったオブジェクトのTransformを返す
        return targets.transform;
    }

    /// <summary>
    /// 矢印UIの表示・非表示を切り替える
    /// </summary>
    private void SetArrowVisible(bool visible)
    {
        // 矢印UIの参照が設定されていない場合は処理を行わない
        if (arrow == null) return;

        // 現在の表示状態と違う場合のみ切り替える
        if (arrow.gameObject.activeSelf != visible)
        {
            arrow.gameObject.SetActive(visible);
        }
    }
}