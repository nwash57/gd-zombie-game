using Godot;
using System;

public partial class Bullet : CsgBox3D
{
	public float Velocity = 0;

	[Export] public float StartVelocity = 710.0f;
	[Export] public float YAccel = -9.8f;
	[Export] public float Mass = 7.9f;
	[Export] public float AirDensity = 1.225f;
	[Export] public float DragCoefficient = 0.3f;
	[Export] public float CrossSectionArea = 0.0000456f; // in m^2

	public float CalculatedDrag => 0.5f * AirDensity * DragCoefficient * CrossSectionArea;

	private float _cachedDrag;
	private Vector3? _lastFramePos;

	protected Marker3D FrontMarker => GetNode<Marker3D>("FrontMarker");
	protected RayCast3D HitRay => GetNode<RayCast3D>("HitRay");

	protected Vector3 Forward => -GlobalTransform.Basis.Z;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_cachedDrag = CalculatedDrag;
		Velocity = StartVelocity;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _PhysicsProcess(double delta)
	{
		_lastFramePos ??= GlobalPosition;

		var movement = Velocity * (float)delta;
		GlobalPosition += Forward * movement;
		HitRay.GlobalPosition = _lastFramePos.Value;
		HitRay.TargetPosition = new Vector3(0, 0, -movement);

		var newPos = GlobalPosition + (0.001f * Forward);
		_lastFramePos = newPos;

		if (HitRay.IsColliding())
		{
			GD.Print("detected bullet hit");
			var collider = HitRay.GetCollider();

			if (collider is IDamageable damageable)
			{
				GD.Print("hit a damageable");
				var damage = Mass * Velocity / 100;
				damageable.DoDamage(damage);
			}

			QueueFree();
		}

		if (GlobalPosition.Length() > 99999)
		{
			QueueFree();
		}
	}

	private float DragForce()
	{
		return (float)(_cachedDrag * Math.Pow(Velocity, 2));
	}

	private float AccelerationDueToDrag() => DragForce() / Mass;
}
