using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class BezierMoveToPoint : MonoBehaviour
{
    float progress;
    [SerializeField] Transform[] points;

    private Vector3 point1;
    private Vector3 lever1;
    private Vector3 lever2;
    private Vector3 point2;

    private int currentSpline = 0;

    [Header("Debug")]
    [SerializeField] bool showPath;
    [SerializeField] bool showLevers;

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

        DEBUG();
    }

    private void DEBUG()
    {
        if (showPath)
        {
            int amountOfSplines = (points.Length - 1) / 3;

            Vector3 previousPoint = points[GetPointIndex(0, 0)].position;
            for (int splineNumber = 0; splineNumber < amountOfSplines; splineNumber++)
            {
                float T = 0;

                Vector3 P1 = points[GetPointIndex(splineNumber, 0)].position;
                Vector3 L1 = points[GetPointIndex(splineNumber, 1)].position;
                Vector3 L2 = points[GetPointIndex(splineNumber, 2)].position;
                Vector3 P2 = points[GetPointIndex(splineNumber, 3)].position;

                while (T <= 1)
                {
                    T += 0.05f;

                    Vector3 currentPoint = Evaluate(T, P1, L1, L2, P2);

                    Debug.DrawLine(previousPoint, currentPoint, Color.blue);

                    previousPoint = currentPoint;
                };

                if (showLevers)
                {
                    Debug.DrawLine(P1, L1, Color.magenta);
                    Debug.DrawLine(L2, P2, Color.magenta);
                }
            }
        }
    }

    private void Playing()
    {
        if (progress < 1)
            progress += Time.deltaTime;

        gameObject.transform.position = Evaluate(progress, point1, lever1, lever2, point2);

        if (progress > 1)
        {
            if (GetPointIndex(currentSpline+1, 3) + 1 <= points.Length)
            {
                currentSpline++;
                Initialise();
            }
        }
    }

    private Vector3 Evaluate(float T, Vector3 P1, Vector3 L1, Vector3 L2, Vector3 P2)
    {
        Vector3 P1L1 = Vector3.Lerp(P1, L1, T);
        Vector3 L1L2 = Vector3.Lerp(L1, L2, T);
        Vector3 L2P1 = Vector3.Lerp(L2, P2, T);

        Vector3 P1L1L2 = Vector3.Lerp(P1L1, L1L2, T);
        Vector3 L1L2P2 = Vector3.Lerp(L1L2, L2P1, T);

        return Vector3.Lerp(P1L1L2, L1L2P2, T);
    }


    private void Initialise()
    {
        progress = 0;

        point1 = points[GetPointIndex(currentSpline, 0)].position;
        lever1 = points[GetPointIndex(currentSpline, 1)].position;
        lever2 = points[GetPointIndex(currentSpline, 2)].position;
        point2 = points[GetPointIndex(currentSpline, 3)].position;
    }
}