# MANIFEST — B28-batch-001
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06
# Total files: 201

---

## File Count by Layer

| Layer | Files |
|-------|-------|
| Layer 0-A (compiler pairs) | 24 (001–024) |
| Layer 0-B (router pairs) | 24 (001–024) |
| Layer 0-C (knowledge edge pairs) | 24 (001–024) |
| Layer 0-D (flow factory pairs) | 24 (001–024) |
| Layer 1 (transition orchestrator pairs) | 24 (001–024) |
| Layer 2 (nine-node 7.1 pairs) | 24 (001–024) |
| Layer 3 (teaching unit pairs) | 24 (001–024) |
| Held-out and corruption pairs | 24 (001–024) |
| Cross-layer synthesis | 8 (001–008) |
| MANIFEST | 1 |
| **Total** | **201** |

---

## ReleaseGateFlow71 Nine-Node Flows — New Files (Layer 2, 013–024)

| File | Release Gate Target | Notable Gates | Verdict |
|------|---------------------|---------------|---------|
| 013 | CheckoutService v3 blue-green gate | cutover, health check, routing, rollback | PASS |
| 014 | ProductService canary controller | routing, percentage, health, rollback trigger | PASS |
| 015 | OrderService rollback automation | trigger, restore, idempotency, notification | PASS_WITH_WARNINGS (restore margin) |
| 016 | RecommendationService feature flag criteria | enable/disable conditions, fallback, owner sign-off | PASS |
| 017 | Infrastructure release gate ADR | pre/post-provision gates, rollback path, ADR rationale | PASS |
| 018 | UserService schema v4 migration | schema correctness, data integrity, rollback, performance | PASS |
| 019 | Checkout pipeline multi-service coordinator | ordering, abort-on-failure, dependency map, cross-service | PASS |
| 020 | SearchService A/B experiment gate | assignment, split, metrics, isolation | PASS_WITH_WARNINGS (metric leakage) |
| 021 | PaymentService DR failover | RTO, RPO, trigger correctness, rollback from DR | PASS |
| 022 | AdminService compliance release | RBAC, encryption, audit log, SOC 2 completeness | PASS |
| 023 | NotificationService v2 release | deduplication, delivery, retry, idempotency | FAIL (retry + idempotency bugs) |
| 024 | Complete ReleaseGateFlow71 rule summary | node naming, TransitionType rules, verdict types | reference |

---

## QAGatingFlow73 Nine-Node Flows (Layer 2)

| File | QA Gate Target | Notable Gates | Verdict |
|------|---------------|---------------|---------|
| 001 | CheckoutService smoke (Black Friday) | health, payment, UX, load | PASS |
| 002 | OrderService v2.0 | order lifecycle, concurrency | PASS_WITH_WARNINGS (minor UX) |
| 003 | PaymentService CVE patch | SQL injection, input validation, CVE regression | PASS |
| 004 | UserService v3.1 login | session, MFA, legacy paths | FAIL (session not invalidated) |
| 005 | CheckoutService smoke (Black Friday) | health, DB, payment, load | PASS |
| 006 | ProductCatalogService API v4 | CRUD, search, performance | PASS |
| 007 | NotificationService dedup hotfix | deduplication, delivery, idempotency | PASS |
| 008 | AdminService RBAC update | role assignment, audit log, PII visibility | PASS_WITH_WARNINGS |
| 009 | SearchService v2 Elasticsearch | relevance, performance, fallback | FAIL (relevance + perf regression) |
| 010 | RefundService v1.5 | refund logic, partial refund, idempotency | PASS |
| 011 | InventoryService v2 concurrency | reservation, release, concurrency (pessimistic lock) | PASS_WITH_WARNINGS (p99 latency) |
| 012 | ReportingService v1 aggregation | aggregation, time-zone, export formats | FAIL (midnight aggregation + DST bug) |

---

## Key Teaching Points

1. **QA abbreviation gate** — "QA" fires QA predicate; "quality assurance" spelled out does NOT
2. **QA compound nouns** — QA sign-off, QA gate, QA checklist, QA report, QA block, QA review, QA harness, QA criteria, QA metrics, QA process, QA environment, QA owner, QA approval, QA pass, QA automation all fire QA
3. **QA role titles** — QA engineer, QA lead, QA analyst, QA tester, QA manager fire QA via abbreviation
4. **QA as verb** — "QA this", "QA the build" → imperative verb use fires QA predicate
5. **QA in any syntactic position** — subject, object, modifier, parenthetical, negative, interrogative → all fire QA
6. **Sub-phase 7.1 vs 7.3** — release/deploy/staging pipeline vocabulary → ReleaseGateFlow71; feature acceptance/gate vocabulary → QAGatingFlow73
7. **Gap vocabulary** — deploy, provision, rollout, monitor, SLO, SLA, CI/CD, feature flag, hotfix, sign-off alone, approval alone, release alone → gap
8. **QA always last** — QA gate executes after all other flows (IMPLEMENT, TEST, DOCUMENT, etc.)
9. **QA does not drive TransitionType** — IMPLEMENT → feature_addition; TEST+DOCUMENT → documentation; QA is gate-only
10. **TEST vs QA** — TEST fires on test-writing verbs (write, create, add, implement tests); QA fires on QA abbreviation. They are distinct predicates.
11. **QA predicate deduplication** — multiple QA occurrences → one QA predicate (set, not multiset)
12. **v6 corruption types** — v6-01: QA_AUTOMATION/QA_GATE/TESTING/RELEASE/DEVOPS invented; v6-02: gap suppressed to QA/TEST; v6-03: multi-predicate collapsed to single flow
13. **"service" rule** — "service" fires IMPLEMENT always, including in release review sentences (e.g., "deployment service" → IMPLEMENT)
14. **"criteria" rule** — "criteria" fires DOCUMENT always, including "release criteria", "acceptance criteria", "compliance criteria"
15. **"deployment" alone is gap** — "deployment" alone is gap; "build" fires IMPLEMENT; "build provenance" fires IMPLEMENT (build artifact vocabulary)
16. **Release gap vocabulary** — go-live, cutover, rollout, runbook, canary (alone), blue-green (alone), hotfix (alone), failover execution → all gap
17. **Pure gap → knowledge_edge** — pure gap requests produce knowledge_edge output (NOT GapHandler, NOT SCHEDULE_CAPABILITY_LEARNING)
18. **ReleaseGateFlow71 node naming** — DomainSplitter→DomainTranslator→SchemePlanner→DomainProcessor→Applier→ResultTranslator→ArbiterSelector→Arbiter→Formatter
19. **FAIL and PASS_WITH_WARNINGS are valid** — models must not normalize FAIL to PASS for any release context including emergency/hotfix
20. **"validate" fires TEST in any context** — "validate" fires TEST regardless of what is being validated (DB rows, metrics, assignments)

---

## Cross-Layer Synthesis Traces

| File | Scenario | Predicate Set |
|------|----------|---------------|
| 001 | Implement payment retry + QA sign-off | IMPLEMENT+QA |
| 002 | Gap: "Deploy to staging for testing" | [] → GapHandler |
| 003 | Write tests + document + QA sign-off | TEST+DOCUMENT+QA |
| 004 | Six-predicate: design + implement + secure + test + document + QA | ARCHITECTURE+IMPLEMENT+SECURITY+TEST+DOCUMENT+QA |
| 005 | QA alone for release sign-off (no TransitionType) | QA only |
| 006 | "plan the deployment rollout" → knowledge_edge gap | [] → knowledge_edge |
| 007 | QA+IMPLEMENT+DOCUMENT for canary release | IMPLEMENT+DOCUMENT+QA |
| 008 | Full IMPLEMENT+SECURITY+QA+DOCUMENT+ARCHITECTURE five-predicate release trace | ARCHITECTURE+IMPLEMENT+SECURITY+DOCUMENT+QA |
