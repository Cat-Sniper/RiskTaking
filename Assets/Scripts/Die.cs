using UnityEngine;
using System.Collections;

public class Die : MonoBehaviour {      //Generic Die class, can be used for any sized numeric die.
	private int nSides = 6;
    private Animator anim;
	// Use this for initialization
	void Start () {
        anim = gameObject.GetComponent<Animator>();
	}

	public void Roll(){
		int topFace = Random.Range(1, nSides + 1);
        anim.SetInteger("DieFace", topFace);

	}

	public void SetNumberOfSides(int sides){ nSides = sides;}
	public int GetNumberOfSides(){return nSides;}
}
