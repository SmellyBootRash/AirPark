using System;
using UnityEngine;

namespace AirPark
{
    public class AirPark : PartModule
    {
		[KSPField(isPersistant = true, guiActive = true, guiName = "AirParked")]
		Boolean Parked;
		[KSPField(isPersistant = true, guiActive = true, guiName = "Auto UnPark")]
		Boolean autoPark;

		[KSPField(isPersistant = true, guiActive = false)]
		private Vector3 ParkPosition = new Vector3(0f, 0f, 0f);
		[KSPField(isPersistant = true, guiActive = false)]
		private Vector3 ParkVelocity = new Vector3(0f, 0f, 0f);
		[KSPField(isPersistant = true, guiActive = false)]
		private Vector3 ParkAcceleration = new Vector3(0f, 0f, 0f);
		[KSPField(isPersistant = true, guiActive = false)]
		private Vector3 ParkAngularVelocity = new Vector3(0f, 0f, 0f);

		[KSPField(isPersistant = true, guiActive = false)]
		Vessel.Situations previousState = Vessel.Situations.LANDED;

		//have you ever clicked "AirParked"? Rember to keep interesting things from happening
		[KSPField(isPersistant = true, guiActive = false)]
		public bool isActive = false;

		private static Vector3 zeroVector = new Vector3(0f, 0f, 0f);

		public override void OnStart(StartState state)
		{
			if (state != StartState.Editor)
			{
				InitBaseState();
			}
		}

		private void InitBaseState()
		{
			if (vessel != null)
			{
				ParkPosition = vessel.transform.position;
				part.force_activate();
			}
		}

		public override void OnSave(ConfigNode node)
		{
			base.OnSave(node);
			if(vessel != null)
			{
				ParkPosition = GetVesselPostion();
			}
		}

		private void RememberPreviousState()
		{
			if (!Parked && previousState != Vessel.Situations.LANDED)
			{
				previousState = vessel.situation;
			}
		}

		public override void OnFixedUpdate()
		{
			// can't Park if we're orbiting
			if (vessel.situation == Vessel.Situations.SUB_ORBITAL || vessel.situation == Vessel.Situations.ORBITING)
			{
				autoPark = false;
				Parked = false;
			}
			// if we're inactive, and autopark is set
			if (!vessel.isActiveVessel && autoPark)
			{
				// if we're less than 1.5km from the active vessel and Parked, then wake up
				if ((vessel.GetWorldPos3D() - FlightGlobals.ActiveVessel.GetWorldPos3D()).magnitude < 1500.0f && Parked)
				{
					vessel.GoOffRails();
					RestoreVesselState();
				}
				// if we're farther than 2km, auto Park if needed
				if ((vessel.GetWorldPos3D() - FlightGlobals.ActiveVessel.GetWorldPos3D()).magnitude > 2000.0f && (!Parked))
				{
					ParkVessel();
				}
			}
			if (Parked)
			{
				ParkVessel();
			}
		}

		private void RestoreVesselState()
		{
			if (isActive == false) { return; } //we only want to restore the state if you have parked somewhere intentionally
			vessel.situation = previousState;
			if (vessel.situation != Vessel.Situations.LANDED) { vessel.Landed = false; }
			if (Parked) { Parked = false; }

			FreezeVesselInPlace();

			//Restore Velocity and Accleration
			vessel.SetWorldVelocity(ParkVelocity);
			vessel.acceleration = ParkAcceleration;
			vessel.angularVelocity = ParkAngularVelocity;
		}

		[KSPEvent(guiActive = true, guiName = "Toggle Park")]
		public void TogglePark()
		{
			// cannot Park in orbit or sub-orbit
			if (vessel.situation != Vessel.Situations.SUB_ORBITAL && vessel.situation != Vessel.Situations.ORBITING)
			{
				if (!Parked)
				{
					ParkPosition = GetVesselPostion();

					//we only want to remember the initial velocity, not subseqent updates by onFixedUpdate()
					ParkVelocity = vessel.GetSrfVelocity();
					ParkAcceleration = vessel.acceleration;
					ParkAngularVelocity = vessel.angularVelocity;

					ParkVessel();
				}
				else
				{
					RestoreVesselState();
				}
				isActive = true;
			}
		}


		[KSPEvent(guiActive = true, guiName = "Toggle AutoPark")]
		public void ToggleAutoPark()
		{
			autoPark = !autoPark;
		}

		private void ParkVessel()
		{
			RememberPreviousState();
			FreezeVesselInPlace();

			vessel.situation = Vessel.Situations.LANDED;
			vessel.Landed = true;
			Parked = true;
		}

		private void FreezeVesselInPlace()
		{
			vessel.SetWorldVelocity(zeroVector);
			vessel.acceleration = zeroVector;
			vessel.angularVelocity = zeroVector;
			vessel.geeForce = 0.0;
			SetVesselPosition();

		}

		//Code Adapted from Hyperedit landing functions 
		//https://github.com/Ezriilc/HyperEdit


		private Vector3d GetVesselPostion()
		{
			double latitude = 0, longitude = 0, altitude = 0;
			var pqs = vessel.mainBody.pqsController;
			if (pqs == null)
			{
				Destroy(this);
				return zeroVector;
			}

			altitude = pqs.GetSurfaceHeight(vessel.mainBody.GetRelSurfaceNVector(latitude, longitude)) - vessel.mainBody.Radius;

			return vessel.mainBody.GetRelSurfacePosition(latitude, longitude, altitude);
		}

		private void SetVesselPosition()
		{
			vessel.orbitDriver.pos = ParkPosition;
		}
	}
}
