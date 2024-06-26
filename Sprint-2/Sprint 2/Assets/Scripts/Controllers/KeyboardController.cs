using System;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class KeyboardController : BaseController
{
	public float Speed = 10;
	public Direction LatestDirection = Direction.S;
	public Direction CurrentDirection = Direction.S;

	[SerializeField] Character Character;
	[SerializeField] int StartPositionX;
	[SerializeField] int StartPositionY;
	PlayerAnimation PlayerAnimation;

	private void Start()
	{
		PlayerAnimation = Character.GetComponent<PlayerAnimation>();
	}

	private void Update()
	{
		if(Character.ActiveTile == null)
		{
			Character.ActiveTile = MapManager.Instance.Map[new Vector2Int(StartPositionX, StartPositionY)];
			PlayerAnimation.Play("IdleBottom");
		}
	}

	void FixedUpdate()
	{
		var horizontal = Input.GetAxis("Horizontal") * Speed;
		var vertical = Input.GetAxis("Vertical") * (Speed - 1);

		Walk(new Vector2(horizontal, vertical));
	}

	Direction GetDirection(Vector2 direction)
	{
		float step = 360 / 8;
		float offset = step / 2;
		float angle = Vector2.SignedAngle(Vector2.up, direction.normalized);

		angle += offset;
		if (angle < 0) angle += 360;

		float stepCount = angle / step;

		return (Direction)Enum.ToObject(typeof(Direction), Mathf.FloorToInt(stepCount));
	} 

	void Walk(Vector2 direction)
	{
		Character.GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.CeilToInt(direction.x), Mathf.CeilToInt(direction.y));
		var worldPosition = MapBuilder.Instance.Tilemap.WorldToCell(new Vector3(Character.transform.position.x, Character.transform.position.y, 0));

		Character.GetComponent<CharacterRenderer>().RenderOnTile(MapManager.Instance.Map[new Vector2Int(worldPosition.x, worldPosition.y)]);

		var latestDirection = GetDirection(direction);

		if ((direction.magnitude/Speed) > 0.001)
			CurrentDirection = latestDirection;

		if ((direction.magnitude/Speed) > 0.5)
			LatestDirection = latestDirection;

		PlayerAnimation.Play(
			direction.magnitude < 0.001 
			? GetIdleAnimation(LatestDirection)
			: GetWalkAnimation(latestDirection)
		);

	}

	string GetWalkAnimation(Direction direction) => direction switch
	{
		Direction.N => "WalkUp",
		Direction.NW => "WalkUpLeft",
		Direction.NE => "WalkUpRight",
		Direction.S => "WalkBottom",
		Direction.SE => "WalkBottomRight",
		Direction.SW => "WalkBottomLeft",
		Direction.E => "WalkRight",
		Direction.W => "WalkLeft",
		_ => "IdleBottom"
	};

	string GetIdleAnimation(Direction direction) => direction switch
	{
		Direction.N => "IdleUp",
		Direction.NW => "IdleUpLeft",
		Direction.NE => "IdleUpRight",
		Direction.S => "IdleBottom",
		Direction.SE => "IdleBottomRight",
		Direction.SW => "IdleBottomLeft",
		Direction.E => "IdleRight",
		Direction.W => "IdleLeft",
		_ => "IdleBottom"
	};
}
