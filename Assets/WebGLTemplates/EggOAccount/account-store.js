/* Account state is kept outside Unity so Auth and Firestore use the web SDK. */
(function (root, factory) {
  if (typeof module === "object" && module.exports) module.exports = factory();
  else root.EggoAccountStore = factory();
})(typeof globalThis !== "undefined" ? globalThis : this, function () {
  "use strict";

  function level(value) {
    return Number.isInteger(value) && value >= 0 && value <= 7 ? value : 0;
  }

  function createStore(backend, storage, clock) {
    const now = clock || Date.now;
    const listeners = new Set();
    const memory = new Map();
    let generation = 0;
    let subscriptions = [];
    let saveQueue = Promise.resolve();
    let authSubscription;
    let state = { ready: false, uid: "", name: "", completedLevel: 0,
      access: {}, error: "", syncing: false };

    function read(key) {
      if (memory.has(key)) return memory.get(key);
      try { return level(Number(storage.getItem(key))); } catch (_) { return 0; }
    }
    function write(key, value) {
      memory.set(key, value);
      try { storage.setItem(key, String(value)); } catch (_) { /* Storage may be disabled. */ }
    }
    function guestLevel() { return Math.min(1, read("eggo.guest.completedLevel")); }
    function pendingKey(uid) { return "eggo.pending.completedLevel." + uid; }
    function emit() { for (const listener of listeners) listener(getState()); }
    function getState() { return { ...state, access: { ...state.access } }; }
    function active(epoch, uid) { return epoch === generation && uid === state.uid; }

    function hasAccess(requestedLevel) {
      if (!Number.isInteger(requestedLevel) || requestedLevel < 1 || requestedLevel > 7) return false;
      if (requestedLevel === 1) return true;
      if (!state.ready || !state.uid) return false;
      if (requestedLevel === 2) return true;
      const access = state.access;
      return access.permanent === true || Number(access.subscriptionExpiresAt) > now() ||
        Number((access.levelExpiresAt || {})[String(requestedLevel)]) > now();
    }

    // Capture the owner when scheduling. An outstanding save must never be
    // redirected to a newly signed-in account.
    function flushPending() {
      const uid = state.uid;
      const epoch = generation;
      if (!state.ready || !uid) return Promise.resolve(false);
      const target = read(pendingKey(uid));
      if (!target) return Promise.resolve(true);
      state.syncing = true;
      emit();
      const operation = saveQueue.catch(() => {}).then(async function () {
        if (!active(epoch, uid)) return false;
        try {
          const saved = level(await backend.saveProgress(uid, target));
          if (!active(epoch, uid)) return false;
          if (read(pendingKey(uid)) <= saved) write(pendingKey(uid), 0);
          state.completedLevel = Math.max(state.completedLevel, saved);
          state.error = "";
          return true;
        } catch (_) {
          if (active(epoch, uid)) state.error = "progress-save";
          return false;
        } finally {
          if (active(epoch, uid)) {
            state.syncing = read(pendingKey(uid)) > 0;
            emit();
          }
        }
      });
      saveQueue = operation;
      return operation;
    }

    async function switchUser(user) {
      const epoch = ++generation;
      subscriptions.forEach(unsubscribe => unsubscribe());
      subscriptions = [];
      state = { ready: !user, uid: user ? user.uid : "", name: user ? user.displayName || "" : "",
        completedLevel: user ? 0 : guestLevel(), access: {}, error: "", syncing: false };
      emit();
      if (!user) return;
      const uid = user.uid;
      try {
        // Import only guest level 1, never progress belonging to another account.
        const saved = level(await backend.ensureProfile(uid, guestLevel()));
        if (!active(epoch, uid)) return;
        state.completedLevel = Math.max(saved, read(pendingKey(uid)));
        let profileReady = false;
        let accessReady = false;
        let initiallyReady = false;
        function publish() {
          state.ready = profileReady && accessReady;
          emit();
          if (state.ready && !initiallyReady) {
            initiallyReady = true;
            void flushPending();
          }
        }
        function failed() {
          if (!active(epoch, uid)) return;
          state.ready = false;
          state.access = {};
          state.error = "account-load";
          emit();
        }
        subscriptions.push(backend.watchProgress(uid, function (completedLevel) {
          if (!active(epoch, uid)) return;
          state.completedLevel = Math.max(level(completedLevel), read(pendingKey(uid)));
          profileReady = true;
          publish();
        }, failed));
        subscriptions.push(backend.watchAccess(uid, function (access) {
          if (!active(epoch, uid)) return;
          state.access = access || {};
          accessReady = true;
          publish();
        }, failed));
      } catch (_) {
        if (!active(epoch, uid)) return;
        state.error = "account-load";
        emit();
      }
    }

    return {
      start: function () {
        if (!authSubscription) authSubscription = backend.observeAuth(user => { void switchUser(user); });
      },
      subscribe: function (listener) {
        listeners.add(listener);
        listener(getState());
        return () => listeners.delete(listener);
      },
      getState,
      hasAccess,
      saveProgress: function (completedLevel) {
        const value = level(completedLevel);
        if (!value) return Promise.resolve(false);
        if (!state.uid) {
          state.completedLevel = Math.max(guestLevel(), Math.min(value, 1));
          write("eggo.guest.completedLevel", state.completedLevel);
          emit();
          return Promise.resolve(true);
        }
        if (!state.ready) return Promise.resolve(false);
        // Previously earned progress remains valid after access expires. A new
        // completion, however, must belong to a level currently available.
        if (value > state.completedLevel && !hasAccess(value)) return Promise.resolve(false);
        state.completedLevel = Math.max(state.completedLevel, value);
        write(pendingKey(state.uid), Math.max(read(pendingKey(state.uid)), value));
        emit();
        return flushPending();
      },
      retry: function () {
        if (!state.ready) return switchUser(backend.currentUser());
        return flushPending();
      },
      signIn: () => backend.signIn(),
      signOut: () => backend.signOut(),
      dispose: function () {
        generation++;
        if (authSubscription) authSubscription();
        subscriptions.forEach(unsubscribe => unsubscribe());
        listeners.clear();
      }
    };
  }

  return { createStore };
});
