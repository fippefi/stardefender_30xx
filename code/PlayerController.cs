using System;
using System.Numerics;

public sealed class PlayerController : Component
{
	[Property] private GameObject model { get; set; }
	[Property] private GameObject cam;
	[Property] private GameObject cameraRig;
	[Property] private GameObject mouseAim;
	[Property] public float drag;
	[Property] public float slideDrag;
	[Property] public float speed;
	[Property] private float camSmoothSpeed = 5f;
	[Property] private float mouseSensitivity = 5f;
	[Property] private float aimDistance = 500f;
	private Vector3 frozenDirection = Vector3.Forward;
	private bool isMouseAimFrozen = false;
	private bool useFixed = true; //Fixedupdate y/n
	private Vector3 movement;

	public Vector3 BoresightPos
	{
		get
		{
			return (model.Transform.Rotation.Forward * aimDistance) + model.Transform.Position;
		}
	}

	public Vector3 MouseAimPos
	{
		get
		{
			if (mouseAim != null)
			{
				return isMouseAimFrozen
					? GetFrozenMouseAimPos()
					: mouseAim.Transform.Position + (mouseAim.Transform.Rotation.Forward * aimDistance);
			}
			else
			{
				return Transform.Rotation.Forward * aimDistance;
			}
		}
	}

	protected override void OnEnabled()
	{
		if (model == null)
			Log.Error("MouseFlightController - No aircraft transform assigned!");
		if (mouseAim == null)
			Log.Error("MouseFlightController - No mouse aim transform assigned!");
		if (cameraRig == null)
			Log.Error("MouseFlightController - No camera rig transform assigned!");
		if (cam == null)
			Log.Error("MouseFlightController - No camera transform assigned!");

		// To work correctly, the entire rig must not be parented to anything.
		// When parented to something (such as an aircraft) it will inherit those
		// rotations causing unintended rotations as it gets dragged around.
		Transform.Parent.Clear();
	}

	protected override void OnUpdate()
	{
		if (useFixed == false)
			UpdateCameraPos();

		RotateRig();
	}

	protected override void OnFixedUpdate()
	{
		if (useFixed == true)
			UpdateCameraPos();
	}

	private void RotateRig()
	{
		if (mouseAim == null || cam == null || cameraRig == null)
			return;

		// Freeze the mouse aim direction when the free look key is pressed.
		if (Input.Down("View"))
		{
			isMouseAimFrozen = true;
			frozenDirection = mouseAim.Transform.Rotation.Forward;
		}
		else if  (!Input.Down("View"))
		{
                isMouseAimFrozen = false;
		}

		// Mouse input.
		float mouseX = Input.MouseDelta.x * mouseSensitivity;
		float mouseY = -Input.MouseDelta.y * mouseSensitivity;

		// Rotate the aim target that the plane is meant to fly towards.
		// Use the camera's axes in world space so that mouse motion is intuitive.
		// mouseAim.Rotate(cam.right, mouseY, Space.World);
		// mouseAim.Rotate(cam.up, mouseX, Space.World);



		// The up vector of the camera normally is aligned to the horizon. However, when
		// looking straight up/down this can feel a bit weird. At those extremes, the camera
		// stops aligning to the horizon and instead aligns to itself.
		Vector3 upVec = (MathF.Abs(mouseAim.Transform.Rotation.Forward.y) > 0.9f) ? cameraRig.Transform.Rotation.Up : Vector3.Up;

		// Smoothly rotate the camera to face the mouse aim.
		cameraRig.Transform.Rotation = Damp(cameraRig.Transform.Rotation,
									mouseAim.Transform.Rotation,
									camSmoothSpeed,
									Time.Delta);
	}

	private Vector3 GetFrozenMouseAimPos()
	{
		if (mouseAim != null)
			return mouseAim.Transform.Position + (frozenDirection * aimDistance);
		else
			return Transform.Rotation.Forward * aimDistance;
	}

	private void UpdateCameraPos()
	{
		if (model != null)
		{
			//Move the whole rig to follow the aircraft.
			Transform.Position = model.Transform.Position;
		}
	}
	private Quaternion Damp(Quaternion a, Quaternion b, float lambda, float dt)
	{
		return Quaternion.Slerp(a, b, 1 - MathF.Exp(-lambda * dt));
	}

}
