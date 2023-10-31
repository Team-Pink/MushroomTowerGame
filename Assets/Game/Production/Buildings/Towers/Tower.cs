using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public struct Target
{
    public Vector3 position; // Be warned this is not a reference to enemy.transform.position it is in most cases it's own self contained value as .position returns a value not a reference.
    public Enemy enemy;


    //public Vector3 GetPosition()
    //{
    //    if (enemy)
    //    {
    //        return enemy.transform.position;
    //    }
    //    else
    //    {
    //        return position;
    //    }
    //}


    float timeFound
    {
        get => Time.time;
    }

    public Target(Vector3 targetPos, Enemy targetEnemy = null)
    {
        position = targetPos;
        enemy = targetEnemy;
    }
}

[Serializable]
public enum TargeterType
{
    SelectAType,
    Close,
    Cluster,
    Fast,
    Strong,
    Track
} // For Editor Use Only
[Serializable]
public enum AttackerType
{
    SelectAType,
    Area,
    Single,
    Trap
} // For Editor Use Only

[Serializable]
public struct Details
{
    public string name;
    public TargeterType targeterType;
    public AttackerType attackerType;

    public Details(string nameInit, TargeterType targeterInit, AttackerType attackerInit)
    {
        name = nameInit;
        targeterType = targeterInit;
        attackerType = attackerInit;
    }
} // For Editor Use Only

public class Tower : Building
{
    // Startup
    [SerializeField] float growthDuration = 5.0f;
    private float growthTime = 0.0f;
    private bool recovering = false;
    [SerializeField] float recoveryDuration = 2.0f;
    private float recoveryTime = 0.0f;


    // Components
    [SerializeField] protected Animator animator;
    [SerializeReference] private Attacker attackerComponent;
    [SerializeReference] private Targeter targeterComponent;
    public Details details; // For Editor Use Only
    protected new Transform transform;

    public Attacker AttackerComponent { get => attackerComponent; set => attackerComponent = value; }

    public Targeter TargeterComponent { get => targeterComponent; set => targeterComponent = value; }

    // References
    private HashSet<Target> targets = new();

    // Upgrading
    [SerializeField] bool upgradeable;
    public bool Upgradeable { get; private set; }
    private GameObject upgradePrefabL;
    public int upgradeCostL;
    private GameObject upgradePrefabR;
    public int upgradeCostR;

    // Purchasing
    public int purchaseCost = 10;
    [Range(0, 1)] public float sellReturnPercent = 0.5f;


    // prefabs
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] GameObject attackObjectPrefab;

    // Tower values
    [SerializeField] private float projectileSpeed = 1.5f; // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! assign this properly on prefabs!
    // setting the above to zero means time to target will equal zero because (Distance * projectileSpeed). Doing this will cause attacks to occur the frame their cooldown ends and they have a target.

    // Tags from Lochlan

    //Lock On
    private bool chargingLaser = false;
    [SerializeField] private bool lockOn = false; // determines if the tower will lock on to an enemy.
    [SerializeField] private float lockOnDuration = 1.5f;
    private float lockOnTimer = 0;
    [SerializeField] private float lockOnFiringIntermissionTime = 0;
    private Target lockOnTarget;

    [SerializeField] GameObject ChargeUpTransform;
    [SerializeField] GameObject chargeUpParticlePrefab;
    private GameObject chargeDownParticleRef;




    [SerializeField] AudioClip buildAudio;

    [SerializeField] SkinnedMeshRenderer[] renderers;
    [HideInInspector] private Material[] activeMaterials;
    [SerializeField, Space()] Material[] deactivatedMaterials;

    [Space(20)]
    [SerializeField, Tooltip("ONLY FOR USE ON THE BOOMERANG TOWER")] SkinnedMeshRenderer boomerangCap;

    private void Awake()
    {
        attackerComponent.animator = animator;

        transform = gameObject.transform;
        targeterComponent.transform = transform;
        attackerComponent.transform = transform;

        targeterComponent.enemyLayer = LayerMask.GetMask("Enemy");

        attackerComponent.bulletPrefab = bulletPrefab;
        AttackerComponent.attackObjectPrefab = attackObjectPrefab;
        attackerComponent.originReference = this; // I am very open to a better way of doing this so please if you can rearchitect this go ahead. !!!

        radiusDisplay.transform.localScale = new Vector3(2 * targeterComponent.range, 2 * targeterComponent.range);

        AudioManager.PlaySoundEffect(buildAudio.name, 1);

        activeMaterials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            activeMaterials[i] = renderers[i].sharedMaterial;
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0) return; // for use by the pause menu.

        if (growthTime < growthDuration)
        {
            growthTime += Time.deltaTime;
            return;
        }

        // This will stop the tower from attacking imediately after being reactivated.
        if (recovering)
        {
            if (recoveryTime < recoveryDuration)
            {
                recoveryTime += Time.deltaTime;
                return;
            }
            else
            {
                recovering = false;
                base.Reactivate();
            }
        }

        // Attack Logic.
        if (Active)
        {
            if (attackerComponent.bounce && attackerComponent.bounceBulletInTowerPossession)
            {
                if (attackerComponent.returning)
                {
                    boomerangCap.enabled = true;
                    attackerComponent.returning = false;
                    animator.SetBool("Attack Recoil", true);
                }
            }

            targets = targeterComponent.AcquireTargets();
            if (targets != null)
            {
                if (lockOn)
                {
                    // animate laser charge up if an enemy enters range.
                    if (!chargingLaser)
                    {
                        animator.SetTrigger("Attack Charge Up");
                        chargingLaser = true;
                        chargeDownParticleRef = Instantiate(chargeUpParticlePrefab, ChargeUpTransform.transform);
                    }

                    if (LockedOn())
                    {
                        CalcTimeToTarget(targets, transform.position);
                        attackerComponent.Attack(targets);
                    }
                }
                else if (attackerComponent.bounce)
                {
                    if (attackerComponent.bounceBulletInTowerPossession)
                    {
                        if (attackerComponent.CheckCooldownTimer())
                            attackerComponent.Attack(targets); // Generates an attack query that will create an attack object.
                    }
                    else if (boomerangCap.enabled == true) boomerangCap.enabled = false;
                }
                else if (attackerComponent.CheckCooldownTimer())
                {
                    CalcTimeToTarget(targets, transform.position);
                    attackerComponent.Attack(targets);
                }
            }
            else if (lockOn && chargingLaser)
            {
                animator.SetTrigger("Attack End");
                chargingLaser = false;
                Destroy(chargeDownParticleRef);              
            }
        }
        else if(chargingLaser)
        {
            chargingLaser = false;
            Destroy(chargeDownParticleRef);           
        }
    }



    public void Upgrade(int upgradePath)
    {
        if (upgradePath == 0 || upgradePath == 1)
        {
            GameObject selectedUpgradePrefab;

            if (upgradePath == 1)
                selectedUpgradePrefab = upgradePrefabL;
            else
                selectedUpgradePrefab = upgradePrefabR;

            Instantiate(selectedUpgradePrefab, transform.position, transform.rotation, transform.parent);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("Upgrade only accepts an int value of 0 or 1", this);
        }
    }

    public override void Deactivate()
    {
        base.Deactivate();
        animator.SetTrigger("Deactivate");
        recoveryTime = 0.0f;

        for (int i = 0; i < deactivatedMaterials.Length; i++)
        {
            renderers[i].material = deactivatedMaterials[i];
        }
    }

    public override void Reactivate()
    {
        recovering = true;
        animator.SetTrigger("Reactivate");

        for (int i = 0; i < activeMaterials.Length; i++)
        {
            renderers[i].material = activeMaterials[i];
        }
    }

    public override void Sell()
    {
        CurrencyManager currencyManager = GameObject.Find("GameManager").GetComponent<CurrencyManager>();
        currencyManager.IncreaseCurrencyAmount(purchaseCost, sellReturnPercent);

        Destroy(gameObject);

        base.Sell();
    }
    public void NewPrice(float multiplier) => purchaseCost = (int)(purchaseCost * multiplier);
    public int SellPrice() => (int)(purchaseCost * sellReturnPercent);

    /// <summary>
    /// literally exists because bounce needs to port it's own stuff
    /// </summary>
    /// <returns></returns>
    public GameObject GetAttackObjectPrefab()
    {
        return attackObjectPrefab;
    }

    #region LockOn

    private bool LockedOn()
    {
        if (targets.Count == 0) return false; // this is unneccessary due to the targeter returning null so count will never be zero and there is already a null check in the update loop.

        bool lockedOn = false;

        if (lockOnTarget.enemy != targets.First().enemy)
        {
            lockOnTarget = targets.First();
            lockOnTimer = 0;
            return lockedOn;
        }
        //Debug.DrawLine(transform.position, lockOnTarget.position, Color.blue, Time.deltaTime); // This debug line proves that the position value in a Target is not a reference.

        lockOnTimer += Time.deltaTime;
        if (lockOnTimer > lockOnDuration + lockOnFiringIntermissionTime)
        {
            lockedOn = true;
            lockOnTimer = lockOnDuration;
            Debug.DrawLine(transform.position, lockOnTarget.position, Color.red, Mathf.Infinity);
        }
        return lockedOn;
    }
    #endregion

    /// <summary>
    /// uses distance to target and attackSpeed to calculate the travel time of an attack to it's target.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="originPos"></param>
    public void CalcTimeToTarget(HashSet<Target> targets, Vector3 originPos)
    {
        //int TargetCounter = 0;
        foreach (Target target in targets)
        {

            attackerComponent.attackDelay = Vector3.Distance(originPos, target.position) / projectileSpeed;
            return;
            /*TargetCounter++;
            if (TargetCounter == targetProjectileSpeedCounter)
            {
                if (targetProjectileSpeedCounter >= numTargets)
                    targetProjectileSpeedCounter = 0;
                return; // yes I am indeed entering a foreach loop just to get a reference to the first object in targets and then returning without examining the other targets.
            }*/

        }

    }
}
