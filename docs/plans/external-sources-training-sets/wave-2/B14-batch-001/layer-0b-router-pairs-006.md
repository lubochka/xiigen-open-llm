# Layer 0-B — Router Pairs (006)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: router

---

## B14-router-006-001 — ARCHITECTURE+SECURITY: security architecture sub-phase

A: ["ARCHITECTURE", "SECURITY"] — zero-trust design context
B: ARCHITECTURE → 3.5 (infra sub-phase) | SECURITY → 6.3 (audit sub-phase)
  Teaching: SECURITY always routes to 6.3 when audit/review/scan vocabulary is present. ARCHITECTURE routes to 3.5 when infra vocabulary co-occurs. The two sub-phases are independent.

---

## B14-router-006-002 — ARCHITECTURE+SECURITY: implement security context

A: ["ARCHITECTURE", "SECURITY"] — implement/enforce security controls context
B: ARCHITECTURE → 3.5 | SECURITY → 5.2 (implement-security sub-phase)
  Teaching: When SECURITY co-occurs with "implement"/"enforce" (not "audit"/"scan"), SECURITY routes to 5.2. ARCHITECTURE still routes to 3.5. Sub-phase of SECURITY depends on verb context.

---

## B14-router-006-003 — ARCHITECTURE+API_CONTRACT: API gateway design

A: ["ARCHITECTURE", "API_CONTRACT"]
B: ARCHITECTURE → 3.5 | API_CONTRACT → 3.3
  Teaching: API gateway architecture → 3.5 (structural design). API contract definition (OpenAPI, schema) → 3.3. Two distinct sub-phases for related concerns.

---

## B14-router-006-004 — ARCHITECTURE: disambiguation 3.5 vs 3.1 by vocabulary

A: ["ARCHITECTURE"] — infra vocabulary (infrastructure, cluster, container, networking, VPC, service mesh)
B: → 3.5
  vs ["ARCHITECTURE"] — application vocabulary (domain model, service layer, module, pattern, dependency)
B: → 3.1
  Teaching: The router uses vocabulary co-occurrence to disambiguate 3.5 from 3.1. Infra vocabulary → 3.5. Application vocabulary → 3.1.

---

## B14-router-006-005 — v6-03 corruption: multi-branch collapse

A: ["ARCHITECTURE", "IMPLEMENT", "TEST"]
B: CORRUPT: only one flow instantiated (model collapses to single flow) → v6-03
  Correct: Three flows instantiated: 3.5 + 4.1 + 5.1
  Teaching: v6-03 collapses multi-predicate routing to a single sub-phase. Each predicate always routes independently.

