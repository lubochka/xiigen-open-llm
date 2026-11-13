# 01 · Упорядоченный список ремонтов RP01–RP05

**15.10.2026.** Основание: P=`7278c9c24ff3bd3d2f8176914e717b5897409a01`, K=`97e4af3eadd3981320f698d104134a01984a2542`. [Паспорт источников и границы](04-SOURCE-AND-VERIFICATION-RECEIPT.md). Это порядок анализа и приоритетов, **не 16 обязательных последовательных запусков перед первым уроком**.

Метки: **DELTA_VERIFIED** — сравнивались старое и новое содержимое; **CURRENT_CONTRACT_VERIFIED** — прочитан текущий исправленный договор, но не вся история его появления; **CURRENT_CODE_VERIFIED** — прочитан указанный участок текущего кода; **RESIDUAL** — оставшееся противоречие; **PROPOSED_TRANSFER** — способ применить урок в другом пакете. Ни одна из этих меток не означает измеренный прирост Student.

## 1. Восстановить полный функциональный объём, не повторяя исследование с нуля

**Что было не так.** В `previous rounds thinking results.md` прямо записано, что R07 не прочитал назначенные MECH-01, RESEARCH-03, исходный DPO algorithm и относящиеся первичные RAG-источники. Это частичный результат, а не завершённое исследование всей обучающей архитектуры. Сужение к S04/нескольким callers теряет Unit/Flow, граф, DTC/IDE и требования локального обучения.

**Что уже изменено — CURRENT_CONTRACT_VERIFIED.** Исправленный JOIN сохраняет R07_REPAIR_REQUIRED; R01–R25 — переиспользуемые источники. В [52-requirement crosswalk](../R02-52-REQUIREMENTS-32-SLICE-CROSSWALK-2026-10-14.md) назначены владельцы не только DPO, но и восьми task types, Hebrew→English, Figma/Canva, IDE, тарифных бюджетов, приватности и переноса состояния. 52/52 routed не означает 52/52 implemented.

**Как повторить.** Сначала составить строки `requirement_id → actual owner → task → observable consumer → examination → trigger`. Для каждого требования выбранного слайса пройти один нужный RAG MD/NDJSON указатель к первичному G/F/V абзацу; записать прочитанный смысл, не только путь. Для других требований сохранить владельца и триггер, не превращать их в prerequisite первого урока. Не закрывать R07 именем нового документа.

**Контекст/skills.** Original task, task prompt, goal of the round, Алгоритм создания ДПО, WorkLeft MASTER/usage/MECH-01/RESEARCH-03; phase-planning, trainable-architecture-decision, plan-review. **Проверка результата:** требования не исчезли и не переназначены по сходству названий; `INDEX_ONLY` не назван `READ_PRIMARY`. GOOD: RP06 оставлен владельцем DTC; BAD: C# pilot закрывает Figma и Hebrew; EDGE: отсутствующий primary ограничивает только зависимое утверждение.

## 2. Отделить действующие инструкции от исторических требований к процессу

**Проблема.** `task prompt.md` сохраняет историческое «ДПО в последнюю очередь». В architecture/plan-review skills присутствуют 30/50-pass и старые model/ledger предписания. Механическое наследование отменяет нынешнюю цель раннего обучения. Обратная ошибка — удалить вместе с историческим порядком важные права, состояние и четыре роли.

**Что уже изменено — CURRENT_CONTRACT_VERIFIED.** RP01 V6 и RP02 V7.2 явно отменяют противоречащий learning-first порядок, а не предметные требования. RP01 отдельно исправляет превращение старого `TP1_LOCAL_EVAL_ONLY/G_DPO_FINAL` ярлыка в вечный запрет китайского DPO.

**Что осталось — RESIDUAL.** Skills остаются многослойными. Их наличие само по себе не создаёт новый обязательный 30/50-pass gate в каждом RP task. Здесь они не редактировались.

**Как исправлять в других слайсах.** В рабочем prompt указать точную область: текущий пользовательский scope, локальный актуальный контракт, сохраняемые source decisions, исторический фрагмент и разрешённая дельта. Не вводить model/grant ограничения из чужого сценария; настоящие права и достигнутые code guards сохранить. Предел вмешательства — один действительный конфликт, не глобальное переписывание skills. **Проверка:** исполнитель может однозначно назвать свой следующий эффект; старое «DPO_LAST» не добавляет скрытую очередь. Skills: phase-planning, plan-review, conversation-intent-and-self-reflection.

## 3. Оставить один исполняемый dispatcher и согласовать машинный manifest

**DELTA_VERIFIED.** [RP02 manifest до последнего ремонта](https://github.com/lubochka/xiigen-planning/blob/51298617c26a6b3187bf69fbdc96c6f2f718dbca/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/PACKAGE-MANIFEST.json) содержал 17 ID, не содержал RP02.00C и направлял в `tasks/`/V6. [Версия после ремонта](https://github.com/lubochka/xiigen-planning/blob/7278c9c24ff3bd3d2f8176914e717b5897409a01/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/PACKAGE-MANIFEST.json) содержит 18 ID, `tasks-v7/`, V7.2 и условный 00C. Это подтверждает содержанием смысл коммита `7278c9c`, а не только его заголовком.

**Почему важно.** Хорошая карточка бесполезна, когда runner получает старую. Два RP02 пакета — не два независимых владельца E2. Аналогично S20-COMP и S20-NOVEL нельзя объединять по номеру без полного qualified path.

**SPLIT STATUS, VERIFIED 15.10.2026.** Отдельный старый **RP01 root CURRENT V4** указатель против V6 остаётся самостоятельным вопросом владельца RP01, не меняется RP05. **RP05 RESOLVED AT DOCUMENT DISPATCH LEVEL:** раньше V5 manifest заявлял 14 active cards, но первые V5 headers были лишь у 7/14; теперь остальные 7 получили индивидуальные V5.1 direct prompts, исторические V4/V3 явно обозначены, README/start/board/AGENTS/manifest согласованы. См. [F21–F24 + readback](05-NEW-RP05-FINDINGS-AND-CLOSURE-2026-10-15.md). Исправлена инструкция, НЕ алгоритм и НЕ обучение Student.

**Перенос.** Проверить `root → pack README → startup → board → manifest → task/child → AGENTS` и обратные ссылки. Один semantic task должен иметь один исполняемый путь; история — помеченный reference/forwarder. **Проверка:** сравнить множества ID, qualified paths и смысл первого действия, а не только file count. Это проверка документов, 0 новых product tests. Skills: plan-review, prompt-patch-authoring, generated-code-review.

## 4. Заменить «в плане NEW» на доказательство текущего дефекта

**Что изменено — CURRENT_CONTRACT_VERIFIED.** RP01 01A/01B/01C выполняются только по доказанной проблеме живого producer/objective/recipient. RP02 00C создаёт bridge только при `NOT_WIRED`. RP05 01A допускает REUSE_NO_CHANGE для законного готового feedback. Это убирает повторное создание исправного trainer, adapter или UI.

**Как проверять.** Зафиксировать текущие P/K HEAD и relevant blobs; прочитать producer, definition, registration/injection, caller и ordinary consumer. Для старого finding сравнить именно затронутый метод, не только дату коммита. Если код существует, но caller не найден — `CODE_PRESENT / CALLER_UNVERIFIED`, не `IMPLEMENTED`. Если runtime не выполнен — `SOURCE_TRACED`, не `WITNESSED_RUNTIME`.

**Пример.** `KnnLogicalUnit` существует; это не доказывает, что его результат участвует в нужной EN→RU генерации. Установленный Qwen/CPU profile не доказывает создание пары. **Recovery:** получить узкую source trace; ремонтировать только отсутствующий seam. Skills: generated-code-review, code-examination, trainable-architecture-decision. **Выход:** один дефект с методом/consumer или REUSE_NO_CHANGE; 0 новых тестов при чтении/отсутствии изменения.

## 5. Перестроить зависимости вокруг первого причинно различимого урока

**Что изменено — CURRENT_CONTRACT_VERIFIED.** RP01 отделяет trace/exam preparation и только необходимые repairs. RP02 делает 02B1 первым θ learner без ожидания 02A fusion и полного adaptive-k. RP03 начинает Flow 02B по реальному сигналу без ожидания всего typed lane. RP04 экзаменирует E3 ИЛИ E4 после готовности одной ветви. RP05 не требует нового feedback UI для B1.

**Почему это не умаляет kNN.** Изолированный Student DPO опыт отвечает узкому вопросу при фиксированном kNN. Он не доказывает качество всего hybrid. RP02 идёт параллельно как обязательный самостоятельный механизм для будущего domain/hybrid claim; его нельзя отложить «когда-нибудь после DPO».

**Метод переноса.** Для каждой dependency написать тип: источник; input/type; mutable state; права; достоверность экзамена; integration; release. Удалить из раннего пути только несущественную зависимость, оставив successor owner/trigger. `B0 accepted` нужен RP05 mutation, но не source preparation. **Проверка:** путь до одного authorized update не проходит через все слайсы, все домены или Windows release; небезопасный update не разрешён ради скорости. Skills: phase-planning, plan-review.

## 6. Сделать CODEX/Chinese/CODEX роли проверяемыми по байтам и вызовам

**CURRENT_CONTRACT_VERIFIED.** WorkLeft RESEARCH-03 R16, RP01, RP02 и RP04/RP05 закрепляют: CODEX **astra 6 ultracode** выбирает учебный case в human-role; допущенная внешняя открытая китайская модель пишет ОБЕ окончательные стороны; отдельный CODEX **astra 6 ultracode** оценивает точную пару.

**CURRENT_CODE_VERIFIED.** В `FeedbackRouter.cs` EDITED основан на UserCorrection/ModelResponse, а REJECTED содержит no-preferred sentinel. Эти generic feedback records не являются доказательством Chinese BOTH-side authorship. Это не объявление generic feedback универсально неправильным: в данном governed DPO пути требуется отдельный authoring/admission.

**Ремонт в другом слайсе.** Разделить case_id/context/rights, author invocation и exact side digests, затем judge invocation/digest/rubric/verdict. Показать same prompt и существенный contrast. Запретить judge переписывать стороны под чужим provenance. `TIE/BOTH_BAD/UNJUDGEABLE` не отправлять optimizer. Отсутствующий реальный model call — NOT_INVOKED, не «Astra проверил».

**GOOD:** новая Chinese пара по законному TRAIN case, judged unchanged. **BAD:** пользовательская правка переименована Chinese-chosen; внешняя модель оценила себя; Student-rejected приписан учителю. **EDGE:** valid pair не дала gain — сохранить отрицательный результат эксперимента, не фальсифицировать judge. Skills: prompt-patch-authoring, generated-code-review; для исполнителя дополнительно dpo-triple-format, experiment-design.

## 7. Не смешивать стандартный DPO, preference ranker и семантическое преобразование

**Что изменено — CURRENT_CONTRACT_VERIFIED.** RP01 сохраняет exact material, response-only objective и фактический LoRA recipient; RP02 02B1 и RP03 02B называют отдельную pairwise ranking objective. CODEX grade/similarity не заменяет policy/reference log-ratio.

**Как переносить.** В карточке назвать `input representation → loss → trainable parameter set → optimizer → persistent state → reader`. Для standard DPO проверять exact prompt/chosen/rejected, shifted response masks, frozen reference и разрешённые LoRA A/B параметры. Для RP02 θ и Flow W не обещать response-DPO: objective может быть `softplus(-w·(φ+−φ−))` с отдельно определённой регуляризацией. Для LoRA projection назвать matrix/codebook/encoder/decoder и downstream смысл; одинаковая shape не доказательство общего пространства.

**GOOD:** corrected C0/T code одинаков, меняется только разрешённый параметр. **BAD:** judge 0.9 подставлен как log probability; loss уменьшился, но caller читает старое состояние; hidden vector reinterpret cast объявлен tokenization. **EDGE:** при policy=reference стартовый margin равен нулю — это не делает качественную пару недопустимой автоматически. **Проверка:** один локальный changed-risk probe только при изменённой sign/mask/parameter boundary; качество — независимый экзамен. Источники: RESEARCH-03, RP01 V6, RP02 02B1, RP03 02B; [DPO](https://arxiv.org/abs/2305.18290), [LoRA](https://arxiv.org/abs/2106.09685).

## 8. Развести урок, row, atomic group, optimizer step и два смысла слова sealed

**CURRENT_CODE_VERIFIED.** В текущем `ExecuteCapturedPreparedAsync` подготовленный CodeDocumentation путь отклоняет `DpoStepCount != 1`. В `FileDpoPairStore.Transactions.cs::SelectAtomicTrainingUnits` группа CriticUnit/ImproverUnit должна содержать две упорядоченные строки. Поэтому generic configurable step count не разрешает несколько шагов через prepared entry, а «один урок» не означает принудительно одну запись БД.

**Что уже исправлено.** RP05 V4 требует именовать настоящий entrypoint, lesson/group ID, constituent pair IDs, transaction, update и journal. RP01 вводит условный untrained B0 bootstrap отдельно от LoRA learning.

**Дополнительное уточнение — PROPOSED_TRANSFER.** В пакетах встречаются «sealed DPO» (сохранённый immutable TRAIN artifact) и «SEALED» (закрытая проверочная семья). В task receipt писать два разных поля: `artifact_integrity=SEALED_IMMUTABLE` и `data_partition=TRAIN|OP_DEV|EXAM_SEALED`. Запечатанный TRAIN материал не становится экзаменом; закрытый экзамен не разрешается к обучению из-за целостности файла. Нельзя молча мигрировать существующие DTO только ради этих названий.

**Проверка:** реальные значения step/rows/transaction совместимы с reached method; семантически тот же урок с новым ID не replay. **EDGE:** unknown effect после crash — сначала установить terminal state/recover, не повторять gradient. Skills: generated-code-review, phase-planning, plan-review.

## 9. Заменить абстрактный «вектор» версионированным предметным представлением

**CURRENT_CODE_VERIFIED.** `KnnLogicalUnit.Vectorize` суммирует BPE token buckets и hashed predicate buckets, затем нормализует. В этой функции нет явной последовательности, dependency/referent структуры. При одинаковых token IDs/counts и predicate set итог одинаков; один фиксированный bag не является доказательством грамматического анализа всей системы.

**Что изменено в плане.** RP02 V7.2 требует domain-specific φ/ψ: C# AST и nullability, RU POS/референт/полярность, SQL dialect/schema/NULL/cardinality. Минимальный первый опыт использует две реально вычислимые различающие характеристики одного домена, а не ожидает универсальный encoder.

**Перенос.** Указать domain/intent/direction, schema/version, dimensions, units, normalization, UNKNOWN masks, encoder/index/checkpoint lineage и ordinary consumer. Проверить сохранение чисел, отрицаний, порядка и связей. При изменении пространства reindex/compatibility является реальной зависимостью, не optional polish.

**GOOD:** род согласуется с конкретным referent_id. **BAD:** после любого женского существительного запрещён любой мужской pronoun; LoRA названа tokenizer. **EDGE:** неоднозначное «он» сохраняет несколько гипотез, а не ложный hard veto. Skills: trainable-architecture-decision, code-examination; scientific checks — документ 03.

## 10. Проверять domain-law kNN, а не количество найденных соседей

**CURRENT_CODE_VERIFIED.** `ExecuteAsync` делает Search(query, _k), выбирает hits[0]. Остальные hits попадают в evidence IDs, не в отдельное вычисление итогового TOutput. Это конкретный consumer, а не описание всех XIIGEN kNN путей.

**Что изменено.** RP02 V7.2 делает first learner 02B1: lawful same-query candidates, domain-conditioned θ scorer, finite once update, persistence, cold user effect. 02B2 отдельно отвечает за learned adaptive-k; 02A typed fusion не принудителен, когда обычный результат должен быть одним корректным типизированным объектом.

**Предметный контракт.** Сначала rights/tenant/type/partition/schema eligibility; затем source-grounded law и uncertainty; потом learned preference среди разрешённых вариантов; затем final arbiter. Positive, negative, warning memory не смешивать. Предпочтительная соседская запись не получает право обходить arbiter. Heuristic contextual k — `HEURISTIC_ADAPTIVE`; реально обученный θk — отдельное утверждение. Вставка в kNN память может быть непараметрическим обучением, но не доказывает θ update или улучшение ответа.

**Проверка переноса:** независимо сравнить fixed-k reranking, contextual/learned k, representation, exact/ANN и обычный результат. Не усреднять произвольные TOutput и не использовать ReplayFusionRanker другого типа как замену. BAD: «восемь evidence IDs — значит восемь соседей повлияли». Источники: F/ARBITER-KNN-AUTHORITY-AMENDMENT, RP02 runbook и 02B1; [kNN-LM](https://arxiv.org/abs/1911.00172).

## 11. Сохранить самостоятельный смысл Unit и Flow

**Что изменено — CURRENT_CONTRACT_VERIFIED.** RP03 V6.3 отдельно ведёт typed many→many и раннее обучение Flow. 02B требует реально наблюдавшееся независимое same-x OP_DEV preference между двумя F0-lawful вариантами; source passport сам по себе не feedback.

**Как проверять.** Unit: два различимых входа, несколько допустимых TOutput, сохранённые InputRef/OutputRef/associations и настоящий следующий consumer. Flow: фиксированные Unit и kNN, изменяемый W у существующего L2 owner, реальная корректировка связи/выхода, а не делегирование без обучения. Один и тот же вход и одинаковый legal pool для обеих альтернатив. Student LoRA не выдаётся за Flow update.

**GOOD:** оба документа сохраняют nullable смысл, observed preference меняет W, следующий Unit читает скорректированный связанный результат. **BAD:** N копий одного output названы many→many; foreign-tenant вариант обучает F1 как обычный rejected; новый интерфейс добавлен из-за опечатки historical IlearnyngFlow. **EDGE:** есть только один законный вариант/TIE/NaN — NO_UPDATE, другие lanes продолжаются. **Проверка:** 02D durable cold immediately, затем новый unseen family contrast; не ждать всех CERT. Skills: trainable-architecture-decision, generated-code-review, phase-planning.

## 12. Отличать асинхронное исполнение и higher-order graph от обученной политики

**CURRENT_CONTRACT_VERIFIED.** RP04 V4 разделяет E3 policy и E4 motifs. 03B не требует обе ветви одновременно; frozen соседние механизмы сохраняются. **CURRENT_CODE_VERIFIED:** `DynamicGraphMachine` при K97e4 остаётся `honest_untrained`; Train/Evaluate/Export/Import возвращают NOT_ACTIVATED. Наличие его factory/графовых типов не доказывает learned graph lifecycle.

**Ремонт аналогичного слайса.** Назвать настоящие policy weights/route features или order-2/3 pattern state, writer/reader/loader, наблюдаемый feedback и конкретный consumer. Bounded async scheduler сам может быть детерминированным; обучается решение route/order/stop, а не слово async. Невыполненной ветви нельзя автоматически присваивать отрицательную оценку. Отдельно проверить side effects, cancellation и все уже затраченные branch resources.

**GOOD:** безопасный новый order-3 composition лучше выбирается после named state update при том же бюджете. **BAD:** больше edges/рандомный порядок/IsLearned=true названы обучением; выигрыш достигнут запуском десяти ветвей против одной без учёта затрат. **EDGE:** цикл, late completion, revoked rights — безопасный stop/no promotion, не false low-cost success. Skills: trainable-architecture-decision, code-examination, generated-code-review.

## 13. Начинать continual learning с пригодного сигнала, но не терять состояние и приватность

**Что изменено — CURRENT_CONTRACT_VERIFIED.** RP05 V4 REUSE-FIRST разрешает использовать уже законный, source-identifiable feedback без нового UI/DB. B1 mutation требует принятый совместимый parent или отдельно разрешённый initial-parent путь. Remote S05 publication не подменяет локальный RP05 цикл.

**Метод.** Signal→source/consent→novel lesson→Chinese both sides→separate judgement→exclusive trial transaction→independent new/old-family exam→accept/revert/inconclusive→cold ordinary consumer. Сохранять все изменяемые weights/adapters/optimizer/RNG/counters/schema/index и applied IDs; trial и accepted pointer не путать. Репозиторное хранение не разрешает публикацию private user corpus.

**GOOD:** cold reader загрузил принятый digest, B2 использует новую слабость и сохраняет старые навыки. **BAD:** повтор старой пары под новым ID; rollback только weights при новом optimizer; отсутствие checkpoint скрыто новым random model. **EDGE:** crash effect unknown, недоступный remote, stale feature schema — отдельный точный статус и safe recovery. Проверка: changed-risk once/restore/type/rights только где менялся код; новый product test не нужен для прочитанного исправного механизма. Skills: phase-planning, generated-code-review, learning-loop-closure для будущего executor.

## 14. Заменить «до/после» причинным и предметным измерением

**Что изменено — CURRENT_CONTRACT_VERIFIED.** В RP01–RP05 закреплены P original, C0 corrected/no-update, T/U same code плюс named learned state. C0−P измеряет engineering; T−C0 — возможный learning effect. Независимые source families, retention и cost отдельно от storage/loss.

**Перенос.** До первого treatment определить legal TRAIN/OP_DEV/EXAM_SEALED, одну изменяемую причину, data/candidate/decoder versions, domain law rubric, минимально полезный эффект, допустимую регрессию, noise/budget и scope claims. При stochastic decoder повторять сравнимые seeds/budgets; при детерминированном inference важна вариативность независимых задач, а не искусственная случайность.

**Bulk-to-bulk.** Оценивать parent и новый checkpoint на одинаковых новых семьях плюс удержанном retention наборе. Один input с несколькими losers не N независимых наблюдений. Постоянное использование одной закрытой панели для выбора кандидатов превращает её в development; нужна отдельная ограниченно раскрываемая confirmatory family. 95% — eventual owner goal, не входной порог первого урока и не значение confidence.

**GOOD:** NO_GAIN/REGRESS/INCONCLUSIVE сохранён с raw results. **BAD:** T получил новый корпус и другой decoder, весь gain приписан LoRA; после отрицательного результата изменили rubric. **Проверка:** arms действительно существуют; unimplemented arm=NOT_MEASURED; причинные зависимости и next owner записаны. Skills: experiment-design, plan-review, generated-code-review.

## 15. Минимизировать software tests, сохраняя функциональное наблюдение и полный PC cost

**Что изменено.** Source-only, prompt repair, exam design, review и NO_CHANGE — **0 новых product tests**. При реально изменённых deterministic rights/type/mask/finite/journal/rollback/cancel boundaries переиспользуется ближайшая пригодная проверка; новая добавляется только для непокрытого изменённого риска. Нет quota на задачи, % coverage и обязательного широкого Windows run ради learning.

**Почему не «вообще без проверок».** Обучаемая функция может быть недетерминированной, но запрет чужого tenant, запись checkpoint, правильный знак update и journal имеют конкретные инженерные свойства. Независимая компиляция/выполнение сгенерированного C# или SQL также может быть предметным oracle, а не тестированием internal coverage.

**PC ledger.** Замерять wall/CPU, p50/p95 с оговоркой малого N, peak RAM/VRAM, base/LoRA/optimizer/index/graph/cache/rollback disk, cold load, build/reindex и стоимость failed teacher/judge/branch calls. Разделить разовую стоимость и amortized per bulk; обе показать. Победа по одной LoRA file size ничего не доказывает.

**Проверка переноса:** каждая новая проверка привязана к изменённому риску, каждый functional claim — к exam; реальные native/release обязанности остаются на соответствующей поставке. Изолированный blocked Windows release не превращается в global science blocked. Skills: phase-planning, plan-review, generated-code-review. Подгонка тестов под систему остаётся гипотезой до анализа конкретной истории изменения требований/ожиданий; число коммитов не доказывает время CI.

## 16. Переписать конкретную карточку, а не добавлять ещё одну универсальную шапку

**Что уже улучшено.** RP04 V4 заменяет 12 оригинальных карточек standalone prompts; RP01 выделяет direct execution и actor-bound children; RP02/RP03 добавляют предметные действия, формулы, BAD/EDGE и local exam. Это полезнее общего «учитывай качество».

**RESIDUAL.** В прочитанной RP02.02B1 одновременно находятся V7, V7.1 и V7.2 инструкции; RP03.02B тоже сохраняет несколько prompt blocks. Приоритет указан, но model context всё ещё требует их самостоятельно объединить. RP04 README содержит escaped backticks: это читаемость, не доказательство алгоритмического дефекта.

**Как ремонтировать.** Собрать один effective task-phase packet с сохранением всех уникальных source requirements. Один эффект/owner, exact inputs, source spans, scoped skill sections, formula/state/caller, steps, GOOD/BAD/EDGE/WHY, falsifier, recovery, allowed/forbidden writes, минимальный evidence budget, return schema и next owner. Историю оставить reference-only. Большую задачу делить по независимому output/effect: SOURCE → один IMPLEMENT effect → EXAM, а не по произвольным отрезкам текста. Каждый child получает такой же полный договор.

**Проверка.** Ревизор читает packet как новый executor без памяти родителя; может назвать первый вызов/изменение и условия NO_CHANGE/STOP. Проверить markdown, все ID/links/manifest и remote blob readback. Реально отдельный Astra reviewer проверяет точный кандидат; авторский self-review никогда не independent PASS. [Пять конкретных поручений](02-REVIEW-WORK-ORDERS.md) задают этот перенос; они не выполнялись как пять независимых модельных вызовов в данной ревизии.

## 17–21. RP04 V5: дополнение после чтения реальных классов (15.10.2026)

Существующие шестнадцать категорий сохранены; добавлены **пять новых адресных находок**, но не пять обязательных последовательных фаз. [Полный проверяемый F17–F21 packet, exact paths/blobs, falsifiers, GOOD/BAD/EDGE](05-RP04-F17-F21-NEW-VERIFIED-FINDINGS.md).

**17. Default ordinary host не гарантирует чтение обученного состояния — CURRENT_CODE_VERIFIED + OPEN CONSUMER.** `DynamicGraphLogicalOrchestratorCompositionExecutorAdapter.Compose` в default варианте строит honest-untrained machine, а `CreateNonLearningExecutionFlags` отключает learned-unit ветвь. Это не доказательство, что каждый user host использует именно эти флаги; RP04.00A/01C/02C теперь ОБЯЗАНЫ установить actual path и сравнить C0/U в нём.

**18. Higher-order граф не следует из неупорядоченного набора рёбер — CURRENT_CODE_VERIFIED + PROPOSED REPAIR.** `AdaptiveLogicalOrchestrator.LearnedEdgeProjectionRule` именует upstream candidate refs `order-free set`. Новая RP04.02A/02B/02C цепь отдельно сохраняет ordered port/dependency law и доказывает downstream consumer. Отсутствие другого ordered carrier во всём проекте не утверждалось.

**19. Unexecuted branch ≠ наблюдённая negative training example — METHODOLOGICAL RISK.** RP04.01A/01B теперь отмечают `OBSERVED|UNOBSERVED|CANCELLED|DENIED` и допускают предпочтение только между законными сравнимыми наблюдениями, иначе NO_UPDATE. Это защита дизайна, а не найденный сбой конкретного trainer.

**20. Chinese-authored response DPO не заменяет E3/E4 route/motif learner — SOURCE-CONTRACT VERIFIED.** RP04.03A различает exact text `prompt/chosen/rejected` и `θroute/θmotif` ranking сигнал. RP04.00B/01B/02B указывают trainable owner, features, loss, optimizer, journal, cold reader. Нет универсального преобразования текста DPO в обновление графа. Проверять без запоздалого требования ждать RP01.

**21. C0/U на разных host/flags — конфаундинг — PROPOSED SCIENTIFIC CONTROL.** RP04.03B фиксирует actual cold entry, flags, RP02/03, source laws, candidates, budget/hardware; только one θ target меняется. Domain-law eval и ресурсы всех асинхронных ветвей вместо покрытия тестами.

**Вердикт:** у каждой находки есть конкретный RP04 writer, proof/falsifier и право на 0 тестов при неизменённом коде. Публикация этого дополнения ≠ actual train, unseen-domain GAIN или независимый Astra review.

## RP04 F28–F30 · direct code facts, local legality trace and domain-path science

**F28 VERIFIED_CODE:** K/AxisSweepCombinationStrategy.cs cross-products ONE node/edge and four flag profiles, bounded validator requires one NodeSetId. Actual θ config preference may be first honest lesson, but is not E3 structural multi-hop routing or E4 order-3 graph learning. **F29 VERIFIED_METHOD_SCOPE/NOT_PROVEN_EXPLOIT:** checked batch status and unique projected candidate refs do not establish each inner LegalStatus at the inspected methods; locate final guard before reporting any bypass or safety claim, and retain narrow deterministic changed-risk verification only. **F30 SCIENTIFIC_RISK:** fixed-pool C0/U ranker evaluation must be supplemented AFTER first learning by independent fresh-pool unseen domain-law path discovery, actual typed user answer/N and full-PC cost; not another code coverage regime. 00A–05A task prompts were directly repaired. Precise source blobs, owners, GOOD/BAD/EDGE, falsifiers: [F28–F30 annex](11-RP04-F28-F30-NEW-DIRECT-SOURCE-FINDINGS-2026-10-15.md). Existing first-learning 00A→00B→(E3 OR E4)→03B route unchanged.

## Остатки, которые нельзя скрыть словом «исправлено»

| Приоритет | Остаток на проверенном snapshot | Следующий владелец/узкое действие | Что НЕ нужно делать |
|---|---|---|---|
| P0 для затронутого dispatcher | Root RP01 V4 против локального V6/execution; множественные current banners | Planning integrator: привести реальный root forwarder к текущему локальному входу, сохранив историю и уникальный смысл | Новый общий цикл R01–R25 |
| RP05 DOCUMENTARY RESOLVED / runtime OPEN | До V5.1 существовало 7 карточек без V5 direct и ACTIVE V3/V4 headers; сейчас 14/14 имеют эффективный V5/V5.1, прежние помечены historical. F22–F24 остаются **code risk**, не исправлены | RP05.06A: подтверждать точный GitHub readback; RP05.00B/01A/02A/03A: source-trace pair/cache/sentinel/zero-DPO и минимальное предметное вмешательство по правам | Считать repair atlas доказательством Student gain либо удалять реальный `DpoStepCount == 1` guard |
| P1 context cost | Несколько поколений инструкций в одной активной task card и inherited fixed-pass skills | Владелец одной карточки: скомпилировать единственный phase packet, source/requirement diff до/после | Загружать все skills/все 75 tasks слабой модели |
| P0 для конкретного runtime claim | θ scorer, Flow/E3/E4 consumer/training ещё требуют реально исполненных evidence; Graph scaffold NOT_ACTIVATED | Соответствующий code-slice owner, только уже разрешённый минимальный эффект и независимый экзамен | Поднимать статус по manifest/hash/этому атласу |
| P1 semantic ambiguity | `sealed TRAIN artifact` и `SEALED holdout` могут читаться как одно понятие | Data/exam owner: partition и integrity отдельными полями evidence packet | Запретить весь sealed TRAIN или разрешить обучение на holdout |

**Финиш этой работы:** источник-обоснованный атлас и готовые поручения для аналогичной проверки опубликованы. **Не финиш продукта:** перечисленные runtime эффекты, эмпирические приросты, общее исправление всех 75/590 tasks и независимая Astra-приёмка этим документом не подтверждены.

## 17–21. Дополнение RP03 V7 после проверки текущего manifest и K consumer (15.10.2026)

Адресное наблюдение и статус: [05-RP03-V7-FOLLOWUP.md](05-RP03-V7-FOLLOWUP.md). Новые классы проблем: **17 P0** несовместимые current `task_files/active_tasks/AGENTS` V6.2/V6.3; **18 P1** множественные V6.3 prompt blocks у RP03.02B; **19 P0** существующий K L3 selected-flow executor выполняет `flow.ExecuteAsync`, но сам по себе не доказывает typed next L2 `ExecuteAsync`; **20 P1** 00C реальный OP_DEV feedback и три-доменные CERT экзамены должны быть независимыми; **21 P1 HYPOTHESIS** F0-law compatibility feature может взаимно уничтожиться в pairwise g=0 при двух легальных вариантах. В V7 эти точки превращены в отдельные действующие task-child work orders. Это source-grounded ремонт промптов, не реализация K и не измеренный gain.
