# Layer 0-C — Knowledge-Edge Pairs (029)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Cloudinary media connector edge cases

---

## B17-0C-029-001 — Cloudinary: "SDK" is integration vocabulary

"SDK" shifts IMPLEMENT from 4.1 to 4.3:
- "Build Cloudinary SDK wrapper" → IMPLEMENT → 4.3 ("SDK" is integration vocabulary)
- "Build Cloudinary image processor" → IMPLEMENT → 4.1 (no integration vocabulary)
- "Build Cloudinary API client" → IMPLEMENT → 4.3 ("API" in external context is integration vocabulary)

SDK, like connector and webhook, is explicitly in the integration vocabulary list.

---

## B17-0C-029-002 — Cloudinary: "signed URL" — not a "token" trigger

"Build Cloudinary connector with signed URL webhook callbacks."
- "signed URL" does NOT contain "token" → SECURITY does NOT fire
- "webhook" → API_CONTRACT (Rule 5)
- "connector" → API_CONTRACT (Rule 4; fires once)

"signed URL" is a URL-signing mechanism. The word "signed" or "URL" does not fire SECURITY.
SECURITY fires on: "token", "authentication", "authenticate", "validate signature", "HMAC validation", "secure".

---

## B17-0C-029-003 — Cloudinary: "API secret token" — token unconditional

"Build Cloudinary SDK wrapper with API secret token validation."
- "API secret token" contains "token" → SECURITY fires (Rule 6)
- "validation" alone does NOT fire SECURITY (must be "validate signature" or "HMAC validation")
- "API secret" before "token" does not suppress SECURITY

SECURITY fires on the word "token" regardless of surrounding modifiers.

---

## B17-0C-029-004 — Cloudinary: QA absolute last position

Input: "Build Cloudinary connector, write upload tests, QA the delivery pipeline."
Predicates: [IMPLEMENT, API_CONTRACT, TEST, QA]
- QA fires on "QA" abbreviation (Rule 21; "quality assurance" written out does NOT fire QA)
- QAGatingFlow73 is always last-absolute — never in parallel with TestingFlow51 or any other flow
Correct orchestration: [4.3 → 3.3 → 5.1 → QA (last)]
