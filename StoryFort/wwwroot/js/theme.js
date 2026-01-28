window.themeInterop = {
    saveTheme: function(themeJson) {
        localStorage.setItem('StoryFort_theme', themeJson);
    },
    loadTheme: function() {
        return localStorage.getItem('StoryFort_theme');
    }
};

