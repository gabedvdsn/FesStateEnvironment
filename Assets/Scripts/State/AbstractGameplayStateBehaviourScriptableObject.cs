﻿using System.Collections.Generic;
using UnityEngine;

namespace FESStateSystem
{
    public abstract class AbstractGameplayStateBehaviourScriptableObject : ScriptableObject
    {
        public abstract List<AbstractGameplayState> GenerateStates(StateActor actor);
        public abstract bool Defines(AbstractGameplayStateScriptableObject state);
        public abstract List<AbstractGameplayStateScriptableObject> Get();
    }
}
