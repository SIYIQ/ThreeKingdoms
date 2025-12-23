using UnityEngine;

// 简单的 3D 玩家控制：按 D 向前（右）移动，使用 Rigidbody 移动以触发碰撞
[RequireComponent(typeof(Rigidbody))]
public class Player3DController : MonoBehaviour
{
	public float moveSpeed = 3f;
	private Rigidbody rb;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		rb.constraints = RigidbodyConstraints.FreezeRotation;
	}

	private void FixedUpdate()
	{
		Vector3 dir = Vector3.zero;
		if (Input.GetKey(KeyCode.D)) dir += Vector3.right;
		if (Input.GetKey(KeyCode.A)) dir += Vector3.left;
		if (dir != Vector3.zero)
		{
			Vector3 newPos = rb.position + dir.normalized * moveSpeed * Time.fixedDeltaTime;
			rb.MovePosition(newPos);
		}
	}
}


