using System;
#if USE_SYSTEM_COLLECTION
using System.Collections.Generic;

#else
using BoysheO.Collection2;
#endif

namespace BoysheO.TinyStateMachine
{
    public readonly struct StateBuilder<TState, TEvent, TContext>
        where TState : Enum
        where TEvent : Enum
        where TContext : class
    {
        public StateMachineBuilder<TState, TEvent, TContext> Builder { get; init; }

        internal State<TState, TEvent, TContext> state { get; init; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onEnter">arg0:machine,arg1:state when event called(maybe not same with machine.State)</param>
        /// <returns></returns>
        public StateBuilder<TState, TEvent, TContext> OnEnter(
            Action<StateMachine<TState, TEvent, TContext>, TState> onEnter)
        {
            Builder.ThrowIfDead();
            state.onEnter.Add(onEnter);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onExit">arg0:machine,arg1:state when event called(maybe not same with machine.State)</param>
        /// <returns></returns>
        public StateBuilder<TState, TEvent, TContext> OnExit(
            Action<StateMachine<TState, TEvent, TContext>, TState> onExit)
        {
            Builder.ThrowIfDead();
            state.onExit.Add(onExit);
            return this;
        }

        public StateBuilder<TState, TEvent, TContext> AddUpdater(TEvent tid,
            Action<StateMachine<TState, TEvent, TContext>> updater)
        {
            this.Builder.ThrowIfDead();

#if USE_SYSTEM_COLLECTION
            if (!state.EventId2Processors.TryGetValue(tid, out var processor))
            {
                processor = new List<EventProcessor<TState, TEvent, TContext>>();
                state.EventId2Processors[tid] = processor;
            }
#else
            if (!state.EventId2Processors.InternalBuffer.TryGetValue(tid, out var processor))
            {
                processor = VList<EventProcessor<TState, TEvent, TContext>>.Rent();
                state.EventId2Processors.InternalBuffer[tid] = processor;
            }
#endif

            var tr = new EventProcessor<TState, TEvent, TContext>()
            {
                EventId = tid,
                NoStateGo = true,
                onTrigger = updater,
            };
            processor.Add(tr);
            return this;
        }

        public StateBuilder<TState, TEvent, TContext> AddTransition(TEvent tid, TState toState,
            Func<StateMachine<TState, TEvent, TContext>, bool>? condition = null)
        {
            Builder.ThrowIfDead();
#if USE_SYSTEM_COLLECTION
            if (!state.EventId2Processors.TryGetValue(tid, out var processor))
            {
                processor = new List<EventProcessor<TState, TEvent, TContext>>();
                state.EventId2Processors[tid] = processor;
            }
#else
            if (!state.EventId2Processors.InternalBuffer.TryGetValue(tid, out var processor))
            {
                processor = VList<EventProcessor<TState, TEvent, TContext>>.Rent();
                state.EventId2Processors.InternalBuffer[tid] = processor;
            }
#endif

            var tr = new EventProcessor<TState, TEvent, TContext>()
            {
                EventId = tid,
                ToState = toState,
                Condition = condition
            };
            processor.Add(tr);
            return this;
        }

        public StateBuilder<TState, TEvent, TContext> AddTransition(ReadOnlySpan<TEvent> triggerId, TState toState,
            Func<StateMachine<TState, TEvent, TContext>, bool>? condition = null)
        {
            Builder.ThrowIfDead();
            foreach (var trigger in triggerId)
            {
                this.AddTransition(trigger, toState, condition);
            }

            return this;
        }

        public StateBuilder<TState, TEvent, TContext> AddTransition(TEvent tid, TEvent triggerId2, TState toState,
            Func<StateMachine<TState, TEvent, TContext>, bool>? condition = null)
        {
            this.AddTransition(tid, toState, condition);
            this.AddTransition(triggerId2, toState, condition);
            return this;
        }

        public StateBuilder<TState, TEvent, TContext> AddTransition(TEvent tid, TEvent triggerId2,
            TEvent triggerId3,
            TState toState,
            Func<StateMachine<TState, TEvent, TContext>, bool>? condition = null)
        {
            this.AddTransition(tid, toState, condition);
            this.AddTransition(triggerId2, toState, condition);
            this.AddTransition(triggerId3, toState, condition);
            return this;
        }
    }
}