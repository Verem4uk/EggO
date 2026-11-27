(function (root) {
  "use strict";

  function createBackend(firebase, app) {
    const auth = app.auth();
    const db = app.firestore();
    const timestamp = () => firebase.firestore.FieldValue.serverTimestamp();
    const profile = uid => db.collection("eggoUsers").doc(uid);
    const normalLevel = value => Number.isInteger(value) && value >= 0 && value <= 7 ? value : 0;
    const millis = value => value && typeof value.toMillis === "function" ? value.toMillis() : 0;

    async function updateProgress(uid, requestedLevel) {
      return db.runTransaction(async function (transaction) {
        const ref = profile(uid);
        const snapshot = await transaction.get(ref);
        if (!snapshot.exists) {
          const completedLevel = Math.min(1, normalLevel(requestedLevel));
          transaction.set(ref, { schemaVersion: 1, completedLevel, createdAt: timestamp(), updatedAt: timestamp() });
          return completedLevel;
        }
        const current = normalLevel(snapshot.data().completedLevel);
        const completedLevel = Math.max(current, normalLevel(requestedLevel));
        if (completedLevel !== current) transaction.update(ref, { completedLevel, updatedAt: timestamp() });
        return completedLevel;
      });
    }

    return {
      observeAuth: callback => auth.onAuthStateChanged(callback),
      currentUser: () => auth.currentUser,
      signIn: () => auth.signInWithPopup(new firebase.auth.GoogleAuthProvider()),
      signOut: () => auth.signOut(),
      ensureProfile: updateProgress,
      saveProgress: updateProgress,
      watchProgress: (uid, next, error) => profile(uid).onSnapshot(snapshot => {
        if (!snapshot.exists) { error(new Error("Profile is missing")); return; }
        next(normalLevel(snapshot.data().completedLevel));
      }, error),
      watchAccess: (uid, next, error) => profile(uid).collection("entitlements").doc("current")
        .onSnapshot(snapshot => {
          const data = snapshot.exists ? snapshot.data() : {};
          const levelExpiresAt = {};
          for (let level = 3; level <= 7; level++) {
            levelExpiresAt[String(level)] = millis((data.levelExpiresAt || {})[String(level)]);
          }
          next({ permanent: data.permanent === true,
            subscriptionExpiresAt: millis(data.subscriptionExpiresAt), levelExpiresAt });
        }, error)
    };
  }

  root.EggoFirebaseBackend = { createBackend };
})(typeof globalThis !== "undefined" ? globalThis : this);
