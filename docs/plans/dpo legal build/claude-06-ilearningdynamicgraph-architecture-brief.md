---
artifact_type: ilearningdynamicgraph_architecture_brief
authority_source: latest unquoted Luba work order for DPO legal build library
status: populated_now_planning_only
last_updated: 2026-06-23
write_scope: C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\claude-*.md
---

# ILearningDynamicGraph Architecture Brief

## Architecture Intent

`ILearningDynamicGraph` is a trainable graph-policy layer for choosing, combining, and improving logical flows. It should learn from DPO rows which graph path, branch combination, evidence route, or final answer is better for a task, while preserving the existing project hierarchy.

## Component View

```mermaid
flowchart TD
  A["DynamicGraphMachine: ILearningMachine<DynamicGraphDomain>"] --> B["DynamicGraphTrainingStore"]
  A --> C["DynamicGraphCheckpointStore"]
  A --> D["DynamicGraphOrchestrator: ILogicalOrchestrator"]
  D --> E["Flow Router"]
  D --> F["Dependency Planner"]
  D --> G["Branch Executor"]
  G --> H["ILogicalFlow<string>"]
  H --> I["ILogicalUnit"]
  G --> J["Graph State Merger"]
  J --> K["Graph State"]
  B --> L["DPO Pair Builder"]
  L --> M["Judge and Provenance Gate"]
```

## Data Flow

```mermaid
sequenceDiagram
  participant User
  participant Machine as DynamicGraphMachine
  participant Orchestrator as DynamicGraphOrchestrator
  participant Flow as ILogicalFlow
  participant Unit as ILogicalUnit
  participant Store as CheckpointStore
  User->>Machine: request orchestrator
  Machine->>Store: load current graph policy
  Machine->>Orchestrator: provide trained state
  User->>Orchestrator: run request
  Orchestrator->>Orchestrator: plan branch graph
  Orchestrator->>Flow: execute selected flows
  Flow->>Unit: execute units
  Unit-->>Flow: unit result and optional transition
  Flow-->>Orchestrator: flow result
  Orchestrator-->>User: aggregate result and graph evidence
```

## Graph State

Graph state should include:

```text
node registry
edge registry
flow binding map
unit capability map
branch run history
transition signal history
evidence id map
dpo preference history
metric snapshots
checkpoint manifest
```

Graph state must distinguish deterministic infrastructure from learned policy. Closure and bridge detection are helper functions. DPO-trained path preference is learned policy.

## Routing Model

The routing model should accept:

```text
user request
compiled prompt features
available flow bindings
known graph transitions
context packet
source evidence ids
current graph policy checkpoint
```

It should return:

```text
selected flow ids
parallel branch levels
sequential dependency edges
bridge subphases
transition follow-up plan
confidence metrics
reason codes
```

## Async Experimentation

Independent branches should run as parallel candidates when dependency analysis permits. Dependent branches must wait for their prerequisites. The measurement layer compares complete branch combinations, not just isolated unit outputs.

```mermaid
flowchart LR
  A["Task"] --> B["Compile Context"]
  B --> C{"Dependency Planner"}
  C --> D["Parallel Level 1: Evidence Flow"]
  C --> E["Parallel Level 1: Risk Flow"]
  D --> F["Merge Context"]
  E --> F
  F --> G["Level 2: Critic Flow"]
  G --> H["Final Flow"]
```

## Interfaces In Prose

`DynamicGraphMachine` owns training, evaluation, export, import, and runtime orchestrator construction.

`DynamicGraphDpoPairBuilder` turns graph run comparisons into legal DPO rows with chosen and rejected paths.

`DynamicGraphLegalGate` validates source state, model or judge route, and provider terms status before rows enter train or held-out sets.

`DynamicGraphTrainingRunner` trains on source-disjoint rows and evaluates held-out rows without mutating during evaluation.

`DynamicGraphMeasurementRunner` reports DPO, graph, lifecycle, leakage, and resource metrics.

`DynamicGraphOrchestrator` runs selected graph branches through existing `ILogicalFlow` and `ILogicalUnit` contracts.

## Solved Subtasks

| subtask | decision |
|---|---|
| hierarchy | Use `ILearningMachine` above `ILogicalOrchestrator`, do not put training on unit or flow |
| runtime entry | Runtime requests enter through orchestrator returned by machine |
| DPO row shape | Use prompt, chosen, rejected plus graph provenance metadata |
| legal boundary | Closed APIs blocked for training until provider terms gate passes |
| branch execution | Use dependency levels for parallel and sequential branch planning |
| transition handling | Reuse transition signals and context packets |
| graph helpers | Use closure and bridge detection as deterministic helpers |
| learned claim | Require held-out metrics, fresh-load, and ablation |
| implementation state | Not authorized in this library work |

## Evidence-To-Architecture Mapping

| evidence path | architecture consequence |
|---|---|
| `00.Core\LearningMachineContracts.cs` | Machine owns training and returns orchestrator |
| `00.Core\LogicalContracts.cs` | Units, flows, orchestrator, transition signals, context packets are the runtime spine |
| `21.AgentInfrastructure\Logical\AdaptiveLogicalOrchestrator.cs` | Branch execution and transition follow-up already have precedent |
| `17.DynamicDomains\DynamicDomainGraph.cs` | Closure and bridge subphase expansion can support dynamic graph planning |
| `22.AgentAlgorithms\MultiAgent\DependencyGraph.cs` | Kahn-level dependency sorting supports parallel branch levels |
| `11.DPO\DpoTrainer.cs` | Reward margin and accuracy are existing DPO measurement primitives |
| `27.TranslationDomain\TranslationMachine.cs` | Existing machine lifecycle shows training history and checkpoint boundary concerns |
