using Vector3List = System.Collections.Generic.List<UnityEngine.Vector3>;
using UnityEngine;
using Text = TMPro.TMP_Text;

public class Enemy : MonoBehaviour
{
    [Header("Health")]
    public int health;
    public bool Dead()
    {
        return health <= 0;
    }

    [Header("Movement")]
    [SerializeField, Range(0.1f, 1.0f)] float speed = 0.5f;
    [SerializeField] Path pathToFollow;

    private float progress = 0.0f;

    [Header("Attacking")]
    [SerializeField] float attackCooldown;
    private float elapsedCooldown;
    [HideInInspector] public bool attackMode { get; private set; }
    private bool attackInProgress;

    [Header("Components")]
    [SerializeField] Hub hub;
    public LayerMask range;

    [Header("Debug")]
    [SerializeField] bool showPath;
    [SerializeField] bool showLevers;

    [Header("Provides On Death")]
    [SerializeField] int bugBits = 2;

    [Space()]
    [SerializeField] Text healthText;

    private int currentPoint;
    private Vector3List points = new();

    private void Awake()
    {
        points = pathToFollow.GetPoints();
    }

    private void Update()
    {
        Playing();
    }

    private void Playing()
    {
        healthText.text = health.ToString();

        if (attackMode)
        {
            if (!attackInProgress)
            {
                hub.Damage(1);
                attackInProgress = true;
            }
            else
            {
                elapsedCooldown += Time.deltaTime;

                if (elapsedCooldown >= attackCooldown)
                {
                    attackInProgress = false;
                    elapsedCooldown = 0;
                }
            }
           
            return;
        }

        if (progress < 1)
            progress += Time.deltaTime * speed;

        gameObject.transform.position = Vector3.Lerp(points[currentPoint], points[currentPoint + 1], progress);

        if (progress >= 1)
        {
            if (currentPoint + 1 < points.Count)
            {
                progress = 0;
                currentPoint++;
            }
            else
                attackMode = true;
        }
        else
            attackMode = false;
    }

    public void OnDeath()
    {
        // do a sphere cast for towers in range and remove this enemy from them
        Collider[] towers = Physics.OverlapSphere(transform.position, 0.2f, range);
        foreach (Collider tower in towers)
        {
            if (tower.gameObject.GetComponent<TurretController>().inRangeEnemies.Contains(gameObject))
            {
                tower.gameObject.GetComponent<TurretController>().inRangeEnemies.Remove(gameObject); //The Remove method returns false if item is not found in the Hashset.
            }
        }

        CurrencyManager currencyManager = GameObject.Find("GameManager").GetComponentInChildren<CurrencyManager>();
        currencyManager.IncreaseCurrencyAmount(bugBits);

        Debug.Log(gameObject.name + " is dead");
        
        for(int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void SpawnIn()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }

        //whatever else needs to be done before fully spawning in do within here

    }
}