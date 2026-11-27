const { test } = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const vm = require("node:vm");
const path = require("node:path");

function setup(window) {
  const functions = {};
  vm.runInNewContext(fs.readFileSync(path.join(__dirname, "../../Assets/Plugins/WebGL/WebGLBridge.jslib"), "utf8"), {
    window, console, UTF8ToString: value => value, LibraryManager: { library: functions },
    mergeInto: (target, source) => Object.assign(target, source)
  });
  return functions;
}

test("missing account bridge never trusts legacy PHP token parameters", () => {
  const bridge = setup({ EGGO_LEVEL: 7, EGGO_TRIAL: false, EGGO_TOKEN: "legacy" });
  assert.equal(bridge.GetEggoLevel(), 0);
  assert.equal(bridge.IsEggoSignedIn(), 0);
  assert.equal(bridge.CanPlayEggoLevel(1), 1);
  assert.equal(bridge.CanPlayEggoLevel(2), 0);
  assert.equal(bridge.CanPlayEggoLevel(7), 0);
});

test("Unity bridge forwards progress and account events without credentials", () => {
  const calls = [];
  const bridge = setup({ EggoAccount: {
    getState: () => ({ uid: "alice", ready: true, completedLevel: 2 }),
    hasAccess: level => level <= 2,
    registerUnity: name => calls.push(["receiver", name]),
    requestRegistration: () => calls.push(["registration"]),
    saveProgress: level => calls.push(["save", level]),
    setLanguage: index => calls.push(["language", index])
  } });
  assert.equal(bridge.GetEggoLevel(), 2);
  assert.equal(bridge.IsEggoAccountReady(), 1);
  assert.equal(bridge.IsEggoSignedIn(), 1);
  assert.equal(bridge.CanPlayEggoLevel(3), 0);
  bridge.RegisterEggoAccountReceiver("Controller");
  bridge.SaveEggoLevel(2);
  bridge.RequestEggoRegistration();
  bridge.SetEggoAccountLanguage(2);
  assert.deepEqual(calls, [["receiver", "Controller"], ["save", 2], ["registration"], ["language", 2]]);
});
