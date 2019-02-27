using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grade : MonoBehaviour, CresceInteractable {

	public float velocidadeVoo = 10f;
	public float tempoVida = 1f;
	public float velocidadeRotacao = 1f;
	public int dano = 1;

	private bool quebrada = false;
	private float timer;
	private Rigidbody2D rigid;

	// Use this for initialization
	void Start () {
		timer = 0f;
		rigid = GetComponent<Rigidbody2D> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (quebrada) {
			timer += Time.deltaTime;
			transform.Rotate (new Vector3 (0,0,velocidadeRotacao));
			if (timer >= tempoVida) {
				Destruir ();
			}
		}
	}

	void FixedUpdate(){
		if(quebrada){
			//rigid.velocity += Vector2.up * Physics2D.gravity * Time.deltaTime;
		}
	}

	public void interacaoCrescida (){
		rigid.velocity = Vector2.up * velocidadeVoo;
		quebrada = true;
	}
		
	private void Destruir(){
		Destroy (this.gameObject);
	}

	void OnCollisionEnter2D(Collision2D col){
		if (quebrada) {
			IDamageable dmg = col.collider.GetComponent<IDamageable> ();
			if (dmg!=null) {
				dmg.tomarDano (1);
			}
			Destruir ();
		}
	}
}
