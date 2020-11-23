using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hover : MonoBehaviour
{
    // Making this class a singleton will allow all obstacles to call on the activate function and replace their
    // sprite into the hover instance while the mopuse is down
    private static Hover HoverInstance;

    private GameObject EnemyInstance;
    private SpriteRenderer obstacle;

    public static Hover GetHover()
    {
        
        return HoverInstance;
    }

    private void Awake()
    {
        HoverInstance = this;
    }


    

    // Start is called before the first frame update
    void Start()
    {
        obstacle = this.GetComponent<SpriteRenderer>();   
    }

    // Update is called once per frame
    void Update()
    {
        FollowMouse();
    }

    private void FollowMouse()
    {
        if(obstacle.enabled)
        {
            Vector3 MouseWorldCoord = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(MouseWorldCoord.x, MouseWorldCoord.y, -10);
        }
    }

    public void Activate(Sprite sprite)
    {
        obstacle.sprite = sprite;
        obstacle.enabled = true;
    }

    public void Deactivate()
    {
        obstacle.enabled = false;
    }

    public void SetEnemyIntance(GameObject EnemyInstance)
    {
        this.EnemyInstance = EnemyInstance;
    }

    public void CreateEnemyInstance()
    {
        if(EnemyInstance != null)
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            
            Debug.Log(hit);
            if (Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero))
            {
                Debug.Log("Something Hit");
                Debug.Log(hit.transform.name);
                if (hit.transform.tag == "Map")
                {
                    Debug.Log("Map Hit");
                    Transform trans = hit.transform;
                    trans.SetPositionAndRotation(new Vector3(trans.position.x, trans.position.y, trans.position.z - 5f), trans.rotation);
                    GameObject.Instantiate(EnemyInstance, trans, false);
                    EnemyInstance = null;
                }

            }
            Debug.DrawLine(transform.position, Vector3.forward*100, Color.red, 5.0f);
                                                 
           
        }
        Debug.Log(this.transform.position);
        Debug.DrawLine(this.transform.position, transform.forward*100, Color.red, 5.0f);

    }

    private void OnMouseDown()
    {
        Debug.Log("Clicked");
        CreateEnemyInstance();
    }
    private void OnMouseDrag()
    {
        
    }
}
