using Godot;
using System;

public class Interface : Control
{
	// Nodes
	Label DebugText;

	Label HealthText;
	Label AmmoText;

    public override void _Ready()
    {
		DebugText = GetNode("DebugText") as Label;

		HealthText = GetNode("HealthBar/txtHealth") as Label;
		AmmoText = GetNode("AmmoBar/txtClip") as Label;
    }

	public override void _Process(float delta)
	{
		float fps = Engine.GetFramesPerSecond();
		DebugText.Text = fps.ToString() + " fps\n" + (((1.0f/fps) * 1000)).ToString("0.0") + " ms";
	}

	public void SetHealth(int health)
	{
		HealthText.Text = health.ToString("000");
	}

	public void SetAmmo(int ammo)
	{
		AmmoText.Text = ammo.ToString("00");
	}
}
