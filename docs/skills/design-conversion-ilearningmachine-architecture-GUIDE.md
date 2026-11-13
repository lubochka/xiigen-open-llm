# Design Conversion ILearningMachine Architecture Guide

## Luba Why / Loophole Repair-First Rule

When Luba asks "why", "почему", challenges an answer, or says a failure came
from a loophole in plans, guides, skills, agent files, prompts, or governance
rules, this is an answer-first repair trigger, not a normal status question and
not permission to continue product work first.

Required order:
1. Answer Luba's why/loophole question directly.
2. Classify the previous behavior or claim as verified, overclaim, false,
   misread, or governance gap.
3. Identify the exact missing, weak, stale, or contradictory rule that allowed
   the failure.
4. Repair the governing plan, guide, skill, agent file, or start prompt first.
5. Verify the repair with direct readback, marker search, diff check, or another
   evidence-bound check.
6. Only after the repair is verified, resume the nearest already-authorized
   product task.

Do not substitute apology, status, a plan-quality note, or "I will continue" for
this repair. If no file repair is needed, state the exact evidence proving that
before continuing.

## Luba Global Plan-Quality / Skill-Stack Rule

This file inherits the global project rule: when Luba asks which plan is better,
judge by Luba's criteria: human clarity, readable architecture, architectural
reliability, evidence honesty, requirement fidelity, execution-control quality,
and low overclaim risk. Newer status tables, file counts, ledgers, count
freshness, or claimed pass counts are delta evidence only; use valid deltas to
patch the stronger plan instead of treating bookkeeping freshness as plan
superiority.

No executor session may speak as the active product executor until SK-INTENT,
SK-PLAN, SK-REVIEW, and SK-ARCH when applicable are loaded for the current task.
If the stack is not loaded, say "not loaded yet" and load it before plan
comparison, plan choice, status, executor work order, or implementation.

Этот guide задает архитектурную рамку для будущего `DesignConversionDomain`: как
поместить дизайн-в-код обучение в иерархию `ILearningMachine<TDomain>` и не
перепутать обучаемую власть с deterministic QA helpers.

## When to use / Когда использовать

Используй этот guide, когда план, обзор или work order касается
`DesignConversionMachine`, преобразования Figma/Canva design source в
HTML (HyperText Markup Language) / CSS (Cascading Style Sheets), визуального
сравнения результата или обучения на DPO (Direct Preference Optimization)
парах для дизайна.

Он обязателен до написания Деталей 8a, если там появляется хотя бы один из
этих элементов: `DesignConversionDomain`, `DesignConversionMachine`,
`ILearningMachine`, `ILogicalOrchestrator`, `ILogicalFlow`, `ILogicalUnit`,
`ILearningMachinePair`, visual diff, render QA, checkpoint, promotion или
client API (Application Programming Interface) для Figma/Canva.

## Evidence-Bound Architecture Claims

Architecture artifacts must not overclaim readiness, review depth, or
sub-agent evidence. A design row, ledger row, generated checklist, or summary is
not a review/fix cycle. If the architecture says "N reviews", "N cycles",
"verified", "ready", or "sub-agent reviewed", it must name the independent
evidence records, reviewed scope, reviewer/sub-agent id when applicable,
finding/no-fix result, repair action, and recheck evidence.

If evidence is partial, the artifact must downgrade the claim before presenting
it to Luba: `planned`, `ledgered`, `designed`, `not verified`, or `verified only
for <narrow scope>`.

## Inputs / Входы

- Current authority quote from Luba and bounded work order.
- Stage 1a packet as evidence only: целевой домен `DesignConversionDomain`;
  нужен `ILearningMachine` / `ILearningMachinePair` lifecycle; gaps include DPO
  schema, visual-diff taxonomy, negative DPO, render QA, tier policy, exact
  Figma/Canva payload contract, promotion/upload workflow.
- Locked architecture decisions:
  - A-LM-01: `ILearningMachine<TDomain>` sits above `ILogicalOrchestrator`.
  - A-LM-02: `Train*` methods live only on `ILearningMachine`.
  - A-LM-05: `30.DesignConversionDomain` is the design-to-code domain number.
  - A-LM-12: only `ILearningMachine`, `ILogicalOrchestrator`, `ILogicalFlow`,
    `ILogicalUnit`, and `ILearningMachinePair` are allowed trainable
    architecture levels.
- Existing model lifecycle pattern from local model lifecycle guidance:
  checkpoint, manifest, export/import, fresh-load, continue-training, rollback,
  and common/adaptive split.

## Outputs/artifacts / Выходы и артефакты

Future architecture artifacts must name these planned components explicitly:

- `DesignConversionDomain`: domain marker for design-to-code training.
- `DesignConversionMachine : ILearningMachine<DesignConversionDomain>`:
  the only owner of training, evaluation, export/import, and checkpoint
  promotion.
- `DesignConversionOrchestrator : ILogicalOrchestrator`: runtime router for
  a design conversion request after a trained or honest untrained state is
  loaded.
- `DesignSourceIntakeFlow : ILogicalFlow<DesignSourceIntakeRequest>`: parses
  Figma/Canva/source payload into a typed design tree and provenance bundle.
- `HtmlCssCandidateGenerationFlow : ILogicalFlow<HtmlCssCandidateRequest>`:
  produces candidate HTML/CSS output from the normalized design source.
- `RenderQaFlow : ILogicalFlow<RenderQaRequest>`: server-side render,
  screenshot capture, and artifact packaging. This is QA/eval/internal, not
  trained final authority.
- `VisualDiffEvaluationFlow : ILogicalFlow<VisualDiffRequest>`: computes
  taxonomy-based visual evidence. This is QA/eval/internal, not trained final
  authority.
- `DesignDpoAuthoringFlow : ILogicalFlow<DesignDpoAuthoringRequest>`: turns
  approved visual verdicts and corrections into chosen/rejected/quarantine DPO
  rows.
- Candidate `ILogicalUnit` examples inside flows:
  `SourceTreeNormalizerUnit`, `LayoutIntentUnit`, `StyleTokenUnit`,
  `ResponsiveBreakpointUnit`, `AssetMappingUnit`, `AccessibilityHintUnit`,
  `RenderArtifactCollectorUnit`, `VisualMismatchClassifierUnit`,
  `DpoRowValidatorUnit`.
- Optional `ILearningMachinePair<DesignConversionPersonalDomain,
  DesignConversionGlobalDomain>` when common repo learning and user-adaptive
  learning both exist.

## Required gates / Обязательные gates

- Training location gate: `TrainBatchAsync`, `EvaluateAsync`, `ExportAsync`,
  `ImportAsync`, and checkpoint mutation belong only to
  `DesignConversionMachine`.
- Runtime separation gate: `ILogicalUnit`, `ILogicalFlow`, and
  `ILogicalOrchestrator` may execute inference and QA routes, but they do not
  own training methods.
- Deterministic boundary gate: renderers, screenshot capture, pixel diff,
  DOM (Document Object Model) checks, color distance, bounding boxes,
  breakpoint probes, and visual-diff taxonomies are QA/eval/internal evidence.
  They may generate training signals or block promotion, but they are not
  trained final authority.
- Lifecycle gate: any trained claim must have checkpoint, manifest,
  export/import, locator/server upload path, fresh-load probe,
  continue-training probe, held-out evaluation, and rollback path.
- Pair gate: if user-specific adaptation is required, the plan must use
  `ILearningMachinePair` and name common/global vs personal/adaptive storage.
- Thin-client gate: Figma/Canva clients send payloads and receive artifacts;
  core conversion, training, DPO authoring, and promotion stay in
  `llm_mvp_core`.
- No-hidden-determinism gate: a deterministic visual rule may be a guard,
  feature extractor, QA evaluator, or bootstrap helper only when labeled as
  such and excluded from final trainable authority by ablation.

## Forbidden substitutes / Запрещенные подмены

- A visual-diff score, screenshot matcher, CSS selector, DOM rule, regex, or
  source-path lookup cannot be called `DesignConversionMachine`.
- A Figma/Canva plugin, browser script, or external client cannot own DPO
  training or model checkpoint logic.
- A generated HTML/CSS fixture cannot be evidence that the trained model
  learned unless held-out eval, ablation, fresh-load, and continue-training
  probes pass.
- A server render pass cannot silently replace human-approved visual verdicts
  for DPO rows.
- A common/global checkpoint cannot include private user source trees,
  screenshots, file paths, project identifiers, or unapproved proprietary
  design data.
- A personal/adaptive leg cannot be silently overwritten by a global promotion.

## How it feeds the future plan / Как это питает будущий план

Детали 8a должны import this guide as the architecture guardrail. The plan must
map every planned design conversion capability to one allowed hierarchy level,
name which deterministic pieces are QA/eval/internal only, and add lifecycle
work orders for checkpoint, manifest, export/import, locator/server upload,
promotion, rollback, fresh-load, continue-training, held-out eval, and
ablation.

If Детали 8a names a scorer, visual diff, renderer, taxonomy, or client API as
the final trained mechanism, this guide makes the plan fail until the mechanism
is remapped under `DesignConversionMachine` or downgraded to QA/eval/internal.
