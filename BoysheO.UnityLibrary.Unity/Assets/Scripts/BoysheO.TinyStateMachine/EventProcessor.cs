using System;

namespace BoysheO.TinyStateMachine
{
    internal readonly struct EventProcessor<TState, TTrigger, TContext>
            where TState : Enum
            where TTrigger : Enum
            where TContext : class
    {
        public TTrigger EventId { get; init; }
        public bool NoStateGo { get; init; }
        public TState ToState { get; init; }
        public Func<StateMachine<TState, TTrigger, TContext>, bool>? Condition { get; init; }
        public Action<StateMachine<TState, TTrigger, TContext>> onTrigger { get; init; }
    }
}