# Layer 0-C — Knowledge Edge Pairs (031) — DependencyCheckGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E031-001 — DependencyCheckGate trigger edge

A: "pass DependencyCheckGate" → SECURITY | "check dependencies for vulnerabilities" → SECURITY
  Teaching: Dependency vulnerability check fires SECURITY (security domain check).

---

## E031-002 — "update dependencies" is gap

A: "update npm dependencies to latest version" → [] → knowledge_edge
  Teaching: "update" (maintenance) + "npm" (tooling) → gap. No vulnerability check = no SECURITY.

---

## E031-003 — "validate dependency versions" fires TEST

A: "validate all dependency versions before release" → TEST
  Teaching: "validate" fires TEST in any context — including dependency validation.

---

## E031-004 — "promotion" is gap

A: "pass DependencyCheckGate before promotion" → SECURITY for gate; "promotion" → gap modifier
  Teaching: "promotion" (pipeline stage advancement) is gap vocabulary.

---

## E031-005 — Dependency check vs dependency update distinction

A: "check dependencies for vulnerabilities" → SECURITY | "update all dependencies" → [] → gap
  Teaching: Security vulnerability checks fire SECURITY. Plain update/maintenance tasks are gap.
