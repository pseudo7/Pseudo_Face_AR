using System.Collections;
using System.Collections.Generic;
using DlibFaceLandmarkDetector;
using UnityEngine;
using UnityEngine.UI;

public class PseudoFaceDetector : MonoBehaviour
{
    [SerializeField] LibraryName library = LibraryName.Dlib_6;
    [SerializeField] Texture2D sourceImage;
    [SerializeField] RawImage targetImage;
    [SerializeField] Sprite glassSprite;

    readonly Dictionary<LibraryName, string> LibraryMap = new Dictionary<LibraryName, string> {
    { LibraryName.Dlib_6, "sp_human_face_6.dat" },
    { LibraryName.Dlib_17, "sp_human_face_17.dat" },
    { LibraryName.Dlib_68, "sp_human_face_68.dat" },
    };

    private FaceLandmarkDetector faceLandmarkDetector;
    private List<Image> spexMap;
    private Transform spawnParent;
    private float size;

    Vector2 leftPoint, rightPoint;
    Rect rect;
    List<Rect> detectResult;
    List<Vector2> points;

    enum LibraryName
    {
        Dlib_6, Dlib_17, Dlib_68
    }

    private void Start()
    {
        SetupFaceDetection();
        DetectFaces();
        Apply();
        //DetectAndApply();
    }

    //struct FaceData
    //{
    //    internal Vector2 leftEye;
    //    internal Vector2 rightEye;
    //    internal Image spex;

    //    internal void ReOrient(Vector2 leftEye, Vector2 rightEye, Image spex)
    //    {
    //        leftEye 
    //    }
    //}

    void SetupFaceDetection()
    {
        spexMap = new List<Image>();
        faceLandmarkDetector = new FaceLandmarkDetector(System.IO.Path.Combine(Application.streamingAssetsPath, LibraryMap[library]));
        faceLandmarkDetector.SetImage(sourceImage);
    }


    void ReOrient(ref Image spex, ref Vector2 leftEye, ref Vector2 rightEye)
    {
        size = Vector2.Distance(leftEye, rightEye);

        spex.rectTransform.sizeDelta = new Vector2(size * 1.65f, size);

        var newPoint = (leftEye + rightEye) / 2;
        spex.rectTransform.localPosition = new Vector3(newPoint.x, Screen.height - newPoint.y);
        spex.rectTransform.localRotation = Quaternion.Euler(0, 0, -180 + Vector2.SignedAngle(leftEye - rightEye, Vector2.right));
    }

    void DetectFaces()
    {
        detectResult = faceLandmarkDetector.Detect();

        spawnParent = new GameObject("Spawn Parent").transform;
        spawnParent.SetParent(targetImage.transform, false);

        for (int i = 0; i < detectResult.Count; i++)
        {
            Debug.Log("face : " + (rect = detectResult[i]));

            points = faceLandmarkDetector.DetectLandmark(rect);
            Debug.Log("face points count : " + points.Count);
            //for (int j = 0; j < points.Count; j++)
            {
                //Debug.Log($"Face Point {j} : {points[j]}");

                //if (j == 2 || j == 36)
                {
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

                    var spex = new GameObject("Glass " + i).AddComponent<Image>();
                    spex.preserveAspect = true;
                    spex.transform.SetParent(spawnParent, false);
                    spex.sprite = glassSprite;
                    spexMap.Add(spex);

                    ReOrient(ref spex, ref leftPoint, ref rightPoint);
                }
            }
        }
    }

    void ReDetectFaces()
    {
        detectResult = faceLandmarkDetector.Detect();

        for (int i = 0; i < detectResult.Count; i++)
        {
            points = faceLandmarkDetector.DetectLandmark(rect);

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
            var spex = spexMap[i];
            ReOrient(ref spex, ref leftPoint, ref rightPoint);
        }
    }

    void Apply()
    {
        //faceLandmarkDetector.Dispose();

        targetImage.texture = sourceImage;
        targetImage.SetNativeSize();

        spawnParent.localPosition = new Vector3(sourceImage.width / -2, (sourceImage.height / 2) - Screen.height);
    }

    //private void DetectAndApply()
    //{

    //    //GameObject point;
    //    for (int i = 0; i < detectResult.Count; i++)
    //    {
    //        Debug.Log("face : " + (rect = detectResult[i]));

    //        List<Vector2> points = faceLandmarkDetector.DetectLandmark(rect);
    //        Debug.Log("face points count : " + points.Count);
    //        for (int j = 0; j < points.Count; j++)
    //        {
    //            Debug.Log($"Face Point {j} : {points[j]}");

    //            if (j == 2 || j == 36)
    //            {
    //                switch (library)
    //                {
    //                    case LibraryName.Dlib_6:
    //                        leftPoint = points[2];
    //                        rightPoint = points[5];
    //                        break;
    //                    case LibraryName.Dlib_17:
    //                        leftPoint = points[2];
    //                        rightPoint = points[5];
    //                        break;
    //                    case LibraryName.Dlib_68:
    //                        leftPoint = points[36];
    //                        rightPoint = points[45];
    //                        break;
    //                }

    //                //point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //                //point.name = $"Rect: {i} Point: {j}";
    //                //point.transform.SetParent(spawnParent);
    //                //point.transform.localPosition = new Vector3(leftPoint.x, Screen.height - leftPoint.y);
    //                //point.transform.localScale = Vector3.one * 5;

    //                //point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //                //point.name = $"Rect: {i} Point: {j}";
    //                //point.transform.SetParent(spawnParent);
    //                //point.transform.localPosition = new Vector3(rightPoint.x, Screen.height - rightPoint.y);
    //                //point.transform.localScale = Vector3.one * 5;

    //                spex = new GameObject("Glass " + i).AddComponent<Image>();

    //                spex.preserveAspect = true;
    //                spex.transform.SetParent(spawnParent, false);
    //                spex.sprite = glassSprite;
    //                spexMap.Add(rect.GetHashCode(), spex);


    //                ReOrient(rect.GetHashCode(), leftPoint, rightPoint);

    //                //glasses.rectTransform.localScale = Vector3.one;

    //                //glasses.rectTransform.sizeDelta = new Vector2(size * 1.65f, size);
    //                //newPoint = (leftPoint + rightPoint) / 2;
    //                //glasses.rectTransform.localPosition = new Vector3(newPoint.x, Screen.height - newPoint.y);
    //                //glasses.rectTransform.localEulerAngles = new Vector3(0, 0, -180 + Vector2.SignedAngle(leftPoint - rightPoint, Vector2.right));
    //            }

    //            //point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //            //point.name = $"Rect: {i} Point: {j}";
    //            //point.transform.SetParent(spawnParent);
    //            //point.transform.localPosition = new Vector3(points[j].x, Screen.height - points[j].y);
    //            //point.transform.localScale = Vector3.one * 5;
    //        }

    //        //faceLandmarkDetector.DrawDetectLandmarkResult(dstTexture2D, 0, 255, 0, 255);
    //    }

    //    //faceLandmarkDetector.DrawDetectResult(dstTexture2D, 255, 0, 0, 255, 2);

    //    faceLandmarkDetector.Dispose();

    //    targetImage.texture = sourceImage;// dstTexture2D;
    //    targetImage.SetNativeSize();

    //    spawnParent.localPosition = new Vector3(sourceImage.width / -2, (sourceImage.height / 2) - Screen.height);
    //}
}
