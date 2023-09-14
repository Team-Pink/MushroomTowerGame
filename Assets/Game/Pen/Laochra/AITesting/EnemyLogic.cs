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
    [System.Serializable]
    public enum EnemyState
    {
        None,
        Approach,
        Hunt,
        Attack
    } public EnemyState state = EnemyState.Approach;

    // Values
    [SerializeField] float speed;
    [SerializeField] float steeringForce;
    [SerializeField] float rotateSpeed;
    [SerializeField] int maxNeighbourhoodSize;
    [SerializeField] float attackRadius;
    private float AttackRadiusSqr { get => attackRadius * attackRadius; }

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
    [HideInInspector] public Transform hubTransform;

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
        switch (state)
        {
            case EnemyState.Approach:
                ApproachState();
                break;
            case EnemyState.Hunt:
                HuntState();
                break;
            case EnemyState.Attack:
                AttackState();
                break;
        }
    }

    protected virtual void ApproachState()
    {
        if ((hubTransform.position - transform.position).sqrMagnitude < AttackRadiusSqr)
        {
            rigidbody.velocity = Vector2.zero;
            state = EnemyState.Attack;
            neighbourhood.Clear();
            return;
        }

        // Get Boids in Neighbourhood
        PopulateNeighbourhood();

        // Flocking
        Vector3 newVelocity = Flock();

        // Targeting
        newVelocity += targetingStrength * levelData.GetFlowAtPoint(transform.position);

        // Apply New Velocity
        rigidbody.velocity = speed * Vector3.MoveTowards(rigidbody.velocity.normalized, newVelocity.normalized, steeringForce);

        // Face Direction of Movement
        if (rigidbody.velocity != Vector3.zero)
        {
            transform.forward = rigidbody.velocity.normalized;
        }
    }
    protected virtual void HuntState()
    {

    }
    protected virtual void AttackState()
    {

    }

    private void PopulateNeighbourhood()
    {
        neighbourhood.Clear();
        var boidColliderList = Physics.OverlapSphere(transform.position, NeighbourhoodRange, boidLayers);

        for (int colliderIndex = 0; colliderIndex < boidColliderList.Length; colliderIndex++)
        {
            Collider boidCollider = boidColliderList[colliderIndex];
            if (boidCollider.gameObject != gameObject)
            {
                EnemyLogic boidLogic = boidCollider.GetComponent<EnemyLogic>();

                if (boidLogic.state != EnemyState.Approach) continue;

                Transform boidTransform = boidCollider.transform;
                Rigidbody boidRigidbody = boidCollider.GetComponent<Rigidbody>();

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
    }

    private Vector3 Flock()
    {
        Vector3 alignmentInfluence = Vector3.zero, cohesionInfluence = Vector3.zero, seperationInfluence = Vector3.zero;
        foreach (BoidReference boid in neighbourhood)
        {
            alignmentInfluence += Align(boid);
            cohesionInfluence += Cohere(boid);
            seperationInfluence += Seperate(boid);
        }
        Vector3 result = alignmentInfluence.normalized * alignmentStrength;
        result += cohesionInfluence.normalized * cohesionStrength;
        result += seperationInfluence.normalized * seperationStrength;

        return result;
    }

    private bool BoidInRange(Transform boidTransform, float range)
    {
        return (boidTransform.position - transform.position).sqrMagnitude < (range * range);
    }

    private Vector3 Align(BoidReference boid)
    {
        if (BoidInRange(boid.transform, alignmentRange))
            return boid.rigidbody.velocity;
        else
            return Vector3.zero;
    }

    private Vector3 Cohere(BoidReference boid)
    {
        if (BoidInRange(boid.transform, cohesionRange))
            return (boid.transform.position - gameObject.transform.position).normalized;
        else
            return Vector3.zero;
    }

    private Vector3 Seperate(BoidReference boid)
    {
        if (BoidInRange(boid.transform, seperationRange))
            return (boid.transform.position - gameObject.transform.position).normalized;
        else
            return Vector3.zero;
    }
}