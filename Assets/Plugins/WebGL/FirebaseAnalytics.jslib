mergeInto(LibraryManager.library, {
SendAnalyticsEvent: function(eventNamePtr, level, duration) {
        var eventName = UTF8ToString(eventNamePtr);

        if (window.sendAnalyticsEvent)
        {
            window.sendAnalyticsEvent(eventName, level, duration);
        }
    }
});