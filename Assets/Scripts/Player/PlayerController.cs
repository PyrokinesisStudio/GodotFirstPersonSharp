using Godot;
using System;
using System.Collections.Generic;

public class PlayerController : RigidBody
{
    // Consts
	const float MoveAcceleration = 8.0f;
	const float JumpForce = 4.0f;

	// Nodes
	Spatial BodyNode;
	CameraFirstPerson CameraNode;
	RayCast FloorRay;
	PlayerViewModel ViewModel;
	RayCast FiringRaycast;

	// Player variables
	int MaxHealth = 100;
	int CurHealth;

	// Weapon data
	int MaxAmmo = 20;
	int CurAmmo;

	// Local variables
	bool IsMoving = false;
	bool IsJumpJustPressed = false;
	bool IsFiring = false;
	bool IsReloading = false;
	bool IsAiming = false;

	float NextThink = 0.0f;

    public override void _Ready()
    {
		// Get nodes
		BodyNode = GetNode("Body") as Spatial;
		CameraNode = BodyNode.GetNode("Camera") as CameraFirstPerson;
		ViewModel = CameraNode.GetNode("ViewModel") as PlayerViewModel;

		// Firing Raycast
		FiringRaycast = CameraNode.GetNode("FiringRaycast") as RayCast;
		FiringRaycast.Enabled = true;
		FiringRaycast.AddException(this);
		FiringRaycast.CastTo = new Vector3(0, 0, -100.0f);

		// FloorRay setup
		FloorRay = GetNode("FloorRay") as RayCast;
		FloorRay.AddException(this);

		// Set mouse mode to captured
		Input.SetMouseMode(Input.MOUSE_MODE_CAPTURED);

		// Set default vars
		ResetVars();
    }

	void ResetVars()
	{
		IsMoving = false;
		IsJumpJustPressed = false;
		IsFiring = false;
		IsReloading = false;
		IsAiming = false;

		NextThink = 0.0f;

		CurHealth = MaxHealth;
		CurAmmo = MaxAmmo;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion)
		{
			float Sensitivity = 0.3f;
			if (IsAiming)
			{
				Sensitivity *= 0.5f;
			}

			Vector3 BodyRotation = BodyNode.RotationDeg;
			BodyRotation.y = ((BodyRotation.y - ((InputEventMouseMotion)@event).Relative.x * Sensitivity) % 360.0f);

			Vector3 CameraRotation = CameraNode.RotationDeg;
			CameraRotation.x = Mathf.clamp(CameraRotation.x - ((InputEventMouseMotion)@event).Relative.y * Sensitivity, -89.0f, 89.0f);

			BodyNode.RotationDeg = BodyRotation;
			CameraNode.RotationDeg = CameraRotation;

			ViewModel.AddMotion(((InputEventMouseMotion)@event).Relative);
		}

		if (@event is InputEventMouseButton)
		{
			if (((InputEventMouseButton)@event).ButtonIndex == GD.BUTTON_RIGHT)
			{
				ToggleAim(((InputEventMouseButton)@event).Pressed);
			}

			if (((InputEventMouseButton)@event).ButtonIndex == GD.BUTTON_LEFT)
			{
				IsFiring = ((InputEventMouseButton)@event).Pressed;
			}
		}
	}

	public override void _Process(float delta)
	{
		NextThink = Mathf.max(NextThink - delta, 0.0f);

		Main mainNode = GameManager.Instance.MainNode;

		if (mainNode != null)
		{
			mainNode.Interface.SetHealth(CurHealth);
			mainNode.Interface.SetAmmo(CurAmmo);
		}
	}

	public override void _PhysicsProcess(float delta)
	{
		MovementLogic(delta);

		if (IsMoving && !IsFiring)
			ViewModel.IsBobbing = true;
		else
			ViewModel.IsBobbing = false;

		CheckFiring();
		CheckReload();
	}

	void MovementLogic(float delta)
	{
		Vector3 MovementDir = new Vector3();
		Basis AimBasis = CameraNode.GlobalTransform.basis;

		// Forward
		if (Input.IsKeyPressed(GD.KEY_W))
			MovementDir -= AimBasis.get_axis(2);

		// Backward
		if (Input.IsKeyPressed(GD.KEY_S))
			MovementDir += AimBasis.get_axis(2);

		// Left
		if (Input.IsKeyPressed(GD.KEY_A))
			MovementDir -= AimBasis.get_axis(0);

		// Right
		if (Input.IsKeyPressed(GD.KEY_D))
			MovementDir += AimBasis.get_axis(0);

		// Normalize the direction
		MovementDir.y = 0.0f;
		MovementDir = MovementDir.normalized();

		// Motion
		float MoveSpeed = 4.0f;
		Vector3 MoveMotion = MovementDir * MoveSpeed;

		MoveMotion.y = LinearVelocity.y;
		MoveMotion = LinearVelocity.linear_interpolate(MoveMotion, MoveAcceleration * delta);

		// Is colliding with floor
		bool OnFloor = FloorRay.IsColliding();

		// Jump check
		if (Input.IsKeyPressed(GD.KEY_SPACE) && !IsJumpJustPressed)
		{
			if (OnFloor)
			{
				// Jump!
				MoveMotion.y = JumpForce;
				ViewModel.AddForce(new Vector2(0.0f, -0.006f));
			}

			IsJumpJustPressed = true;
		}

		// Reset key
		if (IsJumpJustPressed && !Input.IsKeyPressed(GD.KEY_SPACE))
			IsJumpJustPressed = false;


		// Set the velocity
		LinearVelocity = MoveMotion;

		// Check whether the player is moving
		IsMoving = (Mathf.abs(LinearVelocity.x) >= 0.2f || Mathf.abs(LinearVelocity.z) >= 0.2f);
	}

	void CheckReload()
	{
		if (NextThink > 0.0f || CurAmmo >= MaxAmmo)
			return;

		if (!IsReloading)
		{
			if (Input.IsKeyPressed(GD.KEY_R) || CurAmmo <= 0)
			{
				IsReloading = true;
				NextThink = 1.5f;

				if (IsAiming)
					ToggleAim(false);

				ViewModel.SetWpnAnimation("reload", true);
			}
		}
		else
		{
			NextThink = 0.1f;
			IsReloading = false;
			CurAmmo = MaxAmmo;
		}
	}

	void CheckFiring()
	{
		if (!IsFiring || IsReloading || CurAmmo <= 0 || NextThink > 0.0)
			return;
		
		Vector3 from = CameraNode.GlobalTransform.origin;
		Vector3 to = from + ((-CameraNode.GlobalTransform.basis[2]).normalized() * 100.0f);

		ViewModel.SetWpnAnimation("shoot", true);
		CurAmmo = Mathf.min(Mathf.max(CurAmmo - 1, 0), MaxAmmo);
		NextThink = 1.0f / 10.0f;

		if (FiringRaycast.IsColliding())
		{
			Godot.Object collider = FiringRaycast.GetCollider();
			GD.print("Collider: ", collider.GetClass());
		}
	}

	public void ToggleAim(bool aim)
	{
		if (aim)
		{
			if (!IsReloading)
			{
				CameraNode.SetFov(35.0f);
				ViewModel.IsAiming = true;
				IsAiming = true;
			}
		}
		else
		{
			CameraNode.SetFov(60.0f);
			ViewModel.IsAiming = false;
			IsAiming = false;
		}
	}
}
