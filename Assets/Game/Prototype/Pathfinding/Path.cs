using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Path : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] bool showPath;
    [SerializeField] bool showLevers;
    [SerializeField] bool showPoints;

    private Transform[] splineTransforms;
    private List<Vector3> points = new();

    private void Awake()
    {
        Transform[] transformsToAdd = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            transformsToAdd[i] = transform.GetChild(i);
        }
        splineTransforms = transformsToAdd;

        GeneratePoints();
    }

    private void Update()
    {
        Transform[] transformsToAdd = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            transformsToAdd[i] = transform.GetChild(i);
        }
        splineTransforms = transformsToAdd;

        GeneratePoints();

        if (showPath)
        {
            ShowPath();
        }
    }

    private void OnValidate()
    {

    }

    public List<Vector3> GetPoints()
    {
        return points;
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

    private int GetSplineTransformIndex(int splineNumber, int positionNumber) //Should be 0 to 3
    {
        if (positionNumber < 0 || positionNumber > 3)
        {
            Debug.LogError("The index given is not valid. Position number should be between 0 and 3. Defaulted to 0.", this);
            return 0;
        }

        return splineNumber * 3 + positionNumber;
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

    private void ShowPath()
    {
        #if UNITY_EDITOR
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


            foreach (Transform splineTransform in splineTransforms)
            {
                splineTransform.gameObject.GetComponent<MeshRenderer>().enabled = showPoints;
            }
        }
        #endif
    }
}