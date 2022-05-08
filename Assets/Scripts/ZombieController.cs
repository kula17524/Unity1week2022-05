using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    Animator animator;
    NavMeshAgent agent;

    // 移動スピード
    public float walkingSpeed = 1f;
    public float runSpeed = 1.3f;

    // 状態
    enum STATE {IDLE, WANDER, ATTACK, CHASE, DEAD};
    STATE state = STATE.IDLE;

    GameObject target;

    public int attackDamage;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        // プレイヤーをターゲットとする
        if(target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player");
        }
    }

    // モーションを止める関数
    public void TurnOffTrigger()
    {
        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);
        animator.SetBool("Attack", false);
        animator.SetBool("Death", false);
    }

    // float型のプレイヤーとゾンビの距離を返す関数
    float DistanceToPlayer()
    {
        if (GameState.GameOver)
        {
            return Mathf.Infinity;
        }
        return Vector3.Distance(target.transform.position, transform.position);
    }

    // プレイヤーが近くにいると発見
    bool canSeePlayer()
    {
        if (DistanceToPlayer() < 15)
        {
            return true;
        }

        return false;
    }

    // プレイヤーが離れると見失う
    bool ForgetPlayer()
    {
        if (DistanceToPlayer() > 20)
        {
            return true;
        }

        return false;
    }

    public void DamagePlayer()
    {
        if (target != null)
        {
            target.GetComponent<FPSController>().TakeHit(attackDamage);
        }
    }

    public void ZombieDeath()
    {
        TurnOffTrigger();
        animator.SetBool("Death", true);
        state = STATE.DEAD;
        Destroy(gameObject, 3f);
    }

    // Update is called once per frame
    void Update()
    {
        switch(state)
        {
            case STATE.IDLE:
                TurnOffTrigger();

                if (canSeePlayer())
                {
                    state = STATE.CHASE;
                }
                else if (Random.Range(0, 5000) < 5)
                {
                    state = STATE.WANDER;
                }
                break;
            
            case STATE.WANDER:
                // 目的地があるかどうか
                if(!agent.hasPath)
                {
                    // 徘徊
                    float newX = transform.position.x + Random.Range(-5, 5);
                    float newZ = transform.position.z + Random.Range(-5, 5);

                    Vector3 NextPos = new Vector3(newX, transform.position.y, newZ);
                    agent.SetDestination(NextPos);
                    agent.stoppingDistance = 0;

                    TurnOffTrigger();

                    agent.speed = walkingSpeed;
                    animator.SetBool("Walk", true);
                }

                if (Random.Range(0, 5000) < 5)
                {
                    state = STATE.IDLE;
                    agent.ResetPath();
                }

                if (canSeePlayer())
                {
                    state = STATE.CHASE;
                }
                break;

            case STATE.CHASE:

                if (GameState.GameOver)
                {
                    TurnOffTrigger();
                    agent.ResetPath();
                    state = STATE.WANDER;

                    return;
                }
                agent.SetDestination(target.transform.position);
                agent.stoppingDistance = 2;

                TurnOffTrigger();

                agent.speed = runSpeed;
                animator.SetBool("Run", true);

                if(agent.remainingDistance <= agent.stoppingDistance)
                {
                    state = STATE.ATTACK;
                }

                if (ForgetPlayer())
                {
                    agent.ResetPath();
                    state = STATE.WANDER;
                }

                break;

            case STATE.ATTACK:

                if (GameState.GameOver)
                {
                    TurnOffTrigger();
                    agent.ResetPath();
                    state = STATE.WANDER;

                    return;
                }
                TurnOffTrigger();
                animator.SetBool("Attack", true);

                transform.LookAt(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z));

                if(DistanceToPlayer() > agent.stoppingDistance + 2)
                {
                    state = STATE.CHASE;
                }

                break;
            
            case STATE.DEAD:
                Destroy(agent);
                break;
        }
    }
}
