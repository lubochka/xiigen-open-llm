# Layer 0-C — Knowledge-Edge Pairs (057)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: GCS (Google Cloud Storage) connector

---

## B17-0C-057-001 — GCS: non-trigger terms

**Proper nouns — no predicate:** GCS, Google Cloud Storage, Google Cloud
**Domain nouns — no predicate:** bucket, object, blob, lifecycle, versioning, IAM (Identity and Access Management), signed URL, HMAC key, pub/sub notification
**Acronyms — no predicate:** GCS (Google Cloud Storage), IAM (Identity and Access Management)

---

## B17-0C-057-002 — GCS: signed URL does not fire SECURITY

"Signed URLs" in GCS are pre-authorized URLs — they are not "tokens" in the Rule 6 sense. SECURITY does NOT fire from "signed URL" alone. "Access token" does fire SECURITY.

---

## B17-0C-057-003 — GCS: "HMAC key" vs HMAC validation phrase

"GCS HMAC key for webhook notifications" → no SECURITY (no HMAC validation phrase; no "token").
"Validate the HMAC-SHA256 signature on GCS notifications" → SECURITY fires (Rule 8: complete phrase).
"Service account access token for GCS" → SECURITY fires (Rule 6: "token" present).

---

## B17-0C-057-004 — GCS: service account OAuth fires AUTHORIZATION + SECURITY

"GCS with OAuth 2.0 service account token exchange":
- "OAuth" → AUTHORIZATION
- "token" → SECURITY
Both fire independently. DecisionCount includes both at 3.4 (two sub-entries at same sub-phase).
UniqueSubPhases counts 3.4 once; DecisionCount counts 4 (IMPLEMENT at 4.3, API at 3.3, SEC at 3.4, AUTH at 3.4).
