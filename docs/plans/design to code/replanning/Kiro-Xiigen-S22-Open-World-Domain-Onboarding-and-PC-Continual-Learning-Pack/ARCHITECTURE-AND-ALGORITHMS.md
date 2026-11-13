# S22 — Open-world domain admission and hybrid learning architecture

## Narrow conceptual novelty (not a claimed implementation)
This slice tests one concrete gap: **the transition from an unknown subject in a normal user request to a source-law verified, provisional but usable subject-domain descriptor**. It does NOT claim the transition is missing from K; S22.00A must trace actual caller and selected state. It does not own native domain classifier internals, semantic encoder, tokenizer, kNN, Unit, Flow, Orchestrator, Graph, Student/LoRA trainer, experimental lab or release.

```text
ordinary user request
 → StackDetector (D-07, raw markers protected)
 → D-04 task INTENT (CODE_Q/BUG/PLAN/ARCH/REFACTOR)
 + subjectDomain/project/stack/lawVersion/sourceRights/typed output
 → existing subject mapper (RP02)
 → [S22 ONLY IF MISSING: source-law verified NEW→PROVISIONAL admission]
 → compatible semantic predicates + codebook/token/vector (RP02/RP18)
 → rights-, law-, index-version eligible memory (RP02)
 → BM25 + lawful multi-neighbor adaptive kNN (RP02)
 → ILearningUnit typed many→many (RP03) → ILearningFlow correction (RP03)
 → bounded async ILearningOrchestrator (RP04)
 → higher-order ILearningDynamicGraph legal pattern paths (RP04)
 → arbiter D-15 → existing Student/project LoRA (RP01)
 → normal response with provenance and bounded PC
 → consented feedback, weakness isolation, optional authorized CODEX/Chinese DPO
 → existing one-time Student update (RP01/RP05), independent RP09 exam
 → RP10 cold permitted state pointer; next normal feedback
```

### Conceptual provisional descriptor — proposed, not K DTO
`{subjectDomainId, taskIntentD04, projectId, stack, lawSourceId, lawEdition, dialect, effectiveDate, inputSchema, outputSchema, predicateVocabularyId, directionNegationUnits, tokenCodebookVersion, vectorSpaceId, indexCompatibility, allowedMemorySourceIds, adapterProjectId, rightsAndTenant, status, confidence, provenance}`.
The implementation must map **only** existing necessary fields into actual K contracts. Never add all proposed fields by default. States are analytical `UNSEEN→SOURCE_PENDING→PROVISIONAL_READONLY→RETRIEVAL_ELIGIBLE→LEARNING_CANDIDATE→ACCEPTED`; no automatic transition to trainable/accepted when law or rights are unknown. Approved adapters remain per project (D-05); graph and vector source can remain distinct.

### Four distinct learning owners
| Role | Deterministic contract | Trainable state must be explicitly proven |
| --- | --- | --- |
| ILearningUnit | typed domain/vector/input/output envelope, many→many association IDs; legal candidates | actual Unit mapper/ranker/weights or supervised memory state; not a single output ZIP |
| ILearningFlow | typed source OutputRef→corrected next input, version and relation validity | actual inter-Unit relation/correction state; Unit frozen in Flow causality |
| ILearningOrchestrator | DAG, async fanout/fanin, cancellation/deadline, total resource budget | learned selection of parallel lawful routes; not `Task.Run` alone |
| ILearningDynamicGraph | legal higher-order relations/patterns and arbiter-compatible edge semantics | actual learned edge/motif preference and new combinations, not static graph |
Student project LoRA and Builder teaching policy are separate fifth/sixth state *families*, NOT new layers or an alternate trainer. `ILearningMachine` owns training per K/00.Core/LearningMachineContracts.cs.

### Authority constraints & real source warning
K `PromptDomainClassifier.cs` classifies D-04 via SemanticCompilerAdapter. K `KnnLogicalUnit.cs` uses token+predicate retrieval; prior RP02 documents a first-hit consumer, **verify current code** before assuming still present. K `17.DynamicDomains/DynamicDomainGraph.cs` constructs transition closures/bridges; no premise that it solves unknown subject classification. K `LogicalContracts.cs` currently exposes `ILogicalUnit<TInput,TOutput>` with `LogicalUnitResult<TOutput>`, not automatically the exact original `IEnumerable` many→many dream. Map conceptual requirement to real producer→consumer before designing adapter.

### Falsifiable S22-specific hypotheses
- **H22-N1:** exact provenance-backed provisional admission reduces wrong-confident subject selection on truly withheld domains at matched PC and no Student update.
- **H22-N2:** lawful subject-specific semantic coordinates + compatible multi-hit retrieval improve route utility over intent-only, lexical and forced-known baselines *where those owners truly exist*.
- **H22-N3:** existing typed four-role combination transfers new subject law under budget better than cheapest deterministic valid path; otherwise deterministic wins.
- **H22-N4:** a new author/judged unique Student lesson improves unseen families in a new subject beyond C0, without old-domain regression; optional, not assumed.
- **H22-N5:** accepted novelty state can be cold loaded, safely reverted, and maintained by bounded consented user feedback.
Reject a hypothesis without widening task scope if mechanism already exists, effect is zero/negative, law unverifiable, no downstream consumer or PC cost unacceptable.

### No second-owner decision
If the missing factor is native domain mapper/kNN → RP02; semantic transposition → RP18; Unit/Flow → RP03; async Graph → RP04; DPO objective/LoRA → RP01; feedback/replay → RP05; Builder chooser → RP07; causal lab → RP09; cold pointer → RP10; law drift → S17/RP14; multi-domain interference → S19. S22 owns only actual unseen-domain eligibility handoff in normal consumer, when proven absent.
