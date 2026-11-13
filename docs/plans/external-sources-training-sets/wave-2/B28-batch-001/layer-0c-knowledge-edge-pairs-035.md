# Layer 0-C — Knowledge Edge Pairs (035) — ContainerScanGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E035-001 — ContainerScanGate trigger edge

A: "pass ContainerScanGate" → SECURITY | "scan container image for CVEs" → SECURITY
  Teaching: Container scan fires SECURITY (image vulnerability scanning).

---

## E035-002 — "push container to registry" is gap

A: "push the scanned container image to the artifact registry" → [] → knowledge_edge
  Teaching: "push", "registry" = artifact distribution vocabulary; gap.

---

## E035-003 — "build container image" fires IMPLEMENT

A: "build the container image for the AuthService" → IMPLEMENT (build verb + service noun via "AuthService")
  Teaching: "build" fires IMPLEMENT; "AuthService" contains "Service" → IMPLEMENT (deduplicated).

---

## E035-004 — ContainerScanGate vs StaticAnalysisGate: SECURITY vs TEST

A: "ContainerScanGate" → SECURITY | "StaticAnalysisGate" → TEST
  Teaching: Container scans fire SECURITY. Static analysis fires TEST. Different predicates.

---

## E035-005 — "Docker" and "image" alone are gap

A: "the Docker image" → [] → knowledge_edge
  Teaching: "Docker", "image" alone are container platform vocabulary; gap. Need "scan" or "secure" verb to fire SECURITY.
