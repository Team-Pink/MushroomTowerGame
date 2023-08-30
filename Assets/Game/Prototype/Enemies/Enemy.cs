using Vector3List = System.Collections.Generic.List<UnityEngine.Vector3>;
using UnityEngine;
using Text = TMPro.TMP_Text;

public class Enemy : MonoBehaviour
{
    protected virtual void CustomAwakeEvents()
    {

    }

    private void Awake()
    {
        points = pathToFollow.GetPoints();

        CustomAwakeEvents();
    }

    private void Update()
    {
        
    }

    protected virtual void Playing()
    {
        if (isDead) return;

        healthText.text = health.ToString();

        if (AttackMode)
        {
            AttackHub();
            return;
        }

        Travel();
    }

    #region ALIVE STATUS
    [Header("Health")]
    [SerializeField] Text healthText;
    public int health;
    public bool isDead
    {
        get;
        protected set;
    }          
    // this is specifically for the ondeath function. to replace the functionality of checking
    // health in update and setting isDead in Ondeath so it can only run once.

    [Header("Provides On Death")]
    [SerializeField] int bugBits = 2;
    public int expValue = 1;

    public virtual void TakeDamage(int damage)
    {
        health -= damage;
        if(CheckIfDead()) OnDeath();
    }
    public void SpawnIn()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
        GetComponent<Rigidbody>().detectCollisions = true;
        isDead = false;

        //whatever else needs to be done before fully spawning in do within here

    }

    private bool CheckIfDead()
    {
        return health <= 0;
    }
    private void OnDeath()
    {
        if (isDead) return; // don't increase currency twice.
        isDead = true; // object pool flag;

        // death animation

        // increment currency
        CurrencyManager currencyManager = GameObject.Find("GameManager").GetComponentInChildren<CurrencyManager>();
        currencyManager.IncreaseCurrencyAmount(bugBits);

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        GetComponent<Rigidbody>().detectCollisions = false;
    }

    #endregion

    #region MOVEMENT
    [Header("Movement")]
    [SerializeField, Range(0.0f, 5.0f)]
    protected float speed = 2f;

    [SerializeField] protected LayerMask range;

    public float mass
    {
        get;
        protected set;
    }

    [HideInInspector]
    public float Speed
    {
        get
        {
            return speed;
        }
    }
    public Path pathToFollow;

    float progress = 0.0f;
    int currentPoint;
    Vector3List points = new();

    protected void Travel()
    {
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
    }

    private void RotateToFaceTravelDirection()
    {
        Vector3 lookDirection = (points[currentPoint + 1] - points[currentPoint]).normalized;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), progress);
    }
    #endregion

    #region Attacking
    [Header("Attacking")]
    [SerializeField] protected int damage;
    
    [SerializeField] protected float attackCooldown = 0;
    [SerializeField] protected float attackDelay = 0; //would be really nice if we could automatically set the attackDelay to the time of a specific keyframe in an animation clip.
    protected float elapsedCooldown = 0;
    protected float elapsedDelay = 0;

    protected bool AttackMode;
    protected bool attackInProgress = false;
    protected bool attackCoolingDown = false;

    //there are areas I can further optimise and clean up but that will be a later thing
    protected virtual void AttackHub()
    {
        if(elapsedCooldown == 0 && elapsedDelay == 0)
        {
            animator.SetBool("Attacking", true);
            attackInProgress = true;
        }
        
        if (elapsedDelay < attackDelay)
        {
            elapsedDelay += Time.deltaTime;

            if (elapsedDelay >= attackDelay)
            {
                hub.Damage(damage);
                attackInProgress = false;
            }
        }            
        else
        {
            if (elapsedCooldown == 0)
                attackCoolingDown = true;

            elapsedCooldown += Time.deltaTime;

            if (elapsedCooldown >= attackCooldown)
            {
                elapsedDelay = 0;
                elapsedCooldown = 0;
                attackCoolingDown = false;
                animator.SetBool("Attacking", false);
            }
        }
    }
    #endregion

    #region MISC
    [Space]
    [HideInInspector] public Hub hub;
    [SerializeField] protected Animator animator;
    #endregion

    #region DEBUG
    [Header("Debug")]
    [SerializeField] bool showPath;
    [SerializeField] bool showLevers;
    #endregion

}