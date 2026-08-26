mergeInto(LibraryManager.library, {
    OpenURLInNewTab: function(urlPtr) {
        var url = UTF8ToString(urlPtr);
        window.open(url, "_blank");
    },

    GetEggoLevel: function() {
        return window.EGGO_LEVEL || 0;
    },

    GetEggoTrial: function() {
        return window.EGGO_TRIAL ? 1 : 0;
    }
});