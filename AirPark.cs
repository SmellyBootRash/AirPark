using System;
using UnityEngine;

namespace AirPark
{
    public class AirPark : PartModule
    {
        Vector3 stationary = new Vector3(0.0f, 0.0f);
        [KSPField(isPersistant = true, guiActive = false)]
        Vector3 freezepos = new Vector3(0.0f, 0.0f);
        [KSPField(isPersistant = true, guiActive = false)]
        Vector3 freezevel = new Vector3(0.0f, 0.0f);
        [KSPField(isPersistant = true, guiActive = true, guiName = "AirParked")]
        Boolean airparked = false;
        [KSPField(isPersistant = true, guiActive = true, guiName = "AutoPark")]
        Boolean autopark = false;

        public override void OnFixedUpdate()
        {
            if (vessel.situation == Vessel.Situations.ORBITING || vessel.situation == Vessel.Situations.SUB_ORBITAL)
            {
                autopark = false;
            }
            if (!vessel.isActiveVessel && autopark)
            {
                freezepos = vessel.GetWorldPos3D();
                if ((vessel.GetWorldPos3D() - FlightGlobals.ActiveVessel.GetWorldPos3D()).magnitude < 1500.0f && airparked == true)
                {
                    airparked = false;
                    vessel.GoOffRails();
                    vessel.Landed = false;
                    vessel.situation = Vessel.Situations.FLYING;
                    vessel.SetWorldVelocity(freezevel - Krakensbane.GetFrameVelocity());
                }
                if ((vessel.GetWorldPos3D() - FlightGlobals.ActiveVessel.GetWorldPos3D()).magnitude > 2000.0f && airparked == false)
                {
                    freezepos = vessel.GetWorldPos3D();
                    freezevel = vessel.GetSrfVelocity();
                    airparked = true;
                }
            }
            if (airparked == false && !vessel.isActiveVessel && vessel.situation == Vessel.Situations.FLYING)
            {
                vessel.GoOffRails();
            }
            if (airparked == true)
            {
                vessel.SetWorldVelocity(stationary);
                vessel.acceleration = stationary;
                vessel.angularVelocity = stationary;
                vessel.situation = Vessel.Situations.LANDED;
                vessel.CoriolisAcc = stationary;
                vessel.gForce = stationary;
                vessel.Landed = true;
                vessel.SetPosition(freezepos);
            }
        }
        public override void OnStart(StartState state)
        {
            if (state != StartState.Editor)
            {
                part.force_activate();
                freezepos = vessel.GetWorldPos3D();
            }

        } 
        [KSPEvent(guiActive = true, guiName = "Toggle Airpark")]
        public void ToggleAirPark()
        {
            if (vessel.situation != Vessel.Situations.ORBITING && vessel.situation != Vessel.Situations.SUB_ORBITAL)
            {
                if (airparked == false)
                {
                    airparked = true;
                    freezepos = vessel.GetWorldPos3D();
                    freezevel = vessel.GetSrfVelocity();
                }
                else
                {
                    airparked = false;
                    vessel.SetWorldVelocity(freezevel);
                }
            }
        }
        [KSPEvent(guiActive = true, guiName = "Toggle AutoPark")]
        public void ToggleAutoPark()
        {
            autopark = !autopark;
        }
    }
}
