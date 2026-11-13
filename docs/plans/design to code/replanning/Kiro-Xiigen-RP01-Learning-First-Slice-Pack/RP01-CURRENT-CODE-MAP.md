# RP01 — текущий offline runner, уже выданный Main500 и границы доказательств

**15.10.2026 · `B500-LIVE-RUN-REPAIR-20261015`.** Прочитанный core snapshot K=`58d12ecfe7af05e3221f984bf6c4c67d82fe3b96`; code parent=`bc55daea505e581309e2d1e0c6d72ccef73d25e5`. Planning reading P=`76657d964769914a88556237f28fc47e6d46da01`; перед публикацией проверены четыре последующих коммита до `8a32384fd9859877468220e2e4f2130610d47238`: они относятся к RP02/Atlas, а не заменяют прочитанные RP01 карточки.

Это source/artifact review, **не прямое наблюдение PC и не повторное обучение**. Единственный вход — [RP01-EXECUTE-NOW](RP01-EXECUTE-NOW.md).

## Реальный путь, который следует переиспользовать

`tools/Rp01SimulationDpo/Program.cs` → `CodeDocumentationSimulationDpoExperiment.Inspect/Run` → existing governance/content authority → `LogicalDpoPreparedBatchMaterializer` / exact response material → content-bound training lease → существующий `DpoTrainer` / project-LoRA recipient → private checkpoint + optimizer/RNG → readback. Отдельная `evaluate` команда использует настоящий likelihood и CodeDocumentation generation consumer; не встроенный Qwen grader.

| Открытый источник | Установленный факт и граница |
|---|---|
| [Simulation README](https://github.com/lubochka/llm_mvp_core/blob/58d12ecfe7af05e3221f984bf6c4c67d82fe3b96/tools/Rp01SimulationDpo/README.md), полностью | Уже есть inspect/calibrate/train/evaluate; один trainer/reference/optimizer на конечную кампанию;0 external calls; private candidate, не serving adoption. |
| [Program.cs](https://github.com/lubochka/llm_mvp_core/blob/58d12ecfe7af05e3221f984bf6c4c67d82fe3b96/tools/Rp01SimulationDpo/Program.cs), полностью | Exact root/config/B0/train500/calibration pins; main ровно500, одна эпоха; `none` разрешён. Нет generic resume, другого домена или загрузки descendant как training parent. |
| [Experiment.cs](https://github.com/lubochka/llm_mvp_core/blob/58d12ecfe7af05e3221f984bf6c4c67d82fe3b96/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs), прочитанные начало/Inspect/Run admission/material/observer sections | Strict UTF8/count/schema/duplicate checks; `simulation`, `csharp`, `CODE_TO_DOC`; typed DeterministicGenerator; exact response masks; independent frozen reference; dataset+arm replay marker; existing lease. Полный численный backward/optimizer здесь заново не аудирован. |
| Старый `tools/Rp01RawDpoLesson/README.md`, scientific/recovery sections | Старый12×5×2 и native evaluator существуют отдельно. Их нельзя500 раз вызывать для новой кампании или ослаблять frozen verdict ради ускорения. |

**Непрерывность внутри main не равна межпроцессному resume.** Сохранение/открытие optimizer/RNG не доказывает наличия разрешённой команды продолжения C#→ENRU. Такой стык, если понадобится, принадлежит B500.03/05, не второму trainer.

## Точные артефакты существующего опыта

Все пути ниже в K под `.kiro/xiigen-slices/work/results/`. Читать метаданные/receipt, не закрытые case/gold bodies в контекст автора данных.

| Относительный путь | Что он подтверждает |
|---|---|
| `RP01-astra-synthetic-dev-csharp-500-20261015/dataset-manifest.json` |500 byte-distinct records,25 templates×20; actual `simulation`; programmatic materialization обеих сторон; независимость families не установлена. Авторская пометка «review NOT_RUN» не заменяет более поздний reviewer result. |
| `RP01-simulation-exact-review-and-heldout-20261015/frozen-protocol.json` |25 preference cases +3 free subset; primary margin tolerance1e-6; same decoder;112 scorer forwards/6 generations; scope narrow near-transfer, broad retention не измеряется. |
| `RP01-simulation-exact-review-and-heldout-20261015/handoff-metadata.json` |Начальный NEEDS_FIX F1/F2, ссылки на semantic validation526 cases. Это проверка содержания примеров/oracle, не526 ученических успехов. |
| `RP01-simulation-exact-review-and-heldout-20261015/delta-review/delta-review-return.json` |Поздний PASS ровно F1/F2: internal trusted mutation entry и optional wall limit. Candidate-bound source pins; не новый runtime/scientific PASS. Не требовать повторной починки уже закрытого замечания. |
| `RP01-simulation-main500-execution-20261015/frozen-runtime-manifest.json` |Pinned executable/dependencies/B0/data/protocol; main ID; batch10/max50/epoch1/wall none; original order C0score→C0free→main→Tscore→Tfree. |
| `RP01-simulation-main500-execution-20261015/main-existing-cost-observation.json` |Четыре completed training-step windows и процессный sample; это не завершение всех500 или итоговый gain. |

[Открыть runtime manifest](https://github.com/lubochka/llm_mvp_core/blob/58d12ecfe7af05e3221f984bf6c4c67d82fe3b96/.kiro/xiigen-slices/work/results/RP01-simulation-main500-execution-20261015/frozen-runtime-manifest.json) · [Frozen protocol](https://github.com/lubochka/llm_mvp_core/blob/58d12ecfe7af05e3221f984bf6c4c67d82fe3b96/.kiro/xiigen-slices/work/results/RP01-simulation-exact-review-and-heldout-20261015/frozen-protocol.json) · [Dataset manifest](https://github.com/lubochka/llm_mvp_core/blob/58d12ecfe7af05e3221f984bf6c4c67d82fe3b96/.kiro/xiigen-slices/work/results/RP01-astra-synthetic-dev-csharp-500-20261015/dataset-manifest.json).

## Реальные расходы вместо догадок

В cost observation четыре полных step windows: **101.6866939;110.2845364;111.2888932;106.7415123 секунды**. Sample: wall514.4443229s, CPU512.78125s, working set2,492,129,280 bytes, peak2,669,109,248 bytes. CPU/wall≈0.997 относится к одному процессу; не доказывает ни недоступность многопоточности, ни оптимальность загрузки PC. Это свидетельство конкретного запуска, не прогноз времени следующего.

Step включает forwards/backward/optimizer/transaction/checkpoint. Внутренний optimizer-only не измерен; checkpoint serialization и optimizer/RNG save вложены в step total и не складываются с ним второй раз. Калибровочная короткая пара не является throughput benchmark для500. Исторические5ч43м нельзя целиком приписать Qwen или optimizer без таймеров.

Ближайшая оптимизация: после reconcile текущего опыта взять **один** подтверждённый hotspot, измерить его отдельным узким профилем и сохранить same-input comparison. Не делать полной instrument-everything фазы перед уже готовым обучением. Не менять работающий frozen binary посреди Main500. Whole-PC, GPU и временные затраты CODEX отдельны; если не измерены — UNKNOWN.

## Какие ошибки планирования устранить в пяти карточках

1. First50 streaming не поддержан exact500 CLI: текущий main reuse; streamed-next-campaign требует отдельного реального adapter, а не выдуманной команды.
2. Generic DEV/Q_law/40+20 не заменяет уже frozen25+3. Дополнительные открытые диагностические measurements не выдавать за новый независимый primary verdict.
3. `simulation`/DeterministicGenerator не переименовывать в `ASTRA_SYNTHETIC_DEV` или Qwen. Для будущих реальных Astra outputs сохранять настоящее авторство и отдельный J, без legal/provenance spoofing.
4. Уже есть отдельная calibration и exact F1/F2 review: reuse, не повтор ради закрытия task ID.
5. C# hardcoding не доказывает multidomain/continual capability. Подготовить конкретный translation consumer/parent contract; не скрывать этот gap и не тормозить им текущий C# опыт.

## Не потерять более широкую архитектуру

Прочитаны relevant sections: D/original task.md; WorkLeft-RAG-MASTER-INDEX→WorkLeft-02 teaching и RESEARCH-01; R03/SOURCES; previous rounds начало; PARALLEL-R01-R03-JOIN; R01-R25-to-code-slices; F/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md. Это реальная навигация к первичным решениям, **не полный обход всех RAG/WorkLeft/S00–S10**. Не объявлять R01/R03/R07 закрытыми этим ремонтом.

Сохраняются семь доменов WorkLeft и владельцы RP02–RP11. Domain/intent/version→semantic roles/vector→law-aware positive/negative/warning retrieval→learned ranking/k→actual consumer — не просто `hits[0]`→majority vote. Unit many→many, Flow corrections, async Orchestrator и higher-order Graph сопоставляются с actual ILogical*/ILearningMachine по поведению, без новых trainers ради названия. LoRA, tokenizer и semantic projection — разные контракты.

Skills реально прочитаны на K58: architect-behavior-classifier1–150; phase-planning1–125; plan-review85–190; generated-code-review55–155; prompt-patch-authoring1–165; human-readable-architecture-audit1–145. Применение: ограниченное поручение, exact source→consumer, literal good/bad, отсутствие ложного review/learning, понятное русское описание. Это не независимый вызов CODEX Astra.

## История и итоговая граница

[Предыдущая карта scientific/full-LoRA/raw/RAG anchors](https://github.com/lubochka/xiigen-planning/blob/76657d964769914a88556237f28fc47e6d46da01/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-CURRENT-CODE-MAP.md) сохраняет все старые ссылки и ограниченные выводы; их не считать свежим повторным аудитом.

Наблюдение Main-issued/нескольких steps не даёт MAIN_COMPLETE, QUALITY_GAIN или ADOPTED. Их проверяет фактический runtime owner по более свежим artifacts. В этой сессии изменяются только planning docs; Student, training data, private exam bodies, code K и workflows не изменяются. Выполнение новых экспериментов и независимая Astra-проверка этой редакции: NOT_PERFORMED.
