using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Eli.ECS;
using Microsoft.Xna.Framework.Input;

namespace Eli
{
    public class DebugHelper : SceneComponent
    {
        public static DebugHelper Instance;
        private static List<Action> _actions = new List<Action>();

        public static Ps4Input input = new Ps4Input();

        public DebugHelper()
        {
            Instance = this;
        }

        public static void AddOnUpdate(Action action) { _actions.Add(action);}
        public static void AddOnKeyPress(Keys key, Action action)
        {
            _actions.Add(delegate
            {
                if (Input.IsKeyPressed(key))
                {
                    action?.Invoke();
                }
            });
        }
        public override void Update()
        {
            DebugRenderKey(Keys.F1);
            TimeScaler(Keys.F5, 1);
            TimeScaler(Keys.F6, 0.5f);
            TimeScaler(Keys.F7, 0.25f);
            foreach (var a in _actions)
                a?.Invoke();

        }

        private void DebugRenderKey(Keys key)
        {
            if (Input.IsKeyPressed(key))
                Core.DebugRenderEnabled = !Core.DebugRenderEnabled;
            if (Input.GamePads[0].IsButtonPressed(Buttons.Start))
                Core.DebugRenderEnabled = !Core.DebugRenderEnabled;
        }

        private void TimeScaler(Keys key, float scale)
        {
            if (Input.IsKeyPressed(key))
                Time.TimeScale = scale;
        }
    }
}
