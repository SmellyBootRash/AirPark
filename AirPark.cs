using System;
using UnityEngine;

namespace AirPark
{
    public class AirPark : PartModule
    {
		[KSPField(isPersistant = true, guiActive = true, guiName = "AirParked")]
		Boolean Parked;
		[KSPField(isPersistant = true, guiActive = true, guiName = "AutoPark")]
		Boolean autoPark;
		[KSPField(isPersistant = true, guiActive = false)]
		Vector3 ParkPosition = new Vector3(0f, 0f, 0f);
		[KSPField(isPersistant = true, guiActive = false)]
		Vector3 ParkVelocity = new Vector3(0f, 0f, 0f);
		[KSPField(isPersistant = true, guiActive = false)]
		Vessel.Situations previousState = Vessel.Situations.LANDED;

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
				ParkPosition = vessel.GetWorldPos3D();
				part.force_activate();
				RememberPreviousState();
			}
		}

		private void RememberPreviousState()
		{
			if (previousState != Vessel.Situations.LANDED)
			{
				// don't overwrite the previous state
			}
			else
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
			// if we're inactive, see if we want to Park 
			if (!vessel.isActiveVessel && autoPark)
			{
				ParkPosition = vessel.GetWorldPos3D();
				// if we're less than 1.5km from the active vessel and Parked, then wake up
				if ((vessel.GetWorldPos3D() - FlightGlobals.ActiveVessel.GetWorldPos3D()).magnitude < 1500.0f && Parked)
				{
					Parked = false;
					vessel.GoOffRails();
					vessel.Landed = false;
					vessel.situation = previousState;
				}
				// if we're farther than 2km, auto Park if needed
				if ((vessel.GetWorldPos3D() - FlightGlobals.ActiveVessel.GetWorldPos3D()).magnitude > 2000.0f && (!Parked))
				{
					Parked = true;
					ParkVessel();
				}
			}
			// if we're not Parked, and not active and flying, then go off rails
			if (!Parked && !vessel.isActiveVessel && vessel.situation == Vessel.Situations.FLYING)
			{
				vessel.GoOffRails();
			}
			if (Parked)
			{
				vessel.SetWorldVelocity(zeroVector);
				vessel.acceleration = zeroVector;
				vessel.angularVelocity = zeroVector;
				vessel.geeForce = 0.0;
				vessel.situation = Vessel.Situations.LANDED;
				vessel.Landed = true;
			}
		}

		[KSPEvent(guiActive = true, guiName = "Toggle Park")]
		public void TogglePark()
		{
			// cannot Park in orbit or sub-orbit
			if (vessel.situation != Vessel.Situations.SUB_ORBITAL && vessel.situation != Vessel.Situations.ORBITING)
			{
				if (!Parked)
				{
					ParkVessel();
				}
				else
				{
					vessel.situation = previousState;
				}
				Parked = !Parked;
			}
		}


		[KSPEvent(guiActive = true, guiName = "Toggle AutoPark")]
		public void ToggleAutoPark()
		{
			autoPark = !autoPark;
		}

		private void ParkVessel()
		{
			ParkPosition = vessel.GetWorldPos3D();
			ParkVelocity = vessel.GetSrfVelocity();
			RememberPreviousState();
		}
	}
}
