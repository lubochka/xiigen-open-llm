# Layer 3 — Teaching Unit Pairs (029)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: teaching-unit | focus: Cloudinary connector

---

## B17-TU-029-001 — Cloudinary: API secret is not a token

**A:** "Build the Cloudinary connector using the API secret for image transformations." Does SECURITY fire?

**B:** No. "API secret" does not contain "token" and is not an authenticate/HMAC phrase. SECURITY does NOT fire. CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]. DecisionCount: 2.

---

## B17-TU-029-002 — Cloudinary: "API secret token" fires SECURITY

**A:** "Build the Cloudinary connector with API secret token for signed uploads." Does SECURITY fire?

**B:** Yes. "API secret token" contains "token" → SECURITY fires (Rule 6). CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]. DecisionCount: 3.

Teaching: The word "token" must be explicitly present. "API secret" alone is not sufficient. "API secret token" has "token" → SECURITY fires.

---

## B17-TU-029-003 — Cloudinary: "media upload" does not fire

**A:** "Upload media to Cloudinary and generate transformation URLs." Does IMPLEMENT fire?

**B:** No. "Upload" is not an IMPLEMENT trigger. "Generate" is not an IMPLEMENT trigger. "Build the Cloudinary connector to handle media uploads" → IMPLEMENT fires from "Build".

---

## B17-TU-029-004 — Cloudinary: "integrate" rule

**A:** "Integrate Cloudinary for image management." What fires?

**B:** API_CONTRACT only. Rule 1: "integrate" → ["API_CONTRACT"] → 3.3 → documentation.
