using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESStateSystem
{
    [CreateAssetMenu(menuName = "FESState/Trigger/State Trigger")]
    public class StateTriggerScriptableObject : AbstractStateTriggerScriptableObject
    {
        [Header("Moderator Trigger")]
        public bool ForceOverrideModerator;
        public StateModeratorScriptableObject OverrideModerator;

        [Space] 
        public bool TryPreserveAllStates;
        public List<StatePriorityTagScriptableObject> TryPreserveStates;
    
        [Header("States Trigger")]
    
        public bool FoceOverrideStates;
        public SerializedDictionary<StatePriorityTagScriptableObject, AbstractGameplayStateScriptableObject> OverrideStates;
    
        [Header("Other")]
        
        public bool ReEnterSameStates;

        /// <summary>
        /// Activates the state trigger WRT some State Actor
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="flag">Does the state change interrupt (true) or change normally (false)</param>
        /// <returns></returns>
        public override bool Activate(StateActor actor, bool flag)
        {
            // We want to change the actor's state moderator
            if (OverrideModerator)
            {
                if (!ForceOverrideModerator)
                {
                    if (OverrideModerator.ModeratorPriority > actor.Moderator.BaseModerator.ModeratorPriority)
                    {
                        // If no override states, we are done and return false (nothing was done)
                        if (OverrideStates is null) return false;
                
                        // There are override states
                        foreach (StatePriorityTagScriptableObject priorityTag in OverrideStates.Keys)
                        {
                            if (!actor.Moderator.DefinesState(priorityTag, OverrideStates[priorityTag]) && !FoceOverrideStates) continue;

                            ChangeState(actor.Moderator, priorityTag, OverrideStates[priorityTag], flag);
                        }

                        return true;
                    }

                    if (OverrideModerator == actor.Moderator.BaseModerator) return false;
                }
            
                // Let's implement the override moderator
                StateModerator.MetaStateModerator overrideModerator = StateModerator.GenerateMeta(OverrideModerator);

                if (TryPreserveAllStates)
                {
                    Dictionary<StatePriorityTagScriptableObject, AbstractGameplayState> activeStates = actor.Moderator.GetActiveStates();
                    // We want to preserve states at all priorities
                    foreach (StatePriorityTagScriptableObject priorityTag in activeStates.Keys)
                    {
                        // Ensure the override moderator defines the current state
                        if (!overrideModerator.DefinesState(priorityTag, activeStates[priorityTag].StateData)) continue;
                        // Ensure we want to re-enter that state
                        if (!ReEnterSameStates) continue;
                        overrideModerator.ChangeState(priorityTag, activeStates[priorityTag].StateData);
                    }
                }
                else if (TryPreserveStates.Count > 0)
                {
                    // We want to preserve state(s) at some priorities
                    foreach (StatePriorityTagScriptableObject priorityTag in TryPreserveStates)
                    {
                        // Ensure the override moderator defines the current state
                        if (actor.Moderator.TryGetActiveState(priorityTag, out AbstractGameplayState state) && !overrideModerator.DefinesState(priorityTag, state.StateData)) continue;
                        // Ensure we want to re-enter that state
                        if (!ReEnterSameStates) continue;
                        overrideModerator.ChangeState(priorityTag, state.StateData);
                    }
                }
            
                foreach (StatePriorityTagScriptableObject priorityTag in OverrideStates.Keys)
                {
                    if (!overrideModerator.DefinesState(priorityTag, OverrideStates[priorityTag]) && !FoceOverrideStates) continue;
                    ChangeState(actor.Moderator, priorityTag, OverrideStates[priorityTag], flag);
                }
            
                actor.Moderator.ImplementModeratorMeta(overrideModerator, ReEnterSameStates, flag);
                return true;
            }
        
            if (OverrideStates is not null)
            {
                Dictionary<StatePriorityTagScriptableObject, AbstractGameplayState> activeStates = actor.Moderator.GetActiveStates();
                if (ReEnterSameStates)
                {
                    foreach (StatePriorityTagScriptableObject priorityTag in activeStates.Keys)
                    {
                        if (OverrideStates.Keys.Contains(priorityTag)) continue;
                    
                        ChangeState(actor.Moderator, priorityTag, activeStates[priorityTag].StateData, flag);
                    }
                }
            
                foreach (StatePriorityTagScriptableObject priorityTag in OverrideStates.Keys)
                {
                    if (!actor.Moderator.DefinesState(priorityTag, OverrideStates[priorityTag]) && !FoceOverrideStates) continue;
                    if (OverrideStates[priorityTag] == activeStates[priorityTag].StateData && !ReEnterSameStates) continue; 

                    ChangeState(actor.Moderator, priorityTag, OverrideStates[priorityTag], flag);
                }

                return true;
            }
        
            if (ReEnterSameStates)
            {
                Dictionary<StatePriorityTagScriptableObject, AbstractGameplayState> activeStates = actor.Moderator.GetActiveStates();
                foreach (StatePriorityTagScriptableObject priorityTag in activeStates.Keys)
                {
                    ChangeState(actor.Moderator, priorityTag, activeStates[priorityTag].StateData, flag);
                }
                return true;
            }

            return false;
        }

        private void ChangeState(StateModerator moderator, StatePriorityTagScriptableObject priorityTag,
            AbstractGameplayStateScriptableObject state, bool interrupts)
        {
            if (interrupts) moderator.InterruptChangeState(priorityTag, state);
            else moderator.DefaultChangeState(priorityTag, state);
        }
    
        protected virtual void OnValidate()
        {
            if (!OverrideModerator && OverrideStates.Count == 0 && !ReEnterSameStates) throw new Exception($"({name}) State Trigger must define either (or both) Override Moderator or Transition State, or ReEnterSameStates must be true (performs simple moderator reset)");
        }
    }
}