using BoidList = System.Collections.Generic.List<BoidReference>;
using UnityEngine;

public struct BoidReference
{
    public GameObject gameObject;
    public Transform transform;
    public Rigidbody rigidbody;
    public EnemyLogic logic;

    public BoidReference(GameObject GameObject, Transform Transform, Rigidbody Rigidbody, EnemyLogic Logic)
    {
        gameObject = GameObject;
        transform = Transform;
        rigidbody = Rigidbody;
        logic = Logic;
    }
}


public class EnemyLogic : MonoBehaviour
{
    // Values
    [SerializeField] float speed;

    private Vector3 targetPosition = new(0.0f, 0.0f, 0.0f);
    [Header("Influences"), SerializeField, Range(0.0f, 1.0f)] private float targettingStrength = 0.2f;
    [Space()]
    [SerializeField] private float alignmentRange = 4.0f;
    [SerializeField, Range(0.0f, 1.0f)] private float alignmentStrength = 0.1f;
    [Space()]
    [SerializeField] private float cohesionRange = 3.0f;
    [SerializeField, Range(0.0f, 1.0f)] private float cohesionStrength = 0.2f;
    [Space()]
    [SerializeField] private float seperationRange = 2.0f;
    [SerializeField, Range(0.0f, 1.0f)] private float seperationStrength = 1.0f;

    // Components
    private new Transform transform;
    private new Rigidbody rigidbody;

    private LayerMask boidLayers;

    // References
    [HideInInspector] public BoidList neighbourhood = new();

    private void Awake()
    {
        transform = GetComponent<Transform>();
        rigidbody = GetComponent<Rigidbody>();

        boidLayers = LayerMask.GetMask("Boid");
    }

    private void Update()
    {
        // Reset Velocity
        rigidbody.velocity = speed * Time.deltaTime * transform.forward;

        // Targetting
        rigidbody.velocity = speed * Time.deltaTime * (rigidbody.velocity + targettingStrength * (targetPosition - transform.position).normalized).normalized;

        // Get Boids in Neighbourhood
        neighbourhood.Clear();
        var boidColliderList = Physics.OverlapSphere(transform.position, 4, boidLayers);

        foreach (var boidCollider in boidColliderList)
        {
            GameObject boidGameObject = boidCollider.gameObject;
            Transform boidTransform = boidCollider.transform;
            Rigidbody boidRigidbody = boidCollider.GetComponent<Rigidbody>();
            EnemyLogic boidLogic = boidCollider.GetComponent<EnemyLogic>();

            if (boidGameObject != gameObject)
                neighbourhood.Add(new(boidGameObject, boidTransform, boidRigidbody, boidLogic));
        }

        // Flocking
        Align();
        Cohere();
        Seperate();

        // Blocking
        if ((targetPosition - transform.position).magnitude > 100)
            rigidbody.velocity = speed * Time.deltaTime * (rigidbody.velocity + targettingStrength * 5 * (targetPosition - transform.position).normalized).normalized;

        // Face Direction of Movement
        if (rigidbody.velocity != Vector3.zero)
            transform.forward = rigidbody.velocity;
    }

    private bool BoidInRange(Transform boidTransform, float range)
    {
        return (boidTransform.position - transform.position).sqrMagnitude < (range * range);
    }

    private void Align()
    {
        Vector3 alignmentInfluence = Vector3.zero;

        foreach (BoidReference boid in neighbourhood)
        {
            if (BoidInRange(boid.transform, alignmentRange))
            {
                alignmentInfluence += boid.rigidbody.velocity * alignmentStrength;
            }
        }

        rigidbody.velocity = speed * Time.deltaTime * (rigidbody.velocity + alignmentInfluence).normalized;
    }

    private void Cohere()
    {
        Vector3 cohesionInfluence = Vector3.zero;

        foreach (BoidReference boid in neighbourhood)
        {
            if (BoidInRange(boid.transform, cohesionRange))
            {
                cohesionInfluence += (boid.transform.position - gameObject.transform.position).normalized * cohesionStrength;
            }
        }

        rigidbody.velocity = speed * Time.deltaTime * (rigidbody.velocity + cohesionInfluence).normalized;
    }

    private void Seperate()
    {
        Vector3 seperationInfluence = Vector3.zero;

        foreach (BoidReference boid in neighbourhood)
        {
            if (BoidInRange(boid.transform, seperationRange))
            {
                seperationInfluence -= (boid.transform.position - gameObject.transform.position).normalized * seperationStrength;
            }
        }

        rigidbody.velocity = speed * Time.deltaTime * (rigidbody.velocity + seperationInfluence).normalized;
    }
}