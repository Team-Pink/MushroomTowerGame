using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class TrapAttackObject : MonoBehaviour
{
    private bool active;

    // Data
    public TrapDetails details = new TrapDetails();
    public float cleanupDuration;
    private float cleanupTime = 0.0f;

    // Components
    private new Transform transform;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] new GameObject particleSystem;

    // Detection
    private LayerMask enemies;
    private const string groundedEnemy = "Enemy";

    private void Awake()
    {
        // Components
        transform = gameObject.transform;
        meshRenderer.enabled = false;
        particleSystem.SetActive(false);

        // Detection
        enemies = LayerMask.GetMask("Enemy");
    }

    private void Update()
    {
        if (!active)
        {
            meshRenderer.enabled = true;
            particleSystem.SetActive(true);
            active = true; return;
        }

        if (cleanupTime < cleanupDuration)
        {
            cleanupTime += Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, transform.localScale.x * 0.5f, enemies);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag(groundedEnemy))
            {
                Enemy enemy = hit.GetComponent<Enemy>();

                if (details.conditions.Length > 0)
                {
                    enemy.ApplyConditions(details.conditions);
                }
            }
        }
    }

}