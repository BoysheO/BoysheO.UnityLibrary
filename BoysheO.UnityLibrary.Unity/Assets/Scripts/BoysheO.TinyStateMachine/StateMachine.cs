using System;

#if USE_SYSTEM_COLLECTION
using System.Buffers;
using System.Collections.Generic;

#else
using BoysheO.Collection2;
using BoysheO.Collection2.Linq;
#endif
namespace BoysheO.TinyStateMachine
{
    /// <summary>
    /// Creat by <see cref="StateMachineBuilder{TState,TTrigger,TState}"/>
    /// </summary>
    public class StateMachine<TState, TTrigger, TContext> : IDisposable
        where TState : Enum
        where TTrigger : Enum
        where TContext : class
    {
#if USE_SYSTEM_COLLECTION
        private readonly SortedList<TState, State<TState, TTrigger, TContext>> id2state;
#else
        private readonly VBinarySortedList<TState, State<TState, TTrigger, TContext>> id2state;
#endif
        public TState State { get; private set; }

        public TContext Context { get; init; }

        /// <summary>
        /// 每次切换状态，在老状态OnExit之后，新状态OnEnter前增加1
        /// </summary>
        public int StateOrder { get; private set; }

        private bool isDead;

        /// <summary>
        /// arg0:machine,arg1:state when event called(maybe not same with machine.State)
        /// 在触发事件时，如果前面执行的事件更改了状态机的状态则会导致参数0读出的当前状态和参数1事件发送时的状态不同，需要注意处理
        /// </summary>
        public event Action<StateMachine<TState, TTrigger, TContext>, TState>? onStateChanged;

        /// <summary>
        /// 规范上要求使用lamda，直接使用函数组可能导致定位不到日志
        /// </summary>
        public event Action<string>? Logger;

        public void Fire(TTrigger trigger)
        {
            if (isDead)
            {
                if (this.Logger != null)
                {
                    this.Logger.Invoke("you are trying to fire after stateMachine dead,this call will be ignore");
                }

                return; //disposed
            }
#if USE_SYSTEM_COLLECTION
            var state = id2state[State];
            if (!state.EventId2Processors.TryGetValue(trigger, out var tr))
#else
            var state = id2state.InternalBuffer[State];
            if (!state.EventId2Processors.InternalBuffer.TryGetValue(trigger, out var tr))
#endif
            {
                if (this.Logger != null)
                {
                    var stateName = Enum.GetName(typeof(TState), state.Id);
                    var triName = Enum.GetName(typeof(TTrigger), trigger);
                    this.Logger($"[{stateName}]Fire {triName}:state doesn't have it");
                }

                return; //当前状态不响应此事件
            }

            //根据第一个可通行的路径改变状态
            foreach (var trigger1 in tr)
            {
                if (trigger1.NoStateGo)
                {
                    if (this.Logger != null)
                    {
                        var stateName = Enum.GetName(typeof(TState), state.Id);
                        var triName = Enum.GetName(typeof(TTrigger), trigger1.EventId);
                        this.Logger($"[{stateName}]Fire {triName}:update");
                    }

                    trigger1.onTrigger(this);
                    break;
                }

                if (trigger1.Condition == null || trigger1.Condition(this))
                {
                    if (this.Logger != null)
                    {
                        var stateName = Enum.GetName(typeof(TState), state.Id);
                        var triName = Enum.GetName(typeof(TTrigger), trigger1.EventId);
                        var stateName2 = Enum.GetName(typeof(TState), trigger1.ToState);
                        this.Logger($"[{stateName}]Fire {triName}:to {stateName2}");
                    }

                    var curState = state.Id;
                    if (state.onExit.Count > 0)
                    {
#if USE_SYSTEM_COLLECTION
                        var len = state.onExit.Count;
                        var onExitLst =
                            ArrayPool<Action<StateMachine<TState, TTrigger, TContext>, TState>>.Shared.Rent(len);
                        state.onExit.CopyTo(onExitLst);
                        foreach (var action in onExitLst.AsSpan(0, len))
                        {
                            action(this, curState);
                        }

                        Array.Clear(onExitLst, 0, len);
                        ArrayPool<Action<StateMachine<TState, TTrigger, TContext>, TState>>.Shared.Return(onExitLst,
                            false);
#else
                        using var onExitLst = state.onExit.ToVList();
                        foreach (var action in onExitLst.Span)
                        {
                            action(this, curState);
                        }
#endif
                    }

                    this.StateOrder++;
                    this.State = trigger1.ToState;
#if USE_SYSTEM_COLLECTION
                    state = this.id2state[this.State];
                    if (state.onEnter.Count > 0)
                    {
                        var count = state.onEnter.Count;
                        var copy =
                            ArrayPool<Action<StateMachine<TState, TTrigger, TContext>, TState>>.Shared.Rent(count);
                        foreach (var action in copy.AsSpan(0, count))
                        {
                            action.Invoke(this, trigger1.ToState);
                        }

                        Array.Clear(copy, 0, count);
                        ArrayPool<Action<StateMachine<TState, TTrigger, TContext>, TState>>.Shared.Return(copy);
                    }
#else
                    state = this.id2state.InternalBuffer[this.State];
                    if (state.onEnter.Count > 0)
                    {
                        using var copy = state.onEnter.ToVList();
                        foreach (var action in copy.Span)
                        {
                            action.Invoke(this, trigger1.ToState);
                        }
                    }
#endif
                    this.onStateChanged?.Invoke(this, trigger1.ToState);
                    return;
                }
            }

            //运行到这里表示没有任何一个路径是可以通过的
            if (this.Logger != null)
            {
                var stateName = Enum.GetName(typeof(TState), state.Id);
                var triName = Enum.GetName(typeof(TTrigger), trigger);
                this.Logger($"[{stateName}]Fire {triName}:all condition fail");
            }
        }

#if USE_SYSTEM_COLLECTION
        internal StateMachine(SortedList<TState, State<TState, TTrigger, TContext>> id2State, TState firstState,
            TContext context)
#else
        internal StateMachine(VBinarySortedList<TState, State<TState, TTrigger, TContext>> id2State, TState firstState,
            TContext context)
#endif
        {
            id2state = id2State;
            State = firstState;
            this.Context = context;
        }

        public void Dispose()
        {
            if (isDead) return;
            isDead = true;
            if (this.Logger != null)
            {
                this.Logger($"statemachine disposed");
            }
#if USE_SYSTEM_COLLECTION
#else
            for (var i = id2state.Count - 1; i >= 0; i--)
            {
                var state = id2state.InternalBuffer.GetValueAtIndex(i);
                state.onExit.Dispose();
                state.onEnter.Dispose();
                foreach (var (_, triggers) in state.EventId2Processors.InternalBuffer)
                {
                    triggers.Dispose();
                }
            }

            id2state.Dispose();
#endif
            onStateChanged = null;
        }
    }
}