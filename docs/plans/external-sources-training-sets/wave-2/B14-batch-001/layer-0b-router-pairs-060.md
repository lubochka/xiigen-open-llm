# Layer 0-B — Router Pairs (060)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-060-001 — Complete routing invariant: SECURITY compound phrases always fire

Per rule 036-004: SECURITY compound phrases fire regardless of surrounding verb context:
  "zero-trust policy", "mTLS enforcement", "TLS termination", "secret rotation", "certificate lifecycle", "encryption at rest", "encryption in transit", "permission boundary enforcement"
  These compound phrases fire SECURITY even in presence of gap verbs ("enable zero-trust" → SECURITY fires)
  Exception: negated context ("without zero-trust" → does NOT fire SECURITY)

---

## B14-router-060-002 — Complete routing table: all 6 predicates → sub-phases (final master)

ARCHITECTURE → 3.5 if infra vocabulary density > app vocabulary; → 3.1 if app/domain vocabulary dominant
IMPLEMENT → 4.1 always (BackendServiceFlow41)
SECURITY → 6.3 always (SecurityAuditFlow63)
AUTHORIZATION → authorization sub-phase always
API_CONTRACT → 2.1 always (APIContractFlow21)
DOCUMENT → 6.2 always (DocumentationFlow62)

---

## B14-router-060-003 — Complete TransitionType decision matrix (final master)

IMPLEMENT present → feature_addition (always overrides)
IMPLEMENT absent + DOCUMENT present → documentation
IMPLEMENT absent + DOCUMENT absent + ARCHITECTURE (design verb) → feature_addition
IMPLEMENT absent + DOCUMENT absent + ARCHITECTURE absent → unknown (zero predicates → GapHandler)
SECURITY alone → feature_addition
AUTHORIZATION alone → feature_addition
API_CONTRACT alone → feature_addition

---

## B14-router-060-004 — Complete sequential ordering rule (final master)

When multiple predicates fire, sequential ordering:
  3.5 (ARCHITECTURE) → 2.1 (API_CONTRACT) → 6.3 (SECURITY) → AUTHORIZATION → 4.1 (IMPLEMENT) → 5.1 (TEST) → 6.2 (DOCUMENT) → 7.3 (QA)
  Rule: design precedes contract; security validates before implementation; documentation records all decisions last.
  When ARCHITECTURE absent, ordering continues from API_CONTRACT onward.
