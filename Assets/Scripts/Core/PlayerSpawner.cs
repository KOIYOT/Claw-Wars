using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject PlayerPrefab_;

    void Start()
    {
        GameObject Player_ = Instantiate(PlayerPrefab_, transform.position, transform.rotation);
        Player_.tag = "Player";
    }
}

// © 2025 KOIYOT. All rights reserved.