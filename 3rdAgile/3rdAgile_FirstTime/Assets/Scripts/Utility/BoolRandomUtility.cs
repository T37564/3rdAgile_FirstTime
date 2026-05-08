using UnityEngine;

public static class BoolRandomUtility
{
    public static bool RandomBool()
    {
        return Random.Range(0, 2) == 1;
    }
}
