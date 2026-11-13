# DPO и LoRA: что именно сравнивать с external_domains и какие исправления сохранить

Дата: 2026-10-19. Статус: **SOURCE_REVIEW / эксперименты NOT_RUN**. Это ограниченный пакет внутреннего устройства и контрольных опытов; новые доказанные дефекты нейронного DPO здесь не заявлены.

Сравниваются `external_domains@3036a01acec938f0b83fa4ffbda26202cc35d639` и `codex/dpo-implementation@44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76`. Реальный исторический успех, сообщённый пользователем, принимается как основание искать работающий механизм и его потребителя. Он не означает, что каждая библиотечная реализация старого коммита обеспечивала этот успех.

## 1. В начале восстановления: разделить три фактически разных контура

В старом generic capability-контуре Transformer создаёт embedding запроса, HNSW возвращает примеры, затем lexical/pattern fusion выбирает **сохранённый `example.Output`**. Это конкретный обучаемый механизм, который может успешно воспроизводить знания, даже если отдельный нейронный DPO trainer имеет дефект. До: [external Unit, L141–168][old-unit-consumer]; после: [current Unit, L181–208][new-unit-consumer]. Для такого опыта смотреть только свободную генерацию токенов из `LlmModel` означает измерять другой потребитель.

| Свойство | external generic capability | current generic capability | current RP01 C# documentation |
|---|---|---|---|
| Конфигурация Transformer | H=32, 4 слоя, 4 attention heads, 2 KV heads, context=64, vocab=512 | Те же численные параметры в `BuildTinyConfig` | H=512, 4 слоя, 8 attention heads, 2 KV heads, context=2048, vocab=629 |
| Настройка LoRA | rank=2, alpha=4, dropout=0, q/v | rank=2, alpha=4, dropout=0, q/v | rank=8, alpha=16, dropout=0, q/v |
| Реальная роль рассмотренного Unit | Transformer embedding → HNSW → fusion → stored output | Тот же тип потребителя; состояния и допуски усложнены | Exact DPO над ответами и проверенным q/v recipient; качество свободного ответа требует отдельного измерения |
| Основание | [config L856–895][old-tiny], [stored output][old-unit-consumer] | [config L4048–4085][new-tiny], [stored output][new-unit-consumer] | [generation config L7–42][rp01-config], [реальный recipient L128–131][rp01-recipient] |

Размеры LoRA вычисляются из этих конфигураций и формулы `A[rank,in]`, `B[out,rank]` в [current LoraLinear L53–62][new-lora-shapes]. Для tiny q: A[2,32], B[32,2]; v: A[2,32], B[16,2]. Это 896 скаляров на четыре слоя **при установленном q/v recipient**. Для RP01 q: A[8,512], B[512,8]; v: A[8,512], B[128,8]: 53 248 скаляров, 16 тензоров A/B. Это расчёт числа параметров, не результат обучения. Для external нельзя называть 896 фактически обновлявшимися параметрами: ниже показано, что старый applicator лишь возвращал обёртки.

Отдельный четвёртый механизм — current `27.TranslationDomain`. `ExecutorUnit.TrainOnExample` при разрешённом non-heldout примере читает chosen, обновляет exact-source map и добавляет пары в KNN под semantic/prefix/structure keys [L403–422][translation-train]. Inference читает `FindNearestCascade(signature,prefix,structure,k:5,querySourceText)` [L333–345][translation-inference]. Это обновление таблиц/примеров; его нельзя описывать как AdamW над A/B. Для проверенного external root каталог `27.TranslationDomain` отсутствует; исторический аналог перевода надо устанавливать по соседней ветке отдельно. Наличие current generic18, RP01 и translation27 в одном solution не означает, что один запуск использовал все три.

### Точная функция нейронного обновления current

Для exact-пары обозначим через `lθ(y|x)` сумму log-softmax только response targets. Current вычисляет:

~~~text
margin_i = β [(lθ(chosen_i) − lref(chosen_i))
            − (lθ(rejected_i) − lref(rejected_i))], β = 0.1
weight_i = max(0.01, w_i) / Σ_j max(0.01, w_j)
signal_i = clamp(1 + active_slots_i / 60, 1, 2)
loss = Σ_i weight_i · signal_i · softplus(−margin_i)
~~~

Здесь `active_slots` — число компонент TeachingVector с `abs(value)>1e-6`; сами значения координат не становятся дополнительными нейронными targets. Реальный `StepVector` берёт противоположные upstream derivatives для chosen/rejected, суммирует вклады всех пар и **один раз** вызывает optimizer, затем ZeroGrad: [L858–923][vector-step], [VectorSignal L1160–1167][vector-signal]. Это формула exact-ветки: legacy loss reporting имеет отдельный весовой контракт, поэтому его не следует подставлять в эту формулу без оговорки.

LoRA forward добавляет `alpha/rank · (dropout(x) Aᵀ) Bᵀ` к base projection [L107–126][lora-forward]. AdamW для каждой обучаемой координаты обновляет `m=β1m+(1−β1)g`, `v=β2v+(1−β2)g²`, затем `w ← (1−lr·decay)w − lr·mHat/(sqrt(vHat)+eps)` [L72–106][adam-update]. DPO defaults — lr=1e-5 и decay=0.01 [L363–370][dpo-defaults]. Поэтому «вес пары ×2» не означает автоматически «смысловой эффект ×2»: адаптивная нормализация и контраст выбранного/отвергнутого действуют совместно. Эта тема уже принадлежит core30/P06.

### Короткий приоритетный опыт: вернуть тот же потребитель знания

**Задача.** Для одного уже существующего generic capability из удачного исторического сценария зафиксировать память примеров, токенизатор, embedding-модель, настройки индекса и те же запросы. Получить actual selected example IDs, итоговые stored outputs и составляющие fusion в external и current. Затем проверить ту же панель после одного разрешённого нового урока и после холодного восстановления. Никаких дополнительных DPO/LoRA изменений одновременно.

**Панель.** Сначала 6–12 запросов в том же предметном домене исторической capability. Для C# анализа — разные формы управления потоком, а не переименование одного класса; для orchestration — разные структуры требований, а не смена project name. Train и проверка не используют одну исходную семью. Перевод EN→RU сам по себе не является новым предметным доменом.

**Измерения.** Сначала технические: одинаков ли corpus, какие реальные IDs доступны индексу, тот ли output возвращается, сохранилась ли память после restore. Затем смысловые: CODEX с Astra 6 UltraCode оценивает реальные ответы по заранее принятому рубрикатору. Внешняя открытая модель допустима только как генератор новых DPO; она не выбирает лучшую ветку и не судит качество. Frozen corpus означает, что в сравнении A/B преподаватель заново пары не генерирует.

**Опровержение.** Если восстановленные memory/index/consumer равны и ошибок этой панели между контурами нет, различие здесь не объясняет потерю качества. Если меняются только нейронные likelihood, а stored-output consumer выдаёт то же самое, нельзя объявлять ни восстановление, ни ухудшение capability по этим likelihood. Это конкретизация известных проверок P24/P25 и historical-consumer recovery, не новая гипотеза.

### Что уже показали сохранённые RP01 опыты

Два первичных `scientific-result.json` различают изменение параметров и качество результата. Они прочитаны как существующие research receipts; checkpoints и все grade bindings заново не исполнялись и не пересчитывались.

| Сохранённый опыт | Нейронное состояние | Свободные ответы | Предел вывода |
|---|---|---|---|
| C# main500 | 500 пар, 50 updates; изменены 16/16 разрешённых adapter tensors, base 0/39; записано успешное reopening weights/optimizer/RNG | C0 **0/3**, T **0/3**; все 3 пары ответов побайтно одинаковы | Незначительный preference сигнал не стал улучшением ответа на этой маленькой панели; независимость semantic families 25 preference cases не установлена |
| EN→RU continuation | Ещё 500 пар, 50 updates, global step100; изменены 16 LoRA tensors / **53 248** elements, 39 base tensors сохранены; frozen C50 reference unchanged | C0 **0/4**, T **0/4**, все ответы invalid | Сам receipt квалифицирует результат как margin-only, без полезного translation skill |

Источники: [C# main500 L8–53][csharp-scientific]; [EN→RU state/semantic L11–55][enru-scientific]. EN→RU receipt прямо фиксирует, что original saved B0 был random/untrained, **и сразу ограничивает это как source limitation, а не доказательство единственной причины** [L144–149][enru-limitations]. Такая оговорка сохраняется здесь. Нельзя выводить из этих результатов, что не работали все успешные исторические KNN/Unit сценарии external_domains; измерялся конкретный свободный Student и маленькая панель.

Это усиливает приоритет уже известной P02 readiness/warm-start проверки. Сначала получить actual свободные ответы исходного B0 и имеющегося обученного стартового checkpoint на новой закрытой предметной панели; затем при наличии подходящего warm checkpoint сравнивать одинаковый небольшой frozen DPO batch и один update в одинаковом q/v recipient. Меняется стартовое обученное состояние, а не маски/optimizer/label policy одновременно. Отдельно записать стоимость подготовки warm state. Если подходящего warm checkpoint нет, это незакрытая prerequisite гипотезы SFT→DPO, а не повод назвать synthetic margin достаточной исходной способностью. Этот опыт — уточнение известного cold-start/SFT/DPO различия, новый causal ID ему не присваивается.

### Как один исходный DPO превращается в записи нескольких Units

В current `TranslationStructuredBatchBuilder.BuildGeneralBatch` один принятый `TranslationTrainReadyPair` становится **тремя** записями: SemanticTokenizer, направленный Executor, QualityVerifier. Это проекция учебного материала, а не три независимых исходных урока. Код создаёт новый rule tokenizer, в режиме `ExactPair` добавляет `|PAIR:<id>` в signature и дополняет её тегами из rejected. [Материализация, строки 77–142](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L77-L142).

| Получатель | Реально построенный target | Где возможно потерять исходный контраст |
|---|---|---|
| SemanticTokenizer | Pattern/signature от имеющегося tokenizer; отрицательная сторона `UNKNOWN` и `WRONG|signature` | Подтверждение собственного признака не тождественно обучению новому синтаксическому правилу |
| Executor | Исходный chosen, word/phrase mappings; rejectedCandidate и фиксированные confidence 0.93/0.31 | `BuildRejectedCandidate` при отсутствии явного rejected и не-omission типе возвращает сам chosen: нужно проверять реальное смысловое различие итоговых сторон |
| QualityVerifier | Вход — rejectedCandidate; сконструированный target с отрицательной оценкой 0.42, requiredTerm и correction=chosen; alternative с оценкой 0.91 | Builder не выполняет независимую проверку качества: он создаёт эти targets; маркер слова не является полной оценкой перевода |

Источники: [поля трёх записей, строки 106–141](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L106-L141), [fallback rejectedCandidate, строки 269–289](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L269-L289). Это уже известная семья [структурных Unit-пробелов](../21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md); новых номеров ей не присваивается.

Малый опыт G4-Units: взять две реально принятые DPO-пары — omission и иной тип смысловой ошибки — и сохранить исходные строки рядом со всеми шестью Unit-записями. Сравнить текущую проекцию с отдельно проверенной предметной проекцией; менять только одно действительно потребляемое поле target одного Unit, остальные записи держать одинаковыми. Зафиксировать, какие поля реально читает `TrainOnExample`: Executor сохраняет chosen в exact map и KNN, а не оптимизирует rejected через нейронный DPO. Изменение одного `Executor.RejectedOutputJson` при том же chosen служит отрицательным контролем: на этом пути ожидается отсутствие изменения памяти и ответа. Положительное плечо должно менять проверенное потребляемое поле chosen/word mapping, сохраняя смысл принятой пары. Затем проверить 6 новых случаев той же ошибки и 4 старых правильных ответа, до/после холодного восстановления. Если потребляемый target исправлен, но обычный ответ не меняется, проверить получателя и маршрутизацию; новую тренировку LoRA автоматически не добавлять.

### Токенизация, вектор и ключ памяти: четыре разные сущности

У RP01 `OwnerInput` кодируется BPE без E10. У tiny generic capability tokenizer обучается до vocab 512 на seed, текущем teaching text и 512 `semantic_seed` строках; embedding Unit ограничивает вход `min(64, ContextLen)`. У перевода semantic/prefix/structure signature — символические ключи, а у C# AST KNN используются структурные координаты узлов. Эти числа и алгоритмы нельзя переносить между контурами как общие «параметры токенизации».

Источники: [tiny tokenizer и config, строки 4053–4085](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L4053-L4085), [embedding maxLen, строка 115](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L115), [переводческие ключи, строки 413–422](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/27.TranslationDomain/TranslationTrainedUnits.cs#L413-L422), [raw Student](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/14.ContinuousLearning/Training/CodeDocumentationStudentGeneration.cs#L62-L77).

Для каждого сохранённого состояния записывать exact encoder/version, ID-последовательность или координаты, отсечённый хвост, corpus IDs и query transform. Известные риск prefix-64 и tokenizer-key drift проверяются одним контрастом: смысловое различие поместить до/после границы, либо изменить только согласование train/query encoder. Размер вектора сам по себе не гарантирует сохранение порядка, отрицания или receiver identity. Предыдущие владельцы: [vector/KNN разбор](../05-SEMANTIC-VECTOR-KNN.md), [N28 key drift](../28-NEW-N28-TOKENIZER-TO-EXECUTOR-KEY-DRIFT-2026-10-16.md).

## 2. Не откатывать исправленный current DPO к старым библиотечным ошибкам

### 2.1. Адаптер должен участвовать в model.Forward

External `LoraApplicator.Apply` замораживал `model.Parameters`, создавал `LoraLinear` через reflection и возвращал список. В этом методе нет установки обёрток в attention; комментарий прямо говорит, что forward базовой модели не меняется: [L6–39][old-apply]. Current планирует конкретные projection slots, замораживает base, устанавливает пары через `lease.PopulateAdapterPair` и включает градиенты A/B: [L432–492][new-apply]. Это положительное различие current, которое надо сохранить.

Из одного этого метода нельзя сделать вывод, что **вся** external ветка не училась: старый код также содержит обучаемую память и KNN consumer, приведённые выше. Вывод ограничен данным способом установки LoRA и его использованием внутри old generic DPO.

### 2.2. Производная должна дойти от objective до A/B

External `BackwardLogProbs` заново делал forward, записывал в `logits.Grad` выражение `(softmax−oneHot)*scale` и заканчивался без вызова `logits.Backward`: [L219–263][old-backward]. Current сохраняет отдельно policy-графы выбранного и отвергнутого ответа [L1014–1030][new-graphs]; backward использует `(oneHot−softmax)*targetScale` и вызывает `logits.Backward(logits.Grad)`: [L1072–1120][new-backward].

Это необходимые исходниковые исправления цепочки, уже разобранные в [core41][prior-gradient] и [P10][prior-catalog]. Исходниковая правдоподобность не заменяет численного опыта на составной производной и не доказывает, что конкретная DPO пара улучшает свободный ответ.

### 2.3. Exact response mask и legacy mask — разные контракты

Current `TargetMask` выбирает exact response mask только для exact material; exact likelihood суммирует response targets, legacy усредняет по маске: [L1002–1016][new-mask], [L1036–1068][new-logp]. Current backward применяет величину каждого элемента маски и тот же sum/mean контракт [L1083–1116][new-backward]. Старый backward считал количество положительных позиций, а не сумму их весов [L229–260][old-backward]. Сохранять исправление дробных масок; его прежний owner — [planning38][prior-fractional].

При recovery нельзя незаметно заменить current exact-контракт старой `Compact(prompt) + " " + Compact(response)` и full-sequence mask. Эта старая материализация непосредственно видна в [external Unit L418–445][old-batch]. Сравнение objective следует делать как отдельный опыт с замороженными исходными строками и явной версией материала, сохраняя установленную LoRA и исправленный backward.

### 2.4. Проверенная гипотеза про probability floor здесь не подтверждается

В exact `ComputeTargetLogProbs` используется `logits[target] − max − logsumexp`, без `max(p, epsilon)` перед логарифмом: [L1052–1068][new-logp]. Поэтому конкретная гипотеза «exact backward расходится с forward из-за clamp малой вероятности» на этом участке исходников отвергается. Это не сертификат отсутствия всех численных проблем.

Для exact objective current использует устойчивый softplus и ветвь производной без вычитания из sigmoid, округлившегося в единицу: [L1131–1150][new-objective]. Legacy objective имеет иной путь. Отдельный уже известный finite-gradient → nonfinite-AdamW-state дефект [N48-02][prior-adam] не надо переименовывать в новую находку.

### 2.5. Dropout и tokenizer identity оценивать на реальном entrypoint

В RP01 конфигурации dropout=0 [config][rp01-config]. Рассмотренный simulation entrypoint дополнительно отказывает при ненулевом dropout и проверяет actual tokenizer SHA exact-материала: [L108–124][rp01-admission]. Поэтому неконтролируемый dropout chosen/rejected не является подтверждённой причиной на этом конкретном пути.

В более общем `DpoTrainer.CaptureBatch` expected tokenizer SHA не передаётся [L991–999][new-capture]; это уже известный N39, а не новая ошибка. Для каждого реального запуска нужно проверить, вызвана ли внешняя проверка с actual SHA. Нельзя переносить защиту simulation entrypoint на все callers или, наоборот, объявить её отсутствующей в RP01 только по общей функции.

## 3. Два коротких технических контроля перед любым новым learning A/B

### Контроль A: составная производная exact DPO (известный P10)

Использовать настоящий current код, зафиксированные reference/policy/base/A/B/optimizer, dropout=0, одни и те же точные ID и маски. Две пары с разными длинами ответов и общий prompt достаточны для первого численного контроля. Проверить несколько координат A и B в q/v первого и последнего блока конечной разностью objective и actual backward. Для контроля A нужны ненулевые B: при штатном старте B=0 нулевой градиент A сам по себе ожидаем, а не разрыв графа. Это диагностическое состояние; не переносить его как изменение алгоритма обучения.

Сохранять objective value, analytic/numeric gradients, выбранный epsilon, absolute/relative error и конкретные координаты. При неустойчивой FP32 разности сравнить несколько разумных epsilon и записать предел точности, а не назначать PASS по отсутствию исключения. Зелёный результат означает согласованную производную на этих состояниях; он не означает качество DPO корпуса. Расширение recipient и изменение learning rate в этот опыт не входят.

### Контроль B: полный forward и cached generation (известный P09)

На одном actual trained q/v checkpoint зафиксировать 5–10 token IDs и сравнить logits одинаковых позиций: полный forward и prefill/добавления чанками, например 2+2+1. Перед каждым плечом очищать KV cache. Обновлений весов — ноль; sampling не нужен. Это проверяет, что генератор потребляет то же обученное состояние. Если full/cache расходятся, сначала локализовать эту ошибку; увеличение числа DPO пар её не доказывает и не устраняет.

Domain-specific quality этап для обоих контролей строится только после технического опыта: C# документация с различными control-flow families; отдельная EN→RU панель внутри того же предметного домена, если этот сценарий действительно поддержан entrypoint. Замерять chosen log-likelihood и rejected log-likelihood отдельно от их разности, а также реальные ответы независимым CODEX: увеличение margin само по себе не доказывает улучшение выбранного текста. Это уже отмечено предыдущими материалами, поэтому новый номер гипотезы не присваивается.

## 4. Что проверено на новизну и кому принадлежит прежняя находка

| Рассмотренный кандидат | Решение этой проверки |
|---|---|
| Exact q/v gradient chain, GQA/RoPE | Уже core41/core47/P10; сохраняем исправления и предлагаемый численный контроль |
| TeachingVector превращается в scalar и взаимодействует с AdamW | Уже core30/P06; не новая находка |
| Дробная legacy response mask | Уже planning38; current исправленный backward сохраняется |
| Tokenizer SHA не связан с model на общей границе CaptureBatch | Уже N39; внешний current simulation caller проверяет actual SHA |
| Trained semantic tokenizer vs raw translation executor key | Уже planning28/N28; не новая находка |
| `_hasStructuredTraining` после heldout/no-op | Уже planning16/P53; влияет на статус/измерение Evaluate, не доказанное забывание или другая orchestration ветка |
| Перепроекция replay prompt/subphase losing detail | Уже PREPARETOGPL-UNITS-RESTORE/core13/P52 |
| LoRA A0 vs 2A0 при B=0 и нулевом optimizer | Уже `2026-10-19-language-restore/04a-lora-factor-experiment.md` |
| Finite gradient квадратом превращает AdamW state в Inf | Уже N48-02 |

Novelty проверялась по обоим README, релевантным полным предыдущим документам и доступной локальной текстовой выгрузке audit inventory. Это не заявление о полном чтении каждого вложенного файла двух репозиториев.

## 5. Пределы и метод

Нативного .NET в среде нет; обучение, numerical gradient, KV parity и независимая оценка ответов **не запускались**. Здесь новые model-quality receipts: **0**; прочитаны два сохранённых scientific-result отчёта. Предложенные контрольные опыты — same consumer recovery, исходная readiness, exact gradient, full/cache — явно PLAN/NOT_RUN. Кодовые связи подкреплены pinned источниками; результаты пользователя не заменены локальными игрушечными метриками.

Использованы действительно прочитанные repository skills: `code-examination-SKILL.md`, `experiment-design-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `human-communication-GLOSSARY.md`, `prompt-patch-authoring-SKILL.md`; также просмотрен `planning-dpo-authoring-SKILL.md`. Применялись правила проверки caller→consumer, понятной русской формулировки, одного причинного изменения и разделения механики/качества. Неактивные product-execution gates не превращались в требования к read-only исследованию.

[old-tiny]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L856-L895
[new-tiny]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L4048-L4085
[old-unit-consumer]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L141-L168
[new-unit-consumer]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L181-L208
[old-apply]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/10.LoRA/LoraApplicator.cs#L6-L39
[new-apply]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/10.LoRA/LoraApplicator.cs#L432-L492
[new-lora-shapes]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/10.LoRA/LoraLinear.cs#L53-L62
[old-backward]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/11.DPO/DpoTrainer.cs#L219-L263
[new-graphs]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/11.DPO/DpoTrainer.cs#L1014-L1030
[new-backward]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/11.DPO/DpoTrainer.cs#L1072-L1120
[new-mask]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/11.DPO/DpoTrainer.cs#L1002-L1016
[new-logp]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/11.DPO/DpoTrainer.cs#L1036-L1068
[new-objective]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/11.DPO/DpoTrainer.cs#L1131-L1150
[new-capture]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/11.DPO/DpoTrainer.cs#L991-L999
[old-batch]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L418-L445
[rp01-config]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/config/code-documentation/generation-v1.json#L7-L42
[rp01-admission]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L108-L124
[rp01-recipient]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L128-L131
[prior-gradient]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/41-current-dpo-gradient-chain-source-review.md
[prior-catalog]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/83-check-catalog-and-prompts.md
[prior-fractional]: https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/38-NEW-LEGACY-FRACTIONAL-RESPONSE-MASK-GRADIENT-MISMATCH-2026-10-16.md
[prior-adam]: https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/02-NEW-HYPOTHESES.md
[translation-train]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L422
[translation-inference]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/27.TranslationDomain/TranslationTrainedUnits.cs#L333-L345
[vector-step]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/11.DPO/DpoTrainer.cs#L858-L923
[vector-signal]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/11.DPO/DpoTrainer.cs#L1160-L1167
[lora-forward]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/10.LoRA/LoraLinear.cs#L107-L126
[adam-update]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/04.TrainingLoop/Optimizers/AdamW.cs#L72-L106
[dpo-defaults]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/11.DPO/DpoTrainer.cs#L363-L370
[csharp-scientific]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/.kiro/xiigen-slices/work/results/RP01-simulation-main500-execution-20261015/scientific-result.json#L8-L53
[enru-scientific]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/.kiro/xiigen-slices/work/results/RP01-enru-continuation-20261016/runtime-20261016-01/analysis-20261016-01/scientific-result.json#L11-L55
[enru-limitations]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/.kiro/xiigen-slices/work/results/RP01-enru-continuation-20261016/runtime-20261016-01/analysis-20261016-01/scientific-result.json#L144-L149
