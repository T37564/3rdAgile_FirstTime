
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MeshCombiner : MonoBehaviour
{
    [SerializeField] private MeshFilter meshFilter;

    [ContextMenu("SubMeshを結合して保存")]
    private void CombineAndSave()
    {
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();

        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError("MeshFilterまたはMeshがありません");
            return;
        }

        Mesh sourceMesh = meshFilter.sharedMesh;

        CombineInstance[] combineInstances =
            new CombineInstance[sourceMesh.subMeshCount];

        for (int i = 0; i < sourceMesh.subMeshCount; i++)
        {
            combineInstances[i].mesh = sourceMesh;
            combineInstances[i].subMeshIndex = i;
            combineInstances[i].transform = Matrix4x4.identity;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.name = sourceMesh.name + "_Combined";

        combinedMesh.CombineMeshes(
            combineInstances,
            true,
            false
        );

#if UNITY_EDITOR
        // 保存先フォルダ
        string folderPath = "Assets/PolyKebap/Meshes";

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Meshes");
        }

        // 保存するアセットのパス
        string assetPath =
            AssetDatabase.GenerateUniqueAssetPath(
                folderPath + "/" + combinedMesh.name + ".asset"
            );

        // Project内に保存
        AssetDatabase.CreateAsset(combinedMesh, assetPath);
        AssetDatabase.SaveAssets();

        // 保存したMeshを設定
        meshFilter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);

        Debug.Log($"Mesh保存完了: {assetPath}");
#else
        Debug.LogWarning("Meshの保存はUnity Editor上で実行してください");
#endif
    }
}
