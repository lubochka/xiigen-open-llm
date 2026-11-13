# R05 — правильная пара должна дойти до обучаемого решения без потери смысла

**13.10.2026 · CP2 · содержательный исследовательский результат. Общий R05 остаётся OPEN: непрослеженные переходы названы в §8.** Найден и специфицирован конкретный исправляемый seam. Это не запущенное обучение, не изменение product code, не независимая Astra-экспертиза и не доказанное ускорение. Все показатели обучения — **NOT_MEASURED**.

## 1. Главный результат

**Переписывать DPO trainer не нужно. В существующем рабочем пути обнаружены два конкретных несоответствия учебному замыслу:**

1. `LogicalDpoTrainingUnit.BuildBatch` перед токенизацией сворачивает whitespace и обрезает **каждый** из prompt/chosen/rejected до 480 символов. Паспорт при этом связан с исходной полной парой. Следовательно, оценённое CODEX смысловое различие может не попасть в optimizer вообще.
2. `DpoTrainer.ComputeLogProbs` усредняет next-token log-probability по всем непустым позициям **prompt+ответ**, а не считает likelihood полного ответа при заданном контексте. При разной длине сторон вклад prompt не сокращается. Это отличается от response-conditional DPO из действующего plan-07 §3.2a.

**Ближайшее действие:** исправить связку exact-pair → lossless domain-sensitive tokenization → response objective внутри существующих владельцев. Подробный future work order с двумя ограниченными частями находится в [DECISIONS.md](DECISIONS.md). Он не требует нового trainer, четырёх отдельных training stacks или Windows/coverage кампании. Реальный учебный запуск пока не объявляется готовым: его actual lesson/checkpoint/evaluator/ordinary-consumer значения не открыты в этом проходе.

## 2. Источники, область и роли

Research pins: planning P=`0d7ccfac70f189ceb618a6da5c499bd2351beed4`; core K=`dcdded6bb6039c03e80a2786312435350894a4ee`. Перед CP2 core ref перечитан и остался K. Параллельная planning delta до P2=`66b98d1f375f0de9b5bbb708c44770ebda09e467` сохранена; новый `run-prompts/R05-PARALLEL6.md` прочитан и принят как текущая упаковка того же R05. Это не новый проход. CP1 опубликован коммитом `76e6f4083cf8327c2db3b173dd62e26cbd5cc8aa`; его выводы ниже сохранены и уточнены.

Точные repo/ref/path/blob/span и степень чтения — [SOURCES.csv](SOURCES.csv). Обозначения Cxx/Pxx/SKxx в тексте являются строками этого реестра, а не утверждением полного чтения файла. R01–R03 использованы как атрибутированные передачи; их источники не объявлены автоматически прочитанными. Начальный results tree не содержал R04; это не текущий статус другого worker. Ни один sibling не помечен DONE этим R05.

**Распределение ролей неизменно:** CODEX с запрошенной **Astra 6 ultracode**, то есть утверждённой конфигурацией `GPT-6-Astra` / `ultra`, выбирает разрешённое учебное задание вместо человека; допущенная открытая китайская модель создаёт **обе окончательные стороны chosen/rejected**; CODEX той же конфигурации отдельно оценивает точную пару в точном разрешённом контексте; XIIGEN Student обучается. Слабый ответ Student — возможный context автора, а не автоматически final rejected. Судья не переписывает стороны под чужим provenance. DPO reference — математическая опора, не этот судья и не независимый evaluator Student. Эти product calls здесь не выполнялись. Основание: P08 §§2.1–2.3 и P02 §0.

Сохранены четыре функциональные роли: Unit — domain/vector/output schemas и typed many→many; Flow — отношения и коррекция между units; Orchestrator — асинхронные комбинации; DynamicGraph — связи паттернов разных порядков. `Train` принадлежит Machine, Graph — L5 specialization. Выбранный solver не равен обучаемому selector. Сохраняются original DPO/DynamicGraph, DTC и IDE направления, все slices и WorkLeft; очередь целиком прочитана в P09, но весь код этих направлений не исследован в R05.

## 3. Сквозная карта: что действительно прочитано

| Переход | Фактический carrier/owner и источник | Статус, смысл и точный остаток |
|---|---|---|
| Selection → разрешённый контекст | S04 RUN-CONTRACT §§2.1–2.3; `CodeDocumentationOneLessonEndpoint` и названные там Session/SourceSelection owners; P08,C07 | POLICY_READ; вход/recovery прочитан. Реальный saved selection, полный corpus, запуск CODEX и весь producer selection не открыты. Не выдаётся за состоявшийся выбор. |
| Контекст → Chinese-authored exact pair → CODEX assessment | P08; WorkLeft-02 P12; provenance/loader C05,C06 | Действующее авторство обеих сторон установлено. Реальный teacher response и exact assessment не открыты; typed author/hash не заменяет semantic acceptance. |
| Канон → нижний carrier | P04 §§3.1–3.2a; C02,C04,C06,C11 | В плане GraphDpoPair имеет 14 полей; дополнительные diagnostics принадлежат context/sidecar. Нижний LearningTrainingItem имеет 4 поля; Bounded projector сохраняет TeachingVector/Context. В CodeDocumentation rich pair/lease несут provenance, поэтому объявлять всё потерянным по 4 полям нельзя. Полный generic GraphDpoPair→Machine bridge NOT_TRACED. |
| Получатель | C08–C11: request.PolicyModel/ReferenceModel + domain + существующая session/lease | CodeDocumentation обновляет явно переданный PolicyModel, не новый trainer по имени UnitId. `LogicalDpoTrainingUnit.Id` обозначает capability исполнения, не автоматически семантического получателя graph preference. Generic selector recipient/version bridge NOT_TRACED. |
| Exact bytes → prepared execution | C11 `TryBuildLogicalRequest`; C12 `PrepareExecution` | Raw prompt/chosen/rejected и author kinds проходят в `LogicalDpoPreferenceExample`; rich passport остаётся в паре/lease. Default TeachingVector строится из prompt через TrainingVectorBuilder; вектор копируется, ограничивается и проверяется на finite values. |
| Prepared execution → tensors | C12 `BuildBatch`, `Compact`, `ToMask` | **CODE_TRACED_DEFECT F10:** passport по raw bytes, обучение по whitespace-collapsed первым 480 символам каждой части; маска включает prompt. |
| Tensors → loss → update | C12 `ExecuteWithExclusivePolicyLeaseAsync`; C01 `StepVector`/scorer/backward | Existing capability вызывает StepVector и AdamW. **CODE_TRACED_OBJECTIVE_MISMATCH F11** и current vector heuristic F12. Это кодовая трасса, не real-run. |
| LoRA/reference/optimizer | C03,C12,C16,C17 | A/B, seeded application, independent full reference, RequiresGrad=false у reference, восстановление optimizer/RNG и per-policy exclusion прочитаны. Точная итоговая trainable group base/shared/project и dtype во всём пути не сертифицированы. |
| Trial → quality → publication | C09 `TrainAndPublishCoreAsync`; C10 | Есть single-pair lifecycle, reload trial checkpoint, отдельная baseline/trial quality decision, публикация только через текущий owner. Already-applied pair отвергается, существующая попытка возобновляет решение. Не запускалось. |
| Publication → обычный пользовательский ответ | C18 `LocalModelRepository.LoadWeightsAsync` | Прочитан generic stream loader; это **не** доказательство того, что обычная CodeDocumentation generation выбирает опубликованный checkpoint/adapter. Точный обычный caller/selected version остаётся NOT_TRACED. Evaluator trial reload не подставляется вместо этого перехода. |

Таким образом, впервые локализованный разрыв — не «DPO отсутствует», а **несоответствие оценённого материала фактическому training input/objective**. Отдельно сохраняется незакрытая проверка ordinary consumer. Чинить дважды существующие coordinator, optimizer journal или seeded adapter preparation не предлагается.

## 4. Findings с сохранением CP1 IDs

Для каждой строки origin=`(R05, ID, CP2)`; primary locators раскрыты в SOURCES. «Действие» — адресная передача, не выполненный патч.

| ID | Наблюдение / статус | Основание | Действие и непринятый остаток |
|---|---|---|---|
| R05-F01 | Реальный DPO-like policy/reference backward и AdamW уже реализованы. CODE_READ. | C01,C09–C12 | R18 переиспользует; не объявляет generic graph learner или реальный урок готовым. |
| R05-F02 | `Accuracy=margin>0` и loss относятся к обучающей паре до шага. | C01 StepVector; C12 result | R13 не использует их вместо независимого after quality; фактический gain NOT_MEASURED. |
| R05-F03 | TeachingVector не является предсказываемым выходным вектором. | C01 VectorSignal; C12 PrepareExecution | R04/R06/R13 различают output representation и pair weighting; semantic projection НЕ доказана. |
| R05-F04 | В LoraLinear A=rank×in, B=out×rank; base(x)+scale·dropout(x)·Aᵀ·Bᵀ. | C03 whole class | R04/R13 проверяют semantic alignment отдельно. Полный итоговый freeze/shared/project scope ещё не прослежен; constructor сам не является доказательством freeze. |
| R05-F05 | 14-field Graph canon и four-field item не доказывают потерю адреса сами по себе; CodeDocumentation имеет explicit PolicyModel и rich lease. | P04,C02,C04,C11 | R07 прослеживает generic recipient bridge; нельзя автоматически назначить winner solver получателем. |
| R05-F06 | Typed authors и raw text fingerprint — техническая связность, не semantic acceptance. | C05,C06,C11,C12 | R08 сохраняет exact CODEX judgement; actual pair/assessment NOT_OPENED. |
| R05-F07 | Endpoint сохраняет RecoveryRequired при неопределённых эффектах; terminal readback не повторяет урок. | C07 | R09 сохраняет recovery; полная end-to-end crash recovery здесь не выполнена. |
| R05-F08 | Server endpoint вызывает существующий publication coordinator и передаёт tokenizer/LoRA/quality context. | C08 | R18 не создаёт параллельный server pipeline; observed evaluator configuration остаётся неизвестной. |
| R05-F09 | Coordinator→server executor→Stable executor→Logical unit→StepVector→trial reload→quality→publish действительно связан кодом. | C09–C12,C01 | R13/R18 стартуют от этой цепи, а не от нового trainer. Ordinary consumer после publish всё ещё NOT_TRACED. |
| R05-F10 | **Lossy Compact на реальном optimizer path:** raw pair passport не предотвращает потерю хвоста/whitespace до Encode. | C11 TryBuildLogicalRequest; C12 PrepareExecution и BuildBatch/Compact | R18 часть A; R04 получает domain-sensitive tokenization implication; исправление ещё не сделано. |
| R05-F11 | **Mean по полному prompt+answer отличается от conditional whole-response objective.** Binary masks покрывают и prompt. | C01 ComputeLogProbs/BackwardLogProbs; C12 ToMask; P04 §3.2a | R12 фиксирует явный objective; R18 часть B. Публичный average inference scorer нельзя менять незаметно вместе с DPO. |
| R05-F12 | VectorSignal использует только число активных координат и clamp [1,2]; значение/семантика координат не выбирают target. Gradient умножается на signal, reported loss этим же signal не взвешен. | C01 StepVector/VectorSignal | R13 отдельная ablation unweighted vs current heuristic после lossless seam; R04/R06 не считают это learned domain metric. |
| R05-F13 | Step snapshot хранит параметры, gradients/flags, AdamW и переданный RNG; он явно in-process, не durable restart proof. | C16; C10,C11 durable artifact callbacks; C12 prepared transaction | R09/R18 сохраняют оба слоя. Graph/index/project state нельзя объявлять восстановленным только этим snapshot. |
| R05-F14 | **S06 уже имеет multi-lesson campaigns/learning curves:** S04 one-update-total не вечный лимит. S05 переносит существующие артефакты без нового обучения. | P10,P11,P13 | R16/R18/R22 переиспользуют S06.08B/S06.09C; bulk не повторяет старые пары и не создаёт безлимитную новую кампанию. |
| R05-F15 | В plan-07 остались pseudocode `NodeOrEdgeId=winner.UnitId` и запрет teacher-vs-teacher final pair. Это текстовые противоречия с целевым selector recipient и current Chinese-both-sides policy, не доказанный runtime bug. | P04:380–460; P08; P02 | R16/R17 применяют точную semantic поправку с учётом consumer. R12 не переоткрывает уже решённое авторство. |
| R05-F16 | Прочитанные старые skills всё ещё требуют test-count layer / автоматический Tier3 и называют SCALE_UP подтверждением алгоритма. Эти правила нельзя переносить в текущий documentary/learning-first R05. | SK07,SK08 + G03 | R16 согласует только конфликтующие нормы. Нехватка данных означает INCONCLUSIVE, а не доказанную правильность и не обязанность бесконечного rerun. |

## 5. Математический walkthrough и смысловые контрпримеры

### 5.1 Что именно обучается

Для одной и той же разрешённой задачи x и двух окончательных авторских ответов y+ и y− каноническая цель из P04 и Rafailov et al. (EXT01):

`m = beta * [(log pi(y+|x) - log pi_ref(y+|x)) - (log pi(y-|x) - log pi_ref(y-|x))]`

`L = -log sigmoid(m)`.

Action здесь — полный ответ либо согласованная сериализация полного structured output. Для autoregressive модели `log pi(y|x)` — сумма conditional next-token log-probabilities именно response tokens; x доступен как context, но не как обучаемая сторона пары. Judge preference score выбирает/оценивает пару и не подставляется вместо этих likelihoods. Для произвольной vector distance вероятность автоматически не возникает: pairwise/ranking objective нужно определить отдельно, сохраняя preference-learning цель.

В C01 beta=0.1 — факт текущего кода, не рекомендация оптимального гиперпараметра. Reference создаётся/проверяется на исходном принятом состоянии для выбранного урока и замораживается численно; refresh policy между уроками должна сохранять существующий lifecycle, а не незаметно переноситься внутрь шага.

### 5.2 Почему текущее усреднение имеет значение

В упрощённом случае общего неизменного token prefix пусть Sx — сумма policy-minus-reference log-probabilities prompt positions, S+ и S− — суммы по ответам, n — соответствующие числа scored positions. Текущая full-sequence mean даёт:

`m_current = beta * [(Sx+S+)/(nx+n+) - (Sx+S-)/(nx+n-)]`.

При `n+ != n-` коэффициенты перед Sx различаются, поэтому общий prompt не сокращается. Whole-response conditional DPO даёт `beta*(S+−S−)`. Даже при одинаковых длинах mean вносит length-dependent scaling. BPE boundary также надо устанавливать по фактическому serializer/tokenizer: нельзя без проверки брать длину отдельно encoded prompt за границу в совместно encoded строке.

Это аналитический вывод из C01/C12, **не выполненное сравнение качества**. Исправление response mask и reduction должно согласованно затронуть forward/backward. `EvaluateAverageNextTokenLogProbability` имеет собственный публичный average contract — сохранить его, не менять глобально при исправлении DPO. Другая осмысленная length-normalized цель возможна только как явно названный и сравненный вариант, не под названием standard DPO.

### 5.3 Потеря различия до обучения

Следующие примеры — сконструированные иллюстрации дефекта, **не production DPO-пары и не заявления о Chinese authorship**.

**Хорошая проверка содержания:** две разные разрешённые авторские стороны имеют одинаковое вступление длиннее 480 символов; в хвосте chosen правильно описывает отмену операции, rejected ошибочно обещает пустой результат. Текущий Compact может сделать их одинаковыми строками/tensors. На новом кодовом примере проверяется понимание отмены, а не воспроизведение этой формулировки.

**Второй контрпример:** для source-sensitive задания различие между строковыми литералами `"a  b"` и `"a b"`, либо между разными отступами в Python, семантично. Whitespace collapse может стереть его даже без overflow. Аналогично нельзя сортировать meaningful OrderedPatternIds ради candidate-set permutation invariance: порядок поступления кандидатов не равен порядку действий внутри кандидата.

**Плохое подтверждение:** raw SHA совпал, dtype/shape верны, loss уменьшился или файл весов изменился — значит пара дошла правильно. AdamW decay/RNG могут менять численные значения и при утраченном различии; это не полезное предпочтение. Не требовать изменения всех LoRA параметров после первого шага: например B=0 влияет на ранний градиент A.

### 5.4 Zero margin и честная граница поиска

При одинаковых conditional probabilities policy/reference `m=0`, `L=log(2)`, а `dL/dm=-1/2`. Это не семантический дефект пары. Ненулевой градиент по нужным параметрам всё равно требует различающихся сторон, действующих masks и реального differentiable path. В текущем коде policy forward использует training=true, reference=false; при dropout одни лишь одинаковые веса не гарантируют одинаковых реализованных scores. Математический cold-start пример предполагает одинаковое распределение/scoring, а не утверждает наблюдённое значение runtime margin.

В прочитанных Step/StepVector/RequirePairAdmission нет отказа по положительному pre-update margin. **Глобальное отсутствие PE1.2p5/LOW_CONTRAST consumer не установлено.** R02 сообщил о плановом конфликте; точный consumer за пределами прочитанной цепи остаётся NOT_TRACED, а не PLAN_ONLY_CONFIRMED. R12 получает это ограничение и не удаляет неизвестный guard вслепую. TIE/BOTH_BAD/UNJUDGEABLE, невидимый контекст и stale assessment не превращаются в arbitrary preference ради первого шага.

## 6. Hybrid алгоритм: что не должно исчезнуть при исправлении

| Блок | Детерминированная часть | Обучаемое решение и сигнал | Что должно оцениваться отдельно |
|---|---|---|---|
| Unit | Корректная schema/serialization, domain laws, exact operations | Domain mapping/projection, response/action preference, нужные параметры конкретного recipient | Typed many→many результат на новых source families; ложные домены/abstention; LoRA vs no-adaptation; time/memory |
| Flow | Совместимость портов/версий, допустимость переходов | Выбор связей и коррекция одного выхода в следующий вход | Units зафиксированы, меняются отношения; полный lawful результат и цена всех веток, не только финального solver |
| Orchestrator | Lifecycle, cancellation, resource/authorization bounds | Асинхронная комбинация и выбор следующего полезного шага | Success under budget, latency tails, лишние ветки/ожидания, credit assignment; случайный порядок завершения не обучающий target |
| DynamicGraph | Идентичность/типизация, допустимость структуры, laws | Отношения паттернов разных порядков и query-dependent route | Семантический порядок, unseen composition, retention; адрес обучения selector, а не автоматически выбранный solver |

Четыре строки — сохранённые обязанности, не четыре выполненных реализации. Для kNN требуется разделить representation, candidate retrieval, query-dependent metric/k/radius и final arbiter. Learned distance может менять предпосылки существующего index; корректность и стоимость reindex нельзя скрывать. Этот design выбирает R06 по реальному коду, а не R05 по удобной формуле. В LoRA matrix transpose — операция линейной алгебры, не автоматическое преобразование в правильный domain token. Нужны versioned input/output coordinates и downstream semantic examination; shared base/project adapters не должны смешиваться между проектами. Наличие компактных A/B не доказывает PC-fit: память base/reference/optimizer/activations/index и одновременных consumers также считается.

## 7. Передача измерений R13: один урок сначала, широта — следующими разрешёнными уроками

**Не новый универсальный benchmark и не coverage.** Первый опыт использует существующий CodeDocumentation lifecycle после исправления faithful input/objective, с фактическим allowed lesson и отдельными evaluation families. Затем переиспользуются S06.08B/S06.09C для разных новых уроков; S07 сохраняет собственные сравнительные arms/rounds, Builder и subjects. Никакого переноса S04 one-update-total на все кампании.

| Направление, сохраняемое в программе | Domain law / пример смыслового различия | Будущее наблюдение, не результат R05 |
|---|---|---|
| Перевод EN↔RU | Отрицание, числа, единицы, сущности, смысл вне literal similarity; оба направления | Эквивалентность смысла на независимых families, domain confusion, время/retention |
| Orchestration | Preconditions/postconditions и смысловой порядок; lawful короткий путь vs бессмысленный короткий | End task success + sum cost всех branches; фиксированные units при изменении learned relations |
| Figma→HTML/CSS | Декларированные layout/accessibility constraints и поведение разных размеров | Semantic/visual constraints на новых design families + render/route/full cost; не один screenshot hash |
| C# documentation — первый ограниченный сценарий | Реальная cancellation/exception/nullability semantics, точные code literals | Correct explanation на других methods; harmful assertions отдельно от fluency; ordinary selected checkpoint |
| Angular documentation | Реальные component/template/binding/lifecycle semantics и version | Другие component families; correct domain/context mapping + useful documentation |
| SQL documentation | NULL/duplicates/order/transaction semantics конкретного dialect | Различение lawful outputs и explanations; иной schema/query family, не смена имён |
| Самостоятельный DPO Builder | Обе стороны China-authored; valid useful chosen и genuinely inferior rejected в одном контексте | CODEX exact-pair acceptance/uncertainty, novelty, downstream student gain per accepted new lesson; admission rate не gain |

Это anchors для семи направлений, не претензия на полную проверку их dossiers; extras/другие языки/IDE/DTC границы сохраняются владельцами R04/R06/R08/R21–R25. Они не все prerequisites первого ограниченного урока.

**Сравнение и единицы независимости.** До update зафиксировать operating panel и rubric, тот же accepted baseline и matched no-update control с тем же разрешённым memory/context и сопоставимым compute. Candidate получает один новый допущенный урок. Не смешивать добавление памяти с изменением параметров; memory-only эффект выделяется. Единица независимости — source family, не N−1 пар из одного исходника и не переименованная копия. Финальная закрытая panel не идёт автору/выбору k, rank или threshold. Уже раскрываемую operational panel нельзя называть новым unseen certification.

**Что записывать от bulk к bulk:** качество/accuracy и lawful completion по доменам; confusion/abstention при mapping; route success и избыточные действия; latency p50/p95 с достаточностью выборки; authoring/assessment/train/evaluation/cold-load/reindex и failed branches; RSS/VRAM/storage, accepted trainable parameters и retained knowledge. Отдельно code/state effect, semantic pair acceptance и user-facing usefulness. Все поля сейчас NOT_MEASURED; hardware, sample precision и numeric budget не выдуманы.

**Условие вывода.** Применяется существующая noise-aware policy с task-derived precision и минимально значимым эффектом, определёнными до просмотра candidate outcomes. Нет универсального нового 95% или обязательного роста каждого bulk. Ухудшение — текущий full-state reject/restore; indistinguishable/недостаточно families — INCONCLUSIVE и прежняя принятая версия; неподходящая/потерянная pair/context, wrong consumer или неизвестные эффекты — INVALID_EVIDENCE/recovery, не повод повторять градиент. Полноценный negative result сохраняется. Отдельная CODEX/Astra expertise должна объяснить domain errors и выбор дальнейшего пути; model label без фактического вызова не считается выполненной экспертизой.

## 8. Непрослеженный остаток и ограниченные владельцы

| Остаток | Точное следующее чтение/owner | Влияет ли на ближайшее действие |
|---|---|---|
| Actual saved selection/authoring/exact CODEX assessment, законный lesson и evaluator/run budget | R08/R18: current S04 RUN-CONTRACT §2.3 → held Session/SourceSelection и `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.cs`; реальные artifacts | Да для live lesson; нет для документирования двух доказанных кодовых дефектов. |
| Ordinary selected-checkpoint generation caller, не trial evaluator | R05 continuation/R18: `RepositoryOwnedModelCheckpointLoader` callers и текущая CodeDocumentation production composition; R09 для lifecycle | Да для признания useful learning; stream loader C18 недостаточен. |
| Generic GraphDpoPair→Machine session→selector recipient/Context bridge | R07 с P04,C02,C04,C11; R06 даёт настоящий selector/arbiter | Не требуется переписывать CodeDocumentation путь; требуется до переноса урока на graph/flow. |
| PE1.2p5/LOW_CONTRAST actual consumer вне прочитанных методов | R05 continuation/R12: primary rule locator из R02 и его exact producer/consumer; не default-branch absence search | Не удалять непроверенный guard. Холодный counterexample и loss/projection findings уже пригодны для работы. |
| Итоговые base/shared/project trainable groups, dtype/rank, output coordinate/index version | R04/R06/R18: current LoraApplicator call continuation, LlmModel checkpoint entries, representation/index owners | До реального изменения соответствующего state; не основание создавать новый LoRA stack. |
| Дополнительные original/RAG/RESEARCH-03 primary spans нового PARALLEL6 prompt | R05 continuation адресно по его прямым входам; broad catalog остаётся R01–R03 | Честный read-depth gap. RAG-00→WorkLeft-02/INTEGRATION/TASK-PROMPTS и первичный code path уже прочитаны; весь RAG не заявлен. |

**Что можно передавать уже сейчас:** F10/F11 и work order R18; F03/F12 и coordinate implications R04/R06; F05/F15 R07/R16; recipe R13; source snapshot/limits R10. **Что нельзя объявлять:** полный R05 DONE, выполненный live patch, закрытый S04/S06, обученный graph, PC-fit, улучшение, независимую Astra экспертизу или отсутствие всех runtime margin gates.
