using Godot;
using System;

public class CameraFirstPerson : Camera
{
	// Variables
	float ZNear;
	float ZFar;
	float CamFov;
	float CurFov;

    public override void _Ready()
    {
		ZNear = GetZnear();
		ZFar = GetZfar();
		CamFov = CurFov = GetFov();
    }

	public override void _Process(float delta)
	{
		if (!IsCurrent())
			return;
		if (Mathf.abs(CurFov - CamFov) > 0.1)
		{
			CurFov = Mathf.lerp(CurFov, CamFov, 8 * delta);
			SetPerspective(CurFov, ZNear, ZFar);
		}
	}

	public void SetFov(float fov)
	{
		if (!IsCurrent())
			return;
		
		CamFov = fov;
	}
}
