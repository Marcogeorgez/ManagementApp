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
    if (!audioContext || !audioBuffer) return;

    try {
        let source = audioContext.createBufferSource();
        source.buffer = audioBuffer;
        source.connect(audioContext.destination);
        source.start();
    }
    catch (err) {
        if (err.name === "NotAllowedError") {
            console.debug("Audio playback not allowed until user interaction.");
        } else {
            console.error("Audio playback failed:", err);
        }
    }
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

class Badger {
    constructor(options) {
        Object.assign(
            this, {
            backgroundColor: "#f00",
            color: "#fff",
            size: 0.72,      // 0..1 (Scale in respect to the favicon image size)
            position: "ne", // Position inside favicon "n", "e", "s", "w", "ne", "nw", "se", "sw"
            radius: 0.50,      // Border radius, better used as % value + 4px base
            src: "",        // Favicon source (dafaults to the <link> icon href),
            srcs: false,
            onChange() { },
        },
            options
        );
        this.canvas = document.createElement("canvas");
        this.ctx = this.canvas.getContext("2d");

        this.src = "";
        this.img = "";
        this.srcs = this.srcs || this.faviconELs;
    }

    faviconELs = document.querySelectorAll("link[rel$=icon]");

    _drawIcon() {
        this.ctx.clearRect(0, 0, this.faviconSize, this.faviconSize);
        this.ctx.drawImage(this.img, 0, 0, this.faviconSize, this.faviconSize);
    }

    _drawShape() {
        const r = Math.floor(this.faviconSize * this.radius) + 4;
        const xa = this.offset.x;
        const ya = this.offset.y;
        const xb = this.offset.x + this.badgeSize;
        const yb = this.offset.y + this.badgeSize;
        this.ctx.beginPath();
        this.ctx.moveTo(xb - r, ya);
        this.ctx.quadraticCurveTo(xb, ya, xb, ya + r);
        this.ctx.lineTo(xb, yb - r);
        this.ctx.quadraticCurveTo(xb, yb, xb - r, yb);
        this.ctx.lineTo(xa + r, yb);
        this.ctx.quadraticCurveTo(xa, yb, xa, yb - r);
        this.ctx.lineTo(xa, ya + r);
        this.ctx.quadraticCurveTo(xa, ya, xa + r, ya);
        this.ctx.fillStyle = this.backgroundColor;
        this.ctx.fill();
        this.ctx.closePath();
    }

    _drawVal() {
        const margin = (this.badgeSize * 0.18) / 2;
        this.ctx.beginPath();
        this.ctx.textBaseline = "middle";
        this.ctx.textAlign = "center";
        this.ctx.font = `bold ${this.badgeSize * 0.82}px Arial`;
        this.ctx.fillStyle = this.color;
        this.ctx.fillText(this.value, this.badgeSize / 2 + this.offset.x, this.badgeSize / 2 + this.offset.y + margin);
        this.ctx.closePath();
    }

    _drawFavicon() {
        this.src.setAttribute("href", this.dataURL);
    }

    _draw() {
        this._drawIcon();
        if (this.value) this._drawShape();
        if (this.value) this._drawVal();
    }

    _setup(el) {
        this.img = el.img;
        this.src = el.src;

        this.faviconSize = this.img.naturalWidth;
        this.badgeSize = this.faviconSize * this.size;
        this.canvas.width = this.faviconSize;
        this.canvas.height = this.faviconSize;
        const sd = this.faviconSize - this.badgeSize;
        const sd2 = sd / 2;
        this.offset = {
            n: { x: sd2, y: 0 },
            e: { x: sd, y: sd2 },
            s: { x: sd2, y: sd },
            w: { x: 0, y: sd2 },
            nw: { x: 0, y: 0 },
            ne: { x: sd, y: 0 },
            sw: { x: 0, y: sd },
            se: { x: sd, y: sd },
        }[this.position];
    }

    // Public functions / methods:
    imgs = [];
    updateAll() {
        this._value = Math.min(99, parseInt(this._value, 10));
        var self = this;

        if (this.imgs.length) {
            this.imgs.forEach(function (img) {
                self._setup(img);
                self._draw();
                self._drawFavicon();
            })
            if (this.onChange) this.onChange.call(this);
        } else {
            // load all
            this.srcs.forEach(function (src) {
                var img = {};
                img.img = new Image();
                img.img.addEventListener("load", () => {
                    self._setup(img);
                    self._draw();
                    self._drawFavicon();
                    if (self.onChange) self.onChange.call(self);
                });
                img.src = src;
                img.img.src = src.getAttribute("href");
                self.imgs.push(img);
            })
        }
    }

    get dataURL() {
        return this.canvas.toDataURL();
    }

    get value() {
        return this._value;
    }

    set value(val) {
        this._value = val;
        this.updateAll();
    }
}
window.Badger = Badger;
window.faviconBadger = {
    instance: null,

    init(options) {
        if (!window.Badger) return console.error("Badger class is not defined");
        this.instance = new Badger(options || {});
    },

    update(value) {
        if (!this.instance) {
            console.warn("Badger not initialized. Calling init with defaults.");
            this.init();
        }
        this.instance.value = value;
    }
};
