using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcController : MonoBehaviour
{
	public float moveSpeed = 10f;
	public float smoothTime = 0.1f;
	public Transform target;

	private Vector3 lastDirectionToTarget;
	private Rigidbody rb;

	void Start() {
		rb = GetComponent<Rigidbody>();
		lastDirectionToTarget = transform.forward;
	}

	void FixedUpdate() {
		var grabityUp = (transform.position - FauxGravityAttractor.instance.transform.position).normalized; 
		var directionToTarget = Vector3.ProjectOnPlane(target.position - transform.position, grabityUp).normalized;
		Vector3 vel = Vector3.zero;
		lastDirectionToTarget = Vector3.SmoothDamp(lastDirectionToTarget, directionToTarget, ref vel, smoothTime).normalized;
		lastDirectionToTarget = Vector3.ProjectOnPlane(lastDirectionToTarget, grabityUp).normalized;
		rb.MovePosition(rb.position + lastDirectionToTarget * moveSpeed * Time.fixedDeltaTime);
		transform.forward = directionToTarget;
	}

	public void OnCollisionEnter(Collision collision) {
		if (collision.collider.CompareTag("Player")) {
			WeatherManager.Instance.ApplyDamage();
		}
	}

}
