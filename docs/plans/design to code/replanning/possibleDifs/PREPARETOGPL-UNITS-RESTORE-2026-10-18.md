# Unit, Flow и восстановление: точные отличия от PrepareToGPL и новые проверяемые гипотезы

Дата аудита: 18.10.2026. Статус: **исходники проверены; новые опыты не запускались**. Эта записка охватывает generated capability из проекта 18, её обычный Flow и materializer orchestration reasoning из проекта 43. Она не утверждает, что любой из этих путей исполнялся в неудачном RP01.

Снимки: **PrepareToGPL `31346d8928be4469b01aaf313780e8f950ff91e2`** и **codex/dpo-implementation `a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6`**. Ссылки ниже закреплены на этих SHA. Исходники сохранены для проверки в `audit/units_src/{old,current}`.

**Дополнительная сверка перед публикацией, planning a4a3e709.** Новые N40–N42 не дублируют точные U-N1/U-N2/U-N3 цепи. [N41-T3](41-TRANSLATION-TOPICDOMAIN-INPUT-ERASURE-AND-MAP-COLLISION-2026-10-18.md) уже описывает потерю topic-domain у другого Translation Executor, поэтому новая здесь только конкретная generated-capability связь. [G40-02](40-PREPARETOGPL-RECOVERY-PRIORITY-CHAPTER-2026-10-18.md) устанавливает пустой admission ledger shipped orchestration dataset: для него U-N3 остановится до BuildPattern и имеет статус NOT_REACHED / BLOCKED_BY_ADMISSION. Диагностика override возможна лишь на другом допустимом fixture/producer; это не обнаруженный вред данного заблокированного пути.

## Что исследовать первым для восстановления поведения PrepareToGPL

В этом участке найдены реальные изменения восстановления и представления памяти. **Не найден сохранённый результат успешного запуска именно старого DefaultLogicalCapabilityTrainer**, поэтому здесь нет утверждения «этот diff уже доказан как причина измеренного регресса». Успешное обучение в PrepareToGPL, о котором сообщает пользователь, задаёт приоритет археологии, но его нельзя без связанного receipt приписать любому классу ветки.

| Очерёдность | Проверка | Что действительно меняется | Что пока неизвестно |
|---|---|---|---|
| 1 | U-R1: прежняя память при холодной загрузке | PrepareToGPL собирает историю manifest того же домена; нынешний loader берёт подписанный `KnnExamples` конкретной версии | Зависел ли конкретный старый успешный ответ от строки, исчезнувшей из нового корпуса |
| 2 | U-R2: принятие старого checkpoint bundle нынешним loader | Старые generated descriptors/manifest не удовлетворяют нынешнему executable-v2 контракту | Был ли такой старый bundle реально использован при переходе на RP01 |
| 3 | U-N1: одинаковый запрос в двух разных subphase | Factory выбирает newest unit домена, а scorer не читает требуемую subphase | Теряет ли ordinary многошаговый запрос правильность именно из-за этого |
| 4 | U-N2: объединение нескольких ветвей в один KNN документ | В старом live пути переданы branchPlans; теперь registrationPlans после группировки | Помогло или повредило покрытию запросов, ведущих к нужной subphase; старый duplicate-ID дефект обязателен как контрфактор |
| 5 | U-N3: DPO features и предварительно записанный learned_pattern | Нынешний materializer может взять готовый pattern вместо вывода из features | Есть ли такой режим в реальных исходных учебных payload; semantic learning этим API не обещано |

U-R1 — **уточнение уже известной семьи N27/P24/P92**, а не новая находка. U-R2 — конкретная проверка совместимости, а не новая формулировка общей просьбы «проверить restore». U-N1 — новая конкретная связь caller→registry→retrieval. U-N2 — дополнительная гипотеза гранулярности. U-N3 — **диагностика контракта materializer**, её нельзя назвать доказанным повреждением trainer.

## U-R1. Холодное восстановление теперь восстанавливает другой набор KNN примеров

**Уровень:** память примеров и построение ANN index; не LoRA rank, не tokenizer, не веса матриц.

### До и после

В PrepareToGPL `TeachingRegistryPersistence.LoadGeneratedCapabilityUnit` собирает `allManifests`, отбирает тот же `DomainTag` и `CreatedAtUtc <= manifest.CreatedAtUtc`, затем создаёт `TransformerKnnTrainingExample` из каждой записи. Короткая цитата: `var examples = allManifests.Where(candidate => ...)`. [Старый loader, 295–382](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/18.CapabilityTraining/TeachingRegistryPersistence.cs#L295-L382).

Теперь `LoadGeneratedCapabilityUnit` вызывает `ProjectBoundKnnCorpus(manifest)` и восстанавливает индекс из **`knnCorpus.Value.Examples`**. Это обеспечивает совпадение с подписанной памятью данной версии. [Нынешний loader, 1605–1724](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/TeachingRegistryPersistence.cs#L1605-L1724).

Но при создании новой версии `BuildKnnTrainingCorpus` копирует только плоскую строку каждого выбранного replay manifest плюс текущие registration rows; вложенные `manifest.KnnExamples` не раскрываются. Replay selector выбирает максимум три прежних manifest. [Создание корпуса, 3467–3508](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L3467-L3508), [выбор replay, 1790–1882](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L1790-L1882).

**Вывод по коду:** старое холодное восстановление могло иметь более широкий банк памяти, чем старый live Unit и нынешняя точная версия. Нынешний loader сам по себе не теряет подписанные примеры; различие создаёт политика формирования нового банка. Поэтому ремонт loader, который молча подмешивает все найденные файлы, был бы неправильным: он опять сделал бы state identity зависимой от окружения.

**Существенный контраргумент:** старые ExampleId строились только из subphase. Несколько исторических manifest одной subphase могли иметь один ID. Старый `Train` добавлял все vectors в HNSW, но перезаписывал `exampleMap[id]` последней записью; поэтому «all manifests» не равно «все старые варианты доступны корректно». [Старое построение IDs, 753–778](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L753-L778), [старый Train, 88–103](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L88-L103). Возвращать эту коллизию не нужно.

### Небольшой опыт U-R1

Взять один проверенный старый successful bundle с минимум четырьмя различными законами **одного настоящего DomainTag**, и сперва установить, на каком обычном input он действительно работал. Для первого структурного опыта использовать разные subphase/ExampleId, чтобы старое перезаписывание ID не было скрытым фактором.

A — нынешняя подписанная память выбранной версии. B — тот же policy/tokenizer, но в разрешённой экспериментальной версии памяти добавлен ровно один прежний проверенный пример, который исторический cold loader включал, а A не содержит. Никаких новых optimizer steps. Индекс каждой версии строится тем же кодом; остальные scoring coefficients неизменны.

Панель: 4 новые формулировки закона добавленного примера, 4 запроса остальных законов, 4 неприменимых/противоположных условия. Измерить correct/all, wrong accepted, отказ, выбранный ExampleId, контент ответа, memory size и latency. Обычный caller — `TeachingSessionBuilder.OpenReadOnlyServing` → реальный зарегистрированный Flow, с допустимым verifier/manifest; если нет легального importer для старого bundle, получить **INCOMPATIBLE**, а не менять подпись вручную.

Гипотеза поддержана, если B возвращает прежний правильный закон через настоящий consumer и сохраняет остальные cases. Если missing example не нужен для ответа или ни разу не выбирается, причина этого падения не подтверждена. Успех только на точном старом prompt отдельно обозначается recall, не обобщением.

**Конкретный ремонт после положительного результата:** держать retrieval corpus независимым от бюджета optimizer replay; в `BuildKnnTrainingCorpus` брать проверенный предыдущий signed corpus и явно выбранную политику retention, записывать новый canonical corpus hash, перестраивать индекс и сохранять его состав в manifest. Replay DPO может остаться маленьким. Перенос не снимает проверки provenance, duplicate IDs, размера и доменного scope.

**Дедупликация:** [существующий N27](https://github.com/lubochka/xiigen-planning/blob/81a0f456ef318d812a55c3d07185c69615fe2fdd/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/27-NEW-REPLAY-APERTURE-DPO-KNN-DIVERGENCE-2026-10-16.md) уже установил top3/flatten/cap256; [P92.3](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/cards/P92-dpo-memory-flow-bridge.md) задаёт cold parity. Новое здесь — точный baseline PrepareToGPL и оговорка про старые одинаковые IDs.

## U-R2. Старый успешный bundle нельзя просто открыть новым generated-capability loader

**Уровень:** descriptor/manifest/import, наличие обученного состояния в runtime. Это может выглядеть как «модель забыла», хотя веса вообще не загрузились.

Старый trainer регистрировал generated unit с `isLearned: true`; старая запись `GeneratedCapabilityCheckpointManifest` не имела `SchemaVersion`, `KnnExamples`, `ReplayAttestation`, `ExecutableManifestAttestation`, model/tokenizer hashes и полного positional configuration. [Старая регистрация, 332–349](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L332-L349), [старый manifest, 46–88](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/18.CapabilityTraining/TeachingRegistrySnapshot.cs#L46-L88).

Нынешний `IsExactGeneratedCapabilityBinding` требует `!entry.IsLearned`; `BuildManifestRuntimeIdentity` требует schema-v2, `RopeTheta`, `RmsNormEps` и hashes. Для executable restoration нужны также подписанные memory/executable bindings. [Проверка descriptor, 2878–2900](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/TeachingRegistryPersistence.cs#L2878-L2900), [проверка runtime identity, 1746–1788](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/TeachingRegistryPersistence.cs#L1746-L1788), [executable admission, 1825–1888](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/TeachingRegistryPersistence.cs#L1825-L1888).

Нынешний `TeachingSessionBuilder.OpenCore` передаёт snapshot этому loader и возвращает его ошибку. Он не мигрирует старый bundle в данном пути. [Caller, 299–350](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/15.OrchestratorModel/Logical/TeachingSessionBuilder.cs#L299-L350).

**Что доказано:** контракт несовместим без отдельного перехода. **Что не доказано:** что приложение при этом молча создавало пустую модель; рассмотренный caller явно отказывает. Само исправление `IsLearned` с true на false честно отличает fixed retrieval от обученного решения и не является изменением математики KNN.

### Небольшой опыт U-R2

Одна копия реального старого bundle, без редактирования его доказательств. Сначала открыть прежним loader и записать Unit IDs, weights/tokenizer identity и 8 ordinary outputs. Затем подать **ту же** копию нынешнему sanctioned import/loader и записать точный отказ или успешное восстановление; zero learning.

Если отдельный утверждённый importer существует, его выход должен создать новую подписанную версию на основе проверенных старых bytes и явно восстановленных config/corpus, сохранив исходник неизменным. Сравнить те же 8 outputs и ещё 4 отрицательных scope cases. Hash mismatch/admission failures не отключать. Не изобретать отсутствующие teacher receipts и не ставить `IsLearned=false` вручную ради успешной проверки.

**Ремонт:** отдельный, явно названный importer PrepareToGPL→executable-v2, а не ослабление ordinary loader. Если исторический корпус или происхождение невозможно удостоверить, сохранить его как legacy evidence и измерить перенос в отдельном разрешённом recovery run. Пока данные не найдены, статус этой проверки — **CONTRACT_INCOMPATIBILITY, causal impact unknown**.

## U-N1. Разные subphase одного домена обращаются к KNN с одинаковой задачей

**Уровень:** вход Unit и привязка Flow к задаче. Это новая конкретизация по сравнению с общей P94 cardinality/P92 bridge.

### Реальная последовательность

1. `AdaptiveLogicalOrchestrator.ExecuteBindingAsync` записывает `routed_sub_phase_id` в metadata, но в `LogicalFlowRequest<string>.Input` для каждого binding передаёт **исходный `request.UserRequest`**. Предыдущие результаты также лежат в metadata `context.packet`. [Caller, 4131–4204](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs#L4131-L4204).
2. `GeneratedCapabilityFlowFactory.Create` выбирает `ResolveLatest<string,string>(GeneratedCapabilityUnitRole, _unitDomainTag, Probationary)`. **SubPhaseId/TaskType не участвуют в ключе выбора.** [Factory, 7311–7337](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L7311-L7337).
3. Flow компилирует `request.Input` и передаёт canonical text в Unit. Metadata передаётся, но `TransformerEmbeddingKnnLogicalUnit.ExecuteAsync` использует для embedding/reranking только `request.Input`; поля metadata не читаются. [Flow, 7383–7399](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L7383-L7399), [Unit, 169–210](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L169-L210).
4. Flow после непустого результата возвращает `PASS` и добавляет собственные subphase/task/predicate annotations. Они описывают **назначенный Flow**, а не доказывают соответствие выбранного примера этой задаче. [Выход, 7420–7471](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L7420-L7471).

**Строгое условное следствие:** два Flow с разными subphase, одним DomainTag, неизменным registry и одним UserRequest передадут одному latest Unit один и тот же scoring input. Требуемая subphase и upstream result не могут изменить его выбор через metadata. Речь об одинаковом **retrieved output**, не о полностью одинаковом финальном тексте: annotations отличаются.

Например, один запрос «подготовить изменение API и проверить отмену» исполняет две ветви одного development domain. Ветвь реализации и ветвь проверки могут получить один и тот же stored capability output, хотя должны выполнять разные обязанности. Если downstream нужен результат предыдущей проверки, изменение только `context.packet` также не влияет на это retrieval. Это не утверждение, что все запросы обязаны иметь два ответа: для единственного subphase такого конфликта может не быть.

### Что было в PrepareToGPL

Выбор newest domain Unit и input-only query уже существовали. Поэтому **сам отсутствие scope — inherited gap**. Но старый обычный Factory передавал topology; `TranslateResult` вызывал `CapabilityResultTranslator.Translate`, а старый `Translate` **вообще не использовал `knnOutput`** и строил текст из subphase metadata. Это могло скрывать неверный retrieval в старом пользовательском тексте. [Старый Factory, 979–1013](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L979-L1013), [старый TranslateResult, 1121–1134](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L1121-L1134), [старый translator, 11–25](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/18.CapabilityTraining/CapabilityResultTranslator.cs#L11-L25).

Нынешний translator, напротив, сохраняет output и дописывает annotation. [Нынешний translator, 56–87 и 118–137](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/CapabilityResultTranslator.cs#L56-L137). Восстановление старой подмены metadata не восстановит обучение.

### Опыт U-N1: две subphase, обычный consumer

**Домен:** C# development orchestration; дополнительная панель SQL допускается отдельно, если обе её обязанности принадлежат одному реальному DomainTag. Здесь не тестируется EN→RU free generation.

Сначала короткий read-only диагностический запуск: две разрешённые subphase одного домена, один query, неизменная память. Через реальный `ExecuteBindingAsync` записать фактические flow IDs, routed subphase, selected unit ID, input digest, выбранный ExampleId и stored output до translator. Изменить только routed subphase; отдельно, другим сравнением, изменить только upstream typed result. Это проверяет наблюдаемую инвариантность, ещё не качество.

**Граница фактического producer:** `ProjectCurrent` и `ProjectAuthorizedReplay` вызывают один `BuildChosen(subPhase)`. Его текст зависит только от `SubPhaseId/Name`; разные предметные условия внутри одной subphase не создают разных answer labels. [ProjectCurrent, 118–146](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/21.AgentInfrastructure/Logical/LogicalCapabilityTrainingPreferenceProjector.cs#L118-L146), [ProjectAuthorizedReplay, 203–213](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/21.AgentInfrastructure/Logical/LogicalCapabilityTrainingPreferenceProjector.cs#L203-L213), [BuildChosen/BuildRejected, 356–360](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/21.AgentInfrastructure/Logical/LogicalCapabilityTrainingPreferenceProjector.cs#L356-L360). Поэтому первый native опыт проверяет соответствие **запрошенная subphase → raw retrieved subphase output**, а не обучение двум разным законам внутри одного scope.

Далее frozen policy/tokenizer и 8 разрешённых current source rows: по 4 варианта условия/формулировки на каждую из двух subphase. Штатная группировка может дать по одной corpus row на subphase; её в U-N1 не менять. A — нынешний общий corpus. B — **экспериментальная scoped view того же корпуса по подписанному source SubPhaseId**, при тех же embeddings/scoring и без нового DPO; общий lawful filter одинаков. Нельзя выводить scope из TEST gold или display-name эвристики. Если текущее поле корпуса не сохраняет subphase, предложенная добавка к `GeneratedCapabilityKnnExample` — `SourceSubPhaseId`, привязанная к исходному manifest, schema/hash и source proof; без неё B — CONDITIONAL. `BuildChosen`, registration grouping и stored outputs одинаковы в A/B.

Панель: 6 source-disjoint запросов, каждый требует две разные ветви; 4 single-subphase controls; 4 wrong-scope/unknown-task controls. Scorer проверяет subphase и описанную обязанность в **исходном выбранном output до translator**, а не наличие имени subphase в annotation. Реальное выполнение обязанности и составной задачи учитывать отдельно, только если ordinary consumer действительно вызывает соответствующий исполнитель и оставляет предметный результат. Сам generated текст не является выполненным изменением C#.

Отрицательный memory control должен переставлять **различающиеся labels между scope**, а не одинаковые labels внутри scope. Его допускается создать только разрешённым fixture/source owner как новый контрольный корпус с переставленным соответствием source query→subphase, сохранив штатный `BuildChosen` и новые честные provenance; не редактировать существующие signatures или output projector. Если такой control нельзя легально выразить, пометить его CONDITIONAL. Если scoped B остаётся правильным независимо от этого соответствия, это ожидаемый эффект готового routing scope: он подтверждает согласованность consumer с назначенной ролью, но **не показывает обучение различению предметных законов по памяти**.

Заранее измерять: requested/retrieved scope agreement/all, wrong accepted, отказ, ID каждой реально выбранной записи, latency/index bytes; фактически выполненные branch/compound obligations — отдельной метрикой при наличии исполнителя. Успех B при неизменном уровне ошибок поддерживает диагноз потери scope; отсутствие различий или правильное использование scope обычным caller опровергает его для выбранного пути. После получения сигнала проверить cold restore с тем же scoped corpus. Полноценный law-quality A/B внутри subphase — **отдельный CONDITIONAL контракт**, возможный только после нахождения настоящего producer, который сохраняет разные содержательные outputs; подменять им нынешний projector в этом опыте нельзя.

**Конкретное решение:** передавать типизированный `GeneratedCapabilityRetrievalRequest` с canonical prompt, requested subphase/task type и необходимым typed context; либо разделять разрешённые memory views по task scope. Если нужны разные предметные действия внутри scope, это требует отдельно определённого обучаемого content contract: текущий producer отдаёт только одну subphase label. Непустой ответ и его соответствие routing ID не маркировать как выполненную предметную работу.

**Новизна:** core 26/70 и P92 уже знают Factory/translator, а P94 — typed cardinality. В 168 вложенных Markdown нет описания приведённой инвариантности двух subphase через `ExecuteBindingAsync` и input-only scorer. Это новое причинное соединение существующих фактов, **не новый neural defect**.

## U-N2. При DPO→KNN несколько ветвей одной subphase превращаются в один длинный документ

В старом live вызове `BuildKnnTrainingExamples(replayManifests, branchPlans)` получал отдельные current branches. Нынешний `BuildKnnTrainingCorpus(legalTrainingReplayManifests, registrationPlans)` получает результат `BuildRegistrationPlans`, сгруппированный по SubPhaseId. `ProjectAuthorizedReplay` объединяет тексты группы через `string.Join(" ", texts)`. [Старый caller, 287–295](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L287-L295), [нынешний caller, 952–969](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L952-L969), [группировка, 3990–4043](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L3990-L4043), [конкатенация, 169–225](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/21.AgentInfrastructure/Logical/LogicalCapabilityTrainingPreferenceProjector.cs#L169-L225).

DPO использует исходные branchPlans при формировании preferences, то есть neural recipient и retrieval recipient видят разную гранулярность. [Final preferences, 524–544](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L524-L544). При этом current chosen и grouped replay output создаёт один `BuildChosen(subPhase)`, поэтому разные branches **одной** subphase имеют одинаковый stored output. [ProjectCurrent, 133–146](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/21.AgentInfrastructure/Logical/LogicalCapabilityTrainingPreferenceProjector.cs#L133-L146), [grouped output, 203–213](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/21.AgentInfrastructure/Logical/LogicalCapabilityTrainingPreferenceProjector.cs#L203-L213), [формат chosen, 356–360](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/21.AgentInfrastructure/Logical/LogicalCapabilityTrainingPreferenceProjector.cs#L356-L360). Проверяемый здесь эффект — покрытие разнообразных branch queries при выборе правильного scope среди других subphase, а не потеря разных answer laws внутри одной subphase.

Embedding Unit ограничен `maxLen: Math.Min(64, model.Config.ContextLen)`; lexical scorer считает длину документа, частоты и IDF уже для сгруппированных records. [Индексирование, 110–134](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L110-L134), [BM25, 247–273](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L247-L273).

**Гипотеза:** если различающий фрагмент второй ветви оказывается за embedding budget, вектор одной длинной строки хуже представляет этот случай, а lexical normalization дополнительно меняет баланс. Этот эффект зависит от домена и длины. Он не доказывается одним фактом конкатенации.

**Обязательная поправка:** старые raw branches имели одинаковые ExampleId внутри subphase и перезаписывали exampleMap; нынешний Train такие duplicates отвергает. Нельзя сказать «раньше каждая ветвь сохранялась совершенно корректно», и нельзя чинить нынешний duplicate guard удалением проверки.

**Опыт:** A — текущая одна строка на subphase; B — те же разрешённые branches как отдельные corpus rows со стабильным уникальным `(sourceBinding, branchIdentity)`, при одинаковом frozen policy/tokenizer и coefficients. Выход каждой ветви остаётся прежним `BuildChosen` её subphase; DPO, projector и scope policy не меняются. 2 subphase, по 2 ветви; 8 новых запросов по вторым ветвям, 4 по первым, 4 scope negatives. Сначала зафиксировать token offsets различающих сведений: они должны присутствовать внутри 64-token представления отдельной строки и исчезать за границей сгруппированной. Затем сравнить actual neighbor и **выбранную subphase/raw output**: восстановилось ли покрытие запросов по вторым ветвям без роста ложного выбора этого scope. Разные выбранные ExampleId с одинаковым правильным subphase output сами по себе не learning gain. Short-input control проверяет границу budget; отрицательный label control — только между разными scope по правилам U-N1. Если gains только от смены ID или исправления старой коллизии, это отдельный результат, не приписанный конкатенации. Law-level качество допустимо лишь отдельным CONDITIONAL опытом при найденном producer разных содержательных outputs; менять current projector ради такого эффекта нельзя.

**Решение после сигнала:** разделить registration identity и retrieval-example identity. Один Unit/subphase может иметь несколько источников-примеров; signed `KnnExamples` сохраняет их отдельные source-bound IDs и порядок, а Flow/Unit registration остаётся единственной. Не разделять текст произвольно на фрагменты, теряющие отрицание, область действия или предметный закон.

## U-N3. Orchestration materializer может восстановить готовый pattern, не используя изменение DPO features

**Уровень:** преобразование структурированного учебного материала в pattern state. Это **CONTRACT-DEPENDENT diagnostic**, а не доказательство плохого обучения RP01.

В PrepareToGPL `OrchestrationReasoningTrainer.BuildPattern` строил `OrchestrationReasoningLearnedPattern` из `chosen_scenario_plan_features`: intent, reasoning steps, evidence, decision guards. [Старый BuildPattern, 156–188](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/43.SmartSkillsDomain/OrchestrationReasoningTrainer.cs#L156-L188).

Теперь после проверки наличия этих полей и непустого `rejected_scenario_plan_features.near_miss_signals` есть ветка:

```csharp
if (row.TryGetProperty("learned_pattern", out var learnedPattern)
    && learnedPattern.ValueKind == JsonValueKind.Object)
{
    ...
    return OperationResult<OrchestrationReasoningLearnedPattern>.Ok(fixturePattern);
}
```

[Точная ветка, 220–260](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/43.SmartSkillsDomain/OrchestrationReasoningTrainer.cs#L220-L260).

Следовательно, при одном и том же `learned_pattern` изменение валидных chosen/rejected feature arrays не меняет материализованный pattern. При отсутствии override `rejectedSignals` в обоих снимках проверяется на наличие, но не записывается в результирующее learned pattern. Это точное свойство данных, а не предположение о gradients.

**Почему это не автоматически баг:** API явно материализует common/universal_reasoning payload, отвергает `training_started`, `external_ai_used`, `dpo_train_ready` и возвращает `SemanticQualityClaimed=false`. Его source admission допускает `project_internal_self_authored` и `ilearning_dynamic_graph`, а не произвольного внешнего учителя. [Ограничения, 197–218](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/43.SmartSkillsDomain/OrchestrationReasoningTrainer.cs#L197-L218), [AllowedKinds, 148–155](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/43.SmartSkillsDomain/OrchestrationReasoningSourceAdmission.cs#L148-L155). Для checkpoint reconstruction готовая запись может быть намеренным источником истины.

Но если этот материализатор считают «DPO научил unit отличать правильную последовательность от near miss», такая интерпретация не подтверждается: source receipt доказывает происхождение bytes, не причинное влияние rejected стороны. В обычном scenario Unit используются загруженные required-signal patterns; runtime допускает лишь exact signal contract. [Consumer, 294–369](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/43.SmartSkillsDomain/OrchestrationReasoningScenarioUnit.cs#L294-L369).

### Два коротких однофакторных сравнения

**U-N3a, diagnostic:** найти реальный разрешённый payload с `learned_pattern`; если таких rows нет, закрыть current-runtime гипотезу как NOT_REACHED. В отдельной test-scope копии через штатный source-admission материализовать A: исходные features+готовый pattern; B: изменённые валидные features при том же pattern. Создать новые honest source hashes/ledger штатным fixture owner, не менять старые receipts. Сравнить **semantic pattern state**, а не manifest/receipt hash, который закономерно меняется от новых bytes. Затем вызвать `OrchestrationReasoningScenarioUnit.Decide` на 6 одинаковых structured queries. Ожидаемая инвариантность демонстрирует authority готового pattern; сама по себе она не ошибка восстановления.

**U-N3b, если требуется обучение контрасту:** один chosen-plan «проверить состояние → повторить временную операцию» и два разных rejected near misses: «сохранить после неудачной проверки» и «повторять постоянную ошибку». Тот же chosen, те же task signals, 8 независимых structured cases: временная/постоянная ошибка и отрицательные ограничения. A — текущая материализация; B — отдельный согласованный learner, сохраняющий typed rejected constraints и применяющий их к candidate plans; никаких правил из TEST gold. Измерить forbidden transition errors, полезные правильные планы и abstention, а не `RowCount` и не PASS на hashes. Не выдавать B за существующий код; сначала нужен конкретный контракт обучаемых negative fields.

**Решение по результату:** если готовые patterns предназначены только для restore/fixtures, обозначить этот режим отдельным versioned contract и проверять semantic equality с разрешённым projection при смешанном payload. Если требуется реальное preference learning, отдельный путь обязан сохранять обе стороны и показывать влияние contrast на ordinary decisions. Внешняя открытая китайская модель создаёт DPO в целевом teacher-контуре; выбор урока и оценку качества выполняет CODEX с **astra 6 ultracode** по инструкции пользователя. Описанный self-authored materializer не является таким teacher-контуром; здесь не утверждается, что он им уже стал.

## Что не стоит возвращать из старой ветки без проверки

- **Warm reuse одного policy object.** В старом `BuildTeachingStack` возвращался `cached.PolicyModel`, который уже использовался прежними embedding Units, тогда как их index был построен раньше. Последующее обучение могло менять encoder без перестройки этих старых индексов. Нынешний путь создаёт новую policy из parent configuration и загружает checkpoint, что отделяет опубликованные объекты. [Старый cache, 489–503](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L489-L503), [нынешняя загрузка, 2343–2377](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L2343-L2377). Это source-level counterevidence против простого «убрать cold reload»; measured effect не заявлен.
- **Старую подмену retrieval-output пояснением subphase.** Она могла улучшать видимость routing correctness, не качество выбранного ответа; нынешний translator сохраняет результат.
- **Старый IncrementalTrainer как готовое SFT/DPO обучение.** В PrepareToGPL он только возвращал intended adapter path и не вызывал gradients в своём теле. [Старый IncrementalTrainer, 40–57](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/14.ContinuousLearning/Training/IncrementalTrainer.cs#L40-L57). Успешные старые runs надо привязывать к их настоящему executor.

## Связь с известными исследованиями и границы проверки

Проверены оба существующих каталога, включая **168 вложенных Markdown** `docs/plans/replanning-possible-diffs/experimets`; поиск специфических identifiers дополнен чтением P25, P85, P92, P94, P95, core 20/26/70 и N27. Core-каталог **не изменён**. Общие hidden determinism, cold restore, top3 replay, metadata-only export, classifier omission и typed cardinality не заявляются новыми.

Новый source chain U-N1 не найден в этих прочитанных материалах; U-N2 — узкая гипотеза новой группировки; U-N3 — отдельный неописанный contract-dependent materializer. U-R1 усиливает существующее исследование baseline-сравнением, U-R2 требует фактического donor bundle. Отрицательное `rg` само по себе не доказательство всемирной новизны.

Соседний положительный результат P125 действительно показывает, что ordinary EN→RU Flow меняется после memory lessons, но это **другой Unit/домен**: он не доказывает эти generated-capability hypotheses и не заменяет historical PrepareToGPL receipt. [Сохранённый P125 report](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md).

**Что широкая дополнительная проверка ещё может найти:** скрытый caller, который уже добавляет scope в собственный input; отдельный importer старых bundles; нынешние реальные payload с/без learned_pattern; потребителя, использующего только отдельные generated Units по точному ID; различия сохранённых исторических compiler/weights/tokenizer. Случаи выше ограничены названными owners. Runtime training, inference, native tests, Windows/CI и publication sub-agent не выполнял.

**Skill-load status:** `human-readable-architecture-audit-SKILL.md` найден/прочитан; `plan-review-SKILL.md` найден, прочитаны адресные gates hidden determinism, evidence masquerade, per-domain heldout/seed и source scope; огромный файл не выдаётся за полностью прочитанный. Управляющие проектные skills и AGENTS загружает root. Current user scope — аудит и документы/малые протоколы; code changes не выполнялись.

**Return packet:** worker `/root/units_restore`; artifact `audit/units_findings.md`; claim scope — source facts/conditional causal hypotheses; measured new effects = **0**; independent experimental confirmations = **0**. Текущий source audit не является отдельным independent PASS final artifact.

