# Source Authority Coverage Audit v1
# Date: 2026-05-05

---

## Purpose

Record the available source authority status after Wave 2 Center Phase Batch
001.

This audit explains why the corpus cannot be safely declared complete by
mechanically expanding the current batch.

---

## Source Pools Currently Used

| Source pool | Current use | Authority status | Limitation |
|---|---|---|---|
| local orchestration experiment documents | WDA, Wave 2 Batch 001, topology, logical-unit, inspector, corpus plans | Tier 1 local project authority | strong for orchestrator architecture and center-phase logic, thin for UI/UX, functional spec, testing, QA, QA automation details |
| local C# source files | Wave 2 Batch 001 backend and connector rows | Tier 1 repository source authority | strong for implementation/runtime rows, not enough for design, QA, or functional-spec document packets |
| CB4 external-source security pairs | security-verification-3.4 and Wave 2 3.4 rows | license checked by user | strong for security/auth model, not a substitute for all 31 development sub-phases |

---

## Current Quota Evidence

```text
Batch 001 source document packets: 20 training + 5 held-out
Batch 001 selected sub-phases: 5
Full v4 target: 3,100 training + 620 held-out + 60 transition held-out
```

Current source packet quota completion:

```text
training packets: 20 / 3,100
sub-phase held-out packets: 5 / 620
transition held-out packets: 0 / 60
```

---

## Why Mechanical Expansion Is Not Safe

The v4 corpus requires each row to carry:

```text
source authority
one primary phase
one primary sub-phase
three verifiable claims
known failure mode
expected top predicates
expected knowledge edges
expected conflicts
train or held-out status
promotion status
```

The current Batch 001 sources are enough to prove the full-path package shape.
They are not enough to fill all 31 sub-phases without either:

```text
repeating the same source claim too many times;
creating synthetic rows beyond the allowed reviewed-synthetic boundary;
pretending architecture documents cover domains they do not actually govern;
or accepting pair outputs without runtime conformance for learned units.
```

Any of those would violate the corpus plan.

---

## Required Source Intake Before Full Completion

To continue toward real completion, the next source intake must add reviewed
material for the weakest areas:

```text
Phase 1 UI/UX definition: design tokens, layout, interaction, accessibility, motion
Phase 2 functional specification: user stories, business rules, integration specs, data boundaries
Phase 5 testing: unit, integration, e2e, performance
Phase 6 QA: functional review, regression, security/compliance, accessibility audit
Phase 7 QA automation: scenario automation, regression maintenance, CI/CD gates, coverage reporting
Transition documents: 60 held-out phase-boundary cases
```

---

## Audit Verdict

```text
Available source authority supports continued corpus expansion: YES
Available source authority supports declaring full v4 corpus complete now: NO
```
