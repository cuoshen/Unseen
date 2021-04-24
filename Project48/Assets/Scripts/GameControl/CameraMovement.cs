using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jail
{
    class CameraMovement : MonoBehaviour
    {
        public TopDownPlayerController Player;
        public enum CameraMode { FOLLOWING, FREE}
        public CameraMode Mode;
        public Vector3 Offset;

        private void Update()
        {
            if (Mode == CameraMode.FOLLOWING)
            {
                transform.position = Player.transform.position + Offset;
            }
        }

        public void DragToVantagePoint()
        {

        }
    }
}
