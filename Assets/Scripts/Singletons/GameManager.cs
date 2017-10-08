using Godot;
using System;

public class GameManager : Node
{
	// Singleton instance
	public static GameManager Instance;

	// Nodes
	public Main MainNode;

	public override void _Ready()
	{
		GameManager.Instance = this;
	}
}
