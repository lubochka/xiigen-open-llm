# XIIGen Export 1000-Case Learning Set Generation Guidance

Version: 1.0  
Date: 2026-05-06  
Extends: `XIIGEN-EXPORT-TEACHING-ADDENDUM-v2.md`  
Applies to: Claude/XIIGen session exports in `C:\Users\lubao\Documents\xiigen\old\`

---

## 1. Purpose

This guidance describes how to generate a 1000-case XIIGen teaching package from real session data.

The goal is not to invent coverage. The goal is to take real human prompts from old XIIGen/software-development conversations, preserve their rough wording, map them to the current orchestrator topology, and produce learning rows that can be used for compiler and router teaching runs.

The first accepted package generated under this guidance is:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\claude-export\batch-0000\xiigen-session-1000-oldest-first
```

Compressed copy:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\claude-export\batch-0000\xiigen-session-1000-oldest-first.zip
```

---

## 2. Non-Negotiable Rules

### Rule 1 - Real Human Prompt Only

The A-side must be a real human message from the session export.

Allowed:

```text
Raw human messages asking for a feature, fix, plan, review, test, architecture decision, skill, or documentation.
```

Not allowed:

```text
Assistant summaries.
Tool output.
Architecture-document prose rewritten as a prompt.
Synthetic prompts created only to fill a missing sub-phase.
Session-control messages such as "continue", "load session", or "please proceed".
```

### Rule 2 - Oldest First

Rows are selected in chronological order. The first package intentionally starts with older conversations because those sessions produced early documentation and skill-library work based on older code, which is a high-value modern use case.

Sort order:

```text
conversation.created_at ascending
  -> message.created_at ascending
```

If two export zips overlap, de-duplicate prompts by normalized human message text.

### Rule 3 - Stream The Export

`conversations.json` can be more than 1 GB uncompressed. Do not load it fully into memory.

Allowed:

```text
Zip entry stream -> JsonSerializer.DeserializeAsyncEnumerable<Conversation>
```

Not allowed:

```text
File.ReadAllText(conversations.json)
ConvertFrom-Json on the full file
json.loads(full_json)
one in-memory list containing all conversations/messages
```

### Rule 4 - Match The Current Engine Topology

The learning set must use the topology that the current engine knows how to route.

Current sub-phase authority:

```text
C:\Projects\llm_mvp_core\21.AgentInfrastructure\Logical\DevelopmentLifecycleTopologySeeder.cs
```

Do not generate rows for older external-source topology IDs that are not registered in the current code.

### Rule 5 - Compiler Predicates Must Match The Current Compiler

Compiler row B-sides must use predicate IDs from:

```text
C:\Projects\llm_mvp_core\21.AgentInfrastructure\Logical\PromptRoutingInputCompilerUnit.cs
```

Current predicate IDs:

```text
SECURITY
AUTHORIZATION
API_CONTRACT
ARCHITECTURE
IMPLEMENT
TEST
QA
UI
DOCUMENT
LEARNING
```

If a real prompt does not trigger one of these predicates, skip it for the first 1000-case package and count it as `no_engine_predicate`.

---

## 3. Output Shape

Write the package here:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\claude-export\batch-0000\{package-id}
```

For the first 1000-case run:

```text
package-id = xiigen-session-1000-oldest-first
```

Required files:

```text
MANIFEST.md
cycle-map.yaml
conversation-index.jsonl
rows.jsonl
layer-0-a\layer-0a-compiler-pairs-001.md ... layer-0a-compiler-pairs-020.md
layer-0-b\layer-0b-router-pairs-001.md ... layer-0b-router-pairs-020.md
held-out\held-out-rephrases.md
```

Optional but recommended:

```text
tools\GenerateXiigenSession1000.cs
GUIDANCE.md
```

The package should also be zipped after generation:

```text
{package-id}.zip
```

---

## 4. Row Layers

### Layer 0-A - Compiler Pair

Contract:

```text
PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
```

A-side:

```yaml
run_id: "{row id}"
scenario_id: "adaptive-logical-orchestration"
user_request: "{raw human message}"
flags:
  UseLearnedUnitWhenAvailable: true
  UseKnowledgeEdgeDetection: true
```

B-side:

```yaml
DocumentId: "{row id}-prompt"
SourcePath: "claude-export/batch-0000/{conversation_uuid}/messages/{message_uuid}"
SourceTier: "claude_export_session"
LicenseOrAuthority: "first_party_private"
DocumentType: "compiled_prompt"
DocumentTitle: "{target sub-phase name}"
CompiledText: "{same raw human message}"
CompiledPredicateIds:
  - "{predicate id}"
SourceStatus: "ready"
KnownFailureMode: null
Confidence: 0.92
```

### Layer 0-B - Router Pair

Contract:

```text
MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
```

A-side:

```yaml
A_document_id: "{row id}-prompt"
A_compiled_predicate_ids:
  - "{predicate id}"
```

B-side:

```yaml
routing_decisions:
  - DocumentId: "{row id}-prompt"
    SourceStatus: "ready"
    PredictedPhaseId: "{phase id}"
    PredictedSubPhaseId: "{sub-phase id}"
    Confidence: 0.92
    NearestWrongSubPhaseScore: 0.00
    EvidencePredicates:
      - "{predicate id}"
    KnowledgeEdge: false
    FailureClassification: "source_classified_by_multi_sub_phase_router"
```

Layer 0-B may include up to eight sub-phase routing decisions. This mirrors the branch cap used in teaching runs.

---

## 5. Prompt Filtering

Accept a human message when it contains real software-development intent and activates at least one current compiler predicate.

High-signal include terms:

```text
xiigen
figma
code
angular
react
nestjs
typescript
api
plugin
skill
claude code
orchestrator
flow
architecture
build
test
documentation
implementation
frontend
backend
pipeline
mcp
database
redis
elasticsearch
auth
security
review
plan
runner
package
zip
manifest
```

Reject:

```text
setup-only commands
tool-output transcripts
assistant answers pasted back wholesale
unrelated language-learning/form-filling/music conversations
messages shorter than 18 characters
messages longer than 5000 characters for the first pass
duplicates after normalized-text de-duplication
```

The long-message rejection is a first-pass safety rule. Long messages can be split into smaller teaching rows later, but should not be pushed into the first 1000-case compiler/router package raw.

---

## 6. Sub-Phase Selection

Use deterministic heuristic selection only to prepare the first learning set. The teaching run itself will measure whether the engine accepts or rejects the row.

Primary examples:

```text
architecture / topology / orchestrator / registry       -> 3.1
api / endpoint / contract / schema                      -> 3.3
security / auth / jwt / oauth / tenant                  -> 3.4
figma / ui / screen / frontend / client                 -> 4.2
implement / code / backend / service                    -> 4.1
refactor / cleanup / quality                            -> 4.4
test / verify / fixture                                 -> 5.1
regression / compatibility                              -> 5.4
qa / acceptance                                         -> 6.1
performance / cpu / ram / reliability                   -> 6.4
ci / build passing / commit / push                      -> 7.2
report / ledger / manifest / evidence                   -> 7.4
skill / teaching / learning set / corpus / model        -> 7.5
documentation / docs / spec / plan                      -> 2.1
```

When one real prompt naturally spans several areas, include several Layer 0-B routing decisions, capped at eight.

Important: if a prompt only weakly matches a sub-phase, prefer fewer routes. The learning set should teach precision, not lifecycle-wide over-routing.

---

## 7. Trace Requirements

Every row must preserve:

```yaml
conversation_trace:
  export_batch: "batch-0000"
  conversation_uuid: "{uuid}"
  conversation_name: "{name}"
  human_message_uuid: "{uuid}"
  human_message_index: 0
  message_created_at: "{timestamp}"
```

Also write:

```text
conversation-index.jsonl
rows.jsonl
```

These are the audit files used to trace a teaching result back to the original real session.

---

## 8. Validation Before Acceptance

The package is accepted only when all checks pass:

```text
[ ] 1000 rows in rows.jsonl.
[ ] 20 Layer 0-A compiler files when using 50 rows per file.
[ ] 20 Layer 0-B router files when using 50 rows per file.
[ ] 100 held-out seeds when using every 10th row.
[ ] All rows have a non-empty conversation_uuid.
[ ] All rows have a non-empty message_uuid.
[ ] All rows have at least one compiled predicate.
[ ] All primary sub-phase IDs exist in the current code topology.
[ ] No setup-only prompts are present.
[ ] MANIFEST.md records skipped counts and domain/sub-phase distribution.
[ ] cycle-map.yaml records date window and covered sub-phases.
[ ] Zip package exists beside the folder.
```

For the first generated package, the accepted validation numbers were:

```text
Rows produced: 1000
Conversations used: 457
Layer 0-A files: 20
Layer 0-B files: 20
Held-out seeds: 100
First row message: 2025-09-03T10:43:59.809174Z
Last row message: 2026-03-11T11:01:47.894878Z
```

---

## 9. How To Use The Package In Teaching

Use the package as a batch teaching source, not as isolated single-row demos.

Recommended run shape:

```text
1. Load rows.jsonl.
2. Group rows by domain label.
3. Chunk large domains into 20-30 rows per DPO batch.
4. Teach each domain batch with the cumulative registry and warm model cache enabled.
5. After each batch, run a regression window:
   - current batch representative prompts
   - 2-3 old prompts for the same domain
   - 2-3 old prompts from other domains
6. Record per-batch:
   - elapsed time
   - CPU percent
   - working-set delta
   - managed allocation delta
   - checkpoint bytes
   - domain
   - sub-phase coverage
   - accepted/resource-review/logic-review verdict
```

Do not treat a resource-review batch as logical failure. It means the teaching budget needs tuning or chunking.

Do treat a PASS with obviously wrong sub-phase routing as a failure. Routing correctness matters more than a green execution envelope.

---

## 10. What To Do If The Package Reveals A Problem

### Too Many Rows Route To 7.5

Meaning:

```text
The old sessions are rich in learning/skills/corpus language.
```

Action:

```text
Keep 7.5 rows, but make sure the router also carries architecture/code/test branches when the prompt asks for skills based on a codebase.
```

### Too Many Rows Route To One Broad Branch

Meaning:

```text
The heuristic selector is too broad, or the engine predicate lexicon is missing a sharper term.
```

Action:

```text
Do not invent rows. Add a precise predicate/anchor only when several real prompts prove the missing vocabulary.
```

### Large Domain Batch Is Slow

Meaning:

```text
DPO cost is scaling with batch size or accumulated optimizer state.
```

Action:

```text
Chunk that domain into 20-30 row batches. Keep cumulative weights. Keep the regression window.
```

### Setup Messages Slip In

Meaning:

```text
The filter is too permissive.
```

Action:

```text
Remove those rows, add the phrase to the setup-only rejection list, regenerate the package.
```

---

## 11. Executor Rule

The executor must report the package as a learning-set artifact, not as proof that the engine learned 1000 cases.

Correct wording:

```text
Generated and validated a 1000-case learning-set package from oldest real XIIGen session prompts.
```

Incorrect wording:

```text
The model has learned 1000 cases.
```

The model only learns after the package is passed through the cumulative teaching runner and the measured results are accepted.
