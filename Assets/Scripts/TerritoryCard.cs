using UnityEngine;
using System.Collections;

public class TerritoryCard : MonoBehaviour {
	enum ARMY_DENOMINATIONS{WILD, INFANTRY, CAVALRY, ARTILLERY};

	private ARMY_DENOMINATIONS type;
    public void SetArmy(int army) { type = (ARMY_DENOMINATIONS)army; }
    public int GetArmyType() { return (int)type; }

    private TerritoryNode territory;
    public void SetTerritory(TerritoryNode terry) { territory = terry; }
    public TerritoryNode GetTerritory() { return territory; }

    private Sprite sprterritory;
    private Sprite sprArmy;
    private string TerritoryName;
}
