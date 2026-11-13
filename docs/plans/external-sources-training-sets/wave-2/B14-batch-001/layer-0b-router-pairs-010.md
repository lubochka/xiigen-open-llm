# Layer 0-B — Router Pairs (010)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: router

---

## B14-router-010-001 — ARCHITECTURE: sub-phase 3.5 vs 3.1 — borderline

A: "Design the microservices architecture for the cloud platform deployment."
  "design" → ARCHITECTURE | "microservices architecture" → borderline (microservices = architectural style)
  "cloud platform deployment" → infra vocabulary
  Resolution: infra vocabulary ("cloud platform deployment") tips to → 3.5
  Teaching: Borderline cases resolve by dominant vocabulary co-occurrence. Infra vocabulary present → 3.5.

---

## B14-router-010-002 — ARCHITECTURE: "ADR" (Architecture Decision Record) → ARCHITECTURE

A: ["ARCHITECTURE"] | trigger: "ADR" abbreviation
B: → 3.5 (if infra context) or 3.1 (if app context) | TransitionType: documentation
  Teaching: "ADR" fires ARCHITECTURE via abbreviation expansion (Architecture Decision Record). Sub-phase resolved by surrounding vocabulary.

---

## B14-router-010-003 — ARCHITECTURE+IMPLEMENT+TEST: IaC (Infrastructure as Code) workflow

A: ["ARCHITECTURE", "IMPLEMENT", "TEST"]
B: → 3.5 + 4.1 + 5.1 | TransitionType: feature_addition
  Teaching: Canonical IaC workflow: design infrastructure (ARCHITECTURE → 3.5), implement Terraform/Pulumi modules (IMPLEMENT → 4.1), validate with tests (TEST → 5.1).

---

## B14-router-010-004 — ARCHITECTURE+SECURITY+DOCUMENT: security architecture doc

A: ["ARCHITECTURE", "SECURITY", "DOCUMENT"]
B: → 3.5 + 6.3 + 6.2 | TransitionType: documentation (no IMPLEMENT)
  Teaching: Documenting security architecture decisions fires all three predicates with documentation TransitionType.

---

## B14-router-010-005 — ARCHITECTURE: "refactor the architecture" → refactoring TransitionType

A: ["ARCHITECTURE"] | verb: "refactor"
B: → 3.5 | TransitionType: refactoring
  Teaching: "refactor" in architecture context overrides documentation TransitionType. Architectural refactoring (extract services, reorganize layers, decouple components) → refactoring.

