using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage), typeof(AspectRatioFitter))]
public class StreamManager : MonoBehaviour
{
    internal WebCamTexture WebCam { get; private set; }
    internal float WebCamRatio { get; private set; } = 1;
    internal bool CamStreamLoaded;

    [Header("UI")]
    [SerializeField] CanvasScaler mainCanvas;
    [SerializeField] AspectRatioFitter.AspectMode aspectMode = AspectRatioFitter.AspectMode.None;
    [SerializeField] internal RawImage background;
    [SerializeField] internal AspectRatioFitter fitter;

    [Header("Camera")]
    [Range(1, 100)]
    [SerializeField] int camQuality = 20;
    [SerializeField] bool useFrontCamera;

    private void Awake()
    {
        mainCanvas.referenceResolution = new Vector2(Screen.width, Screen.height);
    }

    IEnumerator Start()
    {
        WebCamDevice[] camDevices = WebCamTexture.devices;

        if (camDevices.Length == 0)
        {
            Debug.LogWarning("No Cameras Available");
            yield break;
        }

        SetUpWebCam();

        if (!WebCam)
        {
            useFrontCamera = !useFrontCamera;
            SetUpWebCam();
        }

        if (!WebCam)
        {
            Debug.LogWarning("No Front Camera");
            yield break;
        }

        WebCam.Play();

        yield return new WaitUntil(() => WebCam.didUpdateThisFrame);
        Debug.LogWarning("Web Cam Loaded");

        fitter.aspectMode = aspectMode;
        WebCamRatio = WebCam.width / (float)WebCam.height;
        background.texture = WebCam;
        Debug.LogWarning("Web Cam Ratio: " + WebCamRatio);

        CamStreamLoaded = true;
    }

    void SetUpWebCam()
    {
        int width = (int)(Screen.width / 100f * camQuality);
        int height = (int)(Screen.height / 100f * camQuality);

        Debug.Log($"Calc Width: {width} Calc Height: {height}");

        foreach (var cam in WebCamTexture.devices)
            if (useFrontCamera)
            {
                if (cam.isFrontFacing)
                {
                    WebCam = new WebCamTexture(cam.name, width, height);
                    break;
                }
            }
            else
            {
                if (!cam.isFrontFacing)
                {
                    WebCam = new WebCamTexture(cam.name, width, height);
                    break;
                }
            }
    }

    void SetUpWebCam(int camWidth, int camHeight)
    {
        Debug.Log($"Cam Width: {camWidth} Cam Height: {camHeight}");

        if (WebCamTexture.devices.Length > 0)
            WebCam = new WebCamTexture(WebCamTexture.devices[0].name, camWidth, camHeight);
        else Debug.LogError("No Web Cam Devices");
    }

    private void Update()
    {
        fitter.aspectRatio = WebCamRatio;// WebCam.width / (float)WebCam.height;
        background.rectTransform.localScale = new Vector3(1, WebCam.videoVerticallyMirrored ? -1 : 1, 1);
        background.rectTransform.localEulerAngles = new Vector3(0, 0, -WebCam.videoRotationAngle);
    }
}