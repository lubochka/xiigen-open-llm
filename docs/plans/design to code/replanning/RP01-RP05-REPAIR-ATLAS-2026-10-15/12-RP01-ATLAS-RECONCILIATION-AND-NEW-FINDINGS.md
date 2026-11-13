# 12 · RP01: сверка findings, три новых уточнения и применённые правки

**15 октября 2026 · AUTHOR_SOURCE_REVIEW / APPLIED_DOCUMENT_REFINEMENTS / DRAFT.** Промежуточная версия опубликована `30bee7fdbdf32a491319d084a243669115b36c3f`, затем проверка и правки продолжены. Это не новый цикл R01–R25, не четыре дополнительных обязательных запуска и не независимая приёмка модели.

## Результат

Сопоставлены основные классы атласа с уже опубликованным RP01. Исправлен внешний RP01 shortcut, который ещё направлял в V6/K979256. Добавлены **три source-specific уточнения**: граница root-training/ordinary-serving readiness; достижимость порога +2/12; разница между чистым приростом и отсутствием потерь по отдельным семьям. Это одно условие запуска и два следствия текущего scientific predicate, **не три наблюдавшихся production сбоя**.

Уточнения внесены в реальные [T2](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/children/RP01.03A-T2-EXAM.md) и [T3.C](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/children/RP01.03A-T3-COLD-USER.md), а не оставлены только в отчёте. Новых задач, product tests, model sampling или Windows workflow gates нет. Существующая strict scientific policy и её приёмочные пороги не менялись.

## Источники и глубина проверки

P исходной сверки=`c751f58cf2f9c0c583b81c93f654752edf6c658d`; K=`025c42c74b1d30ffa4660f9824a52cf8fcaf7c88`. Live refs прочитаны. A=этот atlas; P1=`../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/`.

| Источник | Что действительно проверено |
|---|---|
| A/README.md | Полный текущий текст в последовательных чтениях, включая исторические оговорки и owner-specific ссылки. |
| A/01-ORDERED-REPAIRS.md | Основные классы 1–16 и таблица остатков; последующие RP03/RP04 notes прочитаны как чужие source reports, не собственное runtime evidence. |
| A/05-ADDITIONAL-REPAIRS.md | Source map, applicability, детали 17–23 и начало 24; не свежий полный аудит каждого пункта 17–36. |
| A/09-OPERATIONAL-REPAIR-EXTENSION.md | Полная summary-таблица 37–46, source/skills boundaries и детали 37–39; не повтор исполнения этих work orders. |
| A/11-LEARNING-FIRST-REPAIR-DETAILS.md | Полный outline 47–58 и уже отмеченная SR-неоднозначность. Outline не называется двенадцатью полностью выполненными fixes. |
| A/02-REVIEW-WORK-ORDERS.md | Opening, A/B и точный `Extension to B` через поиск по fetched content; подтверждено сочетание optional и `Use exactly one`. Полный повтор всех A–E исполнений не проводился. |
| P1/PACKAGE-MANIFEST.json | Current 3.1, aliases, 16 задач/9 основных children/4 incident packets, роли, no-replay, policy и diagnostic ограничения. |
| P1/T2, T3.C | Полные исходные карточки до изменений, затем remote readback именно изменённых sections на commits. |
| K/25.ModelServer/Program.cs | Host entry/composition/DI lines 1–470 и retained activator 1820–1945. Не весь 2000+-строчный файл. |
| K/25.ModelServer/ContinuousLearning/CodeDocumentationGenerationEndpoint.cs | Полный короткий endpoint и самостоятельное разрешение generation service. |
| K/26.TrainableRouter/Training/CodeDocumentationGenerationService.cs | Generation options/Generate и relevant GenerateCore→PrepareStart часть; не полный свежий аудит control/export helpers. |
| K/26.TrainableRouter/Training/CodeDocumentationScientificTrainingLifecycle.cs | Свежий `ValidScientificVerdict`/raw binding участок 142–177; prior same-conversation complete hold/publish source reading переиспользовано с тем же blob. |
| K/25.ModelServer/ContinuousLearning/CodeDocumentationInitialTrainingConfiguration.cs | В этой беседе ранее полностью прочитан на том же exact K025c42c; текущий commit/activator join перепроверен. Bound root/disabled selector условия не выведены из имени файла. |

Прямые anchors: [initial training configuration](https://github.com/lubochka/llm_mvp_core/blob/025c42c74b1d30ffa4660f9824a52cf8fcaf7c88/25.ModelServer/ContinuousLearning/CodeDocumentationInitialTrainingConfiguration.cs), [Program / hosted activator](https://github.com/lubochka/llm_mvp_core/blob/025c42c74b1d30ffa4660f9824a52cf8fcaf7c88/25.ModelServer/Program.cs), [ordinary endpoint](https://github.com/lubochka/llm_mvp_core/blob/025c42c74b1d30ffa4660f9824a52cf8fcaf7c88/25.ModelServer/ContinuousLearning/CodeDocumentationGenerationEndpoint.cs), [generation service](https://github.com/lubochka/llm_mvp_core/blob/025c42c74b1d30ffa4660f9824a52cf8fcaf7c88/26.TrainableRouter/Training/CodeDocumentationGenerationService.cs), [scientific predicate](https://github.com/lubochka/llm_mvp_core/blob/025c42c74b1d30ffa4660f9824a52cf8fcaf7c88/26.TrainableRouter/Training/CodeDocumentationScientificTrainingLifecycle.cs).

Skills: переиспользованы ранее реально прочитанные в этой беседе sections `trainable-architecture-decision`, `phase-planning`, `code-examination`, `prompt-patch-authoring`, `conversation-intent-and-self-reflection`/`human-readable-architecture-audit`; свежий `plan-review` 45–112 сверяет task/evidence scope и полноту поручения. `experiment-design`/`improvement-measurement` применяются с действующей локальной адаптацией P1/GOVERNANCE-AND-REVIEW: behavioral evidence, без возврата test-count/50-pass/Tier-3 quotas. Нового полного чтения всей skills/RAG библиотеки здесь нет.

Original task, WorkLeft-02/RESEARCH-03, предыдущие результаты и PARALLEL join не заменяются новым outline: предыдущая source работа остаётся исходным requirement baseline. Не заявляется свежий полный audit всех RAG, всех S00–S10 или всех RP02–RP05 production consumers.

## 1. Сверка старых findings: что не нужно ремонтировать повторно

| Atlas class | Итог в пределах этой проверки | Что остаётся открытым |
|---|---|---|
| 1 — полный функциональный объём | Требование сохраняется; RP01 не должен заново выполнять 25 исследований. | Полная реализация каждого исходного домена/потребителя не доказана manifest. |
| 2 — исторический порядок/skills | Current RP01 manifest ссылается на локальную адаптацию навыков и learning-first governance. | Global skills и их применимость в других RP не переписаны этим audit. |
| 3 — единственный dispatcher | Current RP01 aliases согласованы; внешний A/README shortcut был устаревшим, теперь исправлен. | Все остальные root/архивные ссылки проекта не объявляются полностью проверенными. |
| 4–5 — proven defect и ранний эффект | Сохраняются conditional repair, existing parent/pair reuse, held→T2 и unknown→recovery. | Actual PC state здесь не наблюдался; нельзя объявить T1 уже выполненным или отсутствующим. |
| 6 — CODEX/Chinese/CODEX | Точные роли закреплены; diagnostic X/A1 не заменяют S/A/J. | Подлинные вызовы и semantic pair quality — runtime evidence, не текст плана. |
| 7–8 — objective/row/step/sealed | Полезные разграничения сохраняются. Root-only initial route не универсальный bulk trainer. | Generic atomic pair-store paths не были заново исполнены или полностью аудированы. |
| 9–10 — representation и law-kNN | Предыдущий `hits[0]` finding остаётся одним consumer. Negative/warning, AST/POS/referent и actual typed reader нельзя заменить count соседей. | Не доказаны domain mapping gain, dynamic-k learning или обычный consumer по всем языкам. |
| 11–12 — Unit/Flow/async Graph | Самостоятельные роли и RP03/RP04 owners сохраняются. | Не повышаются чужие source findings до implemented/learned/runtime-ready. |
| 13 — continual feedback/state | Current RP01 явно не разрешает descendant обучение через first-root reset. | Реальная RP05 continuation transaction и retention требуют своего evidence. |
| 14–15 — причинность, минимальные checks, PC | Принципы подтверждены как требование; T2 уточнён по достижимости и парным потерям. | Нет измерений времени, RAM/VRAM, CI-длительности или доказательства, что тесты подгонялись. |
| 16 — repair actual packet | Исправлены именно T2/T3.C и внешний atlas entry. | Это не новый независимый review всех карточек RP01–RP05. |

Следовательно, старые `RESIDUAL` в 01 о внутренних RP01 V4/V6 bindings нельзя механически переносить на current 3.1. Исторический report верен в своей временной области; эта reconciliation не переписывает историю и не считает старый fix новым открытием.

## 2. ATLAS-RP01-A01 — исправлен устаревший внешний вход

**Evidence class:** VERIFIED_DOCUMENT_MISMATCH, повтор класса 01 §3/05 §18 на конкретном navigation consumer, не новая DPO ошибка.

**До:** A/README «Действующие локальные маршруты» → RP01 V6/K979256. **Current source:** P1 manifest3.1 → RP01-EXECUTE-NOW.md. Исторические оговорки в README были, но shortcut по-прежнему назывался текущим.

**После:** RP01 row ведёт в единственный state-aware dispatcher и current manifest. Для остальных RP ясно указана область snapshot; существующие owner-specific ссылки сохранены. **Applied:** `0f1221079c1c5ba16b5dbcdaae990b6e8bdec881`.

**Промпт для такого ремонта:** «Прочитай ровно внешний referring row и current target manifest/entry. Сравни next effect для complete-held, partial-intent и clean-not-started. Замени неверную ссылку/формулировку, не создавай новый dispatcher и не меняй runtime permissions». **GOOD:** held→T2. **BAD:** ещё один T1 ради следования старой V6 цепочке. **EDGE:** нет PC access→STATE_UNKNOWN/00B, не ABSENT. **Проверка:** remote navigation readback, 0 product tests/model calls. **Falsifier:** новый shortcut всё ещё направляет в другой current effect/owner. **Status:** DOCUMENT_PATCH_APPLIED_AND_READ_BACK.

## 3. ATLAS-RP01-A02 — first-root startup readiness не равна обычной генерации

**Evidence class:** SOURCE_VERIFIED_CONDITIONAL_BRANCH + PROMPT_REFINEMENT; runtime failure не наблюдался. **Расширяет:** 01 §§4/5/13/16, 11 §54/56, но добавляет конкретный K025c42c callchain.

**Source chain:** `CodeDocumentationInitialTrainingConfiguration.BindCheckpointForStartup` → `ContinuousLearningRetainedObjectHostedActivator.StartAsync` → retained composition/gateway. При включённом first-root выборе admissible parent обязан быть exact pinned Step1, parentless, InitialBaselineNoUpdate. После публикации latest может стать trained descendant. Если старый selector trio унаследован новым host и loader вернул этот descendant, root bind отказывает; activator отмечает failed readiness и закрывает retained gateway.

**Важное ограничение вывода:** `CodeDocumentationGenerationEndpoint` разрешает отдельный `CodeDocumentationGenerationService`; его ordinary `GenerateCore` вызывает `PrepareStart`. Из failed retained-training readiness **не следует автоматически**, что normal Generate недоступен или веса потеряны. Это отдельная проверяемая цепочка. Source condition не превращается в claim «каждый cold host падает».

**Что исправлено в T3.C:** purpose нового host, initial-selector presence, retained-training readiness, actual Generate result и served SHA отражаются раздельно. Serving-only процесс не наследует root optimizer выбор по инерции. Disabled branch source требует пустой/отсутствующий MODE и null REASON/PARENT; частичное удаление одного MODE не равнозначно отсутствию trio.

**Конкретный prompt:** «Получив actual T3.P publication и разрешение обычного restart/inference, прочитай текущую startup binding и ordinary generation ветви. Запусти поддерживаемый serving-only process с accepted configuration, без случайного first-root selector trio. Не изменяй user/machine-wide environment или другой training host. Запиши actual request/served identity отдельно от retained readiness. При несовпадении различи configuration, publication, loader и endpoint failures, не запускай новый урок».

**GOOD:** actual accepted SHA обслуживается обычным процессом без initial-root authority. **BAD:** readiness отказ назван потерей Student; latest trained model переименован B0 для «исправления». **EDGE:** activator не участвует→NOT_APPLICABLE; mixed-purpose host→собственный supported contract, не снятие guard; MODE отсутствует, REASON остался→source-bound partial configuration; publication неизвестна→readback, не reset.

**Owner:** существующий T3.C/host owner, code01C только при действительно доказанном code defect. **Examination:** один уже предусмотренный ordinary cold witness; никаких дополнительных Teacher/C0/optimizer. **Falsifier:** данный host не использует root binding или actual source уже допускает нужное serving состояние и receipt это подтверждает — тогда соответствующий риск не применяется. **Applied:** T3.C `5031e809764790debaf9516b4d70e799a4ea5d85`, изменённый раздел перечитан с remote. Product code/configuration здесь не изменялись.

## 4. ATLAS-RP01-A03 — достижимость +2 зависит от baseline

**Evidence class:** математическое следствие прочитанного `ValidScientificVerdict` и current 12-family policy. Это ограничение критерия, не observed failure Student и не универсальный аргумент против DPO. **Расширяет:** 01 §14, 09 §45; не дублирует общий запрет posthoc thresholds.

`b` = число baselineCorrect из frozen family scores; `t ≤ 12`. Следовательно, `t-b ≤ 12-b`. Порог `t-b≥2` возможен только при `b≤10`.

| Baseline b | Maximum possible gain | Возможен нынешний +2? |
|---|---|---|
| 9 | 3 | Да |
| 10 | 2 | Да, только при Trial=12 |
| 11 | 1 | Нет |
| 12 | 0 | Нет |

Это **арифметический пример**, не реальные результаты обучения. Улучшение качества объяснения или времени при тех же binary scores также не изменяет этот конкретный family-count gate.

**Что исправлено:** T2 companion report получает `baseline_correct`, `maximum_possible_family_gain`, `required_gain`, `gain_threshold_attainable`. Нет новых strict JSON полей, нового scorer или изменения policy. Нет предварительного дополнительного T2/C0: использовать уже существующие допустимые baseline scores либо вычислить при обычной оценке held. Пока scores отсутствуют — UNKNOWN.

**Prompt:** «После фиксации независимых оценок вычисли headroom. Если threshold недостижим, сохрани фактический прирост и объясни ограничение нынешнего критерия. Не ухудшай C0, не заменяй семьи, не повторяй использованную пару. Публикация под существующим не пройденным gate не разрешается; новая policy/другая способность могут исследоваться только отдельным prospective owner решением».

**GOOD:** 11→12 записано как +1, полезное наблюдение, не достаточное для +2. **BAD:** назвать алгоритм сломанным или повторять урок до невозможного +2; после ответов убрать лёгкую family из denominator. **EDGE:** missing/ambiguous score→не подставлять 0; другой versioned N/gain→пересчитать `N-b`, не жёстко применять 12. **Falsifier:** проверяемая версия gate не основана на этих binary counts/пороге; тогда вывод требует другого расчёта.

**Owner/status:** T2 interpretation, retrospective policy остаётся неизменной. Applied вместе с A04 в `4d2fa08d97c3145e0bda07b10a9427052bee6630`; remote section reread. Никаких extra model calls или нового теста.

## 5. ATLAS-RP01-A04 — net gain не доказывает сохранение каждого навыка

**Evidence class:** source-specific граница scientific predicate и арифметическое различие; не утверждение, что полный publisher реально принял регрессирующий Trial. **Расширяет:** существующие retention requirements 01 §§13–15, 05/09 denominator и bulk rules.

Current `ValidScientificVerdict` требует правильные hashes/roles/family bindings, zero trial/new critical и net correct-count gain. Он **не требует** `n10=0`, где n10 — family baseline true→Trial false. Полная публикация имеет дополнительный native gate и actual authority; их результат здесь не вычислялся.

Из уже фиксированных парных scores вывести n00, n01, n10, n11, где индексы — baseline/Trial booleans. `N=sum(nij)`, `b=n10+n11`, `t=n01+n11`, `gain=n01-n10`.

**Пример без model calls:** n11=8,n01=3,n10=1,n00=0 → 9/12→11/12, net+2 и одна lost family. Возможная потеря не обязательно critical по предзаданной rubric. Поэтому «+2 и critical=0» нельзя перевести в «никакого forgetting».

**Что исправлено:** T2 показывает improvements и losses, разрешённые идентичности lost families/их предзаданные law categories, и ограниченный общий вывод. Обе стороны используют прежнюю rubric. Summary — companion evidence; strict verdict JSON не расширяется. Требование n10=0 не добавляется постфактум как новый обязательный acceptance gate.

**Prompt:** «Используй те же raw responses и frozen family decisions. Составь paired table, проверь арифметические тождества, покажи n10 отдельно от critical count. Сформулируй, где был gain и где loss. Не называй mean/net improvement гарантией retention по каждому домену/закону. Не меняй оценку, чтобы таблица выглядела лучше».

**GOOD:** qualified net gain с явно названной потерей и next learning hypothesis. **BAD:** скрыть lost family среди 60 repeats или переименовать её в unscored после результата. **EDGE:** несколько lawful answers→прежняя смысловая rubric, не literal match; неполная связка IDs→INVALID_EVIDENCE, не false scores; posthoc law grouping→только exploratory description, не заранее подтверждённая subgroup claim. **Falsifier:** отсутствуют matched family IDs/применяется иной scoring contract — такой paired вывод нельзя строить без нового основания.

**Owner/status:** T2; тот же applied commit `4d2fa08...`, не отдельный обязательный этап. New product tests=0; new Student/Qwen calls=0; реальный n10/quality здесь NOT_MEASURED.

## 6. Уже существующее замечание 11 §58: подтверждено, не засчитано новым

В A/02 opening specialization разрешён `only when useful`, а `Extension to B` говорит `Use exactly one complete specialization`. Точная пара формулировок найдена в текущем fetched content. Это уже описано в 11, поэтому **не новое A05 и не ещё один code bug**.

Безопасная выдача поручения по действующему принципу: ноль SR, когда сам complete B packet уже содержит нужный discriminator; иначе включить одну применимую специализацию, не весь atlas. Такой parent должен сохранить actor/inputs/primary/skills/consumer/state/GOOD-BAD-EDGE/output, а не сокращать за счёт функциональных требований.

**Status:** RECONFIRMED_WORDING_AMBIGUITY. Тело 02 этой дельтой не переписано; не объявляется завершённым repair. Это точечный остаток у автора 02, не условие нового общего цикла и не blocker уже определённого RP01 T1/T2.

## 7. Публикация и воспроизводимая авторская проверка

| Действие | Commit / remote evidence |
|---|---|
| Сохранён промежуточный report до окончания проверки | `30bee7fdbdf32a491319d084a243669115b36c3f` |
| T2 дополнен headroom и paired losses | `4d2fa08d97c3145e0bda07b10a9427052bee6630`; blob `788e241c56ddef401066a4ef22f73a74992af0df`, section readback |
| T3.C разделяет startup/serving | `5031e809764790debaf9516b4d70e799a4ea5d85`; blob `60a40a47885635aa58293bb47d860549a021ca2f`, section readback |
| Atlas README исправлен и ссылается на 12 | `0f1221079c1c5ba16b5dbcdaae990b6e8bdec881`; blob `ae87ea19bad11057f2ccb791889af4cb4806c4ee`, RP01 row readback |

Проверка — чтение конкретного кода, арифметика условий, before/after instructions и remote readback. Это авторская проверка документов/источников, не software suite и не независимый Astra verdict. Семантические сценарии проверки: clean start; complete held; partial intent; post-publish inherited root selectors; baseline11/12; net+2 при n10=1; missing scores; absent activator. Ни один не выдаётся за реально исполненный PC experiment.

Изменены только этот report, A/README и две RP01 child cards. Не перенумеровываются general 1–58 или RP-specific F/N findings. Состав основных RP01 tasks/children не увеличен; manifest не требует нового task ID. Непрочитанные source-claim части атласа не помечены CLOSED.

## 8. Что это меняет для ближайшего исполнения

Продолжать **по имеющемуся state**, а не по количеству review-документов. Ready/no intent → один существующий T1; complete held → T2 с дополнительной дешёвой интерпретацией тех же scores; partial intent → recovery; published → ordinary T3.C с правильным purpose/configuration. RP02 domain laws/kNN/representation, RP03 Unit/Flow, RP04 async/Graph, RP05 continual/PC остаются параллельными owned slices, а не ожиданием всех 25 rounds.

**CODEX с astra 6 ultracode выполняет роль человека в выборе учебного случая. Разрешённая внешняя открытая китайская модель создаёт обе окончательные chosen/rejected стороны. Отдельный CODEX с astra 6 ultracode оценивает качество точной пары.** T2 и independent reviewer — отдельные назначения с настоящими model/session receipts. Действующие разрешения переиспользуются в своём scope, нового общего согласования эта дельта не вводит.

Core, weights, DPO/private exam data, machine configuration, Windows workflows и permissions не изменены. Реальные Student/Chinese/CODEX calls, независимая научная приёмка, runtime recovery, RAM/VRAM/time benchmarks и learning gain здесь **NOT_PERFORMED/NOT_MEASURED**. Список расширен проверяемыми условиями и применёнными prompt refinements, не объявлением обученной системы.
