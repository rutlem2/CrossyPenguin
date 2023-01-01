using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    private const int MaxLanes = 4;
    private const float VehicleStaggerDelay = 0.6f;
    private const float CoroutineDurationSufficientForSpawningAllVehicles = 1.4f;

    private static bool spawnerReady = true;
    private static Queue<int> vehicleSpawnPointsQueue;

    private int[,] startingSeeds;
    private int[] vehiclesPerLane;
    [SerializeField] GameObject[] fastVehicles;
    [SerializeField] GameObject[] normalVehicles;
    [SerializeField] GameObject[] slowVehicles;

    [SerializeField] Transform[] spawnEndPoints;
    Transform target;

    private int vehicleID;
    private int spawnPoint;
    private int endPoint;

    GameObject newVehicle;
    VehicleType newVehicleType;

    private void Awake() 
    {
        startingSeeds = new int[,]
        {
            {4, 2, 7, 1},
            {0, 1, 6, 3},
            {5, 2, 0, 3},
            {7, 5, 2, 0},
            {2, 4, 3, 5}
        };
        vehicleSpawnPointsQueue = new Queue<int>();
        vehiclesPerLane = new int[MaxLanes];
    }

    private void Start()
    {
        for (int col = 0, seed = Random.Range(0,5); col < MaxLanes; col++)
            vehicleSpawnPointsQueue.Enqueue(startingSeeds[seed, col]); //must be called in Start() to allow for reloading game
    }

    public static void ResetSpawnerForReloadGame()
    {
        spawnerReady = true;
        vehicleSpawnPointsQueue.Clear();
    }

    public void DespawnVehicle(GameObject vehicle)
    {
        int spawnLaneToOpen = vehicle.GetComponent<VehicleMovement>().GetLaneID();
        vehiclesPerLane[spawnLaneToOpen] -= 1;
        if (LaneIsClear(spawnLaneToOpen))
            vehicleSpawnPointsQueue.Enqueue(spawnLaneToOpen);

        Destroy(vehicle);
    }

    private bool LaneIsClear(int lane)
    {
        return vehiclesPerLane[lane] == 0;
    }
    
    public bool CanSpawnAVehicle()
    {
        return spawnerReady && vehicleSpawnPointsQueue.Count > 0;
    }

    public IEnumerator SpawnVehicle()
    {
        spawnerReady = false; //using a lock to minimize calls to SpawnVehicle()
        while (VehiclesInQueue())
        {
            GenerateVehicle();
            GenerateExtraTraffic();
            PlayPassBySound();
            yield return new WaitForSeconds(CoroutineDurationSufficientForSpawningAllVehicles);
        }
        spawnerReady = true;
    }

    private bool VehiclesInQueue()
    {
        return vehicleSpawnPointsQueue.Count > 0;
    }

    private void GenerateVehicle()
    {
        SetNextSpawnPoint();
        CreateVehicleForDifficulty();
        SetupVehicle();
    }

    private void SetNextSpawnPoint()
    {
        spawnPoint = RandomizeSide(vehicleSpawnPointsQueue.Dequeue());
    }

    private int RandomizeSide(int rawSpawnPoint)
    {
        const int NoShift = 0;
        const int ShiftSide = 4;
        return Random.Range(0,2) == 0 ? (rawSpawnPoint + NoShift) % spawnEndPoints.Length : (rawSpawnPoint + ShiftSide) % spawnEndPoints.Length;
    }

    private void CreateVehicleForDifficulty()
    {
        switch (GameManager.Instance.GetDifficulty())
        {
            case Difficulty.Easy:
                DoEasyVehicleSpawning();
                break;
            case Difficulty.Medium:
                DoMediumVehicleSpawning();
                break;
            case Difficulty.Hard:
                DoHardVehicleSpawning();
                break;
            case Difficulty.Expert:
                DoExpertVehicleSpawning();
                break;
            default:
                break;
        }
    }

    private void DoEasyVehicleSpawning()
    {
        const int normalGroupProbabilityMax = 5;
        bool hasNormalVehicleProbability = Random.Range(0, 10) < normalGroupProbabilityMax;
        if (hasNormalVehicleProbability)
            CreateNormalVehicle();
        else
            CreateSlowVehicle();
    }

    private void DoMediumVehicleSpawning()
    {
        const int normalGroupProbabilityMax = 8;
        bool hasNormalVehicleProbability = Random.Range(0, 10) < normalGroupProbabilityMax;
        if (hasNormalVehicleProbability)
            CreateNormalVehicle();
        else
            CreateSlowVehicle();
    }

    private void DoHardVehicleSpawning()
    {
        const int fastGroupProbabilityMax = 4;
        const int normalGroupProbabilityMax = 7;
        int probabilitySample = Random.Range(0, 10);
        if (probabilitySample < fastGroupProbabilityMax)
            CreateFastVehicle();
        else if (probabilitySample < normalGroupProbabilityMax)
            CreateNormalVehicle();
        else
            CreateSlowVehicle();
    }

    private void DoExpertVehicleSpawning()
    {
        const int fastGroupProbabilityMax = 15;
        const int normalGroupProbabilityMax = 18;
        int probabilitySample = Random.Range(0, 20);
        if (probabilitySample < fastGroupProbabilityMax)
            CreateFastVehicle();
        else if (probabilitySample < normalGroupProbabilityMax)
            CreateNormalVehicle();
        else
            CreateSlowVehicle();
    }

    private void CreateFastVehicle()
    {
        vehicleID = Random.Range(0, fastVehicles.Length);
        newVehicle = Instantiate<GameObject>(fastVehicles[vehicleID], spawnEndPoints[spawnPoint].position, spawnEndPoints[spawnPoint].rotation); 
        newVehicleType = VehicleType.FastCar; 
    }

    private void CreateNormalVehicle()
    {
        vehicleID = Random.Range(0, normalVehicles.Length);
        newVehicle = Instantiate<GameObject>(normalVehicles[vehicleID], spawnEndPoints[spawnPoint].position, spawnEndPoints[spawnPoint].rotation);
        newVehicleType = VehicleType.NormalCar; 
    }

    private void CreateSlowVehicle()
    {
        vehicleID = Random.Range(0, slowVehicles.Length);
        newVehicle = Instantiate<GameObject>(slowVehicles[vehicleID], spawnEndPoints[spawnPoint].position, spawnEndPoints[spawnPoint].rotation);
        newVehicleType = VehicleType.SlowCar; 
    }

    private void SetupVehicle()
    {
        int spawnLaneID = spawnPoint % 4;
        vehiclesPerLane[spawnLaneID] += 1;
        target = spawnEndPoints[(spawnPoint + 4) % 8];
        newVehicle.transform.rotation = Quaternion.LookRotation(target.position - newVehicle.transform.position);

        newVehicle.GetComponent<VehicleMovement>().SetVehicleTarget(target);
        newVehicle.GetComponent<VehicleMovement>().SetVehicleLane(spawnLaneID);
        newVehicle.GetComponent<VehicleMovement>().SetSpeedByType(newVehicleType);
    }

    private void GenerateExtraTraffic()
    {
        int rollForExtraTraffic = Random.Range(0, 20);
        int extraVehicles = 0;
        switch (GameManager.Instance.GetDifficulty())
        {
            case Difficulty.Easy:
                return;
            case Difficulty.Medium:
                if (rollForExtraTraffic < 12)
                    return;
                else
                    extraVehicles = 1;
                break;
            case Difficulty.Hard:
                if (rollForExtraTraffic < 5)
                    return;
                else if (rollForExtraTraffic < 15)
                    extraVehicles = 1;
                else
                    extraVehicles = 2;
                break;
            case Difficulty.Expert:
                if (rollForExtraTraffic < 2)
                    return;
                else if (rollForExtraTraffic < 8)
                    extraVehicles = 1;
                else
                    extraVehicles = 2;
                break;
            default:
                break;
        }

        StartCoroutine(CreateExtraVehicles(extraVehicles));
    }

    private IEnumerator CreateExtraVehicles(int extraVehiclesCount)
    {
        for (int i = 0; i < extraVehiclesCount; i++)
        {
            yield return new WaitForSeconds(VehicleStaggerDelay);
            CreateExtraVehicleForType();
            SetupVehicle();
        }
    }

    private void CreateExtraVehicleForType()
    {
        if (newVehicleType == VehicleType.SlowCar)
            CreateSlowVehicle();
        else if (newVehicleType == VehicleType.NormalCar)
            CreateNormalVehicle();
        else
            CreateFastVehicle();
    }

    private void PlayPassBySound()
    {
        if (!AudioManager.Instance.IsSongPlaying(AudioManager.VehiclePassBy))
            AudioManager.Instance.Play(AudioManager.VehiclePassBy);
    }
}
