using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Player : MonoBehaviour {
    private List<TerritoryNode> currentTerritories;
    private TerritoryCard[] currentCards;
    private int heldArmies = 0;
    public Color armyColour = Color.white;
    public bool alive = false;

    private GameObject playerInfoUI;
    // Use this for initialization after game mode is picked
    public void InitializePlayer() {
        playerInfoUI = GameObject.Find(gameObject.name + "info");
        currentTerritories = new List<TerritoryNode>();
        alive = true;
    }

    // Update is called once per frame
    void Update() {
        if (playerInfoUI != null)
        {
            playerInfoUI.GetComponent<Text>().text = (gameObject.name + " armies to place: " + heldArmies.ToString());
        }
    }

    public void AddArmies(int troops) { heldArmies += troops; }
    public int GetArmies() { return heldArmies; }
    public void AddTerritory(TerritoryNode terry) { currentTerritories.Add(terry); }
    public void RemoveTerritory(TerritoryNode terry) { currentTerritories.Remove(terry); }
    public List<TerritoryNode> GetTerritories() { return currentTerritories;}
}
