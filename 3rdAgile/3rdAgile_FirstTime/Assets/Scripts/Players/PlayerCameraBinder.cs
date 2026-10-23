//=========================================================================
// 制作者：スズキ
//=========================================================================

using UnityEngine;
using Fusion;

public class PlayerCameraBinder : NetworkBehaviour
{
    public override void Spawned()
    {
        if(!Object.HasInputAuthority) return;

        PlayerTracking playerTracking = Camera.main.GetComponent<PlayerTracking>();
        playerTracking.SetTarget(transform);
        CameraWallOcclusion cameraWallOcclusion = Camera.main.GetComponent<CameraWallOcclusion>();
        cameraWallOcclusion.SetTarget(transform);
    }
}
