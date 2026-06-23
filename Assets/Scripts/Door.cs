using UnityEngine;
using System.Collections;
using FMODUnity;

public class Door : MonoBehaviour
{
    [Header("Key Card")]
    [SerializeField] private string KeyCardName;

    [Header("Other References")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private GameObject indicator;
    [SerializeField] private GameObject instructionKey;
    [SerializeField] private GameObject highlighter;

    [Header("FMOD")]
    public EventReference unlockEventFmod;
    public EventReference deniedEventFmod;

    private int playersInside = 0;
    private Coroutine closeCoroutine;
    private Camera mainCamera;

    private bool canUnlock = false;
    private bool isUnlocked = false;

    private Renderer _rend;
    private Material _material;

    private void Awake()
    {
        _rend = indicator.GetComponent<Renderer>();
        _material = _rend.material;
    }

    void Start()
    {
        if (doorAnimator == null)
        {
            doorAnimator = GetComponentInChildren<Animator>();
        }

        if (doorAnimator != null)
            doorAnimator.SetBool("isOpen", false);

        if (instructionKey != null)
            instructionKey.SetActive(false);

        if (highlighter != null)
            highlighter.SetActive(false);

        GetComponent<BoxCollider>().enabled = false;

        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!isUnlocked)
        {
            if (canUnlock && InputManager.instance.controls.Player.Interaction.WasPressedThisFrame() && InventoryManager.Instance != null)
            {
                foreach(string currentKeyName in InventoryManager.Instance.keyCards)
                {
                    if (currentKeyName == KeyCardName)
                    {
                        GetComponent<BoxCollider>().enabled = true;
                        _material.SetColor("_EmissionColor", Color.green * 2f);
                        highlighter.SetActive(false);
                        instructionKey.SetActive(false);

                        isUnlocked = true;

                        if (!unlockEventFmod.IsNull)
                        {
                            RuntimeManager.PlayOneShotAttached(unlockEventFmod, gameObject);
                        }
                        break;
                    }
                }

                if (!isUnlocked)
                {
                    Debug.LogWarning($"[{Time.frameCount}] Required Key card not found on {gameObject.name} (id {gameObject.GetInstanceID()})");

                    if (!deniedEventFmod.IsNull)
                    {
                        RuntimeManager.PlayOneShotAttached(deniedEventFmod, gameObject);
                    }
                }
                else
                {
                    return;
                }
            }

            LayerMask mask = ~LayerMask.GetMask("Player", "Weapon");

            if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out RaycastHit hit, 20f, mask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.CompareTag("Card Reader"))
                {
                    canUnlock = true;
                    highlighter.SetActive(true);
                    instructionKey.SetActive(true);
                }
                else
                {
                    canUnlock = false;
                    highlighter.SetActive(false);
                    instructionKey.SetActive(false);
                }
            }
            else
            {
                canUnlock = false;
                highlighter.SetActive(false);
                instructionKey.SetActive(false);
            }
        }
    }

    private void LateUpdate()
    {
        if (instructionKey.activeSelf)
        {
            Vector3 lookDirection = instructionKey.transform.position - mainCamera.transform.position;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                instructionKey.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInside++;

            if (closeCoroutine != null)
            {
                StopCoroutine(closeCoroutine);
            }

            if (doorAnimator != null)
                doorAnimator.SetBool("isOpen", true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInside--;

            if (playersInside <= 0)
            {
                closeCoroutine = StartCoroutine(CloseDoorAfterDelay());
            }
        }
    }

    IEnumerator CloseDoorAfterDelay()
    {
        yield return new WaitForSeconds(2f);

        if (playersInside <= 0)
        {
            if (doorAnimator != null)
                doorAnimator.SetBool("isOpen", false);
        }
    }
}
