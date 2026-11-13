# Карта источников: что читать и в каком запуске

**Дата подготовки:** 2026-10-13. Это начальная карта маршрутов, не акт завершённого чтения всей библиотеки. Во время подготовки прочитаны указанные ниже документы/разделы и список корня planning-папки. Остальные носители установлены по Git tree; их содержательная проверка назначена будущим Rxx.

## 1. Зафиксированные источники подготовки

Planning: `lubochka/xiigen-planning`, ветка `llm_plan3`, исходный commit `62bbe7f4cf9572da4fb704fa27fc8c250e0728c6`.
Core: `lubochka/llm_mvp_core`, ветка `codex/dpo-implementation`, прочитанный commit `b4eb32aab127618cef2e8f83816cdeb737aab147`.

Коммиты добавления `replanning` идут после planning-опоры. На каждом будущем запуске фиксировать фактические refs и относящиеся к задаче дельты. Не выдавать старые статусы WorkLeft за состояние 13 октября.

Для краткости **D** = `llm mvp/design to code session` в planning repo; **K** = `docs/skills` в core repo.

| Источник подготовки | Что действительно прочитано | Что это позволяет утверждать |
|---|---|---|
| Вложения `XIIGEN-25-RUN-REPLANNING-PLAN(2).md`, `Pasted text(1).txt` | Предоставленный текст; файлы также открыты по фактическим sandbox-путям | Прежняя подготовка сама ограничивает базу конкретным S04 roadmap и отдельными skills; 25 сессий не выполнены |
| Git tree D | Имена файлов/каталогов, blob SHA, размеры | Есть семейства WorkLeft, общие задания и S00–S10; не доказаны ни актуальность каждого тела, ни выполнение задач |
| `D/AGENTS.md` | Полный возвращённый текст | Локальные правила, evidence honesty, различие подготовки и применения; шапка 09.09 не является текущим поручением |
| `D/task prompt.md` | Полный текст | Исходная DTC-цепь: дерево/HTML/CSS/скрины → API → обработка → рендеры → оценка → БД → обучение → публикация |
| `D/goal of the round.md` | Полный текст | Хранение пар/весов в canonical repo, продолжение с диска, переиспользование работы и обход RAG; старые команды не исполнялись |
| `D/original task.md` | Строки 1–80 | Исходные роли Unit/Flow/Orchestrator/DynamicGraph, many-to-many, асинхронные комбинации, собственный Builder; исторический shortlist не даёт актуального допуска провайдера |
| `D/Kiro-Xiigen-Original-Slice-Queue.md` | Полный текст | Область S00–S10 и широкий контекст; очередь условная, не доказательство отсутствующего кода |
| `D/09.09.26WorkLeft-00-ИТОГ.md` | Начальные разделы, запрос строк 1–95; длинный ответ частично ограничен выводом | Ближайшая цель и три линии программы; historical source claims требуют свежей сверки |
| `D/09.09.26WorkLeft-RAG-MASTER-INDEX.md` | Начальные разделы, запрос 1–180; ответ ограничен выводом | RAG — указатель, а не доказательство выполнения; маршруты к DPO/DTC/веткам/IDE |
| `D/09.09.26WorkLeft-RAG-00-usage-guide.md` | Строки 1–70 | Переход по source root и различие источника, статуса, кода и учебного результата |
| `D/09.09.26WorkLeft-RAG-01-dpo-legal-build.ndjson` | Первые 3 строки, не весь набор | Есть исторические ссылки и цепочки замещения; полный NDJSON-разбор ещё не выполнен |
| `D/09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md` | Строки 1–70 | Уже существует сверка учебного алгоритма; различаются исследовательская программа, решения владельца и реализационная семья dpo legal build |
| `D/S05-S10-learning-role-audit-2026-10-08.md` | Полный возвращённый текст, запрос 1–110 | Есть отдельные обязанности S05–S10 и role contract; отчёт о planning-проверках не доказывает обучение или текущую готовность продукта |

Прямой исходный адрес для этих файлов: `https://github.com/lubochka/xiigen-planning/blob/62bbe7f4cf9572da4fb704fa27fc8c250e0728c6/llm%20mvp/design%20to%20code%20session/` плюс имя файла с URL-кодированием.

## 2. Общие задания — читать содержательно, не только шапку

В R02 восстановить требования из `original task.md`, `task prompt.md`, `task prompt.txt`, `goal of the round.md`, `Изменения в плане работы - цель.md`, `Алгоритм создания ДПО.md`, `Исполнение кода без внешней модели.md`, `Обучение создания HTML CSS - 1st level.md`, `planning of testing.md`, `Claude session decisions.md`. Имена этих носителей найдены в Git tree; это не утверждение, что все они прочитаны при подготовке пакета.

Классифицировать каждое исходное требование: действует / явно заменено / историческое / конфликтует / актуальность не установлена. Историческая маркировка файла не делает исходную цель несуществующей. Старые ветки, модели, команды и разрешения не реактивируются автоматически. Решение владельца нельзя молча заменить собственным удобным вариантом.

Связанные библиотеки искать по RAG и этим промптам: `dpo legal build`, `Prepare to merge`, `visual studio & visual studio code continue`; а также названные в общих заданиях `Small model racional`, `language translate learning`, `local wiki learning`, `extensions`, `documentation learning` и `Planning sessions/session-start-extracted`. Наличие каждой текущей папки/версии перепроверить, а не выводить из Windows-строки в старом тексте.

## 3. Обход RAG — R01 начинает, R03 завершает карту маршрутов

Начало: `09.09.26WorkLeft-RAG-MASTER-INDEX.md` и `09.09.26WorkLeft-RAG-00-usage-guide.md`.

Затем обязательно пройти каждую найденную текущую ветку:

| Маршрут | Подтверждённые имена носителей в D | Что найти в первоисточниках |
|---|---|---|
| DPO и DynamicGraph | `09.09.26WorkLeft-RAG-01-dpo-legal-build.md` и `.ndjson` | Реализационную плановую семью, алгоритм, решения, данные, правила обновления |
| Design-to-Code | `09.09.26WorkLeft-RAG-02-design-to-code.md` и `.ndjson` | API, рендеры, обучение конверсии, результаты, внешние проекты |
| Ветки и слияние | `09.09.26WorkLeft-RAG-03-merge-and-branches.md` и `.ndjson` | Реальные владельцы кода, актуальные refs, уже выполненную работу |
| IDE и продолжение | `09.09.26WorkLeft-RAG-04-vs-vscode-continue.md` и `.ndjson` | Продуктовые host-сценарии, восстановление, клиентские контракты |
| Поздние изменения | `09.09.26WorkLeft-RAG-DELTAS.md` | Какие старые утверждения заменены и чем |

Это минимальный обнаруженный набор, НЕ закрытый список всех RAG. R01 рекурсивно ищет другие RAG, local-RAG, индексы/карты в planning repo, core repo и доступных связанных корнях; имена вариантов поиска — не заявление об их существовании. R03 идёт по найденным ссылкам с visited-set, чтобы циклические ссылки не вызывали бесконечное чтение.

Для каждого маршрута сохранить минимум один содержательный переход к первичному документу и все переходы, на которых основаны критические решения. Ни одна текущая RAG-ветка не остаётся просто строкой «найдена». Недоступную цель описать точным путём и влиянием; нельзя заменить её догадкой или публичным аналогом.

## 4. WorkLeft: первичный владелец чтения каждого семейства

Префикс ниже — `09.09.26WorkLeft-`, если свежая инвентаризация не установила явного преемника. Искать рекурсивно `*workleft*` без учёта регистра; дополнительно учитывать ссылки/алиасы `LeftToWork*` и варианты истории. Результат R01 должен перечислять конкретные файлы, а не только эти patterns.

| Rxx | Семейства, закреплённые для содержательного чтения | Назначение |
|---|---|---|
| R01 | `INDEX-01-file-index-and-archive-plan`, `REDATE-01-one-version-per-kind-plan`, входы RAG | Роли, версии, корни, авторство и распределение нагрузки; не начинать массовые переименования |
| R02 | `00-ИТОГ`, `PLAN-HUMAN-01`, `03-luba-decisions`, `03-decisions-register`, `COVERAGE-01-three-projects-requirements`, `PROMPTS`, `GLOSSARY-human-names`, `PRIKAZ-1808-poryadok-deystviy`, `RESEARCH-02-owner-decisions-resolution` | Исходные цели, решения и обязательства |
| R03 | Все `RAG-*` MD/NDJSON, `TASK-PROMPTS-CHRONICLE`, `TASK-PROMPTS-COVERAGE` | Маршруты источников, изменения и полнота исходных задач |
| R04 | `01-code-changes`, `05-branch-reconciliation-codex-dpo-implementation`, `SOLUTIONS-01-four-code-findings`, `VERIFY-01-status-vs-reality`, `VERIFY-02-counters-and-parallel-worktree` | Повторная работа, факты кода и доказательства S00–S03; относящиеся к другим слайсам разделы передать их владельцам |
| R05 | `02-teaching-chinese-model`, `INTEGRATION-01-training-into-work-program`, `TASK-PROMPTS-TRAINING` | S04–S06, первая реальная учебная цепь, canonical artifacts и перенос |
| R06 | `DESIGN-ACTIONS-01`, `TASK-PROMPTS-P0-P1`, `TASK-PROMPTS-P2-P3`, `TASK-PROMPTS-P4-P6-PT` | S07–S10, граф, DTC и интеграции; классифицировать реальную область разделов, не выводить её только из P-номера |
| R07 | `MECH-01-mechanism-validation`, `RESEARCH-03-dpo-algorithm-conformance` | Полная архитектура обучаемых механизмов и исходный алгоритм |
| R08 | `RESEARCH-01-training-quality-speed-accuracy-program`, `RESEARCH-01-REVIEW-20-passes` | Переиспользовать существующие исследования качества/скорости; старые 20 проходов не считать новыми Rxx |
| R09 | `INTEGRATION-02-test-batching`, `TASK-PROMPTS-TEST-BATCHES`, `DEC-01-smart-app-control-runtime-path`, `DEC-02-architect-adjudication`, `DEC-04-runtime-blocked-executor-playbook`, `MATRIX-01-progress-by-dates`, `REPORT-01-progress` | Среда, затраты, реальные блокировки, история повторов, проверка гипотезы о подгонке тестов |
| R10 | `DOCDEBT-01-documentation-debt-map`, `TASK-PROMPTS-DOCDEBT`, `VERIFY-03-session-documentation-consistency`, `VERIFY-04-integrity-and-readability`, `VERIFY-05-summary-and-verdict` и непрочитанные обязательные разделы | Закрыть адресные пробелы и сверить полноту; не повторять всё исследование |

Большие файлы имеют исторические вложения. Владелец читает все актуальные разделы своей темы и первичные решения, а историю — по зависимостям. Файлу с несколькими темами назначить одного владельца учёта и отдельные диапазоны другим Rxx; не заставлять каждого снова загружать весь мегабайтный файл. Неизвестное новое семейство получает владельца до окончания R01; новая находка позже — при ближайшей передаче.

## 5. Все слайсы и пакеты, не только S04

| Владелец чтения | Найденные пакеты/входы в D | Сохраняемый пользовательский смысл |
|---|---|---|
| R04 | `Kiro-Xiigen-Luna-S00-to-S02-Pack`, `Xiigen-S00-S02-Validation-c0080964.md`, `S01-session-handoff.md`, `S01-session-handoff/`, `s01 execution so far.md` | Разведка, продолжение уже применённого урока, полное восстановление |
| R04 | `Kiro-Xiigen-S03-Luna-xhigh-Pack`, `Kiro-Xiigen-S03-Luna-xhigh-v2-Pack`, `Kiro-Xiigen-S03-GPT6-Luna-high-Finalization-Pack`, `Kiro-S03-v2-All-Tasks.md` | Независимая оценка и граница закрытой сертификации; определить эффективную версию |
| R05 | `Kiro-Xiigen-S04-Luna-xhigh-v2-Pack` и связанные S04-входы | Реальный урок CodeDocumentation; не весь проект |
| R05 | `Kiro-Xiigen-S05-Luna-xhigh-Pack` | Canonical данные/веса и проверяемое повторное открытие |
| R05 | `Kiro-Xiigen-S06-Luna-xhigh-Pack`, `Kiro-Xiigen-S06-GPT6-Luna-high-v2-Pack` | Перенос жизненного цикла на предметы и уровни; не выбирать старую версию по удобству |
| R06 | `Kiro-Xiigen-S07-Luna-xhigh-Pack` | Обучаемая graph policy, комбинации и собственный DPO Builder |
| R06 | `Kiro-Xiigen-S08-Luna-xhigh-Pack` | Полный DTC за пределами core |
| R06 | `Kiro-Xiigen-S09-Luna-xhigh-Pack` | Клиенты, IDE, provider/user surfaces |
| R06 | `Kiro-Xiigen-S10-Luna-xhigh-Pack` | Сквозная исходная программа и отдельная финальная сертификация |

Общие входы: `Kiro-Xiigen-Original-Slice-Queue.md`, `XIIGEN-human-slice-map-2026-09-24.md`, `Kiro-Xiigen-Slice-Pack`, `Kiro-Autonomous-Slices-Authorization-RU.md`, `GOLDEN-RULE-S04-S10.md`, `ARCHITECT-ADVISOR-ESCALATION-S04-S10.md`, `GIT-SLICE-PUBLICATION-CONTRACT-S04-S10.md`, `S05-S10-learning-role-audit-2026-10-08.md`.

Для каждой версии отдельно установить: действующий source, промежуточный compiler/template, фактически установленный core overlay, effective role packet и историческая копия. Совпадающее имя слайса не доказывает, что установлен именно этот пакет. Свежая дата имени не доказывает, что он принят.

Внешние продуктовые репозитории названы исходными заданиями: `xiigen-platform-dot-net-angular`, `xiigen-flows-dot-net-angular`, `FigmaToCode-design-gen`, `CanvaToCode`. При подготовке этого пакета их код не исследован; R06 устанавливает доступ, владельцев API/данных и минимальные зависимости, R24 связывает с пользовательской приёмкой. Их недоступность не разрешает убрать продуктовый результат из плана.

## 6. Навыки, действительно использованные при подготовке

Прочитаны выбранные разделы следующих файлов K на core-опоре: startup 1–90; architect classifier 1–100; phase planning 1–100; plan review 1–115; generated code review 1–95; prompt patch authoring 1–100; output readability 1–100; reference inventory 1–150; trainable architecture decision 125–225. Это выборочное применение навыков, НЕ заявление о полном load всех skills или всех AGENTS/CLAUDE. Будущая рабочая сессия должна выполнить обязательную для своей роли загрузку.

Применены их содержательные правила: точная текущая задача; полный source-to-requirement маршрут; различение learned/deterministic; реконструкция узлов/рёбер графа; конкретный work order с хорошим/плохим примером; code-first evidence; DRAFT не равен ACCEPT; человеческий результат до служебных полей. Специальные исторические счётчики и модельные параметры не перенесены как универсальные новые требования.

## 7. Методические источники — не факты о выполнении XIIGEN

[Rafailov et al., DPO](https://arxiv.org/abs/2305.18290) — первичная постановка метода; R07/R12 сравнивают с ней фактический алгоритм, а не навязывают переписывание по названию ветки.

[Zheng et al., LLM-as-a-Judge](https://arxiv.org/abs/2306.05685) — описаны смещения порядка, многословия и предпочтения собственных ответов; это основание проверять пригодность оценщика, не доказательство его текущего дефекта в XIIGEN.

[GitHub: skipping workflow runs](https://docs.github.com/en/actions/how-tos/manage-workflow-runs/skip-workflow-runs) — skipped required checks могут остаться Pending. Это не основание отключать их и не доказательство конфигурации текущих веток.

## 8. Пределы подготовки

**На момент первой подготовки этого source map** ещё не выполнялись R01–R25 и не проводился полный рекурсивный смысловой аудит WorkLeft/RAG/code/Windows. Сейчас имеются отдельные результаты R01–R18 и опубликованные результаты R20, однако полная leaf-инвентаризация/проверка learning не следует из этих записей; R19 — только активная маршрутизация источников. Не проводились обучение и независимая субагентская приёмка. Новые документы исправляют программу будущего пересмотра; они не заменяют его результаты.


## 9. R19 — current source/algorithm/consumer routing (без повторной инвентаризации)

| Exact source family and current evidence | Где R19 сохраняет и передаёт содержание | Что нельзя считать исполненным |
|---|---|---|
| Original task/goal/task prompt, WorkLeft INDEX/MASTER/MECH/RESEARCH, RAG MD+NDJSON и исходные S00–S10/extras | START packet step 4; R01–R03/R07 direct primary; R20 manifest | S04-only inventory не вся программа; R19 не закрывает R01 leaf remaining |
| S04 §§2.1–2.3 and R16 current WorkLeft-02/INTEGRATION-01 | COMMON/R18 packet; R20 25-row compiler recipe | Chinese open model authors both DPO sides; CODEX/Astra lesson choice + separate quality judge; не вызваны здесь |
| R07 REPAIR A–I and J; R11 two DAG; R12-ARCH-2 A–F; R13 causal compiler | START scoped governing repair; R19 sources/findings and R20 actual packet manifest | R12 design ≠ runtime READY; R13 REPAIR_REQUIRED; R07 scope cannot be narrowed |
| R14 DECISIONS E1–E4 and original-only obligations | PLAN capability DAG, README, R19 decisions | Fixed M0→M7 sequence not mandated; DTC/IDE retained |
| R17 policy, live workflow definitions, 403 required-check observation | START risk-specific evidence and R20 packet; CI migration external proposal | No skipped required checks asserted PASS; workflows not reconfigured |
| R18 existing CodeDocumentation work order A–D, findings G1–G6 | PLAN/README/START E1 candidate, R20 child packet, R25 review | No Student update, trained LoRA effect, ordinary consumer or domain gain measured |
| PARALLEL-R01-R20-CONTRACT and R01-R20-REPAIR-MATRIX; actual R20 results/dispatch | Single START with scoped row and exact current checkpoint | Twenty authorized starts != twenty executed calls; R10 integrates nine research inputs |

Это current R19 semantic route; первые §§1–8 сохраняют своё историческое read depth. Для claim о runtime читать direct code/callers по назначенному Rxx, не заменять его R19 таблицей.
