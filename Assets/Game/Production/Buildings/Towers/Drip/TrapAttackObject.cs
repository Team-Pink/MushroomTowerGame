using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class TrapAttackObject : MonoBehaviour
{
    public TrapDetails details = new TrapDetails();
    private new Transform transform;
    private LayerMask enemies;
    private const string groundedEnemy = "Enemy";

    private void Awake()
    {
        transform = gameObject.transform;
        enemies = LayerMask.GetMask("Enemy");

        // Start the delay here instead of in TrapAttacker so the ink has time to spray out.
        // (until animation is done just enable mesh and collider after delay)
    }

    private void Update()
    {
        if (details.startupTime > 0)
        {
            details.startupTime -= Time.deltaTime; return;
        }
        Collider[] hits = Physics.OverlapSphere(transform.position, transform.localScale.x * 0.5f, enemies);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag(groundedEnemy))
            {
                Enemy enemy = hit.GetComponent<Enemy>();

                enemy.StartCoroutine(enemy.TakeDamage(details.dps * Time.deltaTime));

                if (details.conditions.Length > 0)
                {
                    enemy.ApplyConditions(details.conditions);
                }
            }
        }
    }

}