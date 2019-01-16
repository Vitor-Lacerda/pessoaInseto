using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerControl : MonoBehaviour {

	/*Parametros*/
	public float velocidadeHorizontal = 2.5f;
	public float deslocamentoHorizontalPicoPulo = 1.25f;
	public float alturaPulo = 4f;
	public float multiplicadorDescida = 2.5f;
	public float multiplicadorPuloMenor = 2f;
	public float tamanhoRaioChao = 0.5f;
	public LayerMask camadasChao;

	/*Calculados*/
	private float forcaPulo;
	private float gravidade, gravidadeNormal;

	/*Condicoes*/
	private bool pulo, seguraPulo;

	/*Componentes*/
	private Rigidbody2D rigidBody2D;
	private Animator animator;

	/*Input*/
	private float moveX;







	// Use this for initialization
	void Start () {
		forcaPulo = 2 * alturaPulo * velocidadeHorizontal / deslocamentoHorizontalPicoPulo;
		gravidadeNormal = -2*alturaPulo*(Mathf.Pow(velocidadeHorizontal,2))/Mathf.Pow(deslocamentoHorizontalPicoPulo,2);
		rigidBody2D = this.GetComponent<Rigidbody2D> ();
		animator = this.GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {
		bool grounded = IsGrounded ();
		pulo = Input.GetButtonDown ("Jump") && grounded;
		seguraPulo = Input.GetButton ("Jump");
		moveX = Input.GetAxisRaw ("Horizontal");

		animator.SetBool ("Grounded", grounded);

		if(moveX != 0){
			transform.localScale = new Vector3 (moveX / Mathf.Abs (moveX), 1, 1);
		}


	}

	void FixedUpdate(){
		rigidBody2D.velocity = new Vector2 (moveX * velocidadeHorizontal, rigidBody2D.velocity.y);

		if(pulo){
			rigidBody2D.velocity = new Vector2(rigidBody2D.velocity.x, forcaPulo);
			gravidade = gravidadeNormal;
		}

		if(rigidBody2D.velocity.y < 0){ //Caindo
			rigidBody2D.velocity += Vector2.up *gravidade * multiplicadorDescida *Time.deltaTime;
		}
		else if(rigidBody2D.velocity.y > 0 && !seguraPulo){
			rigidBody2D.velocity += Vector2.up *gravidade * multiplicadorPuloMenor *Time.deltaTime;

		} else  {
			rigidBody2D.velocity += Vector2.up *gravidade *Time.deltaTime;
		}

		animator.SetFloat ("VelocidadeX", Mathf.Abs(rigidBody2D.velocity.x));




	}

	protected bool IsGrounded(){
		
		RaycastHit2D hit;
		Debug.DrawRay (transform.position, Vector2.down * (tamanhoRaioChao), Color.white);
		hit = Physics2D.Raycast (transform.position, Vector2.down, tamanhoRaioChao, camadasChao);
		if (hit.collider != null) {
			return true;
		}
		return false;

	}
}
