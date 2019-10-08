/*\
 *   DeckOfTerritoryCards.cs: 
 *   Acts as a warehouse for all cards in the game. Initializes and Shuffles the cards at the start of the game. There are 45 cards
 *   in total, one for each territory + 2 wild cards.
\*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckOfTerritoryCards : MonoBehaviour {

    [SerializeField] private GameObject[] cards;
    [SerializeField] private GameObject CardPrefab;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void GenerateCards(TerritoryNode[] allTerritories) {

        int armyValue = 1;

        for(int i = 0; i < allTerritories.Length + 2; i++) {

            TerritoryCard cardScript = cards[i].GetComponent<TerritoryCard>();

            if (i < allTerritories.Length) {

                cardScript.InitializeTerritoryCard(allTerritories[i], armyValue);
                armyValue++;

            } else {

            }

            if(armyValue > 3) {
                
            }
        }
    }
}
