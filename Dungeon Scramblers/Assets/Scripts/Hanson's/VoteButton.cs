using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VoteButton : MonoBehaviour
{
    [SerializeField]
    private Perk perk;

    public Perk getPerk() { return perk; }

    TextMeshPro VoteCounter;
    
    int vote = 0;

    public int getVote() { return vote; }

    // Start is called before the first frame update
    void Start()
    {
        GameObject child = transform.GetChild(0).gameObject;
        if (child)
        {
            VoteCounter = child.GetComponent<TextMeshPro>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        VoteCounter.text = (++vote).ToString();
        Debug.Log(vote);
    }
}
