# Codex Recon — Механизм загрузки скилов в расширение (MVP)

Дата: 2026-06-23. Роль: рекон-аналитик (read-only). Задача: собрать evidence для плана
ILearningDynamicGraph. Это НЕ разрешение на реализацию.

Вопрос: как устроен механизм загрузки скилов в расширение.

---

## 1. Что это и где (точные пути)

Целевая область `C:/Users/lubao/Documents/xiigen/llm mvp/skills loading as an ai extension`
содержит 3 файла, и в них уже зафиксирована архитектура skill-loading. Это не пустая папка —
это готовый дизайн-черновик.

Точные ключевые пути:

- `C:/Users/lubao/Documents/xiigen/llm mvp/skills loading as an ai extension/skills-inventory-universal-vs-project-specific.md`
  — опись 164 skill-файлов (`docs/skills`), деление universal core vs project adapter,
  карта арбитров, рекомендация структуры AI extension (registry.json / load-rules.json /
  conflict-policy.json).
- `C:/Users/lubao/Documents/xiigen/llm mvp/skills loading as an ai extension/# Next planning.md`
  — основной транскрипт (2914 строк). Содержит сам дизайн механизма загрузки: requirements
  ledger, plan review (verdict REJECT), и финальное архитектурное решение.
- `C:/Users/lubao/Documents/xiigen/llm mvp/skills loading as an ai extension/Please review the plan again and va.txt`
  — дубликат транскрипта `# Next planning.md` (тот же объём 2914 строк, то же начало).
  Отдельной информации не несёт.

Главный итоговый план-артефакт упомянут в транскрипте, но лежит ВНЕ моей зоны (не читал, путь
из текста): `C:/Users/lubao/Documents/xiigen/extensions reasearch/PLAN-XIIGEN-SKILL-SELECTION-MACHINE-EXTENSION-v1.md`.

Реальные code-якоря в репозитории (read-only, подтверждены):

- `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/OrchestrationPlanMachine.cs:7`
  — `public sealed class OrchestrationPlanMachine : ILearningMachine<OrchestrationPlanDomain>`.
- `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/OrchestrationPlanMachine.cs:65`
  — точка входа для skill-loading: `if (contract.Kind == IntentKind.SkillLoadSessionInit)`
  сейчас возвращает пустой `GenerationRouteMode.Passthrough` (НИЧЕГО не загружает).
- `C:/Projects/llm_mvp_core/00.Core/LearningMachineContracts.cs:208`
  — `interface ILearningMachine<TDomain>` с TrainBatchAsync/EvaluateAsync/ExportAsync/
  ImportAsync/GetOrchestratorAsync (полный trainable lifecycle).

---

## 2. Как устроено / работает (кратко)

Зафиксированный целевой механизм загрузки скилов (двухпроходный):

Проход 1 (по первому prompt):
```
UserPromptSubmit
-> CandidateSkillRetriever (детерминистический: BM25/registry) собирает кандидатов
-> SkillSelectionMachine : ILearningMachine<SkillSelectionDomain> ВЫБИРАЕТ bundle
-> ContextInjector грузит выбранные user + XIIGen skills в контекст AI
-> AI выдаёт результат
```

Проход 2 (по результату AI, fallback если первый выбор неполон):
```
AI output + prompt + выбранный bundle
-> ResultGapInspector ищет признаки пропущенных скилов
-> emit DPO row (chosen = верный bundle/патч, rejected = неполный bundle)
-> Stop hook патчит неполную часть (полный redo только если пропущенный skill меняет
   архитектурную роль / план / gate / безопасность / весь workflow)
```

Host: первым продуктовым target выбран Claude Code hooks (`hooks.json`, 5 событий, на базе
плана token optimizer); Codex — первый measurable verification target (точные JSON/token/
session метрики). Bootstrap day-0: пока нет обученных весов, детерминистический retriever
работает как candidate generator, а НЕ как решатель.

Уровни обучения (аналог D1-D4): S1 тип задачи -> S2 выбор bundle по первому prompt ->
S3 конфликты (user/project skills выше universal, project-specific не затирать) ->
S4 добор скилов после результата AI.

Метрики Gate B (пороги из транскрипта): first_prompt_skill_selection_accuracy >= 0.85,
top_k_skill_recall >= 0.90, bundle_precision >= 0.80, after_output_recovery_accuracy >= 0.85,
user_skill_preservation_accuracy >= 0.95, wrong_skill_overload_rate <= 0.10.

Статус артефактов: единого утверждённого плана на момент транскрипта НЕ было (plan review дал
REJECT, потому что R-003/R-004/R-005 unmapped, R-006 contradicted — BM25 был главным решателем
в token optimizer). Дизайн зафиксирован, исполнения нет.

---

## 3. Релевантность к ILearningDynamicGraph

Очень высокая. Этот механизм — почти калька той же дисциплины, что нужна для
ILearningUnit/ILearningFlow/ILearningOrchestrator/ILearningDynamicGraph:

- DPO/обучение: skill selection уже спроектирован как DPO-задача (chosen/rejected bundle,
  50 train + 50 held-out, rejected объясняет почему wrong) — прямой источник DPO-протокола.
- граф/оркестрация: цепочка retriever -> selection machine -> injector -> inspector -> Stop
  hook — это уже узлы/рёбра с разделением "детерминистический кандидат" против "trainable
  решение per edge", что и есть тема DynamicGraph (fan-in кандидатов, fan-out выбранного bundle).
- измерение: набор именованных метрик (recall/precision/recovery/overload) — готовый
  evidence/gate шаблон для held-out оценки графа.
- teacher-provenance: в транскрипте teacher/distillation-провенанс НЕ зафиксирован (пробел,
  см. §5). Для DynamicGraph его надо добавить отдельно.
- точка интеграции: `IntentKind.SkillLoadSessionInit` passthrough в OrchestrationPlanMachine —
  конкретное место, куда можно подключить обучаемый skill/graph путь вместо заглушки.

---

## 4. Классификация для кода (по правилам скила)

Правило (HOW-TO-USE-SKILLS.md строки 327, 342, 346; trainable-architecture-decision):
реальный уровень = ILearningMachine / ILogicalOrchestrator / ILogicalFlow / ILogicalUnit /
ILearningMachinePair с trained state + checkpoint + DPO lifecycle. scorer/selector/classifier/
router/KNN/Jaccard/BM25/regex/lookup — это НЕ уровень архитектуры, а внутренний
детерминистический хелпер; допустим только как candidate-generation/eval/fallback внутри
разрешённого уровня.

| Компонент | Класс | Уровень / вердикт |
|---|---|---|
| `SkillSelectionMachine` (целевой) | spec | Должен быть `ILearningMachine<SkillSelectionDomain>` — это решатель выбора скилов. Финальное решение НЕ должно принимать BM25/regex. |
| `OrchestrationPlanMachine` (код, есть) | factual | `ILearningMachine<OrchestrationPlanDomain>` — корректный уровень (Level 5). Файл:7. |
| `CandidateSkillRetriever` / BM25 / `AstRelevanceRanker` | spec/factual | Детерминистический ХЕЛПЕР. Допустим ТОЛЬКО как генератор кандидатов/fallback до первой обученной версии. Как финальный решатель — нарушение (R-004/R-006, FC-12). |
| `HumanIntentBoundaryClassifier.Classify` (код, OrchestrationPlanMachine:62) | factual | Детерминистический классификатор-хелпер внутри ILearningMachine. Допустим как роутинг намерения, не как trainable решатель. |
| `_humanExplanationKnn.FindNearest` (код, OrchestrationPlanMachine:82) | factual | KNN-хелпер внутри ILearningMachine. Допустим как nearest/eval-роль; не самостоятельный уровень. |
| `ResultGapInspector` (целевой) | spec | По описанию — детектор пропущенных скилов + DPO-эмиттер. Если станет финальным authority выбора, требует обёртки в разрешённый уровень (ILogicalUnit/ILearningMachine) с trained state; как чистый детектор сигналов это helper. Уровень не зафиксирован — product/architecture decision. |
| `SkillSelectionDomain`, `skills-registry.json`, `conflict-policy.json` | spec | Domain-тип и данные, не исполнительный уровень. |

Вывод по классификации: дизайн правильный ровно в той части, где решение отдано
`ILearningMachine<SkillSelectionDomain>`, а BM25/KNN/classifier понижены до кандидатов/хелперов.
Это совпадает с требованием для ILearningDynamicGraph: trainable authority per edge, hidden
deterministic shortcut запрещён.

---

## 5. Пробелы / что доисследовать

- Единый план `PLAN-XIIGEN-SKILL-SELECTION-MACHINE-EXTENSION-v1.md` лежит вне моей зоны
  (`extensions reasearch`) — не читал. Его надо прочитать отдельным реконом, чтобы взять точные
  контракты компонентов, фазы и Gate B (в транскрипте упомянуты секции 5.2/5.3/5.7/8/13).
- Teacher/distillation-provenance для DPO НЕ зафиксирован (teacher_access_mode, hosting_provider,
  model_id, model_license, provider_terms_checked). Для ILearningDynamicGraph обязателен —
  пробел плана.
- Уровень `ResultGapInspector` (helper-детектор vs trainable ILogicalUnit) — нерешённый
  product/architecture decision.
- Naming: пользователь говорил `IMachineTraining`, в коде зафиксирован `ILearningMachine<TDomain>`.
  Решено делать `ILearningMachine`, `IMachineTraining` как alias/adapter позже — но это не
  закреплено в утверждённом артефакте.
- Связь с существующим `ITrainableRoutingFlow` (26.TrainableRouter) и с доменами
  `42.ProjectWikiDomain` (там уже есть trainable units/DPO generators) не прочерчена —
  доисследовать перед планом графа.
- Реальная замена passthrough в `OrchestrationPlanMachine.cs:65` на обучаемый путь не сделана
  (исполнения нет, только дизайн).
- Дрейф каталога: HOW-TO-USE-SKILLS.md в шапке говорит про ~159 skills, живой каталог 164
  (отмечено в inventory-файле) — мелкий, но реальный рассинхрон.
