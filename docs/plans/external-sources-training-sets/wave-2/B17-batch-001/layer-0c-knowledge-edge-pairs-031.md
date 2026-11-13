# Layer 0-C — Knowledge-Edge Pairs (031)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Segment analytics connector edge cases

---

## B17-0C-031-001 — Segment: "write key" is not a "token" trigger

"Write key" and "API write key" do NOT contain "token" → SECURITY does NOT fire from "write key" alone.
Input: "Build Segment connector with write key."
Predicates: [IMPLEMENT, API_CONTRACT]

Compare: "Build Segment connector with write key token authentication."
- "token" present → SECURITY fires
- "authentication" → SECURITY reinforces; fires once
Predicates: [IMPLEMENT, API_CONTRACT, SECURITY]

Key edge: SECURITY requires the specific words "token", "authentication", "authenticate", "validate signature", "HMAC", or "secure". "Write key" alone is not enough.

---

## B17-0C-031-002 — Segment: "track", "identify", "group", "batch" are method names

These are Segment API method names — domain nouns, not predicates.
- "track" does NOT fire any predicate (not a build verb in this corpus)
- "identify" does NOT fire IMPLEMENT (not in the IMPLEMENT trigger set)
- "batch" does NOT fire any predicate (not in any trigger set)

Only "build", "implement", "create", "code", "develop", "write", "refactor", "restructure", "rewrite" fire IMPLEMENT.

---

## B17-0C-031-003 — Segment: refactor TransitionType overrides feature_addition

Input: "Refactor the Segment connector and write unit tests."
- "Refactor" → IMPLEMENT with TransitionType=refactoring
- "connector" → integration vocabulary → 4.3
- "write unit tests" → TEST

TransitionType: refactoring (not feature_addition — refactoring > feature_addition in hierarchy)
Even though TEST is present, TransitionType is determined by the highest-ranked verb in the hierarchy.
