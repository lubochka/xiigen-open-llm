# Layer 0-C — Knowledge-Edge Pairs (049)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-049-001 — Edge cases: "build" fires IMPLEMENT even in gap-heavy contexts

"Build" fires IMPLEMENT (rule 6) regardless of surrounding gap vocabulary.

| Sentence | PredicateSet |
|---|---|
| "Build the changelog automation pipeline for the supply chain release." | ["IMPLEMENT"] (build fires; pipeline in code artifact context consolidates) |
| "Build the attestation service for the gated release." | ["IMPLEMENT"] (build + service) |
| "Build the SBOM generation service for the regulatory submission." | ["IMPLEMENT"] (build + service; regulatory, SBOM = context) |

Gap nouns (supply chain, gated, regulatory, SBOM) do not reduce or suppress IMPLEMENT.

---

## B38-0C-049-002 — "build" + "service" in provenance/compliance: always IMPLEMENT

| Sentence | PredicateSet |
|---|---|
| "Build the provenance tracking service." | ["IMPLEMENT"] |
| "Build the signing service." | ["IMPLEMENT"] |
| "Build the compliance service." | ["IMPLEMENT"] |
| "Build the regulatory submission service." | ["IMPLEMENT"] |
| "Build the audit trail service." | ["IMPLEMENT"] |
| "Build the release gate service." | ["IMPLEMENT"] |
| "Build the release reporting service." | ["IMPLEMENT"] |

All seven produce ["IMPLEMENT"] — "build" fires (rule 6), "service" fires (rule 2), consolidate.

---

## B38-0C-049-003 — "build" without "service" in provenance/compliance

"Build" alone (without "service") still fires IMPLEMENT (rule 6).

| Sentence | PredicateSet |
|---|---|
| "Build the artifact signing infrastructure." | ["IMPLEMENT"] (build fires; "infrastructure" is not service — but build alone fires) |
| "Build the SBOM generation tool." | ["IMPLEMENT"] (build fires; "tool" is not service but build fires) |
| "Build the audit logging system." | ["IMPLEMENT"] (build fires; "system" is not service) |
