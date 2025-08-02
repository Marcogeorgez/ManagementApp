using System.Collections.Concurrent;
namespace ManagementApp.Services.Events;


public class CircuitUpdateBroadcaster
{
    // A thread-safe collection to hold callbacks for active circuits
    private readonly ConcurrentDictionary<string, Func<Task>> _subscribers = new();

    // Method to subscribe a circuit
    public void Subscribe(string circuitId, Func<Task> callback)
    {
        _subscribers[circuitId] = callback;
    }
    // Method to check if user is subscribed in a circuit
    public bool IsSubscribed(string circuitId)
    {
        if (_subscribers.TryGetValue(circuitId, out var callback))
        {
            return true; // User is subscribed
        }
        else
        {
            return false; // User is not subscribed
        }
    }
    // Method to unsubscribe a circuit (e.g., when a user disconnects)
    public void Unsubscribe(string circuitId)
    {
        _subscribers.TryRemove(circuitId, out _);
    }

    // Notify all subscribers
    public async Task NotifyAllAsync()
    {
        foreach (var subscriber in _subscribers.Values)
        {
            try
            {
                await subscriber.Invoke();
            }
            catch (Exception ex)
            {
                // handle exceptions to prevent breaking the notification chain
                Console.WriteLine($"Error notifying circuit: {ex.Message}");
            }
        }
    }
}
