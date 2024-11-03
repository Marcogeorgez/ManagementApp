window.loadDarkModePreference = function () {
    // Retrieve the dark mode preference from cookies
    const darkModeCookie = document.cookie.split('; ').find(row => row.startsWith('DarkMode='));
    return darkModeCookie ? darkModeCookie.split('=')[1] === 'true' : false;
};

window.setDarkMode = function (isDarkMode) {
    // Set the dark mode preference in cookies
    document.cookie = `DarkMode=${isDarkMode}; path=/; max-age=${365 * 24 * 60 * 60}`;
    // Optionally, apply the dark mode style here, e.g.:
    if (isDarkMode) {
        document.body.classList.add('dark-mode');
    } else {
        document.body.classList.remove('dark-mode');
    }
};
