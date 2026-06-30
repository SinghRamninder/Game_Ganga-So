using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyCardUIManager : MonoBehaviour
{
    public static KeyCardUIManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private RectTransform uiRoot;
    [SerializeField] private Image keyCardImagePrefab;

    [Header("Animation Settings")]
    [SerializeField] private float moveDuration = 0.7f;
    [SerializeField] private Vector2 bigSize = new Vector2(180f, 180f);
    [SerializeField] private Vector2 smallSize = new Vector2(80f, 80f);

    [Header("Bottom Left Slot Settings")]
    [SerializeField] private Vector2 firstSlotPosition = new Vector2(80f, 80f);
    [SerializeField] private float slotSpacingX = 95f;

    private readonly List<Image> collectedKeyCards = new List<Image>();
    private readonly HashSet<string> shownKeyCards = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (uiRoot == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();

            if (canvas != null)
            {
                uiRoot = canvas.GetComponent<RectTransform>();
            }
            else
            {
                Debug.LogError("KeyCardUIManager cannot find Canvas. Put it inside Canvas or assign uiRoot manually.");
            }
        }
    }

    public void ShowKeyCard(string keyCardID, Sprite keyCardSprite)
    {
        if (uiRoot == null)
        {
            Debug.LogError("UI Root is missing on KeyCardUIManager.");
            return;
        }

        if (keyCardImagePrefab == null)
        {
            Debug.LogError("Key Card Image Prefab is missing on KeyCardUIManager.");
            return;
        }

        if (string.IsNullOrEmpty(keyCardID))
        {
            Debug.LogWarning("KeyCard ID is missing.");
            return;
        }

        if (keyCardSprite == null)
        {
            Debug.LogWarning("KeyCard sprite is missing for: " + keyCardID);
            return;
        }

        if (shownKeyCards.Contains(keyCardID))
        {
            return;
        }

        shownKeyCards.Add(keyCardID);

        Image newKeyCardImage = Instantiate(keyCardImagePrefab, uiRoot);
        newKeyCardImage.sprite = keyCardSprite;
        newKeyCardImage.gameObject.SetActive(true);

        RectTransform rectTransform = newKeyCardImage.rectTransform;

        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(0f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        Vector2 startPosition = GetScreenCenterPosition();
        Vector2 targetPosition = GetNextSlotPosition();

        rectTransform.anchoredPosition = startPosition;
        rectTransform.sizeDelta = bigSize;

        collectedKeyCards.Add(newKeyCardImage);

        StartCoroutine(AnimateKeyCard(rectTransform, startPosition, targetPosition));
    }

    private Vector2 GetScreenCenterPosition()
    {
        return new Vector2(
            uiRoot.rect.width * 0.5f,
            uiRoot.rect.height * 0.5f
        );
    }

    private Vector2 GetNextSlotPosition()
    {
        int index = collectedKeyCards.Count;

        return new Vector2(
            firstSlotPosition.x + index * slotSpacingX,
            firstSlotPosition.y
        );
    }

    private IEnumerator AnimateKeyCard(RectTransform rectTransform, Vector2 startPosition, Vector2 targetPosition)
    {
        float timer = 0f;

        while (timer < moveDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / moveDuration;
            t = Mathf.Clamp01(t);
            t = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            rectTransform.sizeDelta = Vector2.Lerp(bigSize, smallSize, t);

            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
        rectTransform.sizeDelta = smallSize;
    }
}