using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : Fighter
{
    public TextMeshProUGUI health;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        SetText();
    }

    protected override void SetFullHealth()
    {
        base.SetFullHealth();
        SetText();
    }

    private void SetText()
    {
        if(health != null)
        {
            health.text = currentHealth + " / " + maxHealth;
        }
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        SetText();
    }

    protected override void Death()
    {
        GameManager.instance.SpawnPlayer();
        SetFullHealth();
    }
}
