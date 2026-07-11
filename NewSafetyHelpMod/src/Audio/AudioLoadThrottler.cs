using System.Collections;
using UnityEngine;
using UnityEngine.Profiling;

namespace NewSafetyHelp.Audio
{
    public static class AudioLoadThrottler
    {
        private static int activeLoadCount;

        private const int MaxConcurrentLoads = 5;

        private const long MemoryPressureThreshold = 2_000_000_000L; // 2.0 GB

        /// <summary>
        /// Coroutine for waiting for an open slot before executing.
        /// </summary>
        /// <returns>(IEnumerator) Coroutine to run. </returns>
        public static IEnumerator WaitForSlot(bool isHotReload, long audioFileSize)
        {
            // If we are not in hot reload, we don't throttle and allow the work automatically.
            if (!isHotReload)
            {
                yield break;
            }

            // We check if we are above capacity. (Shouldn't usually happen)
            while (activeLoadCount >= MaxConcurrentLoads)
            {
                yield return null;
            }

            // We then check if our allocated memory is exceeding normal levels.
            // If we do, we simply wait until we do not exceed the memory.

            long allocatedMemory = Profiler.GetTotalAllocatedMemoryLong();

            int blockedTimes = 0;

            while (allocatedMemory > MemoryPressureThreshold
                   || audioFileSize + allocatedMemory > MemoryPressureThreshold)
            {
                yield return new WaitForSecondsRealtime(Random.Range(0.1f, 0.7f));

                allocatedMemory = Profiler.GetTotalAllocatedMemoryLong();

                if (blockedTimes >= 5)
                {
                    break;
                }
                else
                {
                    blockedTimes++;
                }
            }

            System.Threading.Interlocked.Increment(ref activeLoadCount);
        }

        /// <summary>
        /// Release slot that was being used.
        /// </summary>
        public static void ReleaseSlot(bool fromHotReload)
        {
            if (!fromHotReload)
            {
                return;
            }

            System.Threading.Interlocked.Decrement(ref activeLoadCount);
        }
    }
}