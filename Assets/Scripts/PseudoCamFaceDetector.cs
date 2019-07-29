using System.Collections;
using System.Collections.Generic;
using DlibFaceLandmarkDetector;
using UnityEngine;
using UnityEngine.UI;

public class PseudoCamFaceDetector : MonoBehaviour
{
    [SerializeField] LibraryName library = LibraryName.Dlib_6;
    [SerializeField] StreamManager streamManager;
    [SerializeField] RawImage overlayImage;
    [SerializeField] AspectRatioFitter overlayFitter;
    [SerializeField] Sprite glassSprite;


    public float offset;

    const int MAX_FACES = 1;

    enum LibraryName
    {
        Dlib_6, Dlib_17, Dlib_68
    }

    readonly Dictionary<LibraryName, string> LibraryMap = new Dictionary<LibraryName, string> {
    { LibraryName.Dlib_6, "sp_human_face_6.dat" },
    { LibraryName.Dlib_17, "sp_human_face_17.dat" },
    { LibraryName.Dlib_68, "sp_human_face_68.dat" },
    };

    private List<Image> spexMap;
    private FaceLandmarkDetector faceLandmarkDetector;
    private bool hasInit, reArrange;
    private bool fillVertically;
    private Texture2D texture;
    private Transform spawnParent;

    int textureHeight, textureWidth;
    List<Rect> detectResult;
    List<Vector2> points;
    Vector2 leftPoint, rightPoint;
    float widthRatio, heightRatio, screenRatio;
    Color32[] colors;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => streamManager.CamStreamLoaded);
        SetupFaceDetection();
        SetupGlasses();
    }

    private void Update()
    {
        if (!hasInit || !streamManager.WebCam.didUpdateThisFrame) return;

        //colors = streamManager.WebCam.GetPixels32();

        //faceLandmarkDetector.SetImage(colors, textureWidth, textureHeight, 4, true);

        faceLandmarkDetector.SetImage(streamManager.WebCam);
        detectResult = faceLandmarkDetector.Detect();

        reArrange = true;

        //foreach (var rect in detectResult)
        //{
        //    faceLandmarkDetector.DetectLandmark(rect);

        //    faceLandmarkDetector.DrawDetectLandmarkResult(colors, textureWidth, textureHeight, 4, true, 0, 255, 0, 255);
        //}

        //faceLandmarkDetector.DrawDetectResult(colors, textureWidth, textureHeight, 4, true, 255, 0, 0, 255, 2);

        //texture.SetPixels32(colors);
        //texture.Apply(false);

    }

    private void LateUpdate()
    {
        if (!reArrange) return;

        ReDetectFaces();
    }

    void SetupFaceDetection()
    {
        spexMap = new List<Image>();
        faceLandmarkDetector = new FaceLandmarkDetector(System.IO.Path.Combine(Application.streamingAssetsPath, LibraryMap[library]));
        faceLandmarkDetector.SetImage(streamManager.WebCam);
        textureWidth = streamManager.WebCam.width;
        textureHeight = streamManager.WebCam.height;
        Debug.Log($"Width: {textureWidth} Height: {textureHeight}");
        texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        overlayImage.texture = texture;
        overlayFitter.aspectMode = streamManager.fitter.aspectMode;
        overlayFitter.aspectRatio = streamManager.fitter.aspectRatio;
        widthRatio = Screen.width / (float)textureWidth;
        heightRatio = Screen.height / (float)textureHeight;
        Debug.Log($"Screen Ratio: {screenRatio = Screen.width / (float)Screen.height}");
        Debug.Log($"Width Ratio: {widthRatio} Height Ratio: {heightRatio}");
        fillVertically = streamManager.WebCamRatio > screenRatio;
        hasInit = true;
    }

    void SetupGlasses()
    {
        spawnParent = new GameObject("Spawn Parent").transform;
        spawnParent.SetParent(overlayImage.transform, false);

        for (int i = 0; i < MAX_FACES; i++)
        {
            var spex = new GameObject("Glass " + i).AddComponent<Image>();
            spex.preserveAspect = true;
            spex.transform.SetParent(spawnParent, false);
            spex.transform.localScale = new Vector3(widthRatio, widthRatio, 1);
            spex.sprite = glassSprite;
            spexMap.Add(spex);
        }
    }

    void SetupHair()
    {
        for (int i = 0; i < MAX_FACES; i++)
        {
            var wig = new GameObject("Wig " + i).AddComponent<Image>();
            wig.preserveAspect = true;
            wig.transform.SetParent(spawnParent, false);
            wig.transform.localScale = new Vector3(widthRatio, widthRatio, 1);
            wig.sprite = glassSprite;
            spexMap.Add(wig);
        }
    }

    void ReDetectFaces()
    {
        for (int i = 0; i < Mathf.Min(detectResult.Count, MAX_FACES); i++)
        {
            points = faceLandmarkDetector.DetectLandmark(detectResult[i]);

            switch (library)
            {
                case LibraryName.Dlib_6:
                    leftPoint = points[2];
                    rightPoint = points[5];
                    break;
                case LibraryName.Dlib_17:
                    leftPoint = points[2];
                    rightPoint = points[5];
                    break;
                case LibraryName.Dlib_68:
                    leftPoint = points[36];
                    rightPoint = points[45];
                    break;
            }

            spexMap[i].gameObject.SetActive(true);
            ReArrange(ref i, ref i, leftPoint, rightPoint);
        }
        for (int i = Mathf.Min(detectResult.Count, MAX_FACES); i < MAX_FACES; i++)
            spexMap[i].gameObject.SetActive(false);

        reArrange = false;
    }

    void ReArrange(ref int spexIndex, ref int wigIndex, Vector2 leftEye, Vector2 rightEye)
    {
        var size = Vector2.Distance(leftEye, rightEye);
        //Debug.Log($"Re Arranged: {leftEye} {rightEye}");

        spexMap[spexIndex].rectTransform.sizeDelta = new Vector2(size * (fillVertically ? 1.85f : 1.65f), size);

        //Debug.Log((Screen.height - (textureHeight * widthRatio)) / 2);
        //Debug.Log(((heightRatio)));
        var newPoint = (leftEye + rightEye) / 2;
        Vector2 spexPos;
        if (fillVertically)
            spexPos = new Vector3((newPoint.x * heightRatio) - (textureWidth * heightRatio / 2), (textureHeight * heightRatio / 2) - (newPoint.y * heightRatio) /*(Screen.width / streamManager.WebCamRatio) * ((newPoint.y * widthRatio)) /* (Screen.height / 2 - ((newPoint.y - widthRatio) * widthRatio)) /*- ((Screen.height / 2) + (widthRatio * textureHeight))*/);
        else
            spexPos = new Vector3((newPoint.x * widthRatio) - (Screen.width / 2), ((textureHeight - newPoint.y) * widthRatio) - (textureHeight * widthRatio / 2)/*(Screen.width / streamManager.WebCamRatio) * ((newPoint.y * widthRatio)) /* (Screen.height / 2 - ((newPoint.y - widthRatio) * widthRatio)) /*- ((Screen.height / 2) + (widthRatio * textureHeight))*/);

        spexMap[spexIndex].rectTransform.localPosition = spexPos;
        spexMap[spexIndex].rectTransform.localRotation = Quaternion.Euler(0, 0, -180 + Vector2.SignedAngle(leftEye - rightEye, Vector2.right));
    }
}
