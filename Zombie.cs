using Godot;
using System;

public partial class Zombie : CharacterBody3D, IDamageable
{
	public float MovementSpeed = 1.0f;
	private NavigationAgent3D NavAgent => GetNode<NavigationAgent3D>("NavigationAgent3D");

	public float DamageMultiplier { get; } = 1.0f;
	public bool IsDamageRoot { get; } = true;
	public float Health { get; private set; } = 1000.0f;

	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		Node3D player = (Node3D)GetTree().GetFirstNodeInGroup("player");
		var targetPos = player.GlobalPosition;
		// GD.Print($"target pos: {targetPos}");
		NavAgent.SetTargetPosition(targetPos);
	}

	public override void _PhysicsProcess(double delta)
	{
		Node3D player = (Node3D)GetTree().GetFirstNodeInGroup("player");
		var targetPos = player.GlobalPosition;
		// GD.Print($"target pos: {targetPos}");
		NavAgent.SetTargetPosition(targetPos);

		if (NavAgent.IsNavigationFinished())
		{
			GD.Print("navigation finished");
			return;
		}

		var nextPos = NavAgent.GetNextPathPosition();
		// GD.Print($"next pos: {nextPos}");
		var newVelocity = GlobalPosition.DirectionTo(nextPos) * MovementSpeed;
		// GD.Print($"new velocity: {newVelocity}");
		NavAgent.Velocity = newVelocity;
		Velocity = newVelocity;
		MoveAndSlide();
	}

	public void DoDamage(float damage)
	{
		Health -= damage;
		GD.Print($"zombie health now: {Health}");
	}
}

public interface IDamageable
{
	public float DamageMultiplier { get; }
	public bool IsDamageRoot { get; }
	public float Health { get; }
	public void DoDamage(float damage);
}
