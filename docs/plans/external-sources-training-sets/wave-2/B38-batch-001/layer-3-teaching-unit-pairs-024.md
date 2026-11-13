# Layer 3 — Teaching Unit Pairs (024)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B38-TU-024-001 — Complete B38 teaching summary (013-024 extension)

This file summarizes the teaching curriculum added in the 013-024 extension batch.

Scenario: B38 — Release gates, go-live checklists, and project delivery
Sub-phase: 7.1 (Release and Deployment Automation)
Flow: ReleaseGateFlow71
Domain: software-development

New scenario coverage in this extension:
013 — Environment promotion gates
014 — Release train approval
015 — Hotfix release process
016 — Release notes documentation
017 — Version tagging
018 — Production handoff
019 — Release criteria documentation
020 — Post-release validation
021 — Incident response release
022 — Staged rollout gates
023 — Release sign-off collection
024 — Complete predicate reference (this file)

---

## B38-TU-024-002 — 12 new teaching points for the 013-024 extension

1. "Environment" and "promotion" are context words — no predicate fires from them alone.
2. "Release train" is a delivery-planning concept — no predicate fires; only QA from approval terms.
3. "Hotfix" is a delivery context noun — no predicate fires from "hotfix" alone.
4. "Document" fires DOCUMENT; "publish" and "distribute" fire nothing.
5. "Build" fires IMPLEMENT even in "build version tag" — "version" and "tag" are context.
6. "Handoff" ≠ "sign-off": handoff has no predicate; sign-off fires QA always.
7. "Criteria" fires DOCUMENT for every qualifier without exception (rule 3 is absolute).
8. ARCHITECTURE does not produce feature_addition — only IMPLEMENT does.
9. "Rollout" is gap everywhere — it never fires a predicate in any context.
10. Post-release "validate"/"verify" fire QA; post-release "monitor"/"check" do not.
11. "Incident", "remediation", "rollback" are context words — no predicate fires.
12. IMPLEMENT consolidates — "service" + "build" in same sentence = one IMPLEMENT predicate.

---

## B38-TU-024-003 — Quick scenario lookup: what predicate set does each scenario produce?

| Scenario | Example input (abbreviated) | PredicateSet |
|---|---|---|
| Environment promotion (no service) | "verify promotion criteria and approve" | ["QA","DOCUMENT"] |
| Environment promotion (with service) | "approve auth service promotion" | ["QA","IMPLEMENT","DOCUMENT"] |
| Release train approval | "approve Q2 release train" | ["QA"] |
| Hotfix approval | "approve the hotfix" | ["QA"] |
| Hotfix with service | "review auth service hotfix" | ["QA","IMPLEMENT"] |
| Release notes | "document release notes and sign off" | ["QA","DOCUMENT"] |
| Version tagging (with build) | "verify build version tag" | ["QA","IMPLEMENT"] |
| Production handoff | "collect sign-off for handoff" | ["QA"] |
| Release criteria doc | "document release criteria" | ["QA","DOCUMENT"] |
| Post-release validation | "validate production environment" | ["QA"] |
| Incident response | "review incident and approve remediation" | ["QA"] |
| Staged rollout (architecture) | "review rollout architecture design" | ["QA","ARCHITECTURE"] |
| Sign-off collection | "collect sign-offs from team leads" | ["QA"] |
| Go-live checklist | "sign off on the go-live checklist" | ["QA"] with 3 gap warnings |
| Launch only | "launch the release train on go-live day" | [] → knowledge_edge |
| Monitor only | "monitor the rollout post go-live" | [] → knowledge_edge |
