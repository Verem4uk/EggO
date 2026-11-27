mergeInto(LibraryManager.library, {
    OpenURLInNewTab: function(urlPtr) {
        var url = UTF8ToString(urlPtr);
        window.open(url, "_blank");
    },

    GetEggoLevel: function() {
        return window.EggoAccount ? window.EggoAccount.getState().completedLevel : 0;
    },

    GetEggoTrial: function() {
        return 1;
    },

    RegisterEggoAccountReceiver: function(objectNamePtr) {
        if (window.EggoAccount) window.EggoAccount.registerUnity(UTF8ToString(objectNamePtr));
    },

    IsEggoAccountReady: function() {
        return window.EggoAccount && window.EggoAccount.getState().ready ? 1 : 0;
    },

    IsEggoSignedIn: function() {
        return window.EggoAccount && window.EggoAccount.getState().uid ? 1 : 0;
    },

    CanPlayEggoLevel: function(level) {
        return window.EggoAccount ? (window.EggoAccount.hasAccess(level) ? 1 : 0) : (level === 1 ? 1 : 0);
    },

    RequestEggoRegistration: function() {
        if (window.EggoAccount) window.EggoAccount.requestRegistration();
        else console.error("EggO requires the EggOAccount WebGL template for sign-in.");
    },

    SaveEggoLevel: function(level) {
        if (window.EggoAccount) void window.EggoAccount.saveProgress(level);
    },

    SetEggoAccountLanguage: function(index) {
        if (window.EggoAccount) window.EggoAccount.setLanguage(index);
    }
});
