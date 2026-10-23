//=========================================================================
// 制作者：スズキ
//=========================================================================

using UnityEngine;
using Fusion;

public abstract class SingletonNetworkBehaviour<T> : NetworkBehaviour where T : SingletonNetworkBehaviour<T>
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<T>();

                if (instance == null)
                {
                    Debug.LogError($"{typeof(T).Name}の Instance が見つかりません");
                }
            }
            return instance;
        }
    }

    public static bool HasInstance => instance != null;

    public override void Spawned()
    {
        base.Spawned();

        if (instance == null)
        {
            instance = (T)this;
            OnRegistered();
            return;
        }
        if (instance == this) return;

        Debug.LogWarning($"{typeof(T).Name}が重複しています。：{name}");

        if (Object != null && Object.HasStateAuthority)
            Runner.Despawn(Object);
        else
            gameObject.SetActive(false);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (instance == this) instance = null;
        base.Despawned(runner, hasState);
    }

    protected virtual void OnRegistered() { }
}
