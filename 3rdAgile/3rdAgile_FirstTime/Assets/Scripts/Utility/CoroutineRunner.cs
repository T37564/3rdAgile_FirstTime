// -----------------------------------------------------------------------------------
// どのクラスからでもCoroutineを実行できるようにするクラス
// CoroutineRunner.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using UnityEngine;

public class CoroutineRunner : MonoBehaviour
{
    // 他クラスから参照するためのInstance
    public static CoroutineRunner Instance;

    /// <summary>
    /// 起動時に自身をInstanceへ登録する
    /// </summary>
    private void Awake()
    {
        Instance = this;
    }
}
