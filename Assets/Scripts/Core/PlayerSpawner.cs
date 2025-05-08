using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject PlayerPrefab_;

    void Start()
    {
        if (PlayerController.Instance == null)
        {
            GameObject Player_ = Instantiate(PlayerPrefab_, transform.position, transform.rotation);
        }
    }
}

// © 2025 KOIYOT. All rights reserved.