using GameObjectList = System.Collections.Generic.List<UnityEngine.GameObject>;
using UnityEngine;

public class BoidLogic : MonoBehaviour
{
    // Values
    [SerializeField] float speed;

    private Vector3 targetPosition = new(0.0f, 0.0f, 0.0f);
    private const float targettingStrength = 1.0f;

    private const float alignmentRange = 9.0f;
    private const float alignmentStrength = 0.2f;

    private const float cohesionRange = 5.0f;
    private const float cohesionStrength = 0.5f;

    private const float seperationRange = 2.0f;
    private const float seperationStrength = 1.0f;

    // Components
    private new Transform transform;
    private new Rigidbody rigidbody;

    // References
    [HideInInspector] public GameObjectList boidList;

    private void Awake()
    {
        transform = GetComponent<Transform>();
        rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Reset Velocity
        rigidbody.velocity = speed * Time.deltaTime * transform.forward;

        // Targetting
        rigidbody.velocity = speed * Time.deltaTime * (rigidbody.velocity + targettingStrength * (targetPosition - transform.position).normalized).normalized;

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

    private bool BoidInRange(GameObject boid, float range)
    {
        return (boid.transform.position - transform.position).magnitude < range;
    }

    private void Align()
    {
        Vector3 alignmentInfluence = new();

        foreach (GameObject boid in boidList)
        {
            if (boid == gameObject) continue;

            if (BoidInRange(boid, alignmentRange))
            {
                alignmentInfluence += boid.GetComponent<Rigidbody>().velocity * alignmentStrength;
            }
        }

        rigidbody.velocity = speed * Time.deltaTime * (rigidbody.velocity + alignmentInfluence).normalized;
    }

    private void Cohere()
    {
        Vector3 cohesionInfluence = new();

        foreach (GameObject boid in boidList)
        {
            if (boid == gameObject) continue;

            if (BoidInRange(boid, cohesionRange))
            {
                cohesionInfluence += (boid.transform.position - gameObject.transform.position).normalized * cohesionStrength;
            }
        }

        rigidbody.velocity = speed * Time.deltaTime * (rigidbody.velocity + cohesionInfluence).normalized;
    }

    private void Seperate()
    {
        Vector3 seperationInfluence = new();

        foreach (GameObject boid in boidList)
        {
            if (boid == gameObject) continue;

            if (BoidInRange(boid, seperationRange))
            {
                seperationInfluence -= (boid.transform.position - gameObject.transform.position).normalized * seperationStrength;
            }
        }

        rigidbody.velocity = speed * Time.deltaTime * (rigidbody.velocity + seperationInfluence).normalized;
    }
}