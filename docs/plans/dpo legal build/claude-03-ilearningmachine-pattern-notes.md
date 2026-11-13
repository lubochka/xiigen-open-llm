---
artifact_type: ilearningmachine_pattern_notes
authority_source: latest unquoted Luba work order for DPO legal build library
status: populated_now_with_first_substantial_recon_pass
last_updated: 2026-06-30
recon_pass: B-21 (R12-recon-libs) — first substantial per-file pass, see §"RECON-библиотека: точные строки-якоря"
write_scope: C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\claude-*.md
---

# ILearningMachine Pattern Notes

## Core Hierarchy

The project pattern places trainable behavior above orchestration:

```text
ILearningMachine<TDomain>
  owns training, evaluation, export, import
  returns ILogicalOrchestrator for runtime execution
    ILogicalOrchestrator
      chooses and runs ILogicalFlow<string>
        ILogicalFlow<TInput>
          runs ILogicalUnit<TInput,TOutput> cells
```

This is explicit in `00.Core\LearningMachineContracts.cs`, where `ILearningMachine<TDomain>` sits above `ILogicalOrchestrator`. Training belongs on `ILearningMachine`, not on `ILogicalUnit` or `ILogicalFlow`.

## Evidence References

| path | key evidence |
|---|---|
| `C:\Projects\llm_mvp_core\00.Core\LearningMachineContracts.cs` | `LearningBatch`, `LearningTrainingItem`, `LearningEvaluationSet`, `LearningBatchResult`, `ILearningMachine`, `ILearningMachinePair` |
| `C:\Projects\llm_mvp_core\00.Core\LogicalContracts.cs` | `ILogicalUnit`, `ILogicalFlow`, `ILogicalOrchestrator`, transition signals, context packets |
| `C:\Projects\llm_mvp_core\21.AgentInfrastructure\Logical\AdaptiveLogicalOrchestrator.cs` | Branch execution, adaptive route decisions, transition follow-up, optional capability trainer |
| `C:\Projects\llm_mvp_core\21.AgentInfrastructure\Logical\UniversalNineNodeDevelopmentFlow.cs` | Flow as ordered reusable unit sequence with transition signals |
| `C:\Projects\llm_mvp_core\27.TranslationDomain\TranslationMachine.cs` | Existing `ILearningMachine<TranslationDomain>` lifecycle precedent |

## Pattern Roles

| role | responsibility | not responsible for |
|---|---|---|
| `ILearningMachine<TDomain>` | Train batches, evaluate held-out sets, export and import state, expose orchestrator | Per-request graph branch mechanics |
| `ILogicalOrchestrator` | Select and run one or more flows for a user request | Owning model training lifecycle |
| `ILogicalFlow<TInput>` | Execute a topology or ordered sequence of units and return a normalized flow result | Global checkpoint ownership |
| `ILogicalUnit<TInput,TOutput>` | Execute one bounded transformation and optionally emit transition signal | DPO training authority |
| `ILearningMachinePair<TPersonal,TGlobal>` | Separate personal and global machines with anonymized sync boundary | Raw user content sync |

## ILearningDynamicGraph Fit

`ILearningDynamicGraph` should be an `ILearningMachine<DynamicGraphDomain>` or a pair of personal and global graph machines. It should learn graph-routing, graph-state, and branch-quality policies, while runtime execution still happens through an `ILogicalOrchestrator`.

Practical fit:

```text
DynamicGraphMachine
  TrainBatchAsync(dynamic graph DPO rows)
  EvaluateAsync(held-out graph tasks)
  ExportAsync and ImportAsync(checkpoint plus manifest)
  GetOrchestratorAsync()
    DynamicGraphOrchestrator
      routes graph branches
      runs flows
      folds branch outputs into graph state
```

## Reuse Rules

1. Reuse `LearningTrainingItem` shape for initial pair representation when enough.
2. Extend with domain-specific provenance outside the minimal core record when legal and graph metadata are needed.
3. Reuse `LearningBatchResult` and `LearningEvaluationResult` metric families.
4. Reuse `LogicalContextPacket` and `LogicalContextFragment` for branch context passing.
5. Reuse transition signals for post-branch follow-up instead of inventing a second transition channel.
6. Reuse dependency-level ideas from `DependencyGraph` to separate parallel and sequential branch execution.
7. Keep deterministic graph closure as helper evidence, not as proof of learned behavior.

## Boundary Warning

Graph algorithms such as closure, bridge detection, topological ordering, PageRank, and serialization can be valuable infrastructure. They do not prove that `ILearningDynamicGraph` learned. A learned claim requires DPO rows, held-out evaluation, reward margin, fresh-load checkpoint verification, ablation against no-state baseline, and source-disjoint measurement.

---

## RECON-библиотека: точные строки-якоря (B-21 / R12-recon-libs, первый существенный проход)

> **Человеческим языком (по-русски).** Этот раздел — первый СОДЕРЖАТЕЛЬНЫЙ проход
> рекона по `llm_mvp_core`: вместо «прочитал файл» здесь зафиксированы конкретные
> строки-якоря, которые мастер-план уже использует как канон (поэтому именно они должны
> быть стабильны). Каждая строка проверена на диске 2026-06-30 (см. Gate B ниже). Это не
> разрешение на реализацию — это карта «куда смотреть, чтобы канон не уплыл».

| Якорь (file:line) | Что там (проверено) | Зачем/когда нужно в плане ILearningDynamicGraph |
|---|---|---|
| `00.Core/LearningMachineContracts.cs:26` | `SourceIds` на `LearningBatch` («tracks which corpus sources contributed rows») | Канон Q3/B-17: provenance-манифест связывается через `SourceIds`, БЕЗ правки 00.Core; join `row_id==ItemId` |
| `00.Core/LearningMachineContracts.cs:30` | `LearningTrainingItem` = DPO-триада (ItemId/Prompt/chosen/rejected) | Проекция `GraphDpoPair.chosen/rejected/input` на существующий record (Q2/Q3) |
| `00.Core/LearningMachineContracts.cs:245` | `ILearningMachinePair<TPersonal,TGlobal>` (A-LM-08: только анонимный sync) | Канон B-15-split: контракт пары common/adaptive; privacy-граница sync |
| `00.Core/LogicalContracts.cs` | `ILogicalUnit/ILogicalFlow/ILogicalOrchestrator`, transition signals, context packets | Уровни L2/L3/L4 иерархии графа; reuse context packets для передачи между узлами |
| `11.DPO/DpoTrainer.cs:44` | формула DPO-маржина `margin = chosenReward - rejectedReward` | Канон: единственный движок маржина; training-update всех обучаемых узлов сдвигает веса здесь (не литерал) |
| `11.DPO/DpoTrainer.cs:119` (район) | усреднённый `RewardMargin = marginSum/count` в `LearningBatchResult` | Источник числа для Gate B Q30 (`RewardMargin>=0.025`) — берётся из движка, не хардкод |
| `21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs:19` | `public const int MaxAdaptiveBranchCount = 8` | Источник потолка N комбинаций (Q16/B-7: дефолт N=4, потолок 8) |
| `27.TranslationDomain/TranslationMachine.cs:70/187` | `RewardMargin: 0.03` ЛИТЕРАЛ (находка рекона R-19) | НЕ копировать: эталон обучения сломан хардкодом; маржин обязан идти из `DpoTrainer` (R4) |
| `27.TranslationDomain/TranslationMachine.cs:277` | `SchemaVersion: 1` в `TranslationMachineCheckpoint` | Прецедент schema-guard checkpoint: переиспользован тегами `graph-dpo-common/1` и `graph-client-local/1` |
| `27.TranslationDomain/TranslationMachine.cs` Save/Load (≈:388/:447 по мастеру) | Export/Import round-trip checkpoint | Образец B-18 client-local export/import (round-trip воспроизводит accuracy ±epsilon) |
| `17.DynamicDomains/DynamicDomainGraph.cs` | Graph closure, bridge subphase expansion | ДЕТЕРМИНИРОВАННАЯ infra; ablation-baseline, НЕ доказательство обучения (Boundary Warning) |
| `22.AgentAlgorithms/MultiAgent/DependencyGraph.cs` | Topological levels, parallel/sequential detection | Reuse идеи уровней для разделения параллельных/последовательных веток (порядок ≠ авторитет, B-9) |
| `26.TrainableRouter/Training/RoutingDpoPairBuilder.cs` + `RoutingTrainingRunner.cs` | Прецедент строителя DPO-пар + train/evaluate runner | Образец для DPO-строителя блока 07 и runner-контракта (метрики, held-out) |

> **Boundary честности (claim_scope == evidence_scope).** Это первый существенный
> проход: проверены и заякорены **12 строк** в **10 файлах** `llm_mvp_core`, важных для
> канона графа. Это НЕ «прочитан весь репозиторий» и НЕ «40 проходов». Полный per-file
> аудит всех подпроектов = будущая работа (карта файлов — в `claude-02` §"RECON v1").
