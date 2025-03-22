// service-worker.js
self.addEventListener('install', event => {
    self.skipWaiting();
});

self.addEventListener('activate', event => {
    event.waitUntil(self.clients.claim());
});

// Handle periodic sync events
self.addEventListener('periodicsync', event => {
    if (event.tag === 'check-messages') {
        event.waitUntil(checkForNewMessages());
    }
});
self.addEventListener("push", function (event) {
    const data = event.data.json();
    self.registration.showNotification(data.title, {
        body: data.message,
        icon: "/icon.png"
    });
});

// Function to check for new messages
async function checkForNewMessages() {
    try {
        const response = await fetch('/api/Messages/unread', {
            credentials: 'include' // Important to include cookies
        });

        if (response.ok) {
            const data = await response.json();
            if (data.hasNewMessages) {
                await self.registration.showNotification('Synchron', {
                    body: `You have ${data.count} unread message(s)`,
                    icon: 'Logo192.png',
                    vibrate: [100, 50, 100],
                    data: {
                        url: '/messages' // URL to open when notification is clicked
                    }
                });
            }
        }
    } catch (error) {
        console.error('Failed to check for messages:', error);
    }
}

// Keep existing push and notification click handlers
self.addEventListener('push', event => {
    const options = {
        body: event.data.text(),
        icon: 'Logo192.png',
        vibrate: [100, 50, 100],
        data: {
            dateOfArrival: Date.now(),
            primaryKey: '2'
        }
    };
    event.waitUntil(
        self.registration.showNotification('Synchron', options)
    );
});

self.addEventListener('notificationclick', (event) => {
    event.notification.close();
    if (event.notification.data && event.notification.data.url) {
        clients.openWindow(event.notification.data.url);
    }
});