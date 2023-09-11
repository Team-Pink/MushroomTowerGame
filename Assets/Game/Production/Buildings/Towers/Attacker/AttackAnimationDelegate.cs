using UnityEngine;

public class AttackAnimationDelegate : MonoBehaviour
{
    [SerializeField] Tower tower;
    private Attacker attacker;
    private void Awake()
    {
        if (tower == null) attacker = transform.parent.GetComponent<Tower>().AttackerComponent;
        else attacker = tower.AttackerComponent;
    }

    public void Attack()
    {
        attacker.AnimateProjectile();
    }
}