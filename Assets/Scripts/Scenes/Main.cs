using Godot;
using System;

public class Main : Node
{
	// Nodes
	public Spatial Environment;
	public WorldEnvironment WorldEnv;
	public Interface Interface;
	public PhysicsDirectSpaceState SpaceState;

    public override void _Ready()
    {
		// Initialize game components
		GameManager.Instance.MainNode = this;

		// Get Nodes
		Environment = GetNode("Environment") as Spatial;
		WorldEnv = GetNode("Environment/WorldEnvironment") as WorldEnvironment;
		Interface = GetNode("Interface") as Interface;

		// Node is ready!
		GD.print("Main scenes is ready.");
    }

	public override void _Input(InputEvent @ie)
	{
		// Key Event
		if (ie is InputEventKey && ((InputEventKey)ie).Pressed)
		{
			// Scancode
			int ScanCode = ((InputEventKey)ie).Scancode;

			// Exit game
			if (ScanCode == GD.KEY_ESCAPE)
			{
				GetTree().Quit();
			}

			// Fullscreen
			if (ScanCode == GD.KEY_F1)
			{
				OS.SetWindowFullscreen(!OS.IsWindowFullscreen());
			}
		}
	}
}
