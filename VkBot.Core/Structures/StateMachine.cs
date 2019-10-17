using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VkBot.Core.Structures
{
    public class StateMachine<TState, TActionArgument> : IStateMachine<TState, TActionArgument> where TState : Enum, IComparable, IFormattable, IConvertible
    {
        private TState _currentState;
        private readonly List<State> _states;
        private readonly List<TransitCondition> _transitions;

        public StateMachine()
        {
            _states = new List<State>();
            _transitions = new List<TransitCondition>();
        }

        public TState GetState()
        {
            return _currentState;
        }

        public void SetState(TState state)
        {
            _currentState = state;
        }

        public void PerformCurrentStateAction(TActionArgument actionArgument)
        {
            var currentState = _states.First(x => x.Value.CompareTo(_currentState) == 0);
            currentState.Action.Invoke(actionArgument);
        }

        public bool ProceedNextState(TActionArgument actionArgument)
        {
            var transits = _transitions.Where(x => x.From.CompareTo(_currentState) == 0);
            foreach (var transit in transits)
            {
                if (transit.Condition.Invoke(actionArgument))
                {
                    SetState(transit.To);
                    return true;
                }
            }

            return false;
        }

        public StateMachine<TState, TActionArgument> AddState(TState stateValue, Action<TActionArgument> action)
        {
            if (_states.Any(x => x.Value.CompareTo(stateValue) == 0))
                throw new ArgumentException($"{stateValue.ToString()} already there is in the state machine!");
            var state = new State
            {
                Value = stateValue,
                Action = action
            };
            _states.Add(state);
            return this;
        }

        public StateMachine<TState, TActionArgument> AddTransitCondition(TState from, TState to, Func<TActionArgument, bool> condition)
        {
            if (_transitions.Any(x => x.From.CompareTo(from) == 0
                                               && x.To.CompareTo(to) == 0))
            {
                throw new ArgumentException($"The transit condition from {from.ToString()} to {to.ToString()} already there is in the state machine!");
            }

            var transitCondition = new TransitCondition
            {
                From = from,
                To = to,
                Condition = condition
            };
            _transitions.Add(transitCondition);
            return this;
        }

        public bool RemoveState(TState state)
        {
            if (_currentState.CompareTo(state) == 0)
                return false;

            if (_states.Any(x => x.Value.CompareTo(state) == 0))
            {
                _states.RemoveAll(x => x.Value.CompareTo(state) == 0);

                // delete all links which connected this state
                _transitions.RemoveAll(x => x.From.CompareTo(state) == 0
                                            || x.To.CompareTo(state) == 0);
                return true;
            }

            return false;
        }

        public bool RemoveTransitCondition(TState from, TState to)
        {
            return _transitions.RemoveAll(x => x.From.CompareTo(from) == 0
                                               && x.To.CompareTo(to) == 0) != 0;
        }

        public bool RemoveTransitCondition(Func<TActionArgument, bool> condition)
        {
            return _transitions.RemoveAll(x => x.Condition == condition) != 0;
        }

        private class State
        {
            public TState Value { get; set; }
            public Action<TActionArgument> Action { get; set; }
        }

        private class TransitCondition : IComparable
        {
            public TState From { get; set; }
            public TState To { get; set; }
            public Func<TActionArgument, bool> Condition { get; set; }

            public int CompareTo(object obj)
            {
                if (!(obj is TransitCondition))
                    return -1;
                var o = (TransitCondition)obj;
                var isEqual = From.CompareTo(o.From) == 0
                       && From.CompareTo(o.From) == 0
                      && From.CompareTo(o.From) == 0;
                return isEqual ? 0 : 1;
            }
        }
    }
}
