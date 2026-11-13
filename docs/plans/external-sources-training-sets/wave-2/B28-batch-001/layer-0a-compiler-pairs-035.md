# Layer 0-A — Compiler Pairs (035) — ContainerScanGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-035-001 — "ContainerScanGate" vocabulary

A: "All container images must pass the ContainerScanGate before deployment."
  "ContainerScanGate" → SECURITY (container scan = container security vulnerability check) | "deployment" → gap
  Teaching: ContainerScanGate fires SECURITY (container image vulnerability scanning). "deployment" alone is gap.
  CompiledPredicateIds: ["SECURITY"]

---

## B28-compiler-035-002 — "scan container image" + "QA review"

A: "Scan the Docker container image for CVEs and QA review the security report."
  "scan container" → SECURITY (scan = security check) | "QA review" → QA
  Teaching: "container scan" fires SECURITY; "QA review" fires QA.
  CompiledPredicateIds: ["SECURITY", "QA"]

---

## B28-compiler-035-003 — "build container image" + "secure scan"

A: "Build the container image for the AuthService and secure-scan it before pushing."
  "build" → IMPLEMENT | "container image" alone → gap | "AuthService" service → IMPLEMENT (dedup) | "secure" → SECURITY
  Teaching: "build" fires IMPLEMENT; "service" in "AuthService" fires IMPLEMENT (deduplicated); "secure" fires SECURITY.
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]

---

## B28-compiler-035-004 — "document container scan criteria"

A: "Document the container scan criteria and QA approve the baseline policy."
  "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "QA approve" → QA
  Teaching: DOCUMENT fires twice (deduplicated). "QA approve" fires QA.
  CompiledPredicateIds: ["DOCUMENT", "QA"]

---

## B28-compiler-035-005 — Pure gap: "push container to registry"

A: "Push the scanned container image to the artifact registry for deployment."
  "push" → gap | "deployment" → gap | "registry" → gap
  Teaching: "push", "registry", "deployment" are CI/CD artifact management vocabulary; pure gap.
  CompiledPredicateIds: [] → gap (knowledge_edge)
