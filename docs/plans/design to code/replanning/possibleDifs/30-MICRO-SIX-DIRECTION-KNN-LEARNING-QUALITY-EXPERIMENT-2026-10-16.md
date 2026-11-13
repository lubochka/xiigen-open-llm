# MICRO-N30 — быстрый причинный A/B: обученная память действительно влияет на перевод?

<!-- POST117-CURRENT-2026-11-03:START -->
## Актуализация после P117

[текущая сверка после P117](codex-2026-11-03-POST117-TRANSFER-AND-EXPERIMENT-AUDIT.md) фиксирует уже записанный RU_EN результат N30-A: доступ к памяти 0→1 из 5 без semantic gain. Предлагавшиеся ниже EN_ES и direction-namespace контрасты не следует подменять этим результатом или автоматически запускать следом. Сначала сохранённые данные и текущие ветви; бюджеты/gold старых опытов неизменны.

Обязательные дополнительные проверки для отдельно назначенного переноса: actual KNN export/import с same-count corrupted target, путь контроля без перехвата exact-source, same-bucket distractor, reverse candidate order, полноценный ответ и настоящий отказ. Все результаты, включая copyFaithful/cold/reload, должны влиять на итог; отсутствующий флаг не равен true. Контроль реализации и все физические попытки входят в общий ledger. Исторические разделы ниже сохраняют свой источник и исходные проектные дозы; действующая копируемая цитата в§5 направляет сначала к saved evidence.
<!-- POST117-CURRENT-2026-11-03:END -->


**Дата:** 2026-10-16. **Статус:** READY_TO_IMPLEMENT / NOT_RUN_IN_NATIVE_DOTNET / NOT_INDEPENDENTLY_GRADED. **Система:** реально существующие TranslationKnnStore, ExecutorUnit, TranslationMachine, TrainedTranslationAdapter. **Цель:** не сделать тест зелёным, а измерить пользу и вред уже запомненного урока на незнакомом обычном запросе.

**Независимое обнаружение:** [N30-A direction-gated KNN / N30-B shared-memory cross-language](30-NOVEL-DIRECTION-GATED-KNN-AND-SHARED-CROSS-LANGUAGE-CANDIDATES-2026-10-16.md). Это два эксперимента, их не объединять в один патч. Исходники pinned: llm_mvp_core@79082340f01b744cdf08185616f2bd818dbd06b8. Каталог core docs/plans/replanning-possible-diffs **READ_ONLY**.

## 0. Общие правила научной проверки

- **Одна причинная переменная за A/B:** P1 только условие вызова ordinary KNN, P2 только namespace направления. Не менять одновременно LoRA, BPE, matrix scoring, training set, rank, top-k, source guards, verifier или train order.
- **Три уровня:** L0 source/native mechanics; L1 ordinary consumer before/after learning quality; L2 source-family-disjoint transfer и повторное открытие checkpoint. Нельзя назвать L0 «обученная модель улучшилась».
- **Заморозить до запуска:** core commit SHA, fixture+gold SHA256, source IDs, exact train order, direction, computed signatures, labels, withheld families, control queries, judge rubric. Запрещены ручные замены провалившихся кейсов.
- **Считать всегда:** correct, wrong, abstain, coverage, correct/total, correct/answered, error-at-coverage, negative-transfer rate, latency, adapterId, memory direction provenance, zero-example behavior, cold restore parity.
- **Внешний судья не видит, какой arm отвечал:** русская объяснимая rubric по negation, actor/recipient, grammatical number, terminology и target language. Пример input не является достаточным доказательством. Требуется source-disjoint и отрицательный контроль.
- **Не тратить время на GitHub Windows workflow**, GPU и 500 DPO. Один disposable C# runner/адресный in-process test, затем только при signal небольшой end-to-end TranslationRoutingFlow A/B.

## 1. P1: N30-A — KNN для обычного EN_ES запроса недоступен

**Hypothesis H30-A:** после TrainOnExample EN_ES KNN-pair присутствует в storage, но обычный source без semantic concepts не доходит до FindNearestCascade. Изменение только ветки eligibility (в shadow, на frozen data) меняет ответ. Отрицательный контроль показывает возможный вред от слишком широкого lookup.

- **Training example**: source «large garden window», chosen «ventana grande del jardín», EN_ES; без WordMappings и PhraseMappings. SourceId training-family-01. Это исходная строка обучения, не отдельный blind family.
- **Blind query**: «big garden window» → независимое gold «ventana grande del jardín», source-string-02 (другая строка той же template family; **не** claim об unseen-family generalization). Сначала проверить, что tokenizer обеих строк выдаёт одинаковую signature SVO, а source guard на данном наборе пропускает candidate (overlap по garden и window: 2/3 >= 0.60 как в final Executor source guard). Если нет — кейс невалиден для этого механизма, не подгонять алгоритм.
- **A CURRENT:** обычный EN_ES Executor.Execute. Expected по source: adapterId fallback, confidence 0.49, KNN_NOT_INVOKED. Проверить нативно.
- **B SHADOW-ELIGIBLE:** ТОТ ЖЕ executor/knn store, но в shadow-копии функции разрешить ordinary-KNN для EN_ES с тем же minimumSourceSimilarity и law guard, **без** изменения score, labels или остальных пяти направлений. Ожидаемая наблюдаемая разница — candidate selected. Это ещё не доказательство правильного перевода всех парафраз.
- **NEGATIVE_CONTROL:** «small garden window» с независимым gold «ventana pequeña del jardín» и той же learned memory. Если B возвращает «ventana grande del jardín», учесть как WRONG. Нельзя выбирать только выигравший query.
- **OTHER DIRECTIONS:** повторить L0 для RU_EN/HE_EN/EN_HE/ES_EN только когда действительно нет семантического concept, и отдельно на технической строке «null field» проверить, что concept-branch **уже** достигает KNN: это не дефект eligibility на таком входе.
- **Граница evidence:** large/big/small garden window проверяют новый текст той же template-family, но НЕ unseen-family generalization. L2 требует иной семантической семьи с независимым gold и новым source ID.

**P1 decision:** только если B улучшает blind correctness при контролируемом вреде и consumer действительно использует этот адрес; иначе рассматривать избирательное retrieval или abstention, а не безусловное расширение.

## 2. P2: N30-B — новая EN_ES пара загрязняет EN_HE, хотя target другой

**Полный однофайловый исполнимый probe:** [30-MICRO-N30-NATIVE-SOURCE-PROBE.cs](30-MICRO-N30-NATIVE-SOURCE-PROBE.cs). Он проверяет обоих кандидатов, одну негативную ловушку и default verifier без записи в product source. Ниже сокращённый код для чтения; использовать полный source file как runner. Он проверяет только L0, не L1/L2 и не независимый билингвальный gold.

**Пример Windows PowerShell, локальный .NET 9 (не GitHub Windows CI):**

    $core = "C:\Projects\llm_mvp_core"
    $src = "C:\path\to\xiigen-planning\llm mvp\design to code session\replanning\possibleDifs\30-MICRO-N30-NATIVE-SOURCE-PROBE.cs"
    $tmp = Join-Path $env:TEMP "XIIGEN-MicroN30"
    dotnet new console --framework net9.0 --output $tmp
    Copy-Item $src (Join-Path $tmp "Program.cs") -Force
    dotnet add (Join-Path $tmp "XIIGEN-MicroN30.csproj") reference (Join-Path $core "27.TranslationDomain\27.TranslationDomain.csproj")
    dotnet run --project (Join-Path $tmp "XIIGEN-MicroN30.csproj")

**Перед выполнением** подставить существующий путь planning, убедиться, что checkout core именно pinned commit/его подтверждённый эквивалент, и что локально установлен .NET 9 SDK. Не засчитывать projected text как native результат до получения реального stdout, версии компилятора и SHAs. Если директория уже существует, использовать новый пустой временный каталог; не переписывать чужой Program.cs.

### Прямой native L0 без патча продукта

Компилируемый скелет для disposable runner, ссылающегося на 27.TranslationDomain.csproj:

    using System;
    using System.Text.Json;
    using TranslationDomain;

    static void Teach(ExecutorUnit unit, string source, string sourceLang, string target)
    {
        var tok = new TranslationSemanticTokenizer().Tokenize(source, sourceLang);
        var pair = new TranslationLearningExample(
            ExampleId: "n30-" + unit.UnitId + "-" + source,
            UnitId: unit.UnitId,
            FlowId: TranslationRoutingFlow.FlowId,
            InputJson: JsonSerializer.Serialize(new ExecutorInput(source, sourceLang, "general")),
            ChosenOutputJson: JsonSerializer.Serialize(new ExecutorOutput(target, 0.93, null, null)),
            RejectedOutputJson: JsonSerializer.Serialize(new ExecutorOutput("WRONG", 0.10, null, null)),
            RejectedType: "wrong_target_language",
            SemanticSignature: tok.Signature,
            SourceId: "micro-n30-permitted-teaching-source",
            HeldOut: false);
        unit.TrainOnExample(pair);
    }

    var shared = new TranslationKnnStore();
    var enEs = new ExecutorUnit(TranslationDirection.EN_ES, shared);
    var enHe = new ExecutorUnit(TranslationDirection.EN_HE, shared);
    Teach(enEs, "null property", "en", "propiedad nula");
    var probe = new TranslationSemanticTokenizer().Tokenize("null field", "en");
    var result = enHe.Execute(probe, "general");
    Console.WriteLine($"Signature={probe.Signature}; candidate={result.Text}; adapter={result.AdapterId}; confidence={result.Confidence}");

    var isolatedEnHe = new ExecutorUnit(TranslationDirection.EN_HE, new TranslationKnnStore());
    var control = isolatedEnHe.Execute(probe, "general");
    Console.WriteLine($"control={control.Text}; adapter={control.AdapterId}");

**Важная граница:** это L0 *нативный прогноз*, не исполненный здесь результат. При наличии NULL_LITERAL путь KNN разрешён и для EN_HE. Прежде чем засчитать воспроизведение, проверить одновременно: full/prefix signatures равны, no exact/phrase/word-map, SourceLaw != Contradiction, target «propiedad nula» был возвращён, adapterId содержит -knn. Негативный контроль должен дать fallback, а не ту же испанскую фразу.

### Переход от механики к реальному качеству L1

Для честного A/B нужна неизменная обучающая история и два consumer с одной семантической задачей:
1. Заморозить **две подтверждённые обучающие пары**: EN_ES «null property» → «propiedad nula» и EN_HE «null attribute» → «מאפיין null». Последний пример отражает не всякую возможную норму перевода, а **предварительный текст для bilingual gold review**. Не запускать semantic scoring без утверждённого правильного Hebrew variant.
2. Train order EN_ES → EN_HE. Query EN_HE «null property» (тот же English source был обучен в EN_ES, но **не** в EN_HE; это намеренный cross-direction collision). Полностью source-family-disjoint примеры — отдельный L2. — сначала вычислить signature из **настоящего tokenizer**, а не предполагать совпадение. Сверить candidate provenance: shared store может выбрать испанскую запись первой из-за FIFO, но это **дополнительный confound**; итоговый причинный contrast касается фильтрации по target direction.
3. **A shared:** текущий общий KNN, тот же train set, training order, same verifier.
4. **B direction-scoped:** временная shadow-версия, где AddPair сохраняет sidecar direction metadata и FindNearestCascade фильтрует строго по target direction *до* top-k, но не меняет внутри направления порядок/score/target guard. Для чистого L0 допустимо использовать два реально отдельных TranslationKnnStore, получающих те же записи соответствующего направления.
5. **C negative control:** EN_ES query после обучения EN_HE, чтобы фильтр не удалял собственные законные пары EN_ES. Другой negative: запрос с неправильным source-language, где не должно приниматься чужое совпадение. Проверить, что выигрыш не возник только за счёт abstain.
6. Оценивать не только «script is Hebrew/Spanish», но соответствие **source meaning, negation, role/order, technical identifiers**. Правильная цель — Hebrew answer, а не просто некириллический ответ.
7. Позже провести 8–12 source-family-disjoint heldout cases с минимум 4 negative transfer traps и 4 clean own-direction controls. Source families не повторяются в train; рубрика утверждена до запуска. Счёт вести раздельно по направлению, не усреднять исчезновение ошибок с потерей coverage.

**Если нет реальных EN_HE обучающих примеров:** ограничить вывод формулировкой «чужой урок стал достижимым и был ошибочно принят» (L0); нельзя обещать +N% accuracy при отсутствии правильного обучения EN_HE.

## 3. Consumer / restore / historical fairness

- **L1 ordinary Flow:** пройти через TranslationMachine.TrainStructuredBatchAsync → GetOrchestratorAsync → InlineTranslationOrchestrator → TrainedTranslationAdapter и сохранить raw input, direction, semantic signature, returned text, adapter ID, verifier decision. Приоритет exact/phrase/word может скрыть тестируемый эффект. Для isolated variant применять только disposable shadow assembly, не менять пользовательские core sources.
- **Не использовать TranslationMachine.EvaluateAsync как независимый multi-direction суд:** в [L686–706](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationMachine.cs#L686-L706) оценочный запрос фиксирует ru→en. Для EN_ES/EN_HE требуется явное направление через реальный routing request.
- **Cold restore:** SaveCheckpointAsync или ExportCheckpoint → новый TranslationMachine.ImportCheckpoint (НЕ metadata-only ExportAsync/ImportAsync). Сверить кандидатов до/после cold load, Unit IDs, direction namespace и source-state hashes. В существующей реализации checkpoint сохраняет KNN source/target/signature **без** direction; поэтому shadow-партиционирование потребует версионированного state/проверенной миграции, а не угадывания языка по target.
- **Historical parity:** сохранённые E26 / E14 / E09 / E43 проверяют другие функции/consumer. Не пересчитывать их старые PASS из нового consumer; при желании проверить отдельно один и тот же source+split. Матрица→KNN переносить только при известном feature schema, commit, caller и matched panel.

## 4. Фиксация результата — минимум

| Поле | Что вписать |
| --- | --- |
| core_commit / patch_sha / fixture_sha256 | exact snapshots |
| probe | N30A gate или N30B cross-language; отдельно |
| item_id / train_family / heldout_family / direction | source-disjoint |
| computed_signature / prefix / structure / semantic_concepts | результаты настоящего токенизатора |
| memory_candidate_provenance | source_id, train direction, stored target, rank |
| live path | exact / phrase / word / KNN / fallback; verifier PASS/FAIL |
| A / B / negative control | raw text, adapter ID, confidence, time |
| independent verdict | correct / wrong / abstain, source meaning + language |
| saved_loaded_parity | same result/different result + reason |
| decision | REJECT / CONDITIONAL / NEED_MORE_DATA; без заранее заданного PASS |

**Условие считать гипотезу подтверждённой для качества:** при единственной контролируемой переменной B статистически/практически улучшил blind source-disjoint correct/total **и** не создал неприемлемую ошибку при сопоставимой coverage; эффект наблюдался через обычный consumer и повторное открытие. На 8–12 строках это только pilot effect и proof of mechanism, а не доверительный прогноз большой сети. L0 отдельно может подтвердить баг даже когда L1 пока NOT_RUN.

## 5. Образец короткого executor prompt (исполнение не разрешено этим документом автоматически)

> Прочитай полную MICRO-N30 карточку,120§1,40§2,42 и44 живого core. Зафиксируй current core/planning SHA и сначала сверь сохранённый P117, его реальные signature/path/answer и общий расход23 Execute/9TrainOnExample. Не повторяй PRIMARY ради отчёта, parser или проверяющего. Различай N30-A RU_EN результат, ещё не проверенные направления и самостоятельную N30-B namespace-гипотезу. Для текущего документарного ремонта используй saved evidence; для отдельно назначенного native среза сохрани его fixture/gold SHA и прежний бюджет до запуска. Проверяй actual KNN export/import, все записи и кратности, а не ExpectedKnnWrites+Count; same-count порча target/signature должна отвергаться. Технический контроль должен пройти к заявленной ветви мимо exact-source, distractor — в ту же ячейку, reverse order — отделить source guard от первого кандидата. Оценивай полный смысл, язык и настоящий отказ; missing/false/unknown обязательный control меняет общий verdict/exit. Включай control-copy и все физические попытки в общий ledger. Не меняй production, frozen raw/gold и дозы, не вызывай Windows CI и не переходи автоматически к N30B. В отчёте раздели источник, выполненный host, ordinary consumer и cold restore; independent CODEX/Astra judgement заявляй только с настоящим provider/receipt.

## 6. Самопроверка плана (архитектор / plan review / code review)

- Указан actual learner/consumer? **Да:** ExecutorUnit, TranslationMachine ordinary Flow; не RP01 DPO Student.
- Отделены два алгоритмических изменения? **Да:** gate vs direction namespace, последовательные experiments.
- Контролируются мошеннический PASS и негативные кейсы? **Да:** blind gold, wrong, abstain, negative transfer, cold restore.
- Поддерживаются доменные исключения? **Да:** ordinary language без concepts vs technical concept; C#/AST не смешиваем.
- Нужен ли массовый Windows/generic gate? **Нет:** единичный маленький Native C# contrast.
- Изменён ли защищённый docs каталог core? **Нет.**
- Исполнены ли native learning/quality и независимый reviewer? **Нет, NOT_RUN**.

Смежные доказательства: [N30 расследование](30-NOVEL-DIRECTION-GATED-KNN-AND-SHARED-CROSS-LANGUAGE-CANDIDATES-2026-10-16.md); [старые 44 DIF](01-PRIORITIZED-DIFFERENCES.md); [core learning recipient and historical](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/00-README.md).
