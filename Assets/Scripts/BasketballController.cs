using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketballController : MonoBehaviour {

    // Movement speed of the player
    public float MoveSpeed = 10;

    // References to various transforms in the scene
    public Transform Ball;          // The basketball
    public Transform PosDribble;    // Position for dribbling
    public Transform PosOverHead;   // Position for holding over head
    public Transform Arms;          // The arms of the player
    public Transform Target;        // Target position for throwing the ball

    // Boolean flags to track ball state
    private bool IsBallInHands = true; // Is the ball in the player's hands?
    private bool IsBallFlying = false;  // Is the ball flying through the air?

    private float Time = 0;  // Time variable for ball trajectory

    // Update is called once per frame
    void Update() {

        // Player movement
        Vector3 direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        transform.position += direction * MoveSpeed * UnityEngine.Time.deltaTime;
        transform.LookAt(transform.position + direction);

        // Ball control logic
        if (IsBallInHands) {

            // Hold the ball over head if Space key is pressed
            if (Input.GetKey(KeyCode.Space)) {
                Ball.position = PosOverHead.position;  // Set ball position to over head
                Arms.localEulerAngles = Vector3.right * 180;  // Rotate arms for holding over head

                // Look towards the target position
                transform.LookAt(Target.parent.position);
            }

            // Dribble the ball if Space key is not pressed
            else {
                Ball.position = PosDribble.position + Vector3.up * Mathf.Abs(Mathf.Sin(UnityEngine.Time.time * 5)); // Add vertical motion for dribbling
                Arms.localEulerAngles = Vector3.right * 0;  // Reset arm rotation for dribbling
            }

            // Throw the ball if Space key is released
            if (Input.GetKeyUp(KeyCode.Space)) {
                IsBallInHands = false;  // Ball is no longer in hands
                IsBallFlying = true;    // Ball is now flying
                Time = 0;                  // Reset time variable for ball trajectory
            }
        }

        // Ball movement in the air
        if (IsBallFlying) {
            Time += UnityEngine.Time.deltaTime;  // Increment time
            float duration = 0.66f;  // Duration of ball trajectory
            float t01 = Time / duration;  // Normalized time

            // Move the ball towards the target position
            Vector3 A = PosOverHead.position;  // Start position
            Vector3 B = Target.position;        // End position
            Vector3 pos = Vector3.Lerp(A, B, t01); // Interpolate position

            // Add arc to the ball trajectory
            Vector3 arc = Vector3.up * 5 * Mathf.Sin(t01 * 3.14f); // Calculate arc motion

            Ball.position = pos + arc;  // Update ball position

            // Check if ball has reached the target position
            if (t01 >= 1) {
                IsBallFlying = false;  // Ball is no longer flying
                Ball.GetComponent<Rigidbody>().isKinematic = false;  // Enable physics for ball
            }
        }
    }

    // OnTriggerEnter is called when the Collider other enters the trigger
    private void OnTriggerEnter(Collider other) {
        // Check if the ball is not in hands and not flying
        if (!IsBallInHands && !IsBallFlying) {

            IsBallInHands = true;  // Ball is back in hands

            Ball.GetComponent<Rigidbody>().isKinematic = true;  // Disable physics for ball
            GameManager.instance.Score++;  // Increment score using GameManager instance
        }
    }
}
