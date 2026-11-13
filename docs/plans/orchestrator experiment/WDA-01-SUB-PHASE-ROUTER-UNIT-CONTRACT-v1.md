# WDA-01 Sub-Phase Router Unit Contract v1
# Date: 2026-05-05

---

## Purpose

Define the real logical-unit boundary that replaces the WDA-01 deterministic
classification fixture.

The unit does not receive raw text. It receives a compiled source-document
representation with predicate ids and source metadata, then routes it to one
registered lifecycle sub-phase.

---

## Unit

```text
unit_id: unit-sub-phase-router-v1
unit_role: sub-phase-router
domain_tag: whole_development_area
contract: ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision>
```

---

## A Input Shape

```text
SubPhaseRouterSourceDocument
  DocumentId
  SourcePath
  SourceTier
  LicenseOrAuthority
  DocumentType
  DocumentTitle
  CompiledText
  CompiledPredicateIds
  SourceStatus
  KnownFailureMode
```

The A input is built from the WDA-01 real source ledger.

The unit reads the active sub-phase catalog from `ILogicalTopologyRegistry`.
That catalog is the adaptive topology target. The unit does not hardcode the
31 sub-phases.

---

## B Output Shape

```text
LogicalUnitResult<SubPhaseRoutingDecision>
  UnitId
  UnitRole
  DomainTag
  Output
    DocumentId
    SourceStatus
    PredictedPhaseId
    PredictedSubPhaseId
    Confidence
    NearestWrongSubPhaseScore
    EvidencePredicates
    KnowledgeEdge
    FailureClassification
  Confidence
  PredicateIds
  DomainTags
  EvidenceIds
  Warnings
```

B is the actual routing decision, not a description of what a good routing
decision should contain.

---

## Required Behavior

For WDA-01 Phase 81:

- classify 31 real-source probes;
- return the expected phase id for all 31;
- return the expected sub-phase id for all 31;
- keep minimum confidence at or above 0.70;
- keep maximum wrong-domain score below 0.50;
- return zero source knowledge edges for ready ledger rows;
- return zero router knowledge edges for the accepted seed set;
- expose evidence predicates used by the routing decision;
- return through `OperationResult<T>` and never throw for expected input errors.

---

## Optimization And Safety Flags

Default flags remain active:

```text
UseCompiledPredicateRepresentation = true
UseDomainScopedRetrieval = true
UseConfidenceFloor = true
UseLearnedUnitWhenAvailable = true
UseTraceEvidence = true
UseContextCarryForward = true
UseIdleLearningScheduling = true
UseArbiterAuthority = true
UseKnowledgeEdgeDetection = true
UseInternalFirstExecution = true
RequireExternalAiApproval = true
RejectArbiterContradictions = true
CompareAgainstOracle = true
FailOnMetricDrift = true
```

`UseDeterministicFixtureBaseline` must be false or omitted for this proof.

---

## Failure Classification

| Failure | Error or output signal |
|---|---|
| Missing source document input | `SOURCE_DOCUMENT_REQUIRED` |
| Empty document id | `EMPTY_DOCUMENT_ID` |
| Missing topology registry | `TOPOLOGY_REGISTRY_REQUIRED` |
| Empty topology catalog | `NO_SUB_PHASES_REGISTERED` |
| Score below confidence floor | `knowledge_edge: sub-phase evidence below confidence floor` |
| Valid ready source | `source_classified_by_sub_phase_router` |

---

## Promotion Meaning

Passing this contract opens Wave 1 seed expansion. It does not complete the
broad v4 corpus.

The next corpus step remains quota work:

```text
31 sub-phases * 100 training documents
31 sub-phases * 20 held-out documents
60 transition held-out documents
```
