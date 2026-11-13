# Held-Out and Corruption Pairs (048)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: held_out | corruption

---

## Held-Out Pair B06B1-WDA-HO-96 — Round3FullReference: DOCUMENT predicate fires

```
source_id: xiigen-b06-held-out-96
pair_kind: held_out
```

A:
  run_id: "B06-ho-96"
  scenario: "DocumentPredicateCheck"
  user_request: "Document the requirements for round3fullreference."

Expected B:
  CompiledPredicateIds: ["DOCUMENT"]
  SubPhaseId: "2.1"
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TransitionType: "documentation"
  DomainId: "software-development"

Generalization test: "document" fires DOCUMENT; DomainId is software-development not round3fullreference.

---

## Held-Out Pair B06B1-WDA-HO-97 — Round3FullReference: DOC+ARCH → documentation with 2 SubPhases

```
source_id: xiigen-b06-held-out-97
pair_kind: held_out
```

A:
  run_id: "B06-ho-97"
  scenario: "DocArchCheck"
  user_request: "Describe the round3fullreference requirements and design the service architecture."

Expected B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  RoutingDecisions: [SubPhaseId: "2.1", SubPhaseId: "3.1"]
  TransitionType: "documentation" (ARCHITECTURE does not upgrade to feature_addition)
  CanonicalOrdering: "2.1 before 3.1"

Teaching: ARCHITECTURE alone with DOCUMENT → documentation (not feature_addition). Two SubPhases emitted.

---

## Corruption Pair B06B1-WDA-COR-50 — v6-02: IMPLEMENT false-positive for Round3FullReference

```
source_id: xiigen-b06-corruption-50
pair_kind: corruption
corruption_type: Corruption-v6-02-implement-false-positive
```

A:
  user_request: "Describe the round3fullreference feature requirements."

B-correct:
  CompiledPredicateIds: ["DOCUMENT"]
  Note: "describe" is a DOCUMENT lexicon term. No IMPLEMENT trigger present.

B-corrupt:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Note: IMPLEMENT spuriously added — no implementation verb in request.

Corruption: IMPLEMENT fires only for create/build/develop/implement/write-code verbs. "describe" is not an IMPLEMENT trigger.
Fix: Scan for explicit IMPLEMENT lexicon terms. Do not infer IMPLEMENT from domain name alone.

---

## Corruption Pair B06B1-WDA-COR-50b — v6-03: DomainId wrong for Round3FullReference

```
source_id: xiigen-b06-corruption-50b
pair_kind: corruption
corruption_type: Corruption-v6-03-domainid-wrong
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  Context: "Round3FullReference: document requirements"

B-correct:
  DomainId: "software-development"

B-corrupt:
  DomainId: "round3fullreference"

Corruption: DomainId must always be software-development. Business domain name as DomainId is v6-03.
Fix: DomainId is derived from the project type (software development), not the business subject matter.
