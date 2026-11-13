# Layer 0-B — Router Pairs (017)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-017-001 — QA+ARCHITECTURE → 7.1, 3.1

A: ["QA", "ARCHITECTURE"] | Infrastructure release design + QA | [ReleaseGateFlow71, ArchitectureFlow31] | documentation

## B28-router-017-002 — QA+IMPLEMENT → 7.1, 4.1

A: ["QA", "IMPLEMENT"] | Infrastructure provisioning service + QA gate | [ReleaseGateFlow71, ImplementationFlow41] | feature_addition

## B28-router-017-003 — QA+ARCHITECTURE+IMPLEMENT → 7.1, 3.1, 4.1

A: ["QA", "ARCHITECTURE", "IMPLEMENT"] | Design + build + QA for infra release | 3 flows | feature_addition

## B28-router-017-004 — QA+IMPLEMENT+DOCUMENT → 7.1, 4.1, DocumentationFlow62

A: ["QA", "IMPLEMENT", "DOCUMENT"] | IaC pipeline + criteria docs + QA | 3 flows | feature_addition

## B28-router-017-005 — gap: infrastructure provisioning

A: [] | "provision K8s cluster" → GapFlow | knowledge_edge
