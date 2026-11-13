# Проверяемые вопросы и границы исследования

**Новый этап по прямому запросу пользователя:** план проверки каждого различия — [81](81-validation-program.md), предметные карточки — [83](83-check-catalog-and-prompts.md) и [87](87-new-checks-38-39.md), связь с источниками — [84](84-check-source-coverage.md). Поступление `30f2078122` содержит 31 путь и учтено в [86](86-helper-30f20781-intake.md). Это проектирование проверок; source claims нового поступления не объявлены заново независимо подтверждёнными. Прежний срез ниже сохранён как история.

**Срез по запросу пользователя из-за ограничения времени:** [80 — проверенное и оставшееся](80-research-resume-verified-and-pending.md). Новые поиски сейчас не открываются; исследование целиком не объявлено завершённым. Последний пакет41 путей:4 тела прочитаны без завершённой первичной сверки,37 ожидают содержательного разбора. Worker77 завершился ошибкой доступа сети без итогового файла; прежние записи «77 активен» ниже исторические.

Актуальный новый вход: [79](79-helper-1f42ba7c-intake.md), planning1f42ba7c89,41 путь. Пока не заполнен предметный итог строки, это только inventory. Прежние границы ниже не распространяются автоматически на этот пакет.

Поступление7591dc0b1b рассмотрено в [67](67-helper-7591dc0b-intake.md):30 новых тел и2 полных diff, без запуска scripts. [74](74-helper-master-reconciliation-and-research-priorities.md) сводит новые ограничения и первичные проверки66–73. Положительные E40V2 представлены всеми тремя классами; conditional singleton split не объясняет их результаты. Pooling detach не исключает DPO-изменение encoder через logits. GraphRAG и wordmap имеют отдельные consumers и не объявляются причиной слабых RP01 ответов. Единственный worker продолжает75: downstream feedback; parent ищет actual capability receipts. Это продолжающееся READ_ONLY_RESEARCH.

**Актуальная граница:** [48](48-helper-8a1f8f73-intake.md) — все 32 файла до8a1f8f730a имеют пофайловый итог рассмотрения. [55](55-helper-2423dfb2-intake.md) — следующие 29 уникальных путей до1ebc4bac58 также рассмотрены; [63](63-hnsw-random-state-and-continuation-consumers.md) уточнил последний HNSW lead. [61](61-n33-n34-crosswalk-and-open-evidence.md) связывает выводы с источниками и открытыми вопросами. Продолжается upstream admission в64 и поиск исторического donor. Core source216a1f3d; изменения019cb→216a только исследовательские MD. Это read-only coverage, не runtime-подтверждение причин. Последующие абзацы — история прежних границ.

Последний рассмотренный drop: planning `f4d79705de`,34 файла в [42](42-helper-f4d79705-new-intake.md), все имеют итог рассмотрения. Новые первичные разборы43–45 не подменяют будущую экспериментальную проверку. В41 проверены четыре участка gradient-chain; attention/RoPE/GQA остаётся следующим ограниченным поиском. Историческая реабилитация E24/A50 изучается отдельно; точный matrix→kNN donor пока не установлен. Core snapshot для41/45 — `79082340f01b744cdf08185616f2bd818dbd06b8`.

Актуализация последнего поступления: [35](35-latest-helper-intake-and-fresh-review.md), 71 уникальный файл до planning `30f9fe5b21`; core `d5c2409b24292d361593b8262b0c54997b15f539`. Между прежним core259c617 и этим HEAD продуктовых изменений в diff нет; внешняя публикация исследовательских Markdown не приписана родителю. Пакет28 получен и прочитан родителем; сравнение binary самим родителем повторно не выполнялось. E32/E39 проверены в34, N27 data/history/language — в37. Ни опись71, ни наличие внешних JSON receipts не означают71 первично доказанный дефект или71 эксперимент.

Дата: 16 октября 2026 года. Результат этой работы — сравнительное расследование в обычных Markdown-файлах. Код, данные, веса, правила, ветки и планы реализации не изменяются.

Дополнение текущего поступления:36 получен от `/root/rp01_code_trace_astra` и прочитан родителем;38 и верхний раздел23 содержат первичную сверку E09/RuleTags. Реестр35 теперь имеет71 содержательно заполненную строку без статуса «сопоставляется». Обработка scripts/JSON/proposals означает чтение и проверку границ вывода, не исполнение и не утверждение пригодности каждого gold. Следующий packet39 исследует сохранённый code-trace архив19 мая; открытые причинные вопросы остаются открытыми.

## Прямые уточнения Любы, принятые как направления поиска

1. Прежнее обучение действительно проверялось; успешные результаты могут лежать в соседних ветках. Поэтому поиск охватывает доступные local/remote refs, `git log --all` и чтение исторических файлов через `git show`, без checkout.
2. Часть экспериментов, названных неудачными, была неверно интерпретирована: механизм сначала поместили в матрицу, затем перенесли в функцию kNN и получили успешное обучение. Это приоритетный вопрос о конкретной истории алгоритма. Нельзя объявлять его установленной причиной RP01 до нахождения цепочки «старый код → перенос → положительные измерения → нынешний получатель».

Новые пользовательские гипотезы, которых не было в поручении, не придуманы.

## Что уже просмотрено

Инвентаризированы локальные ветки и remote-tracking refs без сетевого обновления. Среди доступных исторических направлений есть `language_translate`, `external_domains`, `claude/silly-rhodes-f0aa20`, `claude/infallible-kowalevski-a243c0`, `DpoImplementation`, `DesignToCode-clean`, `codex/chat-gpt-5.5`, `main`. Даты commit refs не равны дате последнего fetch; актуальность удалённого сервера этим не установлена.

Прочитаны точечные применимые разделы `AGENTS.md`, `CLAUDE.md` и восьми навыков, перечисленных в поручении. Это не полное чтение большого корпуса. Использованы границы честности утверждений, один делегированный исполнитель, различение исследовательского результата и реализации, признаки скрытой детерминированной власти, различие сохранения/обновления/качества и понятный русский текст. Диапазоны приведены ниже.

`context-mode` использован для начального индекса, но большой ответ был автоматически скрыт, а последующий поиск не завершился. Этот индекс не считается прочитанным доказательством. После прекращения ожидания применены ограниченные локальные чтения и `rg`/read-only git. Автоматические заявления инструмента об экономии токенов не используются.

## Ограничения

**Сейчас разрешено только READ_ONLY_RESEARCH и Markdown.** До ошибочного execution work order новых запусков не было; затем Э-01 был собран и выполнен вне разрешённой пользователем фазы. Это зафиксировано в 07; новые исполнения отменены, код и artifacts не удаляются и не дорабатываются. Исторический `PASS` без чтения алгоритма и получателя не принимается за доказательство качества нынешнего RP01. Исторический `FAIL` без поиска поздних исправлений не принимается за окончательный вывод об алгоритме.

## Последующие уточнения пользователя и граница приёмки

Пользователь потребовал **экспериментально проверить гипотезы**, а не завершить работу статическим согласованием выводов. Затем уточнил: проверять больше гипотез по разным доменам и подбирать архитектуру под домен. Поэтому `source checked` означает только установленное свойство указанного кода; **влияние на качество остаётся experimental pending** до парного опыта. Формулировки «опровергнуто» в 01–03 относятся лишь к узкой source-версии, например «commit отсутствует» или «current route использует legacy hash»; они не закрывают все behavioral варианты.

Дизайн будущих опытов — [05](05-small-discriminating-experiments.md). Прямое уточнение пользователя «вначале искать» отменило ошибочный переход оркестратора к Э-01. Карточки остаются проектами; их наличие и требование экспериментальной приёмки не дают разрешения на запуск. Новые материалы помощников рассмотрены в [06](06-helper-findings-reconciliation.md), журнал исследования — [07](07-study-log-2026-10-16.md).

## Покрытие осей запроса

| Ось | Первично прочитано | Установленное и контрдоказательство | Незакрыто / эксперимент |
|---|---|---|---|
| Историческое полезное обучение | v309 raw summary/4 binary hashes/runner/исторический trainer; E09/E13 raw summaries; E34 JSON; E51A/B archived MD | Старые positive reports реальны; v309 weights одинаковы на 25…88; часть kNN метрик имеет иной denominator | Полезный neural donor с matched free-answer baseline не найден. И-01/02, Э-02/03/06/16 |
| DPO качество | exact material, masks, simulation adapters, ENRU authoring category summary | Text integrity не равна смысловой независимости; current route не использует legacy compact hash | Все 1 000 строк и old v309 corpus не прочитаны; close-error rejected и family holdout требуют отдельной проверки. Э-12 |
| LoRA/base/параметры | recipient boundary, cold-start owner, current state receipts; старый allParameters caller | 16 LoRA реально изменились; base/reference остаются прежними. Нет основания считать нынешний update общим no-op | Rank/alpha/q-v coverage как причина не выделены. Э-10/11; random B0 — кандидат, не единственная причина |
| Objective/gradient | historical/current DpoTrainer, masks, sum/mean, VectorSignal | Новый backward присутствует; teaching vector scalar регулирует силу, а не semantic projection | Нормировка длины и способность полезно обучать конкретный домен. Э-04/16 |
| Токенизация/векторизация | old hash/Take96, current exact BPE, StructuralKnn, KnnLogicalUnit, TrainingVectorBuilder | Текст сохраняется лучше, но предметные координаты/отношения в разных consumers различны | Order/role/negation sensitivity; shared vocab; AST offsets. Э-01/03/05/13 |
| Скрытые детерминированные решения | fixed fusion, PatternClassifier/root calls, distance coefficients, E51B oracle/CH-ID, translation gender mapping | Их наличие показано в конкретных путях. Это не автоматически вред и не автоматически обучение | Изменять только одну функцию при fixed learned state; label/ID controls. Э-01/02/07/08 |
| kNN и функции | structural/method distance, capability ScoreCandidates, generic first-hit, E51 domain-scoped Jaccard | Несколько разных kNN, не только k. Исторический tuned alpha не равен нынешней fixed fusion | Learnable projection/metric/negative memory во всех ветках не обследованы; не объявлены отсутствующими. Э-01/02/05/15 |
| Restore | current RP01 optimizer/RNG/checkpoint path + raw reopened receipt | Общая версия «забыли restore» противоречит прямым сохранённым данным | Ordinary consumer identity/cold process и другие owner API ещё требуют своих replay. Э-09 |
| DPO → sub-units | DefaultLogicalCapabilityTrainer branch/preferences/replay→KNN corpus и factory call; simulation flat→exact batch | Есть два различных owner пути. Автоматического graph decomposition у simulation не видно | Полный learned Graph/Orchestrator runtime не прочитан; causal bridge не измерен. Э-06/07/08 |
| Доменные translation Units | corrections и ExecutorUnit TrainOnExample/store/maps | Реальный mutable memory recipient, выбранная сторона записывается; rule augmentation отдельно | Все Execute/verifier/restore paths и морфология соседних веток не прочитаны. Э-08/09 |
| Междоменное удержание | ENRU saved C# retention 0/3→0/3 | Одинаковые ответы не доказывают сохранение навыка при нулевой исходной правильности | Replay contrast допустим после ненулевого old-domain baseline. Э-14 |
| Git перенос/ветки | доступные refs, log --all, selected show/diff/contains | a98 входит в current; project21→18 move не равен matrix→function move | Remote-tracking freshness неизвестна; полного сравнения всех branch contents не было; exact remembered donor остаётся open |

## Домены не объединяются одной метрикой

| Домен/тип задачи | Закон и представление | Consumer / quality measurement |
|---|---|---|
| Порядок действий | Отношение backup-before-delete, одна и та же сущность | generic learned memory Unit; SAFE/UNSAFE и ложные SAFE; Э-01 |
| C# структура | AST type/parent/scope и реальные token offsets | N-gram/StructuralKnn next token/node; Э-03/15; это не объяснение программы |
| C# документация | Правильные утверждения о поведении конкретного кода, без выдуманных эффектов | прямой Student; semantic correctness, preference отдельно; Э-04/10/12 |
| EN→RU | Сохранение отрицания, agent/patient, референта, чисел | Student и отдельно ExecutorUnit; meaning/critical violation, не one-word overlap; Э-08/12 |
| Intent/episode | Правильный тип задачи и соответствующий учебный эпизод | E34 fusion/классификатор; hit@1/coverage, не generated answer; Э-02/05 |
| Flow/Orchestrator | Зависимости действий, законный arbiter и получатель | реальные generated fields vs copied oracle; subject routing отдельно; Э-06/07 |

Для нового домена нужна отдельная рубрика и совместимые признаки. Один и тот же harness переиспользует fingerprints и pairing, а не навязывает всем один extractor или архитектуру.

## Точное покрытие чтения правил и первичных документов

Canonical paths ниже относительны корню `C:/Projects/llm_mvp_core`. Это **targeted body read**, не заявление о полном load:

- `AGENTS.md`:1590–1656 (honesty),7693–7753 (OP65),8448–8505 (comparison); `CLAUDE.md`:1029–1057,2462–2490.
- `docs/skills/architect-behavior-classifier-SKILL.md`:758–799,920–935; `planning-session-startup-SKILL.md`:1838–1877; `plan-review-SKILL.md`:1580–1627.
- `generated-code-review-SKILL.md`:335–425,576–621; `prompt-patch-authoring-SKILL.md`:63–100; `conversation-intent-and-self-reflection-SKILL.md`:59–158.
- `output-readability-gate-SKILL.md`:169–218; `human-readable-architecture-audit-SKILL.md`:100–200. В одном исходном batch был короткий обрезанный край вывода; это ещё одна причина не заявлять complete load.
- `C:/Users/lubao/.codex/RTK.md` и `context-mode/SKILL.md` прочитаны целиком. Неудавшийся ctx_search не считается источником.

P-библиотека использована как индекс: targeted search в `09.09.26WorkLeft-INDEX-01-file-index-and-archive-plan.md`, затем **первичные** документы родительского `llm mvp/orchestrator implementation`: `PLAN-KNN-MATRIX-INTEGRATION-v1.md:1–75`, `v3.md:1–104,200–289`, `experiment_knn_state.json:1–145`, `IMPLEMENTATION-DOCUMENTATION-v7.md:560–605`, `PLAN-CAPABILITY-TRAINING-SEPARATION-v1.md:1–180`. `PLAN-E35-E38V2-E40V2-E34V2-INTEGRATION-v1.md` — только grep anchors, не body read. Старый .NET legacy plan в этот проход не прочитан и не принят за текущую инструкцию.

Source bodies и git anchors каждого существенного вывода указаны рядом в 01–03/06. Чтение JSON body, grep, inventory, git diff и helper summary различены. Для выбранных historical JSON проверено, что это реальные bytes, а не Git LFS pointer. Бинарные weights v309 читались для SHA/length; содержимое тензоров не декодировалось, модель не исполнялась. Полные raw artifact directories не просматривались.

## Открытый список для следующего этапа

1. Три дополнительные цепочки `language_translate`/`external_domains` теперь описаны в [09](09-domain-functions-and-sibling-branches.md): morphology, expression memory E13B и выбор корпуса при restore. Открыты конкретные evidence-пробелы: положительный run для morph appender; product/RP01 adoption expression unit; парное качество старого и нового restore-корпуса. Никаких запусков, сборок или edits кода.
2. Продолжать точечную родословную matrix→kNN; не считать ранний FAIL окончательным и не менять старые результаты.
3. Дополнять проекты проверок по доменам, ясно отделяя их от исполнения. Записывать найденные первичные источники, ограничения и изменения вывода в 07.

Это пакет сравнительного исследования, **не разрешение на исполнение и не завершённое причинное доказательство** неудачи RP01. Причинные гипотезы остаются открытыми, а непроверенные первичные источники не становятся фактами из-за ссылки помощника.

## Дополнение: адресное чтение sibling branches после уточнения фазы

Подробные evidence/контрдоказательства и неисполненные проверки — [09](09-domain-functions-and-sibling-branches.md), сверка helper claims — [H-10–H-13](06-helper-findings-reconciliation.md). Полностью прочитаны тела branch MorphFamilyIndex/Appender, два E13B raw Markdown, ранний E13B runner, поздние ExpressionCompletionCore/Unit/Evaluator. Прочитаны нужные диапазоны старого loader `276–347`, current loader `1621–1718,1890–1907`, corpus projector `321–408`, actual consumer `LlmIngredientLogicalUnits:110–140,166–244`. Не проводились новые сборки/тесты/прогоны.

Первичный `PLAN-E35-E38V2-E40V2-E34V2-INTEGRATION-v1.md` теперь прочитан в строках **1–220**, а не только найден grep, как в ранней записи выше. Его следующие разделы и первичные результаты E35/E38/E40 не раскрывались в этом ограниченном проходе. Это следующий точный адрес поиска, не установленный donor remembered matrix→kNN.

Дополнительно прочитаны сохранённые P3 reports `run-2026-05-04-210310.md` и `run-2026-05-11-194300.md`, части `P3LearningFromFailureProof.cs@language_translate:94–155,330–455`. В них есть фиксированный BaseService discriminator и положительная workflow-проверка; diff к HEAD — только license header. Эта линия не выбрана в качестве потерянного обучаемого алгоритма: файл сохранился, а перенос matrix→kNN не найден. Соответствующие первичные P3-SOURCE-BINDING/P3-EXPECTED-TRACE пока только найдены по ссылкам, не прочитаны. Исторические позитивные P3 записи не объявляются нейронным качеством или отсутствием обучения во всех остальных опытах.

Доступные `language_translate` и `origin/external_domains` закреплены полными SHA в 09. Локальная `external_domains` отличается от remote-tracking ref; датой commit нельзя установить свежесть fetch. Не просмотрены все файлы этих веток, все runtime receipts, все dynamic callers и весь внешний архив. Отсутствие найденного proof остаётся пробелом поиска, не отрицательным результатом опыта.

## Продолжение: перенос функций E35/E38V2/E40V2

В [10](10-historical-function-transfer-and-learning.md) сопоставлены raw JSON, code snapshots, ранний production commit и actual consumers. E40V2 имеет положительный stratified validation результат и зависимость от train-векторов, тогда как production scorer с первого commit `145f274f0c` использует фиксированную аппроксимацию. E38 содержит существующую функцию `ScoreForKnn`, но адресные non-test searches на integration commit и HEAD нашли только определение; найденный путь передаёт metadata выбранному flow. E35-rule реально участвует в порядке/параллельности веток при flag и специальных route predicates. Классификация этих эффектов и проекты раздельных проверок находятся рядом с Ф-01–Ф-03.

Дополнительно прочитан integration plan `:218–288`; ранняя запись о непрочитанных raw results E35/E38/E40 теперь частично закрыта указанными receipts. Прочитаны текущие тела `E35DependencyDetector`, `E38SkillRetriever`, `E40TrustScorerV2`, `TrustScorer`, `SkillRetriever`, `DependencyGraph`; исторические E38 диапазоны `5257df2e4e:15–65`, `4db575b1c6:1–110`, старый TrustScorer `145f274f0c`, relevant diff/blame и orchestration ranges указаны в 10. Первичные reports — реальные JSON, не pointers. Старые binary builds не восстановлены; заявленные в commit message числа tests не считаются нашими проверками. Вложенные исполнители и новые runtime действия не использовались.

Что остаётся открытым: точный remembered matrix→kNN donor; потребитель E38 score вне inspected tracked C#; deployment/injection конкретного TrustScorer; quality после переноса функций на реальные домены; остальные DPO/base/LoRA/unit оси. User helper drop `a9f66a6ca3` — новые указатели, не автоматически доказанные выводы. Ограниченный packet по 10 является промежуточным: дальнейший source-review и следующий bounded поиск продолжают исходное поручение.

## Продолжение: новые translation/Unit источники и актуальный Student consumer

В [11](11-new-helper-source-reconciliation.md) родитель непосредственно сверил `TranslationDpoTrainerBridge:13–15,118–152,180–216,230–251,314–343,401–428,520–570`, `TranslationDpoTrainingHandoff:244–282,429–430`, `TranslationMachine:89–145,662–705,714–745`, `TranslationTrainedUnits:123–140,333–361`, `TranslationUnitContracts:190–292`, `DpoBatch:11–27`. Это не только повтор helper claims. Подтверждены операции, способные обрезать условие, пропустить получателя, оставить новую коррекцию за пределами k, задать иной языковой маршрут и смешать coverage с accuracy. Причинное влияние на сохранённый RP01 не измерено; его отдельный exact-v2 маршрут не подменён bridge.

Дополнение `a9f66a6ca3 → ba56fae4da` прочитано как git diff одной строки протокола: настоящий handoff вместо искусственного batch, и различение диагностики с увеличением контекста от причинного сравнения при неизменном бюджете. Ни один приложенный runner/probe не запущен. Ещё не сверены первично все новые утверждения о tier balance, encoder/index version, zero-weight и историческом E13 split.

После bounded пакета 10 тот же единственный Astra-исследователь получил следующий READ_ONLY_RESEARCH участок: `CodeDocumentationSimulationDpoEvaluation → TextGenerator → full/cache forward`, состояние B0/LoRA, tokenizer/EOS и уже сохранённые first-token результаты. Это продолжение поиска причин текущего поведения, а не разрешение numerical parity test. Родитель отдельно проверил ключевые Ф-01/02 по старому TrustScorer, current SkillRetriever/Orchestrator и `git grep ScoreForKnn`; это source review, не новый эксперимент.

В [13](13-domain-context-and-dpo-subunits.md) дополнительно прочитаны structured builder `65–174,328–376,403–430`, translation executor `242–330,403–431,486–535,1008–1022`, verifier `1037–1167` и adapter `117–139`. Контекст в ключе learned rule хранится, но цикл применения не проверяет его; builder формирует tokenizer target своим собственным разбором; topic пропадает из exact-memory identity. Найдены исторические commits этих операций, включая явный patch phrase-verifier exception.

В `27aee6b44ff` прочитаны релевантные части training guide/STATE и оба 16-строчных JSONL. Выполнено только сравнение **сохранённых данных**, без вызова продукта: все 16 `(direction,input,expected)` точно присутствуют в training `(direction,a_side,chosen)`. Старый 16/16 остаётся свидетельством заявленного функционирования memory/Flow на знакомых строках; unseen-обобщение этим набором не проверено. Отдельные raw process receipts этого исторического запуска ещё не найдены. Это не полная ревизия всех успешных translation runs и не исследование всех 1000 RP01 пар.

## Продолжение: update-to-generation, содержание данных и helper 20/21

Получен пакет `/root/rp01_diff_investigation_astra` [12](12-student-update-to-generation-path.md). Родитель прочитал отчёт; независимое повторное чтение родителем всех перечисленных методов не заявляется. В пакете source counterevidence против простого adapter bypass, другого head/scaling, legacy EOS, нулевого Weight и пропуска первого response-token именно в RP01. Сохранённый first-decision доказывает изменение distribution при прежнем argmax, но содержит ноль decode calls. Numerical full/cache parity остаётся непроверенным. Суммарный DPO objective, средняя preference-margin и правильность greedy sequence разделены.

Родитель разобрал оба нынешних train500 как JSONL-данные и записал хэши/состав в [14](14-data-quality-and-evaluation-alignment.md). Это полный машинный подсчёт строк/строковых совпадений, не содержательное ревью всех 1000 пар и не вызов токенизатора. Тот же единственный Astra-исследователь получил следующий bounded READ_ONLY_RESEARCH участок: представительные примеры 25 C# и 20 ENRU категорий, сохранённые ответы и экзаменационные рубрики. Никаких новых генераций или native oracle.

Planning HEAD `a3bf8209f132` сверён локально. Из 23 новых/изменённых файлов прочитан индекс заголовков; адресно прочитаны основные helper findings по fusion/BPE/structured learning и direction/admission, а не все executor work orders. В [15](15-memory-growth-fusion-and-bpe.md) первично проверены `LlmIngredientLogicalUnits:169–294`, `TrainingVectorBuilder:104–134`, `KnnLogicalUnit:125–153,265–299`, `BpeTokenizer:42–68,526–538,568–573`, `RegexSplitter:16–21`. Смена ID ограничена контраргументом: чистая общая перестановка координат не меняет cosine.

В [16](16-translation-lesson-admission-and-meaning-loss.md) первично проверены builder `191–250,269–326`, runner `76–159`, executor `366–400,798–857`, tokenizer `98–115`, flat TrainBatch `51–86`, frequency builder `33–96`; выполнены targeted git grep/blame. Найденные inherited операции не объявлены октябрьской регрессией. Frequency builder caller найден только в тесте; flat Translation TrainBatch тоже не привязан к RP01. Helper proxy-результаты не переисполнялись и не считаются native evidence.

## Продолжение: положительные POS-опыты и новые поступления до f5188973

Пакет14 получен от того же единственного `/root/rp01_diff_investigation_astra`. Полностью разобраны JSONL для строковых свойств; смысловое чтение охватывает представителей25C#/20ENRU категорий и отдельно перечисленные строки/семейства, не все1000пар. Прочитаны сохранённые14генеративных текстов и их rubric/grades. Общий C# prefix `It writes ` присутствует на обеих сторонах500пар; вывод о сокращении общего prefix term требует совпадающих token IDs/conditioning, а не только текста. Нельзя из этого выводить нулевой общий gradient. Последующая ненулевая правка одного ENRU free text не стала полезным переводом.

Родитель прочитал первичные E25/E26 runners, старый learned classifier и production29 правила, raw E25/E26 receipts и git lineage — [17](17-positive-pos-knn-versus-production-rules.md). E25 .3438→.875 — POS-kNN на synthetic panel; нынешний fusion PatternClassifier29 не является тем же learner. E26 .4→1 использует предзаполненный контекстный словарь. Это сохраняет положительную полезность признаков в том домене, не доказывая current Student генерацию.

E35/E36 source и raw receipts повторно связаны в [18](18-feedback-routing-and-historical-split.md). Исправлена численность E36:16templates. E35 validation целиком parallel, поэтому baseline always-sequential даёт0; безопасность sequential задач не измерена этой панелью. Наличие связанных seed42 не считается доказательством конкретного состава split без его сохранённого списка или отдельного будущего восстановления.

В [19](19-unit-state-identity-and-partial-learning.md) прочитаны manifest, Unit training order, identity и restore. В SemanticTokenizerUnit запись pattern correction предшествует разбору Input; malformed Input способен оставить часть изменения. В ExecutorUnit оба JSON разбираются прежде записей, поэтому такой же одношаговый вывод туда не переносится. Native исключение не воспроизводилось.

Planning snapshots `9ef3402ffdbd` и затем `f5188973c9fa` сверены локальным rev-parse. Из новых документов выбраны тела prefix64/Tatoeba/state/atomicity/E35E36; некоторые другие пока прочитаны заголовками/указанными source-claims. Прочитан product HEAD `259c61769c3c66c9353f70d3dd569da19682067b`; git diff от полного0eccbdc3SHA исключаяdocs/.kiro пустой, exit0. Первая попытка с сокращением `0ecc` вернула ambiguous object ID; этот неуспешный вызов не считается сверкой.

В [21](21-transformer-knn-prefix-and-fallback.md) проверены EmbeddingModel31–83, Unit98–115/169–294, Hnsw99–110/190–222 и commit04330ca. Полный token-count fallback ограничивает helper-вывод об идентичности embeddings; topK=числопримеров ограничивает версию о малом shortlist.

В [22](22-tatoeba-primary-review-and-domain-scope.md) проверены Tatoeba scorer1439–1538, train747–768, maps1127–1328, save642–668, cache16–64. Дополнительно замечены отсутствие phrase runner-up rejection и direction в map keys; причинное качество не измерено. Положительные/отрицательные helper proxy — чужие сохранённые материалы; новых исполнений нет.

## Продолжение: старые положительные результаты и реальные получатели

Получены и полностью прочитаны отчёты того же единственного Astra-исследователя: [20](20-domain-routing-positive-mechanism.md) про E18, действующий Train и восстановление classifier; [25](25-historical-neural-learning-donor.md) про ProjectWiki. В 25 worker сравнил сохранённые binary tensors и точные heldout triples. Родитель прочитал пакет, но не заявляет самостоятельное повторение всего бинарного анализа. Важное различие: historical head-only update против нынешней q/v-LoRA. Положительные preference receipts сохранены с точным смыслом метрики; качество отдельного rule planner не присвоено нейронной модели.

Родитель первично проследил [23](23-language-context-and-learned-rule-scope.md), [24](24-e29-pair-value-and-probe-objective.md), [26](26-routing-profiles-and-generated-flow-boundary.md): язык и POS/контекст, E29 split/objective, routing profile и generated Flow. Уточнены контрпримеры: BuildRuleTags принимает регистр RU; CapabilityResultTranslator сохраняет knnOutput; текущий repository-owned endpoint требует пустые routing Cases/DpoPairs. Поэтому старые routing шаблоны не объявляются нынешними C#500.

В [27](27-e23-template-projection-and-production-difference.md) прочитаны E23 source, TemplateBeautifier, caller UniversalNineNodeDevelopmentFlow, git diff `1bee6a4276` и сохранённые JSON 30 апреля/1 и 7 мая. Главная метрика — выбор шаблона против случайного baseline, а не нейронная генерация. Проекция, состав случаев и derivation expected изменились одним commit; отдельный causal эффект проекции raw records не выделяют. Неудачный поиск по угаданному пути и усечённый общий `git ls-tree` не считаются полным source inventory; затем прочитаны точные найденные файлы.

Текущий bounded участок ребёнка — code-trace после backward-fix, будущий файл 28. Это продолжение поиска, не запуск и не итоговый аудит всех refs/доменов.

## Поступление planning 959e3eb11c и адресная сверка

Позднейшее обновление границы: [41](41-current-dpo-gradient-chain-source-review.md) покрывает четыре узких участка exact DPO→q/v по исходникам, без runtime; получен от Astra и прочитан родителем. [42](42-helper-f4d79705-new-intake.md) регистрирует34 новых файла, не смешивая их с завершённой предыдущей описью71. [43](43-unicode-language-gates-and-proxy-fidelity.md) и [44](44-structured-lessons-roles-keys-and-word-alignment.md) добавляют адресное первичное чтение TranslationTrainedUnits, StructuredBatchBuilder, ProductHeldOutRunner, DirectionAdapters, SemanticTokenizer и DpoDataset. В старых агрегированных crosswalk повторные ссылки не считаются новым независимым доказательством. Следующий child45: только memory/context consumers.

`rev-parse` подтвердил planning `959e3eb11c95` и прежний core `259c61769c3c`. Прочитаны имена/заголовки новых документов 26; полностью — helper AdamW, E30 condition-identity/V2 и E27; адресно — первые source-claims stop-target/sampler/shape/decoder. Это **не** полный содержательный разбор всех 39 затронутых файлов. Чужие proxy и work orders не исполнялись.

[29](29-e33-failed-status-versus-useful-rule.md): прочитаны E33 runner, compatibility generator, два позднейших StopSignalDetector и два raw receipts. [30](30-teaching-vector-adamw-and-current-recipient.md): traced simulation Prepare → logical materializer → TeachingVector → StepVector → AdamW; source подтверждает использование в текущем пути, но фактические activeSlots/gradients нынешних minibatches не измерялись. [31](31-answer-boundary-and-decoding-scope.md): exact material, tokenizer Decode, detailed generator, sampler и generation config; sampling-ветка не приписана greedy RP01.

[32](32-e30-lost-transition-context-and-historical-comparison.md): E30/V2/cache, raw JSON и адресный поиск constructor caller. Сохранённые шесть baseline hits противоречат source-комментарию V2 о полном отсутствии hits; объяснение этой исторической несогласованности не придумано. [33](33-e27-structure-transfer-and-wordmap-counterexample.md): старые таблица/случаи/raw result и нынешние два места вызова E27. Helper «только fallback» исправлен: полностью покрытый wordmap тоже вызывает перестановку.

История передачи: после прерывания прежний research-agent отсутствовал в live list; незавершённый28 сохранён. Ровно один `/root/rp01_code_trace_astra`, `gpt-6-astra/ultra`, продолжил тот же ограниченный след и затем вернул28 и36. Ранее отмеченное ожидание28 завершено; общая причинная программа не завершена. Parent выполнил адресную сверку источников помощников и запись отчётов; реализации и runtime в этом продолжении не было.
