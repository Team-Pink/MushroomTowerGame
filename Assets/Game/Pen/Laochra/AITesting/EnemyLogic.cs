using BoidList = System.Collections.Generic.List<BoidReference>;
using UnityEngine;

public struct BoidReference
{
    public Transform transform;
    public Rigidbody rigidbody;
    public EnemyLogic logic;

    public BoidReference(Transform Transform, Rigidbody Rigidbody, EnemyLogic Logic)
    {
        transform = Transform;
        rigidbody = Rigidbody;
        logic = Logic;
    }
}


public class EnemyLogic : MonoBehaviour
{
    // Values
    [SerializeField] float speed;
    [SerializeField] float steeringForce;
    [SerializeField] float rotateSpeed;
    [SerializeField] int maxNeighbourhoodSize;

    [Header("Influences"), SerializeField, Range(0.0f, 5.0f)] private float targetingStrength = 0.2f;
    [Space()]
    [SerializeField] private float alignmentRange = 4.0f;
    [SerializeField, Range(0.0f, 5.0f)] private float alignmentStrength = 0.1f;
    [Space()]
    [SerializeField] private float cohesionRange = 3.0f;
    [SerializeField, Range(0.0f, 5.0f)] private float cohesionStrength = 0.2f;
    [Space()]
    [SerializeField] private float seperationRange = 2.0f;
    [SerializeField, Range(0.0f, 5.0f)] private float seperationStrength = 1.0f;

    private float NeighbourhoodRange
    {
        get => Mathf.Max(alignmentRange, cohesionStrength, seperationRange);
    }

    // Components
    private new Transform transform;
    private new Rigidbody rigidbody;
    [HideInInspector] public LevelDataGrid levelData;

    private LayerMask boidLayers;

    // References
    [HideInInspector] public BoidList neighbourhood = new();

    private void Awake()
    {
        transform = GetComponent<Transform>();
        rigidbody = GetComponent<Rigidbody>();

        boidLayers = LayerMask.GetMask("Enemy");
    }

    private void Update()
    {
        Vector3 newVelocity = Vector3.zero;

        // Targeting
        newVelocity += targetingStrength * levelData.GetFlowAtPoint(transform.position);

        // Get Boids in Neighbourhood
        neighbourhood.Clear();
        var boidColliderList = Physics.OverlapSphere(transform.position, NeighbourhoodRange, boidLayers);

        for (int colliderIndex = 0; colliderIndex < boidColliderList.Length; colliderIndex++)
        {
            Collider boidCollider = boidColliderList[colliderIndex];
            if (boidCollider.gameObject != gameObject)
            {
                Transform boidTransform = boidCollider.transform;

                Rigidbody boidRigidbody = boidCollider.GetComponent<Rigidbody>();
                EnemyLogic boidLogic = boidCollider.GetComponent<EnemyLogic>();

                neighbourhood.Add(new(boidTransform, boidRigidbody, boidLogic));

                if (neighbourhood.Count < maxNeighbourhoodSize) continue;

                int furthestIndex = 0;
                float furthestSqrMag = (neighbourhood[furthestIndex].transform.position - transform.position).sqrMagnitude;
                for (int currentIndex = 1; currentIndex < neighbourhood.Count; currentIndex++)
                {
                    float currentSqrMag = (neighbourhood[currentIndex].transform.position - transform.position).sqrMagnitude;
                    if (currentSqrMag > furthestSqrMag) furthestIndex = currentIndex;
                }

                neighbourhood.RemoveAt(furthestIndex);
            }
        }

        // Flocking
        newVelocity += Align();
        newVelocity += Cohere();
        newVelocity += Seperate();

        rigidbody.velocity = speed * Vector3.MoveTowards(rigidbody.velocity.normalized, newVelocity.normalized, steeringForce);

        // Face Direction of Movement
        if (rigidbody.velocity != Vector3.zero)
        {
            transform.forward = rigidbody.velocity.normalized;
        }
    }

    private bool BoidInRange(Transform boidTransform, float range)
    {
        return (boidTransform.position - transform.position).sqrMagnitude < (range * range);
    }

    private Vector3 Align()
    {
        Vector3 alignmentInfluence = Vector3.zero;

        foreach (BoidReference boid in neighbourhood)
        {
            if (BoidInRange(boid.transform, alignmentRange))
            {
                alignmentInfluence += boid.rigidbody.velocity;
            }
        }

        return alignmentInfluence.normalized * alignmentStrength;
    }

    private Vector3 Cohere()
    {
        Vector3 cohesionInfluence = Vector3.zero;

        foreach (BoidReference boid in neighbourhood)
        {
            if (BoidInRange(boid.transform, cohesionRange))
            {
                cohesionInfluence += (boid.transform.position - gameObject.transform.position).normalized;
            }
        }

        return cohesionInfluence.normalized * cohesionStrength;
    }

    private Vector3 Seperate()
    {
        Vector3 seperationInfluence = Vector3.zero;

        foreach (BoidReference boid in neighbourhood)
        {
            if (BoidInRange(boid.transform, seperationRange))
            {
                seperationInfluence -= (boid.transform.position - gameObject.transform.position).normalized;
            }
        }

        return seperationInfluence.normalized * seperationStrength;
    }
}