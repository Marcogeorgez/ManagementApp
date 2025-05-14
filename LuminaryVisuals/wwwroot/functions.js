window.saveAsFile = (filename, base64Content) => {
    const byteCharacters = atob(base64Content);
    const byteNumbers = new Array(byteCharacters.length);

    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }

    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: 'text/csv' });

    const link = document.createElement('a');
    link.href = window.URL.createObjectURL(blob);
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

window.getTimezoneOffset = function () {
    return -new Date().getTimezoneOffset(); // Offset in minutes, negative for UTC-
};



function showLoadingIndicator() {
    const overlay = document.getElementById("loading-overlay");
    if (overlay) {
        overlay.style.display = "block";
    }
}

function hideLoadingIndicator() {
    const overlay = document.getElementById("loading-overlay");
    if (overlay) {
        overlay.style.display = "none";
    }
}



let audioContext;
let audioBuffer;

async function loadAudio() {
    audioContext = new (window.AudioContext || window.webkitAudioContext)();

    let response = await fetch("/audio/notification.mp3");
    let arrayBuffer = await response.arrayBuffer();
    audioBuffer = await audioContext.decodeAudioData(arrayBuffer);
}

function playNotificationSound() {
    if (!audioContext) return;

    let source = audioContext.createBufferSource();
    source.buffer = audioBuffer;
    source.connect(audioContext.destination);
    source.start();
}

// Ensure audio context starts after user interaction
document.addEventListener("click", function () {
    if (!audioContext) {
        loadAudio();
        console.log("Audio context initialized.");
    }
});
window.subscribeToPush = async function () {
    try {
        // First, check if push is supported
        if (!('serviceWorker' in navigator) || !('PushManager' in window)) {
            console.log('Push notifications not supported in this browser');
            return false;
        }
        console.log("Push notifications supported in this browser, requesting permission");
        // Request permission - same for all browsers
        const permission = await Notification.requestPermission();

        console.log('Notification permission:', permission);

        if (permission !== 'granted') {
            console.log('Notification permission denied');
            return false;
        }

        // Important iOS Safari requirements:
        // 1. Service worker must be already registered before subscribing
        // 2. Need to wait for service worker to be activated
        let registration;
        if (navigator.serviceWorker.controller) {
            // Service worker already controlling page
            registration = await navigator.serviceWorker.ready;
        } else {
            // Register and wait for it to become active
            registration = await navigator.serviceWorker.register('/service-worker.js');
            await new Promise(resolve => {
                if (registration.active) {
                    resolve();
                } else {
                    registration.addEventListener('updatefound', () => {
                        const newWorker = registration.installing;
                        newWorker.addEventListener('statechange', () => {
                            if (newWorker.state === 'activated') resolve();
                        });
                    });
                }
            });
        }

        const applicationServerKey = urlB64ToUint8Array(
            "BEj-Wiu59-OGKk2V4EbpdKX3V6ODV7JSaBj_rkjfvSXpJQsAtvSmgyjWyOWkF1RC6F5VtBSCquFDs6w7EmZ4J80"
        );

        // Subscribe to push
        const subscription = await registration.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: applicationServerKey
        });

        const response = await fetch('/api/messages/subscribe', {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(subscription)
        });

        console.log("Push subscription response:", response);

        if (response.ok) {
            return true;
        } else {
            console.error("Failed to save subscription on server:", response.statusText);
            return false;
        }
    } catch (error) {
        console.error('Error subscribing to push notifications:', error);
        return false;
    }
};

function urlB64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding)
        .replace(/\-/g, '+')
        .replace(/\_/g, '/'); 

    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);

    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}
