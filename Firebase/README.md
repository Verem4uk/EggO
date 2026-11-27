# EggO accounts — first implementation step

This change implements Google sign-in and the highest completed level. It does
not create Stripe payments or replace the donation screen with a checkout.
Nothing in Firebase or the live website has been deployed by this change.

## Agreed product rules

- Level 1: guest access. Level 2: signed-in users, free.
- PLN 20: access to one selected level for **24 hours from confirmed payment**,
  including unlimited restarts. No card-sequence persistence.
- PLN 50/month: subscription to all seven levels.
- PLN 500: permanent access to all seven levels.
- Completion history survives expiry of paid access.
- Existing completion requirements and closing/reflection routes are unchanged.

Only the highest completed level is stored, matching the existing sequential map.
It is not a count of play sessions or an independent set of completed levels.

## Source and deployment boundary

`Assets/WebGLTemplates/EggOAccount` is the versioned WebGL template selected in
ProjectSettings. Unity copies its JS/CSS/config into the build output. The
previous PHP/HTML files in ignored `Build/` are **not** edited or deleted.
The new bridge does not trust `EGGO_TOKEN`, `EGGO_LEVEL` or `EGGO_TRIAL`.

Before publishing, build into a new directory and test it separately. Deploy the
complete new output, not just `index.html`. The Apache `DirectoryIndex` for that
directory must serve `index.html`, not legacy `access.php`; existing token links
and customers need an explicit migration decision before removing the old site.
Do not put `tokens.json`, service-account keys or Stripe secrets in public output.

The old hosting configuration uses COOP `same-origin` and COEP `require-corp`.
This non-threaded Unity project does not require cross-origin isolation. For the
new build, check Google popup behavior with COOP `same-origin-allow-popups` and
COEP `unsafe-none`. Do not change unrelated server settings. Also check Brotli
Content-Encoding, MIME types and cross-origin Addressables on the actual host.

## Firebase Console setup to review together

The public configuration reuses project `eggo-dfbed` from the existing pages.
No admin credential is required in the Unity player or browser.

1. Authentication → Sign-in method: Google enabled, support email configured.
2. Authentication → Settings → Authorized domains: the actual production and
   staging hostnames. Add localhost explicitly if testing locally requires it.
3. Firestore: confirm the existing database and its current production rules.
4. Merge the **eggoUsers namespace** from `firestore.rules` into current rules.
   Do NOT replace the whole production ruleset: Coffee already writes `reviews`
   and its current permissions have not been inspected. A broad existing
   wildcard allow would also undermine the new rules, since allows are additive.
5. Test rules in the emulator before deployment: owner can read own profile and
   advance free progress; foreign users and guests cannot read it; progress
   cannot regress or exceed 7; clients cannot write purchases or entitlements;
   paid progress requires a valid server-managed entitlement.

The existing pinned Firebase 9.23 compat SDK family is retained to avoid a
separate SDK migration during this step. SDK upgrades can be evaluated later.

## Data model

`eggoUsers/{firebaseUid}`:

```text
schemaVersion: 1
completedLevel: integer 0..7
createdAt: server timestamp
updatedAt: server timestamp
```

Auth stores the Google identity; the profile does not duplicate email/name.
Profile creation imports only guest completion of level 1. Progress from old
token-based access is not silently imported into another account.

`eggoUsers/{firebaseUid}/purchases/{orderId}` (future payment backend):

```text
kind: level_24h | subscription | permanent
level: 3..7 (level_24h only)
status: paid | refunded | ...
amountMinor: integer (e.g. 2000 groszy)
currency: pln
stripeCheckoutSessionId / stripePaymentIntentId / stripeSubscriptionId
paidAt / expiresAt: timestamps where applicable
```

`eggoUsers/{firebaseUid}/entitlements/current` (future payment backend):

```text
permanent: boolean
subscriptionExpiresAt: Firestore timestamp, or omitted
levelExpiresAt: map of level strings to Firestore timestamps
  "3": timestamp
  "4": timestamp
```

The player can read its access and purchase records but cannot write them.
Stripe fulfillment and purchase creation remain unimplemented in this step.
Entitlements are UI gating, not DRM: publicly shipped Unity content cannot be
made confidential by disabling a button. Any future paid server endpoint must
validate identity and access server-side as well.

## Progress synchronization

- Unity reports only already completed levels, using existing completion rules.
- Firestore transaction takes max(remote, reported), so another device cannot
  lower progress. A failed save keeps a per-user local pending value and a visible
  sync warning; Retry, reconnect and the next sign-in retry it.
- Pending work captures the UID; a late response cannot credit a different user.
- Switching accounts closes an active game, preventing cross-account completion.
- Guest progress is scoped separately; no paid progress is imported as guest data.
- A save not yet synced may be lost if browser storage is cleared. This is not
  an offline-first synchronization guarantee. Paid progress submitted after the
  entitlement has already expired will need a future trusted-session policy.

## Local checks

```powershell
node --test Tests/FirebaseAccount/*.test.cjs
dotnet run --project Tests/SessionRules/SessionRules.csproj
```

These checks do not replace live Auth, deployed Firestore rules, or browser QA.
Player-only C# compilation can also be checked in batch Unity with
`-executeMethod FirebaseAccountCompileCheck.Run` (without `-quit`). It compiles
WebGL scripts into `Library/FirebaseAccountCompile` and exits with a status code;
it does not build or publish a complete WebGL player.
Before calling the step complete on the website, test a fresh Google sign-in,
level 1 guest completion → registration, level 2 completion → reload, a second
device, switching two accounts, rejected/closed popup, blocked storage and a
temporary network failure. Test EN/PL/RU and mobile Safari/Chrome.
