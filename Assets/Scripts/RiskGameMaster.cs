/// TODO in order of priority:
///  - Use Conquer Territory() where applicable: function is complete, just need to implement it
///  - Game Over Phase: What happens when we have a winner?
///  - Each Territory should handle itself better: setting a new owner should tell the territory that all it's attributes need to reflect the new ownership

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class RiskGameMaster : MonoBehaviour {
     
     enum TURN_PHASE {RECRUIT,ATTACK,FORTIFY};
     enum ARMY_DENOMINATIONS {INFANTRY, CAVALRY, ARTILLERY};
     private bool setup = true;
     private bool optionsSelected = false;
     private bool allTsClaimed = false;
     private bool didOnce = false;
     private bool fortified = false;
     private bool territoryConquered = false;
     private bool explosionInQueue = false;

     public Continent[] continents;
     private TerritoryNode[] allTerritories;
     private TerritoryCard[] allCards;
     private TerritoryNode currentTerritory;
     private TerritoryNode defendingTerritory;
     private int unclaimedTerritories = 43;

     private Player[] currentPlayers;
     private int activePlayers = 0;
     private int currentPlayersTurn = -1;
     [SerializeField] private GameObject[] playerInfo;

     private TURN_PHASE currentPhase;
     private Text turnInfo;
     private Text phaseInfoTxt;
     private Image turnInfoCol;
     private Image border;
     private Image attackingCol;
     private Image defendingCol;
     public GameObject setupPanel;
     private GameObject attackUI;
     private GameObject fortifyUI;
     private GameObject reinforceUI;
     private GameObject turnButton;
     [SerializeField] private GameObject explosionPrefab;

     private Vector3[] explosionQueue;
     private int explosionsWaiting = 0;
     private int numAttackingDice = 0;
     private int defendersLost = 0;
     private int attackersLost = 0;

     //#########################//

     private GameObject attackPanel;
     private GameObject debugPopulateButton;
     private Slider attackSlider;
     private Text attackDice;
     private Text attackingCountry;
     private Text attackingSoldierCount;
     private Text defendingCountry;
     private Text defendingSoldierCount;
     private Text attackButton;

     [SerializeField] private GameObject[] attackingDice;
     [SerializeField] private GameObject[] defendingDice;


     // Use this for initialization
     void Start () {

          explosionQueue = new Vector3[defendingDice.Length];
          allTerritories = new TerritoryNode[unclaimedTerritories]; 															//Test
          turnInfo = GameObject.Find("Current Turn").GetComponent<Text>();
          phaseInfoTxt = GameObject.Find("Current Phase").GetComponent<Text>();
          turnInfoCol = turnInfo.gameObject.GetComponentInChildren<Image>();
          border = GameObject.Find("Border").GetComponent<Image>();
          attackingCol = GameObject.Find("AttackerColour").GetComponent<Image>();
          defendingCol = GameObject.Find("DefenderColour").GetComponent<Image>();
          attackUI = GameObject.Find("UI - Attack");
          attackPanel = GameObject.Find("UI - AttackSelection");
          fortifyUI = GameObject.Find("UI - Fortification");
          reinforceUI = GameObject.Find("UI - Reinforcement");
          attackDice = GameObject.Find("DiceUI").GetComponent<Text>();                    //# of attack dice OR # of soldiers moving to country
          attackingCountry = GameObject.Find("AttackingCountry").GetComponent<Text>();
          attackingSoldierCount = GameObject.Find("AttackingSoldierCount").GetComponent<Text>();
          defendingCountry = GameObject.Find("DefendingCountry").GetComponent<Text>();
          defendingSoldierCount = GameObject.Find("DefendingSoldierCount").GetComponent<Text>();
          turnButton = GameObject.Find("End Turn Button");
          attackButton = GameObject.Find("AttackButtonText").GetComponent<Text>();
          attackSlider = GameObject.Find("SoldierSlider").GetComponent<Slider>();
          debugPopulateButton = GameObject.Find("Debugging: Populate Territories");
          attackUI.SetActive(false);
          fortifyUI.SetActive(false);
          reinforceUI.SetActive(false);
          attackPanel.SetActive(false);
        
     }
	
     // Update is called once per frame
     void Update () {

          if (optionsSelected)  {  //gameplay

               if (!setup) {

                    // Game Over - Display Winner and limit interacion
                    if (activePlayers < 2) {

                         //TODO : GAME OVER STUFF

                    }

                    // Main game loop - filtered by gamestate
                    switch (currentPhase) {


                    #region RECRUIT
                    case TURN_PHASE.RECRUIT:
                         
                         // Beginning of round Initialization
                         if (!didOnce) {

                              reinforceUI.SetActive(true);
                              attackUI.SetActive(false);
                              fortifyUI.SetActive(false);
                              CalculateReinforcements(currentPlayers[currentPlayersTurn]);
                              didOnce = true;

                         }
                         
                         // Entry point for input - left mouse button to interact
                         if (Input.GetMouseButtonDown(0) && currentPlayersTurn != -1)    {

                              RaycastHit2D mouseCast2D = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), 100, 1 << LayerMask.NameToLayer("Territory"));       // Raycast mouse position, return hit on territory

                              if (mouseCast2D) {

                                   // Deselect a territory that has been selected already
                                   if (currentTerritory) {

                                        currentTerritory.SetCurrentSelection(false);
                                        currentTerritory = null;

                                   }

                                   currentTerritory = mouseCast2D.rigidbody.GetComponent<TerritoryNode>();                                                      // Set the current active territory to the one that was clicked

                                   if ( currentPlayers[currentPlayersTurn].GetArmies() > 0 && currentTerritory.DisplayOwner() == currentPlayersTurn)  {

                                        currentTerritory.SetCurrentSelection(true);
                                        currentTerritory.AdjustSoldiers(1);
                                        currentPlayers[currentPlayersTurn].AddArmies(-1);

                                        if (currentPlayers[currentPlayersTurn].GetArmies() == 0)
                                             turnButton.SetActive(true);
                                   }
                              }
                         }

                         break;
                    #endregion

                    #region ATTACK
                    case TURN_PHASE.ATTACK:
                        

                         // Beginning of Attack Phase Initialization
                         if (!didOnce) {

                              reinforceUI.SetActive(false);
                              attackUI.SetActive(true);
                              fortifyUI.SetActive(false);
                              didOnce = true;

                         }

                         ResolveCombat();
                         
                         if (territoryConquered) {

                              attackButton.text = "Move";
                              attackSlider.maxValue = currentTerritory.DisplaySoldiers() - 1;
                              attackDice.text = attackSlider.value.ToString();

                              if (!attackPanel.activeInHierarchy)
                                   OpenAttackPanel();


                         //Mouse Input while the attack options panel is closed
                         } else if (Input.GetMouseButtonDown(0) && !attackPanel.activeInHierarchy && currentPlayersTurn != -1) {  

                              RaycastHit2D mouseCast2D = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), 100, 1 << LayerMask.NameToLayer("Territory"));

                              if (mouseCast2D) {

                                   TerritoryNode newTerritory = mouseCast2D.rigidbody.GetComponent<TerritoryNode>();


                                   // clicked territory is a territory adjacent to selected player territory.
                                   if(newTerritory != currentTerritory && newTerritory.GetCurrentSelection()) {

                                        currentTerritory.DeselectAdjacentTerritories();
                                        SelectDefendingCountry(newTerritory);
                                        attackSlider.minValue = 1;
                                        OpenAttackPanel();

                                   //clicked territory is owned by the player and is eligible to attack
                                   } else if (newTerritory.DisplayOwner() == currentPlayersTurn && newTerritory.DisplaySoldiers() > 1) {

                                        SelectActiveTerritory(newTerritory);

                                   //clicked territory does not qualify as an attacking country or defending country
                                   } else
                                        DeselectTerritory();

                              } else {
                                   DeselectTerritory();
                              }
                         }

                         // Attack options panel/finalizing the attack.
                         if(attackPanel.activeInHierarchy && !territoryConquered) {

                              attackButton.text = "Attack!";

                              if(currentTerritory.DisplaySoldiers() > 3)
                                   attackSlider.maxValue = 3;
                              else
                                   attackSlider.maxValue = currentTerritory.DisplaySoldiers() - 1;

                              attackDice.text = attackSlider.value.ToString();

                         } 

                         break;
                    #endregion

                    #region FORTIFY
                    case TURN_PHASE.FORTIFY:

                         // Beginning of phase initialization
                         if (!didOnce) {

                              reinforceUI.SetActive(false);
                              attackUI.SetActive(false);
                              fortifyUI.SetActive(true);

                         }

                         //Mouse Input while the attack options panel is closed
                         if(Input.GetMouseButtonDown(0) && !attackPanel.activeInHierarchy && currentPlayersTurn != -1 && !fortified)  {

                              RaycastHit2D mouseCast2D = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), 100, 1 << LayerMask.NameToLayer("Territory"));

                              if(mouseCast2D) {

                                   TerritoryNode newTerritory = mouseCast2D.rigidbody.GetComponent<TerritoryNode>();

                                   // clicked territory is a territory adjacent to selected player territory.
                                   if(newTerritory != currentTerritory && newTerritory.GetCurrentSelection()) {

                                        currentTerritory.DeselectAdjacentTerritories();
                                        SelectDefendingCountry(newTerritory);
                                        OpenAttackPanel();

                                   } else if(newTerritory.DisplayOwner() == currentPlayersTurn && newTerritory.DisplaySoldiers() > 1) { //clicked territory is owned by the player and is eligible to fortify another territory

                                        DeselectTerritory();
                                        SelectActiveTerritory(newTerritory);

                                   } else                                                                        //clicked territory does not qualify as an attacking country or defending country 
                                        DeselectTerritory();
                              } else
                                   DeselectTerritory();
                         }


                         if (attackPanel.activeInHierarchy)  { //Fortify options panel. (repurposed attack options panel)                          
                        
                              attackButton.text = "Fortify";
                              attackSlider.minValue = 1;
                              attackSlider.maxValue = currentTerritory.DisplaySoldiers() - 1;
                              attackDice.text = attackSlider.value.ToString();
                         }

                         if (fortified)
                              NextTurnButton();   // Advance turn when all actions have been taken

                         break;
                    #endregion


                    default:
                         Debug.Log("Its just a phase....");                        break;
                    }

               #region SETUP
               } else { //Setup Phase -- Ends when all Players are out of soldiers to place           

                    if(unclaimedTerritories <= 0) {

                         allTsClaimed = true;
                         phaseInfoTxt.text = "Reinforce claimed territories";

                    }


                    if (Input.GetMouseButtonDown(0) && currentPlayersTurn != -1) {  //Mouse Input handling ( Click on Territory to place soldier)

                         RaycastHit2D mouseCast2D = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), 100, 1 << LayerMask.NameToLayer("Territory"));

                         if (mouseCast2D) {

                              if (currentTerritory) 
                                   currentTerritory.SetCurrentSelection(false);

                              currentTerritory = mouseCast2D.rigidbody.GetComponent<TerritoryNode>();

                              // can only pick neutral territories until there are no more
                              if(!allTsClaimed) {  
                                   
                                   // Can only pick neutral territories - territories with owner set to -1
                                   if (currentTerritory.DisplayOwner() < 0) {
                                        
                                        // Hide the populate button once the first territory has been selected
                                        if(debugPopulateButton.activeInHierarchy)
                                             debugPopulateButton.SetActive(false);
                                        

                                        currentTerritory.SetCurrentSelection(true);
                                        currentTerritory.AdjustSoldiers(1);
                                        currentPlayers[currentPlayersTurn].AddArmies(-1);
                                        currentPlayers[currentPlayersTurn].AddTerritory(currentTerritory);
                                        currentTerritory.SetColor(currentPlayers[currentPlayersTurn].armyColour);
                                        currentTerritory.SetOwner(currentPlayersTurn);
                                        unclaimedTerritories -= 1;

                                        if (currentTerritory.GetContinent().CheckBonus(currentPlayersTurn)) 
                                             currentTerritory.GetContinent().UpdateBorderColour(GetCurrentPlayer().armyColour);
                                
                                        

                                        AdvanceTurn();
                                   }

                              } else {    //can only click on owned territories to add soldiers
                                   
                                   //TODO: Add new graphic or text to show that all territories have been claimed, and the player must now only choose territories they own

                                   if (currentTerritory.DisplayOwner() == currentPlayersTurn) {
                                        currentTerritory.SetCurrentSelection(true);
                                        currentTerritory.AdjustSoldiers(1);
                                        currentPlayers[currentPlayersTurn].AddArmies(-1);
                                        AdvanceTurn();
                                   }
                              }
                         }   //end of raycasts
                    }   // end of input

                    if (currentPlayers[currentPlayersTurn].GetArmies() < 1) {

                         setup = false;
                         phaseInfoTxt.text = "Current Phase: ";

                    }

               }   //end of setup phase
               #endregion

               //Display Info
               int uITurnInfo = currentPlayersTurn + 1;
               turnInfo.text = "Player " + uITurnInfo + "'s Turn!";
               turnInfoCol.color = currentPlayers[currentPlayersTurn].armyColour;
               border.color = currentPlayers[currentPlayersTurn].armyColour;

          } //end of gameplay
     }

     private void FixedUpdate() {

          if (!setup) 
               CheckPlayers();

          if (attackPanel.activeInHierarchy) {

               attackingSoldierCount.text = (currentTerritory.DisplaySoldiers() - (int)attackSlider.value).ToString();

               if(currentPhase == TURN_PHASE.FORTIFY || territoryConquered)
                    defendingSoldierCount.text = (defendingTerritory.DisplaySoldiers() + (int)attackSlider.value).ToString();
          }
     }

     /// <summary>
     /// Initialize UI and settings for the game
     /// </summary>
     /// <param name="nPlayers">Number of players in the game</param>
     public void SetupGame(int nPlayers) {

          int territories = GameObject.FindGameObjectsWithTag("Map").Length;
          activePlayers = nPlayers;

          for (int i = 0; i < territories; i++) {

               allTerritories[i] = GameObject.FindGameObjectsWithTag("Map")[i].GetComponent<TerritoryNode>();
               allTerritories[i].SetColor(Color.white);

          }

          currentPlayers = new Player[nPlayers];
	     for(int i = 1; i <= nPlayers; i++)                                                   // start counter at 1 to match up with player number before
		     currentPlayers[i-1] = GameObject.Find("Player" +i).GetComponent<Player>();      // the player is assigned to correct array position.

          for (int i = nPlayers; i <= 5; i++)
               playerInfo[i].SetActive(false);
        
          switch (nPlayers) {

	     case 2:

		     foreach (Player newPlayer in currentPlayers) {

			     newPlayer.AddArmies(24);
                    newPlayer.InitializePlayer();

		     }
               
		     break;

	     case 3:

		     foreach(Player newPlayer in currentPlayers) {

			     newPlayer.AddArmies(35);
                    newPlayer.InitializePlayer();

               }

		     break;

	     case 4:

		     foreach (Player newPlayer in currentPlayers) {

			     newPlayer.AddArmies(30);
                    newPlayer.InitializePlayer();

               }

		     break;

	     case 5:

		     foreach (Player newPlayer in currentPlayers) {

			     newPlayer.AddArmies(25);
                    newPlayer.InitializePlayer();

               }

		     break;

	     case 6:

		     foreach (Player newPlayer in currentPlayers) {

			     newPlayer.AddArmies(20);
                    newPlayer.InitializePlayer();

		     }

		     break;

	     default:

		     Debug.Log("invalid number of players for gamesetup");
		     break;

	     }

          int startingPlayer = UnityEngine.Random.Range(0, nPlayers);
          setupPanel.SetActive(false);
          currentPlayersTurn = startingPlayer;
          phaseInfoTxt.text = "Pick an unclaimed territory (white)";
          activePlayers = nPlayers;
          optionsSelected = true;

          //Initialize Game Objects
          foreach (TerritoryNode node in allTerritories) { 
               node.SetSoldiers(0);
          }

          foreach(GameObject die in attackingDice) {
               die.SetActive(false);
          }

          foreach(GameObject die in defendingDice) {
               die.SetActive(false);
          }
     }

     /// <summary>
     /// Calculates the number of soldiers a player gets during the Recruitment phase
     /// </summary>
     /// <param name="playerID"> Who's turn is it? </param>
     private void CalculateReinforcements(Player playerID) {

          int newArmies = 0;

          //Every 3 territories provide a soldier
          for(int i = 1; i <= playerID.GetTerritories().Count; i++) {
               if(i % 3 == 0) 
                    newArmies++;
          }

          //Continents provide a set # of soldiers 
          foreach(Continent bonusboi in continents)
               if (bonusboi.CheckBonus(currentPlayersTurn)) 
                    newArmies += bonusboi.Value;
               
          if (newArmies < 3)
               playerID.AddArmies(3);

          else
               playerID.AddArmies(newArmies);
     }

     private void CheckPlayers() {

          foreach(Player player in currentPlayers) {
               if(player.GetTerritories().Count == 0)
                    player.alive = false;
          }
     }

     private void AdvanceTurn() {

          if(currentPlayersTurn < currentPlayers.Length -1)
               currentPlayersTurn++;

          else 
               currentPlayersTurn = 0;

          if (!currentPlayers[currentPlayersTurn].alive)  //Is this player dead? Skip 'em!
               AdvanceTurn();

          else {

               currentPhase = TURN_PHASE.RECRUIT;
               didOnce = false;
               fortified = false;
               turnButton.SetActive(false);

          }
     }

     /// <summary>
     /// Makes the inputted territory active after deactivating the last selected territory.
     /// This selected territory is now ready to interact with the player
     /// </summary>
     /// <param name="attacker"> The territory that will be either attacking enemy territories, or sending reinforcements</param>
     private void SelectActiveTerritory(TerritoryNode attacker) { //Highlights the active Territory, before querrying it's adjacentTerritories.

          DeselectTerritory();
          attackingCountry.text = attacker.name;
          attackingSoldierCount.text = attacker.DisplaySoldiers().ToString();

          switch (currentPhase) {

               case (TURN_PHASE.ATTACK):

                    attacker.HighlightAdjacentTerritories(false);  
                    break;

               case (TURN_PHASE.FORTIFY):

                    attacker.HighlightAdjacentTerritories(true);
                    break;
          }

          currentTerritory = attacker;
          

     }

     /// <summary>
     /// Prepares the inputted territory to either defend or receive soldiers
     /// </summary>
     /// <param name="defender"> The territory to be attacked or fortified </param>
     private void SelectDefendingCountry(TerritoryNode defender) {

          defender.outline.color = Color.red;
          defendingCountry.text = defender.name;
          defendingSoldierCount.text = defender.DisplaySoldiers().ToString();
          defendingTerritory = defender;
          defendingCol.color = currentPlayers[defendingTerritory.DisplayOwner()].armyColour;

     }

     /// <summary>
     /// Risk dice rolling algorithm - Invoked by the accept button on the attack panel
     /// Note: For now the defender doesn't get to choose how many dice to use - defenders will always use the most dice they have available (
     /// </summary>
     private void RollDice() {

          CloseAttackPanelButton();

          numAttackingDice = (int)attackSlider.value;
          int numDefendingDice = 0;

          if (numAttackingDice > 0) {

               if (defendingTerritory.DisplaySoldiers() == 1 || numAttackingDice == 1)
                    numDefendingDice = 1;

               else
                    numDefendingDice = 2;


               int[] attackDiceRoll = new int[numAttackingDice];
               int[] defendDiceRoll = new int[numDefendingDice];

               for (int i = 0; i < numAttackingDice; i++) {

                    attackingDice[i].SetActive(true);
                    attackingDice[i].GetComponent<RiskDie>().Roll();
                    attackDiceRoll[i] = attackingDice[i].GetComponent<RiskDie>().GetTopFace();

               }

               for (int i = 0; i < numDefendingDice; i++) {

                    defendingDice[i].SetActive(true);
                    defendingDice[i].GetComponent<RiskDie>().Roll();
                    defendDiceRoll[i] = defendingDice[i].GetComponent<RiskDie>().GetTopFace();

               }

               //Sort dice in descending order to ensure the dice match properly
               SortIntArrayDesc(attackDiceRoll);
               SortIntArrayDesc(defendDiceRoll);

               // Combat Results: compare top dice and prepare explosion effects
               explosionInQueue = true;
               explosionsWaiting = numDefendingDice;

               for (int i = 0; i < numDefendingDice; i++) {

                    if (attackDiceRoll[i] > defendDiceRoll[i]) {

                         defendersLost++;
                         explosionQueue[i] = defendingTerritory.transform.position; 

                    } else {

                         attackersLost++;
                         explosionQueue[i] = currentTerritory.transform.position;

                    }

               }

          }
          
     } 

     /// <summary>
     /// Used in conjunction with the Attack Panel: Once the player confirms their attack or fortification, 
     /// AttackButton() calls this to transfer the selected number of troops from the 'attacker' territory
     /// to the 'defender' territory
     /// </summary>
     private void MoveTroops() {

          territoryConquered = false;
          int soldiers = (int)attackSlider.value;
          currentTerritory.AdjustSoldiers(-soldiers);
          defendingTerritory.AdjustSoldiers(soldiers);
          DeselectTerritory();

          
          if (defendingTerritory.GetContinent().CheckBonus(currentPlayersTurn)) 
               defendingTerritory.GetContinent().UpdateBorderColour(GetCurrentPlayer().armyColour);

          else
               defendingTerritory.GetContinent().ResetBorderColour();

          CloseAttackPanelButton();
     }

     /// <summary>
     /// Used by the attack panel button
     /// </summary>
     public void AttackButton() {

          if(territoryConquered)
               MoveTroops();

          else if(currentPhase == TURN_PHASE.FORTIFY) {

               MoveTroops();
               fortified = true;

          } else
               RollDice();

     }

     /// <summary>
     /// Used by the Next Turn button: phase/turn progression
     /// </summary>
     public void NextTurnButton() {

          if (!setup) {

               switch (currentPhase) {

               case TURN_PHASE.RECRUIT:

                    currentPhase = TURN_PHASE.ATTACK;
                    break;

               case TURN_PHASE.ATTACK:

                    if (!attackPanel.activeInHierarchy) 
                         currentPhase = TURN_PHASE.FORTIFY;

                    break;

               case TURN_PHASE.FORTIFY:

                    if(!attackPanel.activeInHierarchy)
                         AdvanceTurn();

                    break;
               }

               didOnce = false;

          }

     } // end NextTurnButton()

     public void CloseAttackPanelButton() { attackPanel.SetActive(false); }

     public void OpenAttackPanel() {

          attackingCol.color = currentPlayers[currentTerritory.DisplayOwner()].armyColour;
          attackPanel.SetActive(true); 

     }
   
     /// <summary>
     /// Helper function that Sorts an integer array in descending order
     /// </summary>
     /// <param name="ary"> The integer array</param>
     private void SortIntArrayDesc(int[] ary) {

          System.Array.Sort(ary,
               delegate (int a, int b) {
                    return b - a;
               });
     }

     /// <summary>
     /// Highlight colour around territory and adjacent territories are removed
     /// </summary>
     private void DeselectTerritory() {
          currentTerritory.DeselectAdjacentTerritories();
          currentTerritory.SetCurrentSelection(false);
     }

     /// <summary>
     /// Used at the beginning of the game to shuffle the territories and distribute them randomly to each player active in the game
     /// </summary>
     public void DebugPopulateTerritories(){

          System.Random rng = new System.Random();
          int n = unclaimedTerritories;
          TerritoryNode[] shuffledTerritories = allTerritories;

          debugPopulateButton.SetActive(false);

          // Shuffle All the territories
          while (n > 1) {
               n--;
               int k = rng.Next(n + 1);
              
               TerritoryNode terry = shuffledTerritories[k];
               shuffledTerritories[k] = shuffledTerritories[n];
               shuffledTerritories[n] = terry;
          }


          // Distribute the territories in order to each active player
          int i = 0;
          n = unclaimedTerritories;
          while (i < n) {
               
               for(int j = 0; j < activePlayers; j++ ) {

                    TerritoryNode terry = shuffledTerritories[i];

                    terry.SetCurrentSelection(true);
                    ConquerTerritory(terry, j);
                    terry.SetCurrentSelection(false);
                    unclaimedTerritories--;
                    i++;

                    if(i >= n) {

                         currentPlayersTurn = j;                      // set player's turn to the last player to place an army before advancing
                         break;

                    }
               }
               
          }

          AdvanceTurn();
     }

     /// <summary>
     /// Every frame during the attack phase we check whether or not there are explosions queued up and if so, we wait for the 
     /// dice to stop rolling before we initiate the explosion and change owners.
     /// </summary>
     private void ResolveCombat() {


          if(explosionInQueue && !defendingDice[0].GetComponent<RiskDie>().GetStillRolling()) {

               for(int i = 0; i < explosionsWaiting; i++) {

                    ExplodeAt(explosionQueue[i]);
                    defendingTerritory.AdjustSoldiers(-defendersLost);
                    currentTerritory.AdjustSoldiers(-attackersLost);

                    

                    //Annex territory, set all properties necessary for the new owner
                    if(defendingTerritory.DisplaySoldiers() == 0) {

                         attackSlider.minValue = numAttackingDice - attackersLost;

                         ConquerTerritory(defendingTerritory, currentPlayersTurn, 0);
                         defendingCol.color = currentPlayers[currentPlayersTurn].armyColour;
                         territoryConquered = true;

                    }

                    defendersLost = 0;
                    attackersLost = 0;
                    
               }

               explosionInQueue = false;
               explosionsWaiting = 0;
          }

         
     }

     /// <summary>
     /// Spawns an explosion at the target location - used within ResolveCombat()
     /// </summary>
     /// <param name="target"> position information held within explosionQueue[] </param>
     private void ExplodeAt(Vector3 target) {

          GameObject explode = CFX_SpawnSystem.GetNextObject(explosionPrefab, false);               //WarFX by JMO
          Vector3 exPos = target;
          exPos.z = 1.1f;
          explode.transform.position = exPos;
          explode.SetActive(true);

     }

     /// <summary>
     /// Flexible way of assigning a territory to a player. Will subtract 'soldierCount' from the player and initialize the territory appropriately
     /// </summary>
     /// <param name="annexedTerritory"> The territory to be assigned to the player. </param>
     /// <param name="playerId"> The player's index within currentPlayers[]. </param>
     /// <param name="soldierCount"> Optional: The number of soldiers to be transfered to the annexedTerritory. </param>
     private void ConquerTerritory(TerritoryNode annexedTerritory, int playerId, int soldierCount = 1){

          if(soldierCount != 0) {

               annexedTerritory.AdjustSoldiers(soldierCount);                             // Add Soldiers to the territory
               currentPlayers[playerId].AddArmies(soldierCount * -1);                     // Remove Soldiers from the player's pool

          } 

          currentPlayers[playerId].AddTerritory(annexedTerritory);                   // Give the player a reference to their new territory
          annexedTerritory.SetColor(currentPlayers[playerId].armyColour);            // Change colour of the territory to the player's colour

          // Remove the old player's ownership of the territory
          if(annexedTerritory.DisplayOwner() >= 0) 
               currentPlayers[annexedTerritory.DisplayOwner()].RemoveTerritory(annexedTerritory); 
          
          annexedTerritory.SetOwner(playerId);                                       // Give the territory their new owner's player ID

     }

     // Accessors / Mutators \\
     public Player GetCurrentPlayer() { return currentPlayers[currentPlayersTurn]; }
}
