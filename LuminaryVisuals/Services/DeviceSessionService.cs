using System.Collections.Concurrent;

public class DeviceSessionService
{
    private ConcurrentDictionary<string, List<DeviceSession>> _userDeviceSessions = new();

    public class DeviceSession
    {
        public string DeviceType { get; set; } // "Chrome", "Firefox", "Edge", etc.
        public string BrowserVersion { get; set; }
        public string OperatingSystem { get; set; }
        public DateTime LastActive { get; set; } = DateTime.UtcNow;
    }

    public void RegisterDeviceSession(string userId, string deviceType, string browserVersion, string operatingSystem)
    {
        _userDeviceSessions.AddOrUpdate(
            userId,
            new List<DeviceSession>
            {
                new DeviceSession
                {
                    DeviceType = deviceType,
                    BrowserVersion = browserVersion,
                    OperatingSystem = operatingSystem,
                    LastActive = DateTime.UtcNow
                }
            },
            (key, existingList) =>
            {
                // Check if this exact device type already exists
                var existingDevice = existingList.FirstOrDefault(d =>
                    d.DeviceType == deviceType &&
                    d.BrowserVersion == browserVersion &&
                    d.OperatingSystem == operatingSystem);

                if (existingDevice == null)
                {
                    existingList.Add(new DeviceSession
                    {
                        DeviceType = deviceType,
                        BrowserVersion = browserVersion,
                        OperatingSystem = operatingSystem,
                        LastActive = DateTime.UtcNow
                    });
                }
                else
                {
                    // Update last active time
                    existingDevice.LastActive = DateTime.UtcNow;
                }

                return existingList;
            }
        );
    }

    public List<DeviceSession> GetUserDeviceSessions(string userId)
    {
        return _userDeviceSessions.TryGetValue(userId, out var sessions)
            ? sessions
            : new List<DeviceSession>();
    }

    public void RemoveDeviceSession(string userId, string deviceType, string browserVersion, string operatingSystem)
    {
        if (_userDeviceSessions.TryGetValue(userId, out var sessions))
        {
            sessions.RemoveAll(d =>
                d.DeviceType == deviceType &&
                d.BrowserVersion == browserVersion &&
                d.OperatingSystem == operatingSystem);

            if (sessions.Count == 0)
            {
                _userDeviceSessions.TryRemove(userId, out _);
            }
        }
    }

    public void ReplaceUserSessions(string userId, List<DeviceSession> newSessions)
    {
        _userDeviceSessions[userId] = newSessions;
    }
}