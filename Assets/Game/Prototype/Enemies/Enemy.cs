using Vector3List = System.Collections.Generic.List<UnityEngine.Vector3>;
using UnityEngine;
using Text = TMPro.TMP_Text;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{
    [Header("Health")]
    public int health;
    
    public bool Dead()
    {
        return health <= 0;
    }
    public bool dead = false; // this is specifically for the ondeath function. to replace the functionality of checking health in update and setting dead in Ondeath so it can only run once.

    [Header("Movement")]
    [SerializeField, Range(0.0f, 5.0f)] protected float speed = 2.0f;
    public Path pathToFollow;

    private float progress = 0.0f;

    [Header("Attacking")]
    [SerializeField] protected float attackCooldown;
    protected float elapsedCooldown;
    private bool attackMode;
    [HideInInspector] public bool AttackMode
    {
        get
        {
            return attackMode;
        }
        private set
        {
            animator.SetBool("Attacking", value);
            attackMode = value;
        }
    }
    protected bool attackInProgress;

    [Header("Components")]
    public Hub hub;
    public LayerMask range;
    [SerializeField] Animator animator;

    [Header("Debug")]
    [SerializeField] bool showPath;
    [SerializeField] bool showLevers;

    [Header("Provides On Death")]
    [SerializeField] int bugBits = 2;
    public int expValue = 1;

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

    protected void Playing()
    {
        if (dead)
            return;
        
        healthText.text = health.ToString();

        
        if (AttackMode)
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

        if (currentPoint + 1 < points.Count)
        {
            if (speed > 0) RotateToFaceTravelDirection();
            transform.position = Vector3.Lerp(points[currentPoint], points[currentPoint + 1], progress);
        }

        if (progress >= 1)
        {
            if (currentPoint + 1 < points.Count)
            {
                progress = 0;
                currentPoint++;
            }
            else
                AttackMode = true;
        }
        else
            AttackMode = false;
    }

    public void OnDeath()
    {
        if (dead) return; // don't increase currency twice.
        dead = true; // object pool flag;

        // death animation

        // increment currency
        CurrencyManager currencyManager = GameObject.Find("GameManager").GetComponentInChildren<CurrencyManager>();
        currencyManager.IncreaseCurrencyAmount(bugBits);

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        Debug.Log(gameObject.name + " is dead");
    }

    public void SpawnIn()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }

        dead = false;

        //whatever else needs to be done before fully spawning in do within here

    }

    void RotateToFaceTravelDirection()
    {
        Vector3 lookDirection = (points[currentPoint + 1] - points[currentPoint]).normalized;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), progress);
    }
}