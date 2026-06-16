using UnityEngine;
using System.Collections.Generic;
using System;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    public event Action ResetArea;

    [SerializeField] private GameObject player;

    [HideInInspector] public Transform spawnPosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<SimplePlayerController>().gameObject;
        }

        spawnPosition = player.transform;
    }

    public void SpawnAtCheckpoint()
    {
        PlayerHealthManager.instance.Heal(100f);
        foreach (GameObject current in PlayerWeaponManager.instance.weaponsSpawned)
        {
            current.GetComponent<Weapon>().CheckpointSpawn();
        }

        ResetArea?.Invoke();
        player.transform.position = spawnPosition.position;
    }
}
