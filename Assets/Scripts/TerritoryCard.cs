using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TerritoryCard : MonoBehaviour {

	enum ARMY_DENOMINATIONS{WILD, INFANTRY, CAVALRY, ARTILLERY};

	private ARMY_DENOMINATIONS type;
     public void SetArmy(int army) { type = (ARMY_DENOMINATIONS)army; }
     public int GetArmyType() { return (int)type; }

     private TerritoryNode territory;
     public void SetTerritory(TerritoryNode terry) { territory = terry; }
     public TerritoryNode GetTerritory() { return territory; }


     public GameObject[] armySprites;

     public Image sprTerritory;
     public Text territoryNameOb;
     private string territoryName;

     void Start () {

          for(int i = 0; i < 3; i++) {
               armySprites[i].SetActive(false);
          }
     }

     public void InitializeTerritoryCard(TerritoryNode newTerritory, int newArmy) {

          //TerritoryInformation
          SetTerritory(newTerritory);
          GetTerritory().SetColor(newTerritory.GetTerritoryColor());
          territoryName = newTerritory.gameObject.name;
          territoryNameOb.text = territoryName;
          sprTerritory.sprite = newTerritory.GetSpriteRenderer().sprite;
        
        

          switch(newArmy) {

               //INFANTRY
               case 1:
         
                    break;

               //CAVALRY
               case 2:
 
             
                    break;

               //ARTILLERY
               case 3:
        
                    break;

          

          }
     }

}
