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