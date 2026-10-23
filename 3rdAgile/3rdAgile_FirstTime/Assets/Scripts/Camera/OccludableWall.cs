using UnityEngine;

public class OccludableWall : MonoBehaviour
{
    private Renderer[] renderers;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    public void SetVisible(bool visible)
    {
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = visible;
            }
        }
    }
}