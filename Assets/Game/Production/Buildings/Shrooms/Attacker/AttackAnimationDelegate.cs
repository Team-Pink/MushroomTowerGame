using UnityEngine;

public class AttackAnimationDelegate : MonoBehaviour
{
    [SerializeField] Shroom shroom;
    private Attacker attacker;
    private void Awake()
    {
        if (shroom == null) attacker = transform.parent.GetComponent<Shroom>().AttackerComponent;
        else attacker = shroom.AttackerComponent;
    }

    public void Attack()
    {
        attacker.AnimateProjectile();
    }
}