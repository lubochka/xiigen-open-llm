# Layer 3 — Teaching Unit Pairs
# Batch: B28-batch-001
# Scenario: B28 — QA automation DevOps, sub-phase 7.1
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: teaching_unit

---

## Pair B28B1-WDA-teach-001 — "deploy" is gap vocabulary; no predicate fires

A: "Deploy the service to production." Does IMPLEMENT fire?
B: No. "deploy" is not in any predicate lexicon. IMPLEMENT fires on: build, create,
write, implement, code, add, develop, generate, scaffold, extend. "Deploy" is a
DevOps operation term — it moves a built artifact to an environment but is not in
the implementation predicate vocabulary. Result: zero predicates → knowledge_edge.
Gap ID: gap-devops-deployment-001.

---

## Pair B28B1-WDA-teach-002 — "QA" literal fires QA; "quality assurance" spelled out does not

A: "Quality assurance the release build." vs "QA the release build." Which fires QA?
B: "QA the release build" → QA predicate fires on the literal abbreviation "QA".
"Quality assurance the release build" — "quality assurance" spelled out does NOT fire QA.
The QA predicate is bound to the abbreviation "QA" as a literal token, not to the
expanded phrase. This is an abbreviation-sensitive boundary.

---

## Pair B28B1-WDA-teach-003 — CI/CD pipeline vocabulary is entirely gap

A: "Set up the GitHub Actions workflow for CI/CD." What predicates fire?
B: Zero predicates. "set up" is gap. "GitHub Actions" is gap (third-party tool name).
"CI/CD" is gap. "workflow" in a CI/CD context is gap (not ARCHITECTURE without design verb).
Result: zero predicates → knowledge_edge. Gap ID: gap-devops-cicd-001.
To fire predicates: "Implement the release pipeline stages" → IMPLEMENT fires.
"Design the CI/CD architecture" → ARCHITECTURE fires.

---

## Pair B28B1-WDA-teach-004 — "smoke test" routes TEST to 5.3 not 5.1

A: "Build a smoke test suite for the release." Does TEST route to 5.1 or 5.3?
B: TEST routes to 5.3 (End-to-End and UI Testing). Smoke tests are system-level broad
checks — they validate the deployed system end-to-end, not individual units in isolation.
Sub-phase disambiguation: "unit test" → 5.1; "smoke test", "E2E test", "integration
test" (system level) → 5.3. IMPLEMENT fires on "build" → 4.1 independently.

---

## Pair B28B1-WDA-teach-005 — observability vocabulary (monitor, alert, metrics) is gap

A: "Monitor the production service and alert on latency spikes." What predicates fire?
B: Zero predicates. "monitor", "alert", "latency spikes" are all gap — DevOps
observability vocabulary. No predicate covers monitoring, alerting, or metrics
terminology. Result: zero predicates → knowledge_edge. Gap ID: gap-devops-monitoring-001.
DomainId: "software-development".
