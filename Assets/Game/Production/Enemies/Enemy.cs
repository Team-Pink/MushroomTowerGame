using Vector3List = System.Collections.Generic.List<UnityEngine.Vector3>;
using UnityEngine;
using Text = TMPro.TMP_Text;
using System.Collections;

public class Enemy : MonoBehaviour
{
    private void Awake()
    {
        points = pathToFollow.GetPoints();
        health = maxHealth;
    }

    protected virtual void Playing()
    {
        if (Dead) return;

        healthText.text = CurrentHealth.ToString();

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
    [SerializeField] int maxHealth;
    private int health;
    public int CurrentHealth
    {
        get => health;
        protected set => health = value;
    }
    public int MaxHealth
    {
        get => maxHealth;
        private set { }
    }
    public bool Dead
    {
        get;
        protected set;
    }          
    // this is specifically for the ondeath function. to replace the functionality of checking
    // health in update and setting isDead in Ondeath so it can only run once.

    [Header("Provides On Death")]
    [SerializeField] int bugBits = 2;
    public int expValue = 1;

    public IEnumerator TakeDamage(int damage, float delay)
    {
        yield return new WaitForSeconds(delay);
        CurrentHealth -= damage;
        if(CheckIfDead()) OnDeath();
    }
    public void SpawnIn()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
        GetComponent<Rigidbody>().detectCollisions = true;
        Dead = false;

        //whatever else needs to be done before fully spawning in do within here

    }

    private bool CheckIfDead()
    {
        return CurrentHealth <= 0;
    }
    private void OnDeath()
    {
        if (Dead) return; // don't increase currency twice.
        Dead = true; // object pool flag;

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
        else
            AttackMode = false;
    }

    private void RotateToFaceTravelDirection()
    {
        Vector3 lookDirection = (points[currentPoint + 1] - points[currentPoint]).normalized;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), progress);
    }
    #endregion

    #region Attacking
    [Header("Attacking")]
    [SerializeField] protected float attackCooldown;
    [HideInInspector] protected bool AttackMode
    {
        get
        {
            return attackMode;
        }
        private set
        {
            attackMode = value;
        }
    }
    protected float elapsedCooldown;
    protected bool attackInProgress;
    private bool attackMode;

    protected void AttackHub()
    {
        if (!attackInProgress)
        {
            animator.SetTrigger("Attack");
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
    }
    #endregion

    #region MISC
    [Header("Components")]
    public Hub hub;
    [SerializeField] protected LayerMask range;
    [SerializeField] protected Animator animator;
    #endregion
    //move to different location if there is a better spot for these variables

    #region DEBUG
    [Header("Debug")]
    [SerializeField] bool showPath;
    [SerializeField] bool showLevers;
    #endregion

}