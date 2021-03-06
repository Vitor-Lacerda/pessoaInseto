﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerControl : MonoBehaviour {

	/*Valores Fixos*/
	private const float epsilon = 0.001f;




	/*Parametros*/
	[Header("Componentes Externos")]
	public Collider2D colisorChecaCrescida;
	public LayerMask camadasChao, camadasChaoPequeno, camadasInterageCrescida;


	[Header("Valores Gerais")]
	public float velocidadeHorizontal = 2.5f;
	public float tamanhoRaioChao = 0.5f;
	public float tamanhoRaioParede = 0.2f;
	public float tamanhoGrande = 1;
	public float tamanhoPequeno = 0.4f;
	public float tempoDiminuicao = 0.1f;
	public float multiplicadorDescida = 2.5f;
	public float multiplicadorPuloMenor = 2f;
	public float forcaEmpurraoDano = 1f;
	public float tempoDBoost = 0.2f;




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
	private LayerMask camadasChaoAtual;

	/*Condicoes*/
	private bool pulo, seguraPulo, temControle, tomandoDano;
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
		temControle = true;
		tomandoDano = false;
		
		CalculaValoresPulo ();

		diferencaTamanho = tamanhoGrande - tamanhoPequeno;
		razaoTamanho = tamanhoPequeno / tamanhoGrande;
		decrementoTamanho = diferencaTamanho / tempoDiminuicao;

		tamanhoAtual = tamanhoGrande;
		vetorGravidade = Vector2.up;
		camadasChaoAtual = camadasChao;



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
			camadasChaoAtual = camadasChao;
			gameObject.layer = LayerMask.NameToLayer ("Default");
		} else {
			transform.localScale = new Vector3 (tamanhoPequeno * sinalX, tamanhoPequeno, 1);
			camadasChaoAtual = camadasChaoPequeno;
			gameObject.layer = LayerMask.NameToLayer ("Pequeno");
		}
		tamanhoAtual = transform.localScale.y;

		isPequeno = !isPequeno;

		yield return null;
		mudandoTamanho = false;

		/*
		if (groundedAntes) {
			CalculaValoresPulo ();
		}
		*/


	}

	protected bool podeCrescer(){
		Collider2D[] contatos = new Collider2D[1];
		ContactFilter2D filtro = new ContactFilter2D ();
		filtro.SetLayerMask (camadasInterageCrescida);
		filtro.useLayerMask = true;

		if (colisorChecaCrescida.OverlapCollider (filtro, contatos) > 0) {
			for (int i = 0; i < contatos.Length; i++) {
				//Debug.Log (contatos [i].gameObject.name);
				CresceInteractable ci = contatos [i].gameObject.GetComponent<CresceInteractable> ();
				if (ci != null) {
					ci.interacaoCrescida ();
					return true;
				}
			}
			return false;
		}
		return true;

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

		animator.SetBool ("Grounded", grounded);

		if (temControle) {
			moveX = Input.GetAxisRaw ("Horizontal");
			seguraPulo = Input.GetButton ("Jump");
			pulo = Input.GetButtonDown ("Jump") && grounded;

			if (moveX != 0) {
				transform.localScale = new Vector3 (tamanhoAtual * moveX / Mathf.Abs (moveX), transform.localScale.y, 1);
			}


			if (Input.GetKeyDown (KeyCode.Z) && !mudandoTamanho) {
				if (!isPequeno || podeCrescer ()) {
					StartCoroutine (RotinaTrocaTamanho ());
				}
			}

			if (Input.GetKeyDown (KeyCode.Q)) {
				StartCoroutine (tomarDano ());
			}

		}
		groundedAntes = grounded;





	}

	void FixedUpdate(){

		if (temControle) {
			rigidBody2D.velocity = new Vector2 (moveX * velocidadeHorizontal, rigidBody2D.velocity.y);
	
			if (pulo) {
				if (!isPequeno) {
					rigidBody2D.velocity = new Vector2 (rigidBody2D.velocity.x, forcaPulo);
					gravidade = gravidadeNormal;
				} 
			}
			animator.SetFloat ("VelocidadeX", Mathf.Abs (rigidBody2D.velocity.x));
		}

		if(rigidBody2D.velocity.y < 0){ //Caindo
			rigidBody2D.velocity += vetorGravidade *gravidade * multiplicadorDescida *Time.deltaTime;
		}
		else if(rigidBody2D.velocity.y > 0 && !seguraPulo){
			rigidBody2D.velocity += vetorGravidade *gravidade * multiplicadorPuloMenor *Time.deltaTime;

		} else  {
			rigidBody2D.velocity += vetorGravidade *gravidade *Time.deltaTime;
		}


	}

	protected void calculaChao(){
		
		RaycastHit2D hit;
		Debug.DrawRay (transform.position, -transform.up* (tamanhoRaioChao), Color.white);
		hit = Physics2D.Raycast (transform.position, -transform.up, tamanhoRaioChao, camadasChaoAtual);
		if (hit.collider != null) {
			grounded = true;
			vetorGravidade = hit.normal;
			ultimaNormal = hit.normal;
			/*
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
			*/
		} else {
			grounded = false;
			resetaRotacao ();
		}

	}

	protected IEnumerator tomarDano(){
		if (!tomandoDano) {
			Debug.Log ("Dano");
			tomandoDano = true;
			temControle = false;

			float direcaoMovimento = moveX != 0 ? -moveX : -1;
			rigidBody2D.velocity = new Vector2 (direcaoMovimento * forcaEmpurraoDano/2, forcaEmpurraoDano);

			float timer = 0f;
			while (timer < tempoDBoost) {
				timer += Time.deltaTime;
				yield return null;
			}

			tomandoDano = false;
			temControle = true;
		}
		yield return null;

	}

	protected void resetaRotacao(){
		vetorGravidade = Vector2.up;
		ultimaNormal = Vector2.up;
		Quaternion target = Quaternion.identity;
		transform.rotation = target;
	}
		
}
