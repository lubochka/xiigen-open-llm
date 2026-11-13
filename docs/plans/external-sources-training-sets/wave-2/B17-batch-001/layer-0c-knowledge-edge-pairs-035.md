# Layer 0-C — Knowledge-Edge Pairs (035)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: GitHub source control connector edge cases

---

## B17-0C-035-001 — GitHub: "HMAC-SHA256 signature validation" fires SECURITY

"validate signature" and "HMAC validation" fire SECURITY (Rule 8).
- "HMAC-SHA256 signature validation" → SECURITY fires (contains "validate signature" pattern)
- "token" also fires SECURITY (Rule 6)
Both present in same input → SECURITY fires once (predicates are sets)

SECURITY fires from EITHER "token" OR "validate signature" OR "HMAC validation" OR "authenticate" — any single trigger is sufficient.

---

## B17-0C-035-002 — GitHub: "push", "PR", "issue" are domain nouns

GitHub-specific domain vocabulary that fires NO predicate:
- push event, pull request (PR), issue, commit, repository, branch, merge
None of these are predicate triggers. They describe the data model of GitHub, not the predicate vocabulary.

---

## B17-0C-035-003 — GitHub: "wire up" as the only verb — no IMPLEMENT

Input: "Wire up the GitHub webhook endpoint and add tests for push and PR events."
- "Wire up" → API_CONTRACT (Rule 22) — like "integrate"; never fires IMPLEMENT
- "add tests" → TEST (Rule 13)
- No build verb → IMPLEMENT absent

Predicates: [API_CONTRACT, TEST]
TransitionType: documentation (no IMPLEMENT = documentation for this input shape)

---

## B17-0C-035-004 — GitHub: "GitHub Actions" is a product name — no predicate

"GitHub Actions" is a CI/CD (Continuous Integration/Continuous Deployment) product.
"actions" does NOT fire any predicate. "GitHub" proper noun: transparent.
Input: "Build GitHub Actions connector" → IMPLEMENT → 4.3 only because of "connector".
