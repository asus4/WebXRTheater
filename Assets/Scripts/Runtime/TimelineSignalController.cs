using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace WebXRTheater
{
    /// <summary>
    /// Seek and play the timeline at the time of the signal.
    /// </summary>
    [RequireComponent(typeof(PlayableDirector))]
    public sealed class TimelineSignalController : MonoBehaviour
    {
        public void SendSignalAsset(SignalAsset signal)
        {
            if (TryStartTimelineAtSignal(signal))
            {
                Debug.Log($"Started timeline at signal: {signal.name}");
            }
            else
            {
                Debug.LogWarning($"No timeline found for signal: {signal.name}");
            }
        }

        bool TryStartTimelineAtSignal(SignalAsset signal)
        {
            if (!TryGetComponent(out PlayableDirector director))
            {
                Debug.LogError("PlayableDirector component is required on TimelineSignalController.");
                return false;
            }
            if (director.playableAsset is not TimelineAsset timeline)
            {
                Debug.LogError("PlayableAsset must be a TimelineAsset.");
                return false;
            }

            // Find signal time from timeline
            if (TryGetSignalTime(timeline, signal, out double time))
            {
                director.Pause();
                director.time = time;
                director.Play();
                return true;

            }
            else
            {
                Debug.LogWarning($"Signal {signal.name} not found in timeline {timeline.name}.");
                return false;
            }
        }

        static bool TryGetSignalTime(TimelineAsset timeline, SignalAsset signal, out double time)
        {
            if (timeline.markerTrack != null)
            {
                if (TryGetSignalEmitter(timeline.markerTrack.GetMarkers(), signal, out var emitter))
                {
                    time = emitter.time;
                    return true;
                }
            }
            foreach (var track in timeline.GetOutputTracks())
            {
                if (TryGetSignalEmitter(track.GetMarkers(), signal, out var emitter))
                {
                    time = emitter.time;
                    return true;
                }
            }

            time = 0;
            return false;
        }

        static bool TryGetSignalEmitter(IEnumerable<IMarker> makers, SignalAsset signal, out SignalEmitter target)
        {
            foreach (var marker in makers)
            {
                if (marker is SignalEmitter emitter && emitter.asset == signal)
                {
                    target = emitter;
                    return true;
                }
            }

            target = null;
            return false;
        }
    }
}
