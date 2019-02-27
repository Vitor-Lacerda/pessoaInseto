using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inimigo : MonoBehaviour, CresceInteractable, IDamageable {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void interacaoCrescida(){
		Destruir ();
	}

	public void tomarDano(int dano){
		Destruir ();
	}

	protected void Destruir(){
		Destroy (this.gameObject);
	}
}
