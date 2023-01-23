﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Eli
{
    /// <summary>
    /// provides frame timing information
    /// </summary>
    public static class Time
    {
        /// <summary>
        /// total time the game has been running
        /// </summary>
        public static float TotalTime;

        /// <summary>
        /// delta time from the previous frame to the current, scaled by timeScale
        /// </summary>
        public static float DeltaTime;

        /// <summary>
        /// unscaled version of deltaTime. Not affected by timeScale
        /// </summary>
        public static float UnscaledDeltaTime;

        /// <summary>
        /// secondary deltaTime for use when you need to scale two different deltas simultaneously
        /// </summary>
        public static float AltDeltaTime;

        /// <summary>
        /// total time since the Scene was loaded
        /// </summary>
        public static float TimeSinceSceneLoad;

        /// <summary>
        /// time scale of deltaTime
        /// </summary>
        public static float TimeScale = 1f;

        /// <summary>
        /// time scale of altDeltaTime
        /// </summary>
        public static float AltTimeScale = 1f;

        /// <summary>
        /// total number of frames that have passed
        /// </summary>
        public static uint FrameCount;

        /// <summary>
        /// access to gametime.
        /// </summary>
        public static GameTime GameTime;

        internal static void Update(float dt, GameTime gameTime)
        {
            TotalTime += dt;
            DeltaTime = dt * TimeScale;
            AltDeltaTime = dt * AltTimeScale;
            UnscaledDeltaTime = dt;
            TimeSinceSceneLoad += dt;
            FrameCount++;
            GameTime = gameTime;
        }


        internal static void SceneChanged()
        {
            TimeSinceSceneLoad = 0f;
        }


        /// <summary>
        /// Allows to check in intervals. Should only be used with interval values above deltaTime,
        /// otherwise it will always return true.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckEvery(float interval)
        {
            // we subtract deltaTime since timeSinceSceneLoad already includes this update ticks deltaTime
            return (int)(TimeSinceSceneLoad / interval) > (int)((TimeSinceSceneLoad - DeltaTime) / interval);
        }
    }
}
