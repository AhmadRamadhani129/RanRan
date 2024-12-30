using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float chaseRadius = 10f;  // Radius pengejaran
    [SerializeField] private float stopChasingRadius = 15f; // Radius berhenti mengejar
    private bool isChasing = false;
    private Transform player;
    private Transform currentPatrolPoint;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Tentukan titik patrol yang ada
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

        // Mulai patrol
        StartCoroutine(Patrol());
    }

    void Update()
    {
        // Cek keberadaan pemain dalam radius pengejaran
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

        // Jika pemain terdeteksi dan musuh belum mengejar
        if (playerDetected && !isChasing)
        {
            StartChasing();
        }

        // Jika musuh sedang mengejar pemain
        if (isChasing)
        {
            // Periksa jika pemain keluar dari radius berhenti mengejar
            if (player == null || Vector3.Distance(transform.position, player.position) > stopChasingRadius)
            {
                StopChasing();
            }
            else
            {
                agent.SetDestination(player.position); // Tetap mengejar pemain
            }
        }
    }

    private void StartChasing()
    {
        if (!isChasing)
        {
            isChasing = true;

            // Periksa apakah player masih valid sebelum menetapkan tujuan
            if (player != null)
            {
                agent.SetDestination(player.position);
            }

            // Hentikan coroutine patrol saat mulai mengejar
            StopAllCoroutines();
        }
    }

    private void StopChasing()
    {
        if (isChasing)
        {
            isChasing = false;
            Debug.Log("Player out of range or destroyed. Returning to patrol.");

            // Hentikan pengejaran dan mulai patrol lagi
            StopAllCoroutines();
            StartCoroutine(Patrol());
        }
    }

    private IEnumerator Patrol()
    {
        while (!isChasing)
        {
            // Pilih titik patrol secara acak
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

            // Beri sedikit waktu sebelum bergerak ke titik patrol berikutnya
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

    // Debug untuk melihat radius pengejaran
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopChasingRadius);
    }
}
