const { test } = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const vm = require("node:vm");
const path = require("node:path");

function setup(initial) {
  const writes = [];
  let record = initial;
  let progressCallback, accessCallback;
  const ref = {
    onSnapshot(next) { progressCallback = next; return () => {}; },
    collection(name) {
      assert.equal(name, "entitlements");
      return { doc(name) {
        assert.equal(name, "current");
        return { onSnapshot(next) { accessCallback = next; return () => {}; } };
      } };
    }
  };
  const db = {
    collection(name) { assert.equal(name, "eggoUsers"); return { doc: uid => { assert.equal(uid, "alice"); return ref; } }; },
    runTransaction: async callback => callback({
      get: async () => ({ exists: !!record, data: () => record }),
      set(target, value) { assert.equal(target, ref); record = value; writes.push(value); },
      update(target, value) { assert.equal(target, ref); record = { ...record, ...value }; writes.push(value); }
    })
  };
  const context = { firebase: {}, console };
  vm.createContext(context);
  vm.runInContext(fs.readFileSync(path.join(__dirname, "../../Assets/WebGLTemplates/EggOAccount/firebase-backend.js"), "utf8"), context);
  const firebase = { firestore: { FieldValue: { serverTimestamp: () => "SERVER_TIME" } } };
  const backend = context.EggoFirebaseBackend.createBackend(firebase, { auth: () => ({}), firestore: () => db });
  return { backend, writes, getRecord: () => record,
    sendAccess: data => accessCallback({ exists: !!data, data: () => data }),
    sendProgress: data => progressCallback({ exists: !!data, data: () => data }) };
}

test("new profile cannot import paid progress and uses server timestamps", async () => {
  const app = setup();
  assert.equal(await app.backend.ensureProfile("alice", 7), 1);
  assert.equal(app.getRecord().completedLevel, 1);
  assert.equal(app.getRecord().schemaVersion, 1);
  assert.equal(app.getRecord().createdAt, "SERVER_TIME");
  assert.equal(app.getRecord().updatedAt, "SERVER_TIME");
});

test("transaction retains greater remote progress and avoids unnecessary writes", async () => {
  const app = setup({ completedLevel: 6 });
  assert.equal(await app.backend.saveProgress("alice", 2), 6);
  assert.equal(app.writes.length, 0);
  assert.equal(await app.backend.saveProgress("alice", 7), 7);
  assert.deepEqual(Object.keys(app.writes[0]).sort(), ["completedLevel", "updatedAt"]);
});

test("access reader accepts Firestore timestamps and missing access grants nothing", () => {
  const app = setup();
  let access;
  app.backend.watchAccess("alice", data => { access = data; }, assert.fail);
  app.sendAccess(null);
  assert.equal(access.permanent, false);
  assert.equal(access.subscriptionExpiresAt, 0);
  app.sendAccess({ permanent: true, subscriptionExpiresAt: { toMillis: () => 5000 },
    levelExpiresAt: { "3": { toMillis: () => 2000 }, "4": "not a timestamp" } });
  assert.equal(access.subscriptionExpiresAt, 5000);
  assert.equal(access.levelExpiresAt["3"], 2000);
  assert.equal(access.levelExpiresAt["4"], 0);
});

test("a missing profile reports a load error instead of showing false saved progress", () => {
  const app = setup();
  let error;
  app.backend.watchProgress("alice", assert.fail, failure => { error = failure; });
  app.sendProgress(null);
  assert.equal(error.message, "Profile is missing");
});
