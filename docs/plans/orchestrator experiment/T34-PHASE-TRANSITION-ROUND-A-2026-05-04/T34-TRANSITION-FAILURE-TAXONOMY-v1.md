# T34 Transition Failure Taxonomy
# Phase 3 -> Phase 4 transition gate verdict rules
# Date: 2026-05-04

## Purpose

This taxonomy tells node-08 arbiters how to classify transition findings.
It is the authority for concern versus block versus pass in this experiment.

## Verdict Meanings

| Verdict | Meaning |
|---|---|
| PASS | The downstream source preserves, acknowledges, or correctly scopes the upstream item. |
| CONCERN | The downstream source can start, but a named gap must be repaired before accepting the downstream phase output. |
| BLOCK | The downstream source is missing or contradicting upstream authority in a way that makes the phase unsafe to start. |

## Failure Types

### F1: Dropped Constraint

Definition:

An upstream constraint is absent from the downstream source even though the
downstream work depends on it.

Observable signal:

The upstream context packet names a constraint. The downstream source neither
states the constraint nor enforces it.

Correct verdict:

BLOCK.

Why:

The downstream phase would start without a required upstream constraint.

### F2: Stated-But-Unenforced Constraint

Definition:

The downstream source states a constraint, but no gate, test, or pre-condition
would fail if the constraint were violated.

Observable signal:

The source says the rule exists, but the Gate B commands and tests do not
verify it.

Correct verdict:

CONCERN.

Why:

The phase may start because the constraint is visible, but the implementation
plan must add enforcement before the phase output can be accepted.

Scenario:

T34-1. BaseService inheritance is a service-class constraint. The downstream
plan does not contain a Gate B check or test assertion that catches omission.

### F3: Absent Knowledge Edge Acknowledgment

Definition:

An upstream knowledge edge is genuinely outside the downstream component scope,
but the downstream source does not say it is out of scope.

Observable signal:

The upstream edge is not mentioned downstream. The component can proceed, but
only because the edge is outside its local responsibility, not because the edge
was resolved.

Correct verdict:

CONCERN.

Why:

The phase can start, but the source packet is incomplete. It must explicitly
carry an out-of-scope marker so the edge does not silently disappear across
future phases.

Scenario:

T34-2. The cross-phase context packet mechanism is outside the local
AutonomousLearning implementation scope, but the plan does not mark it out of
scope.

### F4: Silent Knowledge Edge Disappearance

Definition:

An upstream knowledge edge disappears downstream and the downstream source
behaves as if the missing competence is resolved.

Observable signal:

No downstream source authority resolves the edge, but downstream work depends
on the edge as if solved.

Correct verdict:

BLOCK.

Why:

The downstream source is inventing or assuming missing competence.

### F5: Reinstated Rejected Item

Definition:

The downstream source reintroduces an upstream rejected item with the same
semantic role and artifact type.

Observable signal:

The upstream context rejects item X. The downstream source uses X again for the
same purpose.

Correct verdict:

BLOCK.

Why:

The downstream phase is contradicting a known rejection.

### F6: Superficial Reinstatement With Artifact-Type Difference

Definition:

The downstream source uses a form that resembles a rejected upstream item, but
the artifact type and purpose are different.

Observable signal:

The upstream rejected item concerns runtime execution topology. The downstream
item is a build-time implementation sequence.

Correct verdict:

PASS.

Why:

The downstream source did not reinstate the rejected runtime decision. It used
a fixed sequence for a different artifact class.

Scenario:

T34-3. The AutonomousLearning plan's phases are build-time implementation
order, not a static runtime pipeline.

## Severity Boundaries

Do not call every finding blocking.

A transition gate is valuable only if it can distinguish:

- a phase that must not start,
- a phase that may start with named gaps,
- and a phase whose apparent conflict is actually valid.

The hardest boundary in this experiment is T34-3. A mechanical checker sees a
fixed sequence and flags it as the rejected static pipeline. A correct arbiter
checks artifact type and passes it.

## Scenario Verdict Summary

| Scenario | Finding type | Expected verdict |
|---|---|---|
| T34-1 | Stated-but-unenforced constraint | CONCERN |
| T34-2 | Absent knowledge edge acknowledgment | CONCERN |
| T34-3 | Superficial reinstatement with artifact-type difference | PASS |

Expected BLOCK count: 0.

