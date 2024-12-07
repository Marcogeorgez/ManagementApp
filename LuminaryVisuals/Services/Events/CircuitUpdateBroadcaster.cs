using System.Collections.Concurrent;
namespace LuminaryVisuals.Services.Events;


public class CircuitUpdateBroadcaster
{
    // A thread-safe collection to hold callbacks for active circuits
    private readonly ConcurrentDictionary<string, Func<Task>> _subscribers = new();

    // Method to subscribe a circuit
    public void Subscribe(string circuitId, Func<Task> callback)
    {
        _subscribers[circuitId] = callback;
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
                // Log or handle exceptions to prevent breaking the notification chain
                Console.WriteLine($"Error notifying circuit: {ex.Message}");
            }
        }
    }
}
