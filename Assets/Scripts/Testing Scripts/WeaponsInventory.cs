using UnityEngine;
using UnityEngine.SceneManagement;

public class WeaponsInventory : MonoBehaviour
{
    public GameObject[] weaponPrefabs;

    private bool oneEnabled = false;
    private int currentIndex = 0;

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void Start()
    {
        var store = WeaponLoadoutStore.Instance;

        if (store != null && store.HasData && SceneManager.GetActiveScene().name != "TrainingCamp")
        {
            foreach (GameObject weaponPrefab in store.OwnedWeaponPrefabs)
            {
                if (weaponPrefab != null)
                {
                    PlayerWeaponManager.instance.SpawnWeapon(weaponPrefab);
                }
            }

            if (PlayerWeaponManager.instance.weaponsSpawned.Count > 0)
            {
                currentIndex = Mathf.Clamp(store.CurrentIndex, 0,
                    PlayerWeaponManager.instance.weaponsSpawned.Count - 1);
                PlayerWeaponManager.instance.ChangeWeapon(
                    PlayerWeaponManager.instance.weaponsSpawned[currentIndex]);
            }
        }
        else
        {
            foreach (GameObject weapon in weaponPrefabs)
            {
                if (weapon != null)
                {
                    PlayerWeaponManager.instance.SpawnWeapon(weapon);  // first-scene default loadout
                }
            }
            
            if (PlayerWeaponManager.instance.weaponsSpawned.Count > 0)
            {
                currentIndex = 0;
                PlayerWeaponManager.instance.ChangeWeapon(
                    PlayerWeaponManager.instance.weaponsSpawned[0]);
            }
        }
    }

    private void Update()
    {
        if (PauseMenuController.Instance?.IsGamePaused == true)
            return;

        if (CutsceneManager.Instance?.IsCutscenePlaying == true)
            return;

        if (GameOverManager.instance?.IsGameOver == true)
            return;

        if (PlayerWeaponManager.instance.weaponsSpawned.Count == 0) return;

        float scrollValue = InputManager.instance.controls.Weapon.ScrollWeapon.ReadValue<Vector2>().y;
        if (scrollValue > 0)
        {
            currentIndex++;
            if (currentIndex >= PlayerWeaponManager.instance.weaponsSpawned.Count) currentIndex = 0;
            PlayerWeaponManager.instance.ChangeWeapon(PlayerWeaponManager.instance.weaponsSpawned[currentIndex]);
        }
        else if (scrollValue < 0)
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = PlayerWeaponManager.instance.weaponsSpawned.Count - 1;
            PlayerWeaponManager.instance.ChangeWeapon(PlayerWeaponManager.instance.weaponsSpawned[currentIndex]);
        }

        if (InputManager.instance.controls.Weapon.SelectWeapon.triggered)
        {
            int selectedIndex = Mathf.RoundToInt(InputManager.instance.controls.Weapon.SelectWeapon.ReadValue<float>()) - 1;
            if (selectedIndex >= 0 && selectedIndex < PlayerWeaponManager.instance.weaponsSpawned.Count)
            {
                currentIndex = selectedIndex;
                PlayerWeaponManager.instance.ChangeWeapon(PlayerWeaponManager.instance.weaponsSpawned[currentIndex]);
            }
        }
    }
}
