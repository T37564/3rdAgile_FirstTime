using UnityEngine;

public class ItemChecker : MonoBehaviour
{
    [Header("自分自身の位置")]
    [SerializeField] private Transform playerTransform = null;

    // アイテム探知距離
    private float itemDetectionRange = 2.5f;

    private void OnTriggerEnter(Collider other)
    {
        // 何かしらのアイテムに触れた場合
        if(other.CompareTag("Item"))
        {

        }
    }
}
