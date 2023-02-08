using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomBullets : MonoBehaviour
{
    //Assignables
    public Rigidbody rb;
    public GameObject explosion;
    public LayerMask whatIsEnemies;

    //stats
    [Range(0f, 1f)]
    public float bounciness;
    public bool useGravity;

    //Damage
    private int damage;
    public float explosionRange;

    //Lifetime
    public float maxLifetime;

    PhysicMaterial physics_mat;

    //Graphics
    public GameObject bulletHoleGraphic;

    private void Start()
    {
        Setup();
    }

    private void Update()
    {
        //Count down lifetime
        maxLifetime -= Time.deltaTime;
        if(maxLifetime <= 0)
        {
            Explode();
        }
    }

    private void Explode()
    {
        //Instantiate explosion
        if(explosion != null)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
        }

        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRange, whatIsEnemies);
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].SendMessage("TakeDamage", damage);
        }

        //Add a little delay, just to make sure everything works fine
        Invoke("Delay", 0.005f);
    }

    private void Delay()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (bulletHoleGraphic != null)
        {
            if (collision.collider.CompareTag("Ground"))
            {
                GameObject bulletHole = Instantiate(bulletHoleGraphic, collision.contacts[0].point + collision.contacts[0].normal * 0.0001f, Quaternion.identity);
                bulletHole.transform.LookAt(collision.contacts[0].point + collision.contacts[0].normal);
                Explode();
            }
        }
        if(collision.collider.CompareTag("Fighter"))
        {
            Explode();
        }
    }

    private void Setup()
    {
        //Creatge a new Physic material
        physics_mat = new PhysicMaterial();
        physics_mat.bounciness = bounciness;
        physics_mat.frictionCombine = PhysicMaterialCombine.Minimum;
        physics_mat.bounceCombine = PhysicMaterialCombine.Maximum;

        //Assign material to collider
        GetComponent<SphereCollider>().material = physics_mat;

        rb.useGravity = useGravity;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }

    public void SetDamage(int damage)
    {
        this.damage = damage;
    }

}
