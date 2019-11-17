﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class PlayerMovementController : MonoBehaviour {

	public float xSpeed;
	public float jumpForce;
	public float accelSlowdown;
	public float landingTolerance;
	public float sprintMultiplier;
	public int waterJumpFrames;

	private bool isJumping;
	private Vector2 lastVelocity;
	private bool canStartSprint;
	private bool isInWater;
	private float nextWaterJump;
    private SpriteRenderer sprite;

    private void Awake()
    {
        Debug.Log(File.Exists(Application.persistentDataPath + "/gamesave.save"));
        if (File.Exists(Application.persistentDataPath + "/gamesave.save"))
        {
            Debug.Log("Reading Save File");
            // 2
            // player = GameObject.FindGameObjectWithTag("Player");
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/gamesave.save", FileMode.Open);
            Save save = (Save)bf.Deserialize(file);
            file.Close();

            // 3

            float xPosition = save.xSpawnPosition;
            float yPosition = save.ySpawnPosition;
            Debug.Log(xPosition);
            Debug.Log(yPosition);

            gameObject.GetComponent<Transform>().position = new Vector3(xPosition, yPosition, 0);
            Debug.Log("Game Loaded");

            //Unpause();
        }
        else
        {
            Debug.Log("No game saved!");
        }
    }

    void Start () {
		isJumping = false;
		lastVelocity = new Vector2(0.0f, 0.0f);
		canStartSprint = true;
        sprite = GetComponent<SpriteRenderer>();
    }

	void FixedUpdate () {
		Rigidbody2D rb = GetComponent<Rigidbody2D> ();
        
		horizontalMovement (rb);
		verticalMovement (rb);

	}

	void horizontalMovement (Rigidbody2D rb) {
		float momentum = xSpeed;

		float moveHorizontal = Input.GetAxis ("Horizontal");

		//float sprintHorizontal = Input.GetAxis ("Sprint");
		float sprintHorizontal = 0;

		if (isJumping || isInWater) {
			//if (sprintHorizontal == 0)
			canStartSprint = false;
			if (rb.velocity.x == 0) {
				momentum = 0;
				if (Input.GetButton ("Horizontal"))
					momentum = moveHorizontal;
			}
		} else
			canStartSprint = true;

        if (moveHorizontal < 0)
        {
            sprintHorizontal = -sprintHorizontal;
            sprite.flipX = false;
            //momentum = -xSpeed;
        }
        else if (moveHorizontal == 0) sprite.flipX = sprite.flipX;
        else sprite.flipX = true;

        

		if (!(Input.GetButton("Horizontal") && canStartSprint) || isInWater)
			sprintHorizontal = 0;

		Vector2 movement = new Vector2 (
			(moveHorizontal * momentum) + (sprintHorizontal * sprintMultiplier), 
			rb.velocity.y);
		
		rb.velocity = movement;


	}

	void verticalMovement(Rigidbody2D rb) {

		float acceleration = (rb.velocity.y - lastVelocity.y) / Time.fixedDeltaTime;
		lastVelocity = rb.velocity;

        
		if (isJumping && -landingTolerance <= rb.velocity.y && rb.velocity.y <= landingTolerance) {
			Debug.Log (isJumping);
			isJumping = false;
		}

		else if (Input.GetButton("Jump")){
			if (isInWater) {
				if (Time.time > nextWaterJump) {
					nextWaterJump = Time.time + (waterJumpFrames / 60f);
					rb.velocity = new Vector2 (rb.velocity.x, 0);
					rb.AddForce (new Vector2 (0.0f, jumpForce));
				}

			} else {

				if (!isJumping && -landingTolerance <= acceleration && acceleration <= landingTolerance) {
					
					rb.velocity = new Vector2 (rb.velocity.x, 0);
					rb.AddForce (new Vector2 (0.0f, jumpForce));
					isJumping = true;
				}


				else if (isJumping && acceleration < 0 && rb.velocity.y > 0) {
					rb.velocity = new Vector2 (rb.velocity.x, rb.velocity.y * (1+accelSlowdown));
				}
			}
		}
			
}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.tag == "Water") {

			isInWater = true;
		}

	}

	void OnTriggerExit2D(Collider2D other){
		if (other.tag == "Water") {
			isInWater = false;
		}

	}



}
