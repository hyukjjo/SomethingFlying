using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
//using MyBox;
//using Freya;

public class DynamicCamera : MonoBehaviour
{
    public Transform characterTransform;
    private Transform characterPivotTop;
    private Transform characterPivotBottom;
    public new Camera camera;
    public CinemachineTargetGroup targetGroup;
    public Transform backgroundQuadTransform;


    private void Awake() {
        characterPivotTop = new GameObject("characterPivotTop").transform;
        characterPivotTop.SetParent(characterTransform.parent);
        characterPivotTop.gameObject.hideFlags = HideFlags.HideInHierarchy;
        characterPivotTop.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        targetGroup.AddMember(characterPivotTop, 1f, 0f);

        characterPivotBottom = new GameObject("characterPivotBottom").transform;
        characterPivotBottom.SetParent(characterTransform.parent);
        characterPivotBottom.gameObject.hideFlags = HideFlags.HideInHierarchy;
        characterPivotBottom.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        targetGroup.AddMember(characterPivotBottom, 1f, 0f);
    }


    private void LateUpdate() {
        characterPivotTop.localPosition = characterTransform.localPosition + Vector3.up * characterTransform.localPosition.y;
        characterPivotBottom.localPosition = characterTransform.localPosition - Vector3.up * characterTransform.localPosition.y;

        float backgroundQuadScale = camera.orthographicSize * 2f;
        backgroundQuadTransform.localScale = new Vector3(
            backgroundQuadScale * camera.aspect,
            backgroundQuadScale,
            1f
        );
    }
}
