using Godot;
using System;

public partial class Damageable : Node3D, IDamageable
{
	[Export]
	public float DamageMultiplier { get; set; } = 1.0f;

	[Export] public bool IsDamageRoot { get; set; } = false;

	[Export] public float MaxHealth { get; set; } = 100f;

	[Export] public float Health { get; set; } = 100f;


	public void DoDamage(float damage)
	{
		Health -= damage;

		if (!IsDamageRoot)
		{
			var parent = GetParent();
			while (parent != null && parent is not IDamageable)
			{
				parent = parent.GetParent();
			}

			if (parent is IDamageable parentDamageable)
			{
				parentDamageable.DoDamage(damage * DamageMultiplier);
			}
		}
	}
}
