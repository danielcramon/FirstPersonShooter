using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth;


    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    protected virtual void Update()
    {
        if(currentHealth <= 0)
        {
            Death();
        }
    }

    protected virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
    }

    protected virtual void Death()
    {

    }

    protected virtual void SetFullHealth()
    {
        currentHealth = maxHealth;
    }
}
