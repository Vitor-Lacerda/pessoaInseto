using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerControl : MonoBehaviour {

	/*Valores Fixos*/
	private const float epsilon = 0.001f;


	/*Parametros*/
	[Header("Valores Gerais")]
	public float velocidadeHorizontal = 2.5f;
	public float tamanhoRaioChao = 0.5f;
	public float tamanhoRaioParede = 0.2f;
	public float tamanhoGrande = 1;
	public float tamanhoPequeno = 0.4f;
	public float tempoDiminuicao = 0.1f;
	public float multiplicadorDescida = 2.5f;
	public float multiplicadorPuloMenor = 2f;
	public LayerMask camadasChao, paredeQueSobe;

	[Header("Valores Grande")]
	public float deslocamentoHorizontalPicoPulo = 1.25f;
	public float alturaPulo = 4f;


	[Header("Valores Pequeno")]
	public float deslocamentoHorizontalPicoPuloPequeno = 1.25f;
	public float alturaPuloPequeno = 1f;

	[Header("Componentes")]
	public GameObject cameraPequeno;


	/*Calculados*/
	private float forcaPulo;
	private float gravidade, gravidadeNormal;
	private float decrementoTamanho, diferencaTamanho, razaoTamanho;
	private float tamanhoAtual;
	private Vector2 vetorGravidade, ultimaNormal;

	/*Condicoes*/
	private bool pulo, seguraPulo, puloParede;
	private bool isPequeno, isPequenoAntes;
	private bool grounded, groundedAntes;
	private bool mudandoTamanho;

	/*Componentes*/
	private Rigidbody2D rigidBody2D;
	private Animator animator;

	/*Input*/
	private float moveX;







	// Use this for initialization
	void Start () {

		rigidBody2D = this.GetComponent<Rigidbody2D> ();
		animator = this.GetComponent<Animator> ();
		isPequeno = false;
		isPequenoAntes = isPequeno;
		
		CalculaValoresPulo ();

		diferencaTamanho = tamanhoGrande - tamanhoPequeno;
		razaoTamanho = tamanhoPequeno / tamanhoGrande;
		decrementoTamanho = diferencaTamanho / tempoDiminuicao;

		tamanhoAtual = tamanhoGrande;
		vetorGravidade = Vector2.up;



	}


	IEnumerator RotinaTrocaTamanho(){
		int sinal = isPequeno ? 1 : -1;
		float sinalX = transform.localScale.x / Mathf.Abs (transform.localScale.x);
		float quantidadeMudada = 0f;
		Vector3 temp = transform.localScale;

		mudandoTamanho = true;


		//Troca a camera
		cameraPequeno.SetActive(!isPequeno);



		//'Interpola' ate o tamanho
		while (quantidadeMudada < diferencaTamanho) {
			temp = transform.localScale;
			sinalX = transform.localScale.x / Mathf.Abs (transform.localScale.x);
			temp.x += decrementoTamanho * sinal * Time.deltaTime * sinalX;
			temp.y += decrementoTamanho * sinal * Time.deltaTime;
			quantidadeMudada += decrementoTamanho * Time.deltaTime;
			tamanhoAtual = temp.y;
			transform.localScale = temp;

			yield return null;
		}

		//Faz um 'snap' pro tamanho correto
		if (isPequeno) {
			transform.localScale = new Vector3 (tamanhoGrande * sinalX, tamanhoGrande, 1);
		} else {
			transform.localScale = new Vector3 (tamanhoPequeno * sinalX, tamanhoPequeno, 1);
		}
		tamanhoAtual = transform.localScale.y;

		isPequeno = !isPequeno;

		if (!isPequeno && ultimaNormal.x != 0) {
			puloParede = true;
			Debug.Log ("Cresceu parede");
		}
		yield return null;
		mudandoTamanho = false;

		/*
		if (groundedAntes) {
			CalculaValoresPulo ();
		}
		*/


	}

	protected void CalculaValoresPulo(){
		float deslocamento = isPequeno ? deslocamentoHorizontalPicoPuloPequeno : deslocamentoHorizontalPicoPulo;
		float altura = isPequeno ? alturaPuloPequeno : alturaPulo;
		forcaPulo = 2 * altura * velocidadeHorizontal / deslocamento;
		gravidadeNormal = -2*altura*(Mathf.Pow(velocidadeHorizontal,2))/Mathf.Pow(deslocamento,2);
		gravidade = gravidadeNormal;
		isPequenoAntes = isPequeno;
	}

	// Update is called once per frame
	void Update () {
		calculaChao ();
		/*
		if (grounded && !groundedAntes) {
			if (isPequeno != isPequenoAntes) {
				CalculaValoresPulo ();
			}
		}
		*/
		pulo = Input.GetButtonDown ("Jump") && grounded;
		seguraPulo = Input.GetButton ("Jump");
		moveX = Input.GetAxisRaw ("Horizontal");

		animator.SetBool ("Grounded", grounded);


		if(moveX != 0){
			transform.localScale = new Vector3 (tamanhoAtual * moveX / Mathf.Abs (moveX), transform.localScale.y, 1);
		}


		if (Input.GetKeyDown (KeyCode.Z) && !mudandoTamanho) {
			StartCoroutine (RotinaTrocaTamanho());
		}

		groundedAntes = grounded;





	}

	void FixedUpdate(){
		if (isPequeno && ultimaNormal.x != 0) {
			rigidBody2D.velocity = (Vector2)transform.right * moveX * velocidadeHorizontal;
		} else {
			rigidBody2D.velocity = new Vector2 (moveX * velocidadeHorizontal, rigidBody2D.velocity.y);
		}


		if(pulo){
			if (!isPequeno) {
				rigidBody2D.velocity = new Vector2 (rigidBody2D.velocity.x, forcaPulo);
				gravidade = gravidadeNormal;
			} else {
				if (ultimaNormal.x != 0 && ultimaNormal.y <= epsilon) {
					rigidBody2D.velocity = ultimaNormal * forcaPulo;
				}
			}
		}

		if (puloParede) {
			Debug.Log ("Fez pulo parede");
			rigidBody2D.velocity = new Vector2(vetorGravidade.x * forcaPulo, forcaPulo*1.2f);
			Debug.Log (rigidBody2D.velocity);
			puloParede = false;
		}

		if(rigidBody2D.velocity.y < 0){ //Caindo
			rigidBody2D.velocity += vetorGravidade *gravidade * multiplicadorDescida *Time.deltaTime;
		}
		else if(rigidBody2D.velocity.y > 0 && !seguraPulo){
			rigidBody2D.velocity += vetorGravidade *gravidade * multiplicadorPuloMenor *Time.deltaTime;

		} else  {
			rigidBody2D.velocity += vetorGravidade *gravidade *Time.deltaTime;
		}

		animator.SetFloat ("VelocidadeX", Mathf.Abs(rigidBody2D.velocity.x));




	}

	protected void calculaChao(){
		
		RaycastHit2D hit;
		Debug.DrawRay (transform.position, -transform.up* (tamanhoRaioChao), Color.white);
		hit = Physics2D.Raycast (transform.position, -transform.up, tamanhoRaioChao, camadasChao);
		if (hit.collider != null) {
			grounded = true;
			vetorGravidade = hit.normal;
			ultimaNormal = hit.normal;
			if (isPequeno) {
				RaycastHit2D hitParede = Physics2D.Raycast (transform.position, transform.right*moveX, tamanhoRaioParede, paredeQueSobe);
				Debug.DrawRay (transform.position, transform.right*moveX*tamanhoRaioParede, Color.red);
				if (hitParede.collider != null) {
					Quaternion target = Quaternion.FromToRotation (Vector2.up, hitParede.normal);
					transform.rotation = Quaternion.Lerp (transform.rotation, target, Time.deltaTime*10);
				} else {
					Quaternion target = Quaternion.FromToRotation (Vector2.up, hit.normal);
					transform.rotation = Quaternion.Lerp (transform.rotation, target, Time.deltaTime*10);
				}
			}
		} else {
			grounded = false;
			resetaRotacao ();
		}

	}

	protected void resetaRotacao(){
		vetorGravidade = Vector2.up;
		ultimaNormal = Vector2.up;
		Quaternion target = Quaternion.identity;
		transform.rotation = target;
	}
		
}
