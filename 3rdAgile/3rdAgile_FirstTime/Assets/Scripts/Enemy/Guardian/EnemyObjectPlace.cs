using NUnit.Framework;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static Unity.VisualScripting.Metadata;

public class EnemyObjectPlace : MonoBehaviour
{
    private List<StageTypeKinds> stageTypeKindList = new();

    private Dictionary<StageTypeKinds, List<BoxCollider>> groundDictionary = new();

    

    public void RegisterGrounds()
    {
        groundDictionary.Clear();
        
        //stageTypeKindList.Clear();

        StageTypeSetting[] stages = FindObjectsByType<StageTypeSetting>(FindObjectsSortMode.None);

        foreach (StageTypeSetting stage in stages)
        {
            StageTypeKinds type = stage.StageType;
            if(!groundDictionary.ContainsKey(type))
            {
                groundDictionary[type] = new List<BoxCollider>();
            }

            Transform[] children = stage.GetComponentsInChildren<Transform>();
            //Debug.Log($"敵出現地面の登録数: {enemyGroundColliders.Count}");
            foreach (Transform child in children)
            {
                Debug.Log($"敵出現地面: {child.name}");
                if (!child.CompareTag("ItemGround"))
                {
                    Debug.Log("ItemGroundタグがついていないオブジェクトをスキップ: " + child.name);
                    continue;
                }

                BoxCollider boxCollider = child.GetComponent<BoxCollider>();
                if (boxCollider != null)
                {
                    Debug.Log("ItemGround登録: " + child.name);
                    groundDictionary[type].Add(boxCollider);
                }
            }
        }
    }

    public Vector3 GetRandomPosition(StageTypeKinds stageType)
    {
        List<BoxCollider> grounds = new();
        StageTypeSetting[] stages = FindObjectsByType<StageTypeSetting>(FindObjectsSortMode.None);
        foreach (StageTypeSetting stage in stages)
        {
            if (stage.StageType != stageType)
            {
                continue;
            }
            Transform[] children=stage.GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
            {
                if (!child.CompareTag("ItemGround"))
                {
                    continue;
                }
                BoxCollider boxCollider = child.GetComponent<BoxCollider>();

                if (boxCollider != null)
                {
                    Debug.Log(
                            $"ItemGround発見: {child.name}, " +
                            $"位置: {child.position}, " +
                            $"Bounds: {boxCollider.bounds}"
                        );
                    grounds.Add(boxCollider);
                }
            }
        }

        if (grounds.Count == 0)
        {
            Debug.LogError(
                $"{stageType} の敵出現用地面がありません"
            );

            return Vector3.zero;
        }
        BoxCollider ground = grounds[Random.Range(0, grounds.Count)];

        Debug.Log(
    $"Ground確認: {ground.name}\n" +
    $"Transform.position = {ground.transform.position}\n" +
    $"Bounds.center = {ground.bounds.center}\n" +
    $"Bounds.min = {ground.bounds.min}\n" +
    $"Bounds.max = {ground.bounds.max}"
);

        //int index=Random.Range(0, grounds.Count);
        //BoxCollider ground = enemyGroundColliders[index];
        Physics.SyncTransforms();
        Bounds boxBounds=ground.bounds;

        float x = Random.Range(boxBounds.min.x + 0.5f, boxBounds.max.x - 0.5f);
        float z = Random.Range(boxBounds.min.z + 0.5f, boxBounds.max.z - 0.5f);
        float y=boxBounds.max.y;

        Vector3 position = new Vector3(x, y, z);
        Debug.Log($"生成位置: {position}");
        
        return position;
    }
}
