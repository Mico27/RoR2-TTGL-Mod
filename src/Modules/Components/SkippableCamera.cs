using System;
using System.Collections.ObjectModel;
using RoR2;
using UnityEngine;

namespace TTGL_Survivor.Modules.Components
{
    public class SkippableCamera : MonoBehaviour, ICameraStateProvider
    {
        private void Update()
        {
            ReadOnlyCollection<CameraRigController> readOnlyInstancesList = CameraRigController.readOnlyInstancesList;
            for (int i = 0; i < readOnlyInstancesList.Count; i++)
            {
                CameraRigController cameraRigController = readOnlyInstancesList[i];
                if (!cameraRigController.hasOverride)
                {
                    cameraRigController.SetOverrideCam(this, this.entryLerpDuration);
                }
            }
        }

        private void OnDisable()
        {
            ReadOnlyCollection<CameraRigController> readOnlyInstancesList = CameraRigController.readOnlyInstancesList;
            for (int i = 0; i < readOnlyInstancesList.Count; i++)
            {
                CameraRigController cameraRigController = readOnlyInstancesList[i];
                if (cameraRigController.IsOverrideCam(this))
                {
                    cameraRigController.SetOverrideCam(null, this.exitLerpDuration);
                }
            }
        }

        public void GetCameraState(CameraRigController cameraRigController, ref CameraState cameraState)
        {
            cameraState.position = base.transform.position;
            cameraState.rotation = base.transform.rotation;
            if (this.fovOverride > 0f)
            {
                cameraState.fov = this.fovOverride;
            }
        }

        public bool IsUserLookAllowed(CameraRigController cameraRigController)
        {
            return this.allowUserLook;
        }

        public bool IsUserControlAllowed(CameraRigController cameraRigController)
        {
            return this.allowUserControl;
        }

        public bool IsHudAllowed(CameraRigController cameraRigController)
        {
            return this.allowUserHud;
        }

        private void OnDrawGizmosSelected()
        {
            Color color = Gizmos.color;
            Matrix4x4 matrix = Gizmos.matrix;
            Gizmos.color = Color.yellow;
            Matrix4x4 identity = Matrix4x4.identity;
            identity.SetTRS(base.transform.position, base.transform.rotation, Vector3.one);
            Gizmos.matrix = identity;
            Gizmos.DrawFrustum(Vector3.zero, (this.fovOverride > 0f) ? this.fovOverride : 60f, 10f, 0.1f, 1.7777778f);
            Gizmos.matrix = matrix;
            Gizmos.color = color;
        }

        public float entryLerpDuration = 1f;

        public float exitLerpDuration = 1f;

        public float fovOverride;

        public bool allowUserLook;

        public bool allowUserHud;

        public bool allowUserControl;
    }
}