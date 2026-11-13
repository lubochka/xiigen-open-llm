# Layer 0-C — Knowledge-Edge Pairs (041)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-041-001 — "service" fires IMPLEMENT: provenance/compliance scenarios

"Service" fires IMPLEMENT (rule 2) in all provenance and compliance scenarios without exception.

| Sentence | IMPLEMENT trigger |
|---|---|
| "Build the provenance tracking service." | "service" (+ "build") |
| "Review the attestation service." | "service" |
| "Build the SBOM generation service." | "service" (+ "build") |
| "Audit the signing service." | "service" |
| "Build the compliance service." | "service" (+ "build") |

Gate B assertion for all: IMPLEMENT in PredicateSet.

---

## B38-0C-041-002 — "system" does NOT fire IMPLEMENT

"System" is not in the IMPLEMENT lexicon. Only "service" fires IMPLEMENT from rule 2.

| Sentence | PredicateSet |
|---|---|
| "Audit the security of the artifact provenance tracking system." | ["QA","SECURITY"] — system does not fire IMPLEMENT |
| "Review the gated release system." | ["QA"] — system does not fire IMPLEMENT |
| "Inspect the compliance system." | ["QA"] — system does not fire IMPLEMENT |

---

## B38-0C-041-003 — "pipeline" context: does it fire IMPLEMENT?

"Pipeline" fires IMPLEMENT only when it refers to a code artifact (build pipeline, CI/CD pipeline).
"Pipeline" as a process noun (release pipeline, documentation pipeline) is context — no predicate.

| Sentence | PredicateSet |
|---|---|
| "Build the CI/CD pipeline." | ["IMPLEMENT"] ("build" fires; "pipeline" in code artifact context consolidates) |
| "Review the release pipeline." | ["QA"] ("review" fires; "release pipeline" is process context) |
| "Configure the release pipeline." | [] ("configure" gap; "pipeline" process context) |
| "Build the changelog automation pipeline for the release service." | ["IMPLEMENT"] ("build" fires; "service" fires — consolidate) |
