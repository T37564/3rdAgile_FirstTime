using UnityEngine;

public class StageTypeSetting : MonoBehaviour
{
    [SerializeField] private StageTypeKinds stageType;

    public StageTypeKinds StageType => stageType;
}
