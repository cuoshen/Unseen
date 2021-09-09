using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using NaughtyAttributes;

namespace GameplayIngredients.Rigs
{
    [AddComponentMenu(ComponentMenu.rigsPath + "Director Control Rig")]
    public class DirectorControlRig : Rig
    {
        public override int defaultPriority => 0;
        public override UpdateMode defaultUpdateMode => UpdateMode.Update;

        public enum PlayMode
        {
            Stop,
            Play,
            Reverse
        }

        public enum WrapMode
        {
            Stop,
            Loop,
            PingPong
        }

        [NonNullCheck]
        public PlayableDirector director;

        public PlayMode InitialPlayMode = PlayMode.Play;
        public float InitialTime = 0.0f;
        public bool UnscaledGameTime = false;

        public WrapMode wrapMode = WrapMode.Stop;
        
        public PlayMode playMode { get { return m_PlayMode; } set { m_PlayMode = value; } }
        public float stopTime { get { return m_StopTime; } set { m_StopTime = value; } }
        [ShowNativeProperty]
        public float time { get { return (float)director.time; } set { director.time = value; } }
        public TimelineAsset timeline { get { return director.playableAsset as TimelineAsset; } set { director.playableAsset = value; } }

        float m_StopTime = -1.0f;
        PlayMode m_PlayMode;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (director != null)
            {
                m_PlayMode = InitialPlayMode;
                director.timeUpdateMode = DirectorUpdateMode.Manual;
                director.time = InitialTime;
            }
        }

        public override void UpdateRig(float deltaTime)
        {
            if(m_PlayMode != PlayMode.Stop)
            {
                float dt = UnscaledGameTime? Time.unscaledDeltaTime : Time.deltaTime;

                float prevTime = (float)director.time;
                float newTime = prevTime + (m_PlayMode == PlayMode.Reverse ? -1.0f : 1.0f) * dt;

                if (m_StopTime >= 0.0f && 
                    ( (m_PlayMode == PlayMode.Play && prevTime < m_StopTime && m_StopTime <= newTime) 
                    || (m_PlayMode == PlayMode.Reverse && newTime <= m_StopTime && m_StopTime < prevTime)
                    ))
                {
                    director.time = m_StopTime;
                    m_PlayMode = PlayMode.Stop;
                    m_StopTime = -1.0f;
                }
                else
                {
                    newTime = Mathf.Clamp(newTime, 0f, (float)director.playableAsset.duration);
                    director.time = newTime;
                }


                director.Evaluate();

                if((director.time <= 0.0f && m_PlayMode == PlayMode.Reverse) ||
                    (director.time >= director.playableAsset.duration && m_PlayMode == PlayMode.Play))
                {
                    switch(wrapMode)
                    {
                        case WrapMode.Loop:
                            if (director.time <= 0.0f)
                                director.time = director.playableAsset.duration;
                            else
                                director.time = 0.0f;
                            break;
                        case WrapMode.PingPong:
                            if (m_PlayMode == PlayMode.Play)
                                m_PlayMode = PlayMode.Reverse;
                            else if (m_PlayMode == PlayMode.Reverse)
                                m_PlayMode = PlayMode.Play;
                            break;
                        case WrapMode.Stop:
                            m_PlayMode = PlayMode.Stop;
                            break;
                    }
                }
            }
        }
    }
}
