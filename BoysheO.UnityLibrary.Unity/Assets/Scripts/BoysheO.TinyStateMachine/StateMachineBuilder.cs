using System;
using System.Runtime.CompilerServices;

#if USE_SYSTEM_COLLECTION
using System.Collections.Generic;

#else
using BoysheO.Collection2;
#endif

namespace BoysheO.TinyStateMachine
{
    public readonly struct StateMachineBuilder<TState, TTrigger, TContext>
        where TState : Enum
        where TTrigger : Enum
        where TContext : class
    {
#if USE_SYSTEM_COLLECTION
        private SortedList<TState, State<TState, TTrigger, TContext>> id2state { get; init; }
        private StrongBox<bool> _isAlive { get; init; }
#else
        private VBinarySortedList<TState, State<TState, TTrigger, TContext>> id2state { get; init; }
#endif

        public static StateMachineBuilder<TState, TTrigger, TContext> Creat()
        {
            var builder = new StateMachineBuilder<TState, TTrigger, TContext>
            {
#if USE_SYSTEM_COLLECTION
                id2state = new SortedList<TState, State<TState, TTrigger, TContext>>(),
                _isAlive = new StrongBox<bool>(true),
#else
                id2state = VBinarySortedList<TState, State<TState, TTrigger, TContext>>.Rent(),
#endif
            };
            
            return builder;
        }

        public StateBuilder<TState, TTrigger, TContext> AddState(TState id)
        {
            ThrowIfDead();
#if USE_SYSTEM_COLLECTION
            if (id2state.ContainsKey(id)) throw new Exception("你已定义过这个状态");
            var state = new State<TState, TTrigger, TContext>()
            {
                Id = id,
                onEnter = new List<Action<StateMachine<TState, TTrigger, TContext>, TState>>(),
                onExit = new List<Action<StateMachine<TState, TTrigger, TContext>, TState>>(),
                EventId2Processors = new SortedList<TTrigger, List<EventProcessor<TState, TTrigger, TContext>>>(),
            };
            id2state[id] = state;
#else
            if (id2state.InternalBuffer.ContainsKey(id)) throw new Exception("你已定义过这个状态");
            var state = new State<TState, TTrigger, TContext>()
            {
                Id = id,
                onEnter = VList<Action<StateMachine<TState, TTrigger, TContext>, TState>>.Rent(),
                onExit = VList<Action<StateMachine<TState, TTrigger, TContext>, TState>>.Rent(),
                EventId2Processors = VBinarySortedList<TTrigger, VList<EventProcessor<TState, TTrigger, TContext>>>.Rent(),
            };
            id2state.InternalBuffer[id] = state;
#endif

            return new StateBuilder<TState, TTrigger, TContext>()
            {
                Builder = this,
                state = state
            };
        }

        public StateMachine<TState, TTrigger, TContext> Build(TState firstState, TContext context)
        {
            ThrowIfDead();
#if USE_SYSTEM_COLLECTION
            var newBuff = id2state;
            _isAlive.Value = false;
#else
            var newBuff = VBinarySortedList<TState, State<TState, TTrigger, TContext>>.Rent();
            var inBuff = newBuff.InternalBuffer;
            inBuff.Capacity = id2state.InternalBuffer.Capacity;
            id2state.InternalBuffer.FastCopyTo(inBuff);
            id2state.Dispose();
#endif
            return new StateMachine<TState, TTrigger, TContext>(newBuff, firstState, context);
        }

        internal void ThrowIfDead()
        {
#if USE_SYSTEM_COLLECTION
            if (_isAlive == null || _isAlive.Value == false) throw new Exception("builder can build only once or not creat by Creat()");
#else
            if (!id2state.IsAlive) throw new Exception("builder can build only once or not creat by Creat()");
#endif
        }
    }
}