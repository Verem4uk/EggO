const { test } = require("node:test");
const assert = require("node:assert/strict");
const { createStore } = require("../../Assets/WebGLTemplates/EggOAccount/account-store.js");

const tick = () => new Promise(resolve => setImmediate(resolve));
function deferred() {
  let resolve, reject;
  const promise = new Promise((yes, no) => { resolve = yes; reject = no; });
  return { promise, resolve, reject };
}
function setup(initial = {}) {
  const data = new Map(Object.entries(initial));
  const profiles = new Map();
  const accesses = new Map();
  const watchers = [];
  const saves = [];
  let authCallback, user = null;
  const backend = {
    observeAuth(callback) { authCallback = callback; callback(user); return () => {}; },
    currentUser: () => user,
    async ensureProfile(uid, guest) {
      const value = Math.max(profiles.get(uid) || 0, guest);
      profiles.set(uid, value);
      return value;
    },
    watchProgress(uid, next, error) {
      const watch = { uid, next, error, kind: "progress", active: true };
      watchers.push(watch);
      next(profiles.get(uid) || 0);
      return () => { watch.active = false; };
    },
    watchAccess(uid, next, error) {
      const watch = { uid, next, error, kind: "access", active: true };
      watchers.push(watch);
      next(accesses.get(uid) || {});
      return () => { watch.active = false; };
    },
    async saveProgress(uid, requested) {
      saves.push([uid, requested]);
      const value = Math.max(profiles.get(uid) || 0, requested);
      profiles.set(uid, value);
      return value;
    },
    signIn: async () => {},
    signOut: async () => { user = null; authCallback(null); }
  };
  const storage = { getItem: key => data.get(key) || null, setItem: (key, value) => data.set(key, value) };
  let currentTime = 1000;
  const store = createStore(backend, storage, () => currentTime);
  store.start();
  return { store, backend, profiles, accesses, watchers, saves, data,
    setTime: value => { currentTime = value; },
    async login(uid) { user = { uid, displayName: uid }; authCallback(user); await tick(); }
  };
}

test("guest can play only level 1, and its completion survives a new store", async () => {
  const app = setup();
  assert.equal(app.store.hasAccess(1), true);
  assert.equal(app.store.hasAccess(2), false);
  await app.store.saveProgress(1);
  assert.equal(app.data.get("eggo.guest.completedLevel"), "1");
  const restarted = setup(Object.fromEntries(app.data));
  assert.equal(restarted.store.getState().completedLevel, 1);
});

test("registration imports only guest level 1 and saves completed level 2", async () => {
  const app = setup({ "eggo.guest.completedLevel": "7" });
  await app.login("alice");
  assert.equal(app.store.getState().completedLevel, 1);
  assert.equal(app.store.hasAccess(2), true);
  assert.equal(app.store.hasAccess(3), false);
  await app.store.saveProgress(2);
  assert.equal(app.profiles.get("alice"), 2);
  assert.equal(app.store.getState().syncing, false);
});

test("locked paid levels cannot record a new completion", async () => {
  const app = setup();
  await app.login("alice");
  assert.equal(await app.store.saveProgress(3), false);
  assert.deepEqual(app.saves, []);
});

test("24-hour access is scoped to its level and expires without a reload", async () => {
  const app = setup();
  app.accesses.set("alice", { levelExpiresAt: { "3": 2000 } });
  await app.login("alice");
  assert.equal(app.store.hasAccess(3), true);
  assert.equal(app.store.hasAccess(4), false);
  app.setTime(2000);
  assert.equal(app.store.hasAccess(3), false);
});

test("subscription and permanent access grant all seven levels", async () => {
  for (const access of [{ subscriptionExpiresAt: 2000 }, { permanent: true }]) {
    const app = setup();
    app.accesses.set("alice", access);
    await app.login("alice");
    for (let level = 1; level <= 7; level++) assert.equal(app.store.hasAccess(level), true);
    assert.equal(app.store.hasAccess(8), false);
    assert.equal(app.store.hasAccess(-1), false);
  }
});

test("an expired purchase does not erase previously completed levels", async () => {
  const app = setup();
  app.profiles.set("alice", 5);
  await app.login("alice");
  assert.equal(app.store.getState().completedLevel, 5);
  assert.equal(app.store.hasAccess(5), false);
  await app.store.saveProgress(2);
  assert.equal(app.store.getState().completedLevel, 5);
});

test("switching users does not import another account's progress or paid access", async () => {
  const app = setup();
  app.profiles.set("alice", 7);
  app.accesses.set("alice", { permanent: true });
  await app.login("alice");
  await app.login("bob");
  assert.equal(app.store.getState().completedLevel, 0);
  assert.equal(app.store.hasAccess(3), false);
  const stale = app.watchers.find(watch => watch.uid === "alice" && watch.kind === "progress");
  stale.next(7);
  assert.equal(app.store.getState().completedLevel, 0);
});

test("a late profile load cannot restore a signed-out user's state", async () => {
  const app = setup();
  const load = deferred();
  app.backend.ensureProfile = () => load.promise;
  await app.login("alice");
  await app.store.signOut();
  load.resolve(7);
  await tick();
  assert.equal(app.store.getState().uid, "");
  assert.equal(app.store.getState().completedLevel, 0);
});

test("failed saves remain visibly pending and are retried after reconnect", async () => {
  const app = setup();
  await app.login("alice");
  const save = app.backend.saveProgress;
  app.backend.saveProgress = async () => { throw new Error("offline"); };
  assert.equal(await app.store.saveProgress(2), false);
  assert.equal(app.store.getState().completedLevel, 2);
  assert.equal(app.store.getState().error, "progress-save");
  assert.equal(app.store.getState().syncing, true);
  assert.equal(app.data.get("eggo.pending.completedLevel.alice"), "2");
  app.backend.saveProgress = save;
  assert.equal(await app.store.retry(), true);
  assert.equal(app.profiles.get("alice"), 2);
  assert.equal(app.store.getState().error, "");
  assert.equal(app.store.getState().syncing, false);
});

test("pending saves survive a restart and stay scoped to their owner", async () => {
  const app = setup({ "eggo.pending.completedLevel.alice": "2" });
  await app.login("bob");
  assert.deepEqual(app.saves, []);
  assert.equal(app.store.getState().completedLevel, 0);
  await app.login("alice");
  assert.deepEqual(app.saves, [["alice", 2]]);
  assert.equal(app.profiles.get("alice"), 2);
});

test("a save completing after account switch cannot modify the new account", async () => {
  const app = setup();
  await app.login("alice");
  const save = deferred();
  app.backend.saveProgress = () => save.promise;
  const completion = app.store.saveProgress(2);
  await tick();
  await app.login("bob");
  save.resolve(2);
  await completion;
  assert.equal(app.store.getState().uid, "bob");
  assert.equal(app.store.getState().completedLevel, 0);
  assert.equal(app.data.get("eggo.pending.completedLevel.bob"), undefined);
});

test("progress never regresses when two saves overlap", async () => {
  const app = setup();
  app.accesses.set("alice", { permanent: true });
  await app.login("alice");
  await Promise.all([app.store.saveProgress(3), app.store.saveProgress(4)]);
  assert.equal(app.profiles.get("alice"), 4);
  assert.equal(app.store.getState().completedLevel, 4);
  assert.equal(app.store.getState().syncing, false);
});

test("failed account load does not grant access, and retry recovers", async () => {
  const app = setup();
  const load = app.backend.ensureProfile;
  app.backend.ensureProfile = async () => { throw new Error("permission-denied"); };
  await app.login("alice");
  assert.equal(app.store.getState().ready, false);
  assert.equal(app.store.hasAccess(2), false);
  assert.equal(app.store.hasAccess(1), true);
  app.backend.ensureProfile = load;
  await app.store.retry();
  assert.equal(app.store.hasAccess(2), true);
});

test("sign-out restores guest progress rather than leaking account progress", async () => {
  const app = setup({ "eggo.guest.completedLevel": "1" });
  app.profiles.set("alice", 7);
  await app.login("alice");
  await app.store.signOut();
  assert.equal(app.store.getState().completedLevel, 1);
  assert.equal(app.store.hasAccess(2), false);
});

test("unavailable browser storage does not prevent guest play or account loading", async () => {
  let authCallback;
  const backend = {
    observeAuth(callback) { authCallback = callback; callback(null); return () => {}; }
  };
  const storage = { getItem() { throw new Error(); }, setItem() { throw new Error(); } };
  const store = createStore(backend, storage);
  store.start();
  assert.equal(store.hasAccess(1), true);
  assert.equal(await store.saveProgress(1), true);
  assert.equal(store.getState().completedLevel, 1);
});

test("signed-in progress is sent to backend even if localStorage is unavailable", async () => {
  let auth;
  const saves = [];
  const backend = {
    observeAuth(callback) { auth = callback; callback({ uid: "alice" }); return () => {}; },
    ensureProfile: async () => 0,
    watchProgress(uid, next) { next(0); return () => {}; },
    watchAccess(uid, next) { next({}); return () => {}; },
    async saveProgress(uid, value) { saves.push([uid, value]); return value; }
  };
  const storage = { getItem() { throw new Error(); }, setItem() { throw new Error(); } };
  const store = createStore(backend, storage);
  store.start();
  await tick();
  assert.equal(await store.saveProgress(2), true);
  assert.deepEqual(saves, [["alice", 2]]);
  assert.equal(store.getState().syncing, false);
});

test("guest completion during initial Auth load is retained for registration", async () => {
  let auth;
  const imported = [];
  const backend = {
    observeAuth(callback) { auth = callback; return () => {}; },
    async ensureProfile(uid, value) { imported.push(value); return value; },
    watchProgress(uid, next) { next(1); return () => {}; },
    watchAccess(uid, next) { next({}); return () => {}; }
  };
  const data = new Map();
  const store = createStore(backend, { getItem: key => data.get(key), setItem: (key, value) => data.set(key, value) });
  store.start();
  assert.equal(store.getState().ready, false);
  await store.saveProgress(1);
  auth({ uid: "alice" });
  await tick();
  assert.deepEqual(imported, [1]);
});
