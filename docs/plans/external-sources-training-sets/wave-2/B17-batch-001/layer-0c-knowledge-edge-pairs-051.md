# Layer 0-C — Knowledge-Edge Pairs (051)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Firebase connector

---

## B17-0C-051-001 — Firebase: non-trigger terms

**Proper nouns — no predicate:** Firebase, Google Firebase
**Domain nouns — no predicate:** Realtime Database, Firestore, Cloud Messaging, push notification, document, collection, rule, storage bucket, function, emulator
**Acronyms — no predicate:** FCM (Firebase Cloud Messaging), SDK (Software Development Kit when standalone)

---

## B17-0C-051-002 — Firebase: "push" and "sync" do not fire IMPLEMENT

"Push data updates to Firebase" → no predicate ("push" is not an IMPLEMENT trigger).
"Sync offline state through Firebase" → no predicate ("sync" is not an IMPLEMENT trigger).
"Build the Firebase connector to push and sync data" → IMPLEMENT fires from "Build", not "push" or "sync".

---

## B17-0C-051-003 — Firebase: Firebase Admin SDK uses "service account token"

"Firebase Admin SDK service account authentication token" → SECURITY fires (Rule 6: "token" in compound).
"Firebase Admin SDK service account key" → SECURITY does NOT fire (no "token").
Teaching: Even the same Firebase concept (service account) fires SECURITY only when described with "token".

---

## B17-0C-051-004 — Firebase: OIDC in Firebase Auth does not fire AUTHORIZATION

Firebase Authentication supports OIDC providers. "OIDC" alone does not fire AUTHORIZATION.
"Firebase connector with OAuth 2.0 provider" → AUTHORIZATION fires.
