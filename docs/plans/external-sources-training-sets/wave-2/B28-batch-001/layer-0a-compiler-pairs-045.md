# Layer 0-A — Compiler Pairs (045) — ArtifactPublishing
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-045-001 — "publish artifact" is gap

A: "Publish the release artifact to the registry."
  "publish" → gap | "artifact" → gap | "registry" → gap
  Teaching: Artifact publishing vocabulary is gap. No predicates fire.
  CompiledPredicateIds: []

---

## B28-compiler-045-002 — IMPLEMENT+QA for artifact publisher service

A: "Build the artifact publishing service and QA the published artifact integrity."
  "build" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "QA" → QA
  Teaching: Build+service → IMPLEMENT. QA fires on abbreviation. "artifact integrity" is context.
  CompiledPredicateIds: ["IMPLEMENT", "QA"]

---

## B28-compiler-045-003 — "push to registry" and "upload artifact" are gap

A: "Push the Docker image to the container registry and upload the npm artifact."
  "push to registry" → gap | "upload artifact" → gap | "Docker image" → gap | "npm artifact" → gap
  Teaching: All artifact upload/push vocabulary is gap.
  CompiledPredicateIds: []

---

## B28-compiler-045-004 — SECURITY+QA for artifact signing

A: "Secure the artifact signing pipeline and QA the signature verification."
  "secure" → SECURITY | "QA" → QA
  Teaching: "secure" fires SECURITY in artifact signing context. QA last.
  CompiledPredicateIds: ["SECURITY", "QA"]

---

## B28-compiler-045-005 — IMPLEMENT+DOCUMENT+QA for artifact publishing with criteria

A: "Build the artifact metadata service, document the publishing criteria, and QA the publication workflow."
  "build" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "QA" → QA
  Teaching: Three predicates. "metadata service" fires IMPLEMENT via "service" rule.
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT", "QA"]
