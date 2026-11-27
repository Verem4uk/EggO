(function () {
  "use strict";
  const copy = {
    en: { account: "My EggO", title: "Your EggO", invitation: "Sign in to open level 2 for free and save your completed levels across devices.",
      signIn: "Continue with Google", signOut: "Sign out", close: "Close", loading: "Loading your account…", progress: "Completed levels: ",
      saved: "Progress saved", syncing: "Progress has not synced yet. Keep this browser's data until it is saved.",
      retry: "Try again", error: "Could not load or save your account. Check your connection and try again.",
      loginError: "Sign-in did not finish. Allow the Google sign-in window and try again.", unavailable: "Sign-in is not available. You can still play level 1.",
      noAccess: "No paid access yet", permanent: "Permanent access", subscription: "Full access until ", dayPass: "Level access until ",
      unsavedLogout: "Some progress has not synced. Try again before signing out." },
    pl: { account: "Moje EggO", title: "Twoje EggO", invitation: "Zaloguj się, aby bezpłatnie otworzyć poziom 2 i zapisywać ukończone poziomy na różnych urządzeniach.",
      signIn: "Kontynuuj z Google", signOut: "Wyloguj się", close: "Zamknij", loading: "Wczytywanie konta…", progress: "Ukończone poziomy: ",
      saved: "Postęp zapisany", syncing: "Postęp nie został jeszcze zsynchronizowany. Zachowaj dane tej przeglądarki do czasu zapisu.",
      retry: "Spróbuj ponownie", error: "Nie udało się wczytać lub zapisać konta. Sprawdź połączenie i spróbuj ponownie.",
      loginError: "Logowanie nie zostało zakończone. Zezwól na okno logowania Google i spróbuj ponownie.", unavailable: "Logowanie jest niedostępne. Nadal możesz grać na poziomie 1.",
      noAccess: "Brak płatnego dostępu", permanent: "Stały dostęp", subscription: "Pełny dostęp do ", dayPass: "Dostęp do poziomu do ",
      unsavedLogout: "Część postępu nie została zsynchronizowana. Spróbuj ponownie przed wylogowaniem." },
    ru: { account: "Мой EggO", title: "Твой EggO", invitation: "Войди, чтобы открыть второй уровень бесплатно и сохранять пройденные уровни между устройствами.",
      signIn: "Продолжить с Google", signOut: "Выйти", close: "Закрыть", loading: "Загружаем аккаунт…", progress: "Пройдено уровней: ",
      saved: "Прогресс сохранён", syncing: "Прогресс ещё не синхронизирован. Не очищай данные этого браузера до сохранения.",
      retry: "Повторить", error: "Не удалось загрузить или сохранить аккаунт. Проверь соединение и попробуй снова.",
      loginError: "Вход не завершён. Разреши окно входа Google и попробуй снова.", unavailable: "Вход пока недоступен. Первый уровень можно пройти без регистрации.",
      noAccess: "Платного доступа пока нет", permanent: "Постоянный доступ", subscription: "Полный доступ до ", dayPass: "Доступ к уровню до ",
      unsavedLogout: "Часть прогресса ещё не синхронизирована. Повтори сохранение перед выходом." }
  };
  let language = "en";
  let unity;
  let receiver;
  let store;
  let requestedRegistration = false;
  let busy = false;
  let signInError = "";
  let previousFocus;
  let lastState = { ready: false, uid: "", name: "", completedLevel: 0, access: {}, syncing: false, error: "" };
  const text = () => copy[language];
  const button = document.getElementById("eggo-account-button");
  const dialog = document.getElementById("eggo-account-dialog");
  const title = document.getElementById("eggo-account-title");
  const description = document.getElementById("eggo-account-description");
  const status = document.getElementById("eggo-account-status");
  const access = document.getElementById("eggo-account-access");
  const error = document.getElementById("eggo-account-error");
  const signIn = document.getElementById("eggo-sign-in");
  const signOut = document.getElementById("eggo-sign-out");
  const retry = document.getElementById("eggo-account-retry");
  const close = document.getElementById("eggo-account-close");

  function notify() {
    if (!unity || !receiver) return;
    unity.SendMessage(receiver, "ReceiveAccountState", JSON.stringify({
      ready: lastState.ready, uid: lastState.uid, completedLevel: lastState.completedLevel
    }));
  }
  function render() {
    const t = text();
    button.textContent = t.account;
    title.textContent = t.title;
    description.textContent = lastState.uid ? lastState.name : t.invitation;
    status.textContent = !store ? t.unavailable : !lastState.ready ? t.loading : !lastState.uid ? "" :
      t.progress + lastState.completedLevel + "/7. " + (lastState.syncing ? t.syncing : t.saved);
    const lines = [];
    const grants = lastState.access || {};
    const format = value => new Date(value).toLocaleString(language);
    if (lastState.uid && lastState.ready) {
      if (grants.permanent) lines.push(t.permanent);
      else {
        if (grants.subscriptionExpiresAt > Date.now()) lines.push(t.subscription + format(grants.subscriptionExpiresAt));
        for (const [level, expiry] of Object.entries(grants.levelExpiresAt || {})) {
          if (expiry > Date.now()) lines.push(level + ": " + t.dayPass + format(expiry));
        }
      }
      if (!lines.length) lines.push(t.noAccess);
    }
    access.textContent = lines.join("\n");
    error.textContent = signInError || (lastState.error ? t.error : "");
    signIn.textContent = t.signIn;
    signIn.hidden = !!lastState.uid;
    signIn.disabled = busy || !store;
    signOut.textContent = t.signOut;
    signOut.hidden = !lastState.uid;
    signOut.disabled = busy || lastState.syncing;
    retry.textContent = t.retry;
    retry.hidden = !store || !(lastState.error || lastState.syncing);
    retry.disabled = busy;
    close.textContent = t.close;
    button.dataset.sync = lastState.error || lastState.syncing ? "pending" : "saved";
  }
  function open() {
    if (!dialog.open) {
      previousFocus = document.activeElement;
      dialog.showModal();
    }
    render();
  }
  function dismiss(cancelRegistration) {
    dialog.close();
    if (cancelRegistration && requestedRegistration && unity && receiver) {
      unity.SendMessage(receiver, "CancelAccountRegistration");
    }
    requestedRegistration = false;
    if (previousFocus) previousFocus.focus();
  }

  // The bridge exists even if a CDN is blocked: guest play remains available,
  // while paid access and registration fail closed.
  window.EggoAccount = {
    attachUnity: function (instance) { unity = instance; notify(); },
    registerUnity: function (objectName) { receiver = objectName; notify(); },
    getState: () => lastState,
    hasAccess: level => store ? store.hasAccess(level) : level === 1,
    saveProgress: level => store ? store.saveProgress(level) : Promise.resolve(false),
    requestRegistration: function () { requestedRegistration = true; open(); },
    setLanguage: function (index) { language = ["en", "pl", "ru"][index] || "en"; render(); }
  };
  button.addEventListener("click", open);
  close.addEventListener("click", () => dismiss(true));
  dialog.addEventListener("cancel", event => { event.preventDefault(); dismiss(true); });
  signIn.addEventListener("click", async function () {
    if (!store || busy) return;
    busy = true;
    signInError = "";
    render();
    try { await store.signIn(); }
    catch (failure) {
      if (failure.code !== "auth/popup-closed-by-user" && failure.code !== "auth/cancelled-popup-request") {
        signInError = text().loginError;
        console.error("EggO sign-in failed:", failure.code || "unknown");
      }
    } finally { busy = false; render(); }
  });
  signOut.addEventListener("click", async function () {
    if (!store || busy) return;
    if (lastState.syncing) { signInError = text().unsavedLogout; render(); return; }
    busy = true;
    render();
    try { await store.signOut(); }
    catch (_) { signInError = text().error; }
    finally { busy = false; render(); }
  });
  retry.addEventListener("click", async function () {
    if (!store || busy) return;
    busy = true;
    signInError = "";
    render();
    try { await store.retry(); } finally { busy = false; render(); }
  });
  window.addEventListener("online", () => { if (store) void store.retry(); });

  try {
    const app = firebase.initializeApp(window.EGGO_FIREBASE_CONFIG);
    const backend = EggoFirebaseBackend.createBackend(firebase, app);
    // No sign-in credentials are stored by our code; Firebase manages Auth.
    let storage;
    try { storage = window.localStorage; } catch (_) { storage = { getItem: () => null, setItem: () => {} }; }
    store = EggoAccountStore.createStore(backend, storage);
    store.subscribe(function (state) {
      lastState = state;
      render();
      notify();
      if (requestedRegistration && state.uid && state.ready) dismiss(false);
    });
    store.start();
    if (firebase.analytics && firebase.analytics.isSupported) {
      firebase.analytics.isSupported().then(supported => {
        if (supported) app.analytics();
      }).catch(() => {});
    }
  } catch (failure) {
    console.error("EggO account setup failed:", failure.code || failure.name);
    lastState.ready = true;
    render();
  }
  window.sendAnalyticsEvent = function (eventName, level, duration) {
    try {
      if (window.firebase && firebase.apps.length) firebase.analytics().logEvent(eventName, { level, duration });
    } catch (_) { /* Analytics never gates play or saving. */ }
  };
  render();
})();
