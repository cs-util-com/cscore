﻿using System;
using UnityEngine;

namespace com.csutil {

    public class InputV2 {

        [Obsolete("Instead use instance version by first calling GetInputSystem()")]
        public static int touchCount => GetInputSystem().touchCount;

        [Obsolete("Instead use instance version by first calling GetInputSystem()")]
        public static bool GetKeyUp(KeyCode keyCode) { return GetInputSystem().GetKeyUp(keyCode); }
        
        [Obsolete("Instead use instance version by first calling GetInputSystem()")]
        public static bool GetKey(KeyCode keyCode) { return GetInputSystem().GetKey(keyCode); }

        [Obsolete("Instead use instance version by first calling GetInputSystem()")]
        public static bool GetMouseButton(int button) { return GetInputSystem().GetMouseButton(button); }

        private static readonly Func<IUnityInputSystem> DefaultInputSystemFactory = () => new DefaultUnityInputSystem();
        
        public static IUnityInputSystem GetInputSystem() { return IoC.inject.GetOrAddSingleton(null, DefaultInputSystemFactory); }

    }

    public interface IUnityInputSystem {

        bool GetKeyUp(KeyCode keyCode);
        bool GetKey(KeyCode keyCode);
        int touchCount { get; }
        bool GetMouseButton(int button);

    }

    public class DefaultUnityInputSystem : IUnityInputSystem {

        public bool GetKeyUp(KeyCode keyCode) { return Input.GetKeyUp(keyCode); }
        public bool GetKey(KeyCode keyCode) { return Input.GetKey(keyCode); }
        public int touchCount => Input.touchCount;
        public bool GetMouseButton(int button) { return Input.GetMouseButton(button); }

    }

}