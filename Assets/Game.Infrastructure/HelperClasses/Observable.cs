using System;
using System.Collections.Generic;

namespace UtilityClasses
{

    public class Obseravable<t1>
    {
        private t1 ObseravableObject;
        // Action and number of times to be notified
        private List<ObservableActionModel<t1>> subscribers;
        public Obseravable(t1 initialObject)
        {
            this.subscribers = new List<ObservableActionModel<t1>>();
            this.ObseravableObject = initialObject;
        }
        public t1 Get()
        {
            return this.ObseravableObject;
        }
        public void Set(t1 updatedObject)
        {
            this.ObseravableObject = updatedObject;
            this.NotifyAllSubscribers();
        }

        public Subscription Subscribe(Action<t1> subscription)
        {
            this.subscribers.Add(new ObservableActionModel<t1>(subscription, -1));
            Subscription subscriptionRefernce = new Subscription(delegate ()
            {
                this.subscribers.Remove(this.subscribers.Find(sub => { return sub.action == subscription; }));
            });
            subscription(this.ObseravableObject);

            return subscriptionRefernce;
        }

        // Subscribes without triggering the subscription
        public Subscription SubscribeQuietly(Action<t1> subscription)
        {
            this.subscribers.Add(new ObservableActionModel<t1>(subscription, -1));
            Subscription subscriptionRefernce = new Subscription(delegate ()
            {
                this.subscribers.Remove(this.subscribers.Find(sub => { return sub.action == subscription; }));
            });
            return subscriptionRefernce;
        }

        // Subscribes without triggering the subscription, only triggers once.
        public Subscription SubscribeQuietlyNumberTimes(Action<t1> subscription, int numberOfNotifications)
        {
            this.subscribers.Add(new ObservableActionModel<t1>(subscription, numberOfNotifications));
            Subscription subscriptionRefernce = new Subscription(delegate ()
            {
                this.subscribers.Remove(this.subscribers.Find(sub => { return sub.action == subscription; }));
            });
            return subscriptionRefernce;
        }

        public void NotifyAllSubscribers()
        {
            for (int i = this.subscribers.Count - 1; i >= 0; i--)
            {
                this.subscribers[i].action(this.ObseravableObject);
                if (this.subscribers[i].notifcationsLeft > 0)
                {
                    this.subscribers[i].notifcationsLeft--;
                    if (this.subscribers[i].notifcationsLeft == 0) this.subscribers.RemoveAt(i);
                }
            }
        }
    }

    public class Obseravable
    {
        private IList<Action> subscribers;
        public Obseravable()
        {
            this.subscribers = new List<Action>();
        }

        public Subscription Subscribe(Action subscription)
        {
            this.subscribers.Add(subscription);
            Subscription subscriptionRefernce = new Subscription(delegate ()
            {
                this.subscribers.Remove(subscription);
            });
            return subscriptionRefernce;
        }

        public void NotifyAllSubscribers()
        {
            foreach (Action subscriber in this.subscribers)
            {
                subscriber();
            }
        }
    }

    public class ObservableActionModel<t1>
    {
        public ObservableActionModel(Action<t1> _action, int _notifcationsLeft)
        {
            this.action = _action;
            this.notifcationsLeft = _notifcationsLeft;
        }
        public Action<t1> action;
        public int notifcationsLeft;
    }
}