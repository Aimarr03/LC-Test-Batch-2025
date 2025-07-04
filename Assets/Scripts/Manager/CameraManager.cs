using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] CinemachineBrain brain;
    [SerializeField] CinemachineCamera followCamera;
    [SerializeField] private List<CinemachineCamera> goblinCamera;

    CinemachineFollow transposer;
    float cameraZoomBuffer;
    private void Awake()
    {
        cameraZoomBuffer = followCamera.Lens.OrthographicSize;
        followCamera.Lens.OrthographicSize = cameraZoomBuffer * (0.45f);
        transposer = followCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineFollow;
        transposer.FollowOffset = new Vector3(1.5f, 1, -10);
        Debug.Log(transposer.FollowOffset);
    }
    private void OnDestroy()
    {
        followCamera.Lens.OrthographicSize = cameraZoomBuffer;
    }
    public IEnumerator DisplayObjective()
    {
        followCamera.gameObject.SetActive(false);
        CinemachineCamera currentCamera = null;
        float transitionDuration = 0;
        for (int i = 0; i < goblinCamera.Count; i++)
        {
            if(currentCamera != null) currentCamera.gameObject.SetActive(false);
            currentCamera = goblinCamera[i];
            currentCamera.gameObject.SetActive(true);
            yield return null;
            transitionDuration = brain.ActiveBlend.Duration;

            yield return new WaitForSeconds(transitionDuration + 1f);
        }
        currentCamera.gameObject.SetActive(false);
        followCamera.gameObject.SetActive(true);
        var confiner = followCamera.GetComponent<CinemachineConfiner2D>();
        if (confiner != null)
        {
            confiner.InvalidateBoundingShapeCache(); // Forces it to recompute bounds
        }
        followCamera.Lens.OrthographicSize = cameraZoomBuffer;
        transposer.FollowOffset = new Vector3(0, 0, -10);
        yield return null;
        transitionDuration = brain.ActiveBlend.Duration;
        yield return new WaitForSeconds(transitionDuration + 1f);
    }
    public IEnumerator ZoomInPlayer(float duration = 3f)
    {
        float zoomValue = followCamera.Lens.OrthographicSize;
        yield return DOTween.To(() => zoomValue, x => zoomValue = x, 2f, duration).OnUpdate(() =>
        {
            followCamera.Lens.OrthographicSize = zoomValue;
        });
    }
}
