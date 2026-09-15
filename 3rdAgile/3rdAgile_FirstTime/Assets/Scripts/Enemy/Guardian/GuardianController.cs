using Fusion;
using Network.Player;
using UnityEngine;
using UnityEngine.AI;


public class GuardianController : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent navMeshAgent;

    [SerializeField] private GuardianWanderingArea guardianWanderingArea;

    public Transform currentPlayer { get; private set; }

    public float currentDistance {  get; private set; }

    [Header("索敵範囲の距離")]
    [SerializeField] public float searchRange = 0.0f;

    // 徘徊中かどうか
    private bool isWandering = false;

    // 現在の徘徊目的地
    private Vector3 wanderingPoint;

    // 徘徊目的地に到着したと判断する距離
    [SerializeField] private float wanderingArrivalDistance = 1.0f;

    public override void Spawned()
    {
        // Playerタグのオブジェクトを探す
        //GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        Debug.Log($"[Enemy Spawned] {gameObject.name} Position={transform.position}");

        guardianWanderingArea.FindWanderingGround();
    }

    private void Update()
    {
        if (Object == null) return;
    }

    public override void FixedUpdateNetwork()
    {
        if(!HasStateAuthority) return;
        //Debug.Log($"[AI開始前] {transform.position}");
        FindNearestPlayer();

        PlayerController playerController = currentPlayer?.GetComponent<PlayerController>();


        if (playerController == null)
        {
            return;
        }
        Debug.Log($"IsInStartArea = {playerController.IsInStartArea}");

        if (playerController.IsInStartArea)
        {
            Debug.Log("プレイヤーはスタートエリア内にいます。徘徊状態に移行します。");
            WanderingState();
        }
        else
        {
            ChaseState();
        }
    }

    private void WanderingState()
    {
        if (!navMeshAgent.isOnNavMesh)
        {
            Debug.LogWarning("NavMeshAgentがNavMesh上にありません。");
            return;
        }
        navMeshAgent.isStopped = false;

        if (!isWandering)
        {
            isWandering = true;

            wanderingPoint = guardianWanderingArea.GetRandomPoint();

            navMeshAgent.SetDestination(wanderingPoint);

            return;
        }

        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance<= wanderingArrivalDistance)
        {
            wanderingPoint = guardianWanderingArea.GetRandomPoint();
            
            navMeshAgent.SetDestination(wanderingPoint);
        }
    }

    private void ChaseState()
    {
        isWandering = false;

        if (!navMeshAgent.isOnNavMesh)
        {
            Debug.LogWarning("NavMeshAgentがNavMesh上にありません。");
            return;
        }

        Vector3 targetPos = currentPlayer.position;

        // ガーディアンから見たプレイヤーの方向
        currentDistance = Vector3.Distance(transform.position, currentPlayer.position);

        if(currentDistance<= searchRange)
        {
            targetPos.y = transform.position.y;

            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(targetPos);
        }
        else
        {
            navMeshAgent.isStopped = true;
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
