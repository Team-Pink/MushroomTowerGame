using UnityEngine;

public class AttackAnimationDelegate : MonoBehaviour
{
    [SerializeField] Shroom tower;
    private Attacker attacker;
    private void Awake()
    {
        if (tower == null) attacker = transform.parent.GetComponent<Shroom>().AttackerComponent;
        else attacker = tower.AttackerComponent;
    }

    public void Attack()
    {
        attacker.AnimateProjectile();
    }
}