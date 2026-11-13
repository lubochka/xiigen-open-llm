# S26 source, RAG and ownership map — no S04-only narrowing
Aliases: P= `lubochka/xiigen-planning@llm_plan3`; D=P/`llm mvp/design to code session`; R=D/`replanning`; G=P/`llm mvp/dpo legal build`; F=P/`llm mvp/llm dynamic flow`; V=P/`llm mvp/visual studio & visual studio code continue`; K=`lubochka/llm_mvp_core@codex/dpo-implementation`. Pin exact commits/blobs again at EACH execution. `R01–R25` are existing separate researchers; this pack never asserts all their primary sources were fully read.

## Direct entry and semantic route
- [original four-role goal](../../original%20task.md), [original DPO design](../../Алгоритм%20создания%20ДПО.md), [goal](../../goal%20of%20the%20round.md), [task prompt](../../task%20prompt.md), [WorkLeft PROMPTS](../../09.09.26WorkLeft-PROMPTS.md), [WorkLeft model role](../../09.09.26WorkLeft-02-teaching-chinese-model.md), [WorkLeft mechanism](../../09.09.26WorkLeft-MECH-01-mechanism-validation.md), [WorkLeft training](../../09.09.26WorkLeft-TASK-PROMPTS-TRAINING.md), [WorkLeft RAG master](../../09.09.26WorkLeft-RAG-MASTER-INDEX.md), [RAG usage](../../09.09.26WorkLeft-RAG-00-usage-guide.md) and question-linked RAG-01 DPO, RAG-02 DTC, RAG-04 IDE incl NDJSON; **locate and open original G/F/V primary** according to `source.file` in RAG, not by assuming D root.
- [previous rounds thinking](../previous%20rounds%20thinking%20results.md) first status correction, [R01-R03 JOIN](../PARALLEL-R01-R03-JOIN.md), [R25 exact candidate](../R25-ASTRA-REVIEW-PACKET.md), [R21 seven domain laws](../results/R21/FINDINGS.md), [R22 experiments](../R22-CAUSAL-EXPERIMENT-CATALOG.md), [R23 state/feedback](../R23-PC-STATE-FEEDBACK-LIFECYCLE.md), [R24 host](../R24-DTC-IDE-ORDINARY-ACCEPTANCE.md).
- K actual focused entrypoints to re-read IN FULL only when touched: `14.ContinuousLearning/Feedback/FeedbackRouter.cs` (generic EDITED stores UserCorrection chosen and ModelResponse rejected; **NOT** S04 Chinese two-sided canonical teacher), `14.ContinuousLearning/Pipeline/ContinuousLearningPipeline.cs`, `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs` (search k vs actual first-hit consumer risk), `18.CapabilityTraining/LlmIngredientLogicalUnits.cs` (Compact before encode risk), `11.DPO/DpoTrainer.cs`, `11.DPO/Models/DpoBatch.cs`, `14.ContinuousLearning/Training/DpoTrainingExecution.cs` and actual selected Student/adapter/cold caller. Existing code evidence is narrower than a proven all-state revocation feature.
- F's **DECISIONS-LOCKED** (current direct primary must be reopened) protects D-04 taskIntent/subject distinction, D-05 project LoRA, D-06 replay historical conflict vs WorkLeft one-pair/no-repeat, D-07 StackDetector first, D-08 independent families + stratification, D-10 BM25+kNN, D-11 internal TensorCore, D-14 PC/idle/internal first, D-15 arbiter. RAG sources are navigators, not substitute authorities.

## Existing owners: route internals to them
| Layer | Existing RP/S owner | S26's only question |
|---|---|---|
| Feedback/consent/transaction/cold full-state | RP05, RP10, S23 | can already-admitted evidence be located and safely quarantined/rolled back? |
| CodeDocumentation exact DPO and LoRA | RP01/R18 | how to prove affected parent/trial without reauthoring existing learner? |
| Subject/multi-neighbor/semantic kNN | RP02, RP18 | does a withdrawn source still influence a normal answer? |
| Typed Unit many→many/Flow | RP03 | how to invalidate one learned association without breaking OutputRef identity? |
| Bounded async/Graph/path credit/compaction | RP04, RP11–RP13, RP16 | can ongoing/cached unlawful route be revalidated before use? |
| Domain law drift / open-world / interference | S17/RP14/RP15, S19, S22 | does evidence correction cross their compatible version boundaries? |
| Independent law/PC exam & native user host | RP09, RP10, RP06/R24, historical S08–S10 | was safe correction actually consumed and useful? |
| Meta-learning Builder | RP07 | can future lawful weakness be handed over, not auto-promoted to DPO? |

For any observed seam fill `original_claim | primary path/blob/lines | visited_depth | caller | mutable owner | recipient | ordinary consumer | effect | existing equivalent | missing adapter | experiment falsifier | rights | status`.
**This publication has NOT performed a recursive full G/F/V/K code survey.** That obligation is explicitly assigned as bounded S26.00A and scope-specific later tasks, never claimed completed because files were listed.
