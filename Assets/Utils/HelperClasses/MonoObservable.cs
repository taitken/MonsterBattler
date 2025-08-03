using System;
using System.Collections.Generic;
using UnityEngine;

namespace UtilityClasses
{
    public class MonoObseravable<t1>
    {
        private Obseravable<t1> observableObject { get; set; }
        public MonoObseravable(t1 initialObject)
        {
            this.observableObject = new Obseravable<t1>(initialObject);
        }
        public t1 Get()
        {
            return this.observableObject.Get();
        }
        public void Set(t1 updatedObject)
        {
            this.observableObject.Set(updatedObject);
        }

        public Subscription Subscribe(MonoObject monobehaviour, Action<t1> subscription)
        {
            Subscription subscriptionRefernce = this.observableObject.Subscribe(subscription);
            if (monobehaviour != null) monobehaviour.AddSubscription(subscriptionRefernce);
            return subscriptionRefernce;
        }

        // Subscribes without triggering the subscription
        public Subscription SubscribeQuietly(MonoObject monobehaviour, Action<t1> subscription)
        {
            Subscription subscriptionRefernce = this.observableObject.SubscribeQuietly(subscription);
            if (monobehaviour != null) monobehaviour.AddSubscription(subscriptionRefernce);
            return subscriptionRefernce;
        }

        // Subscribes without triggering the subscription
        public Subscription SubscribeQuietlyOnce(MonoObject monobehaviour, Action<t1> subscription)
        {
            Subscription subscriptionRefernce = this.observableObject.SubscribeQuietlyNumberTimes(subscription, 1);
            if (monobehaviour != null) monobehaviour.AddSubscription(subscriptionRefernce);
            return subscriptionRefernce;
        }

        public void NotifyAllSubscribers()
        {
            this.observableObject.NotifyAllSubscribers();
        }
    }

    public class MonoObseravable
    {
        private Obseravable observableObject { get; set; }
        public MonoObseravable()
        {
            this.observableObject = new Obseravable();
        }

        public Subscription Subscribe(MonoObject monobehaviour, Action subscription)
        {
            Subscription subscriptionRefernce = this.observableObject.Subscribe(subscription);
            if (monobehaviour != null) monobehaviour.AddSubscription(subscriptionRefernce);
            return subscriptionRefernce;
        }

        public void NotifyAllSubscribers()
        {
            this.observableObject.NotifyAllSubscribers();
        }
    }
}