using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ActivateEnemiesSlowly : MonoBehaviour
{
    private List<GameObject> enemies = new List<GameObject>();
    [SerializeField] private float activationDelay = 0.5f;

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            enemies.Add(child.gameObject);
            child.gameObject.SetActive(false);
        }
        this.enabled = false;
    }

    private void OnEnable()
    {
        StartCoroutine(ActivateEnemiesCoroutine());
    }

    private IEnumerator ActivateEnemiesCoroutine()
    {
        foreach (GameObject enemy in enemies)
        {
            enemy.SetActive(true);
            yield return new WaitForSeconds(activationDelay);
        }
    }
}
