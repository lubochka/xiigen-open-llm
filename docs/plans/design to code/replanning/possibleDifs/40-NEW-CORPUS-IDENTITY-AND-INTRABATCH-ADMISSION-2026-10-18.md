# N40 — дополнительные гипотезы: идентичность обучающего корпуса при resume и новизна внутри batch

**Дата:** 2026-10-18. **SOURCE_CONFIRMATION + EXECUTED JS SOURCE-LOGIC SHADOW; нативное .NET обучение и измерение прироста качества НЕ ПРОВЕДЕНЫ.**
**Два независимых новых *кандидата* именно этого аудита.** До заявления об уникальности относительно *всех* ~367 planning и ~93 protected файлов нужна полная машинная дедупликация; сравнение проводилось адресно с planning README, N21, N27, N39 и protected catalog 83/78. Protected \`llm_mvp_core/docs/plans/replanning-possible-diffs/\` — **READ ONLY**.

## Человеческое резюме

Обучающаяся память может перестать соответствовать своему источнику **даже когда алгоритм не изменился между ветками**:

1. При восстановлении модель доверяет пути и идентификаторам строк. Если текст правильного ответа в исходном корпусе исправили «на месте» без изменения ID, checkpoint может сохранить *старое* правило, а отчёт и продолжение опираться на *новую* строку.
2. Проверка новизны примера сравнивает его с **предыдущей историей**, но не с уже отобранными примерами **этой же поступающей партии**. Партия может содержать повторённый или противоречивый сигнал. Обратная крайность — bag-of-words Jaccard может ошибочно отвергнуть важный новый закон, различающийся только отрицанием.

Оба механизма есть в **PrepareToGPL**: для восстановления его успехов это не объяснение ухудшения *между ветками*, а дополнительный контроль достоверности новых результатов. Достоверного факта, что RP01 neural DPO вообще использует эти конкретные пути, **нет**.

## N40-A — checkpoint перенёс старое обученное содержание на исправленный корпус

**Тема обучения:** перевод; **профили проверки:** EN→RU и RU→EN. **Получатель:** \`TranslationMachine\` и обучаемые translation executor/kNN; **механизм:** checkpoint state + resume training history; **ordinary consumer:** \`TranslationRoutingFlow\` / \`ExecutorUnit.Execute\` после загрузки.

### Прямое доказательство в исходном коде

- [TatoebaPilotLoopRunner.RunAsync L177–269](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L177-L269): \`Cache.TryGet\` либо \`LoadCheckpointAsync\` создаёт \`loadedCheckpoint\`, затем \`initialTrainingSkippedDueCheckpoint=true\` и \`ResolvePairRefs(rows, acceptedTrainingRefs,...)\`. При resume не проводится повторное обучение уже принятых строк.
- [ValidateLoopStateMatchesOptions L539–591](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L539-L591) сверяет нормализованный путь, размеры train/test, cursor, число acceptedRefs, но НЕ hash содержимого.
- [ResolvePairRefs / PairRefKey L615–683](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L615-L683) rebind по \`Direction|SourceSentenceId|TargetSentenceId|SourcePattern\`; \`TargetText\`/русская правильная сторона не включены в ключ. Если ID и source pattern прежние, строка с исправленным target считается той же ссылкой.
- [TranslationMachine.Save/Load L548–650](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L548-L650) действительно сохраняет/загружает state; вопрос не в том, что checkpoint совсем не восстанавливается.
- [TranslationCheckpointContracts.cs](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationCheckpointContracts.cs) содержит идентификаторы строк и путь, но в \`TatoebaLoop\` нет обязательного digest точного содержимого corpus.

**Наблюдаемый механизм по исходникам:** \`PairRefKey\` идентичен для \`EN_RU|101|222|she did not approve\` при \`TargetText=Она не одобрила\` и при \`TargetText=Она одобрила\`. При том же пути и числе строк проверки совпадают. После resume существует путь, при котором \`trainingRows\` построен по исправленному корпусу, а \`machine\` содержит прежнее сохранённое обучение. Это **подтверждённая логическая возможность**, а не измеренная потеря accuracy.

**Не путать** с ранее найденным [mutable cached checkpoint alias](27-NEW-MUTABLE-TRANSLATION-CHECKPOINT-CACHE-ALIAS-2026-10-16.md): там ошибка возможна из-за разделяемого изменяемого объекта кеша, здесь — из-за **смены содержимого source file с тем же идентификатором**, в том числе после реального restart и без cache.

### Быстрый различающий эксперимент A

A0: оригинальный файл, 2 изученных пары, save, полный новый process, ordinary Generate / Translate; A1: **тот же путь, те же IDs, та же длина и количество строк**, но заменён один target так, чтобы смысл и отрицание менялись; checkpoint старый, process cold-restart; A2: те же изменённые строки, но content digest/version проверяется и либо отклоняет resume с \`CORPUS_CHANGED\`, либо требует авторизованного пересоздания памяти из валидной истории. 4–8 heldout *других* формулировок этого закона, отдельно exact remembered source, плюс unchanged control. Все остальные параметры фиксировать. Метрики: source digest, checkpoint lineage, обычный ответ до/после restart, negative-operator error, coverage, latency and memory. **Фальсификатор:** actual parser включает target в дополнительные key/checks в реальном caller, или путь из changed corpus вообще не достигает \`ResolvePairRefs\`; тогда N40-A не активен на данном product route.

Нельзя молча пересоздавать модель по исправленной разметке и назвать это «restore». Любой corpus digest должен покрывать **семантически значимые поля и порядок обработки**, а не просто имя файла/размер, и быть привязан к checkpoint manifest. При mismatch запись нельзя считать подтверждённым продолжением прежнего эксперимента.

## N40-B — novelty admission не разделяет новые законы внутри текущей партии

**Тема:** перевод; **профили:** polarity/negation, coreference, gender/roles, sentence order. **Получатель:** составитель \`TranslationTrainReadyPair\` перед возможным обучением \`ExecutorUnit\`. **Механизм:** near-duplicate Jaccard admission; это НЕ нейронная LoRA.

### Прямое доказательство

- [TranslationStructuredBatchBuilder.cs L201–252](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L201-L252) — \`FilterByTrainingHistoryUniqueness(candidates, trainedASides, threshold=.7)\`. Если history пустая, немедленно возвращает \`candidates\`; иначе создаёт immutable \`historyFingerprints\` один раз и фильтрует через \`Where\`. Ни один принятый кандидат НЕ добавляется в comparison universe для следующего кандидата.
- [L233–252](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L233-L252): \`TokenSetForSimilarity\` — bag of lowercase letters/digits, без order/negation-role features. У «She did not approve the request today» и «She did approve the request today» Jaccard = **6/7 ≈ 0.857** и при threshold .7 новый отрицательно/положительно противоположный закон может быть отклонён как повтор.
- [ExecutorUnit.TrainOnExample L404–455](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L404-L455) заносит \`chosen\` в exact-source, три signature KNN, word/phrase; при двух conflicting chosen для одного ключа нет предметного судьи на уровне этой операции. Наличие этого пути **не доказывает**, что именно этот фильтр вызывается в production.
- \`PrepareToGPL\` builder SHA **40d35475c8769d5e694fd40380f2901bae22db42**, current SHA **7ceed82476edae9173acc44e2298885c561ca4d5**; прочитанный diff добавляет только ES направления, поэтому проблема — **унаследованная**, не новая из-за перехода ветки.

### Узкий исполнявшийся shadow-result

[Код source-only proxy](40-SOURCE-ONLY-NOVELTY-AND-RESUME-PROXY.mjs) → [сырой JSON receipt](40-SOURCE-ONLY-NOVELTY-AND-RESUME-RESULT.json) через Node v22, 18 октября; **JS моделирует выбранные предикаты, не выполняет C#**.

- Три кандидата \`p1, p2, p3\`, где p1=p2 (один source и chosen), p3 отличается отсутствием \`not\`.
- Пустая previous history → принято **p1,p2,p3**; несвязанная previous history → принято **p1,p2,p3**. Повтор внутри batch не исчезает.
- \`trainedASides\` содержит отрицательный p1 → **все три отклонены**, в том числе p3 с обратной полярностью.
- В точном сравнении токен-множеств p1↔p3 = 0.857; это честное объяснение *почему может исчезнуть* обучающий контраст, а не утверждение, что любой такой пример нужно принять.

### Быстрый различающий эксперимент B

Зафиксировать 12 train rows: 2 буквальных повторения, 2 противоречащих chosen для одного source, 4 полярных и 4 устойчивых законов. A0: нынешний admission; A1: intra-batch exact dedup + conflict quarantine (не решать first/last wins без независимого судьи); A2: A1 + law-aware/operator-aware near-dup admission, где \`not/no/never/не/нет\`, AST role/order и direction являются обязательными смысловыми признаками в соответствующих доменах. Не смешивать с одновременной сменой KNN feature extractor/LoRA optimizer. Зафиксировать разрешённый training budget / число реально применённых update и 12 независимых семейно непересекающихся heldout cases. Сравнить \`unique-law count\`, \`contradiction admitted\`, операторные ошибки, correct/12, memory, время на PC; отдельно сообщить coverage/abstention. Native .NET + blind exam **NOT_RUN**.

**Фальсификатор:** current real caller всегда проводит эквивалентную внутрипартийную фильтрацию до этого метода, или сам метод не вызывается при обучении целевого consumer. Тогда источник имеет уязвимую ветвь, но не фактический путь потери качества.

## Сопоставление с ранее найденными агентами

- [N21 operator-blind law admission](21-MICRO-OPERATOR-AWARE-ADMISSION-AND-UNIT-DIRECTION-EXPERIMENT.md) уже изучает семантику токенного множества; **N40-B дополнительно фиксирует отсутствие внутри-batch candidate-to-candidate comparison**.
- [N27 mutable cache](27-NEW-MUTABLE-TRANSLATION-CHECKPOINT-CACHE-ALIAS-2026-10-16.md) и [protected 78 restored-memory receipt](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/78-translation-restored-memory-positive-receipt.md) описывают другие доказательные границы; **N40-A — корпус на том же пути с неизменённым PairRefKey**.
- [N39 overall audit](39-RP01-CROSSWALK-NOVELTY-PRIORITIES-AND-MICRO-EXPERIMENTS-2026-10-16.md) уже считает neural warm start и learned consumer главным; N40 не подменяет P0.

## Статус и ограничения

| Claim | Evidence grade | Допустимый вывод |
|---|---|---|
| Идентичный \`PairRefKey\` для разных target | Source C + JS shadow | Resume **может** считать новую строку прежней, риск подмены learning history |
| Не включён corpus content hash в показанные preflight checks | Source C | **В этом пути** такой проверкой mismatch не закрывается |
| Внутри-batch самофильтрация отсутствует | Source C + JS shadow | При пустом/несвязанном history буквальные дубликаты допускаются |
| Bag-Jaccard перепутает противоположную полярность | Source C + JS shadow | Возможна потеря law-contrastive урока при наличии близкой старой истории |
| Вызов метода в actual RP01 neural Student | NOT_ESTABLISHED | **НЕЛЬЗЯ** утверждать, что объясняет free 0/3 или 0/4 |
| Качество после одного native исправления выросло | NOT_RUN | Нужен experiment A/B и независимый heldout |

**Не менять** frozen PrepareToGPL/RP01 evidence, исторические чужие документы/эксперименты, protected core folder. Любой будущий исправляющий patch — один механизм, один meaningful semantic check, re-evaluate / cold consumer, измеренные benefit **и** cost.