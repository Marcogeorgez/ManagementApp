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
