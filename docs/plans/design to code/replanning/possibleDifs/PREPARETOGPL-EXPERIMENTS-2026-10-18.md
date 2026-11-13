# Короткие эксперименты: восстановление PrepareToGPL и новые различия

Дата: 18.10.2026. **Это план native-проверок, не отчёт об их исполнении.** Исключения — два приложенных source/proxy результата, явно обозначенные в PG-01 и PG-09. Приоритеты и уже выполненные другими результаты — в [главном списке](00-PREPARETOGPL-RECOVERY-FIRST-2026-10-18.md). Подробные before/after источники находятся в [KNN](PREPARETOGPL-KNN-HISTORY-2026-10-18.md), [DPO/LoRA](PREPARETOGPL-DPO-LORA-TOKENS-2026-10-18.md) и [Units/restore](PREPARETOGPL-UNITS-RESTORE-2026-10-18.md).

## Общий минимальный протокол

Каждый опыт меняет один заявленный фактор. Если это невозможно, результат называется сравнением двух конфигураций, а не доказательством одной причины. Начальные tensors, tokenizer, memory corpus и обычный caller фиксируются по содержимому; одинаковое имя модели или seed без сохранённого состояния недостаточно.

Для каждого задания сохраняются input, ожидаемое предметное свойство, фактический output, выбранный Unit/Flow и состояние A/B. Gold используется только оценщиком после ответа. Для retrieval дополнительно нужны query/ExampleId/кандидаты и состав памяти; для neural — token IDs, mask, точные trainable slots, grads/updates и reference identity. Данные без фактического измерения обозначаются null, а не нулём.

Отчёт показывает **correct/all, incorrect accepted, abstain/fallback**, отдельно exact recall и новые формулировки/сочетания. Время и RAM процесса, размер состояния, число уроков/optimizer steps фиксируются доступным счётчиком; VRAM и нейронные слои для чистого memory update — N/A. Маленькая панель различает механизм на выбранных случаях, не доказывает универсальное улучшение.

В опытах с новыми DPO роли сохраняются: **CODEX / запрошенная astra 6 ultracode** выбирает закон и учебный случай; разрешённая внешняя открытая китайская модель создаёт **обе** окончательные стороны chosen/rejected; другой CODEX в той же запрошенной конфигурации независимо проверяет точную пару и ответы. Фактический provider/model/version записывается по runtime receipt. Отсутствующую аттестацию нельзя заменить надписью в плане.

Предложенные ниже примеры — спецификация панели для исполнения, не выдаваемые за внешне созданные и одобренные DPO.

## PG-01. Первым: старый четырёхнаправленный checkpoint в нынешнем runtime

**Задача:** восстановление памяти перевода PrepareToGPL. **Причина:** current требует шесть исполнителей от schema 1, а прежний сохранённый asset содержит четыре. [Точные строки и состав состояния](PREPARETOGPL-KNN-HISTORY-2026-10-18.md).

**Подготовка без обучения.** Взять неизменённый tracked checkpoint с blob a4cf054ba3f8a6ee6aa9818edc6c5705e3456c25. Сначала выполнить приложенный [preflight](PREPARETOGPL-CHECKPOINT-PREFLIGHT-2026-10-18.py) на old/current TranslationMachine.cs и checkpoint. [Результат этого аудита](PREPARETOGPL-CHECKPOINT-PREFLIGHT-RESULT-2026-10-18.json): old missing пусто; current missing EN_ES/ES_EN; все три источника совпали с API blob SHA. Это проверяет source contract, не заменяет native loader.

**Три состояния.** O — старый runtime и точный старый файл; A — current runtime и тот же файл; B — current runtime и отдельная диагностическая копия с добавлением ровно двух пустых ExecutorUnit ES_EN/EN_ES. Рекомендуемый будущий продуктовый ремонт — явный migrator/versioned topology, а не ручное редактирование пользовательского состояния. В диагностике не менять старые поля и не обходить проверки происхождения на верхнем уровне.

**Сначала измерить:** native LoadCheckpointAsync result/error; canonical hashes всех старых executor maps, shared KNN, tokenizer, verifier, history и identities. Для A ожидается EXECUTOR_DIRECTIONS_INCOMPLETE; для B — отсутствие именно этого препятствия. Другой отказ записывается как другая незакрытая причина. Missing RU_EN и duplicate direction в отдельных копиях должны по-прежнему давать отказ.

**Затем ответы:** выполнить одну фиксированную панель через обычный GetOrchestratorAsync().RunAsync в O и B. На маленьком tracked fixture проверить доступность его фактической записи; не оценивать английский source под RU_EN как корректный русский перевод. Для claim о восстановлении хорошей модели отдельно идентифицировать богатый успешный snapshot: 4 прежних точных запроса, 4 новых формулировки, 4 перестановки/отрицания, без новых уроков. Сравнить и state, и реальный ответ, и правильность: совпадение ошибочных ответов означает только parity.

**Что опровергает гипотезу для пользовательского провала:** фактически использованный snapshot уже шестинаправленный; отказ отсутствует по native path; либо проблема возникает после успешного полного импорта. **Стоимость:** 0 optimizer steps, 0 новых уроков; только несколько загрузок и ответов. P25 служит уже доказанным положительным контролем современного шестинаправленного cold restore.

## PG-01G. Другой путь восстановления: orchestration guide schema 1 → 2

Это **не новая находка**: [G40-01/02 и P40-A/B коллег](40-PREPARETOGPL-RECOVERY-PRIORITY-CHAPTER-2026-10-18.md), [дополнительные проверки asset/consumer E42-A/B/C](42-NEW-CAUSAL-HYPOTHESES-AND-DOMAIN-MICROEXPERIMENTS-2026-10-18.md). Внесено в начало, поскольку тоже относится к возврату старой способности. В этом аудите дополнительно прочитаны current ValidateCheckpoint и tracked named JSON: [loader отвергает schema1 и требует verified receipt](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/43.SmartSkillsDomain/OrchestrationReasoningMachine.cs#L972-L1015); [asset](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/models/orchestration-reasoning/orchestration-reasoning-v1/orchestration-reasoning-v1-checkpoint.json) имеет schema1, 8 train rows и15 patterns.

Самый дешёвый опыт — native load точных старых checkpoint+manifest в соответствующих old/current loaders, без обучения: actual path/hash, LoadErrorCode и возможность обычного Plan consumer использовать state. Не считать models/index единственным путём: в каталоге коллег указан direct manifest caller и два разных checkpoint filenames.

**Отличие от translation PG-01:** [текущий source-admission ledger](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/training-data/orchestration-reasoning/source-admission-ledger.json) имеет entries=[] и computed_training_allowed=false. Требуется восстановление допуска исходных уроков и новый проверенный v2 state; замена числа schema не является миграцией. Исторические результаты сохраняются как архивные, недопущенные строки не переобучаются и не получают вымышленный proof.

После существующего разрешённого admission — отдельное сравнение 4 прежних и4 новых source-disjoint prompts, raw Plan output/зависимости/ошибки/отказы. Пока допуска нет, стадия нового обучения честно остаётся BLOCKED_BY_ADMISSION. Это наблюдаемая текущая граница, а не запрос разрешения на публикацию этого аудита.

## PG-02. Сохранить подтверждённый путь: подчасти урока → Executor → обычный ответ

**Задача:** короткие команды EN→RU, композиция знакомых действий и объектов. **Статус:** причинный A/B **уже выполнен другими**; исправление входит в current. [Отчёт 11/24 → 19/24](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md), [commit](https://github.com/lubochka/llm_mvp_core/commit/2487d2c43ba76f3efe31d2355188355952e42100).

В сохранённом результате reader/builder терял уже предоставленные phrase_mappings. Исправление передало их в тот же Executor: 8 exact records остались, phrase memory стала 16 вместо 0; 8 новых сочетаний стали правильными. Это полезный образец того, как проверять преобразование DPO в подуроки.

**Повторять только при переносе в новую среду/новый вход.** Те же 8 уроков из receipt: validate/file, delete/cache, copy/report, move/config, upload/image, download/package, save/draft, archive/log. Не заученные сочетания: delete/file, validate/cache, move/report, copy/config, download/image, upload/package, archive/draft, save/log. Сначала снять materialized Example до Train; затем память после Train и ответы ordinary Flow. Единственный фактор — перенос optional mappings, одинаковая Domain DLL.

Сохранить также do not delete the file, did you delete the file, delete two files, validate the file and the cache, delete the database. Они нужны для видимости смысловых границ, не для пересмотра уже принятого результата. Увеличение coverage не всегда повышает долю правильных среди принятых.

**Для другого домена:** переносить принцип проверки транспортного поля, а не английские фразы. Например, в C# отдельно проследить сохранение null-condition или порядка operand в structured lesson до получателя. Нельзя менять mappings, verifier и расстояние KNN одновременно.

## PG-03. Структура в KNN: восстановить полезную функцию без неверного вывода из E07

**Задача A:** предсказание следующего SyntaxKind в C# — именно такой target позволяет проверить исторический KNN. **История:** первичные E07 0.1531→0.1462 и E09 0.2413→0.3507 используют разные carrier/labels/panels. E07 здесь MultidimensionalNGram, не нейронная матрица. [Первичные файлы и ограничения](PREPARETOGPL-KNN-HISTORY-2026-10-18.md).

**Самый дешёвый первый опыт.** 32 небольших parseable C# метода, разделённые по исходным семействам; 24 query позиции из новых методов. До arms построить один стабильный Roslyn SyntaxKind словарь и один train/query manifest. Один и тот же KNN обучить один раз одинаковыми примерами. A — нынешняя установленная дистанция; B — та же дистанция с включённым одним исторически поддерживаемым компонентом, например parent/role, если A его не использует. Если current уже использует тот же компонент, записать NO_IMPLEMENTATION_DIFF и перейти к его отдельной ablation, не притворяться «переносом».

Фиксировать k, search mode, tie policy, остальные веса, encoder и порядок вставки. В отчёте 24 парных next-kind answers, разбивка по return/call/assignment/conditional context. Source-bound split и остановка переходов на границе файла одинаковы. Если эта ограниченная целевая метка не различает перестановку операндов, она не может доказывать сохранение их смысла.

**После подтверждения пользы KNN-компонента** можно сравнить carrier matrix/таблица/KNN на одном target и одинаковом feature tensor. Минимальная 2×2 постановка: carrier × наличие конкретного структурного признака; одни labels, sample IDs и budget. Это отдельный опыт, потому что исторические E07/E09 не образуют такие чистые arms. Нейронную матрицу нельзя назвать проверенной, если исполняется словарь n-gram.

**Задача B, отдельный языковой профиль.** 8 уроков неоднозначного слова и 12 новых контекстов: существительный/глагол book, действие/object file, неоднозначное bank; actual pairs и gold готовятся указанными ролями. Frozen dictionary/corpus; A — без left/right POS в расстоянии, B — тот же словарь с историческим POS-context; не менять порядок и POS одновременно. Отдельная последующая проверка order: subject/object reversal и negation при одинаковых словах. В ContextualDictionary Any-overlap внутри каждой стороны ещё не учитывает полный порядок.

**Критерий:** улучшение на закрытых контекстах при неизменной метке и consumer; ухудшение/нейтральность в другой группе сохраняется в отчёте. AST для C# не переносится автоматически на перевод, SQL или простой lookup. SQL потребует собственного parser/role contract.

## PG-04. Память истории отдельно от бюджета gradient replay

**Задача:** возврат ранее выученного закона generated capability. [U-R1/U-R2](PREPARETOGPL-UNITS-RESTORE-2026-10-18.md) показывают old history-union против current signed corpus и несовместимость старого bundle с executable-v2.

**Предусловие:** связать реальный successful bundle с old runtime, входом и содержательным ответом. В этом аудите такой receipt именно DefaultLogicalCapabilityTrainer не найден. Если его нет, состояние опыта NEEDS_DONOR; синтетический manifest не заменяет найденную историческую способность.

Для старого bundle отдельно проверить импорт: schema/config/tokenizer/model hash, descriptor IsLearned, provenance и полный состав памяти. Importer должен иметь явный legacy contract. Нельзя подделать replay/manifest подпись или менять IsLearned для обхода проверки.

После законного импорта A — нынешняя память версии; B — та же policy/tokenizer и новый canonical corpus, куда добавлен ровно один проверенный исторический пример, отсутствующий в A. Веса не учатся. IDs уникальны и source-bound в обоих arms: old duplicate-ID map overwrite не восстанавливается. Панель — 4 новые формулировки возвращаемой способности, 4 retention cases, 4 неприменимых/противоположных условия. Ordinary registered Flow обязателен; прямой поиск в тестовом словаре недостаточен.

**Подтверждение:** B выбирает отсутствовавшую полезную запись и возвращает требуемую обязанность без потери контролей. Совпадение memory counts без полезного выбора — только state difference. Если API формирует исключительно subphase scaffold, измерить только этот scope-level результат и прямо ограничить claim.

**Ремонт после различения:** сохранение предыдущего проверенного retrieval corpus с отдельной retention policy, созданием нового corpus hash и перестройкой индекса. Optimizer replay остаётся отдельным бюджетом. Его увеличение не требуется для нулевого memory A/B.

## PG-05. Исходная модель и расходование RNG при cold restore

**Задача:** новые generic capability families; восстановление сравнимого старта с PrepareToGPL. [DLT-1](PREPARETOGPL-DPO-LORA-TOKENS-2026-10-18.md).

В двух свежих процессах создать одну целевую модель. A — сразу; B — сначала выполнить ровно дополнительные cold policy/reference constructors реального current caller, затем целевую модель. Записать наличие внешнего AsyncLocal seed scope. Сравнить именованные initial tensors, LoRA-A, первые logits и embeddings; **0 optimizer updates**.

Следующий контроль меняет только способ инициализации целевой модели: одинаковый независимый seed scope вокруг её конструктора и LoRA. Если hashes A/B не выровнялись, причина не изолирована. Scope не возвращает старые полезные веса; для настоящего donor comparison использовать сохранённый old B0/состояние.

Только после подтверждения зависимости — один домен, 8 разрешённых уроков, 12 закрытых запросов через тот же consumer. Использовать конкретные сохранённые начальные состояния A и B из zero-step сравнения, а не произвольную «хорошую модель». Отдельно показать initial competence и after-learning competence. Это проверка всей зависимости инициализации от очередности. Дополнительные конструкторы меняют и base, и LoRA-A; если нужен вывод именно о base, в отдельном сравнении зафиксировать одинаковые initial adapters. Разница hash без разницы ответа не является падением обучения.

Для нейронного RP01 вопрос другой: его сохранённый B0/C50 не заменять новой моделью. Если найден подтверждённый historical neural donor с изменённой output head, сравнить этот recipient с q/v-only на тех же исходных tensors в отдельном последующем опыте. Указатель на такую узкую историю есть в [core/25](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/25-historical-neural-learning-donor.md); этот аудит не повторял чтение его бинарных checkpoints и не связывает их автоматически с хорошей моделью PrepareToGPL пользователя. Сначала проверить, что frozen base умеет хотя бы выражать целевой доменный ответ; preference win-rate не тождественна free generation. Rank, targets, pretrained base и optimizer нельзя менять одним пакетом и приписывать эффект одному фактору.

## PG-06. Subphase присутствует в metadata, но не участвует в KNN

**Задача:** один многошаговый запрос в одном предметном домене, две разные обязанности. Пример: «сначала проверь ограничения входа, затем опиши изменения схемы». **Доказанный source chain:** ordinary caller передаёт исходный UserRequest каждому binding; factory выбирает latest Unit по role/domain; scorer читает request.Input. [U-N1](PREPARETOGPL-UNITS-RESTORE-2026-10-18.md).

Две реальные subphase с разными именами/ID, по 4 source rows в каждой, один newest registered KNN и frozen model/tokenizer. **Current BuildChosen производит одну строку на subphase**, поэтому 8 rows не означают 8 различных предметных ответов.

A — нынешнее общее пространство кандидатов. B — те же примеры и query, но eligibility кандидата ограничена явно требуемой subphase. Никакого дополнительного query prefix, новой embeddings модели или reranker. Если текущий corpus не хранит scope как typed field, derived view строится из проверенных provenance-ID связей; эта связь должна быть наблюдаемой.

8 составных запросов, на каждом две реальные binding invocation; ещё 4 single-duty и 4 запроса, для которых нужной обязанности нет в памяти. Сравнивать сырой retrieved output и выбранный ExampleId с фактически требуемой обязанностью; добавленная Flow annotation PASS не является правильным ответом.

Permutation control возможен только между **разными** допустимыми scope labels в отдельном честно зарегистрированном тестовом источнике. Переставлять идентичные chosen внутри одного scope бессмысленно. Если B оставляет единственную label, успех показывает согласованность маршрута, не обучение предметному закону.

**Фальсификатор:** реальные invocation имеют разные registry/unit/input; scorer уже получает нужный scope другим путём; или ответы B по содержанию не лучше. **Дальнейший ремонт:** typed retrieval request с subphase/task type и наблюдаемым context либо scoped memory views. Обучение закону внутри subphase — отдельный опыт только при реальном producer, который несёт такие outputs.

## PG-07. DPO видит ветви, KNN — один сгруппированный документ

**Задача:** найти правильную subphase по признаку поздней ветви. [U-N2](PREPARETOGPL-UNITS-RESTORE-2026-10-18.md). Не обещает различать разные outputs внутри одной subphase: текущий projector их не создаёт.

Взять 4 настоящие branch rows, по 2 в каждой из 2 subphase. Проверить настоящие token IDs и положение различающего фрагмента: он должен находиться внутри 64-token budget отдельной branch row и после границы в grouped representation хотя бы у выбранных длинных cases. Если это условие не достигнуто, гипотеза для них не активна.

A — один grouped registration document на subphase. B — отдельные branch records с уникальными source-bound IDs, но с тем же законным chosen соответствующей subphase. Model, tokenizer, BM25 параметры, distance weights, scope eligibility и запросы одинаковы. Это вмешательство в granularity memory; изменение BM25 document length является его наблюдаемым следствием и сохраняется в trace.

Панель: 8 запросов по поздним branches, 4 по ранним, 4 unrelated/negative. Дополнительный короткий контроль — те же смысловые категории, где обе части помещаются в budget. Измерять nearest scope, raw output, lexical/embedding score отдельно и final decision.

**Подтверждение:** отдельная запись делает нужный scope доступным именно по признаку поздней ветви. **Опровержение:** признак и так виден, BM25 полностью компенсирует отсечение или ordinary ответ не изменяется. Это не новое открытие самого cutoff64: новизна — изменившееся branch→registration представление. Old одинаковые IDs и output overwrite не копируются.

## PG-08. Предпочитаемая разница должна пережить DPO → tokens → recipient

**Задача:** качество самих пар и сохранение предметного контраста. Это развитие уже известных DPO/tokenization/Unit карточек, не новая находка.

По одному профилю за раз выбрать 4 закона × 2 учебные пары. Примеры законов: C# — && против || в условии допуска, null guard, порядок subtraction, необходимый return; SQL — NULL/IS NULL, AND/OR с группировкой, WHERE/HAVING, INNER/LEFT JOIN; EN→RU — do not, agent/patient reversal, question/command, scope двух действий. Эти примеры — спецификация, окончательные обе стороны даёт внешний teacher, CODEX проверяет смысл и близость near-miss.

**Первый шаг без обучения:** сохранить исходный exact prompt/chosen/rejected, materialized subunits, token IDs, masks и фактически обучаемое target. Проверить, не стали ли законно разные стороны одинаковыми после нормализации, неизвестных tokens, cutoff или templating. Для DPO нужен предпочтительный response contrast; для chosen-only memory отдельно указать, куда должен поступать rejected и обещает ли Unit его учитывать.

**A/B для одного обнаруженного преобразователя:** A — current; B — сохраняет ровно один утрачиваемый элемент, например отрицание либо phrase mappings. Не улучшать сразу teacher, tokenizer, truncation и scope. 8 разрешённых уроков, 12 новых закрытых случаев: 4 paraphrase, 4 композиции, 4 near-miss. Начальное состояние одинаково. Judge не видит arm, принимает фактические outputs.

**Для настоящего neural DPO** записывать objective margin и свободный ответ раздельно; одинаковый optimizer budget 4–8 updates лишь после source check. Если отличие сохранено, updates ненулевые, но ответы остаются неправильными, перейти к способности base/head/recipient. Увеличение количества одинаково разрушенных пар не устраняет преобразователь.

## PG-09. FP32 стирает малую разницу до stable loss

**Задача:** реальные длинные C#/ENRU ответы и почти совпадающие пары. [DLT-2](PREPARETOGPL-DPO-LORA-TOKENS-2026-10-18.md). [Выполненный numeric proxy](PREPARETOGPL-DPO-FP32-PROXY-RESULT-2026-10-18.json), [скрипт](PREPARETOGPL-DPO-FP32-PROXY-2026-10-18.py).

Proxy на 64 log-prob terms показывает float sum difference 0 при double difference 0.000010013580322265625 на тех же представимых float входах. Это не logits RP01; около margin0 соответствующая ошибка производной мала, gradient DPO не становится автоматически нулевым.

**0 updates:** захватить реальные float logits и уже вычисленные current per-token float logP policy/reference для 8 длинных и 8 коротких допущенных пар. A — current float totals; B — double накопление **тех же float logP**, без изменения log-sum-exp. Сохранить double через промежуточные носители BatchLogProbs/PolicyLogProbGraph и разности до итогового margin: раннее приведение к float снова потеряет проверяемый сигнал. Пересчёт самого log-sum-exp в double — отдельный подопыт после проверки totals. Записать response lengths, per-token deltas, абсолютный/относительный error, sign flips, zero/nonzero и derivative difference. Common-prefix, early-difference и late-difference включаются как отдельные виды случаев.

Если отличие на реальных данных не влияет на решение/направление в заранее установленном численном допуске, гипотеза закрыта для этого запуска. Только при существенном различении — 4–8 native updates с одним изменением reduction dtype и закрытая предметная панель. Изменившиеся последние цифры — NUMERIC_DIFFERENCE_ONLY. Возврат к mean objective либо изменение beta не является чистой проверкой precision.

## PG-10. Одноэлементный attention и бесполезные Q-параметры для данного урока

**Задача:** атомарные метки/операции, если настоящий materializer дал PromptLength=1 и полную длину chosen/rejected sequence=2. [DLT-3 и точные исходники](PREPARETOGPL-DPO-LORA-TOKENS-2026-10-18.md).

Сначала проверить реальные IDs. При BOS/instruction, двух prompt tokens или нескольких targets эксперимент неприменим. Нельзя искусственно сжать длинный production prompt и приписать ему результат.

4 допустимые пары; один native forward/backward без optimizer, dropout0. Для q и v отдельно A/B grad-null, norm, maxabs. Для q ожидается 0 из-за единственного доступного key у позиции0; v.B даёт положительный контроль на невырожденной паре. При LoRA B=0 нулевой A-gradient в первом шаге нормален; weight decay может изменить A без содержательного gradient.

Отдельный положительный контроль с двухтокенным контекстом проверяет появление зависимости от Q; это проверка границы математики, не обещание улучшения на любом примере. Если атомарный профиль существенен, самостоятельное quality сравнение q/v против v-only сохраняет v-rank и initial v tensors. Это разные parameter budgets; их следует показать честно. Увеличение v-rank или добавление head — следующий фактор.

**Вывод:** expected zero Q-gradient не означает неверный LoRA implementation. Исправлять требуется ошибочный выбор/ожидание обучаемого recipient только если предметная способность действительно в нём недостижима.

## PG-11. Готовый pattern и контраст chosen/rejected: проверить контракт

**Задача:** orchestration reasoning materializer. [U-N3](PREPARETOGPL-UNITS-RESTORE-2026-10-18.md). Этот API отвергает training_started/external_ai_used/dpo_train_ready и сообщает SemanticQualityClaimed=false; его нельзя выдавать за neural/external-teacher DPO trainer.

Сначала найти настоящий достигнутый payload с learned_pattern. Для shipped expanded dataset текущий ledger пуст, а source admission выполняется до BuildPattern; там статус NOT_REACHED / BLOCKED_BY_ADMISSION, см. PG-01G. Если другого допустимого входа в проверяемом learning path нет, опыт не продолжается. На отдельном законном fixture A/B получают одинаковый готовый pattern и разные допустимые chosen/rejected feature arrays; происхождение новых test sources регистрируется честно, старые receipts не подделываются. Сравнить сформированный semantic state и 6 structured queries.

Ожидаемая инвариантность подтверждает приоритет готового pattern. Если это импорт fixture/восстановление, результат соответствует контракту. Если caller рассчитывал на новое обучение из предпочтений, нарушен его выбор API/контракт передачи материала. Только после этого отдельно проектировать learner, который использует near_miss_signals как отрицательные ограничения; один A/B materializer не доказывает существование такого learner.

## Минимальная форма результата исполнителя

Для каждого PG достаточно одной связанной записи: source SHA; выбранный domain/profile; фактически вызванные методы; хеши input и начального состояния; единственная интервенция; число уроков/updates; raw outputs и парная таблица; различия learned state; ресурсные наблюдения; независимая оценка; вывод и ограничения. Если precondition не выполнен, сохранить NOT_REACHED/NEEDS_DONOR/NO_IMPLEMENTATION_DIFF, объяснив конкретный недостающий факт.

Успешный исход первого restore опыта позволяет сказать «прежнее состояние и поведение перенесены без обучения». Успешный learning A/B позволяет сказать «данное отличие изменило ответы на этой закрытой панели». Для утверждения «обучение снова работает как в PrepareToGPL» нужны связанный успешный donor, тот же получатель, сохранённая старая компетенция и улучшение на новых предметных случаях.
