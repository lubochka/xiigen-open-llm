# Layer 0-B — Router Pairs (004)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: router

---

## B14-router-004-001 — ARCHITECTURE+IMPLEMENT+DOCUMENT → 3.5, 4.1, 6.2

A: ["ARCHITECTURE", "IMPLEMENT", "DOCUMENT"]
B: SubPhase 3.5 (InfrastructureArchitectureFlow35) + SubPhase 4.1 (ImplementationFlow41) + SubPhase 6.2 (DocumentationFlow62)
  Three concurrent flows. No predicate suppresses another.

---

## B14-router-004-002 — ARCHITECTURE+IMPLEMENT+TEST → 3.5, 4.1, 5.1

A: ["ARCHITECTURE", "IMPLEMENT", "TEST"]
B: SubPhase 3.5 + SubPhase 4.1 + SubPhase 5.1
  Teaching: IaC (Infrastructure as Code) workflows commonly fire all three: architect the design (3.5), implement Terraform modules (4.1), write/validate tests (5.1).

---

## B14-router-004-003 — ARCHITECTURE+SECURITY+IMPLEMENT → 3.5, 6.3, 4.1

A: ["ARCHITECTURE", "SECURITY", "IMPLEMENT"]
B: SubPhase 3.5 + SubPhase 6.3 + SubPhase 4.1
  Teaching: Network security architecture design (ARCHITECTURE), existing security audit (SECURITY), implementing firewall rules (IMPLEMENT) → three independent flows.

---

## B14-router-004-004 — ARCHITECTURE+DOCUMENT+QA → 3.5, 6.2, 7.3

A: ["ARCHITECTURE", "DOCUMENT", "QA"]
B: SubPhase 3.5 + SubPhase 6.2 + SubPhase 7.3
  Teaching: Architecture finalization with documentation and QA sign-off: three independent flows, three sub-phases.

---

## B14-router-004-005 — Four-predicate → 4 flows

A: ["ARCHITECTURE", "IMPLEMENT", "TEST", "DOCUMENT"]
B: SubPhase 3.5 + SubPhase 4.1 + SubPhase 5.1 + SubPhase 6.2
  Teaching: Four-predicate routing creates four concurrent flows. Each predicate routes independently to its designated sub-phase.

