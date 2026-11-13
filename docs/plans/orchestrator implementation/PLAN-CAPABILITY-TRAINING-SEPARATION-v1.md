# PLAN-CAPABILITY-TRAINING-SEPARATION-v1

## Goal

Separate the learning execution system from the agent orchestration project.

`21.AgentInfrastructure` must remain the agent/orchestrator layer: it decides when training is needed and calls a training interface.

Training implementation must move out of project 21 into a dedicated project:

`18.CapabilityTraining`

This project owns DPO training, replay selection, checkpoint lifecycle, convergence handling, teaching text preparation, and trainer cache management.

---

## Architectural Decision

Use a new project: `18.CapabilityTraining`.

Do not put the implementation into `16.AutonomousLearning`.

Reason:

- `16.AutonomousLearning` owns learning conditions and governance signals:
  - idle detection
  - replay buffer primitive
  - trust scoring
  - autonomous learning gates
- `18.CapabilityTraining` owns learning execution:
  - capability trainer
  - DPO training unit
  - replay fusion ranking
  - teaching text compression
  - checkpoint save/load lifecycle
  - convergence loop
  - trainer cache
- `21.AgentInfrastructure` owns orchestration:
  - prompt routing
  - flow execution
  - topology registry
  - deciding when to invoke `ILogicalCapabilityTrainer`

This prevents project 21 from becoming both the agent runtime and the learning runtime.

---

## Target Boundaries

### Moves From 21 To 00.Core

Move shared training contracts into `00.Core`.

Files/contracts:

- `ILogicalCapabilityTrainer`
- `LogicalCapabilityTrainingRequest`
- `LogicalCapabilityTrainingResult`
- `LogicalDpoTrainingRequest`
- `LogicalDpoTrainingResult`
- Any small enum/record required by those contracts

Rules:

- Contracts must not reference project 21 types directly unless those types already live in `00.Core`.
- If a contract currently depends on a project-21-only type, move that dependent contract to `00.Core` too or replace it with a core abstraction.
- No training implementation moves to `00.Core`.

### Moves From 21 To 18.CapabilityTraining

Move all learning execution implementation into the new project.

Classes:

- `DefaultLogicalCapabilityTrainer`
- `LogicalDpoTrainingUnit`
- `ReplayFusionRanker`
- `LogicalTeachingTextCompressor`
- `AutonomousLearningReplayMixer`
- Convergence helper/loop code used by DPO training
- Checkpoint lifecycle helpers owned by capability training
- Trainer cache and manifest hydration logic

### Stays In 21.AgentInfrastructure

Project 21 keeps only orchestration integration.

It may:

- Resolve or receive `ILogicalCapabilityTrainer`
- Call `TrainAndRegisterAsync`
- Pass topology, branch results, compiled prompt, and metadata into the trainer
- Register generated units/flows after trainer returns
- Schedule learning from accepted feedback

It must not:

- Own DPO training logic
- Own replay ranking logic
- Own checkpoint writing/reading implementation
- Own teaching text compression
- Own trainer cache
- Instantiate `DpoTrainer` directly

---

## Dependency Direction

Final dependency direction:

```text
00.Core
   ↑
16.AutonomousLearning
   ↑
18.CapabilityTraining
   ↑
21.AgentInfrastructure
```

More precisely:

- `00.Core` references no project above it.
- `16.AutonomousLearning` may reference `00.Core`.
- `18.CapabilityTraining` may reference:
  - `00.Core`
  - `01.Tokenizer`
  - `03.TransformerModel`
  - `10.LoRA`
  - `11.DPO`
  - `16.AutonomousLearning`
  - `22.AgentAlgorithms`
  - `23.OssEquivalents` / semantic adapter project as needed
  - `29.SemanticCompiler`
- `21.AgentInfrastructure` may reference:
  - `00.Core`
  - `18.CapabilityTraining` only at composition/startup boundaries, or preferably receive `ILogicalCapabilityTrainer` through constructor injection.

Forbidden:

- `18.CapabilityTraining` must not reference `21.AgentInfrastructure`.
- `00.Core` must not reference `18` or `21`.
- `16.AutonomousLearning` must not reference `21`.

---

## Phase 0 — Baseline And Safety Gate

### Purpose

Establish the live suite count before moving files.

### Commands

```powershell
cd C:\Projects\llm_mvp_core
dotnet test Tests\LlmFromScratch.Tests.csproj 2>&1 | Select-Object -Last 8
git status --short
```

### Gate

- Suite must be green.
- Record live passing count.
- Do not use remembered floor values.

---

## Phase 1 — Move Training Contracts To 00.Core

### Work

Move training request/result/interface contracts from project 21 into `00.Core`.

Expected new or modified file:

- `00.Core/LogicalCapabilityTrainingContracts.cs`

Project 21 should import the contracts from `00.Core`.

### Tests

Add or update tests:

- `CapabilityTrainingContractPlacementTests`

Required assertions:

1. `typeof(ILogicalCapabilityTrainer).Assembly.GetName().Name == "Core"`
2. `typeof(LogicalCapabilityTrainingRequest).Assembly.GetName().Name == "Core"`
3. `typeof(LogicalDpoTrainingRequest).Assembly.GetName().Name == "Core"`
4. Project 21 no longer declares these contract types.

### Gate

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj 2>&1 | Select-Object -Last 8
```

Commit:

```text
Capability training separation phase 1 - contracts to core
```

---

## Phase 2 — Create 18.CapabilityTraining Project

### Work

Create:

- `18.CapabilityTraining/CapabilityTraining.csproj`

Add it to:

- `LlmFromScratch.sln`

Initial project references:

- `00.Core`
- `01.Tokenizer`
- `03.TransformerModel`
- `10.LoRA`
- `11.DPO`
- `16.AutonomousLearning`
- `22.AgentAlgorithms`
- semantic adapter/compiler projects needed by the moved trainer

### Tests

Add:

- `CapabilityTrainingProjectBoundaryTests`

Required assertions:

1. Project directory exists.
2. Project builds through the solution.
3. Project file does not reference `21.AgentInfrastructure`.
4. `21.AgentInfrastructure` still builds after contracts move.

### Gate

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj 2>&1 | Select-Object -Last 8
```

Commit:

```text
Capability training separation phase 2 - add project 18
```

---

## Phase 3 — Move DPO Training Unit

### Work

Move:

- `LogicalDpoTrainingUnit`

from project 21 to:

- `18.CapabilityTraining/LogicalDpoTrainingUnit.cs`

Keep the same public behavior:

- Uses `LogicalDpoTrainingRequest`
- Returns `LogicalDpoTrainingResult`
- Runs the DPO convergence loop
- Honors `MaxStepCount`
- Honors `ConvergenceWindowSteps`
- Reports real `DpoStepCount`

### Tests

Add or update:

- `LogicalDpoTrainingUnitPlacementTests`
- Existing DPO unit behavior tests

Required assertions:

1. `LogicalDpoTrainingUnit` assembly is `CapabilityTraining`.
2. DPO unit does not live in `AgentInfrastructure`.
3. `DpoStepCount` is greater than `1` when convergence does not stop immediately and max step count allows it.
4. `DpoStepCount` respects max step count.

### Gate

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj 2>&1 | Select-Object -Last 8
```

Commit:

```text
Capability training separation phase 3 - move DPO unit
```

---

## Phase 4 — Move Capability Trainer Implementation

### Work

Move:

- `DefaultLogicalCapabilityTrainer`

from project 21 to:

- `18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs`

Keep all behavior:

- compiled prompt training text
- tokenizer seeding from compiled prompt
- checkpoint creation
- parent manifest discovery
- trainer cache
- `ClearCache`
- trust scoring through `ITrustScorer`
- replay selection
- generated unit registration
- generated flow registration

Project 21 must only depend on `ILogicalCapabilityTrainer`.

### Tests

Add or update:

- `DefaultLogicalCapabilityTrainerPlacementTests`
- Existing trainer behavior tests

Required assertions:

1. `DefaultLogicalCapabilityTrainer` assembly is `CapabilityTraining`.
2. `DefaultLogicalCapabilityTrainer` does not live in `AgentInfrastructure`.
3. Existing trainer tests still pass.
4. `AdaptiveLogicalOrchestrator` can still call training through `ILogicalCapabilityTrainer`.

### Gate

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj 2>&1 | Select-Object -Last 8
```

Commit:

```text
Capability training separation phase 4 - move trainer implementation
```

---

## Phase 5 — Move Replay And Teaching Helpers

### Work

Move training-only helpers from project 21 into project 18:

- `ReplayFusionRanker`
- `LogicalTeachingTextCompressor`
- `AutonomousLearningReplayMixer`
- any private helper that exists only for capability training

Keep orchestrator-facing types in 21 only if they are genuine orchestration types.

### Tests

Add:

- `CapabilityTrainingHelperPlacementTests`

Required assertions:

1. `ReplayFusionRanker` assembly is `CapabilityTraining`.
2. `LogicalTeachingTextCompressor` assembly is `CapabilityTraining`.
3. `AutonomousLearningReplayMixer` assembly is `CapabilityTraining`.
4. Project 21 has no remaining direct references to `DpoTrainer`.
5. Project 21 has no remaining direct references to `ReplayFusionRanker`.

### Gate

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj 2>&1 | Select-Object -Last 8
```

Commit:

```text
Capability training separation phase 5 - move replay and teaching helpers
```

---

## Phase 6 — Agent Project Cleanup

### Work

Clean project 21 so it only orchestrates training.

`21.AgentInfrastructure` should contain:

- call sites to `ILogicalCapabilityTrainer`
- orchestration metadata wiring
- registry wiring for generated units/flows

`21.AgentInfrastructure` should not contain:

- DPO implementation
- DPO optimizer calls
- replay ranking
- checkpoint lifecycle implementation
- teaching text compression
- trainer cache dictionaries

### Tests

Add:

- `AgentInfrastructureTrainingBoundaryTests`

Required assertions:

1. No type in project 21 has name containing `DpoTraining`.
2. No type in project 21 has name `DefaultLogicalCapabilityTrainer`.
3. No type in project 21 has name `ReplayFusionRanker`.
4. No project-21 source file contains `new DpoTrainer`.
5. Orchestrator still exposes learning behavior through `ILogicalCapabilityTrainer`.

### Gate

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj 2>&1 | Select-Object -Last 8
```

Commit:

```text
Capability training separation phase 6 - clean agent boundary
```

---

## Phase 7 — Full Validation And Push

### Commands

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj 2>&1 | Select-Object -Last 8
git status --short
git log --oneline -8
```

### Required Result

- Full suite green.
- No generated experiment artifacts staged.
- All phase commits present.
- Push `main`.

### Final Commit/Push Rule

Every phase commit must be pushed after the final validation passes.

---

## Stop Conditions

Stop and report if:

- Moving contracts to `00.Core` creates a circular dependency.
- `18.CapabilityTraining` requires referencing `21.AgentInfrastructure`.
- Tests require weakening assertions to preserve behavior.
- DPO step count regresses to hardcoded `1`.
- The orchestrator can no longer invoke training through `ILogicalCapabilityTrainer`.

---

## Expected Outcome

After this plan:

- `21.AgentInfrastructure` is no longer the owner of the learning system.
- `18.CapabilityTraining` owns capability training execution.
- `16.AutonomousLearning` owns learning governance and support signals.
- `00.Core` owns shared contracts.
- Agent runtime and learning runtime can evolve independently.

