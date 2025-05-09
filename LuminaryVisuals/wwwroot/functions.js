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

window.subscribeToPush = async function() {
    try {
        const permission = await Notification.requestPermission();
        console.log('Notification permission:', permission);
        
        if (permission !== 'granted') {
            console.log('Notification permission denied');
            return false;
        }
        
        const registration = await navigator.serviceWorker.register('/service-worker.js');
        const subscription = await registration.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: "BEj-Wiu59-OGKk2V4EbpdKX3V6ODV7JSaBj_rkjfvSXpJQsAtvSmgyjWyOWkF1RC6F5VtBSCquFDs6w7EmZ4J80"
        });
        
        const response = await fetch('/api/messages/subscribe', {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(subscription)
        });
        
        console.log("Push subscription response:", response);
        
        // Check if the request was successful (status 2xx)
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


window.triggerInstallPrompt = function () {
    if (window.deferredPrompt) {
        // Show the installation prompt
        window.deferredPrompt.prompt();
        // Wait for the user to respond to the prompt
        window.deferredPrompt.userChoice.then((choiceResult) => {
            // Nullify the prompt object after the prompt has been handled
            window.deferredPrompt = null;
        });
    } else {
        // Call checkPwaSupport when installation isn't available
        window.checkPwaSupport();
    }
};

// Check for PWA support
window.checkPwaSupport = function () {
    if (!('beforeinstallprompt' in window)) {
        alert("This browser doesn't support PWA installation.\nif you are on ios, you have to click share and then add to home manually for the app to be installed");
    }
};