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
        //GameObject target = GameObject.FindWithTag("Player");

        //if (target != null)
        //{
        //    //navMeshAgent.SetDestination(players.position);

        //    players = target.transform;
        //}
        //else if (target == null)
        //{
        //    target = GameObject.FindWithTag("Player");
        //    //Debug.LogError("Playerタグのオブジェクトが見つかりませんでした。");
        //}
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        Debug.Log($"Player数:{players.Length}");
    }

    private void Update()
    {
        if (Object == null) return;
    }

    public override void FixedUpdateNetwork()
    {
        if(!HasStateAuthority) return;

        FindNearestPlauer();


        if (currentPlayer != null)
        {
            Vector3 targetPos = currentPlayer.position;

            // ガーディアンから見たプレイヤーの方向
            currentDistance = Vector3.Distance(transform.position, currentPlayer.position);

            // ガーディアンからプレイヤーの方向にRayを飛ばし
            // searchRangeの範囲にRayが当たるか判定する
            if (currentDistance <= searchRange)
            {
                targetPos.y = transform.position.y;
                navMeshAgent.SetDestination(targetPos);

            }
        }
    }

    /// <summary>
    /// プレイヤーの中で一番近いプレイヤーを追いかけるメソッド
    /// </summary>
    private void FindNearestPlauer()
    {
        GameObject[] playersObject = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log($"見つかったプレイヤー数:{playersObject.Length}");

        float shortestDistance = Mathf.Infinity;
        Transform nearestPlayer = null;

        foreach(GameObject playerObject in playersObject)
        {
            if (playerObject == null) continue;

            float distance = Vector3.Distance(transform.position, playerObject.transform.position);

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestPlayer = playerObject.transform;
            }
        }

        currentPlayer =nearestPlayer;
    }
}
