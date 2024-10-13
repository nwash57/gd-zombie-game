using Godot;
using System;

public partial class Zombie : CharacterBody3D
{
	public float MovementSpeed = 1.0f;
	private NavigationAgent3D NavAgent => GetNode<NavigationAgent3D>("NavigationAgent3D");

	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		Node3D player = (Node3D)GetTree().GetFirstNodeInGroup("player");
		var targetPos = player.GlobalPosition;
		GD.Print($"target pos: {targetPos}");
		NavAgent.SetTargetPosition(targetPos);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	public override void _PhysicsProcess(double delta)
	{
		Node3D player = (Node3D)GetTree().GetFirstNodeInGroup("player");
		var targetPos = player.GlobalPosition;
		GD.Print($"target pos: {targetPos}");
		NavAgent.SetTargetPosition(targetPos);

		if (NavAgent.IsNavigationFinished())
		{
			GD.Print("navigation finished");
			return;
		}

		var nextPos = NavAgent.GetNextPathPosition();
		GD.Print($"next pos: {nextPos}");
		var newVelocity = GlobalPosition.DirectionTo(nextPos) * MovementSpeed;
		GD.Print($"new velocity: {newVelocity}");
		// NavAgent.Velocity = newVelocity;
		Velocity = newVelocity;
		MoveAndSlide();
	}
}
