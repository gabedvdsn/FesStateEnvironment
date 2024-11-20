﻿using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(menuName = "FESState/Retrieval/System Specific")]
public class RetrieveSystemSpecificStateActorScriptableObject : AbstractRetrieveStateActorScriptableObject
{
    [Header("Source Retrieval [Can Be Null]")] 
    
    [Tooltip("If null, retrieves the first actor that fits criteria from every subscribed actor. Be careful about infinite retrieval cycles.")]
    public AbstractRetrieveStateActorScriptableObject SourceRetrieval;

    [Header("Moderator")]
    public List<StateModeratorScriptableObject> LookForModerator;

    [Header("State")] 
    public bool ActiveStatesOnly = true;
    public SerializedDictionary<StatePriorityTagScriptableObject, List<AbstractGameplayStateScriptableObject>> LookForState;
    
    /// <summary>
    /// Try to retrieve the source actor that aligns with LookFor parameters.
    /// </summary>
    /// <param name="actor"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public override bool TryRetrieveActor<T>(out T actor)
    {
        try
        {
            if (SourceRetrieval) return SourceRetrieval.TryRetrieveActor(out actor) && ValidateSource(actor);
            
            actor = RetrieveByAllActors<T>();
            return actor is not null;
        }
        catch
        {
            actor = null;
            return false;
        }
    }

    /// <summary>
    /// Try to retrieve the many source actors that align with LookFor parameters.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="actors"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public override bool TryRetrieveManyActors<T>(int count, out List<T> actors)
    {
        try
        {
            if (SourceRetrieval)
            {
                if (SourceRetrieval.TryRetrieveManyActors(count, out List<T> sources))
                {
                    actors = ValidateSources(sources);
                    return actors.Count > 0;
                }

                actors = null;
                return false;
            }

            actors = RetrieveManyByAllActors<T>(count);
            return actors.Count > 0;
        }
        catch
        {
            actors = null;
            return false;
        }
    }
    
    /// <summary>
    /// Try to retrieve all the source actors that align with LookFor parameters
    /// </summary>
    /// <param name="actors"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override bool TryRetrieveAllActors<T>(out List<T> actors)
    {
        return TryRetrieveManyActors(-1, out actors);
    }

    private List<T> RetrieveManyByAllActors<T>(int count) where T : StateActor
    {
        Dictionary<GameplayStateTagScriptableObject, List<StateActor>> allActors = GameplayStateManager.Instance.AllActors;
        List<T> actors = new List<T>();
        int realCount = count < 0 ? allActors.Sum(kvp => kvp.Value.Count) : count;
        int activeCount = 0;
        
        foreach (GameplayStateTagScriptableObject actorTag in allActors.Keys)
        {
            foreach (StateActor actor in allActors[actorTag])
            {
                if (activeCount >= realCount) return actors;
                if (ValidateSource(actor))
                {
                    actors.Add(actor as T);
                    activeCount += 1;
                }
            }
        }

        return actors;
    }

    private T RetrieveByAllActors<T>() where T : StateActor
    {
        Dictionary<GameplayStateTagScriptableObject, List<StateActor>> allActors = GameplayStateManager.Instance.AllActors;
        foreach (GameplayStateTagScriptableObject actorTag in allActors.Keys)
        {
            foreach (StateActor actor in allActors[actorTag])
            {
                if (ValidateModerator(actor) && ValidateStates(actor)) return actor as T;
            }
        }

        return null;
    }

    private bool ValidateSource<T>(T source) where T : StateActor
    {
        return ValidateModerator(source) && ValidateStates(source);
    }

    private List<T> ValidateSources<T>(List<T> sources) where T : StateActor
    {
        List<T> validated = new List<T>();
        foreach (T source in sources)
        {
            if (ValidateSource(source)) validated.Add(source);
        }

        return validated;
    }
    
    private bool ValidateModerator(StateActor actor)
    {
        if (LookForModerator is not null && LookForModerator.Count > 0) return LookForModerator.Any(m => actor.Moderator.BaseModerator == m);

        return true;
    }

    private bool ValidateStates(StateActor actor)
    {
        if (ActiveStatesOnly)
        {
            Dictionary<StatePriorityTagScriptableObject, AbstractGameplayState> activeStates = actor.Moderator.GetActiveStates();
            foreach (StatePriorityTagScriptableObject priorityTag in LookForState.Keys)
            {
                if (!activeStates.ContainsKey(priorityTag)) continue;
                if (LookForState[priorityTag].All(s => s != activeStates[priorityTag].StateData)) return false;
            }
        }
        else
        {
            foreach (StatePriorityTagScriptableObject priorityTag in LookForState.Keys)
            {
                if (!LookForState[priorityTag].All(s => actor.Moderator.DefinesState(priorityTag, s))) return false;
            }
        }

        return true;

    }

    private void OnValidate()
    {
        if (SourceRetrieval == this) SourceRetrieval = null;
    }
}