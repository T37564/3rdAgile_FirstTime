using UnityEngine;

public class CoroutineRunner : MonoBehaviour
{
    public static CoroutineRunner Instance;

    void Awake()
    {
        Instance = this;
    }
}
