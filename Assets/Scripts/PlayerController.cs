using UnityEngine;

// 简单的玩家控制：按 D 向右移动
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
	public float moveSpeed = 3f;
	private Rigidbody2D rb;

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		// 将刚体设置为可被 kinematic 移动，使用 MovePosition
		rb.bodyType = RigidbodyType2D.Kinematic;
	}

	private void FixedUpdate()
	{
		float h = 0f;
		if (Input.GetKey(KeyCode.D)) h = 1f;
		if (Input.GetKey(KeyCode.A)) h = -1f;
		if (h != 0f)
		{
			Vector2 newPos = rb.position + Vector2.right * h * moveSpeed * Time.fixedDeltaTime;
			rb.MovePosition(newPos);
		}
	}
}


