using Network.Player;
using UnityEngine;

public class DirectionArrowUI : MonoBehaviour
{
    [Header("追跡する対象タグ")]
    [SerializeField] private string targetTag = "";

    [Header("矢印UIのRectTransform")]
    [SerializeField] private RectTransform arrowRect = null;

    [Header("矢印を中心からどれだけ離して表示するか")]
    [SerializeField] private float radius = 120.0f;

    [Header("同じタグの対象が複数ある場合、一番近いものを向くか")]
    [SerializeField] private bool useNearestTarget = true;

    [Header("味方プレイヤー用の矢印などで自分自身を除外するか")]
    [SerializeField] private bool ignoreSelf = false;

    // この矢印UIの基準となる、自分が操作しているプレイヤー
    private Transform player = null;

    // 現在追跡している対象
    private Transform currentTarget = null;

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
        // プレイヤー、矢印UI、中心位置のどれかが未設定なら表示しない
        if (player == null || arrowRect == null )
        {
            SetArrowVisible(false);
            return;
        }

        // 現在向くべき対象を探す
        currentTarget = FindTarget();

        // 対象が見つからなければ矢印を非表示
        if (currentTarget == null)
        {
            SetArrowVisible(false);
            return;
        }

        // プレイヤーから対象までの方向ベクトルを取得
        Vector3 direction = currentTarget.position - player.position;

        // 高さ方向は無視して、XZ平面上の方向だけを見る
        direction.y = 0.0f;

        // プレイヤーと対象がほぼ同じ位置なら矢印を表示しない
        if (direction.sqrMagnitude <= 0.001f)
        {
            SetArrowVisible(false);
            return;
        }

        // 長さを1に正規化して向きだけ取り出す
        direction.Normalize();

        // 3D空間のXZ方向を、UI用の2Dベクトルに変換
        Vector2 dir2D = new Vector2(direction.x, direction.z);

        // 中心から指定半径だけ離した位置に矢印を配置する
        arrowRect.anchoredPosition = dir2D * radius;

        // 矢印画像の向きを対象方向に合わせる
        float angle = Mathf.Atan2(dir2D.y, dir2D.x) * Mathf.Rad2Deg;
        arrowRect.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);

        // 対象があるので矢印を表示
        SetArrowVisible(true);
    }

    /// <summary>
    /// 指定タグを持つ対象の中から、矢印が向くべき対象を探す
    /// </summary>
    private Transform FindTarget()
    {
        // タグが設定されていなければ対象を探せない
        if (string.IsNullOrEmpty(targetTag)) return null;

        // 指定タグを持つオブジェクトをすべて取得
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);

        // 対象が1つも無ければ終了
        if (targets == null || targets.Length == 0) return null;

        Transform result = null;
        float minSqrDistance = float.MaxValue;

        foreach (GameObject obj in targets)
        {
            if (obj == null) continue;

            // 自分自身を除外したい場合はスキップ
            if (ignoreSelf && player != null && obj.transform == player) continue;

            // 一番近いものを使わない場合は、最初に見つかった対象をそのまま返す
            if (!useNearestTarget)
            {
                return obj.transform;
            }

            // プレイヤーから対象までの距離を計算
            float sqrDistance = (obj.transform.position - player.position).sqrMagnitude;

            // これまでで一番近い対象なら更新
            if (sqrDistance < minSqrDistance)
            {
                minSqrDistance = sqrDistance;
                result = obj.transform;
            }
        }

        // 最終的に見つかった対象を返す
        return result;
    }

    /// <summary>
    /// 矢印UIの表示・非表示を切り替える
    /// </summary>
    private void SetArrowVisible(bool visible)
    {
        if (arrowRect == null) return;

        // 現在の表示状態と違う場合のみ切り替える
        if (arrowRect.gameObject.activeSelf != visible)
        {
            arrowRect.gameObject.SetActive(visible);
        }
    }
}