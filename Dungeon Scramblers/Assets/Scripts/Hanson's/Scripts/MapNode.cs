using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapNode : MonoBehaviour
{
    [SerializeField]
    protected Color hoverColor;

    private SpriteRenderer render;
    private Color startColor;
    // Start is called before the first frame update
    void Start()
    {
        render = GetComponent<SpriteRenderer>();
        startColor = render.material.color;
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnMouseEnter()
    {
        Debug.Log("Hovered");
        render.material.color = hoverColor;
    }

    private void OnMouseExit()
    {
        render.material.color = startColor;
    }

    private void OnMouseDown()
    {
        Debug.Log("Clicked");
        Hover.GetHover().Deactivate();
        Hover.GetHover().CreateEnemyInstacne(transform);
    }
}
