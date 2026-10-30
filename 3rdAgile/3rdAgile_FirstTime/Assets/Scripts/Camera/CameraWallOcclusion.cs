
using System.Collections.Generic;
using UnityEngine;

public class CameraWallOcclusion : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTarget;

    [Header("Settings")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float sphereRadius = 0.3f;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private readonly HashSet<OccludableWall> hiddenWalls = new();
    private readonly HashSet<OccludableWall> detectedWalls = new();

    private readonly RaycastHit[] hits = new RaycastHit[128];

    private void LateUpdate()
    {
        if (playerTarget == null)
        {
            RestoreWalls();
            return;
        }

        detectedWalls.Clear();

        Vector3 start = transform.position;
        Vector3 direction = playerTarget.position - start;
        float distance = direction.magnitude;

        if (distance < 0.001f)
        {
            RestoreWalls();
            return;
        }

        direction.Normalize();

        int count = Physics.SphereCastNonAlloc(
            start,
            sphereRadius,
            direction,
            hits,
            distance,
            wallLayer,
            QueryTriggerInteraction.Ignore
        );

        if (showDebug)
        {
            Debug.DrawRay(
                start,
                direction * distance,
                Color.yellow
            );
        }

        for (int i = 0; i < count; i++)
        {
            Collider collider = hits[i].collider;

            if (collider == null)
                continue;

            OccludableWall wall =
                collider.GetComponentInParent<OccludableWall>();

            if (wall == null)
            {
                continue;
            }

            detectedWalls.Add(wall);
        }

        foreach (OccludableWall wall in detectedWalls)
        {
            if (hiddenWalls.Add(wall))
                wall.SetVisible(false);
        }

        hiddenWalls.RemoveWhere(wall =>
        {
            if (wall == null)
                return true;

            if (detectedWalls.Contains(wall))
                return false;

            wall.SetVisible(true);
            return true;
        });
    }

    private void RestoreWalls()
    {
        foreach (OccludableWall wall in hiddenWalls)
        {
            if (wall != null)
                wall.SetVisible(true);
        }

        hiddenWalls.Clear();
        detectedWalls.Clear();
    }

    private void OnDisable()
    {
        RestoreWalls();
    }

    public void SetTarget(Transform target)
    {
        playerTarget = target;
    }
}
