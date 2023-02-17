using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Fighter
{
    public NavMeshAgent agent;

    public Transform player;

    //Patroling
    public Transform[] moveSpots;
    public int randomSpot;
    private float waitTime;
    public float startWaitTime = 1.0f;

    //Ai strafe
    public float distToPlayer = 5.0f;
    private float randomStrafeStartTime;
    private float waitStrafeTime;
    public float t_minStrafe;
    public float t_maxStrafe;
    public Transform strafeRight;
    public Transform strafeLeft;
    private int randomStrafeDir;

    //Ai sight
    public bool playerIsInLOS = false;
    public float fieldOfViewAngle = 160f;
    public float losRadius = 100f;

    //Ai sight and memory
    private bool aiMemorizesPlayer = false;
    public float memoryStartTime = 10f;
    private float increasingMemoryTime;

    //Ai hearing
    Vector3 noisePosition;
    private bool aiHeardPlayer = false;
    public float shootNoiseTravelDistance = 100f;
    public float jumpNoiseTravelDistance = 50f;
    public float spinSpeed = 3f;
    private bool canSpin = false;
    private float isSpinningTime;
    public float spinTime = 3f;

    //Chase
    public float chaseRadius = 100.0f;
    public float facePlayerFactor = 20.0f;

    //Attacking
    public float shootRadius = 50f;
    public float timeBetweenAttacks;
    private bool alreadyAttacked;
    public GameObject bullet;
    public Transform attackPoint;
    public float spread;
    public int damage;
    public float shootForce;
    public AudioSource fireSound;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = true;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        player = GameManager.instance.player.transform;
        waitTime = startWaitTime;
        randomSpot = Random.Range(0, moveSpots.Length);
        moveSpots = GameManager.instance.moveSpots;
    }

    protected override void Update()
    {
        base.Update();
        float distance = Vector3.Distance(player.position, transform.position);
        if(distance <= losRadius)
        {
            CheckLOS();
        }
        if (agent.isActiveAndEnabled)
        {
            if(!playerIsInLOS && !aiMemorizesPlayer && !aiHeardPlayer)
            {
                Patroling();
                NoiseCheck();
                StopCoroutine(AiMemory());
            }
            else if(aiHeardPlayer && !playerIsInLOS && !aiMemorizesPlayer)
            {
                canSpin = true;
                GoToNoisePosition();
            }
            else if (playerIsInLOS && distance <= shootRadius)
            {
                aiMemorizesPlayer = true;
                FacePlayer();
                AttackPlayer();
            }
            else if (playerIsInLOS)
            {
                aiMemorizesPlayer = true;
                FacePlayer();
                ChasePlayer();
            }
            else if(aiMemorizesPlayer && !playerIsInLOS)
            {
                ChasePlayer();
                StartCoroutine(AiMemory());
            }
        }
    }

    private void Patroling()
    {
        agent.SetDestination(moveSpots[randomSpot].position);

        if(Vector3.Distance(transform.position, moveSpots[randomSpot].position) < 2.0f)
        {
            if(waitTime <= 0)
            {
                randomSpot = Random.Range(0, moveSpots.Length);

                waitTime = startWaitTime;
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
        }
    }

    private void ChasePlayer()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= chaseRadius && distance > distToPlayer)
        {
            agent.SetDestination(player.position);
        }
        else if(agent.isActiveAndEnabled && distance <= distToPlayer)
        {
            randomStrafeDir = Random.Range(0, 2);
            randomStrafeStartTime = Random.Range(t_minStrafe, t_maxStrafe);

            if(waitStrafeTime <= 0)
            {
                if (randomStrafeDir == 0)
                {
                    agent.SetDestination(strafeLeft.position);
                }
                else if(randomStrafeDir == 1)
                {
                    agent.SetDestination(strafeRight.position);
                }
                waitStrafeTime = randomStrafeStartTime;
            }
            else
            {
                waitStrafeTime -= Time.deltaTime;
            }
            
        }
    }

    private void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * facePlayerFactor);
    }

    private void NoiseCheck()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        if(distance <= shootNoiseTravelDistance)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                noisePosition = player.position;

                aiHeardPlayer = true;
            }
            else
            {
                aiHeardPlayer = false;
                canSpin = false;
            }
        }
        if(distance <= jumpNoiseTravelDistance)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                noisePosition = player.position;

                aiHeardPlayer = true;
            }
            else
            {
                aiHeardPlayer = false;
                canSpin = false;
            }
        }
    }

    private void GoToNoisePosition()
    {
        agent.SetDestination(noisePosition);

        if(Vector3.Distance(transform.position, noisePosition) <= 5f && canSpin)
        {
            isSpinningTime += Time.deltaTime;

            transform.Rotate(Vector3.up * spinSpeed, Space.World);

            if(isSpinningTime >= spinTime)
            {
                canSpin = false;
                aiHeardPlayer = false;
                isSpinningTime = 0f;
            }
        }
    }

    IEnumerator AiMemory()
    {
        increasingMemoryTime = 0;

        while(increasingMemoryTime < memoryStartTime)
        {
            increasingMemoryTime += Time.deltaTime;
            aiMemorizesPlayer = true;
            yield return null;
        }

        aiHeardPlayer = false;
        aiMemorizesPlayer = false;

    }

    private void CheckLOS()
    {
        Vector3 direction = player.position - transform.position;
        float angle = Vector3.Angle(direction, transform.forward);

        if(angle < fieldOfViewAngle * 0.5f)
        {
            RaycastHit hit;

            if(Physics.Raycast(transform.position, direction.normalized, out hit, losRadius))
            {
                if(hit.collider.name == "Player Body")
                {
                    playerIsInLOS = true;
                    aiMemorizesPlayer = true;
                }
                else
                {
                    playerIsInLOS = false;
                }
            }
        }
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        if (!alreadyAttacked)
        {
            Vector3 directionWithoutSpread = player.transform.position - attackPoint.position;

            //Spread
            float x = Random.Range(-spread, spread);
            float y = Random.Range(-spread, spread);
            float z = Random.Range(-spread, spread);

            //Calculate Direction with Spread
            Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, z);


            fireSound.Play();

            //Bullet
            GameObject currentBullet = Instantiate(bullet, attackPoint.position, Quaternion.identity);
            currentBullet.transform.forward = directionWithSpread.normalized;
            currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootForce, ForceMode.Impulse);
            currentBullet.GetComponent<CustomBullets>().SetDamage(damage);
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    protected override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
    }

    protected override void Death()
    {
        GameManager.instance.SpawnAI(this);
        SetFullHealth();
    }
}
