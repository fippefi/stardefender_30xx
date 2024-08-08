using Sandbox;
using System.Drawing;
using System.Runtime;

public sealed class PlayerController : Component
{
	[Property] public GameObject Model { get; set; }
	[Property] public GameObject Cam { get; set; }
	[Property] public float drag;
	[Property] public float slideDrag;
	[Property] public float speed;
	private Vector3 movement;

	protected override void OnEnabled()
	{
		base.OnEnabled();
		movement = 0;
		
	}

	protected override void OnUpdate()
	{
		float energyRetention = 1f - drag;

		if ( Input.Down( "Forward" ) ) movement += Transform.World.Forward;
		if ( Input.Down( "Backward" ) ) movement += Transform.World.Backward;
		if ( Input.Down( "Jump") ) energyRetention = 1f - slideDrag;
		
		movement *= energyRetention;

		var rot = GameObject.Transform.Rotation;
		var pos = GameObject.Transform.Position + movement * Time.Delta * speed;
		
		if ( Input.Down( "Left" ) )
		{
			rot *= Rotation.From( 0, Time.Delta * 90.0f, 0 );
		}

		if ( Input.Down( "Right" ) )
		{
			rot *= Rotation.From( 0, Time.Delta * -90.0f, 0 );
		}

		Transform.Local = new Transform( pos, rot, 1 );
	}
}
