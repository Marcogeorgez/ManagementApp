window.getBrowserInfo = () => {
    var userAgent = navigator.userAgent;
    var browserInfo = {
        userAgent: userAgent,
        browserName: '',
        operatingSystem: getOperatingSystem(userAgent),

    };

    // Browser detection
    if (userAgent.indexOf("Chrome") > -1) {
        browserInfo.browserName = "Chrome";
        browserInfo.browserVersion = userAgent.match(/Chrome\/([\d.]+)/)[1];
    } else if (userAgent.indexOf("Firefox") > -1) {
        browserInfo.browserName = "Firefox";
        browserInfo.browserVersion = userAgent.match(/Firefox\/([\d.]+)/)[1];
    } else if (userAgent.indexOf("Safari") > -1 && userAgent.indexOf("Chrome") === -1) {
        browserInfo.browserName = "Safari";
        browserInfo.browserVersion = userAgent.match(/Version\/([\d.]+)/)[1];
    } else if (userAgent.indexOf("Edg") > -1) {
        browserInfo.browserName = "Edge";
        browserInfo.browserVersion = userAgent.match(/Edg\/([\d.]+)/)[1];
    } else if (userAgent.indexOf("MSIE") > -1 || userAgent.indexOf("Trident") > -1) {
        browserInfo.browserName = "Internet Explorer";
        browserInfo.browserVersion = userAgent.match(/(MSIE |rv:)([\d.]+)/)[2];
    } else {
        browserInfo.browserName = "Unknown";
        browserInfo.browserVersion = "Unknown";
    }

    // Return as a JSON string to Blazor
    return JSON.stringify(browserInfo);
};
window.getOperatingSystem = () => {
    var userAgent = navigator.userAgent; // Ensure userAgent is properly defined
    if (!userAgent) {
        return "Unknown OS"; // Fallback if userAgent is undefined
    }

    if (userAgent.indexOf("Windows NT 10.0") > -1) {
        return "Windows 10";
    } else if (userAgent.indexOf("Windows NT 6.1") > -1) {
        return "Windows 7";
    } else if (userAgent.indexOf("Windows NT 6.0") > -1) {
        return "Windows Vista";
    } else if (userAgent.indexOf("Mac OS X") > -1) {
        return "MacOS";
    } else if (userAgent.indexOf("Linux") > -1) {
        return "Linux";
    } else if (userAgent.indexOf("Android") > -1) {
        return "Android";
    } else if (userAgent.indexOf("iPhone") > -1 || userAgent.indexOf("iPad") > -1) {
        return "iOS";
    } else {
        return "Unknown OS";
    }
};
