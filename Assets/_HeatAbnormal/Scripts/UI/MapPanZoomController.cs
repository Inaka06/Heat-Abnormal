using UnityEngine;
using UnityEngine.EventSystems;

public sealed class MapPanZoomController : MonoBehaviour, IScrollHandler, IBeginDragHandler, IDragHandler
{
    [SerializeField] private float minZoom = 1f;
    [SerializeField] private float maxZoom = 2.4f;
    [SerializeField] private float zoomStep = 0.15f;
    private RectTransform rectTransform;
    private Vector2 dragStartPosition;
    private Vector2 anchoredStartPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnScroll(PointerEventData eventData)
    {
        var nextScale = Mathf.Clamp(rectTransform.localScale.x + eventData.scrollDelta.y * zoomStep / 10f, minZoom, maxZoom);
        rectTransform.localScale = Vector3.one * nextScale;
        ClampPosition();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragStartPosition = eventData.position;
        anchoredStartPosition = rectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition = anchoredStartPosition + eventData.position - dragStartPosition;
        ClampPosition();
    }

    private void ClampPosition()
    {
        var canvas = rectTransform.parent as RectTransform;
        if (canvas == null) return;
        var scaledWidth = rectTransform.rect.width * rectTransform.localScale.x;
        var scaledHeight = rectTransform.rect.height * rectTransform.localScale.y;
        var maxX = Mathf.Max(0f, (scaledWidth - canvas.rect.width) * .5f);
        var maxY = Mathf.Max(0f, (scaledHeight - canvas.rect.height) * .5f);
        var position = rectTransform.anchoredPosition;
        position.x = Mathf.Clamp(position.x, -maxX, maxX);
        position.y = Mathf.Clamp(position.y, -maxY, maxY);
        rectTransform.anchoredPosition = position;
    }
}
