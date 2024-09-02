
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TTGL_Survivor.RideMeExtendedAddin
{   
    public static class RideMeExtendedAddin
    {
        public static void AddRideMeExtended()
        {
            RideMeExtended.RideMeExtended.RiderBodyBlacklist.Add("GurrenLagannBody");
            RideMeExtended.RideMeExtended.RiderBodyBlacklist.Add("GurrenBody");
            RideMeExtended.RideMeExtended.RegisterRideSeats("LagannBody", (CharacterBody characterBody) =>
            {
                var modelLocator = characterBody.GetComponent<ModelLocator>();
                var childLocator = modelLocator.modelTransform.GetComponent<ChildLocator>();
                var head = childLocator.FindChild("Head");
                return new List<RideMeExtended.RideSeat>()
                {
                    new RideMeExtended.RideSeat() { SeatTransform = head, PositionOffsetGetter = (x) => (x.SeatTransform.rotation * new Vector3(-0.5f, 0.5f, 0.2f)) }
                };
            });
            RideMeExtended.RideMeExtended.RegisterRideSeats("GurrenBody", (CharacterBody characterBody) =>
            {
                var modelLocator = characterBody.GetComponent<ModelLocator>();
                var childLocator = modelLocator.modelTransform.GetComponent<ChildLocator>();
                var chest = childLocator.FindChild("Chest");
                return new List<RideMeExtended.RideSeat>()
                {
                    new RideMeExtended.RideSeat() { SeatTransform = chest, AlignToSeatRotation = true,
                        PositionOffsetGetter = (x) => (x.SeatTransform.rotation * new Vector3(-1.5f, 2f, 0f)),
                        RotationOffsetGetter = (x) => Quaternion.Euler(new Vector3(0, -135, 0))},
                    new RideMeExtended.RideSeat() { SeatTransform = chest, AlignToSeatRotation = true,
                        PositionOffsetGetter = (x) => (x.SeatTransform.rotation * new Vector3(0f, 2.7f, 0.2f)) },
                    new RideMeExtended.RideSeat() { SeatTransform = chest, AlignToSeatRotation = true,
                        PositionOffsetGetter = (x) => (x.SeatTransform.rotation * new Vector3(1.5f, 2f, 0f)),
                        RotationOffsetGetter = (x) => Quaternion.Euler(new Vector3(0, 135, 0))}
                };
            });
            RideMeExtended.RideMeExtended.RegisterRideSeats("GurrenLagannBody", (CharacterBody characterBody) =>
            {
                var modelLocator = characterBody.GetComponent<ModelLocator>();
                var childLocator = modelLocator.modelTransform.GetComponent<ChildLocator>();
                var chest = childLocator.FindChild("Chest");
                var head = childLocator.FindChild("Head");
                return new List<RideMeExtended.RideSeat>()
                {
                    new RideMeExtended.RideSeat() { SeatTransform = chest, AlignToSeatRotation = true,
                        PositionOffsetGetter = (x) => (x.SeatTransform.rotation * new Vector3(0f, 1.8f, -1.5f)) ,
                        RotationOffsetGetter = (x) => Quaternion.Euler(new Vector3(0, -135, 0))},
                    new RideMeExtended.RideSeat() { SeatTransform = head, AlignToSeatRotation = true,
                        PositionOffsetGetter = (x) => (x.SeatTransform.rotation * new Vector3(0f, 1.3f, 0f)),
                        RotationOffsetGetter = (x) => Quaternion.Euler(new Vector3(0, 90, 0))},
                    new RideMeExtended.RideSeat() { SeatTransform = chest, AlignToSeatRotation = true,
                        PositionOffsetGetter = (x) => (x.SeatTransform.rotation * new Vector3(0f, 1.8f, 1.5f)) ,
                        RotationOffsetGetter = (x) => Quaternion.Euler(new Vector3(0, -45, 0))}
                };
            });
        }

        public static void ExpulseAnyRider(GameObject rideable)
        {
            var seatableController = rideable.GetComponent<RideMeExtended.RideableController>();
            if (seatableController)
            {
                foreach(var seat in seatableController.AvailableSeats)
                {
                    if (seat.SeatUser)
                    {
                        seat.SeatUser.CmdExitSeat();
                    }
                }
            }
        }

        public static void ExitSeat(GameObject rider)
        {
            var riderController = rider.GetComponent<RideMeExtended.RiderController>();
            if (riderController && riderController.CurrentSeat != null)
            {
                riderController.CmdExitSeat();
            }
        }
    }
}
