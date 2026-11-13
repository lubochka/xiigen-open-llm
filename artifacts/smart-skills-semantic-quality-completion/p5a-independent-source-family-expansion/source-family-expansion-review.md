# P5A source-family expansion review

## Короткий вывод

P5 заблокировал real training/eval правильно: у `PromptToTask` были две draft-строки из одной семьи `basic prompt.txt`, а у `OrchestrationReasoning` две draft-строки из одной семьи `PATCH-phase-transition-protocol-SKILL.md`. Это не дает source-disjoint split: одна строка не может быть train, а другая held-out eval, если обе учат один и тот же источник.

P5A добавляет только candidate evidence. Здесь нет training, eval, checkpoint/model write, semantic-quality claim или DPO-ready promotion.

## Исправление source-state после parent review

Parent review нашел overclaim в первой версии ledger: все шесть строк были помечены как `done`. Исправленная граница такая:

- только `AGENTS.md` найден в analysis-state как `done`;
- `skills-inventory-universal-vs-project-specific.md`, package `doc-07956-xiigen-universal-orchestrator-plan`, `UniversalDependencies.cs`, `UniversalNineNodeDevelopmentFlow.cs` and `UniversalFlowSingletonTests.cs` наблюдаются как `todo` в analysis-state;
- эти пять источников остаются `named_reference`, потому что они явно названы Luba/current plan, но это не делает их training-ready;
- source со статусом `named_reference` и наблюдаемым состоянием `todo` не может стать training-ready, пока источник либо не будет analyzed/accepted в analysis-state, либо parent явно не примет его как named-reference evidence только для drafting revised split.

## Какие новые source families проверены

Проверены шесть независимых candidate families:

| Candidate | Target | Verdict | Почему независимая |
|---|---|---|---|
| `P5A-CAND-PTT-AUTHORITY-RULES-001` | `PromptToTask` | accepted candidate | Analysis-state `done`; отдельный rules-документ про контракт интерфейса и ограничения; не derived от `basic prompt.txt`. |
| `P5A-CAND-BOTH-SKILLS-INVENTORY-001` | both | accepted candidate, named reference only | Analysis-state `todo`; источник явно назван Luba/current plan; отдельный анализ universal core vs project adapter; не derived от текущих P3/P4 source families. |
| `P5A-CAND-ORCH-DOC07956-001` | `OrchestrationReasoning` | accepted candidate, named reference only | Analysis-state `todo`; источник явно назван Luba/current plan; отдельный universal-orchestrator package про branches/review/reprocess; не phase-transition protocol. |
| `P5A-CAND-ORCH-UNIVERSAL-DEPS-001` | `OrchestrationReasoning` | accepted candidate, named reference only | Analysis-state `todo`; источник явно назван Luba/current plan; отдельная dependency/relation family; duplicate cache copy не считается второй семьей. |
| `P5A-CAND-ORCH-NINE-NODE-FLOW-001` | `OrchestrationReasoning` | accepted candidate, named reference only | Analysis-state `todo`; источник явно назван Luba/current plan; отдельная station-chain implementation evidence family. |
| `P5A-CAND-ORCH-FLOW-ISOLATION-001` | `OrchestrationReasoning` | accepted candidate, named reference only | Analysis-state `todo`; источник явно назван Luba/current plan; отдельная test-evidence family про isolation/dedup/no cross-run contamination. |

## Что universal, а что удалено

`AGENTS.md` candidate для `PromptToTask`: universal часть - извлечь из правил task contract: входы, публичный результат, ограничения, forbidden actions, allowed verification and evidence boundary. Удалены `.NET`, C#, service names, namespace names, index names, code snippets and file/title clues.

`skills-inventory-universal-vs-project-specific.md`: universal часть - отделить reusable skill/session/evidence discipline от project adapter. Удалены `.NET`, C#, `xUnit`, `OperationResult`, concrete subproject map, exact commands, branch/commit conventions and local artifact paths.

`doc-07956-xiigen-universal-orchestrator-plan`: universal часть - держать branches separate, merge after evidence/review, review before formatting, return only failed fragment to repair. Удалены folder/title/node labels, node numbers, XIIGen branding and source-document-line shortcut.

`UniversalDependencies.cs`: universal часть - root decision, dependent evidence/gates and relation labels. Удалены C# namespace/classes, parser mechanics, CoNLL-U syntax, method names and project path.

`UniversalNineNodeDevelopmentFlow.cs`: universal часть - ordered stations with evidence carried forward and review before claim. Удалены class names, exact node ids, singleton mechanics, `Project15Bridge`, `SemanticCompiler29`, metrics and async envelope.

`UniversalFlowSingletonTests.cs`: universal часть - isolation, deduplication and no cross-run contamination evidence before reusable-asset claim. Удалены `xUnit`, `Assert`, test names, temp paths, run ids, singleton identity mechanics and exact-nine assertion as product rule.

## Что пока нельзя тренировать

Нельзя запускать training/eval, потому что P5A не пересобрал split manifests и не прошел parent review. Accepted candidate здесь означает: источник достаточно содержательно описан, чтобы parent review мог решить, стоит ли drafting revised source-disjoint split. Это не означает DPO-ready, train-ready, eval-ready или semantic-quality-ready.

Для пяти named-reference rows это ограничение сильнее: они не могут стать training-ready, пока named-reference source не будет analyzed/accepted в analysis-state или parent явно не примет его как named-reference evidence только для revised split drafting.

Нельзя копировать raw private source text into common training. Все строки здесь используют source refs, hashes and summaries.

## Хватает ли families для revised split draft

Для `PromptToTask` теперь есть исходная семья `basic prompt.txt` плюс две независимые candidate families: authority rules и skills inventory. Parent review может считать, что candidate material enough to draft revised source-disjoint split, но только после проверки leakage and ablation plan.

Для `OrchestrationReasoning` теперь есть исходная phase-transition family плюс четыре независимые candidate families: doc07956, dependency/relation, station-chain flow and isolation evidence. Parent review может считать, что candidate material enough to draft revised source-disjoint split.

Recommended parent verdict for this artifact: `PASS_WITH_REPAIR_NEEDED`, because candidate-family coverage is sufficient to consider revised split drafting, but all rows remain candidate-only and require parent review before any split rewrite, DPO promotion, training or eval.
