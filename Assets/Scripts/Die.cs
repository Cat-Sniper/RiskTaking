using UnityEngine;
using System.Collections;

public class Die : MonoBehaviour {      //Generic Die class, can be used for any sized numeric die.
	private int nSides = 6;
    private int topFace = 1;
    protected Animator anim;
    protected bool startedRoll = false;
    // Use this for initialization
    void Start () {
        anim = gameObject.GetComponent<Animator>();
	}

	public void Roll(){
        anim.SetTrigger("DieRoll");
        int newTop = Random.Range(1, nSides + 1);
		topFace = newTop;
        anim.SetInteger("DieFace", topFace);
        startedRoll = true;
	}

	public void SetNumberOfSides(int sides){ nSides = sides;}
	public int GetNumberOfSides(){return nSides;}
    public int GetTopFace() { return topFace;}
}
