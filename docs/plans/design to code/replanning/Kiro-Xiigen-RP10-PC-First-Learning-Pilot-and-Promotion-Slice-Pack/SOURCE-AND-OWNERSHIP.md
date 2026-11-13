# RP10 source traversal & ownership — no S04-only tunnel

## Live namespaces and authority
- **P**: `lubochka/xiigen-planning@llm_plan3`; **D**: `llm mvp/design to code session`; **K**: `lubochka/llm_mvp_core@codex/dpo-implementation`.
- **G**: `llm mvp/dpo legal build`; **F**: `llm mvp/llm dynamic flow`; **V**: `llm mvp/visual studio & visual studio code continue` (separate roots, not D subdirectories).
- Must inspect D/`original task.md`, D/`Алгоритм создания ДПО.md`, D/`goal of the round.md`, D/`original task.md` and relevant D/`*WorkLeft*` (notably `02-teaching-chinese-model`, `MECH-01`, `RESEARCH-03`, `INTEGRATION-01`, `RAG-00`, `RAG-MASTER-INDEX`, `TASK-PROMPTS-TRAINING`), D/old S00–S10, D/`XIIGEN-human-slice-map-2026-09-24.md`, independent G/F/V primary task files.
- D/replanning/`previous rounds thinking results.md`, `PARALLEL-R01-R03-JOIN.md`, `PARALLEL-R01-R25-CONTRACT.md`, `results/R01–R25/FINDINGS.md`, own `SOURCES.csv`/`DECISIONS.md`/`RUN-LOG.md` **where relevant**; `R01-R25-TO-CODE-SLICES-STRATEGY.md`, `RECOVERED-DESIGN-CONTRACT.md`, `R18-FIRST-LEARNING-IMPLEMENTATION-WORK-ORDER.md`, `R22-CAUSAL-EXPERIMENT-CATALOG.md`, `R23-PC-STATE-FEEDBACK-LIFECYCLE.md`, `R24-DTC-IDE-ORDINARY-ACCEPTANCE.md`, `R25-ASTRA-REVIEW-PACKET.md`.
- RAG MD + NDJSON is **locator only**; open cited PRIMARY and record actual path/span/blob. An `ls` listing, result self-report, non-existent path, old evidence or class name ≠ current code and call graph.

## Real K candidate entrypoints (13.10.26 documented inspection, must refresh)
| Concern | Candidate current source (not runtime proof) | Owning slice | RP10 acceptance observation |
|---|---|---|---|
| exact DPO material | `18.CapabilityTraining/LlmIngredientLogicalUnits.cs`; candidate `Compact(Prompt/Chosen/Rejected)` risk | RP01 | exact approved bytes survive response-only encoding |
| DPO policy/reference | `11.DPO/DpoTrainer.cs` | RP01 | actual identified Student/adapter update, before/after, frozen reference |
| kNN consumption | `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs`; prior read notes `hits[0]` | RP02 | actual local call consumes typed multiple eligible neighbors |
| typed logical contract | `00.Core/LogicalContracts.cs` (`ILogicalUnit<TInput,TOutput>` historical read) | RP03 | no unverified compiled `IQueryable` claim; real association |
| local design conversion seam | `25.ModelServer/DesignConversion/DesignConversionModelServerLocalCallerAdapter.cs` | RP06 | local adapter NOT automatically external host |
| extension boundary | `43.SmartSkillsDomain/XiigenExtensionApiService.cs` | RP06 | API OK ≠ native application adoption |
| feedback/update/state | locate **actual** K owner by current repo search (old cited `ContinuousLearning/Feedback/FeedbackRouter.cs` NOT_FOUND in earlier RP09 read) | RP05 | owner, consumer and persistent journal verified before any change |

These are **candidates only**. RP10.00B must chase the actual production request→entrypoint→mapper→retrieval→Unit/Flow→Orchestrator/Graph→Student→state reader→user output/feedback. If paths diverge, mark `NOT_WIRED`, identify minimal owner-based adapter and falsifier; do not invent implementation on a guessed path.

## Reuse decision required for every proposed change
`REUSE_NO_CHANGE`: current callable owner and actual ordinary-use proof; `MINIMAL_ADAPTER`: one identified missing producer→consumer link with full source/callers; `OWNER_FIX_PACKET`: send precise bounded repair to RP01–RP09 / historic Sxx; `NEEDS_SOURCE`: unresolved primary or live call graph; `NEEDS_GRANT`: rights/effects absent; `REJECT`: no lawful use or fails invariants.

## Source receipt contract
`id | requirement/original | root | repo/ref/blob | primary file:anchor | RAG locator:MD/NDJSON | read_depth [INDEX_ONLY, PRIMARY, CODE, CALL, ACTUAL_RUN] | producer→consumer | state owner | RP slice and task | decision/status | falsifier | resource/rights | next owner`.
Keep old Rxx status untouched. A R25 self-review cannot be re-labelled an actually separate Astra review. Pin HEAD again immediately before any code/effects.
