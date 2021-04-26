using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jail
{
    [RequireComponent(typeof(Camera))]
    class CameraMovement : MonoBehaviour
    {
        public TopDownPlayerController Player;
        public Transform VantagePoint;
        public enum CameraMode { FOLLOWING, VANTAGE}
        public CameraMode Mode;
        public Vector3 Offset;
        private bool hasBeenToVantage = false;
        private float speed = 150.0f;
        private Vector3 followPos;

        private void Update()
        {
            followPos = Player.transform.position + Offset; ;
            if (Mode == CameraMode.FOLLOWING)
            {
                transform.position = followPos;
            }
            if (Mode == CameraMode.VANTAGE)
            {
                if (hasBeenToVantage)
                {
                    transform.position = Vector3.MoveTowards(transform.position, followPos, speed * Time.deltaTime);
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, VantagePoint.position, speed * Time.deltaTime);
                }

                if (Vector3.Distance(transform.position, VantagePoint.position) <= 1.0f)
                {
                    hasBeenToVantage = true;
                }
            }
        }

        public void DragToVantagePoint()
        {
            hasBeenToVantage = false;
            Mode = CameraMode.VANTAGE;
        }
    }
}
