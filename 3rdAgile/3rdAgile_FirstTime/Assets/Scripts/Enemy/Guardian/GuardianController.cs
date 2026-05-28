using Fusion;
using UnityEngine;
using UnityEngine.AI;


public class GuardianController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent navMeshAgent;

    public Transform players;

    private void Start()
    {
        // Playerタグのオブジェクトを探す
        //GameObject target = GameObject.FindWithTag("Player");

        //if (target != null)
        //{
        //    navMeshAgent.SetDestination(players.position);

        //    players = target.transform;
        //}
        //else if(target == null)
        //{
        //    target = GameObject.FindWithTag("Player");
        //    //Debug.LogError("Playerタグのオブジェクトが見つかりませんでした。");
        //}
    }

    // Update is called once per frame
    private void Update()
    {
        // プレイヤーがまだ見つかっていない
        if (players == null)
        {
            FindPlayer();
            return;
        }

        if (players != null)
        {
            Vector3 targetPos = players.position;
            targetPos.y = transform.position.y;
            navMeshAgent.SetDestination(targetPos);

            //Debug.Log(navMeshAgent.velocity);
        }
    }

    private void FindPlayer()
    {
        GameObject target = GameObject.FindWithTag("Player");

        if (target != null)
        {
            players = target.transform;

            Debug.Log("Player取得成功");
        }
    }

    /// <summary>
    /// プレイヤーの中で一番近いプレイヤーを追いかけるメソッド
    /// </summary>
    private void FindNearestPlauer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float shortestDistance = Mathf.Infinity;
        Transform nearestPlayer = null;

        foreach(GameObject playerObject in players)
        {

        }
    }
}
