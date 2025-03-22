async function subscribeToPush() {
    if (!('serviceWorker' in navigator)) {
        console.error("Service Worker is not supported in this browser.");
        return;
    }

    const registration = await navigator.serviceWorker.ready;
    const subscription = await registration.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: '<BEj-Wiu59-OGKk2V4EbpdKX3V6ODV7JSaBj_rkjfvSXpJQsAtvSmgyjWyOWkF1RC6F5VtBSCquFDs6w7EmZ4J80>'
    });

    const sub = JSON.parse(JSON.stringify(subscription)); // Convert object to plain JSON
    console.log("Push Subscription:", sub);

    await fetch('/api/notifications/subscribe', {
        method: 'POST',
        body: JSON.stringify(sub),
        headers: { 'Content-Type': 'application/json' }
    });
}
