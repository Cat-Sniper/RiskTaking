using UnityEngine;
using System.Collections;

public class Continent : MonoBehaviour {
	public int Value = 0;
	public TerritoryNode[] territories;
	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public int CheckBonus(int currentPlayer)
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
        
        if (!differentOwners)
            return Value;
        else
            return 0;
    }
}
