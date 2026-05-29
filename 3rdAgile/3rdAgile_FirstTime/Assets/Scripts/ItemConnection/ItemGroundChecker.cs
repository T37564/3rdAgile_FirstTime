using UnityEngine;

public class ItemGroundChecker : MonoBehaviour
{
    [SerializeField] private LayerMask deliveryOfMaterialsArea;

    [SerializeField] private float rayLength = 1f;

    [SerializeField] private Vector3 boxSize = new Vector3(1.0f, 1.0f, 1.0f);

    
    private void Update()
    {
        if(IsFullyGrounded())
        {
            Debug.Log("アイテムが完全に納品エリアに接地しています。");
        }
    }

    /// <summary>
    /// アイテムが完全に納品エリアに接地しているかを確認する
    /// </summary>
    private bool IsFullyGrounded()
    {
        Vector3 center = transform.position;

        float halfX = boxSize.x * 0.5f;
        float halfY = boxSize.y * 0.5f;
        float halfZ = boxSize.z * 0.5f;

        Vector3[] checkPoints =
        {
            center + new Vector3(halfX,-halfY, halfZ), // 前右
            center + new Vector3(-halfX,-halfY, halfZ), // 前左
            center + new Vector3(halfX,-halfY, -halfZ), // 後右
            center + new Vector3(-halfX,-halfY, -halfZ) // 後左
        };

        foreach(Vector3 point in checkPoints)
        {
            bool isHit = Physics.Raycast(point, Vector3.down, rayLength, deliveryOfMaterialsArea);

            if (!isHit)
            {
                return false;
            }
        }

        return true;

    }
}
