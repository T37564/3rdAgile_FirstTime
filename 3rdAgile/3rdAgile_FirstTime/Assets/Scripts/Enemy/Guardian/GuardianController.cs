using Fusion;
using UnityEngine;
using UnityEngine.AI;


public class GuardianController : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent navMeshAgent;

    public Transform currentPlayer { get; private set; }

    public float currentDistance {  get; private set; }

    [Header("索敵範囲の距離")]
    [SerializeField] public float searchRange = 0.0f;

    public override void Spawned()
    {
        // Playerタグのオブジェクトを探す
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        Debug.Log($"[Enemy Spawned] {gameObject.name} Position={transform.position}");
    }

    private void Update()
    {
        if (Object == null) return;
    }

    public override void FixedUpdateNetwork()
    {
        if(!HasStateAuthority) return;
        Debug.Log($"[AI開始前] {transform.position}");
        FindNearestPlayer();


        if (currentPlayer != null)
        {
            Vector3 targetPos = currentPlayer.position;

            // ガーディアンから見たプレイヤーの方向
            currentDistance = Vector3.Distance(transform.position, currentPlayer.position);

            // ガーディアンからプレイヤーの方向にRayを飛ばし
            // searchRangeの範囲にRayが当たるか判定する
            if (currentDistance <= searchRange)
            {
                // 目的地のY座標をガーディアンのY座標に合わせる
                targetPos.y = transform.position.y;

                if(!navMeshAgent.isOnNavMesh)
                {
                    Debug.LogWarning("NavMeshAgentがNavMesh上にありません。");
                    return;
                }
                // ナビメッシュエージェントの目的地を設定
                navMeshAgent.SetDestination(targetPos);
            }
        }
    }

    /// <summary>
    /// プレイヤーの中で一番近いプレイヤーを追いかけるメソッド
    /// </summary>
    private void FindNearestPlayer()
    {
        GameObject[] playersObject = GameObject.FindGameObjectsWithTag("Player");

        // 最短距離を初期化
        float shortestDistance = Mathf.Infinity;
        Transform nearestPlayer = null;

        // プレイヤーの中で一番近いプレイヤーを探す
        foreach (GameObject playerObject in playersObject)
        {
            if (playerObject == null) continue;

            // プレイヤーとの距離を計算
            float distance = Vector3.Distance(transform.position, playerObject.transform.position);

            if (distance < shortestDistance)
            {
                // 最短距離を更新
                shortestDistance = distance;
                // 一番近いプレイヤーを更新
                nearestPlayer = playerObject.transform;
            }
        }
        // 最も近いプレイヤーを設定
        currentPlayer = nearestPlayer;
    }
}
