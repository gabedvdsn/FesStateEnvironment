﻿using FESStateSystem.TransitionBehaviourDemo;
using UnityEngine;

namespace FESStateSystem
{
    public class DemoTransitionBehaviourConduitScriptableObject : AbstractTransitionBehaviourConduitScriptableObject<DemoPlayerController>
    {
        [Header("Conduit Prefab [Can Be Null]")]
        public DemoTransitionBehaviourConduit ConduitPrefab;
        
        protected override AbstractTransitionBehaviourConduit<DemoPlayerController> CreateConduit(AbstractTransitionBehaviourComponent<DemoPlayerController> transitionComponent)
        {
            return InstantiateConduit(ConduitPrefab);
        }
    }
}