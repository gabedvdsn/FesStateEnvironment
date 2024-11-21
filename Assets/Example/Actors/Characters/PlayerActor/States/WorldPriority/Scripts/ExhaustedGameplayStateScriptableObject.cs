using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FESState/Actor/Player/Exhausted State")]
public class ExhaustedGameplayStateScriptableObject : AbstractPlayerGameplayStateScriptableObject
{
    public override AbstractGameplayState GenerateState(StateActor actor)
    {
        return new ExhaustedGameplayState(this, actor);
    }
    
    public class ExhaustedGameplayState : AbstractPlayerGameplayState
    {

        public ExhaustedGameplayState(AbstractGameplayStateScriptableObject stateData, StateActor actor) : base(stateData, actor)
        {
        }
        public override void Enter()
        {
            
        }
        public override void LogicUpdate()
        {
            
        }
        public override void PhysicsUpdate()
        {
            
        }
        public override void Interrupt()
        {
            
        }
        public override void Conclude()
        {
            
        }
        public override void Exit()
        {
            
        }
    }
}
