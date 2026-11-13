# Короткие эксперименты: сначала восстановить известный механизм

Ни один новый native/training опыт из этого документа в данной сессии не запускался. Исключение в смысле уже имеющегося знания — R02: исправление и его измерение опубликованы ранее и проверены по источникам. Offline пересчёты N-RESTORE-01/N-NEURAL-01 выполнены отдельно и имеют собственные receipts.

## Общий протокол, который делает результат объяснимым

Опыт отвечает на один вопрос: изменилось ли предметное поведение от **одного названного вмешательства** при тех же исходном состоянии, данных и экзамене. Для переноса целого механизма допустим один объявленный системный switch, например доступность memory route; такой опыт не выдаётся за изолированную проверку KNN distance или LoRA.

Урок, вектор, параметр и Unit — разные объекты. В receipt записывать: исходный prompt и обе стороны пары; кто их создал; target Unit/Flow; teacher fields→typed material→token IDs/masks или key/feature vector; имена реально изменённых параметров/записей; save/import owner; ordinary consumer и фактический route ответа. Отсутствующая schema или caller — конкретная зависимость, а не повод подменять продукт функцией, возвращающей gold.

Для **будущих новых DPO-пар** использовать требуемое разделение ролей: CODEX Astra6 ultracode выбирает темы/уроки как делегат человека; внешний открытый китайский model создаёт **обе** альтернативы; отдельный CODEX Astra6 ultracode проверяет точное содержимое пары и допустимость chosen/rejected. Реальные provider/model IDs и происхождение фиксируются по фактическому вызову. Этот документ не утверждает, что названные вызовы уже выполнялись. Retained simulation receipts имеют собственный исторический `DeterministicGenerator` provenance, который нельзя переписать задним числом. Схематические контрасты ниже — спецификация признаков, не готовый авторизованный corpus.

На TRAIN смотреть динамику; DEV отделить по source families и зафиксировать до первого ответа. Известные20 ENRU,25 C# и24 memory cases — retained диагностические панели; их повтор не создаёт новый независимый экзамен. Выигрыш на train recall, близкой формулировке, новой комбинации известных частей и новой предметной семье показывать раздельно. Для transfer обязательно retention там, где исходный навык ненулевой.

### Минимальная запись результата

| Блок | Что записать |
|---|---|
| Предметное качество | Правильные/все, правильные/принятые, coverage, критические смысловые ошибки, отказы; сами paired outputs |
| Neural механизм | Chosen/rejected mean logP отдельно, margin, actual gradients, A/B residual и logits; updates, examples, response targets |
| Memory механизм | Inserted/updated entries, distinct lesson IDs, реальные кандидаты/их происхождение, selected output, route, conflicts |
| Сохранение | State hashes с объявленным canonicalization, load result, ordinary output до/после нового процесса; без скрытого replay |
| Затраты | ms/update либо ms/операция при updates0; total wall, process/group RAM, state/index bytes; GPU/VRAM при наличии |
| Архитектура | Model blocks и trainable targets отдельно от числа tensors; Unit/Flow levels и фактически исполненный путь |

Порог решения задать перед запуском: какой механизм должен различиться и какой предметный эффект нужен. Маленький пилот различает причину и даёт paired cases; он не доказывает population significance. Не увеличивать бюджет автоматически только потому, что первая попытка не дала выигрыш.

## R01 — восстановление legacy4 в runtime6

Полная карточка и точные state invariants — [01](01-LEGACY-CHECKPOINT-GAP.md#5-малый-причинный-опыт-r01). Стартовать здесь, если цель — вернуть старый `translation-v1`.

**A/B/C:** old loader/old bytes; current loader/old bytes; current loader/те же old bytes с явными двумя пустыми ES entries. **Материал:**2 archived probes. **Budget:**0 lessons,0 neural updates,3 initial load processes+1 cold reload C. **Сигнал:** B отказал, C восстановил прежние ответы/маршруты A без обучения. **Контроль:** untrained owner, пустые новые языки не засчитываются как знание. **Falsifier:** реальный current load уже принимает exact файл либо migration не восстанавливает поведение.

Предметная область — технические инструкции и memory translation. Отдельный post-restore новый урок допускается только следующим опытом; иначе нельзя отличить восстановление знания от его повторного приобретения.

## R02 — доставка частей урока: применить уже полученный результат

**Статус: IMPLEMENTED_AND_MEASURED в current, не новое открытие.** [Native report](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md) сопоставляет old/new reader+builder при неизменной Domain DLL. Те же8 уроков,16 mappings и24 questions:11/24→19/24;8 новых комбинаций0/8→8/8. Current [reader](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain.Tools/Program.cs#L4442-L4470) и [builder](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain.Tools/Program.cs#L4682-L4695) передают `phrase_mappings`.

**Что сделать перед любым новым опытом:** проверить, что целевой host запускает именно эти текущие reader/builder и такую же materialized schema. Если да — использовать выполненный результат, не повторять весь старый8/24 цикл только ради нового отчёта. Если другая ingestion ветка теряет mappings, та же причинная проверка ограничивается этой границей: в двух изолированных копиях одинаковые уроки, меняется только сохранение mappings в typed output. Ordinary Flow и scorer прежние.

**Предметная область:** инструкции action/object, новые комбинации знакомых частей. **Граница:** не доказано новое умение на независимых action/objects; negation/question/number остаются отдельными axes. **Следующий отдельный опыт:** один polarity/order feature switch по P54/P55 на новых контрастах с той же map memory. Не смешивать delivery fix и новый semantic guard в одном сравнении. Улучшенный отказ показывать отдельно от правильного перевода.

## R03 — structural KNN снова влияет на C# результат

**Происхождение:** положительные E09 receipts и настоящий production predictor; подробности в [03](03-HISTORICAL-LINEAGE.md#2-e07-и-e09-не-склеивать-разные-метрики-и-серии). Уже существующие гипотезы P26/P27/P82, здесь уточнён приоритет восстановления.

**Получатель:** `SemanticCompilerFacade`/`StructuralKnnPredictor`, затем конкретный caller, который использует `PredictedNextTokens`. Это не generic embedding Unit и не LoRA. **Старт:** одна память с ≥10 lawful train pairs, одинаковые vectors/labels/ordering, k и codec. **A:** обычный ответ не использует прогноз. **B:** тот же ordinary answer consumer использует его через заранее названный интерфейс. Функция fallback и арбитр одинаковы. Отсутствующий ответный consumer означает CONDITIONAL: сначала нужен конкретный proposed diff этого caller, не показ predictor accuracy как product gain.

**Минимальный материал:**4 небольших C# TRAIN files и4 новых DEV files;12–20 structural decisions. Семейства кода разделить до extraction; вложенный `if`/`return`, `try/finally`, scope переменной и порядок statements. Baseline и treatment получают строго одни пары и одну проверочную панель, в отличие от несопоставимых старых split. Сначала мера — верный следующий семантический AST kind; отдельно — сохраняющий поведение выбранный patch/объяснение кода, если именно это делает caller.

**Доказать:** Compile действительно вызвал predictor; training size ненулевой; переданы реальные parent/node features; известен codec; result field прочитан потребителем. Вывести neighbours, distances, votes и решение после арбитра. Если raw prediction меняется, а ответ нет — это лишь возможное ограничение влияния. Caller trace должен различить непрочитанное поле, подавление арбитром и неизменный итоговый выбор; само равенство ответов разрыв потребления не доказывает.

**Контроль:** одинаковый state под двумя именами; no-memory; структура та же после переименования identifiers. **Edge:** перестановка statements, меняющая поведение; полезный ответ может иметь иной поверхностный текст. **Опровержение:** baseline уже потребляет тот же predictor, прогнозы не помогают на matched panel либо B ухудшает итог. **Budget:**0 neural updates, одна фиксированная memory build на плечо и≤20queries; история обучения new cases не смешивается с включением consumer.

**Область:** C# с реальным parser. Для TypeScript/другого языка — отдельный parser/labels; natural-language word order проверяется в своих profiles. Не менять одновременно enum codec, tie-break и consumer.

## R04 — контекстная адресация смысла E26

**Этап R04.1: дешёвая historical parity, без объявления обучения.** A=`TranslateBaseline`, B=`TranslateWithContext` на одном заранее заполненном dictionary и прежних15 cases. Это controlled lookup. [Реальный ContextualDictionary](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/35.ExperimentsV2/Infrastructure/ContextualDictionary.cs) выбирает одного ближайшего кандидата по before/after POS и Priority; `_k=3` сохранён как поле, но не участвует в выборе. Не имитировать3-neighbour voting. Проверка нужна лишь если переносится эта конкретная функция и её parity ещё не установлена.

**Этап R04.2: отдельный опыт настоящего learner**, развитие [P43.7](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/cards/P43-contextual-pos.md): A=`ExecutorWordMap` с нынешним ключом `(lemma,concept)`, B=версионированный ключ с теми же полями+POS-before/after. Оба получают одни принятые TRAIN пары/alignments. Содержание target и все phrase/exact/KNN/verifier состояния одинаковы. Current producer должен реально поставить совместимый context; при отсутствии — dependency, не silent cast старых POS integers.

**Материал:**6–8 TRAIN контекстов технического перевода,8 новых DEV контекстов для неоднозначных слов. Имена смыслов `thread/flow/stream`, `request/query/prompt` — кандидаты для независимой лингвистической проверки, не gold, выведенный из POS numbers. **Рубрика:** правильный технический смысл и сохранённая роль слова; отдельно coverage и неверное разрешение ambiguity.

**Контроль:** слово с единственным смыслом; повтор same context; unknown context; запрещённый перенос integer enum другой схемы. Если exact/phrase-map уже перехватывает запрос, записать route-not-reached. Нельзя удалить эти maps лишь в B. **Сигнал:** новый lesson сохраняет два контекстно разных значения вместо перезаписи одного и ordinary lookup различает их после cold restore. **Falsifier:** producer не имеет признака, key collisions не возникают, existing fallback уже даёт ту же способность либо контекст не улучшает реальные новые фразы.

**Область:** технический RU→EN; на EN/RU/HE/ES нужны свои text→POS contracts. **Budget:**1 small batch из≤8 уроков/плечо,≤12queries+8 retention;0 neural updates. Это отдельный опыт относительно R04.1; готовый `BuildDefault` нельзя объявлять выученным состоянием Executor.

## R05 — DPO→subunits→ordinary Flow: проверить именно выбранный мост

**Это известный P92, не новая гипотеза.** Current [BuildKnnTrainingCorpus](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L3467-L3540) и [`TransformerEmbeddingKnnLogicalUnit`](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L57-L100) — один capability путь. Переводческий `ExecutorUnit` — другой. Нельзя подменять один другим в середине receipt. [P92 и его matched controls](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/cards/P92-dpo-memory-flow-bridge.md).

**A/B:** один frozen Student, tokenizer и corpus. A — обычный generation consumer без capability-memory bridge. B — тот же host с явно выбранным разрешённым TRAIN corpus→existing `BuildKnnTrainingCorpus`→Unit→`GeneratedCapabilityFlowFactory` маршрутом. Это **один системный memory-consumer switch**, включающий доступность памяти; он не измеряет изолированное улучшение DPO weights. Не менять endpoint молча: proposed consumer/typed output/fallback должны быть видны.

**Материал:**8разных коротких C# code-documentation lessons;8 новых проверок их семантических семейств+4 неприменимых вопроса. Проверять null/empty, branch condition, boundary operators, exceptional exits. Если выход Unit — typed predicates, сравнивать по заранее общей семантической рубрике, не требовать искусственного string equality с prose generator.

**Receipt на каждой границе:** pair ID/content hash→subunit input/desired output→записанный chosen→index entry/embedding version→selected neighbour ID→реальный returned output→Flow/host response. Hash изменился, но обычный ответ прежний — возможный adoption gap. Query не видит различие из-за truncation/BPE/hash bag — representation gap; его менять следующим отдельным опытом по P07/P16/P23.

**Контроли:** untrained/no-hit; неприменимый lesson; отдельный fixed-hit shuffled-output контроль при неизменных neighbours показывает, использует ли ответ learned output. Контроль с неправильными targets не становится принятой моделью. **Falsifier:** одинаковый ordinary output несмотря на доказанное изменение выбранной записи или bridge уже работает в A. **Budget:**0 neural updates;1 memory-build/плечо;≤12queries. После положительного эффекта — отдельный cold restore того же state без новых уроков по P92.3.

**Область:** code documentation; для переводов нужна своя соответствующая output schema. Training качество пар, LoRA update и memory bridge впоследствии можно разделить2×2, но это не обязательное расширение первого маленького опыта.

## R06 — первый момент AdamW после смены предметной программы

**N-NEURAL-02 — новое отдельное вмешательство к известной теме DLT-K1; полная формула, references и ограничения — [02](02-NEURAL-SCORES-AND-STATE.md#3-n-neural-02--первый-момент-прошлого-домена-при-новом-reference-c50).** Observed ENRU chosen decrease не доказывает это объяснение. LFS pointer не содержит actual moments. Общая гипотеза и cosine уже есть в [DLT-K1](https://github.com/lubochka/xiigen-planning/blob/412d3364cc94b5aa1cdf516f829960da152826cb/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/PREPARETOGPL-DPO-LORA-TOKENS-2026-10-18.md#6-полезные-уточнения-известных-тем--не-увеличивают-число-новых-находок); данный опыт отдельно от полного reset optimizer оставляет прежними v50 и step50.

**Старт:**2 независимые полные C50 копии; policy/reference одинаковы в обоих плечах, reference frozen; q/v rank8 alpha16 dropout0,4 model blocks; те же base/tokenizer/seeds/RNG/material/order/LR1e−5/DPO beta.1, AdamW β1=.9 β2=.95 decay.01 eps1e−8. **A:**m50/v50/step50. **B:**только m50=0, настоящиеv50/step50 прежние. New reference C50 в обоих плечах одинаков.

**Бюджет:**те же8 новых ENRU TRAIN пар, до 8 updates/плечо;8–12новых DEV families и C# retention cases. Можно закончить раньше при явном NO_CONTRAST/ошибке доступа к slots. Ни scalar LR, ни batch size, ни SFT, ни embedding targets одновременно не меняются.

До update снятьg,m50,v50; после — фактическийΔw. Измерить cosine(m50,g), g·Δw отдельно от decay, нормы градиентов и A/B tensors. После каждого шага — TRAIN chosen/rejected logP, на одинаковых DEV checkpoints — ordinary output. **Причинный сигнал:** устранение old-moment interference совпало с улучшением выбранного ответа и предметного результата. Одно лишь различие weights не успех. **Отрицательный результат:** actual moments малы/согласованы или оба плеча одинаково неправильны; эффект только на локальном loss ограничивает вывод траекторией.

**Предметная область:** технический EN→RU после C# documentation. Рубрика полярности, модальности, ролей, чисел и границ; аналогичный перенос на другую программу проверять отдельно. Исходный провал C# из B0 этим переносом объяснить нельзя. Использовать отдельный research host: изменённоеm50 не должно пройти за точное историческое continuation proof. Защиту restore не обходить.

## C01 — условная LoRA functional identity при bare import

Нижний full-checkpoint codec хранит weights/keys/shapes, но Alpha входит в отдельный descriptor. **RP01 descriptor проверяет это; его исправлять по этой гипотезе не требуется.** Ссылки — N-NEURAL-03 в [02](02-NEURAL-SCORES-AND-STATE.md#4-n-neural-03--full-checkpoint-weights-не-кодируют-lora-alpha-rp01-защищён-descriptor).

Один уже обученный checkpoint сB≠0, те же inputs/keys/rank: A bare import вalpha16; B вalpha32. Измерить `xAᵀBᵀ` и effective residual: он должен измениться2×, если ненулевой. Логиты могут измениться без изменения argmax. Descriptor-controlled mismatch refusal — контроль существующей защиты; B=0 — отрицательный контроль, который скрывает mismatch.0 updates. Только если обнаружен настоящий bare caller без descriptor, переносить conditional границу в production finding.

## C02 — Alpha0 и weight digest без изменения функции

Дополнительная условная проверка, низкий приоритет. A alpha0, B alpha16 из одинаковогоA≠0/B=0; один одинаковыйupdate, всё остальное frozen. Приalpha0 adaptergradients0, но AdamW decay способен менятьAbytes. Поэтому нужны actual gradients/moments/residual/logits, а не только weightsChanged. Отдельно alpha0/decay0 против alpha0/decay.01 проверяет происхождение hash drift. **Фактический RP01 alpha16 исключает это объяснение**, а Oct18 live8 имеет ненулевые first moments. Не расходовать приоритетный бюджет восстановления на этот компонент без достижимого caller.

## После чего выбирать следующий опыт

- R01 доказал доступ к legacy memory — продолжить один новый lesson+retention именно этого owner.
- R02 уже доставляет mappings — выбирать конкретный semantic contrast, не переписывать работающий learner целиком.
- R03/R04 дали лишь component gain — доказать обычного потребителя до заявлений о восстановленном продукте.
- R06 изменил траекторию, но не качество — сохранить отрицательный предметный результат; не объявлять8 updates достаточными для общего вывода о DPO.

Все дальнейшие опыты использовать как отдельные сравнения с неизменённым материалом/оценкой. Положительные старые результаты сохраняются вместе с границами их применимости.
