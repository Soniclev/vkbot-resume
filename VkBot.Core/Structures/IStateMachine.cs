using System;

namespace VkBot.Core.Structures
{
    public interface IStateMachine<TState, TActionArgument> where TState : Enum, IComparable, IFormattable, IConvertible
    {
        StateMachine<TState, TActionArgument> AddState(TState stateValue, Action<TActionArgument> action);
        StateMachine<TState, TActionArgument> AddTransitCondition(TState from, TState to, Func<TActionArgument, bool> condition);
        TState GetState();
        void PerformCurrentStateAction(TActionArgument actionArgument);
        bool ProceedNextState(TActionArgument actionArgument);
        bool RemoveState(TState state);
        bool RemoveTransitCondition(Func<TActionArgument, bool> condition);
        bool RemoveTransitCondition(TState from, TState to);
        void SetState(TState state);
    }
}