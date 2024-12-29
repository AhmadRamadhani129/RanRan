using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float chaseRadius = 10f;  
    [SerializeField] private float stopChasingRadius = 15f;
    private bool isChasing = false;
    private Transform player;
    private Transform currentPatrolPoint;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolPoints = new Transform[5];
        for (int i = 1; i <= 5; i++)
        {
            Transform point = GameObject.Find("Point " + i)?.transform;
            if (point != null)
            {
                patrolPoints[i - 1] = point;
            }
            else
            {
                Debug.LogWarning("Point " + i + " not found in the scene!");
            }
        }
        StartCoroutine(Patrol());
    }

    void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, chaseRadius);

        bool playerDetected = false;

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                player = collider.transform; 
                playerDetected = true;
                break;
            }
        }

        if (playerDetected && !isChasing)
        {
            StartChasing();
        }

        if (isChasing && Vector3.Distance(transform.position, player.position) > stopChasingRadius)
        {
            StopChasing();
        }

        if (isChasing)
        {
            agent.SetDestination(player.position);
        }
    }

    private void StartChasing()
    {
        if (!isChasing)
        {
            isChasing = true;
            agent.SetDestination(player.position); 
            StopCoroutine(Patrol());
        }
    }

    private void StopChasing()
    {
        if (isChasing)
        {
            isChasing = false;
            StartCoroutine(Patrol());
        }
    }

    private IEnumerator Patrol()
    {
        while (!isChasing)
        {
            currentPatrolPoint = GetRandomPatrolPoint();

            if (currentPatrolPoint != null)
            {
                agent.SetDestination(currentPatrolPoint.position);
            }

            // Tunggu hingga musuh mencapai titik tujuan atau mulai mengejar
            while (!agent.pathPending && agent.remainingDistance > agent.stoppingDistance && !isChasing)
            {
                yield return null;
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private Transform GetRandomPatrolPoint()
    {
        if (patrolPoints.Length == 0)
        {
            Debug.LogWarning("No patrol points assigned!");
            return null;
        }

        int randomIndex = Random.Range(0, patrolPoints.Length);
        return patrolPoints[randomIndex];
    }


}
