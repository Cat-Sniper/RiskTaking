using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Continent : MonoBehaviour {
	public int Value = 0;
	public TerritoryNode[] territories;
    public Image bonusDisplay;
    public RiskGameMaster manager;

	// Use this for initialization
	void Start () {
	    foreach (TerritoryNode territory in territories){
            territory.SetContinent(this);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public bool CheckBonus(int currentPlayer)
    {
        int owner = currentPlayer;          //first territory belonging to the continent is set as comparison
        bool differentOwners = false;                       //and as soon as a territory with a different owner appears, we don't have to check anymore.
        foreach (TerritoryNode terry in territories)
        {
            int ownerCheck = terry.DisplayOwner();          
            if(owner != ownerCheck)
            {
                differentOwners = true;
                break;                          
            } 
        }
        Debug.Log(gameObject.name + " is owned by current player: " + !differentOwners);
        if (!differentOwners)
            return true;
        else {
            return false;
        }
    }

    public void UpdateBorderColour(Color col) {
        bonusDisplay.color = col;
    }
    public void ResetBorderColour() {
        bonusDisplay.color = Color.white;
    }
}
