using System;
#if USE_SYSTEM_COLLECTION
using System.Collections.Generic;

#else
using BoysheO.Collection2;
#endif


namespace BoysheO.TinyStateMachine
{
    internal readonly struct State<TState, TEvent, TContext>
        where TState : Enum
        where TEvent : Enum
        where TContext : class
    {
        public TState Id { get; init; }
#if USE_SYSTEM_COLLECTION
        public SortedList<TEvent, List<EventProcessor<TState, TEvent, TContext>>> EventId2Processors { get; init; }
        public List<Action<StateMachine<TState, TEvent, TContext>, TState>> onEnter { get; init; }
        public List<Action<StateMachine<TState, TEvent, TContext>, TState>> onExit { get; init; }

#else
        public VBinarySortedList<TEvent, VList<EventProcessor<TState, TEvent, TContext>>> EventId2Processors { get; init; }
        public VList<Action<StateMachine<TState, TEvent, TContext>, TState>> onEnter { get; init; }
        public VList<Action<StateMachine<TState, TEvent, TContext>, TState>> onExit { get; init; }
#endif
    }
}