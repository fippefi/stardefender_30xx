using Sandbox;
using System.Drawing;
using System.Runtime;

public sealed class PlayerController : Component
{
	[Property] public GameObject Model { get; set; }
	[Property] public GameObject cam;

	protected override void OnEnabled()
	{
		base.OnEnabled();

		if ( IsProxy )
			return;
	}

	protected override void OnUpdate()
	{
		Vector3 movement = 0;
		if ( Input.Down( "Forward" ) ) movement += Transform.World.Forward;
		if ( Input.Down( "Backward" ) ) movement += Transform.World.Backward;

		var rot = GameObject.Transform.Rotation;
		var pos = GameObject.Transform.Position + movement * Time.Delta * 100.0f;
		
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
