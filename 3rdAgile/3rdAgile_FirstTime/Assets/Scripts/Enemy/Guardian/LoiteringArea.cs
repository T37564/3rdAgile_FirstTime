using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 敵の徘徊処理
/// 敵が徘徊するエリアがまだ取得できない状態なので処理だけ書いて動作確認していない
/// </summary>
public class LoiteringArea : MonoBehaviour
{
    [SerializeField] private NavMeshAgent navMeshAgent;

    // 徘徊させる範囲の座標
    // 今現在徘徊させる座標を取得できない状態
    private Vector3 literingAreaSize;

    private void Start()
    {
        MoveRandomPoint();
    }

    private void Update()
    {
        // 取得したランダムな座標に到着したら次の地点に移動する
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 1.0f)
        {
            MoveRandomPoint();
        }
    }

    public void MoveRandomPoint()
    {
        //Vector3 targetPos=徘徊するランダムな座標を呼び出す;

        //navMeshAgent.SetDestination(targetPos);
    }
}
