using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum WaitZones
{
    StopClose,
    StopFar
}

public class PenguinSpawner : MonoBehaviour
{
    private static Queue<int> penguinSpawnPointsQueue;

    [SerializeField] GameObject penguinPrefab; //do not destroy the prefab
    [SerializeField] Transform[] spawnEndPoints;
    [SerializeField] Transform[] waitZones;

    private int spawnPoint;
    private int waitPoint;
    private int endPoint;

    private void Awake() 
    {
        const int MaxPenguins = 4;
        penguinSpawnPointsQueue = new Queue<int>();

        for (int openSpawnPoint = 0; openSpawnPoint < MaxPenguins; openSpawnPoint++)
            penguinSpawnPointsQueue.Enqueue(openSpawnPoint);

        spawnPoint = 0;
        waitPoint = 0;
        endPoint = 0;
    }

    public bool CanSpawnAPenguin()
    {
        return penguinSpawnPointsQueue.Count > 0;
    }

    public static void DespawnPenguin(GameObject penguin)
    {
        int spawnPointToOpen = penguin.GetComponent<PenguinMovement>().GetSpawnPoint();
        penguinSpawnPointsQueue.Enqueue(spawnPointToOpen);
        Destroy(penguin);
    }

    public void SpawnPenguin()
    {
        DetermineRoute();
        GameObject newPenguin = CreatePenguin();

        SetupMovement(newPenguin);
    }

    private void DetermineRoute()
    {
        DetermineSpawnPoint();
        DetermineWaitZone();
        DetermineEndPoint();
    }

    private void DetermineSpawnPoint()
    {
        spawnPoint = penguinSpawnPointsQueue.Dequeue();
    }

    private void DetermineWaitZone()
    {
        if (spawnPoint <= 1)
            waitPoint = (int) WaitZones.StopClose;
        else
            waitPoint = (int) WaitZones.StopFar;
    }

    private void DetermineEndPoint()
    {
        const int Sides = 2;
        bool spawnPointIsClose = spawnPoint % Sides == 0;
        int iglooRandomizer = Random.Range(0,2);

        if (spawnPointIsClose)
            endPoint = (spawnPoint + Sides + iglooRandomizer) % spawnEndPoints.Length;
        else
            endPoint = (spawnPoint + (Sides-1) + iglooRandomizer) % spawnEndPoints.Length;  
    }

    private void SetupMovement(GameObject newPenguin)
    {
        SetPenguinSpeed(newPenguin);
        SetPenguinRouteKeyPoints(newPenguin);
    }

    private void SetPenguinSpeed(GameObject newPenguin)
    {
        const int setupSpeed = 7;
        const float walkingSpeed = 2.5f;

        NavMeshAgent penguinAgent = newPenguin.GetComponent<NavMeshAgent>();
        penguinAgent.speed = GameManager.Instance.GameIsPreparing() ? setupSpeed : walkingSpeed;
    }

    private void SetPenguinRouteKeyPoints(GameObject newPenguin)
    {
        newPenguin.GetComponent<PenguinMovement>().SetSpawnPoint(spawnPoint);
        newPenguin.GetComponent<PenguinMovement>().SetCurrentTarget(waitZones[waitPoint]);
        newPenguin.GetComponent<PenguinMovement>().SetNextTarget(spawnEndPoints[endPoint]);
    }

    private GameObject CreatePenguin()
    {
        return Instantiate<GameObject>(penguinPrefab, spawnEndPoints[spawnPoint].position, spawnEndPoints[spawnPoint].rotation);
    }   
}
