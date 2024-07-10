using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Freya;
using System.Xml.Serialization;
using MyBox;

public class ParallaxScroll : MonoBehaviour
{
    public new Camera camera;
    public Transform characterTransform;
    public AnimationCurve parallaxCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public int parallaxObjectPoolSize = 10;

    private SpriteRenderer[] parallaxObjectSR;
    private List<Transform[]> parallaxObjectTransforms = new List<Transform[]>();
    private Vector3[] parallaxObjectInitLocalPositions;
    private float[] parallaxObjectWidth;

    private void Awake() {
        parallaxObjectSR = GetComponentsInChildren<SpriteRenderer>();
        parallaxObjectInitLocalPositions = new Vector3[parallaxObjectSR.Length];
        for (int i = 0; i < parallaxObjectSR.Length; i++)
        {
            parallaxObjectInitLocalPositions[i] = parallaxObjectSR[i].transform.localPosition;
        }

        parallaxObjectWidth = new float[parallaxObjectSR.Length];
        for (int i = 0; i < parallaxObjectSR.Length; i++)
        {
            parallaxObjectWidth[i] = parallaxObjectSR[i].bounds.size.x;
        }


        for (int i = 0; i < parallaxObjectSR.Length; i++)
        {
            // setup pivot
            string objectName = parallaxObjectSR[i].gameObject.name;
            Transform pivot = new GameObject(objectName).transform;
            pivot.SetParent(transform);
            pivot.localPosition = parallaxObjectSR[i].transform.localPosition;
            pivot.localScale = Vector3.one;

            // init pool
            parallaxObjectTransforms.Add(new Transform[parallaxObjectPoolSize]);
            parallaxObjectTransforms[i][0] = parallaxObjectSR[i].transform;
            parallaxObjectTransforms[i][0].gameObject.name = $"{objectName}:{0}";
            parallaxObjectTransforms[i][0].SetParent(pivot);
            for (int j = 1; j < parallaxObjectPoolSize; j++)
            {
                parallaxObjectTransforms[i][j] = Instantiate(parallaxObjectSR[i].transform, pivot);
                parallaxObjectTransforms[i][j].gameObject.name = $"{objectName}:{j}";
            }
        }
    }


    private void Update() {
        Vector2 cameraSize = new Vector2(camera.orthographicSize * camera.aspect, camera.orthographicSize) * 2f;
        Rect cameraRect = new Rect(camera.transform.position.ToVector2() - cameraSize * 0.5f, cameraSize);

        for (int i = 0; i < parallaxObjectSR.Length; i++)
        {
            for (int j = 0; j < parallaxObjectPoolSize; j++)
            {
                float offset = parallaxObjectWidth[i] * (j - parallaxObjectPoolSize/2);
                Vector3 position = parallaxObjectInitLocalPositions[i] + characterTransform.localPosition.FlattenY() * parallaxCurve.Evaluate(i / (float)(parallaxObjectSR.Length - 1));
                position.x += offset;
                position.x += Mathf.Floor(camera.transform.position.x / parallaxObjectWidth[i]) * parallaxObjectWidth[i];

                parallaxObjectTransforms[i][j].localPosition = position;


                Rect rect = new Rect(parallaxObjectTransforms[i][j].position - parallaxObjectSR[i].bounds.size * 0.5f, parallaxObjectSR[i].bounds.size);
                if(cameraRect.Overlaps(rect)) {
                    parallaxObjectTransforms[i][j].gameObject.SetActive(true);
                } else {
                    parallaxObjectTransforms[i][j].gameObject.SetActive(false);
                }
            }
        }
    }

    private void OnDrawGizmos() {
        if(!Application.isPlaying)
            return;

        Vector2 cameraSize = new Vector2(camera.orthographicSize * camera.aspect, camera.orthographicSize) * 2f;
        Rect cameraRect = new Rect(camera.transform.position.ToVector2() - cameraSize * 0.5f, cameraSize);

        for (int i = 0; i < parallaxObjectSR.Length; i++)
        {
            for (int j = 0; j < parallaxObjectPoolSize; j++)
            {
                Rect rect = new Rect(parallaxObjectTransforms[i][j].position, parallaxObjectSR[i].bounds.size);
                if(cameraRect.Overlaps(rect)) {
                    Gizmos.color = Color.green * 0.1f;
                } else {
                    Gizmos.color = Color.blue * 0.1f;
                }
                Gizmos.DrawCube(rect.center - rect.size * 0.5f, rect.size);
            }
        }


        Gizmos.color = Color.red * 0.5f;
        Gizmos.DrawCube(cameraRect.center, cameraRect.size);
    }
}
