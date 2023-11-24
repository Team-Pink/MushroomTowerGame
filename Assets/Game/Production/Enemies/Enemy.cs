using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

[Serializable]
public class Condition
{
    public enum ConditionType
    {
        None,
        Poison,
        Slow
    }

    public ConditionType type;
    public float value;
    [HideInInspector] public float currentDuration;
    public float totalDuration;
    [HideInInspector] public float timer;
    [HideInInspector] public bool applied;

    public Condition(ConditionType typeInit, float valueInit, float durationInit)
    {
        type = typeInit;
        value = valueInit;
        totalDuration = durationInit;
        applied = false;
        currentDuration = 0;
    }

    public bool Duration()
    {
        if (currentDuration >= totalDuration)
            return true;
        else
            currentDuration += Time.deltaTime;

        return false;
    }
}

public class Enemy : MonoBehaviour
{
    [Serializable] public enum EnemyState
    {
        None,
        Approach,
        Hunt,
        Attack
    }
    public EnemyState state = EnemyState.Approach;

    #region Health Values
    [Header("Health")]
    [SerializeField] int maxHealth;
    //[HideInInspector]
    public float health;
    public bool dead;
    public int MaxHealth { get => maxHealth; }
    public float CurrentHealth { get => health; protected set => health = value; }
    public bool Dead { get => dead; protected set => dead = value; }
    #endregion

    private readonly List<Condition> activeConditions = new();

    #region Movement Values
    public struct BoidReference
    {
        public Transform transform;
        public Rigidbody rigidbody;
        public Enemy logic;

        public BoidReference(Transform Transform, Rigidbody Rigidbody, Enemy Logic)
        {
            transform = Transform;
            rigidbody = Rigidbody;
            logic = Logic;
        }
    }

    [Header("Movement")]
    [SerializeField, Range(0.0f, 5.0f)] float speed = 3.0f;
    public float Speed
    {
        get
        {
            float result = speed;
            foreach (float modifier in speedModifiers)
            {
                result *= modifier;
            }

            return result;
        }
    }
    protected readonly List<float> speedModifiers = new();
    [SerializeField, Range(0.0f, 1.0f)] float steeringForce = 0.15f;
    [SerializeField, Range(0, 10)] int maxNeighbourhoodSize = 10;

    [Serializable]
    public class InfluenceDetails
    {
        [Range(0.0f, 5.0f)] public float targetingStrength = 3.0f;
        [Space()]
        public float alignmentRange = 5.0f;
        [Range(0.0f, 5.0f)] public float alignmentStrength = 0.6f;
        [Space()]
        public float cohesionRange = 5.0f;
        [Range(0.0f, 5.0f)] public float cohesionStrength = 0.1f;
        [Space()]
        public float seperationRange = 1.5f;
        [Range(0.0f, 5.0f)] public float seperationStrength = 2.0f;
        [Space()]
        public float avoidanceRange = 1.5f;
        [Range(0.0f, 5.0f)] public float avoidanceStrength = 2.0f;
    }

    [SerializeField] InfluenceDetails influences = new();

    private float NeighbourhoodRange
    {
        get => Mathf.Max(influences.alignmentRange, influences.cohesionStrength, influences.seperationRange);
    }

    private LayerMask enemyLayers;
    private LayerMask obstacleLayers;

    // References
    [HideInInspector] public List<BoidReference> neighbourhood = new();
    private readonly List<GameObject> obstacles = new();

    #endregion

    #region Attacking Values
    [Header("Attacking")]
    protected Building targetBuilding;

    [SerializeField] protected int damage;

    [SerializeField] protected float attackCooldown = 0;
    [SerializeField] protected float attackDelay = 0;
    protected float elapsedCooldown = 0;
    protected float elapsedDelay = 0;

    [SerializeField, Range(0.0f, 10.0f)] protected float attackRadius = 3.0f;
    private float AttackRadiusSqr { get => attackRadius * attackRadius; }

    protected bool attackInProgress = false;
    protected bool attackCoolingDown = false;
    #endregion

    #region OTHER VALUES
    // Drops
    [Header("Drops")]
    [SerializeField] int bugBits = 2;

    // Components
    [Header("Components")]
    [SerializeField] protected Animator animator;
    protected new Transform transform;
    protected new Rigidbody rigidbody;
    [HideInInspector] public LevelDataGrid levelData;
    [HideInInspector] public Transform meteorTransform;
    [HideInInspector] public Meteor meteor;

    [SerializeField] GameObject deathParticle;
    [SerializeField] protected float particleOriginOffset;
    [SerializeField] SkinnedMeshRenderer meshRenderer;
    private Material defaultMaterial;
    [SerializeField] Material hurtMaterial;
    [SerializeField] GameObject poisonParticles;

    [SerializeField] AudioClip attackAudio;
    [SerializeField] AudioClip deathAudio;

    #endregion

    protected virtual void Awake()
    {
        transform = GetComponent<Transform>();
        rigidbody = GetComponent<Rigidbody>();
        defaultMaterial = meshRenderer.material;

        enemyLayers = LayerMask.GetMask("Enemy");
        obstacleLayers = LayerMask.GetMask("Shroom", "Node");

        health = maxHealth;
    }

    public void Initialise(Meteor meteorInit, LevelDataGrid levelDataInit)
    {
        meteor = meteorInit;
        meteorTransform = meteor.transform;
        levelData = levelDataInit;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (InteractionManager.tutorialMode) rigidbody.velocity = Vector3.zero;
        if (InteractionManager.gamePaused) return;

        if (Dead) return;

        ExecuteConditionEffects();

        switch (state)
        {
            case EnemyState.Approach:
                ApproachState();
                break;
            case EnemyState.Hunt:
                HuntState();
                break;
            case EnemyState.Attack:
                AttackState();
                break;
        }
    }

    #region Health Logic
    public virtual void TakeDamage(float damage)
    {
        Debug.Log("Enemy took damage");
        health -= damage;
        if (health > 0) StartCoroutine(DisplayHurt());
    }

    private IEnumerator DisplayHurt()
    {
        meshRenderer.material = hurtMaterial;
        yield return new WaitForSeconds(0.5f);
        meshRenderer.material = defaultMaterial;
    }

    public bool CheckIfDead()
    {
        return CurrentHealth <= 0;
    }
    public void OnDeath()
    {
        if (Dead) return;
        Dead = true; // object pool flag;

        // death animation
        AudioManager.PlaySoundEffect(deathAudio.name, 0);

        // increment currency
        CurrencyManager currencyManager = GameObject.Find("GameManager").GetComponentInChildren<CurrencyManager>();
        currencyManager.IncreaseCurrencyAmount(bugBits);

        state = EnemyState.None;

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        GetComponent<Rigidbody>().detectCollisions = false;
        if (deathParticle != null)
        {
            GameObject particle = Instantiate(deathParticle, transform);
            particle.transform.position += new Vector3(0, particleOriginOffset, 0);
        }
    }
    #endregion

    #region Condition Logic
    public void ApplyConditions(Condition[] conditions)
    {
        for (int newIndex = 0; newIndex < conditions.Length; newIndex++)
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
            {

                Condition detailCondition = conditions[newIndex];
                Condition condition = new(detailCondition.type, detailCondition.value, detailCondition.totalDuration);
                activeConditions.Add(condition);
                conditions[newIndex].applied = false;
            }
        }
    }

    void ExecuteConditionEffects()
    {
        bool poisoned = false;
        List<Condition> markedForRemoval = new();
        foreach (Condition condition in activeConditions)
        {
            if (condition.type == Condition.ConditionType.Poison)
            {
                poisoned = true;

                TakeDamage(condition.value * Time.deltaTime);

                if (CheckIfDead())
                {
                    OnDeath();
                    return;
                }

                if (condition.Duration())
                    markedForRemoval.Add(condition);
            }//POISON CONDITION
            else if (condition.type == Condition.ConditionType.Slow)
            {
                if (condition.applied == false)
                {
                    speedModifiers.Add(condition.value);
                    condition.applied = true;
                }
                    
                if (condition.Duration())
                {
                    speedModifiers.Remove(condition.value);
                    markedForRemoval.Add(condition);
                }
            }//SLOW CONDITION
            
        }//CONDITIONS

        if (poisonParticles != null)
        {
            if (poisonParticles.activeSelf != poisoned)
            {
                poisonParticles.SetActive(poisoned);
            }
        }

        foreach (Condition condition in markedForRemoval)
            activeConditions.Remove(condition);
    }
    #endregion

    #region Movement Logic
    //APPROACH
    protected virtual void ApproachState()
    {
        if ((meteorTransform.position - transform.position).sqrMagnitude < AttackRadiusSqr)
        {
            rigidbody.velocity = Vector2.zero;
            state = EnemyState.Attack;
            targetBuilding = meteor;
            neighbourhood.Clear();
            return;
        }

        // Get Boids in Neighbourhood
        PopulateNeighbourhood();
        PopulateObstacles();

        // Flocking
        Vector3 newVelocity = Flock();

        // Targeting
        newVelocity += influences.targetingStrength * levelData.GetFlowAtPoint(transform.position);

        newVelocity += Avoid();

        // Apply New Velocity
        rigidbody.velocity = Speed * Vector3.MoveTowards(rigidbody.velocity.normalized, newVelocity.normalized, steeringForce);

        // Face Direction of Movement
        if (rigidbody.velocity != Vector3.zero)
        {
            transform.forward = rigidbody.velocity.normalized;
        }
    }

    private void PopulateNeighbourhood()
    {
        neighbourhood.Clear();
        var boidColliderList = Physics.OverlapSphere(transform.position, NeighbourhoodRange, enemyLayers);

        for (int colliderIndex = 0; colliderIndex < boidColliderList.Length; colliderIndex++)
        {
            Collider boidCollider = boidColliderList[colliderIndex];
            if (boidCollider.gameObject != gameObject)
            {
                Enemy boidLogic = boidCollider.GetComponent<Enemy>();

                //if (boidLogic.state != EnemyState.Approach) continue;

                Transform boidTransform = boidCollider.transform;
                Rigidbody boidRigidbody = boidCollider.GetComponent<Rigidbody>();

                neighbourhood.Add(new(boidTransform, boidRigidbody, boidLogic));

                if (neighbourhood.Count < maxNeighbourhoodSize) continue;

                int furthestIndex = 0;
                float furthestSqrMag = (neighbourhood[furthestIndex].transform.position - transform.position).sqrMagnitude;
                for (int currentIndex = 1; currentIndex < neighbourhood.Count; currentIndex++)
                {
                    float currentSqrMag = (neighbourhood[currentIndex].transform.position - transform.position).sqrMagnitude;
                    if (currentSqrMag > furthestSqrMag) furthestIndex = currentIndex;
                }

                neighbourhood.RemoveAt(furthestIndex);
            }
        }
    }

    private void PopulateObstacles()
    {
        obstacles.Clear();
        var obstacleColliderList = Physics.OverlapSphere(transform.position, influences.avoidanceStrength, obstacleLayers);

        for (int colliderIndex = 0; colliderIndex < obstacleColliderList.Length; colliderIndex++)
        {
            obstacles.Add(obstacleColliderList[colliderIndex].gameObject);
        }
    }

    private Vector3 Avoid()
    {
        Vector3 avoidanceInfluence = Vector3.zero;
        foreach (GameObject obstacle in obstacles)
        {
            avoidanceInfluence += -(obstacle.transform.position - gameObject.transform.position).normalized;
        }

        return avoidanceInfluence;
    }

    private Vector3 Flock()
    {
        Vector3 alignmentInfluence = Vector3.zero, cohesionInfluence = Vector3.zero, seperationInfluence = Vector3.zero;
        foreach (BoidReference boid in neighbourhood)
        {
            alignmentInfluence += Align(boid);
            cohesionInfluence += Cohere(boid);
            seperationInfluence += Seperate(boid);
        }
        Vector3 result = alignmentInfluence.normalized * influences.alignmentStrength;
        result += cohesionInfluence.normalized * influences.cohesionStrength;
        result += seperationInfluence.normalized * influences.seperationStrength;

        return result;
    }

    private bool BoidInRange(Transform boidTransform, float range)
    {
        return (boidTransform.position - transform.position).sqrMagnitude < (range * range);
    }

    private Vector3 Align(BoidReference boid)
    {
        if (BoidInRange(boid.transform, influences.alignmentRange))
            return boid.rigidbody.velocity;
        else
            return Vector3.zero;
    }
    private Vector3 Cohere(BoidReference boid)
    {
        if (BoidInRange(boid.transform, influences.cohesionRange))
            return (boid.transform.position - gameObject.transform.position).normalized;
        else
            return Vector3.zero;
    }
    private Vector3 Seperate(BoidReference boid)
    {
        if (BoidInRange(boid.transform, influences.seperationRange))
            return -(boid.transform.position - gameObject.transform.position).normalized;
        else
            return Vector3.zero;
    }

    // HUNT
    protected virtual void HuntState()
    {
        // use this for node attacker
    }
    #endregion

    #region Attacking Logic
    protected virtual void AttackState()
    {
        if (elapsedCooldown == 0 && elapsedDelay == 0)
        {
            animator.SetTrigger("Attack");
            attackInProgress = true;
        }

        if (elapsedDelay < attackDelay)
        {
            elapsedDelay += Time.deltaTime;

            if (elapsedDelay >= attackDelay)
            {
                meteor.Damage(damage);
                AttackAudio();
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

    protected void AttackAudio()
    {
        AudioManager.PlaySoundEffect(attackAudio.name, 0);
    }
}