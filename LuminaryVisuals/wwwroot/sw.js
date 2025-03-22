self.addEventListener('push', function (event) {
    let data = {};

    try {
        data = event.data ? event.data.json() : {};
    } catch (error) {
        console.error("Invalid push data:", error);
        return;
    }

    const title = data.title || "New Notification";
    const options = {
        body: data.message || "You have a new notification.",
        icon: "/icon.png",
        badge: "/badge.png",
        data: { url: data.url || "/" }
    };

    event.waitUntil(self.registration.showNotification(title, options));
});
