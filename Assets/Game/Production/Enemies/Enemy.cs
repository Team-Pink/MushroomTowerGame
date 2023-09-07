using UnityEngine;
using Text = TMPro.TMP_Text;
using System.Collections;
using System;
using System.Collections.Generic;

public enum ConditionType
{
    None,
    Infection,
    Poison,
    Slow,
    Stagger,
    Vulnerability
}

public class Condition
{
    public ConditionType type;
    public float value;
    public float duration;

    public Condition(ConditionType typeInit, float valueInit, float durationInit)
    {
        type = typeInit;
        value = valueInit;
        duration = durationInit;
    }

    public bool Duration()
    {
        if (duration < 0)
            return true;

        duration -= Time.deltaTime;
        return false;
    }
}

public class Enemy : MonoBehaviour
{
    List<Condition> activeConditions;

    protected virtual void CustomAwakeEvents()
    {

    }

    private void Awake()
    {
        points = pathToFollow.GetPoints();

        health = maxHealth;

        CustomAwakeEvents();
    }

    private void Update()
    {
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


    public void ApplyConditions(Condition[] conditions)
    {
        for(int newIndex = 0; newIndex < conditions.Length; newIndex++)
        {
            bool shouldApply = true;
            for (int activeIndex = 0; activeIndex < activeConditions.Count; activeIndex++)
            {
                if (conditions[newIndex].type != activeConditions[activeIndex].type)
                    continue;

                if (conditions[newIndex].value > activeConditions[activeIndex].value)
                {
                    activeConditions.RemoveAt(activeIndex); break;
                }
                else
                {
                    shouldApply = false; continue;
                }
            }

            if (shouldApply)
                activeConditions.Add(conditions[newIndex]);
        }
    }


    #region ALIVE STATUS
    [Header("Health")]
    [SerializeField] Text healthText;
    public float health;
    public bool isDead;
    [SerializeField] int maxHealth;
    public float CurrentHealth
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

    public virtual IEnumerator TakeDamage(float damage, float delay = 0)
    {
        yield return new WaitForSeconds(delay);
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
    List<Vector3> points = new();

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
    [SerializeField] protected float attackDelay = 0;
    protected float elapsedCooldown = 0;
    protected float elapsedDelay = 0;
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
    private bool attackMode;

    protected bool attackInProgress = false;
    protected bool attackCoolingDown = false;

    //there are areas I can further optimise and clean up but that will be a later thing
    protected virtual void AttackHub()
    {
        if(elapsedCooldown == 0 && elapsedDelay == 0)
        {
            animator.SetTrigger("Attack");
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