using System;
using Cinemachine.Utility;
using UnityEngine;
 
namespace Cinemachine
{
    /// <summary>
    /// An extension for Cinemachine Virtual Camera which post-processes
    /// the final position of the virtual camera.  It listens for CinemachineImpulse
    /// signals on the specified channels, and moves the camera in response to them.
    /// </summary>
    [SaveDuringPlay]
    [AddComponentMenu("")] // Hide in menu
    [DocumentationSorting(DocumentationSortingAttribute.Level.UserRef)]
    [ExecuteAlways]
    [HelpURL(/*Documentation.BaseURL*/"https://docs.unity3d.com/Packages/com.unity.cinemachine@2.8/" + "manual/CinemachineImpulseListener.html")]
    public class CinemachineImpulseListenerAtTarget : CinemachineImpulseListener
    {
        /// <summary>
        /// Target to receive Impulse signal at.
        /// </summary>
        [Tooltip("Gain to apply to the Impulse signal.  1 is normal strength.  "
            + "Setting this to 0 completely mutes the signal.")]
        public Transform m_Target;

        /// <summary>React to any detected impulses</summary>
        /// <param name="vcam">The virtual camera being processed</param>
        /// <param name="stage">The current pipeline stage</param>
        /// <param name="state">The current virtual camera state</param>
        /// <param name="deltaTime">The current applicable deltaTime</param>
        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            if (stage == m_ApplyAfter && deltaTime >= 0)
            {
                bool haveImpulse = CinemachineImpulseManager.Instance.GetImpulseAt(
                    m_Target.position, m_Use2DDistance, m_ChannelMask, 
                    out var impulsePos, out var impulseRot);
                bool haveReaction = m_ReactionSettings.GetReaction(
                    deltaTime, impulsePos, out var reactionPos, out var reactionRot);

                if (haveImpulse)
                {
                    impulseRot = Quaternion.SlerpUnclamped(Quaternion.identity, impulseRot, m_Gain);
                    impulsePos *= m_Gain;
                }
                if (haveReaction)
                {
                    impulsePos += reactionPos;
                    impulseRot *= reactionRot;
                }
                if (haveImpulse || haveReaction)
                {
                    if (m_UseCameraSpace)
                        impulsePos = state.RawOrientation * impulsePos;
                    state.PositionCorrection += impulsePos;
                    state.OrientationCorrection = state.OrientationCorrection * impulseRot;
                }
            }
        }
    }
}
