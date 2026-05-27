using System.Collections;
using UnityEngine;
using UnityEngine.Profiling;

namespace NewSafetyHelp.Audio
{
    public static class AudioLoadThrottler
    {
        private static int activeLoadCount;

        private const int MaxConcurrentLoads = 50;
        
        private const long MemoryPressureThreshold = 3_000_000_000L;

        /// <summary>
        /// Coroutine for waiting for an open slot before executing.
        /// </summary>
        /// <returns>(IEnumerator) Coroutine to run. </returns>
        public static IEnumerator WaitForSlot(bool skipWaiting)
        {
            if (skipWaiting)
            {
                yield break;
            }
            
            // We check if we are above capacity. (Shouldn't usually happen)
            while (activeLoadCount >= MaxConcurrentLoads)
            {
                yield return null;
            }
            
            // We then check if our allocated memory is exceeding normal levels.
            // If we do, we simply wait until we do not exceed 1.8GB of memory.
            while (Profiler.GetTotalAllocatedMemoryLong() > MemoryPressureThreshold)
            {
                yield return new WaitForSecondsRealtime(Random.Range(0.1f, 0.7f));
            }
            
            System.Threading.Interlocked.Increment(ref activeLoadCount);
        }
        
        /// <summary>
        /// Release slot that was being used.
        /// </summary>
        public static void ReleaseSlot(bool skipWaiting)
        {
            if (skipWaiting)
            {
                return;
            }
            
            System.Threading.Interlocked.Decrement(ref activeLoadCount);
        }
    }
}