using Vector3List = System.Collections.Generic.List<UnityEngine.Vector3>;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteInEditMode]
public class SplinePath : MonoBehaviour
{
    float progress;
    [SerializeField] Transform[] splineTransforms;
    private int currentSpline = 0;

    private Vector3 position1;
    private Vector3 lever1;
    private Vector3 lever2;
    private Vector3 position2;

    private Vector3List points = new();

    [Header("Debug")]
    [SerializeField] bool showPath;
    [SerializeField] bool showLevers;

    private void Awake()
    {
        Initialise();
        GeneratePoints();
    }

    private int GetSplineTransformIndex(int splineNumber, int positionNumber) //Should be 0 to 3
    {
        if (positionNumber < 0 || positionNumber > 3)
        {
            Debug.LogError("The index given is not valid. Position number should be between 0 and 3. Defaulted to 0.", this);
            return 0;
        }

        return splineNumber * 3 + positionNumber;
    }

    private void Update()
    {
        if (Application.isPlaying)
            Playing();
#if UNITY_EDITOR
        else if (EditorApplication.isPlaying)
            Playing();
#endif
        DEBUG();
    }

    private void GeneratePoints()
    {
        points.Clear();

        int amountOfSplines = (splineTransforms.Length - 1) / 3;

        for (int splineNumber = 0; splineNumber < amountOfSplines; splineNumber++)
        {
            float T = 0;

            Vector3 P1 = splineTransforms[GetSplineTransformIndex(splineNumber, 0)].position;
            Vector3 L1 = splineTransforms[GetSplineTransformIndex(splineNumber, 1)].position;
            Vector3 L2 = splineTransforms[GetSplineTransformIndex(splineNumber, 2)].position;
            Vector3 P2 = splineTransforms[GetSplineTransformIndex(splineNumber, 3)].position;

            while (T <= 1)
            {
                T += 0.05f;

                Vector3 currentPoint = Evaluate(T, P1, L1, L2, P2);

                points.Add(currentPoint);
            };
        }
    }

    private void DEBUG()
    {
        if (showPath)
        {
            int amountOfSplines = (splineTransforms.Length - 1) / 3;

            for (int splineNumber = 0; splineNumber < amountOfSplines; splineNumber++)
            {
                Vector3 P1 = splineTransforms[GetSplineTransformIndex(splineNumber, 0)].position;
                Vector3 L1 = splineTransforms[GetSplineTransformIndex(splineNumber, 1)].position;
                Vector3 L2 = splineTransforms[GetSplineTransformIndex(splineNumber, 2)].position;
                Vector3 P2 = splineTransforms[GetSplineTransformIndex(splineNumber, 3)].position;

                if (showLevers)
                {
                    Debug.DrawLine(P1, L1, Color.magenta);
                    Debug.DrawLine(L2, P2, Color.magenta);
                }
            }

            for (int i = 0; i < points.Count; i++)
            {
                if (i == 0)
                    Debug.DrawLine(splineTransforms[0].position, points[i], Color.blue);
                else
                    Debug.DrawLine(points[i - 1], points[i], Color.blue);
            }
        }
    }

    private void Playing()
    {
        if (progress < 1)
            progress += Time.deltaTime;

        gameObject.transform.position = Evaluate(progress, position1, lever1, lever2, position2);

        if (progress > 1)
        {
            if (GetSplineTransformIndex(currentSpline+1, 3) + 1 <= splineTransforms.Length)
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

        position1 = splineTransforms[GetSplineTransformIndex(currentSpline, 0)].position;
        lever1 = splineTransforms[GetSplineTransformIndex(currentSpline, 1)].position;
        lever2 = splineTransforms[GetSplineTransformIndex(currentSpline, 2)].position;
        position2 = splineTransforms[GetSplineTransformIndex(currentSpline, 3)].position;
    }
}