# N52 — журнал доказательств, новизна относительно других агентов, качество плана

**Дата:** 19.10.2026. **Где публикация разрешена:** только `xiigen-planning@llm_plan3/llm mvp/design to code session/replanning/possibleDifs/**`. Каталог `llm_mvp_core@codex/dpo-implementation/docs/plans/replanning-possible-diffs/**` использовался для чтения: никаких записей туда делать нельзя.

## Исследовательский protocol

- **Золотой код** `orchestrator-extensions@b293045bd89b1eb96d3e32fedd341f1a4170adbc` и **current** `codex/dpo-implementation@44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76` исследованы через GitHub connector `fetch_file`, `fetch`; документарное доказательство фиксирует точные пути/строки/sha.
- Прочитаны repo skills `architect-behavior-classifier`, `plan-review`, `code-examination`, `prompt-patch-authoring`, `experiment-design`, `claim-verification`, `learning-path-audit`, `human-communication-GLOSSARY`. Их применимые правила: **сначала caller→callee→consumer**, привязка источника к claim, одно изменение на эксперимент, negative/edge, независимая оценка, не выдавать source/read-only за native effect, домен=предметная тема (не синоним Unit). Большие неподходящие plan/test gates не копируются механически на быстрый исследовательский протокол.
- Прочитан RP01 пакет `RP01-2026-10-16-CAUSAL-LEARNING-DIAGNOSTIC-PACK.md`; исходная выдача доказывает в первую очередь отрицательный результат свободной генерации на маленьких фиксированных exam, не дефект любого KNN. Проверены каталоги соседних агентов (`possibleDifs`: 427 entries на начало аудита; read-only `llm_mvp_core/.../replanning-possible-diffs`: 93 entries): широкая тема уже покрыта, нужен **новый привязанный golden branch**.

## Первичные файлы и результаты сравнения

| Доказательный объект | Gold Git blob SHA | Current Git blob SHA | Проверенный вывод |
|---|---|---|---|
| `models/orchestration/orchestration-v1/runner-report.json` | `d94756f722d8e2d362437fcd4d02f5395e0d62ea` | тот же | Исторический положительный отчёт есть в current |
| `models/orchestration/orchestration-v1/orchestration-v1-checkpoint.json` | `286fbf8082133b8a03a86477cf1aa32544928e48` | тот же | Saved trained state не удалён |
| `models/orchestration/orchestration-v1/manifest.json` | `3ca4414098fb5d8e71ce87a74bd61c4a17ef1d92` | тот же | Manifest identity сохранилась |
| `training-data/orchestration/orchestration-v1/training-manifest.json` | `5fd9ec6b3add0550d1d1a71030c374679cd60f18` | тот же | Исторический split manifest идентичен |
| `42.ProjectWikiDomain/OrchestrationPlanMachine.cs` | `ab4097cc4184dde26b27fb8565434b0bdd158e04` | `a4c20612034ec771a9625de3fc40adb31587e97d` | Полный текст **совпадает** после удаления ровно 238 символов текущего лицензионного заголовка |
| `42.ProjectWikiDomain/OrchestrationKnnStore.cs` | `be5c4a34f46a953da8c726f49d5a63c6b606daba` | `c54dcc8e5a934b1851e0de734030f5df92040982` | То же: исходная логика сохранена |
| `42.ProjectWikiDomain/OrchestrationPilotLoopRunner.cs` | `b63a4a04428ba8339f72dacdcab6d7112f37716d` | `765cfcdbe646508d7fb93bfd992ca8d450be848b` | То же: циклы checkpoint/resume сохранены |
| `42.ProjectWikiDomain/OrchestrationPlanOrchestrator.cs` | `a4f97d40289610b9447f1f8e838f8841d522cc9d` | `65b3cc1e654de4ebd6012a87908912fe7767a93c` | То же: direct `RunAsync→machine.GeneratePlanAsync` сохранился |
| `18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs` | `54f9a9d1ab1bf420fc8a186f326cb1fe85743b3e` | `e16657e91d4b0f201aef003f473cd8811bf87893` | Существенно иной training/admission/publication контракт; **это другой тип Units** |
| `18.CapabilityTraining/TeachingRegistryPersistence.cs` | `4f174bb01dd5c4d319e4d961769a4d53df5853c8` | `26155ca4e1960041c5991b72c5a700182c7e0d17` | Обязательные provenance и точный manifest corpus вместо реконструкции из всей истории |
| `21.AgentInfrastructure/Logical/InMemoryLogicalUnitRegistry.cs` | `8c4677005abaaa9f7479770896b743dd2e784147` | `8df425d2f11691d0e3ecf6167b22fcb0d06d2b41` | Current added learned-only selection with `IsLearned` filter |
| `models/skill-selection/skill-selection-v1/runner-report.json` | `fb7266865444a5d58029fa62134a11ac3bd7eedf` | тот же | Ещё один сохранённый результат, но он **другого домена**, не причина RP01 |
| `models/translation/translation-v1/runner-report.json` | `ff321b1033a4c95d833e690c41e6a47d592c6872` | `ff0467e6d7e3526675a5e83ab8f886d4b3477e03` | Отличие требует отдельного перевода/сравнения; не считать утратой качества из blob change |

[Gold tree](https://github.com/lubochka/llm_mvp_core/tree/b293045bd89b1eb96d3e32fedd341f1a4170adbc) · [current tree](https://github.com/lubochka/llm_mvp_core/tree/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76).

**Важно:** утверждение «same except 238-char license header» относится только к четырём перечисленным файлам ProjectWiki и проверено полным текстом, не экстраполируется на все проекты. Равный Git blob не доказывает, что production использует этот asset.

## Новизна и ownership без дублирования 400+ материалов

| Наш ID | Отличие от существующих исследований | Было ранее / не присваивать как new | Независимая проверка |
|---|---|---|---|
| N52-01 | **Именно `orchestrator-extensions`**, а не `external_domains/language_translate/local_wiki` как golden recipient; проверенное сохранение *кода+checkpoint+report* | E09/E26/E43 и подготовленные local wiki donor priorities известны; наличие чужих веток не отменяет orphan current consumer | X01: gold/current same checkpoint on direct API and ordinary consumer |
| N52-02 | Точный source crossbranch `generated capability isLearned true→false` + текущий learned-only selector | `IsLearned` общим образом обсуждался в Unit/audit docs, так что новизна **связка с donor + exact consumer hypothesis**, не сама идея флага | X02: совпадение role/domain/generic type → active consumer |
| N52-03 | Конкретный `CreateDefault` admission path расхождения gold/current | Существование governance/admission, возможно, уже документировано; root cause RP01 **не доказан** | X04: read-only default refusal vs legitimate authorized learning |
| N52-04 | **Gold cold loader = time-ordered all manifests**, current cold loader = signed subset `replay+current`; memory reachability после 2 батчей | Общая проблема replay aperture/index/memory growth многократно обсуждалась — не объявлять общей идеей новизну | X03: single variable, old/new, live/cold consumer |
| N52-05 | Golden generated capability manifest не содержит обязательные now v2 attestation+hash fields | Generic schema drift, translation checkpoint миграция обсуждалась; этот точный формат **отдельный** | X05: real old manifest with authenticated migration; no fake receipts |
| N52-06 | 3 исторически разных Mode1/2/3 donor и обязанность отдельно восстановить их learned consumers | Unseen-style и KNN в целом уже известны; new — direct orchestration branch semantic profile | X01/X07: mode-split quality; validate no leakage |
| N52-07 | Различные `state owners`: old trained retrieval/route vs RP01 random B0 q/v LoRA | **NOT NEW**; это давно установлено в RP01 diagnostic, документ просто препятствует ложной причинной атрибуции | X06/X07: controlled learning and free decoding |

**Смежные документы, которые должны оставаться главными для своих областей:** [`00-MASTER-SYNTHESIS-AND-CONFLICTS`](00-MASTER-SYNTHESIS-AND-CONFLICTS.md), [`51-GOLD-BRANCH-EXPERIMENT-CODE-PARITY`](51-GOLD-BRANCH-EXPERIMENT-CODE-PARITY-AND-E26-DETERMINISTIC-LOOKUP-2026-10-19.md), [`00-EXTERNAL-DOMAINS-POSITIVE-RESTORE`](00-EXTERNAL-DOMAINS-POSITIVE-RESTORE-P0-2026-10-19.md), [`00-LOCAL-WIKI...`](00-00-LOCAL-WIKI-GOLDEN-FIRST-2026-10-19.md), [`RP01 causal diagnostic`](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-CAUSAL-LEARNING-DIAGNOSTIC-PACK.md). Дополнительный read-only catalogue `llm_mvp_core/docs/plans/replanning-possible-diffs` уже содержит DPO, tokenization, E09, E43, restoration, learned consumer и 81–91 validation checks. Не дублировать материалы там.

## Review: что источник позволяет утверждать, а чего — нет

| Claim | Доказано? | Граница |
|---|---|---|
| В `orchestrator-extensions` сохранился архивный отчёт положительного обучения OrchestrationPlan | **YES / saved artifact** | Исторический report, не независимый повторный run |
| Три режима OrchestrationPlanMachine и checkpoint имеются и в current | **YES / full code & blob comparison** | Не подтверждён consumer deployment и показатели качества сейчас |
| Current generated capability имеет другой `IsLearned` и schema/admission | **YES / direct source** | Это может быть сознательно обновлённый тип retrieval unit |
| `ResolveLatestLearnedEntry` НЕ найдёт `IsLearned:false` | **YES / для совпадающей role+domain+typed slot** | Не доказано, что этот resolver используется для конкретного generated unit |
| Current `CreateDefault` с no authority не сможет выполнить соответствующую training mutation | **YES / static code path**, если запрос достигает блока | RP01 simulation may use другой caller |
| Cold restore current может не включить все предыдущие KNN lessons | **POSSIBLE / requires history+replay selection** | Включает только legal signed data; не обещать, что gold full sweep всегда правильный |
| RP01 500 pairs не дали положительной свободной генерации | **По сохранённому RP01 diagnostic**, не текущий независимый run | Относится к 3 C# и 4 ENRU cases; нельзя вывести population quality |
| N52 исправления уже улучшили обучение | **NO / NOT_RUN** | Документы описывают будущие экспериментальные проверки |

## Полнота запроса / self-review

- [x] Действительная золотая ветка `orchestrator-extensions` закреплена точным commit и историческим результатом.
- [x] Проверены настоящие сохранённые checkpoints, и отрицательные source findings не замолчаны.
- [x] История/соседние agents учтены и не переписаны; DPO/LoRA/vector/KNN/Units/restore/deterministic/graph покрыты crosswalk.
- [x] Новые отличия имеют exact code links, класс доказательности, домен и опровержимый A/B.
- [x] Пакет микроопытов разделяет train/heldout, direct/cold/ordinary consumer, позитив/негатив, correct/critical.
- [x] Не предлагается обходить security-admission, signature, checksum или legal data provenance.
- [ ] Native A/B, independent quality judge, old-case equivalence, actual RP01 consumer graph: **NOT_RUN/NOT_TRACED**.
- [ ] Legacy generated manifest доступен в реальном артефакте: **NOT_VERIFIED**.
- [ ] Внешний independent plan reviewer подписал EXACT current SHA: **NOT_DONE**.

**Вердикт:** `RESEARCH_SOURCE_VERIFIED; PLAN_REVIEW_SELF_CHECKED; CAUSAL_QUALITY_UNPROVEN`. Называть это завершённой починкой нельзя.

[Приоритет в начале](00-ORCHESTRATOR-EXTENSIONS-GOLDEN-FIRST-2026-10-19.md) · [различия](52-ORCHESTRATOR-EXTENSIONS-DIRECT-DIFFS-2026-10-19.md) · [микроэксперименты](52-ORCHESTRATOR-EXTENSIONS-CAUSAL-MICRO-EXPERIMENTS-2026-10-19.md).
