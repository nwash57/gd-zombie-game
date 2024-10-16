using Godot;
using System;
using System.Linq;

public partial class Player : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	public const float MouseSensitivity = 0.001f;

	public const float BobFrequency = 1.5f;
	public const float BobAmplitude = 0.05f;
	public double BobPhase = 0.0;

	// public Node3D Head => GetNode<Node3D>("Head");
	public Camera3D Camera => GetNode<Camera3D>("Head/Camera3D");
	public Node3D Weapon => GetNode<Node3D>("Weapon");
	public RayCast3D WeaponRay => Weapon.GetChildren().First().GetNode<RayCast3D>("WeaponRay");
	public RayCast3D CameraRay => Camera.GetNode<RayCast3D>("RayCast3D");

	public bool IsSprinting => Input.IsActionPressed("sprint");
	public float SprintMultiplier = 1.5f;

	public float WeaponSpeed = 1.0f;

	[Export] public PackedScene BulletScene;

	public override void _Ready()
	{
		Input.SetMouseMode(Input.MouseModeEnum.Captured);
		GetNode<Camera3D>("Head/Camera3D").Current = true;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion ev)
		{
			RotateY(-ev.Relative.X * MouseSensitivity);
			// Head.RotateY(-ev.Relative.X * MouseSensitivity);
			Camera.RotateX(-ev.Relative.Y * MouseSensitivity);
			Camera.Rotation = new Vector3(
				Mathf.Clamp(Camera.Rotation.X, Mathf.DegToRad(-40), Mathf.DegToRad(60)),
				Camera.Rotation.Y,
				Camera.Rotation.Z);

		}
		else if (@event is InputEventMouseButton mouseButton)
		{
			if (@event.IsActionPressed("shoot"))
			{
				var bullet = BulletScene.Instantiate() as Bullet;
				GetTree().Root.GetNode("World").AddChild(bullet);
				bullet.GlobalPosition = WeaponRay.GlobalPosition;

				bullet.LookAt(WeaponRay.ToGlobal(WeaponRay.TargetPosition));
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		// Vector3 direction = (Head.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed * (IsSprinting ? SprintMultiplier : 1.0f);
			velocity.Z = direction.Z * Speed * (IsSprinting ? SprintMultiplier : 1.0f);
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		BobPhase += delta * Velocity.Length() * BobFrequency * (IsOnFloor() ? 1.0f : 0.1f);
		var camTransform = Camera.Transform;
		camTransform.Origin = new Vector3(
			Camera.Transform.Origin.X,
			HeadBob(BobPhase).Y,
			Camera.Transform.Origin.Z);
		Camera.Transform = camTransform;

		if (CameraRay.IsColliding())
		{
			var collisionPoint = CameraRay.GetCollisionPoint();
			Weapon.LookAt(collisionPoint);
		}


		Velocity = velocity;
		MoveAndSlide();
	}

	public Vector3 HeadBob(double time)
	{
		var pos = Vector3.Zero;
		pos.Y = Mathf.Sin((float)time * BobFrequency) * BobAmplitude;
		return pos;
	}
}
