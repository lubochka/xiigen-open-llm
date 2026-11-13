# External Source Registry
# Version: 5.0 - 2026-05-05
# Supersedes: EXTERNAL-SOURCE-REGISTRY-v4.md

---

## Canonical Pattern Dependency

This registry uses `DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v2.md` as the
authority. Sources are mapped to runtime contracts, not to informal component
names.

---

## Registry Rule

Every source entry must state:

```text
which transition types it can teach
which phases, sub-phases, and task types it can teach
which runtime contracts it can produce rows for
which uses are excluded
```

If a source cannot be mapped to at least one runtime contract, it remains outside
the learning set.

---

## License States

Allowed states:

```text
first_party
external_license_checked
attribution_required
source_gap
corruption_case
verify_before_use
```

`verify_before_use` cannot enter training rows. It may enter source-gap notes or
candidate future-source lists.

---

## Runtime Contract Registry

### Orchestrator Contracts

| Contract | Source use |
|---|---|
| `LogicalTransitionOrchestrator : ILogicalOrchestrator` | Real prompts, transition candidate evidence, selected/rejected transition examples. |
| `TeachingOrchestrator : ILogicalOrchestrator` | Measured learning-cycle prompts and retraining workflows. |

### Flow Node Contracts

| Contract | Source use |
|---|---|
| `DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>` | Phase, sub-phase, task split rows. |
| `DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>` | Formal domain vocabulary rows. |
| `SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>` | Dependency, parallelism, wait-state, recovery rows. |
| `DomainProcessorUnit : ILogicalUnit<WorkBranch, ProcessedBranchResult>` | Branch reasoning rows. |
| `ApplierUnit : ILogicalUnit<ProcessedBranchResult, AppliedBranchResult>` | Logical application rows, code optional. |
| `ResultTranslatorUnit : ILogicalUnit<AppliedBranchResult[], TranslatedResultFragment[]>` | Formal-to-human fragment rows. |
| `ArbiterSelectorUnit : ILogicalUnit<TranslatedResultFragment[], ArbiterReviewRequest[]>` | Arbiter routing rows. |
| `ArbiterUnit : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>` | Pass, concern, reprocess, reject rows. |
| `FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>` | Human result rows. |

### Teaching Contracts

| Contract | Source use |
|---|---|
| `PairParserUnit : ILogicalUnit<PairFileSet, ParsedPairDataset>` | Pair file ingestion rows. |
| `FeatureExtractionUnit : ILogicalUnit<ParsedPairDataset, FeatureVectorDataset>` | Feature extraction rows. |
| `ModelTrainingUnit : ILogicalUnit<FeatureVectorDataset, TrainingResult>` | Measured training rows. |
| `HeldOutEvaluationUnit : ILogicalUnit<ModelEvaluationInput, HeldOutEvaluationResult>` | Held-out evaluation rows. |
| `CorrectionLearningUnit : ILogicalUnit<CorrectionTrainingInput, TrainingResult>` | One-row correction and retraining rows. |

---

## Source Families

### XIIGen Conversation and Session Material

License state:

```text
first_party
```

Primary value:

```text
real user prompt language
real corrections
real rejected explanations
real development-cycle intent
```

Supported contracts:

```text
LogicalTransitionOrchestrator
DomainSplitterUnit
SchemePlannerUnit
TransitionCandidateSelector
FormatterUnit
CorrectionLearningUnit
```

Excluded use:

```text
Do not turn user corrections into generic rules without preserving the exact failed input and corrected output.
```

### XIIGen Architecture and Experiment Documents

License state:

```text
first_party
```

Primary value:

```text
ground truth for correct orchestration
phase and sub-phase topology
dependency rules
arbiter expectations
learning-cycle requirements
```

Supported contracts:

```text
DomainTranslatorUnit
SchemePlannerUnit
ArbiterSelectorUnit
ArbiterUnit
FormatterUnit
ModelTrainingUnit
HeldOutEvaluationUnit
```

Excluded use:

```text
Do not use an oracle document as a user prompt. It is authority evidence for B, not natural A-side wording.
```

### Wave 1 Security Corpus

Location:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\wave-1\
```

License state:

```text
external_license_checked
```

Primary value:

```text
security authority evidence for OAuth, OIDC, JWT, mTLS, ASVS, NIST SSDF, and CWE cases
```

Supported contracts:

```text
DomainTranslatorUnit
SchemePlannerUnit
ArbiterSelectorUnit
ArbiterUnit
FormatterUnit
HeldOutEvaluationUnit
```

Transition support:

```text
feature_addition when security is one branch of a larger addition
repair when prompt states existing security behavior is unsafe
external_source_integration when prompt asks to ingest or bind security authority sources
qa_release when security review gates promotion
```

Important limit:

```text
Wave 1 is saturated for sub-phase 3.4 but does not cover the whole lifecycle.
It must not force every security-related prompt into repair or external-source integration.
```

### External Architecture Sources

Examples:

```text
NIST SP 800-160
C4 model documentation
ISO/IEC 25010
architecture decision record guidance
```

License state:

```text
external_license_checked or verify_before_use
```

Supported contracts:

```text
DomainSplitterUnit
DomainTranslatorUnit
SchemePlannerUnit
ArbiterUnit
```

Primary sub-phases:

```text
3.1 architecture and component topology
3.5 infrastructure and deployment topology
6.1 architecture review
```

### External API and Contract Sources

Examples:

```text
Google AIP
OpenAPI guidance
HTTP API style guides
```

Supported contracts:

```text
DomainSplitterUnit
DomainTranslatorUnit
SchemePlannerUnit
ArbiterSelectorUnit
FormatterUnit
```

Primary sub-phases:

```text
3.3 API contract definition
4.3 integration and provider connectors
5.2 integration tests
```

### External Testing and QA Sources

Examples:

```text
testing strategy guides
security testing guides
CI/CD gate guidance
QA automation framework docs
```

Supported contracts:

```text
DomainSplitterUnit
SchemePlannerUnit
DomainProcessorUnit
ApplierUnit
ArbiterUnit
HeldOutEvaluationUnit
```

Primary sub-phases:

```text
5.1 unit tests
5.2 integration tests
5.3 end-to-end tests
5.4 performance and load tests
6.3 security and compliance review
7.3 CI/CD gate configuration
7.4 coverage and reporting
```

---

## Current Gap Map

### Covered Deeply

```text
3.4 security and authorization model
```

Wave 1 covers this in depth.

### Seeded But Not Ready

```text
all other 30 sub-phases
```

The router may have seed rows, but external learning-set coverage is not enough
for real training.

### Highest Priority Missing Coverage

```text
6.3 security and compliance review
3.1 architecture and component topology
5.2 integration tests
3.4 -> 4.1 security-to-implementation transition
3.1 -> 3.4 architecture-to-security transition
teaching-unit rows for measured learning
```

---

## Registry Acceptance Checklist

Before a source registry update is accepted:

- [ ] Every source has a license state.
- [ ] Every source maps to at least one runtime contract.
- [ ] Every source states excluded uses.
- [ ] Every source separates prompt language from authority evidence.
- [ ] Every source maps to phase, sub-phase, and task type when applicable.
- [ ] Every source states whether it supports transition selection, expert split, node execution, review, formatting, or teaching.

---

## Last Updated

2026-05-05.

