using System.Collections.Concurrent;
using System.Reflection;

namespace PolyPanic.Bus

// This class contains the EventBus system for PolyPanic.
// (Note): This EventBus was ported from one of my old Kotlin projects with help from Claude 4 Sonnet. All of the comments are by me. 
{
    // This is the attribute itself that can be used to subscribe methods to events.
    [AttributeUsage(AttributeTargets.Method)]
    public class SubscribeAttribute : Attribute
    {
    }

    // This class represents a subscriber.
    public class Subscriber
    {
        public object Listener { get; }
        public MethodInfo Method { get; }

        public Subscriber(object listener, MethodInfo method)
        {
            Listener = listener;
            Method = method;
        }
        
        // This method invokes the subscribed method with the event object.
        public void Invoke(object eventObj)
        {
            Method.Invoke(Listener, new[] { eventObj });
        }
    }

    public class EventBus
    {
        private readonly ConcurrentDictionary<Type, List<Subscriber>> subscribers = new ConcurrentDictionary<Type, List<Subscriber>>();

        // This method allows you to subscribe a listener to the event bus.
        public void Subscribe(object listener)
        {
            var methods = listener.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var method in methods)
            {
                var subscribeAttribute = method.GetCustomAttribute<SubscribeAttribute>();
                if (subscribeAttribute != null)
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length != 1)
                    {
                        throw new ArgumentException(
                            $"Method {method.Name} in {listener.GetType().Name} must have exactly one parameter.");
                    }
                    var eventType = parameters[0].ParameterType;
                    subscribers.AddOrUpdate(
                        eventType,
                        new List<Subscriber> { new Subscriber(listener, method) },
                        (key, existingList) =>
                        {
                            existingList.Add(new Subscriber(listener, method));
                            return existingList;
                        }
                    );
                }
            }
        }

        // This method allows you to unsubscribe a listener from the event bus.
        public void Unsubscribe(object listener)
        {
            foreach (var subscriberList in subscribers.Values)
            {
                subscriberList.RemoveAll(subscriber => subscriber.Listener == listener);
            }
        }

        // This method allows you to post an event to the event bus.
        public void Post(object eventObject)
        {
            if (subscribers.TryGetValue(eventObject.GetType(), out var subscriberList))
            {
                foreach (var subscriber in subscriberList)
                {
                    subscriber.Invoke(eventObject);
                }
            }
        }
    }
}