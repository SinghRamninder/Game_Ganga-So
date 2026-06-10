using UnityEngine;
using System.Collections;

public class DoorTrigger : MonoBehaviour
{
    public Animator doorAnimator;

    private int playersInside = 0;
    private Coroutine closeCoroutine;

    void Start()
    {
        // Force door to start closed
        doorAnimator.SetBool("isOpen", false);
        Debug.Log("Door initialized: CLOSED");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("ENTER detected: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("PLAYER ENTERED ✅");

            playersInside++;

            // Cancel closing if player re-enters
            if (closeCoroutine != null)
            {
                StopCoroutine(closeCoroutine);
            }

            doorAnimator.SetBool("isOpen", true);
            Debug.Log("Door OPENING");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("EXIT detected: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("PLAYER EXITED ❌");

            playersInside--;

            if (playersInside <= 0)
            {
                closeCoroutine = StartCoroutine(CloseDoorAfterDelay());
            }
        }
    }

    IEnumerator CloseDoorAfterDelay()
    {
        Debug.Log("Waiting to close door...");
        yield return new WaitForSeconds(2f);

        if (playersInside <= 0)
        {
            doorAnimator.SetBool("isOpen", false);
            Debug.Log("Door CLOSING");
        }
        else
        {
            Debug.Log("Player re-entered, door stays open");
        }
    }
}