using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BoobaAI : MonoBehaviour
{
    NavMeshAgent navBooba;
    Rigidbody rb;

    public Transform player;
    public FPSController fpsController;

    public float boobaRotSpeed;

    RaycastHit hit;

    public Vector3 playerPos;
    bool seePlayer;

    public float disPlayer;

    bool canAttack;
    public bool isAttacking;
    bool hasAttacked;
    public bool canJump;

    public float boobaJumpForce;
    public float boobaJumpForwardForce;
    public float boobaBackForce;
    public float drag;

    public int damage;

    float startTime;
    public float waitForSec;

    public LayerMask layerGround;
    RaycastHit groundHit;
    public float groundDistance;

    public bool grounded;

    public ParticleSystem splash;
    public ParticleSystem gore;
    void Awake()
    {
        navBooba = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        fpsController = GameObject.FindGameObjectWithTag("Player").GetComponent<FPSController>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        FieldOfView();
        FollowPlayer();
        AttackPlayer();
        BoobaRaycast();
    }

    void FollowPlayer()
    {
        if(!seePlayer && !isAttacking || disPlayer > 6 && !isAttacking)
        {
            navBooba.SetDestination(playerPos);
            navBooba.stoppingDistance = 5;

            Vector3 lookrotation = navBooba.steeringTarget - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookrotation), boobaRotSpeed * Time.deltaTime);
        }
    }

    void AttackPlayer()
    {
        if(seePlayer && disPlayer < 6 && !isAttacking)
        {
            canAttack = true;
        }
        else
        {
            canAttack = false;
        }

        if(canJump)
        {
            rb.drag = drag;
            rb.AddForce(-transform.forward * boobaBackForce, ForceMode.Force);

            if(Time.time - startTime > waitForSec)
            {
                isAttacking = false;
                hasAttacked = false;
                canJump = false;

                rb.drag = 0;
                navBooba.enabled = true;
            }
        }

        if(canAttack)
        {
            if(disPlayer > 2  && !isAttacking)
            {
                navBooba.SetDestination(playerPos);
                navBooba.stoppingDistance = 2;

                Vector3 lookrotation = navBooba.steeringTarget - transform.position;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookrotation), boobaRotSpeed * Time.deltaTime);
            }

            if(disPlayer < 2 && !isAttacking)
            {
                isAttacking = true;

                navBooba.enabled = false;


                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(transform.up * boobaJumpForce, ForceMode.Impulse);
                rb.AddForce(transform.forward * boobaJumpForwardForce, ForceMode.Impulse);

                startTime = Time.time;
            }
        }

    }

    void FieldOfView()
    {
        playerPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        disPlayer = Vector3.Distance(transform.position, playerPos);

        Vector3 dirToPlayer = (player.position - transform.position).normalized;

        Debug.DrawRay(transform.position + transform.forward * 0.3f, dirToPlayer * disPlayer, Color.red);
        if(Physics.Raycast(transform.position + transform.forward * 0.3f, dirToPlayer, out hit, disPlayer))
        {
            if(hit.transform.tag == "Player")
            {
                seePlayer = true;
            }
            else
            {
                seePlayer = false;
            }
        }

        if(seePlayer)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(playerPos - transform.position), boobaRotSpeed * Time.deltaTime);
        }
    }

    void BoobaRaycast()
    {
        Debug.DrawRay(transform.position, -transform.up  * groundDistance);
        if(Physics.Raycast(transform.position, -transform.up, out groundHit, groundDistance, layerGround))
        {
            grounded = true;
        }
        else
        {
            grounded = false;
        }

        if(hasAttacked && grounded)
        {
            canJump = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "Player" && isAttacking && !hasAttacked)
        {
            fpsController.playerHealth -= damage;
            hasAttacked = true;
        }
        else if(collision.transform.tag == "Ground" && isAttacking && !hasAttacked)
        {
            hasAttacked = true;
        }
    }

    private void OnDestroy()
    {
        var particle = Instantiate(splash, transform.position, Quaternion.identity);
        particle.Play();
        Destroy(particle.gameObject, 1f);

        
    }
}
