using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragObstacle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Image image;

    [SerializeField]
    private GameObject EnemyInstance;

    public void Awake()
    {
        image = GetComponent<Image>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin Dragging");
        Hover.GetHover().Activate(image.sprite);
        Hover.GetHover().SetEnemyIntance(EnemyInstance);

    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Dragging");
        Hover.GetHover().FollowMouse();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End Dragging");
        Hover.GetHover().Deactivate();
        Hover.GetHover().CreateEnemyInstance();
    }

    
    
}
