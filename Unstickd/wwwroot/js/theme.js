window.themeInterop = {
    saveTheme: function(themeJson) {
        localStorage.setItem('unstickd_theme', themeJson);
    },
    loadTheme: function() {
        return localStorage.getItem('unstickd_theme');
    }
};
