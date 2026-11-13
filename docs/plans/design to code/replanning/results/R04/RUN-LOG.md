# R04 — журнал исследования и адресной правки плана

**Текущее состояние, 13.10.2026 / R04-REPAIR-1:** исправлены effective prompt/dispatch, DECISIONS, FINDINGS и CHANGE-MAP; ниже зафиксированы источники и реальные commits. Это review/repair одного R04, не повтор R01–R06 и не независимый Astra PASS. Empirical=NOT_MEASURED; product_implementation=NOT_PERFORMED; independent_review=NOT_PERFORMED. Старый журнал сохранён как история. Актуальный остаток и порядок — в последнем разделе этого файла и DECISIONS D00/D03/D07/D08; прежний encoder-first handoff отменён.

## Исторический checkpoint 13.10.2026 · первоначально назначенный R04

**Поручение того checkpoint:** Proceed to R4; исследовать представления/доменные признаки/typed many→many/evaluation с контекстом S00–S03; опубликовать результаты в xiigen-planning/llm_plan3/replanning. Не выполнять все 25 и не возвращаться к старому S04-only обзору.

**Результат того checkpoint:** предметный разбор кода, источников и ограничений; один причинный representation recipe E01; отдельный E02 one-pair learning recipe; role prompts для CODEX selection / Chinese both-side author / CODEX exact-pair judge; bounded handoffs R05/R07/R13/R18; семь предметных маршрутов; source receipts и change map. Приоритет encoder-first исправлен ниже, сами рецепты и источники сохранены.

**Статусы того исследования:** research_result=DELIVERED_WITH_SCOPED_GAPS; empirical=NOT_MEASURED; product_implementation=NOT_PERFORMED; independent_review=NOT_PERFORMED. G01 graph OutputRefs→payload consumer и G02 upstream segment association не объявлены закрытыми. Документный результат не равен готовности всех слайсов или implementation-ready эксперимента.

### Исполнитель и разделение ролей

Автор — соответствующая ChatGPT-сессия. Запрошенная проектная роль экспертного выбора/оценки — CODEX Astra 6 ultracode, alias из текущего dispatcher. Отдельный CODEX launch, китайский generator, Astra ultra reviewer и субагенты в первоначальном R04 не вызывались. Actual external model/effort/run ID отсутствуют и не выдуманы. A/B исследования выполнены одним автором, не двумя независимыми проверяющими.

Skills прочитаны адресно в восьми файлах K/docs/skills, точные spans — R04-S01…S08. Применены architecture, planning, plan-review, code-review, prompt-authoring и human-readable audit. Это не полный load всех skills/AGENTS. Историческое replay70/30 из experiment-design не перенесено вопреки актуальному one-pair/no-repeat.

### Входные refs и параллельная дельта первоначального исследования

Research P0: `de8da48af66094fcdcd803bd5ee97d780c8901ba`.
Research K0: `dcdded6bb6039c03e80a2786312435350894a4ee`.
Publication preparation P1: `76e6f4083cf8327c2db3b173dd62e26cbd5cc8aa`, tree `6d8c67df20d272c1d8db105ebe4e6a0d683ff059`.

Compare P0→P1 показал шесть новых commits и девять изменённых paths: AGENTS/START/JOIN, CP6 checkpoint и пять R05 carriers. Это не source-кодовая миграция и не причина повторять весь R04. Адресно прочитаны CP6 AGENTS/START/checkpoint и R05 CP1 FINDINGS. R05 сохранён; его current StepVector вывод принят как sibling evidence, не новая собственная проверка trainer.

### Что действительно прочитано первоначально

Source ledger различает actual code paths, declarations, active WorkLeft sections, historical bodies, indexes, sibling receipts и tree-only discovery. В нём сохранены repo/ref/path/read_scope. Длинные ответы с обрезанным хвостом не засчитаны как full read.

Собственные core-переходы: request compiler→semantic compiler→raw/canonical/TeachingVector; canonical→baseline subphase router; stored BPE/example/index→query vector→typed bestExample.Output; wrapped typed logical unit→candidate projection→snapshot→in-memory IQueryable. Отдельно прочитаны numeric accuracy meter и translation output scorer.

WorkLeft active code/branch/SOLUTIONS/VERIFY sections прочитаны для восстановления исходного цикла. Старые тела в details целиком не пересматривались. RAG route: WorkLeft RAG-04→nested translation RAG→primary multilingual tokenizer guide. Большой IDE P3-B/P3-C source принят через exact R03 receipt; full primary reread не заявлен. S00–S10 conditional queue прочитана; выполнение S00–S03 не присвоено по таблице.

### Что автор уточнил при первоначальной собственной проверке

1. Отказ от неверного глобального вывода о потере raw: compilation хранит Text; проблема относится к конкретному canonical/normalized consumer.
2. Отказ от приписывания локального TeachingVector vocabulary всем kNN: KnnLogicalUnit сохраняет свой BPE и другую Vectorize функцию.
3. Найден actual typed outcome bestExample.Output. Это top-1 exemplar selection, а не произвольный генератор новых many-output решений.
4. Queryable на graph wrapper уже материализован; его наличие не доказывает resolver или streaming. Candidate ID digest не payload semantic hash.
5. DynamicGraphAccuracyMeter и translation character metric не названы полноценной предметной экспертизой.
6. Убрана возможность считать TeachingVector scaling в R05 предсказываемым выходным вектором LoRA.
7. В proposed prompts сохранены обе Chinese final sides, отдельная CODEX assessment и отсутствие automatic retry/replay.

Это авторская проверка рассуждения/документов, не независимый PASS, не новые product tests и не runtime execution.

### Инструменты, ошибки и доступ первоначального исследования

Использован рабочий GitHub connector для content/tree/ref/compare reads и документной публикации. Первоначальная проблема прямого сетевого доступа из container не является read-only правами GitHub. Branch URL с encoded slash для core был заменён поддержанным git-ref чтением. Поиск по default branch, не вернувший совпадение, не использован как доказательство отсутствия кода. Некоторым большим JSON tree replies не хватало вывода; это отражено в depth.

До той публикации проверены workflow paths P1: `.github/workflows` вернул 404; actual root и полное `.github` tree подтвердили только `copilot-instructions.md` и отсутствие repo workflow files в данном snapshot. Это не утверждение об отсутствии внешних integrations/rules. Ручной dispatch не выполнялся. Commit marker skip-ci не является PASS required checks.

### Исторический публикационный контракт research checkpoint

Ровно пять непустых UTF-8 carriers внутри `results/R04/`. Tree строится поверх актуального проверенного base, не заменяет всё дерево старой копией. Ref update — nonforce с expected parent lease. При конфликте новый HEAD читается, собственная дельта переносится без потери чужих файлов; чужой R04 carrier нельзя молча перезаписывать. Итоговый commit ID и remote readback сообщаются после фактического ответа GitHub; журнал заранее не заявлял ещё не подтверждённый ref update.

Проверка той публикации: compare parent→candidate должен показывать только эти пять файлов; после update читаются remote head/дерево и содержимое результата. Sibling/shared plan/core/tests/workflows/data/weights не входили в research diff. GitHub publication не означает запись на локальный C: пользователя. Текущая прямо разрешённая правка R04 prompt/dispatcher ниже имеет другой явно ограниченный scope.

### Что не делалось и прежний остаток

Ноль запусков product build/tests/coverage/Windows/GitHub Actions/trainer, ноль генераторных кампаний/вызовов независимого судьи, ноль изменений core/весов/корпусов. Не удалены и не отключены действующие проверки. Нет измеренного времени/accuracy/PC footprint, новых DPO pairs, принятого checkpoint или certification.

**Исторический handoff, частично superseded:** R07 G01/G02; R05 продолжить CP1; R06 separate representations/fixed-k; R08 rights/families; R13 конкретизировать E01; R18 versioned encoder; R10 receipts. **Сейчас:** CP2 принят, E01 больше не обязательный первый шаг, новый current handoff — ниже. G01/G02 не закрыты этой поправкой.

Никакие дополнительные R26/30/50, бесконечные review loops или фоновые исполнения не назначены. Неизвестный эффект является вопросом будущего опыта; неизвестный consumer/схема/owner/разрешение остаётся конкретным prerequisite.

## 13.10.2026 · R04-REPAIR-1: фактический review и исправление

### Поручение, классификация и границы

Текущее прямое поручение пользователя: **Please review and repair R4**, с публикацией в `lubochka/xiigen-planning:llm_plan3/replanning`. Потребованы подробные model-ready prompts, сохранение всех ролей/алгоритмов/доменов, WorkLeft/RAG и независимых R01–R06. Это разрешает адресную правку R04-плана и его dispatcher binding; не разрешает product training/code changes.

Предыдущий R04 не был пустым или только S04: F01–F10, E01/E02 и source traversal оказались содержательными. Ошибка review-классификации — **premature priority / incomplete dispatch packaging**, а не отсутствие всего исследования: encoder объявлен первым seam до доказательства ordinary consumer; новый CP2 стык ещё не принят; standalone R04 отсутствовал при существующих R05/R06 packets. Это исправлено непосредственно в active prompt/START и R04 summaries/decisions, не только замечанием в журнале.

Write scope этой правки: `run-prompts/R04-PARALLEL6.md`, `START-NEXT-RUN.md` и собственные `results/R04/{FINDINGS.md,DECISIONS.md,CHANGE-MAP.csv,RUN-LOG.md}`. `SOURCES.csv` сохранён без переписывания старых receipts; следующие RV-записи расширяют его **в существующем RUN-LOG**, не шестым research ledger. Не изменялись siblings, их prompts, slice implementation packs, core/skills/workflows/tests/datasets/weights.

### Новое первичное чтение и принятые источники

Каждый RV ID ниже разрешается здесь в repo/ref/path/blob/read_scope. P2=`lubochka/xiigen-planning@d02acc4ba83e4c903ea9f23374e891c4b863f70a`; K0 определён выше. Источник sibling не становится самостоятельно прочитанным первоисточником всех его ссылок.

| Source ID | Exact locator и степень чтения | Принятое следствие / ограничение |
|---|---|---|
| R04-RV01 | P2 `llm mvp/design to code session/replanning/results/R05/FINDINGS.md`, CP2 §§1,3 и §4 F09–F14 в ответе 1–83; хвост ответа обрезан | Existing trainer/coordinator; Compact; response objective; active-count signal; S06 campaigns. Compact перечитан отдельно RV03; остальное SIBLING_EVIDENCE. R05 общий OPEN не повышен |
| R04-RV02 | P2 `llm mvp/design to code session/replanning/results/R05/SOURCES.csv`, blob `ba28c3d31d266e7c16c4918bc2d4bfb954559d7d`, 28–48 | Source routing к C12 actual LogicalDpoTrainingUnit; не whole-file read всех C01–C18 |
| R04-RV03 | K0 `18.CapabilityTraining/LlmIngredientLogicalUnits.cs`, blob `cb8744fcbbcff99821852e789dc92163976c0ad7`, 2250–EOF BuildBatch/Compact/ToMask; дополнительно прочитаны 1510–1625,1810–1975,2100–2250 | PRIMARY_CODE: raw passport перед collapse/480-character truncation; совпадающий префикс может стереть различие сторон. Не полный аудит большого файла, не measured learning |
| R04-RV04 | K0 `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs`, blob `2957816a88aa9cdcd3020bb81be56b5dfff165e0`, 1–145 и 145–320 | PRIMARY_CODE: same saved BPE; top-1 typed output; confidence warning с Ok; current-thread allocation. Actual external domain/arbiter consumer не доказан самим классом |
| R04-RV05 | P2 `llm mvp/visual studio & visual studio code continue/RAG-06-llm-dynamic-flow.md`, blob `c09b2db12d3d56c60fdf39e99f67eac65d05793a`, 1–95 и 95–148 | RAG_ROUTE_READ: §B указывает точное имя authority amendment. Исторические file counts и CODE-ABSENT не перенесены как current facts |
| R04-RV06 | P2 `llm mvp/llm dynamic flow/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md`, blob `0408bddbcd595f2023dec5001b52bac68af7b8c5`, 1–125 | PRIMARY_DESIGN_READ: positive/negative/warning memory и final arbiter; threshold 0.50 исторический, не новый универсальный gate. Попытка имени без -v1 дала 404 и была исправлена по RAG; не вывод об отсутствии документа |
| R04-RV07 | P2 `llm mvp/design to code session/replanning/START-NEXT-RUN.md`, pre-repair blob `9798696a974d0f36557d33bf598a91587a9a91c7`, 1–75 и 76–EOF; перед write blob подтверждён | FULL_DOCUMENT_READ: R04 получал CP5 assembly, standalone был только у R05/R06. Dispatcher исправлен после создания полного R04 packet |
| R04-RV08 | K0 `docs/skills/experiment-design-SKILL.md`, blob `5987ff02ea7866c69c87b7efffa5658251419de8`, 1–105 | TARGETED_SKILL_READ: baseline/intervention применены; replay70/30, automatic Tier3 и SCALE_UP=correct не унаследованы. Это не правка core skill |
| R04-RV09 | P2 `llm mvp/design to code session/replanning/results/R06/RUN-LOG.md`, blob `fd51077424e97bd7420624e79cbf9ebcebb57ca6`, 1–60 | SIBLING_RECEIPT_READ: R06 принял старый R04 и R05 CP1; отправленная ему CP2 дельта не объявлена уже принятой |
| R04-RV10 | GitHub compare planning `95e1815a81e60e07b1626fa016fd06614f924cf7...d02acc4ba83e4c903ea9f23374e891c4b863f70a`; поздний ref read `ed7fe365467bd35233fa3fb3afab4a74a5814ecc`; direct current target blob reads | BRANCH_DELTA_READ: сохранены параллельные R05 CP2/R06 и другие опубликованные правки; первоначальные R04 blobs проверены перед заменой |

Дополнительно при repair прочитаны current `original task.md` 1–80, blob `10b9c752ec740d439b46e0730f4cde2562ebe033`, и WorkLeft SOLUTIONS 1–48, blob `aa40b48b370915cb182e06b890e1010a0f223a7f`; actual research/control evidence не заменено историческими legal/model claims. `previous rounds thinking results.md` прочитан в начале 1–70 как historical evidence старого R04 test-fitting; позднее compare показал append. Полное сегодняшнее чтение всего выросшего файла не заявляется. JOIN/AGENTS, свой CP5/ROUND-DELTA и пять R04 carriers прочитаны адресно; filenames/tree не названы содержательной экспертизой.

Skills при repair: architecture `trainable-architecture-decision` 80–155; planning `phase-planning` 1–65; plan-review 1–80; code-examination 1–85; prompt-patch-authoring 30–115; human-readable-architecture-audit 1–90; experiment-design 1–105. Все пути — K0/docs/skills/*-SKILL.md; исходные blob identities остаются в R04-S01–S08, соответствующие spans новой загрузки перечислены здесь. Actual независимый reviewer/subagent/CODEX/Chinese call отсутствует; шесть A1–B3 являются частями prompt, не шестью выполненными модельными вызовами.

### Изменения, видимые в результатах

E01 сохранён со frozen BPE/examples/k/search/solver, fair budget, reindex price, oracle/predicted/perturbed и semantic falsifier, но стал условным опытом. E02 больше не зависит от нового encoder. Принят более конкретный Compact seam; objective остаётся R05/R12 и помечен sibling-derived. LoRA output semantics не подменены scalar TeachingVector или transpose shape. Many→many раскрыт через a,b→r1,r2,r3 associations, request-bound payload, lawful zero и async order. Dynamic kNN отделён от top-1 exemplar/evidence; warning не засчитан abstention. Memory/arbiter contract связан с первичным RAG-source.

Standalone содержит current task/not-task, весь необходимый role/rights/skills/context блок, шесть bounded внутренних частей с prompt/input/output/good/bad/edge/documentary/future examination/stop/owner, seven-subject passports, same-context DPO правило, cost/retention/bulk и parallel receipts. START действительно маршрутизирует R04 на него; старый CP5 common не накладывается вторым конфликтующим поручением. Ни 25 раундов, ни роли, ни программная широта не сокращены.

### Реальные publication receipts

Использованы GitHub Contents create/update actions с явной `branch=llm_plan3` и текущим blob SHA для существующих файлов. Это не локальные неподнятые изменения и не внешний apply step. Force/ref-reset не применялся. Список ниже содержит только GitHub-returned commit IDs; commit самого этого журнала заранее не выдумывается.

| Изменение | Подтверждённый GitHub commit | Returned content blob |
|---|---|---|
| Новый standalone R04 | `b8f3e18dce26ce72126d7e5e52a761a8913158a2` | create API вернул commit; blob проверяется readback |
| START binding и corrected priority | `1e7a6c27ccbca7e22b61f0f894443389ab1e07c5` | `024c16f5aa4858d43e8b945eddbcc7ffdec15240` |
| R04 DECISIONS D00–D09 | `54e40632dd76d068c081643073fb828c30184e6e` | `e8e2a8b75ec316ca482070fc44732f38b7c56512` |
| R04 FINDINGS и F11 | `b5af9623962fd0b2dc844dedf0bf2599ae007b6b` | `4e36c2fe286b9891325ae3c6db03cc0bc5503c5b` |
| R04 semantic CHANGE-MAP | `88f5e6943b3565e443db4010e38c694779b38c66` | `a9b37d731321793178dd81e730d8f89af6e2c95d` |

После записи журнала выполняются readback/diff проверки remote результата. Они проверяют документную публикацию, не learning efficacy. Skip-ci message не объявляется результатом checks; запуск GitHub Actions не запрошен. Содержательная самопроверка привязана к этой правке и приведённым sources, но independent Astra approval по-прежнему NOT_PERFORMED.

### Текущий точный остаток и следующий шаг

**R13/R18:** выбрать ближайший разрешаемый existing learning route; R05 CP2 lossless exact-pair/token/objective seam имеет конкретное основание и не ждёт optional encoder E01. До самого урока обязательны actual lesson/recipient/accepted state/evaluator/ordinary consumer и его data/budget; они не выдуманы и не закрыты этой правкой. R05 продолжает только CP2 gaps. R06 получает warning/eligibility/arbiter/coordinate delta. R07 сохраняет G01 и G02, не полный graph restart. R08 получает domain/source-family passports. R10 получает receipts шести workers, не общий DONE; получатели не объявлены принявшими дельты без их фактического результата.

Новые product tests/coverage/Windows/Actions/build/training/paid calls не добавлены и не выполнены. Оценка будущих learning algorithms задана через lawful tasks, admissible DPO, предметный экспертный judgment и измерения quality/time/full footprint/retention по independent families и bulk windows. Published plan repair не означает implementation-ready всего проекта или доказанного улучшения ученика.
