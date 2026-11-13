# PARALLEL-DOMAIN-TRAINING-ROADMAP-v1.md
# Version: 1.0 — 2026-05-28
# Purpose: Define five parallel training directions, the core changes each requires,
#          and the Gate B for each direction's first working slice.
# Dependency: TECHNICAL-DEBT-CLOSURE-PHASE-0.md must be complete before
#             any domain project is scaffolded.
# Authority: ARCHITECT-PLAN-LEARNING-MACHINE-EVOLUTION-v1.md

---

## Shared prerequisite

All five directions share the same prerequisite: TD3 must be complete.
TD3 adds `ILearningMachine<TDomain>` to `00.Core`.
Without it, domain projects cannot implement a common contract.

Directions 2 also requires TD1 and TD2 (static units wrapped, wiki flow registered).
Directions 1, 3, 4, 5 do not depend on TD1 or TD2.

---

## Direction 1 — Translation (RU / HE ↔ EN)

### Purpose

Translate user prompts, chat messages, posts, and texts between Russian, Hebrew, and
English. Enables the networking and chat product for users across language boundaries.

### What already exists

Location: `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\`

- `TRANSLATION-FOUR-LEVEL-LLM-TRAINING-PLAN-v1.md` — training plan for four adapters
- `training-pairs-train-ready.jsonl` — 49 980 bytes of ready DPO pairs
- `TRANSLATION-SOURCE-REGISTRY-v3.md` — corpus authority decisions (ACCEPT/BLOCK per source)
- `TRANSLATION-ROUTING-FLOW-DPO-GUIDANCE-v1.md` — routing-layer DPO guidance
- `TRANSLATION-TOPIC-ROUTER-ORCHESTRATION-PLAN-v1.md` — topic-router design

Dedicated execution guide:

`docs/plans/TRANSLATION-DOMAIN-TRAINING-GUIDE-v1.md`

This guide defines the product use cases (chat, post/text, user-prompt
translation), real held-out prompts, Gate B runner evidence, and the exact
export/import boundary for Direction 1.

The plan specifies four direction adapters: RU_EN, EN_RU, HE_EN, EN_HE.
The batch discipline is: 50 new training pairs + 50 held-out evaluation rows per batch.

### New project

`27.TranslationDomain`

References: `00.Core`, `11.DPO`, `26.TrainableRouter`

### Core changes required

```csharp
// Domain marker
public sealed class TranslationDomain { }

// Input record
public sealed record TranslationRoutingRequest(
    string SourceLanguage,
    string TargetLanguage,
    string TopicDomain,
    string InputText);

// Flow — implements ILogicalFlow from 00.Core
TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>

// Machine — implements ILearningMachine from TD3
TranslationMachine : ILearningMachine<TranslationDomain>
```

`TranslationRoutingFlow` is registered in `15.OrchestratorModel/Logical/
LogicalOrchestratorAssistant.cs` with flow id `translation-routing-flow`.

`ProjectWikiPluginOrchestrationAdapter` adds a `TranslatePromptStep` before
`BuildIntent` for non-English prompts. This step calls `TranslationRoutingFlow`
through `ILogicalOrchestrator`.

### Gate B Direction 1

```bash
# Project scaffold
ls C:/Projects/llm_mvp_core/27.TranslationDomain/
# Expected: TranslationDomain.csproj, TranslationRoutingFlow.cs,
#           TranslationMachine.cs found

# Flow registered
grep -n "translation-routing-flow" \
  C:/Projects/llm_mvp_core/15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs
# Expected: at least 1 line with the flow id

# ILearningMachine implemented
grep -n "ILearningMachine<TranslationDomain>" \
  C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationMachine.cs
# Expected: class declaration line found

# First training batch accepted
dotnet test C:/Projects/llm_mvp_core/Tests/LlmFromScratch.Tests.csproj \
  --filter "TranslationDomainTests" 2>&1 | tail -5
# Expected: 44+ tests pass, 0 failed (44 as of phase 24, 2026-05-29)
# One test must verify TrainBatchAsync returns AccuracyAfter > 0
```

---

## Direction 2 — Smart Code Assistant (Local Wiki)

### Purpose

Help developers in IDEs understand their codebase, find dependencies, trace call chains,
and understand impact of changes. Works on-premise with no external service. Supports
Visual Studio, VS Code, Cursor, Windsurf, Codex CLI.

### What already exists

`42.ProjectWikiDomain` with full DPO infrastructure for:
- `WikiTraceAggregatorUnit` (next action from PROJECT-LOCAL-WIKI-MODEL-STATE.json)
- `WikiContextRoutingUnit`
- `CodeTraceBuilderUnit`
- `EntityImpactMapperUnit`
- `ProjectWikiRequestRouterUnit`
- `ProjectWikiAnswerComposer`
- `DocumentationGeneratorUnit`
- `SystemBoundaryMapperUnit`
- `StackAndVersionClassifierUnit`
- `CrossStackBusinessFlowGraphUnit`

Current model state (PROJECT-LOCAL-WIKI-MODEL-STATE.json):
- Session: SESSION-01 (query planning)
- Status: QUERY_PLANNING_BATCH_030_REJECTED
- Next action: WikiTraceAggregator — train cross-unit trace rows proving
  ProjectWikiQueryPlanningFlow → WikiContextRoutingUnit → WikiAnswerComposer
  order and blocking answer-before-evidence-analysis

### Dependencies

Requires TD1 and TD2 to be complete before the training loop can use
the ILogicalOrchestrator path.

### New machine wrapper

```csharp
// Domain marker
public sealed class CodeWikiDomain { }

// Machine — wraps all of 42.ProjectWikiDomain
CodeWikiMachine : ILearningMachine<CodeWikiDomain>
```

`CodeWikiMachine.TrainBatchAsync()` dispatches to the relevant existing training
runner based on the batch's domain tag (WikiTraceAggregator, IntentBuilder, etc.).

### Platform expansion (18 platforms from extensions plans)

Each platform requires a dedicated `StackAdapterUnit`:

```csharp
ILogicalUnit<ProjectScanRequest, StackEvidence>
```

Confirmed existing support: Angular, .NET, SQL (from ProjectUnderstandingUnit)
Planned: Java (Spring Boot), React, AngularJS, Vue, React Native, Python, etc.

Each platform adapter has its own DPO generator. Training is domain-scoped per platform.

### Gate B Direction 2

```bash
# Machine wrapper present
grep -n "CodeWikiMachine" C:/Projects/llm_mvp_core/42.ProjectWikiDomain/
# Expected: class found implementing ILearningMachine<CodeWikiDomain>

# WikiTraceAggregator next batch passes
dotnet test C:/Projects/llm_mvp_core/Tests/LlmFromScratch.Tests.csproj \
  --filter "WikiTraceAggregatorTests" 2>&1 | tail -5
# Expected: gate test passes with heldout_accuracy >= 0.75

# Orchestrator path (after TD2)
grep -n "project-wiki-orchestration-flow" \
  C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiPluginOrchestrationAdapter.cs
# Expected: RunAsync call with project-wiki-orchestration-flow id found
```

---

## Direction 3 — Affiliate Training Assistant

### Purpose

Help affiliates learn marketing, sales, system design, and how to use XIIGen products.
Personal machine holds the affiliate's own data. Global machine holds shared domain
knowledge for all affiliates. Personal data never leaves the affiliate's device/server.

### What already exists

New first-party training source:

`C:\Users\lubao\Documents\אחותי.zip`

Dedicated guide:

`docs/plans/ACHOTI-ENTREPRENEURSHIP-AFFILIATE-TRAINING-GUIDE-v1.md`

This archive contains Hebrew entrepreneurship, digital marketing, networking,
event-marketing, presentation, funnel, and business self-diagnosis materials.
It is a private first-party source for `ILearningMachine<EntrepreneurshipBootstrappingDomain>`
and for the global side of the affiliate assistant. Raw materials are not
repository assets; only source inventory, manifests, training reports, and
held-out prompt definitions may be stored in the repo.

The domain still requires:
- New subproject `31.AffiliateDomain` (number 28 is occupied by `28.SemanticNlp` — decision A-LM-05 analogue)
- New machine pair

### Core changes required

```csharp
// Domain markers
public sealed class AffiliatePersonalDomain { }
public sealed class AffiliateGlobalDomain  { }

// Machine pair — implements ILearningMachinePair from TD3
AffiliateMachinePair : ILearningMachinePair<AffiliatePersonalDomain, AffiliateGlobalDomain>

// Individual machines
AffiliatePersonalMachine : ILearningMachine<AffiliatePersonalDomain>
AffiliateGlobalMachine   : ILearningMachine<AffiliateGlobalDomain>
EntrepreneurshipBootstrappingMachine : ILearningMachine<EntrepreneurshipBootstrappingDomain>
```

`AffiliateGlobalMachine` lives on the server (25.ModelServer + new
`AffiliateGlobalSyncService` in 15.OrchestratorModel).
`AffiliatePersonalMachine` lives on the affiliate's device as a client library.

`SyncPersonalToGlobalAsync`: sends only anonymised training signal DTOs.
No personally identifying data, no raw user content, no conversation text is included
in the sync payload. The sync contract must be verifiable by inspection.

### Gate B Direction 3

```bash
# Project scaffold
ls C:/Projects/llm_mvp_core/31.AffiliateDomain/
# Expected: AffiliateDomain.csproj, AffiliateMachinePair.cs found

# Sync payload isolation
grep -n "Anonymise\|AnonymisedSignal\|SyncPayload" \
  C:/Projects/llm_mvp_core/31.AffiliateDomain/AffiliateMachinePair.cs
# Expected: anonymisation step found before SyncPersonalToGlobalAsync sends payload

# Test: sync does not include personal text
dotnet test C:/Projects/llm_mvp_core/Tests/LlmFromScratch.Tests.csproj \
  --filter "AffiliateDomainTests" 2>&1 | tail -5
# Expected: test that asserts sync payload contains zero raw user content
```

Additional Achoti Gate B:

```bash
# Guide exists and is linked
grep -n "אחותי.zip\\|ACHOTI-ENTREPRENEURSHIP" \
  C:/Projects/llm_mvp_core/docs/plans/PARALLEL-DOMAIN-TRAINING-ROADMAP-v1.md \
  C:/Projects/llm_mvp_core/docs/plans/PARALLEL-TEACHING-AND-EVALUATION-GUIDANCE-v1.md

# First-party source checks, once Direction 3 implementation exists
dotnet test C:/Projects/llm_mvp_core/Tests/LlmFromScratch.Tests.csproj \
  --filter "AchotiPairValidationTests|AchotiEntrepreneurshipHeldOutTests" 2>&1 | tail -5
# Expected: 50 train rows + 50 held-out rows, can_train=false on held-out prompts,
# no audio/video/image rows without transcript/OCR review, and runtime held-out
# prompts grounded in Achoti source refs.
```

---

## Direction 4 — Design to Code (Figma → Angular / React / Vue / React Native)

### Purpose

Convert design artefacts into client-side code for the XIIGen plugin ecosystem.
Uses the existing codebase analysis from Direction 2 to map Figma screens against
existing components before generating new code.

### What already exists

`C:\Users\lubao\Documents\xiigen\llm mvp\extensions\PLAN-SMART-CODE-WIKI-PHASE1-ALL-18-PLATFORMS-v1.md` (57 KB) — per-platform plans.
`xiigen-planning/extensions research` — research notes on design conversion.

No DPO training data exists yet.

### New project

`30.DesignConversionDomain`
(Note: 29 is already `29.SemanticCompiler` — project number 30 chosen to avoid conflict.)

References: `00.Core`, `42.ProjectWikiDomain` (for existing-code analysis)

### Core changes required

```csharp
// Domain marker
public sealed class DesignConversionDomain { }

// Input record
public sealed record DesignConversionRequest(
    string DesignArtifactPath,
    string TargetFramework,       // e.g. "angular", "react", "vue", "react-native"
    string ActiveProjectRoot,
    string[] LinkedProjectRoots);

// Flow
DesignConversionFlow : ILogicalFlow<DesignConversionRequest>

// Machine
DesignConversionMachine : ILearningMachine<DesignConversionDomain>
```

`DesignConversionFlow` runs `DesignAnalysisUnit` (parse design),
`ExistingCodeMapUnit` (reuse from Direction 2 CodeWikiMachine),
`GapAnalysisUnit` (new vs redesign vs skin-only), `ConversionPlanUnit`
(per-screen per-component plan), `CodeGenerationUnit` (bootstrap: template;
trained: framework-aware generation).

DPO training data strategy: bootstrap pairs from existing Angular/React projects
in `C:\Projects\GenieDynamicApp`, `C:\Projects\bizguide-client`. Pairs map
component structure → correct HTML/CSS output. Added by a new
`DesignConversionDpoGenerator`.

### Gate B Direction 4

```bash
# Project scaffold
ls C:/Projects/llm_mvp_core/30.DesignConversionDomain/
# Expected: DesignConversionDomain.csproj, DesignConversionFlow.cs found

# Flow returns structured plan
dotnet test C:/Projects/llm_mvp_core/Tests/LlmFromScratch.Tests.csproj \
  --filter "DesignConversionDomainTests" 2>&1 | tail -5
# Expected: test verifies DesignConversionFlow returns a result with
# at least 3 stages and non-empty gap analysis
```

---

## Direction 5 — Local Domain Library (Affiliate Domain Sharing)

### Purpose

Any affiliate can train `ILearningMachine<TDomain>` on their professional specialisation,
test it, and publish it to the shared network. Other affiliates can import, upgrade, and
embed domain models in XIIGen products on their server or client.

### What already exists

`25.ModelServer` with:
- `IModelRepository` (upload checkpoint manifest, load weights)
- `IRemoteUnitLocator` (locate by unit id)
- `RemoteUnitManifest` (unit id, role, domain tag, version, endpoint,
  manifest hash, training branch/run id, dataset hash, model config, LoRA config,
  tokenizer path, registration time)
- `LocalModelRepository`
- `InMemoryRemoteUnitLocator`

The current checkpoint and manifest path supports unit transfer and version tags.
It does **not** yet support full learning-machine publish/find/list, upgrade policy,
or rollback pointers. Those are Direction 5 deliverables.

### Core changes required

No new subproject is needed. Direction 5 extends `25.ModelServer`:

```csharp
// New contract in 25.ModelServer/Contracts
public interface ILearningMachineRepository
{
    Task<OperationResult<string>>              PublishAsync(LearningMachineExport export);
    Task<OperationResult<LearningMachineExport>> FindAsync(string machineId, string version);
    Task<OperationResult<LearningMachineExport[]>> ListCompatibleAsync(string domainTag);
    Task<OperationResult<bool>>               IsCompatibleAsync(
        LearningMachineExport from, LearningMachineExport to);
}
```

`LearningMachineExport` (from TD3 in `00.Core`) is the transfer format.
`RemoteUnitManifest.upgrade_policy` and `RemoteUnitManifest.rollback_ref` do not
exist yet. Direction 5 must add their equivalent fields before upgrade/rollback
safety can be claimed.

Upgrade probe and rollback probe are required before any `ImportAsync()` call
succeeds on a version that differs from the currently installed version.
These probes are the same gate pattern as the existing runner reports in
`25.ModelServer`.

### Gate B Direction 5

```bash
# New contract present
grep -n "ILearningMachineRepository" \
  C:/Projects/llm_mvp_core/25.ModelServer/Contracts/
# Expected: interface declaration found

# End-to-end cycle test
dotnet test C:/Projects/llm_mvp_core/Tests/LlmFromScratch.Tests.csproj \
  --filter "LearningMachineRepositoryTests" 2>&1 | tail -5
# Expected: one test covers the full cycle:
# ExportAsync → PublishAsync → FindAsync → ImportAsync → GetOrchestratorAsync
# All five steps return IsSuccess = true
```

---

## Dependency map

```
TD3 (ILearningMachine in 00.Core)
  │
  ├── Direction 1: 27.TranslationDomain
  │     (can start immediately after TD3)
  │
  ├── TD1: Bootstrap unit wrappers in 42.ProjectWikiDomain
  │     │
  │     └── TD2: ProjectWikiOrchestrationFlow in 15.OrchestratorModel
  │           │
  │           └── Direction 2: CodeWikiMachine + WikiTraceAggregator training
  │                 │
  │                 └── Direction 4: 30.DesignConversionDomain
  │                       (reuses CodeWikiMachine analysis)
  │
  ├── Direction 3: 31.AffiliateDomain
  │     (requires TD3 only — no dependency on TD1 or TD2)
  │
  └── Direction 5: 25.ModelServer extension
        (requires TD3 for LearningMachineExport type — no other dependencies)
```

---

## Subproject number register

| Number | Name | Status |
|---|---|---|
| 25 | ModelServer | In Progress (existing) |
| 26 | TrainableRouter | In Progress (existing) |
| 27 | TranslationDomain | Planned — Direction 1 |
| 28 | SemanticNlp | **OCCUPIED** — do not use for any new domain project |
| 29 | SemanticCompiler | Complete (existing — do not reuse this number) |
| 30 | DesignConversionDomain | Planned — Direction 4 |
| 31 | AffiliateDomain | Planned — Direction 3 (number corrected from 28 — 28 is occupied by SemanticNlp) |
