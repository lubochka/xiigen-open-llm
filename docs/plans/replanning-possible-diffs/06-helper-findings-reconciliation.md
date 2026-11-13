# Сверка материалов других помощников с первичными источниками

**Актуальное дополнение:** ещё 31 путь `1f42ba7c89 → 30f2078122` внесён в [реестр 86](86-helper-30f20781-intake.md) и [резюме 80](80-research-resume-verified-and-pending.md). Для него и предыдущих вопросов строится каталог проверок [83](83-check-catalog-and-prompts.md)/[87](87-new-checks-38-39.md), с [картой источников 84](84-check-source-coverage.md). Чтение для постановки опыта не равно новой primary-source верификации, а чужой proxy не равен нашему запуску. Более старые статусы ниже исторические.

**Срез по запросу пользователя из-за ограничения времени:** [80 — проверенное и оставшееся](80-research-resume-verified-and-pending.md). Новые поиски сейчас не открываются; исследование целиком не объявлено завершённым. Последний пакет41 путей:4 тела прочитаны без завершённой первичной сверки,37 ожидают содержательного разбора. Worker77 завершился ошибкой доступа сети без итогового файла; прежние записи «77 активен» ниже исторические.

После предыдущего разбора поступили41 путь7591→1f42 из приложенияab667e0b. Их учёт и действительные пределы чтения — [79](79-helper-1f42ba7c-intake.md). Поручения помощников служат материалом исследования, не расширяют прямую авторизацию пользователя.

Приложение5df1bf7e обработано: повторное поступление1ebc4bac58 отделено от новых32 путей7591dc0b1b. Пофайловый итог — [67](67-helper-7591dc0b-intake.md), содержательная сверка — [74](74-helper-master-reconciliation-and-research-priorities.md). Новые30 файлов прочитаны полностью; два изменённых — по полному diff.66/70/72 получены от `/root/rp01_code_trace_astra` и прочитаны родителем;68/69/71/73/74 — работа родителя. Это не32 эксперимента и не независимая повторная приёмка всех sources. Следующее одиночное поручение75 продолжает поиск реального feedback consumer. Предыдущие итоги ниже — история.

**Последнее состояние сверки:** [48](48-helper-8a1f8f73-intake.md) закрывает пофайловое рассмотрение 32 входов до8a1f8f730a; [55](55-helper-2423dfb2-intake.md) — 29 уникальных входов четырёх следующих поступлений до1ebc4bac58. [61](61-n33-n34-crosswalk-and-open-evidence.md) — текущие существенные поправки. Пакеты53,59,63 получены от `/root/rp01_code_trace_astra` и прочитаны родителем;64 продолжает тот же единственный worker. Перепутанные метрики, caller/config ограничения, ошибочная экстраполяция proxy и 14 сохранённых ответов разобраны отдельно. Новых runtime действий нет. Ни сводная таблица, ни повторная ссылка на старый receipt не считаются независимым экспериментом.

Последнее поступление `30f9fe5b21→f4d79705de`: все34 файла рассмотрены в [42](42-helper-f4d79705-new-intake.md). Разбор45 завершил оставшиеся HNSW/GraphRAG/ProjectWiki вопросы:12-vector проект не различает L0 pruning при m8; semantic текст не потребляется найденным Wiki ответчиком; same-ID correction риск не переносится автоматически на sealed CLI с content-derived IDs. Эти уточнения сохраняют будущую проверку эффекта и положительные Mode1/retrieval consumers. Ни один новый runtime не запускался. Последующий поиск касается открытых primary-source leads, не повторного исполнения поручений помощников.

Дата: 16 октября 2026 года. Чужой каталог `C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/` только читался. Родитель сообщил об обновлении planning до `152b3fa19d`; сетевой fetch/pull этим исполнителем не выполнялся. Тексты помощников — указатели и постановки гипотез, а не независимые runtime доказательства. Здесь `source checked` всегда сопровождается **experimental pending** для причинного эффекта.

Прочитаны тела README, 00-MASTER, оба 12-matrix, 11-E51, 13-parity; позднее тело 11A-MICRO-KNN-ORDER и его fixture. 13-MICRO-KNN-CAUSAL-LEARNING прочитан частично: начало и разделы 3–7 доступны в выводе, середина была обрезана. Два больших ответа были усечены; ключевые 12-matrix/11-E51/00 после этого перечитаны ограниченными чтениями. Дополнение ниже относится к новому drop `470a01fbd2d8`, о котором сообщил родитель; исполнитель pull не делал.

Текущая фаза — READ_ONLY_RESEARCH. H-04 описывает первоначальный дизайн; позднее ошибочное исполнение Э-01 и его отмена отражены в [07, этап 9](07-study-log-2026-10-16.md). Новый изменённый fixture не связан с тем run. Остальные проверки здесь остаются проектами, без разрешения исполнения.

## H-01. E07 действительно не проверял все признаки, перечисленные в tier-описании

Источник гипотезы: [12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md>):7–19.

Первично прочитано: [E07StructuralContextToken.cs](../../../35.ExperimentsV2/Experiments/E07StructuralContextToken.cs):87–113,138–225,275–315. `Train` и `Predict` используют только `(BpeId, AstNodeType, AstDepth)`; tier не передаётся trainer. `posMap[i]` индексируется порядковым номером BPE, а карта строится по `charPos = (int)(s * step)`. То же обнаружено в `git show 88a880abf4:<file>` — snapshot 7 мая; точная build-привязка к прогону 1 мая отдельно не установлена.

Raw [E07 Tier2 1 мая](../../../35.ExperimentsV2/results/E07-STRUCTURAL-CONTEXT-TOKEN/results/run-2026-05-01-002.json):7–17 содержит .1502 → .131, −12,78%, `ast_parent added`. По прочитанному коду это не controlled эксперимент добавления parent. Размер фактической ошибки alignment не измерен. Положительный поздний E09 с AST extraction остаётся другим алгоритмом и выборкой; доказанного буквального port E07→E09 нет.

**Решение:** source checked; точное значение старого отрицательного результата ограничено; experimental pending. Добавлено Г-05. [Э-03/03b/03c](05-small-discriminating-experiments.md) меняют alignment, key и labels раздельно на одинаковых данных. Контрдоказательство чрезмерному выводу: даже правильный alignment не гарантирует превосходство разреженного N-gram или kNN.

## H-02. E51 сохраняет реальный полезный механизм, но не равен обучению свободного ответа

Источник: [11-E51-KNN-ORACLE-SPLIT-AND-POSITIVE-MECHANISMS](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/11-E51-KNN-ORACLE-SPLIT-AND-POSITIVE-MECHANISMS-2026-10-16.md>):7–55.

Первичные archived reports прочитаны через `git show f6032aff81f1f83de7c09cf9b9efe6b058dd12b9:experiments-v2/...`: E51A `run-2026-05-11-044429.md:5–25` сообщает overlap .205→1, domain/trace 100% на V01…V13; E51B `run-2026-05-04-072046.md:5–15` сообщает trace 100% на 4 TE и 2 CH. Это содержимое Markdown, не LFS pointer.

Current [E51A source](../../../35.ExperimentsV2/Experiments/E51ACodeContextPipeline.cs):254–275 выбирает domain-scoped Jaccard nearest и копирует `best.RequiredPredicates`; :854–893 действительно повторяет CodeContext из T у всех V. [E51B source](../../../35.ExperimentsV2/Experiments/E51BDynamicFlowMiniE2E.cs):210–240 возвращает `expected.WorkStatement/SchemePlan/AppliedResult/FinalResult`; :247 выбирает ветку по `record.Id == "CH-01"`. Diff этих двух source files между f6032aff и HEAD — только пять строк license header в каждом. Это связывает выбранный ref с кодом; build SHA самих исторических процессов отсутствует, особенно отчёт 4 мая нельзя автоматически привязать к последующему интеграционному commit 7 мая.

**Решение:** source checked, archived metrics read, experimental pending. Реальный retrieval/routing и знакомый контекст могли давать полезность; oracle fields не доказывают learned generation. Не принимается формулировка «E51 не учился вообще». Для C# predicate selection — [Э-06](05-small-discriminating-experiments.md); для Flow output/CH-ID — Э-07. См. Г-06.

## H-03. Общий root vocabulary — реальный invariant для cosine, но другой контракт, чем scalar DPO

Источник: тот же 11-E51:57–63. Первично прочитан [TrainingVectorBuilder.cs](../../../23.SemanticCompilerAdapter/TrainingVectorBuilder.cs):19–22,47–61,77–98: `Build(prompt)` создаёт vocabulary из одного prompt; overload принимает общий словарь. E51B :307–341 использует один training vocabulary для query и memory. Поэтому смешивать эти режимы в nearest search нельзя без проверки смысла координат.

**Решение:** source checked; experimental pending по [Э-05](05-small-discriminating-experiments.md). В нынешнем DPO coordinates не сравниваются cosine: `VectorSignal` использует число active slots. Это ограничивает причинный перенос ошибки root-space на RP01, но не снимает вопрос о потерянной смысловой функции. Менять vocabulary и loss одновременно нельзя.

## H-04. Готовый микронабор порядка принят; дизайн уточнён по actual core API

Источник: [11A-MICRO-KNN-ORDER](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/11A-MICRO-KNN-ORDER-CAUSAL-PILOT.md>):39–78 и JSON fixture. Код [KnnLogicalUnit.cs](../../../21.AgentInfrastructure/Logical/KnnLogicalUnit.cs):138–153 показывает, что изменение textSelector меняет corpus для BPE training. Тезис «один и тот же BPE при дописывании tags» без frozen-tokenizer seam не выполнен.

Наш [Э-01](05-small-discriminating-experiments.md) использует готовые 16/16 raw inputs, но переносит order-признак в существующие PredicateIds; query принимает их в metadata (:217). BPE/raw text теперь одинаковы. Predicate HashSet не усиливается повторами (:303–309); вес 2 уже задан (:287). HNSW RNG unseeded (:36 в HnswIndex) остаётся явно обозначенным контролем. Feature helper не выдаёт answer; label-flip проверяет зависимость ответа от памяти.

**Решение:** source checked; trial selected, **NOT RUN**. Это проверка архитектуры для порядка действий и качества обученной памяти. При положительном итоге ещё нельзя говорить о DPO, EN→RU или исправлении RP01 ordinary generation.

## H-05. Переводческие Units — отдельные изменяемые получатели

Источник: 00-MASTER:7–14,40–47. Прочитаны [TranslationTrainedUnits.cs](../../../27.TranslationDomain/TranslationTrainedUnits.cs):70–114,232–259,403–450: corrections, `_knnStore`, word/phrase/exact maps, запись chosen в `TrainOnExample`, signature/prefix/structure entries и детерминированное расширение gender forms. Это не только один nearest lookup; rejected в просмотренной части не используется как DPO objective. У обычного `ExecutorUnit.Execute` найден вход в :264; полный нынешний Execute и все verifier/restore owners не прочитаны в этом проходе.

**Решение:** source checked для хранения/признаков; fuller consumer/restore checked не заявляется; experimental pending Э-08/09. Сообщения помощников о `TranslationMachine.TrainBatchAsync +.01`, metadata-only import и cache mtime здесь пока **не перепроверены первично**. Их нельзя присоединить к RP01 full LoRA restore как найденный defect. Для EN→RU нужны собственные role/negation laws и state receipts.

## H-06. EOS/stop-token — адресный кандидат, ещё не первично проверенный здесь

Источник: 00-MASTER:18 и названные им `01.Tokenizer/BpeTokenizer.cs:37–68`, `09.Inference/TextGenerator.cs:212–249`. Тела этих диапазонов в данном расследовании не прочитаны. Статус **helper claim received; source check pending; experimental pending** Э-13. Не утверждать, что прежние RP01 0/3 или 0/4 вызваны EOS; helper сам различает `Generate` и `GenerateDetailed`.

## H-07. Формулировка «real optimizer run» не подтверждает изменение параметров

Источник: [13-CAUSAL-LEARNING-PARITY-PROTOCOL.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/13-CAUSAL-LEARNING-PARITY-PROTOCOL.md>):7–10 перечисляет V191/V194/V210 и calls StepMany/SaveCheckpoint. В [И-01](01-historical-experiments-and-comparability.md) на **v309** первично показаны одинаковые weights и отсутствие graph backward. Поэтому согласие авторов или вызов `optimizer.Step` не доказывают nonzero update.

**Решение:** конфликт терминов сужен; не считать все перечисленные версии no-op по одному v309. Их binary before/after и source snapshot не прочитаны здесь. Для проверки механического эффекта backward — Э-16; для исторического успеха — точный donor receipt. Ни положительный margin, ни наша source-улика не закрывают экспериментальный вопрос полезного neural learning.

## H-08. Старые роли автора данных не переопределяют текущую simulation

README помощников:15–22 и 13-parity:33–34 сохраняют старую схему внешней китайской модели. Прямое текущее поручение пользователя разрешило simulation; происхождение новых RP01 данных здесь называется только simulation. Этот конфликт authority не требует модельного опыта и не является гипотезой качества. Качество simulation против иной пары проверяется отдельно Э-12, без переименования provenance и без запуска внешнего Teacher в текущем этапе.

## H-09. История matrix→kNN остаётся открытой и после новых материалов

Оба helper 12-matrix прямо различают N-gram transitions, численное matrix multiplication и концептуальный Unit, а literal port помечают не найденным. Мы согласуем этот **статус поиска**, но не объявляем воспоминание пользователя опровергнутым. Г-01 содержит первичный P→code путь; H-01 добавляет конкретную ложную интерпретацию отрицательного E07. Следующие опытные карточки проверят полезность структурного признака/функции, но точная историческая родословная требует source ID/diff позднего успешного запуска.

Ни один существенный quality-cause в этой сверке не закрыт статическим чтением. Положительные, отрицательные и неопределённые исходы будущих опытов должны храниться отдельно по доменам и consumers. Общая таблица покрытия — [04](04-hypotheses-and-source-coverage.md); журнал исправлений — [07](07-study-log-2026-10-16.md).

## H-10. Morphology из sibling branch: неперенос подтверждён, результат качества ещё не найден

Полностью прочитаны helper `13-LANGUAGE-TRANSLATE-BRANCH-MORPHOLOGY-GAPS.md` и `12-SIBLING-BRANCH-MORPHOLOGY-DPO-CONTRACT-AND-E52-2026-10-16.md`; их сведения о E52/DPO не проверялись заново целиком. Для morphology получены первичные тела `MorphFamilyIndex`/`MorphDimensionAppender` и commit `986579ae86`, refs/contains/callers. [Н-01](09-domain-functions-and-sibling-branches.md) уточняет: добавленная координата — размер семейства, а не root identity; найденные callers — тесты. **Source checked; experimental pending.** Наличие прототипа полезно как адрес пропущенной функции, но не даёт права утверждать измеренный исторический прирост перевода. Будущий контраст — один corpus/candidate set, baseline embedding против добавленного scalar, отдельно по RU/EN/HE.

## H-11. Identity/predicate/domain риски принадлежат конкретному общему kNN-классу

Helper `14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md` прочитан полностью. По текущему `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs:146–153,216–230` подтверждён условный разрыв: одинаковые IDs после Trim перезаписывают output в словаре, хотя оба вектора добавляются в индекс; query predicates принимаются только при runtime-типе `string[]`. Вход другого типа превращается в пустой набор. `ExecuteAsync:221–244` не фильтрует найденные строки по домену запроса. Это source-свойства данного owner, а не уже найденные причины RP01.

В найденном старом E13B caller IDs уникальны, `PredicateIds=[]`, metadata пустая; данные только одного домена. Поэтому именно эти три риска не объясняют его положительный результат или исчезновение позднее. Для актуального capability-consumer другой owner: `18.CapabilityTraining/LlmIngredientLogicalUnits.cs:123–126` отклоняет повторные IDs; projector корпуса также отвергает дубли. Копировать verdict общего класса на него нельзя. Фактический product caller, передающий `List<string>`/JSON в общий unit, не найден адресным поиском.

**Source checked, условие возникновения не найдено в inspected RP01 route; experimental pending.** Проекты проверок остаются отдельными: два canonical-ID collision примера; одни predicates в разных типах metadata; один смешанный domain pool против разделённых индексов. Каждый меняет одну переменную. Для кода RP01 сначала нужен доказанный путь до этого owner. Ремонт общего contract при подтверждённом входе не заменяет диагностику LoRA.

## H-12. Restore: новый scope корпуса подтверждён, RNG не равен доказанному дрейфу ответа

Helper topology/rebuild привёл к первичному сравнению loader. [Н-03](09-domain-functions-and-sibling-branches.md) содержит старые строки `3036a01ace:312–341`, точный merge `e79ad635c4` и нынешние `1699–1715,1890–1907`: вместо истории домена используется bound corpus. Weight restore, tokenizer restore, memory membership и повторная постройка ANN — разные части состояния. Смена membership доказана; потеря полезной памяти и изменение финального ответа пока не доказаны. Текущий unit запрашивает всех кандидатов и tie-break по ID, что ограничивает простое объяснение через RNG. **Source checked; experimental pending** с отдельным membership-контрастом и отдельным same-corpus rebuild-контролем, без нового запуска.

## H-13. Новые карточки 15 — будущие проекты, не дополнительные старые результаты

У `15-MICRO-E51A-SOURCE-DISJOINT-LEARNING-GAIN.md` и `15-FAST-CAUSAL-PROBES-FOR-NEW-GAPS.md` прочитаны первые 32 строки. Первая явно помечена `DRAFT_SPEC / FIXTURE_CREATED / NOT_RUN / REVIEW_PENDING`; вторая — `READY_TO_IMPLEMENT / NOT_RUN`. Новая 15 JSON fixture здесь не разобрана, пригодность split не подтверждена. Изменённый 11A fixture не подменяет старую версию с сохранённым SHA ошибочного Э-01. **Proposal received, implementation/effect unverified.** Полезное направление source-disjoint E51 сохраняется; исполнять приложенные команды в текущей фазе нельзя.

## H-14. Translation DPO и Unit: новые конкретные source seams

Helper drop `a9f66a6ca3` включает 23 файла; это не 23 независимые найденные ошибки. Прямые первичные проверки и ссылки собраны в [11](11-new-helper-source-reconciliation.md). Bridge ставит исходный переводимый текст после служебного заголовка и обрезает prefix; Unit training может засчитать пропущенную строку; memory retrieval берёт ранние допустимые пары; общий evaluator задаёт RU→EN; `AccuracyAfter` может означать coverage. Сходные 16/17 helper-описания unknown-Unit и RowsTrained объединены, а ProductHost bypass сопоставлен с уже разобранным 09.

Коррекция потенциального преувеличения: `TranslationMemorySourceLaw.Compare` уже отбрасывает обнаруженные противоречия ДО cap. Риск вытеснения нового урока требует, чтобы ранние кандидаты прошли этот фильтр. Все эти участки относятся к своим владельцам; сам по себе нынешний RP01 ENRU500 через другой путь ими не объяснён. Готовые helper shadow/proxy результаты не объявлены нашими native наблюдениями.

## H-15. Последующее уточнение дизайна, а не новый эффект

Однострочный diff planning `a9f66a6ca3 → ba56fae4da` добавляет actual handoff source-elision в `16-MICRO-KNN-CORRECTION-AND-DPO-NOOP-PROTOCOL.md`. Уточнение принято: рост контекста меняет и prefix, и reply budget; он недостаточен для причинного вывода о пользе сохранённого исходника. Для будущего quality-контраста отдельно фиксируются контекст/бюджет и меняется размещение условия, с empty-source контролем. Новый запуск не выполнен и не разрешён этим helper-документом.

## H-16. Поступления a3bf8209 → 9ef3402 → f5188973

Файлы helpers не превращаются автоматически в новые независимые дефекты. Связанные fusion/BPE/admission claims сведены в15/16; E25/E26, E35/E36 и Unit identity — в17–19. Уточнение helper24: E36 имеет16шаблонов, не15. Короткий исторический положительный результат сохраняется, но ограничен составом своих данных; чужой Python split-proxy здесь не исполнялся.

В [21](21-transformer-knn-prefix-and-fallback.md) уточнён helper23prefix64: исходный код обрезает neural input, однако `EnsureNonZeroUnitVector` способен использовать полный текст. Безусловная идентичность конечных embeddings не следует из совпадения первых64ID. Кроме того, caller просит всех examples у HNSW; гипотеза малого shortlist нуждается в отдельном факте.

В [22](22-tatoeba-primary-review-and-domain-scope.md) повторно прочитаны первичные scorer/maps/cache. Смысловая слабость token-recall и отсутствие Hebrew branch подтверждены формулой. Support/margin фильтры словаря и обновление cache после save сохранены как контраргументы чрезмерному выводу. Добавлены условные вопросы phrase tie и scope language. Proxy scripts/result JSON помощников не считаются нашим запуском или measured RP01 quality. Внешний файлН12 в том же каталоге не приписывается родительскому исследователю.

## H-17. Уточнение новых языковых claims и дальнейшая историческая сверка

В [23](23-language-context-and-learned-rule-scope.md) исправлено слишком широкое утверждение о регистре языка: `BuildRuleTags` сравнивает без учёта регистра, тогда как `ClassifyPos/ResolveConcept` требуют точного `ru`. Поэтому «RU ломает все правила» неверно; проверять следует конкретные ветви. Аналогично контекст, хранимый в learned key, не означает его применение в consumer. Signature и SourceMarker имеют разные AND/OR правила.

Helper-указатель на историческое реальное neural learning привёл к первичным весам и raw records в [25](25-historical-neural-learning-donor.md). Они действительно содержат изменение head, но 44/50 → 49/50 связано также с пятью изменёнными rejected. На неизменных 45 triples accuracy осталась 44. На следующем одинаковом наборе 50 triples есть малое улучшение среднего margin без роста accuracy. Эти данные не отвергнуты и не повышены до доказательства свободного ответа. Второй предметный planner найден отдельным потребителем.

Новые .mjs/.py, executor prompts и JSON proxy помощников остаются внешними материалами. Их наличие не разрешает нам запускать алгоритмы. Прочитанные первичные findings и ещё не прочитанные предложения должны оставаться различимыми.

## H-18. Поступление 959e3eb11c: подтверждения и конкретные уточнения

- AdamW N26-B: подтверждён настоящий caller нынешнего RP01. Общий scalar и относительные per-pair веса нельзя смешивать; здесь Prepare задаёт Weight=1, поэтому zero-weight замечание не описывает эти batches. [30](30-teaching-vector-adamw-and-current-recipient.md).
- Stop-target и sampler: exact материал не добавляет отдельного EOS target; topK/topP ветви действительно альтернативны. Но greedy RP01 не использует спорную sampling-ветку, а detailed generation проверяет vocabulary/IDs. [31](31-answer-boundary-and-decoding-scope.md).
- E30: подтверждены отсутствие Current в адресе памяти и разные задачи v1/v2. Сохранённый V2 baseline имеет шесть hits, вопреки комментарию о нулевых векторах и полном miss. Комментарий не является наблюдением. [32](32-e30-lost-transition-context-and-historical-comparison.md).
- E27: подтверждена узкая нынешняя swap-функция вместо таблицы структуры предложения. **Уточнено:** это семь старых случаев, не семь разных структур; нынешний swap вызывается и в полностью покрытом wordmap, не только в fallback. [33](33-e27-structure-transfer-and-wordmap-counterexample.md).

E32 groups и E39 gates затем проверены по первичным источникам в [34](34-decomposition-quality-and-execution-verdicts.md): production decomposer имеет cap, а не всегда постоянное число результатов; quality checker действительно доступен через reflection bridge, но execution PASS не означает содержательную приёмку. Shape/dimension источник повторно прочитан в [37](37-new-helper-findings-data-history-and-language.md); несовпадение размерностей в реальном RP01 не найдено. E09 target/tie затем первично проверены в [38](38-e09-transfer-labels-and-opt-in-consumer.md): port существует, но обычный training/enabling/consumer не найден в tracked C#. RuleTags parity разобрана в верхнем разделе [23](23-language-context-and-learned-rule-scope.md): сохраняется само поле, расходятся два ключа поиска.

## H-19. Полная входная опись трёх последних pull

**Более новое поступление:** [42](42-helper-f4d79705-new-intake.md) содержит34 дополнительных файла до f4d79705de; прежняя опись71 остаётся самостоятельной. В [43](43-unicode-language-gates-and-proxy-fidelity.md) подтверждены Unicode/script/gate условия и исправлена численная интерпретация чужого Python proxy. В [44](44-structured-lessons-roles-keys-and-word-alignment.md) проверены parser concept/context, rejected annotation, tokenizer/index и first-word mapping; добавлены ограничения к будущим протоколам. Матрица4/16 кандидатов не является готовым набором: actual_counts=0.

Отдельно [41](41-current-dpo-gradient-chain-source-review.md) даёт source-контрдоказательства простой версии «DPO теряет градиент»: fresh graphs, additive leaf accumulation, frozen-weight propagation, исправленный RmsNorm и managed MatMul/Transpose. Это не численная аттестация всего trainer. Следующий ограниченный поиск45 исполняет тот же единственный Astra; runtime запрещён.

Дополнительный старый lead, не входящий в71 файл: первичный E52 теперь прочитан в [40](40-e52-contract-success-and-answer-independent-veto.md). Подтверждено, что0%→100% относится к четырём contract traces; три baseline=false литеральны, idle DidRun означает выдачу события. Выявлена конкретная граница duplicate-account arbiter: request сам может удовлетворять всем unsafe substring checks, отчего различение ответов исчезает. Runtime причинность и training consumer не доказаны.

Пофайловая сверка35 закончена для состава поступления: все71 файла имеют результат рассмотрения и адрес разбора. Получен самостоятельный36 от `/root/rp01_code_trace_astra`: нынешний replay selector отличается от legacy latest10; parent state отдельно; cache alias условен и требует живого процесса; readiness reset не очищает память. Для Batch029 созданный heldOutBatch не поступает в Unit score, хотя product-heldout отдельно работает. Verifier scorer использует ru и base tokenizer вместо ordinary trained tokenizer. Эти различия не объявлены причиной isolated RP01 без caller и парного наблюдения качества.

Родитель и ребёнок отдельно отметили ошибку применимости gold polarity: imperative→completed-result не является обычным переводом EN_RU. Существующий helper fixture не изменялся, не запускался и не принят автоматически как эталон. Это уточнение научного дизайна, не новый эксперимент.

Приложение пользователя охватывает `9ef3402ffd → 30f9fe5b21`: 71 уникальный файл (52 Markdown, 12 JSON, 6 mjs, 1 py). [35](35-latest-helper-intake-and-fresh-review.md) связывает каждое поступление с разбором и границей проверки. Внешние proxy receipts читаются как утверждения их авторов; не засчитываются как наши native experiments. Инвентаризация не подменяет содержательную сверку.

В [37](37-new-helper-findings-data-history-and-language.md) сохранены/уточнены N27: статистика минибатчей и префикса подтверждена чтением 500 строк; stratified schedule явно назван в комментарии, но не в поле manifest schedule. E48 содержит промежуточный полезный сигнал при final FAIL, а его57 val-предикатов дают56 оцениваемых переходов. E44/E45 смешивают target pairs и порядок/вес. E50 сравнивает две статистики одной траектории, E49 — иной механизм, чем нынешний поиск bridge-путей. RU→EN rule может сама создать неверный WordMapping; обычный caller частотного builder найден пока только в Tests. Контроль отрицания есть для узких copular clauses, поэтому общий вывод «весь kNN игнорирует not» неверен.
