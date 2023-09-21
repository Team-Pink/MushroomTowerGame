using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class TrapAttackObject : MonoBehaviour
{
    private bool active;

    // Data
    public TrapDetails details = new TrapDetails();

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
            if (details.startupTime > 0)
            {
                details.startupTime -= Time.deltaTime; return;
            }
            else
            {
                meshRenderer.enabled = true;
                particleSystem.SetActive(true);
                active = true; return;
            }
        }


        Collider[] hits = Physics.OverlapSphere(transform.position, transform.localScale.x * 0.5f, enemies);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag(groundedEnemy))
            {
                Enemy enemy = hit.GetComponent<Enemy>();

                enemy.TakeDamage(details.dps * Time.deltaTime);

                if (details.conditions.Length > 0)
                {
                    enemy.ApplyConditions(details.conditions);
                }
            }
        }
    }

}