
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
                    new RideMeExtended.RideSeat() { SeatTransform = head, PositionOffsetGetter = (x) => new Vector3(-0.5f, 0.5f, 0f) }
                };
            });
            RideMeExtended.RideMeExtended.RegisterRideSeats("GurrenBody", (CharacterBody characterBody) =>
            {
                var modelLocator = characterBody.GetComponent<ModelLocator>();
                var childLocator = modelLocator.modelTransform.GetComponent<ChildLocator>();
                var chest = childLocator.FindChild("Chest");
                return new List<RideMeExtended.RideSeat>()
                {
                    new RideMeExtended.RideSeat() { SeatTransform = chest, PositionOffsetGetter = (x) => new Vector3(-2f, 3f, 0f) },
                    new RideMeExtended.RideSeat() { SeatTransform = chest, PositionOffsetGetter = (x) => new Vector3(0f, 3f, 0f) },
                    new RideMeExtended.RideSeat() { SeatTransform = chest, PositionOffsetGetter = (x) => new Vector3(2f, 3f, 0f) }
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
                    new RideMeExtended.RideSeat() { SeatTransform = chest, PositionOffsetGetter = (x) => new Vector3(0f, 3f, -2f) },
                    new RideMeExtended.RideSeat() { SeatTransform = head, PositionOffsetGetter = (x) => new Vector3(0f, 1f, 0f) },
                    new RideMeExtended.RideSeat() { SeatTransform = chest, PositionOffsetGetter = (x) => new Vector3(0f, 3f, 2f) }
                };
            });
        }
    }
}
