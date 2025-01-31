using System;
using Sandbox;

public sealed class PlayerCraft : Component
{  
		[Property] private PlayerController controller = null;
        [Property] public float thrust = 100f;
        [Property] public Vector3 turnTorque = new Vector3(90f, 25f, 45f);
        [Property] public float forceMult = 1000f;
        [Property] public float sensitivity = 5f;
        [Property] public float aggressiveTurnAngle = 10f;


        [Range(-1f, 1f)] private float pitch = 0f;
        [Range(-1f, 1f)] private float yaw = 0f;
        [Range(-1f, 1f)] private float roll = 0f;

        public float Pitch { set { pitch = Math.Clamp(value, -1f, 1f); } get { return pitch; } }
        public float Yaw { set { yaw = Math.Clamp(value, -1f, 1f); } get { return yaw; } }
        public float Roll { set { roll = Math.Clamp(value, -1f, 1f); } get { return roll; } }

        private Rigidbody rigid;

        private bool rollOverride = false;
        private bool pitchOverride = false;

        protected override void OnEnabled()
        {
            rigid = this.Components.Get<Rigidbody>();
        }

        protected override void OnUpdate()
        {
            // When the player commands their own stick input, it should override what the
            // autopilot is trying to do.
            rollOverride = false;
            pitchOverride = false;

            float keyboardRoll = Input.MouseDelta.x;
            if (MathF.Abs(keyboardRoll) > .25f)
            {
                rollOverride = true;
            }

            float keyboardPitch = Input.MouseDelta.y;
            if (MathF.Abs(keyboardPitch) > .25f)
            {
                pitchOverride = true;
                rollOverride = true;
            }

            // Calculate the autopilot stick inputs.
            float autoYaw = 0f;
            float autoPitch = 0f;
            float autoRoll = 0f;
            if (controller != null)
                RunAutopilot(controller.MouseAimPos, out autoYaw, out autoPitch, out autoRoll);

            // Use either keyboard or autopilot input.
            yaw = autoYaw;
            pitch = (pitchOverride) ? keyboardPitch : autoPitch;
            roll = (rollOverride) ? keyboardRoll : autoRoll;
        }

        private void RunAutopilot(Vector3 flyTarget, out float yaw, out float pitch, out float roll)
        {
            // This is my usual trick of converting the fly to position to local space.
            // You can derive a lot of information from where the target is relative to self.
            var localFlyTarget = Transform.Local.PointToLocal(flyTarget).Normal * sensitivity;
            var angleOffTarget = Vector3.GetAngle(Transform.LocalRotation.Forward, flyTarget - Transform.Position);

            // IMPORTANT!
            // These inputs are created proportionally. This means it can be prone to
            // overshooting. The physics in this example are tweaked so that it's not a big
            // issue, but in something with different or more realistic physics this might
            // not be the case. Use of a PID controller for each axis is highly recommended.

            // ====================
            // PITCH AND YAW
            // ====================

            // Yaw/Pitch into the target so as to put it directly in front of the aircraft.
            // A target is directly in front the aircraft if the relative X and Y are both
            // zero. Note this does not handle for the case where the target is directly behind.
            yaw = Math.Clamp(localFlyTarget.x, -1f, 1f);
            pitch = -Math.Clamp(localFlyTarget.y, -1f, 1f);

            // ====================
            // ROLL
            // ====================

            // Roll is a little special because there are two different roll commands depending
            // on the situation. When the target is off axis, then the plane should roll into it.
            // When the target is directly in front, the plane should fly wings level.

            // An "aggressive roll" is input such that the aircraft rolls into the target so
            // that pitching up (handled above) will put the nose onto the target. This is
            // done by rolling such that the X component of the target's position is zeroed.
            var aggressiveRoll = Math.Clamp(localFlyTarget.x, -1f, 1f);

            // A "wings level roll" is a roll commands the aircraft to fly wings level.
            // This can be done by zeroing out the Y component of the aircraft's right.
            var wingsLevelRoll = Transform.Rotation.Right.y;

            // Blend between auto level and banking into the target.
            //var wingsLevelInfluence = Mathf.InverseLerp(0f, aggressiveTurnAngle, angleOffTarget);
            //roll = Mathf.Lerp(wingsLevelRoll, aggressiveRoll, wingsLevelInfluence);
			roll = aggressiveRoll;
        }

        protected override void OnFixedUpdate()
        {
            // Ultra simple flight where the plane just gets pushed forward and manipulated
            // with torques to turn.
            rigid.ApplyForce(Vector3.Forward * thrust * forceMult);
            rigid.ApplyTorque(new Vector3(turnTorque.x * pitch,
                                                turnTorque.y * yaw,
                                                -turnTorque.z * roll) * forceMult);
        }
    }
