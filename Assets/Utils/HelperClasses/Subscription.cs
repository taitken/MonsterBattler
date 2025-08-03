using System;
using System.Collections.Generic;

namespace UtilityClasses {
    public class Subscription{
        private Action unsubscribeCallback;

        public Subscription(Action unsubAction)
        {
            this.unsubscribeCallback = unsubAction;
        }

        public void unsubscribe(){
            this.unsubscribeCallback();
        }

    }
}