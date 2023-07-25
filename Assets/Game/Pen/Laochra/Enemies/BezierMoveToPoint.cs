using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class BezierMoveToPoint : MonoBehaviour
{
    float progress;
    [SerializeField] Transform[] points;

    private Vector3 a;
    private Vector3 b;
    private Vector3 c;
    private Vector3 d;

    private int currentSpline = 0;


    private void Awake()
    {
        Initialise();
    }

    private int GetPointIndex(int splineNumber, int relativePointNumber) //Should be 0 to 3
    {
        return splineNumber * 3 + relativePointNumber;
    }



    private void Update()
    {
        if (Application.isPlaying || EditorApplication.isPlaying)
            Playing();

        // THIS NEEDS TO BE MODIFIED TO TAKE EACH POINT AND DRAW A LINE TO THE NEXT POINT
        /*int amountOfSplines = (points.Length - 1) / 3;
        for (int splineNumber = 0; splineNumber < amountOfSplines; splineNumber++)
        {
            for (int relativePointNumber = 0; relativePointNumber < 4; relativePointNumber++)
            {
                Vector3 pointA = points[GetPointIndex(splineNumber, relativePointNumber)].position;
                Vector3 pointB;

                if (relativePointNumber == 3)
                    pointB = points[GetPointIndex(splineNumber+1, 0)].position;
                else
                    pointB = points[GetPointIndex(splineNumber, relativePointNumber+1)].position;

                Debug.DrawLine(pointA, pointB, Color.magenta);
            }
        }*/
    }

    private void Playing()
    {
        if (progress < 1)
            progress += Time.deltaTime;

        gameObject.transform.position = Evaluate(progress);

        if (progress > 1)
        {
            if (GetPointIndex(currentSpline+1, 3) + 1 <= points.Length)
            {
                currentSpline++;
                Initialise();
            }
        }
    }

    private Vector3 Evaluate(float t)
    {
        Vector3 ab = Vector3.Lerp(a, b, t);
        Vector3 bc = Vector3.Lerp(b, c, t);
        Vector3 cd = Vector3.Lerp(c, d, t);

        Vector3 abc = Vector3.Lerp(ab, bc, t);
        Vector3 bcd = Vector3.Lerp(bc, cd, t);

        return Vector3.Lerp(abc, bcd, t);
    }


    private void Initialise()
    {
        progress = 0;

        a = points[GetPointIndex(currentSpline, 0)].position;
        b = points[GetPointIndex(currentSpline, 1)].position;
        c = points[GetPointIndex(currentSpline, 2)].position;
        d = points[GetPointIndex(currentSpline, 3)].position;
    }
}