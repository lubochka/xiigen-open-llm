# Development Phases, Sub-Phases, and Document Identification Guide
# Extension - Parts 3 through 5
# Continuation of Version 1.0 - 2026-05-04

This extension corrects the phase guidance so it matches the actual XIIGen
dynamic-flow design:

- The fixed contract is the 22 logical-predicate vocabulary.
- Phase, sub-phase, company method, and document type are domain tags or
  learned signatures over that vocabulary.
- The orchestrator operates on compiled predicate vectors first, not on raw
  words and not on deterministic phase schemas.
- Phase transition quality is measured by predicate-vector continuity,
  constraint preservation, rejected-item preservation, and knowledge-edge
  visibility.

---

## Part 3 - Representation Layers

### The Three Layers Must Not Be Mixed

The dynamic flow uses three separate representation layers.

**Layer 1: logical predicates.** These are the 22 shared operation predicates
validated by the semantic compiler. They are the fixed inter-unit language:

`NULL_GUARD`, `FAIL`, `ITERATE`, `HANDLE_ERROR`, `AWAIT`, `THROW`,
`DECLARE`, `ASSIGN`, `CREATE`, `CALL`, `BRANCH`, `RETURN`, `GET_OP`,
`VALIDATE_OP`, `CREATE_OP`, `UPDATE_OP`, `DELETE_OP`, `PROCESS_OP`,
`CONVERT_OP`, `SEND_OP`, `LOG_OP`, `AUTH_OP`.

**Layer 2: domain tags.** These describe where the logical predicates are
active. Examples: design-system, accessibility, business-rule,
api-contract, tenant-security, implementation-plan, test-coverage,
qa-verdict, ci-automation. These are not predicates. They are labels attached
to predicate vectors.

**Layer 3: learned signatures.** These are statistical patterns over
predicate vectors and domain tags. A phase is not a fixed schema. A phase is a
learned signature such as: high `DECLARE` + `VALIDATE_OP` + `AUTH_OP` with
technical-design domain tags and high-confidence constraint edges.

All learning-set B outputs must respect this separation. A B output may say:
"predicate vector activates `DECLARE`, `VALIDATE_OP`, and `AUTH_OP`; domain
tags: api-contract, security-enforcement." It must not invent a predicate
called `api-shape` or `security-enforcement`.

### What a Phase Means in This Implementation

A phase is a topology unit whose active sub-phases are inferred from predicate
evidence. The company methodology is adaptive because phase names and
sub-phase lists are registry data, while the compiled predicate representation
remains stable.

If a company merges design and functional specification, the topology contains
a merged phase node. That node still receives predicate vectors using the same
22 slots. It simply has a learned signature that combines design-domain tags
and functional-spec-domain tags.

If a company runs QA in parallel with implementation, the phase topology stores
parallel edges. The transition gate still compares upstream and downstream
predicate vectors. It does not require a canonical waterfall order.

### Phase Signatures by Predicate Evidence

The following phase descriptions are examples of learned signatures, not new
predicate vocabularies.

**Phase 1 - UI/UX definition.** Typical predicate evidence: `DECLARE` for
design tokens and screen structures, `VALIDATE_OP` for accessibility rules,
`BRANCH` for interaction states, `CONVERT_OP` for responsive transformation
rules. Typical domain tags: design-system, screen-inventory, accessibility,
interaction-state.

**Phase 2 - functional specification.** Typical predicate evidence:
`DECLARE` for actors and capabilities, `VALIDATE_OP` for business rules,
`BRANCH` for conditional behavior, `RETURN` for observable outcomes.
Typical domain tags: actor-goal, acceptance-criteria, business-rule,
system-boundary.

**Phase 3 - technical design.** Typical predicate evidence: `DECLARE` for
component contracts, `VALIDATE_OP` for constraints, `AUTH_OP` for security
model, `PROCESS_OP` for lifecycle responsibilities, `BRANCH` for architectural
decision paths. Typical domain tags: component-contract, data-contract,
api-contract, security-enforcement, infrastructure-topology.

**Phase 4 - implementation.** Typical predicate evidence: `CREATE_OP`,
`UPDATE_OP`, `DELETE_OP`, `CALL`, `ASSIGN`, `RETURN`, `VALIDATE_OP`.
Typical domain tags: code-change, integration-point, service-contract,
verification-command.

**Phase 5 - testing.** Typical predicate evidence: `VALIDATE_OP`, `BRANCH`,
`FAIL`, `RETURN`, `PROCESS_OP`. Typical domain tags: test-scenario,
assertion-condition, coverage-target, failure-evidence.

**Phase 6 - QA.** Typical predicate evidence: `VALIDATE_OP`, `FAIL`,
`GET_OP`, `RETURN`, `LOG_OP`. Typical domain tags: qa-verdict,
coverage-gap, compliance-evidence, regression-boundary.

**Phase 7 - QA automation.** Typical predicate evidence: `CALL`, `ITERATE`,
`VALIDATE_OP`, `SEND_OP`, `LOG_OP`, `FAIL`. Typical domain tags:
automation-binding, ci-gate-condition, suite-maintenance,
threshold-specification.

---

## Part 4 - Adaptive Phase and Sub-Phase Orchestration

### The Phase Registry Is a Learned Topology

The phase registry is not a deterministic methodology catalog. It is a
versioned flow topology stored in the same flow-registry family as other
dynamic-flow topologies.

Each phase node has:

- a phase id,
- a human label,
- a learned predicate signature,
- a set of allowed sub-phase routers,
- transition edges to other phase nodes,
- minimum predicate-continuity thresholds,
- conformance tests for that methodology version.

A methodology change is a topology version change, not code. A rollback is a
pointer move to a previous topology version.

### What the Sub-Phase Router Learns

The sub-phase router receives one compiled predicate vector plus domain tags.
It emits active sub-phase packets with confidence scores.

For example, a Phase 3 router may receive:

- high `DECLARE`, `VALIDATE_OP`, and `AUTH_OP`,
- domain tags: component-contract, security-enforcement,
- dependency edges from business-rule tags.

It may emit:

- sub-phase 3.1 architecture decisions,
- sub-phase 3.4 security and compliance,
- a boundary marker for sub-phase 3.3 API contracts if interface details are
  missing.

The router learns these emissions from examples. It does not inspect raw
document titles as authority.

### Phase Context Compiler

At the end of a phase run, a phase context compiler receives the complete
predicate output of that phase. It produces a compact carry-forward predicate
summary.

The summary is not a fixed schema. It is a ranked set of predicate fragments
with metadata:

- predicate ids from the 22-slot vocabulary,
- domain tags,
- source phase and sub-phase,
- confidence score,
- dependency edges,
- arbiter status,
- evidence count,
- role in transition: carry-forward constraint, boundary, knowledge edge, or
  rejected pattern.

Those four transition roles are classifications on predicate fragments. They
are not schema fields that must always be present.

### Transition Verification Unit

The transition verification unit receives:

- the upstream carry-forward predicate summary,
- the downstream source packet's compiled predicate vector,
- the active phase-topology edge,
- the minimum semantic-continuity threshold for that edge.

It produces:

- `PASS`,
- `PASS_WITH_GAPS`,
- or `BLOCK`.

The verdict is based on predicate-vector similarity, constraint preservation,
rejected-pattern preservation, and knowledge-edge visibility.

`PASS` means high-confidence upstream constraints have semantic neighbors in
the downstream vector and no rejected pattern was reinstated.

`PASS_WITH_GAPS` means a relevant fragment is missing or weak, but it is a
named boundary, a low-confidence item, or a scope-limited gap.

`BLOCK` means a high-confidence constraint was dropped, contradicted, or a
rejected pattern was reinstated.

---

## Part 5 - Experiments Required to Complete the Design

The design is not complete until four experiment families pass.

### P1 - Within-Phase Coverage

**Purpose.** Prove that one phase can cover its active sub-phases without
dropping boundaries or inventing missing content.

**First target.** Phase 3 technical design, because existing project documents
already contain architecture, security, implementation, and learning
constraints.

**Required oracle.**

- Expected 22-slot predicate vector for each active sub-phase.
- Expected domain tags for each active sub-phase.
- Expected knowledge edges for missing sub-phases.
- Expected rejected items.
- Human-readable final result checklist.

**Pass gate.**

- Every active sub-phase appears in the trace.
- Missing sub-phases appear as boundary or knowledge-edge packets.
- No domain tag is treated as a predicate.
- The final result names covered, blocked, rejected, and unknown material.

### P2 - Phase Context Carry-Forward

**Purpose.** Prove that upstream phase output becomes downstream context at
the predicate level.

**First target.** Phase 3 technical design to Phase 4 implementation.

**Scenarios.**

- BaseService inheritance is visible upstream but not enforced downstream:
  `PASS_WITH_GAPS`.
- Cross-phase context packet mechanism is absent downstream:
  `PASS_WITH_GAPS`.
- Downstream build-time implementation sequence resembles the rejected static
  runtime pipeline at surface level but differs in predicate role:
  `PASS`.

**Pass gate.**

- Two concerns, zero blocks, one pass.
- The build-order scenario does not falsely block.
- The evidence shows predicate-vector continuity, not text matching.

### P3 - Learning From Transition Failure

**Purpose.** Prove that a failed transition creates learning material and the
next learned unit improves without over-generalizing.

**Procedure.**

- Run P2 with a corrupted downstream packet that fully removes the
  BaseService constraint neighbor.
- Expect `BLOCK`.
- Store the finding as rejected transition feedback.
- Run the idle learning cycle.
- Run the original P2 case again.

**Pass gate.**

- The corrupted case blocks.
- The original present-but-unenforced case remains `PASS_WITH_GAPS`, not
  `BLOCK`.
- The learned behavior is specific to the dropped predicate.

### P4 - Full Lifecycle Phase Topology

**Purpose.** Prove that a topology of phases can orchestrate a lifecycle
across multiple phase runs.

**First topology.**

- Phase 1 UI/UX definition.
- Phase 3 technical design.
- Phase 4 implementation.

**Pass gate.**

- Phase 1 output produces a carry-forward predicate summary.
- Phase 3 consumes the Phase 1 summary and preserves design constraints.
- Phase 3 output produces a carry-forward predicate summary.
- Phase 4 consumes the Phase 3 summary and preserves technical constraints.
- Both transition verdicts are correct.
- Gap packets are attributed to the phase that produced the gap, not only to
  the node that detected it.

---

## Document Readiness for Phase Experiments

A document is experiment-ready only when all of these are true:

1. It answers at least one phase or sub-phase completion question.
2. It contains source-authoritative decisions, constraints, boundaries,
   knowledge edges, or rejected items.
3. The expected 22-slot predicate vector can be produced by hand for the
   three highest-confidence fragments.
4. The domain tags can be named without inventing new predicate names.
5. The document can produce at least one concrete pass/fail gate.

If condition 3 fails, the document is too vague for a predicate-level
experiment. If condition 4 fails, the author is mixing domain language with
predicate language and must rewrite the oracle.

---

## Learning-Set Rule for These Experiments

Every learning pair created from these experiments must show:

- A input: compiled predicate vector, domain tags, confidence scores,
  dependency edges, and any upstream carry-forward fragments.
- B output: the actual station output in predicate-level form or the actual
  human translation when the station is a translator/formatter.

The B output must never say "a good phase summary" or "the right transition
decision." It must contain the actual vector, packet, verdict, plan, or human
result that the unit is supposed to produce.

This keeps the design adaptive without allowing methodology schemas or
domain labels to masquerade as logical predicates.
