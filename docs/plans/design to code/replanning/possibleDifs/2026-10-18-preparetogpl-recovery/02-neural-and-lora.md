# PrepareToGPL → нынешний DPO: что изменилось в LoRA, производной, токенах и восстановлении

18 октября 2026. Исследование исходников и сохранённых результатов. **Новый условный стык N-L1** отделён от прежних гипотез и от полезных нынешних исправлений. Все предлагаемые эксперименты — **NOT_RUN**.

[Порядок работ](README.md) · [Translation restore и Unit lineage](01-translation-restore-and-unit-lineage.md) · [История KNN](03-historical-knn-and-domain-experiments.md)

Пины сравнения: PrepareToGPL **31346d8928be4469b01aaf313780e8f950ff91e2**; codex/dpo-implementation **a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6**.

## 1. Главный вывод для восстановления прежнего качества

**Восстанавливать нужно проверенный прежний получатель обучения и его состояние. Переносить всю нейронную реализацию PrepareToGPL обратно опасно: нынешняя ветка исправила подключение LoRA к forward, знак производной log-probability и разрывы autograd.** Наличие этих дефектов в прежнем нейронном пути не отменяет успешное обучение других Units, памяти и kNN в той же ветке.

Найдён ещё один конкретный стык восстановления: legacy checkpoint в нынешнем LlmModel намеренно означает **импорт базовых весов с сохранением уже установленных A/B**, хотя общий метод описан как загрузка полного checkpoint. Это подтверждено не только исходником, но и специальным тестом, поэтому исправлять этот codec как случайную ошибку нельзя. Требуется различать намерение вызывающего кода: импорт базы либо полное восстановление. В нынешнем StartService есть дополнительный риск слишком широкого FullCheckpointValidatedEvidence после такого импорта. Для C50/C100 RP01 этот сценарий не доказан; его строгий исследовательский importer отдельно требует полную v2-схему.

## 2. Инженерное «до / после» по частям модели

| Часть | PrepareToGPL | Нынешний код | Классификация и следствие |
|---|---|---|---|
| Подключение LoRA | Apply замораживает base и возвращает отдельный список LoraLinear; внимание продолжает вызывать непосредственно q/k/v/o Linear. Внешний caller должен был сам применять адаптеры. [G1][gpl-apply], [G2][gpl-attention] | Установленные slot-адаптеры регистрируются в модели и вызываются через ForwardProjection. [C1][core-attach], [C2][core-attention] | Реальное изменение графа вычисления. Это исправление внутреннего подключения; прежнее наличие списка adapters не доказывало, что LlmModel.Forward их использовал. |
| Матрицы адаптера | Kaiming A, нулевая B, формула base(x)+scaling·dropout(x)·Aᵀ·Bᵀ. [G3][gpl-linear] | Та же низкоранговая формула; rank/alpha были и в GPL config, нынешний код добавил их публичные свойства, per-slot seeds и явный режим training. [C3][core-linear] | Нет основания говорить, что саму факторизацию AB заменили иным обучающим алгоритмом. |
| Dropout | LoraLinear.Forward всегда вызывает dropout(training:true). [G3][gpl-linear] | Forward(x) по умолчанию evaluation; training mode передаётся явно. [C3][core-linear] | Исправление различия обучения/вывода. В RP01 dropout=0, поэтому это не объяснение его отрицательных результатов. |
| Производная DPO по logits | При upstream=dL/dlogp используется (softmax−oneHot). AdamW затем вычитает gradient. [G4][gpl-backward], [G5][gpl-adam] | Используется (oneHot−softmax), затем противоположные upstream для chosen/rejected. [C4][core-backward], [C5][core-step] | Нынешний знак соответствует производной log-softmax. Возвращать старый знак нельзя. Эффект старого знака в конкретном историческом прогоне зависит от реально достижимых параметров. |
| Повторный policy forward | GPL считает score одним forward и заново строит forward в BackwardLogProbs. [G6][gpl-step] | Current удерживает logits того policy forward, которым рассчитан score. [C6][core-graph] | При stochastic training предотвращает рассогласование score и backward между разными dropout masks. Не отдельная причина RP01 при dropout=0. |
| RMSNorm | Два ручных broadcast возвращают новые Tensor без backward-связи. Final norm стоит непосредственно перед lm_head. [G7][gpl-rms], [G8][gpl-model] | TensorOps.MultiplyBroadcast сохраняет связи через normalization и gamma. [C7][core-rms] | Полезное исправление autograd. Оно не доказывает правильность всей составной производной, но опровергает предложение возвратить старый detach. |
| Токенизация BPE | UTF-8 bytes → RegexSplitter chunks → learned merges; Train начинает словарь заново. [G9][gpl-bpe] | Методы Train/Encode/Decode и применение merges сохранили тот же алгоритм; расширены canonical save/hash/validation. [C8][core-bpe] | Различать BPE-код и конкретный словарь обучающего запуска. Изменённый hash файла сам по себе не доказывает изменение token IDs. Семантический tokenizer/kNN — отдельный путь. |
| Что считается response score | Legacy усредняет log p по маске caller. [G10][gpl-score] | Exact-v2 хранит отдельные prompt/response IDs, response-only masks; objective суммирует log p ответа. Legacy mean остаётся отдельной ветвью. [C9][core-material], [C10][core-score] | Настоящее изменение objective и framing. Не доказательство регрессии. Нужен matched-length/domain contrast, не слепой возврат full-sequence mean. |
| Сохранение модели | Positional записи count→shape→floats; старый loader импортирует min(count,parameterCount), без ключей и полной схемы. [G8][gpl-model] | Current v2 хранит key/shape/config fingerprint и полный base+attached adapters; есть также отдельный adapter-only codec. Legacy import сохранён. [C11][core-save], [C12][core-parse] | Полезная проверка идентичности. Ловушка legacy full/base описана в следующем разделе. |
| Дробная legacy-mask | Forward весит maskVal, backward делил на число положительных позиций. [G4][gpl-backward] | На нынешнем SHA backward использует sum(mask) и умножает каждую строку на её mask. [C4][core-backward] | Старый N38/P86 не надо повторно объявлять открытым исправлением current. В protected каталоге уже есть сохранённый native repair report; его 8-урочный опыт не дал прироста свободных переводов. [P86][prior-mask-run] |

В RP01 конкретная конфигурация — vocabulary629, hidden512, 4 блока, 8 Q-heads/2 KV-heads, FFN1408; rank8, alpha16, dropout0, q/v. Это [явный config][core-config], а не автоматически конфигурация всех успешных GPL-прогонов. Для q на слой A=[8,512], B=[512,8]; для v A=[8,512], B=[128,8]. Четыре слоя дают 16 A/B tensors и 53 248 trainable elements; речь о 4 слоях с адаптерами, не о 16 слоях.

Для signed loss L=softplus(−m) имеем dL/dm<0, а d log p(y)/d z=oneHot(y)−softmax(z). Поэтому chosen-upstream должен повышать выбранную вероятность при gradient descent. Это математическая проверка знака из кода, не выполненная finite difference всего Transformer. При B=0 нулевая dA на самом первом шаге ожидаема; для проверки достижимости A отдельно нужен B≠0. Этот вопрос и составная q/v-проверка уже описаны в [предыдущем source audit][prior-gradient], поэтому не считаются новыми находками.

## 3. N-L1 — один legacy файл может восстановить разные effective weights в зависимости от предыстории receiver

**Статус:** SOURCE_CONFIRMED_INTENTIONAL_BASE_IMPORT; CONDITIONAL_CALLER_CONTRACT_MISMATCH; QUALITY_NOT_MEASURED. Новый конкретный подслучай внутри уже известной семьи restore/identity; не новый диагноз всего RP01.

### Фактическое состояние до и после

Нынешний публичный LoadCheckpoint(Stream) документирован как загрузка complete checkpoint. Если первый Int32 не magic, а неотрицательное число, вызывается ParseLegacyCheckpoint; он сравнивает count только с _baseParameters.Length, проверяет shape/finite/EOF и возвращает назначения только для base. Привязанные adapter slots не сбрасываются и не переимпортируются. [API и branch][core-load], [legacy parser][core-parse].

Отсюда для валидного legacy файла Wg с той же базовой схемой:

| Receiver до вызова | После LoadCheckpoint(Wg) |
|---|---|
| Свежая модель без adapters | Wg |
| Модель с новой нулевой B и случайной A | Wg + нулевая текущая LoRA-поправка |
| Модель с прежними обученными Aprev/Bprev | Wg + (alpha/r)·Bprev·Aprev; прежние A/B остаются |

Это структурный прогноз по присваиваниям, не выполненные измерения logits. Он требует совпадающих legacy count/shapes; неправильные размеры, nonfinite, неполный файл и extra trailing bytes отвергаются до применения.

**Сохранение A/B здесь намеренно.** Tests/LoraTests.cs:1429–1453 задаёт ненулевые adapter values, вызывает legacy load и требует их неизменности. [Точный test contract][core-legacy-test]. Поэтому нельзя исправлять сам факт сохранения адаптера без изменения смысла API и его callers. Полезный режим «загрузить базу, сохранив выбранную адаптацию» должен остаться доступным с явным названием и binding базы.

В GPL встроенного attachment в LlmModel не было: штатный positional checkpoint описывал его зарегистрированные базовые параметры. Нынешний codec добавил совместимость с attached model, в результате один общий метод получил два разных смысла в зависимости от формата файла. Это изменение контракта, а не доказательство ухудшения обучения.

### Реальный caller и предел защиты

CodeDocumentationRepositoryOwnedModelCheckpointStartService.LoadIntoFreshModel сначала пробует строгий adapter-free loader. При неуспехе и наличии loraConfig идёт в TryLoadLoraCheckpoint, где создаёт fresh model, устанавливает seeded/обычные LoRA и прямо вызывает LoadCheckpoint. [Fallback][core-start-fallback], [LoRA import][core-start-lora].

В отличие от adapter-free ветви, этот fallback не вызывает ValidateFullCheckpointArtifact с FullCheckpointLoraTopology. Валидный positional base-файл может пройти этот code path и оставить freshly initialized adapters. После успеха PrepareStart возвращает FullCheckpointValidatedEvidence и сообщение о валидации до создания модели. [Возвращаемое evidence][core-start-evidence]. Внутри fallback фактический порядок обратный: construct→attach→load.

Верхний RepositoryOwnedModelCheckpointLoader сверяет sealed manifest, совместимость и exact bytes; manifest sealer проверяет hash/size и lineage, а не full-LoRA tensor schema. [Loader][core-sealed-loader], [Sealer][core-sealer]. Это не означает обход admission: предпосылкой остаётся уже корректно запечатанный совместимый manifest. Наличие конкретного такого legacy specimen в production здесь не установлено.

Fresh construction защищает этот StartService от A/B прошлого живого receiver. Его иной риск — выдать initialization отсутствующих в legacy файле адаптеров за полное восстановление заявленной адаптации. Нужна явная политика: legacy base import допустим как initialization или full candidate должен содержать все объявленные LoRA weights.

**RP01 C50/C100 этим наблюдением не объяснён.** SimulationEnRuContinuation.ImportFullCheckpoint сначала вызывает ValidateFullCheckpointArtifact с объявленными rank и target modules, затем LoadCheckpoint. Строгий validator принимает только version2 keyed full artifact и полный набор ключей. [Реальный importer][core-enru-import], [строгий gate][core-full-gate]. Это прямое контрдоказательство пути N-L1 в данных сохранённых импортах.

### Почему обычный save/reopen может не обнаружить ошибочное намерение caller

BuildCheckpointEntries включает base и все текущие adapters; SaveCheckpointToStream сохраняет их как полный v2-файл. [Состав][core-entries], [Writer][core-save]. Если caller ошибочно хотел восстановить только Wg как полное исходное состояние, но получил Wg+ΔWprev, последующий save и fresh reopen могут побайтно воспроизвести именно смесь. Такой readback докажет сохранность **получившегося** состояния, но не соответствие задуманному parent. Отдельно должны проверяться base-parent hash, adapter-parent hash и import intent. LlmModel не владеет optimizer — optimizer/RNG требуют своего continuation-контракта, их нельзя объявлять восстановленными по одному LoadCheckpoint.

### Минимальный будущий эксперимент NEURAL-LEGACY-RESTORE-01 — NOT_RUN

Получатель — LlmModel и его attention adapters. Тема проверяемого навыка — документация C# либо перевод; выбрать **одну** тему на первый опыт. Градиентов, обучения и новых DPO для первой части нет.

1. В отдельном runner приготовить legacy base fixture ровно по GPL writer: count/shapes/float values. Выбрать два независимых current receivers с одинаковыми base/config и одинаковой топологией LoRA: у A B=0; у B известная конечная ненулевая A/B из отдельного fixture либо разрешённого trained donor. Не смешивать toy tensor fixture с доказательством полезного навыка.
2. В обоих вызвать один legacy LoadCheckpoint. Проверить base hashes=Wg, A/B до/после, actual adapter keys. Ожидается сохранение разных A/B — это подтверждение documented compatibility behavior, а не автоматический FAIL codec.
3. Ввести только **явное намерение caller**: RestoreFullCheckpoint требует strict full-v2 topology; InitializeBaseFromLegacy явно сообщает base-only import и происхождение сохранённых либо заново созданных адаптеров. В полном режиме тот же legacy вход должен быть отклонён до изменения receiver; в явном base-режиме сохраняется прежняя допустимая семантика.
4. Сохранить результат B как новый v2 и открыть в свежей модели C. Ожидаемое B/C equality показывает, почему саморавенство после cold load недостаточно. Parent-binding контроль должен отличить Wg+ΔWprev от полного выбранного donor.
5. Для предметной части использовать только checkpoint с ранее подтверждённой пользой в выбранной теме и его точный config/BPE. Четыре кратких запроса: два старых освоенных закона, один близкий контраст отрицания/порядка, один unrelated контроль. Сравнить output/logits с правильным full donor при одинаковом sampler и framing. Если полезного neural donor нет, этот этап BLOCKED_BY_DONOR_EVIDENCE и semantic gain не заявляется.

Основная цена: 2 legacy loads, 1 full save/reopen, 1 strict refusal; при отдельной предметной части до 12 коротких ответов. Контроли: полный корректный v2 с A/B; несовместимый count/shape; попытка объявить legacy base полным LoRA-кандидатом; fresh zero-output adapters как допустимая инициализация. Без Windows CI. Никакого автоматического обнуления уже обученных A/B.

## 4. Что действительно сохранено после EN→RU RP01

Прочитан не только planning summary, но и [scientific-result.json][raw-scientific], [main-state-verification.json][raw-state], [analysis-proof.json][raw-proof] и [восемь индивидуальных оценок][raw-grades].

| Наблюдение сохранённого запуска | Что оно устанавливает |
|---|---|
| 500 уникальных completed IDs, 50 новых updates, global step100 | Размер фактически учтённой новой кампании; не 500 независимых законов |
| 39 base tensors/11 920 896 elements неизменны; 16 LoRA tensors/53 248 elements меняются конечным образом | Запись обучающего воздействия на q/v A/B |
| Optimizer format2, step100, 16 keyed entries; weights/optimizer/RNG reopened and equal live | Сохранённый отчёт о продолжении полного исследовательского состояния |
| 0/4 полностью верных и 4/4 invalid до и после | В этой маленькой панели полезный свободный перевод не появился |
| Mean preference margin +0.0001100063; chosen-preferred 12/20→12/20 | Малое изменение относительного scoring, не приобретение свободного навыка |
| C# retention 0/3→0/3, тексты совпадают | Сохранение прежних неудачных ответов; не доказательство удержания полезной компетенции |
| Сохранённая B0 названа random and untrained | Сильная предпосылка cold-start гипотезы, не доказательство единственной причины |

Числа являются **прочитанным историческим evidence**, не новым исполнением и не аттестацией текущего binary. Сами 47.9-MB checkpoint bytes не импортировались. First-decision comparison.json на выбранном SHA доступен через plugin как LFS pointer; полного payload в этом аудите не получено, поэтому его vectors не выдаются за заново проверенные.

Всем будущим DPO назначаются роли пользователя: **внешняя открытая китайская модель создаёт chosen/rejected; CODEX (запрошенный пользователем `astra 6 ultracode`) выполняет выбор заданий и независимую оценку качества**. В прочитанном ENRU receipt provenance=simulation; external teacher/evaluator native calls=0 относится к native фазам, а total orchestration API accounting отсутствует. Эти данные не подтверждают фактический запуск названной внешней модели или runtime-аттестацию модели CODEX.

## 5. Приоритет малых будущих проверок и дедупликация

Для восстановления GPL-поведения при наличии соответствующего Translation asset сначала использовать U-R1/U-R2 из соседней главы: там найден точный старый checkpoint и конкретное препятствие загрузке. Neural N-L1 добавляется как условная проверка intended state при импорте legacy, а не как доказанный источник RP01 failure.

| Уже известная гипотеза | Короткий корректный опыт | Предметная область и предел |
|---|---|---|
| Случайная база + q/v-only DPO недостаточны | Тот же frozen B0, tokenizer и 8 заранее проверенных пар; A DPO-only, B отдельный chosen-CE warmup→DPO. Первым условием является корректная достижимость SFT-параметров. Фиксировать суммарные tokens/updates/время отдельно; warmup не бесплатен. | Документация либо перевод. Не запускать ещё 500 пар до выяснения train-fit. Эта гипотеза уже есть в prior cold-start/P11/P49. |
| Составная производная DPO может быть неверна | Две пары, exact response masks, фиксированные weights/vector signals, B=0 и отдельный B≠0; analytic-vs-numerical gradient по заранее выбранным q/v A/B. Optimizer не нужен для проверки derivative. | Численный компонент; качество навыка только отдельным последующим опытом. Уже есть prior41. |
| Sum-response objective зависит от длины | A нынешняя response-sum, B явная response-mean версия при тех же masks и парах; matched-length поднабор и контрасты с различающим последним токеном. | Перевод отрицания/ролей; документация короткого/длинного объяснения. Не возвращать prompt в loss. Уже есть P05. |
| Tokenizer/model identity могут расходиться | До model call сравнить exact material tokenizer SHA с actual handle tokenizer и IDs; несовпадающий словарь с тем же vocabSize — отрицательный контроль. | Оба neural потребителя. Уже известный N39; данный аудит не обнаружил нового BPE алгоритмического drift. |
| После исправления mask по-прежнему нет пользы | Сохранить нынешнюю правильную derivative; диагностировать train-fit и свободный output отдельно. | P86 live8 уже сообщает 0/24→0/24. Повторять исправление как новое открытие нельзя. |

Дедупликация выполнена по обоим исходным каталогам (544 md/txt/ndjson) и11 новым planning-документам перед публикацией, в том числе core02/41/47/56/62/70, planning N38/N39 и карточки P05/P11/P12/P25/P86. Точный механизм ParseLegacyCheckpoint→сохранённые A/B→full-intent caller не обнаружен в этой текстовой базе. Общая идея cold restore/identity была известна. Новизна N-L1 ограничена конкретным составом состояния, намеренным test contract и найденным fallback. Прочитанные tests являются источником смысла API, не результатом их выполнения.

## 6. E-HEAD — восстановить место нейронной адаптации, сохранив нынешний корректный graph; NOT_RUN

В прежнем [разборе ProjectWiki donor][prior-neural-donor] найдены разные neural checkpoint и изменения output head; в той серии сохранены положительные preference-метрики. Это самостоятельный neural след, его нельзя заменять утверждением «всё прежнее обучение было только KNN». Одновременно опубликованный предметный planner и neural metric runner там различались по consumer. Поэтому нужный перенос формулируется точно: **проверить место адаптации на одном и том же собственном ответе модели**.

В PrepareToGPL сохранён [summary025][gpl-wiki-summary] для продолжения от023 с 50 training/50 heldout и положительным heldout preference. [Runner][gpl-wiki-runner] использует небольшой Transformer и hashed word IDs. Эти IDs и его raw accuracy не совместимы напрямую с RP01 BPE и free-generation экзаменом. Отдельное языковое pretraining до этой серии найденными файлами не аттестовано. Проверенный source snapshot и импортированные historical receipts не удостоверяют точный исполнявшийся binary.

**Домен первого опыта:** документация C# или перевод; выбрать один. Зафиксировать один B0, его tokenizer/reference, 8–16 пар, 8 контрольных запросов, одинаковые decoder и optimizer schedule. A — нынешняя q/v-LoRA. B — адаптация выходного head при сохранённых нынешних autograd/sign/mask. Число trainable parameters уравнять либо сделать два явно разных вопроса: target location при сопоставимой ёмкости и location+capacity. Полный head против маленького q/v adapter без этой оговорки не является чистым тестом места адаптации.

До update записать baseline ответы и вероятность chosen/rejected. После небольшого одинакового бюджета — train preference/CE, точные tensor changes, собственные ответы, fresh-load результат и регрессии старых законов. Нельзя сравнивать один legacy hash-scoring run с новым BPE-generation run как две arms. Если обновлённый head улучшает только margin, установлено изменение preference; полезная генерация пока не восстановлена.

Положительный результат определяет конкретный target repair для дальнейшего маленького подтверждения. Отрицательный результат на random B0 не опровергает полезность head adaptation на другом уже компетентном donor. **Это уже известная гипотеза core25, вновь выделенная в главе восстановления PrepareToGPL, а не N-L2.**

## 7. E-BASE — проверить стартовую компетенцию отдельно от места адаптации; NOT_RUN

Взять тот же B0 и один домен, 8–16 новых проверенных DPO-пар. A — DPO-only; B — chosen-only CE warmup, затем DPO. Суммарные target tokens, optimizer updates и время записать; при требовании чистого равного бюджета распределить общий лимит между двумя стадиями B и удержать его у A. Иначе эффект включает дополнительный ресурс и так и называется.

Перед этим нужно убедиться, что выбранный SFT caller действительно передаёт gradient в разрешённые параметры, а не только меняет loss scalar. Отдельная конфигурация target layers не меняется одновременно с warmup. Контрольная панель — 8 новых формулировок/семейств, выбранных до обучения, с 4 прежними освоенными вопросами только если исходная компетенция уже подтверждена. Нулевой прежний skill нельзя выдать за успешную retention.

Цель опыта — решить, есть ли ранний train-fit и предметный эффект от выбранной предпосылки; он не задаёт обязательного процента успеха и не требует повторения 500 строк. Эта гипотеза уже была в RP01 recovery и предыдущих каталогах. Для нового корпуса обе стороны DPO создаёт внешняя открытая китайская модель, CODEX выбирает и оценивает.

[prior-neural-donor]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/25-historical-neural-learning-donor.md
[gpl-wiki-summary]: https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-project-wiki-query-planning-sequence-002-dpo/dpo-runs/project-wiki-query-planning-flow/project-wiki-query-planning-promotion-confirmation-025-project-wiki-query-planning-flow-20260519175259/project-wiki-dpo-metric-summary.json
[gpl-wiki-runner]: https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs

## Источники

[gpl-apply]: https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/10.LoRA/LoraApplicator.cs#L11-L44
[gpl-attention]: https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/03.TransformerModel/Layers/CausalSelfAttention.cs#L43-L75
[gpl-linear]: https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/10.LoRA/LoraLinear.cs#L23-L63
[gpl-backward]: https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/11.DPO/DpoTrainer.cs#L276-L322
[gpl-adam]: https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/04.TrainingLoop/Optimizers/AdamW.cs#L63-L76
[gpl-step]: https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/11.DPO/DpoTrainer.cs#L38-L62
[gpl-rms]: https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/03.TransformerModel/Layers/RmsNorm.cs#L27-L65
[gpl-model]: https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/03.TransformerModel/LlmModel.cs#L28-L112
[gpl-bpe]: https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/01.Tokenizer/BpeTokenizer.cs#L27-L76
[gpl-score]: https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/11.DPO/DpoTrainer.cs#L239-L273
[core-attach]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/03.TransformerModel/LlmModel.cs#L538-L543
[core-attention]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/03.TransformerModel/Layers/CausalSelfAttention.cs#L129-L170
[core-linear]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/10.LoRA/LoraLinear.cs#L39-L125
[core-backward]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L1072-L1120
[core-step]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L864-L923
[core-graph]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L1007-L1030
[core-rms]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/03.TransformerModel/Layers/RmsNorm.cs#L26-L40
[core-bpe]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/01.Tokenizer/BpeTokenizer.cs#L42-L91
[core-material]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/Models/DpoExactPairMaterial.cs#L19-L89
[core-score]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L1036-L1068
[core-save]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/03.TransformerModel/LlmModel.cs#L634-L685
[core-load]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/03.TransformerModel/LlmModel.cs#L1102-L1167
[core-parse]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/03.TransformerModel/LlmModel.cs#L1475-L1520
[core-config]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/config/code-documentation/generation-v1.json#L7-L48
[core-legacy-test]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/Tests/LoraTests.cs#L1429-L1453
[core-start-fallback]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedModelCheckpointStartService.cs#L421-L456
[core-start-lora]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedModelCheckpointStartService.cs#L513-L558
[core-start-evidence]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedModelCheckpointStartService.cs#L139-L172
[core-sealed-loader]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/RepositoryOwnedModelCheckpointLoader.cs#L181-L205
[core-sealer]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/RepositoryOwnedModelCheckpointManifest.cs#L353-L388
[core-enru-import]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/SimulationEnRuContinuation.cs#L192-L211
[core-full-gate]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/03.TransformerModel/LlmModel.cs#L910-L991
[core-entries]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/03.TransformerModel/LlmModel.cs#L1360-L1375
[raw-scientific]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/.kiro/xiigen-slices/work/results/RP01-enru-continuation-20261016/runtime-20261016-01/analysis-20261016-01/scientific-result.json
[raw-state]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/.kiro/xiigen-slices/work/results/RP01-enru-continuation-20261016/runtime-20261016-01/main-state-verification.json#L683-L705
[raw-proof]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/.kiro/xiigen-slices/work/results/RP01-enru-continuation-20261016/runtime-20261016-01/analysis-20261016-01/analysis-proof.json
[raw-grades]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/.kiro/xiigen-slices/work/results/RP01-enru-blind-output-20261016/grading/grades.json
[prior-gradient]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/41-current-dpo-gradient-chain-source-review.md
[prior-mask-run]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/report.md

