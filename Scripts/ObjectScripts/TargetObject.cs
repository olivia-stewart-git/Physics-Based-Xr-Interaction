using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetObject : MonoBehaviour, IDamageable
{
    [SerializeField] private Animator targetAnimator;
    public float maxHealth = 100f;
    private float curHealth;


    private bool isDead;
    public void OnTakeDamage(float damage, Vector3 position, Vector3 direction, float impulse)
    {
        if (isDead) return;
        curHealth -= damage;

        if(curHealth <= 0f)
        {
            Die();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        curHealth = maxHealth;
    }

    void Die()
    {
        isDead = true;
        targetAnimator.SetTrigger("Dead");
        StartCoroutine(ResetTarget());
    }

    IEnumerator ResetTarget()
    {
        yield return new WaitForSeconds(5f);
        isDead = false;
        curHealth = maxHealth;
    }
}
