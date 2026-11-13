# Executor Prompt - Registry Persistence for Cumulative Teaching v2

Session type: EXECUTOR  
Goal: make teaching cumulative across rows and process restarts.  
Correction from v1: do not replay prior registrations. Load saved registry state directly.

---

## Human Meaning

Teaching has two parts.

The first part is the model artifact. The tokenizer, model checkpoint, KNN index, LoRA/DPO weights, and metrics live on disk. These should persist.

The second part is the registry. The registry tells the orchestrator which learned unit owns which role, domain, phase, sub-phase, and flow binding. If this registry is only an in-memory `List<T>`, the model may still exist on disk but the orchestrator cannot find it. That is what broke cumulative teaching.

The fix is not to teach again and not to replay old registration commands. The fix is to save the registry state after each row and load that state before the next row starts.

Correct sequence:

```text
row 1 starts
  load existing teaching session if present
  teach missing/updated capabilities
  save model artifacts
  save registry state

row 2 starts
  load the same registry state directly
  row 1 learned units are already visible
  teach row 2 additions/updates
  save updated registry state
```

If row 2 touches sub-phase `4.1` and row 1 already taught `4.1`, row 2 must see the row 1 unit before doing any new work.

---

## Non-Negotiable Rule

Do not implement this by replaying registration logic.

Forbidden:

```text
read snapshot
create empty registry
foreach snapshot row:
    call Register(...)
```

That is reconstruction, not loading. It allocates new registration identities, can change ordering, and can silently diverge from the original runtime state.

Required:

```text
read snapshot
deserialize descriptors and artifacts
load entries into registry with original descriptor metadata and registration order
set next registration counter to max loaded order
```

The registry itself owns the load operation.

---

## Code Changes Required

### 1. `InMemoryLogicalUnitRegistry`

Add a direct load method.

Behavior:

- Accept loaded entries containing `LogicalUnitDescriptor` plus executable unit object.
- Validate that the object implements `ILogicalUnit<TInput,TOutput>` matching the descriptor.
- Insert entries with the descriptor's original `RegistrationOrder`.
- Preserve exact trust state, role, domain tag, version, `IsLearned`, input type, and output type.
- Update `_nextRegistrationOrder` to the maximum loaded order.
- Support append mode for teaching sessions that first install base runtime services and then load learned rows.

This is not `Register`. It must not assign new registration order values.

### 2. `InMemoryLogicalFlowRegistry`

Add a direct load method.

Behavior:

- Accept loaded entries containing `LogicalFlowDescriptor` plus executable flow or flow factory.
- Validate that the object implements `ILogicalFlow<TInput>` or `ILogicalFlowFactory<TInput>` matching the descriptor.
- Insert entries with the descriptor's original `RegistrationOrder`.
- Preserve flow id, scenario id, version, phase id, sub-phase id, task type, domain tags, and input type.
- Update `_nextRegistrationOrder` to the maximum loaded order.

This is not `RegisterFactory`. It must not assign new registration order values.

### 3. Generated Capability Artifacts

When `DefaultLogicalCapabilityTrainer` trains a generated capability, save the artifacts needed to load the unit later:

- tokenizer file
- policy checkpoint file
- generated capability manifest
- DPO metric files

The manifest must contain:

- unit id, role, domain tag, version, trust state
- flow id, scenario id, phase id, sub-phase id, task type
- training text and output text used by the executable learned unit
- predicate ids and domain tags
- model config values
- LoRA config values
- paths to tokenizer and policy checkpoint

This manifest is not a command log. It is a load descriptor.

### 4. Teaching Registry Snapshot

Create a single JSON snapshot per teaching bundle.

The snapshot contains:

- unit descriptors
- flow descriptors
- generated capability manifests
- checkpoint paths
- bundle id
- row index
- creation timestamp

The snapshot is updated after every accepted or reviewable row so the next row starts from the same learned state.

### 5. Teaching Session Builder

Create a teaching-session builder used by teaching runners instead of direct `CreateDefault`.

Behavior:

- Build the standard base topology and fixed runtime flows.
- Load prior generated learned registry state from snapshot using registry load methods.
- Keep the same live topology, flow registry, unit registry, capability trainer, and orchestrator for the whole bundle.
- Save the snapshot after every row.

Important: inside a single bundle run, prefer one long-lived teaching session object. Disk load is for process restart and row continuation, not for resetting between rows.

---

## Acceptance Tests

### Test 1 - Unit Registry Direct Load Preserves Order

Create a learned unit descriptor with registration order `7`. Load it through the new load method. Assert:

- descriptor is visible through `ListDescriptors()`
- `RegistrationOrder == 7`
- `ResolveLatest` returns the loaded unit
- the next normal `Register` call receives an order greater than `7`

### Test 2 - Flow Registry Direct Load Preserves Order

Create a generated capability flow descriptor with registration order `9`. Load it through the new load method. Assert:

- descriptor is visible through `ListDescriptors()`
- `RegistrationOrder == 9`
- `ResolveBindingBySubPhase` resolves the loaded flow
- the next normal `Register` call receives an order greater than `9`

### Test 3 - Capability Trainer Writes Loadable Artifacts

Run `DefaultLogicalCapabilityTrainer` for one missing sub-phase. Assert:

- tokenizer file exists
- policy checkpoint exists
- generated capability manifest exists
- manifest names the same unit id registered in the unit registry
- manifest names the same flow id registered in the flow registry

### Test 4 - Snapshot Load Makes Prior Row Visible

Train row 1 for sub-phase `4.1`. Save snapshot. Create a new teaching session from the snapshot. Assert before row 2 training:

- unit registry already contains the row 1 learned unit
- flow registry already contains the row 1 generated flow
- a request routed to `4.1` selects the loaded learned unit without producing a fresh knowledge edge

This is the critical test. It proves cumulative teaching.

### Test 5 - Long-Lived Session Accumulates Rows

Open one teaching session. Run row 1 and save. Run row 2 without rebuilding the session. Assert:

- row 2 sees row 1 registry entries
- registry learned-unit count increases or an existing sub-phase receives a newer learned unit
- row 2 does not discard row 1

---

## What Not To Count As Success

These are not enough:

- row-level `KNOWLEDGE_EDGE -> PASS` when every row starts with fresh registries
- checkpoint file exists but no registry entry points to it
- snapshot file exists but loader calls `Register` for every prior entry
- row 2 re-teaches a sub-phase already taught in row 1 because the prior unit was invisible

The only accepted result is cumulative behavior: row N starts with rows 1 through N-1 already available.

---

## Current Implementation Note

`CreateDefault` may remain as a one-shot runtime constructor for tests and normal assistant use. Teaching runners must use the teaching-session builder instead.

Do not change `CreateDefault` into a persistence-aware method unless a later product decision explicitly asks for that. Teaching persistence is a teaching-session concern.
