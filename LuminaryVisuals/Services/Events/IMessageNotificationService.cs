using LuminaryVisuals.Data.Entities;

namespace LuminaryVisuals.Services.Events
{
    public interface IMessageNotificationService
    {
        IDisposable Subscribe(int ProjectId, Func<int, Task> onMessageReceived);
        Task NotifyNewMessage(int ProjectId);
    }

    // Example implementation (could be in a separate class)
    public class MessageNotificationService : IMessageNotificationService
    {
        private readonly Dictionary<int, List<Func<int, Task>>> _subscribers = new();

        public IDisposable Subscribe(int ProjectId, Func<int, Task> onMessageReceived)
        {
            if (!_subscribers.ContainsKey(ProjectId))
            {
                _subscribers[ProjectId] = new List<Func<int, Task>>();
            }

            _subscribers[ProjectId].Add(onMessageReceived);

            return new Subscription(() =>
            {
                _subscribers[ProjectId].Remove(onMessageReceived);
            });
        }

        public async Task NotifyNewMessage(int ProjectId)
        {
            if (_subscribers.TryGetValue(ProjectId, out var subscribers))
            {
                foreach (var subscriber in subscribers)
                {
                    await subscriber(ProjectId);
                }
            }
        }

        // Helper class to manage subscription disposal
        private class Subscription : IDisposable
        {
            private readonly Action _unsubscribe;

            public Subscription(Action unsubscribe)
            {
                _unsubscribe = unsubscribe;
            }

            public void Dispose()
            {
                _unsubscribe();
            }
        }
    }
}
