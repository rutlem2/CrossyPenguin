using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VehicleType
{
    FastCar,
    NormalCar,
    SlowCar
}

public class VehicleMovement : MonoBehaviour
{
    [SerializeField] float speed = 15f;

    Rigidbody vehicleRigidbody;
    VehicleSpawner gameManagerSpawner;

    private Transform target;
    private int laneID;

    private void Awake() 
    {
        vehicleRigidbody = GetComponent<Rigidbody>();
        gameManagerSpawner = GameObject.Find("GameManager").GetComponent<VehicleSpawner>();
    }

    public int GetLaneID()
    {
        return laneID;
    }

    private void OnCollisionEnter(Collision other) 
    {
        if (other.gameObject.tag == "Barrier")
            Physics.IgnoreCollision(other.gameObject.GetComponent<Collider>(), gameObject.GetComponent<Collider>());

        if (other.gameObject.name == target.name)
            gameManagerSpawner.DespawnVehicle(gameObject);
    }

    private void FixedUpdate() 
    {
        if (target)
            Drive(target.position);
    }

    public void SetVehicleTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetVehicleLane(int targetID)
    {
        laneID = targetID;
    }

    public void SetSpeedByType(VehicleType type)
    {
        switch (type)
        {
            case VehicleType.FastCar:
                speed = 22f;
                break;
            case VehicleType.NormalCar:
                speed = 17.5f;
                break;
            case VehicleType.SlowCar:
                speed = 15f;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
        ScaleSpeedBurden();
    }

    private void ScaleSpeedBurden()
    {
        switch (GameManager.Instance.GetDifficulty())
        {
            case Difficulty.Easy:
                return;
            case Difficulty.Medium:
                speed += 1;
                break;
            case Difficulty.Hard:
                speed += 2;
                break;
            case Difficulty.Expert:
                speed += 3;
                break;
            default:
                Debug.LogError($"Unknown difficulty~~ #{GameManager.Instance.GetDifficulty()} ~~selected, proceeding with no increased burden.");
                break;
        }
    }

    void Drive(Vector3 target)
    {
        Vector3 movement = transform.forward * speed;
        vehicleRigidbody.MovePosition(transform.position + movement * Time.deltaTime);
    }
}
