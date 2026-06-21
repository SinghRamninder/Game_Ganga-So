using System.Collections.Generic;
using UnityEngine;

public class WeaponLoadoutStore : MonoBehaviour
{
    public static WeaponLoadoutStore Instance { get; private set; }

    public bool HasData { get; private set; }
    public List<GameObject> OwnedWeaponPrefabs { get; private set; } = new();
    public int CurrentIndex { get; private set; }

    // Optional: per-weapon ammo. See note at the bottom.
    // public List<int> AmmoCounts { get; private set; } = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);          // duplicate from a later scene — discard it
            return;
        }

        Instance = this;
        transform.SetParent(null);        // DontDestroyOnLoad requires a root object
        DontDestroyOnLoad(gameObject);
    }

    public void Save(List<GameObject> ownedPrefabs, int currentIndex)
    {
        OwnedWeaponPrefabs = new List<GameObject>(ownedPrefabs);  // copy, don't alias
        CurrentIndex = currentIndex;
        HasData = true;
    }

    public void Clear()
    {
        OwnedWeaponPrefabs.Clear();
        CurrentIndex = 0;
        HasData = false;
    }
}

