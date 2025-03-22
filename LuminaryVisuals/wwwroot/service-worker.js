self.addEventListener('install', event => {
    self.skipWaiting();
});

self.addEventListener('activate', event => {
    event.waitUntil(self.clients.claim());
});

self.addEventListener('push', function (event) {
    let data = {};
    try {
        data = event.data ? event.data.json() : {};
    } catch (error) {
        console.error("Invalid push data:", error);
        return;
    }

    const title = data.title || "Luminary Visuals";
    const options = {
        body: data.message || "You have a new notification.",
        icon: "/icon.png",
        badge: "/badge.png",
        data: { url: data.url || "/" },
        vibrate: [100, 50, 100]
    };

    event.waitUntil(self.registration.showNotification(title, options));
});

self.addEventListener('notificationclick', (event) => {
    event.notification.close();
    if (event.notification.data && event.notification.data.url) {
        clients.openWindow(event.notification.data.url);
    }
});

self.addEventListener('periodicsync', event => {
    if (event.tag === 'check-messages') {
        event.waitUntil(checkForNewMessages());
    }
});

async function checkForNewMessages() {
    try {
        const response = await fetch('/api/Messages/unread', {
            credentials: 'include'
        });
        if (response.ok) {
            const data = await response.json();
            if (data.hasNewMessages) {
                await self.registration.showNotification('Luminary Visuals', {
                    body: `You have ${data.count} unread message(s)`,
                    icon: '/icon.png',
                    vibrate: [100, 50, 100],
                    data: {
                        url: '/messages'
                    }
                });
            }
        }
    } catch (error) {
        console.error('Failed to check for messages:', error);
    }
}