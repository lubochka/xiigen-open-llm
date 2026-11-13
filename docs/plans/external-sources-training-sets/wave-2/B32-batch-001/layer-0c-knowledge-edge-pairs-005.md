# Layer 0-C — Knowledge-Edge Pairs (005)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-005-001 — B32 DomainId: "software-development" vs B36/B37 "project-management"

**Knowledge edge:** B32 vibe coding always uses DomainId "software-development". B36 and B37 use "project-management". This is a critical distinction that affects where SCHEDULE_CAPABILITY_LEARNING is routed.

**DomainId assignment table:**

| Scenario | Description | DomainId |
|---|---|---|
| B32 | Vibe coding — exploratory, prototype | software-development |
| B36 | Project planning vocabulary | project-management |
| B37 | Sprint/agile vocabulary | project-management |

---

## B32-0C-005-002 — Why B32 uses "software-development"

**Reasoning:**

Vibe coding is a software development activity. The requests contain software development vocabulary (build, implement, screen, service, module, API). The "vibe" framing is a style — it does not change the domain. A developer "vibe coding a service" is doing software development with an unrecognized style vocabulary.

Contrast with B36 (project planning): "sprint planning", "milestone", "roadmap" are project management vocabulary even when software is the subject. The activity is management, not implementation.

B32 gap vocabulary is unrecognized STYLE within a software-development domain.
B36/B37 gap vocabulary is unrecognized METHODOLOGY within a project-management domain.

---

## B32-0C-005-003 — DomainId in SCHEDULE_CAPABILITY_LEARNING

**When SCHEDULE_CAPABILITY_LEARNING fires (predicates = []):**

```
B32 correct:
{
  action: "SCHEDULE_CAPABILITY_LEARNING",
  domainId: "software-development",
  gapIds: ["gap-vibe-coding-spike-001"],
  requestText: "Run a spike to explore the recommendation engine."
}

B36 correct:
{
  action: "SCHEDULE_CAPABILITY_LEARNING",
  domainId: "project-management",
  gapIds: ["gap-b36-roadmap-001"],
  requestText: "Plan a roadmap for the feature delivery."
}
```

The DomainId determines which learning pipeline receives the signal and which capability gap training dataset is updated.

---

## B32-0C-005-004 — v6-03 corruption: "project-management" for B32

**WRONG:**
```
{
  action: "SCHEDULE_CAPABILITY_LEARNING",
  domainId: "project-management",  ← WRONG for B32
  gapIds: ["gap-vibe-coding-spike-001"]
}
```

**Result of this corruption:**
- The SCHEDULE_CAPABILITY_LEARNING signal reaches the project-management learning pipeline
- The software-development learning pipeline receives no signal
- The gap vocabulary for vibe coding is never learned in the correct domain
- Future B32 requests continue to produce knowledge_edge incorrectly

---
