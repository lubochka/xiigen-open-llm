# T34 Transition Domain Map
# Phase 3 -> Phase 4 transition gate vocabulary
# Date: 2026-05-04

## Purpose

This document defines the domain vocabulary that node-02 must use when it
translates Phase 3 -> Phase 4 transition packets into formal statements.

The transition gate does not ask whether the Phase 4 implementation plan is
good in general. It asks whether the Phase 4 plan is allowed to start given
the Phase 3 context packet.

## Transition Domains

### Domain 1: Constraint Preservation

Question:

Does a constraint from the upstream technical-design context appear in the
downstream implementation source with equal or stronger force?

Formal statement shape:

```
domain: constraint_preservation
upstream_item: [constraint id and text]
downstream_evidence: [exact plan section or absent]
preservation_status: preserved | weakened | absent
enforcement_status: enforced | stated_not_enforced | absent
handoff_marker: pass | concern | block
```

Key vocabulary:

- preserved constraint
- weakened constraint
- stated-but-unenforced constraint
- absent constraint
- enforcement mechanism
- gate check
- test assertion

Covered finding:

The downstream plan preserves the upstream constraint and has a check that
would fail if the constraint were violated.

Boundary finding:

The downstream plan states the constraint but does not enforce it.

Conflict finding:

The downstream plan contradicts the upstream constraint.

### Domain 2: Decision Acknowledgment

Question:

Does the downstream implementation source acknowledge each upstream technical
decision it depends on or explicitly mark it out of scope?

Formal statement shape:

```
domain: decision_acknowledgment
upstream_decision: [decision id and text]
downstream_acknowledgment: incorporated | out_of_scope | absent
scope_reason: [why the downstream plan does or does not need it]
handoff_marker: pass | concern | block
```

Key vocabulary:

- incorporated decision
- explicit out-of-scope marker
- absent acknowledgment
- scope boundary
- implementation relevance

Covered finding:

The downstream plan incorporates the upstream decision or explicitly says why
that decision is outside the component scope.

Boundary finding:

The decision is genuinely outside the component scope, but the downstream
source does not say so.

Conflict finding:

The downstream plan makes a decision that contradicts the upstream decision.

### Domain 3: Knowledge Edge Continuity

Question:

Does a knowledge edge from the upstream context remain visible downstream
unless the downstream source provides new authority that resolves it?

Formal statement shape:

```
domain: knowledge_edge_continuity
upstream_edge: [knowledge edge id and text]
downstream_status: continued | resolved_by_source | absent
resolution_source: [source if resolved, otherwise none]
handoff_marker: pass | concern | block
```

Key vocabulary:

- continued knowledge edge
- resolved by source authority
- silent disappearance
- absent out-of-scope marker
- component-scope exclusion

Covered finding:

The downstream source explicitly continues the knowledge edge, marks it out of
scope, or resolves it with source authority.

Boundary finding:

The edge is out of scope but absent from the downstream source.

Conflict finding:

The downstream source behaves as if the edge is solved without source
authority.

### Domain 4: Rejection Preservation

Question:

Does a rejected upstream item remain rejected downstream unless the downstream
source is discussing a different artifact type?

Formal statement shape:

```
domain: rejection_preservation
upstream_rejection: [rejected item id and text]
downstream_pattern: [what appears downstream]
semantic_match: true | false
artifact_type_match: true | false
verdict: pass | concern | block
reason: [why the downstream item is or is not reinstating the rejected item]
```

Key vocabulary:

- rejected runtime topology
- reinstated rejected item
- semantic match
- artifact type mismatch
- build-time sequence
- runtime execution topology

Covered finding:

The downstream source either preserves the rejection or uses a superficially
similar form for a different artifact type.

Conflict finding:

The downstream source reinstates a rejected item with the same semantic role.

## Required Scenario Vocabulary

### Scenario T34-1

Required formal phrase:

`BaseService inheritance is stated as an upstream service-class constraint but
the downstream implementation plan has no Gate B check or test assertion that
would fail when a relevant service class omits the inheritance.`

### Scenario T34-2

Required formal phrase:

`The cross-phase context packet knowledge edge is outside the local
AutonomousLearning component scope, but the downstream source does not mark it
as out of scope. This is absent acknowledgment, not solved knowledge.`

### Scenario T34-3

Required formal phrase:

`The downstream fixed phase sequence is build-time implementation ordering, not
runtime execution topology. It does not reinstate the rejected static runtime
pipeline.`

