using UnityEngine;

public class PlayerTracking : MonoBehaviour
{
    // プレイヤーの位置に対して、カメラがどれくらい離れているかを定義するオフセット
    private readonly Vector3 OFFSET = new Vector3(0f, 5f, -7f);

    [Header("自分のプレイヤーのTransform")]
    [SerializeField] private Transform target = null;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void LateUpdate()
    {
        if (target == null) return;
        // 常にプレイヤーの位置を追いかける
        transform.position = target.position + OFFSET;
        transform.LookAt(target);
    }
}
