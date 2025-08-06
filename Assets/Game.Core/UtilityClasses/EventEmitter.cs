using System;
using System.Collections.Generic;

namespace Game.Core.UtilityClasses
{
    public class EventEmitter<t1>
    {
        private List<Action<t1>> actionsOnEmit;
        public EventEmitter()
        {
            this.actionsOnEmit = new List<Action<t1>>();
        }
        public void Emit(t1 val)
        {
            this.actionsOnEmit.ForEach(action =>
            {
                action(val);
            });
        }

        public void OnEmit(Action<t1> action)
        {
            this.actionsOnEmit.Add(action);
        }
    }

    public class EventEmitter
    {
        private List<Action> actionsOnEmit;
        public EventEmitter()
        {
            this.actionsOnEmit = new List<Action>();
        }
        public void Emit()
        {
            this.actionsOnEmit.ForEach(action =>
            {
                action();
            });
        }

        public void OnEmit(Action action)
        {
            this.actionsOnEmit.Add(action);
        }
    }
}