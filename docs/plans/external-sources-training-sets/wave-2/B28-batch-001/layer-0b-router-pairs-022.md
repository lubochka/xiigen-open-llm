# Layer 0-B — Router Pairs (022)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-022-001 — QA+DOCUMENT → 7.1, DocumentationFlow62

A: ["QA", "DOCUMENT"] | Compliance release criteria + QA sign-off | 2 flows | documentation

## B28-router-022-002 — QA+IMPLEMENT+SECURITY → 7.1, 4.1, 6.x

A: ["QA", "IMPLEMENT", "SECURITY"] | Compliance gate service + secure audit trail + QA | 3 flows | feature_addition

## B28-router-022-003 — QA+DOCUMENT+SECURITY → 7.1, DocumentationFlow62, 6.x

A: ["QA", "DOCUMENT", "SECURITY"] | Security controls doc + compliance report + QA review | 3 flows | documentation

## B28-router-022-004 — QA+IMPLEMENT+DOCUMENT+SECURITY → 7.1, 4.1, DocumentationFlow62, 6.x

A: ["QA", "IMPLEMENT", "DOCUMENT", "SECURITY"] | Full compliance lifecycle | 4 flows | feature_addition

## B28-router-022-005 — gap: compliance audit scheduling

A: [] | "schedule SOC 2 audit and upload evidence" → GapFlow | knowledge_edge
