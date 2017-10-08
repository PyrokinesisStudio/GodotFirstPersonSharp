using Godot;
using System;

public class PlayerViewModel : Spatial
{
	// Nodes
	Spatial ZoomOrigin;
	AnimationPlayer WeaponAnimation;

	// Variables
	public bool IsBobbing = false;
	public bool IsAiming = false;

	// Weapon data
	string WpnAnim = "";
	string WpnCurAnim = "";

	// Other variables
	float BobAngle = 0.0f;
	float BobSpeed = 1.3f;

	Vector2 AimMotion = new Vector2();
	Vector2 BobForce = new Vector2();
	Vector3 AimOrigin = new Vector3();

    public override void _Ready()
    {
		// Get nodes
		ZoomOrigin = GetNode("ZoomOrigin") as Spatial;
		WeaponAnimation = GetNode("Weapon").FindNode("AnimationPlayer") as AnimationPlayer;
    }

	public override void _Process(float delta)
	{
		Vector2 bobOrigin = new Vector2();

		if (IsBobbing)
		{
			var bobSpeed = BobSpeed;
			if (IsAiming)
				bobSpeed *= 0.4f;
			
			BobAngle = (BobAngle + (360.0f * delta * bobSpeed)) % 360.0f;
			bobOrigin.x = Mathf.sin(Mathf.deg2rad(BobAngle)) * 0.02f;
			bobOrigin.y = ((Mathf.cos(Mathf.deg2rad(BobAngle * 2.0f)) + 1.0f) / 2.0f) * 0.01f;

			if (IsAiming)
				bobOrigin *= 0.2f;
		}
		else
		{
			BobAngle = (BobAngle + (120.0f * delta)) % 360.0f;
			bobOrigin.x = 0.0f;
			bobOrigin.y = ((Mathf.cos(Mathf.deg2rad(BobAngle)) + 1.0f) / 2.0f) * 0.004f;
		}

		BobForce = BobForce.linear_interpolate(new Vector2(), 5 * delta);
		AimMotion = AimMotion.linear_interpolate(bobOrigin, 5 * delta) + BobForce;

		// New translation for viewmodel
		Vector3 viewOrigin = new Vector3(AimMotion.x, AimMotion.y, 0.0f);

		// Move the view origin if aiming
		if (IsAiming && ZoomOrigin != null)
			viewOrigin -= ZoomOrigin.GetTranslation();
		
		SetTranslation(GetTranslation().linear_interpolate(viewOrigin, 8 * delta));

		// Set weapon animation
		if (WpnCurAnim != WpnAnim)
			PlayWpnAnimation(WpnAnim);
	}

	void PlayWpnAnimation(string newAnim, float speed = 1.0f)
	{
		WpnCurAnim = newAnim;
		WeaponAnimation.Play(newAnim, -1, speed);
	}

	// Public functions //

	public void AddMotion(Vector2 dir)
	{
		Vector2 targetMotion = new Vector2();
		targetMotion.x = Mathf.clamp(AimMotion.x + (dir.x * 0.0002f), -0.02f, 0.02f);
		targetMotion.y = Mathf.clamp(AimMotion.y + (dir.y * 0.0002f), -0.02f, 0.02f);

		if (IsAiming)
			targetMotion *= 0.5f;

		AimMotion = AimMotion.linear_interpolate(targetMotion, 0.5f);
	}

	public void AddForce(Vector2 dir)
	{
		if (IsAiming)
			dir *= 0.5f;
		BobForce += dir;
	}

	public void SetWpnAnimation(string anim, bool immediately = false)
	{
		WpnAnim = anim;

		if (immediately)
		{
			PlayWpnAnimation(anim);
		}
	}
}
