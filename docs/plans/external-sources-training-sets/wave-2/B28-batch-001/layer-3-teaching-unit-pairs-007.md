# Layer 3 — Teaching Unit Pairs (007)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU007-001 — Sub-phase 7.1 vs 7.3: release vocabulary → 7.1

A: "QA the release pipeline before we cut the production branch."
B: ["QA"] → ReleaseGateFlow71
Reason: "release pipeline", "production branch" → release/deployment vocabulary → sub-phase 7.1 (ReleaseGateFlow71).
CompiledPredicateIds: ["QA"]

---

## TU007-002 — Sub-phase 7.1 vs 7.3: acceptance vocabulary → 7.3

A: "QA sign-off on the feature before merge."
B: ["QA"] → QAGatingFlow73
Reason: "sign-off on the feature before merge" → feature acceptance vocabulary → sub-phase 7.3 (QAGatingFlow73).
CompiledPredicateIds: ["QA"]

---

## TU007-003 — Sub-phase 7.1: "staging", "deployment" vocabulary

A: "QA the staging deployment."
B: ["QA"] → ReleaseGateFlow71
Reason: "staging deployment" → deployment-pipeline vocabulary → 7.1.
CompiledPredicateIds: ["QA"]

---

## TU007-004 — Sub-phase 7.3: "validate", "gate" vocabulary

A: "QA gate this service before it goes to production."
B: ["QA"] → QAGatingFlow73
Reason: "QA gate" + feature validation context → 7.3 (QAGatingFlow73). Not release pipeline vocabulary.
CompiledPredicateIds: ["QA"]

---

## TU007-005 — Ambiguous: default to 7.3 when no release vocabulary

A: "QA this change."
B: ["QA"] → QAGatingFlow73
Reason: No release/deploy/pipeline vocabulary → default to 7.3 (QAGatingFlow73). Bare "QA this" is feature gate by default.
CompiledPredicateIds: ["QA"]
