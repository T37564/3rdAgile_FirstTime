using UnityEngine;
using Fusion;
using UnityEngine.AI;

public class GuardianWanderingArea : NetworkBehaviour
{
    private float shortestDistance = 0.0f;

    // 徘徊する際の向かう座標
    private Transform wanderingGroundPosition;

    public void FindWanderingGround()
    {
        // Groundタグが付いているオブジェクトを取得
        GameObject[] groundObjects = GameObject.FindGameObjectsWithTag("ItemGround");
        
        // 最小値
        shortestDistance = Mathf.Infinity;

        // Groundタブのオブジェクトで一番近いのを取得する
        foreach (GameObject ground in groundObjects)
        {
            float distance = Vector3.Distance(transform.position,
                ground.transform.position);

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                wanderingGroundPosition = ground.transform;
            }
        }
    }



    public Vector3 GetRandomPoint()
    {
        // 徘徊する地面のBoundsを取得
        Bounds bounds = wanderingGroundPosition.GetComponent<Renderer>().bounds;

        // Bounds内のランダムな座標を生成
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);

        Vector3 randomPoint = new Vector3(randomX, transform.position.y, randomZ);
        //Debug.Log(randomPoint);
        NavMeshHit hit;

        // NavMesh上の有効な位置を取得
        if (NavMesh.SamplePosition(randomPoint,out hit, 2f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return transform.position;
    }
}
