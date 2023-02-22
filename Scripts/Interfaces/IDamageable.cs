using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable 
{
    public void OnTakeDamage(float damage, Vector3 position, Vector3 direction, float impulse);
}
