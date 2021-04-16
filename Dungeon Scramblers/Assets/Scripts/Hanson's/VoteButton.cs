using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VoteButton : MonoBehaviour
{
    [SerializeField]
    private Perk perk;

    public Perk getPerk() { return perk; }
    public void setPerk(Perk inPerk) { perk = inPerk; }

    [SerializeField]
    TextMeshPro VoteCounter;
    
    int vote = 0;

    public int getVote() { return vote; }

    // Start is called before the first frame update
    void Awake()
    {
        GameManager.ManagerInstance.IncrementButton(this);
        perk = GameManager.ManagerInstance.GetPerk();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<Scrambler>())
        {
            VoteCounter.text = (++vote).ToString();
        }
       
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.GetComponent<Scrambler>())
        {
            VoteCounter.text = (--vote).ToString();
        }
    }
}
