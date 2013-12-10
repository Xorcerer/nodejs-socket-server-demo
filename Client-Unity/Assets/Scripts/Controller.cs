using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour
{
	public GameObject Player;
	public float Speed = 10;

	public const int LeftButton = 0;
	public const int RightButton = 1;

	// Update is called once per frame
	void Update ()
	{
		if (Input.GetMouseButton (LeftButton)) {
			// TODO: 1, Set correct position
			// TODO: 2, move with Speed, not instantly.
			var mousePosition = Input.mousePosition;
			var newPosition = new Vector3(mousePosition.x, mousePosition.y, Player.transform.position.z);
			Player.transform.position = newPosition;
		}
	}
}
