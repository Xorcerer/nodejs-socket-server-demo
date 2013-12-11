using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour
{
	public GameObject Player;
	public float Speed = 0.1f;

	public const int LeftButton = 0;
	public const int RightButton = 1;

	Vector3 dest;

	void Start ()
	{
		Player.rigidbody2D.mass = 0f;
	}

	// Update is called once per frame
	void Update ()
	{
		if (Input.GetMouseButton (LeftButton)) {
			dest = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			dest.z = Player.transform.position.z;

			var diff = dest - Player.transform.position;
			var speed = new Vector2(diff.x, diff.y);

			Player.rigidbody2D.velocity = speed.normalized * Speed;
		}

		if ((Player.transform.position - dest).sqrMagnitude < (Speed / 60f))
		{
			Player.transform.position = dest;
			Player.rigidbody2D.velocity = Vector2.zero;
		}
	}
}
