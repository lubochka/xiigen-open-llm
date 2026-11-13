# BPE-REALIZED-MERGE-COUNT-01 — внутренняя действующая версия

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница BPE:** У BpeTokenizer.Train отдельно requested/achieved/merges/reason и настоящий читатель outcome; законный roundtrip сохраняется, shortfall не скрывается. Все вложенные и twin BPE fits входят в полный бюджет; больший request не доказывает больший достигнутый vocab.

**Исходный адрес из паспорта:** `01.Tokenizer/BpeTokenizer.cs`; роль этого файла и фактически достигнутый native/copy путь сверить по собственному runner карточки. Примеры120 не добавляют чужие fixtures/пороги к её набору.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/r/0001/report.md>).
Это ссылки на сохранённые результаты, не новый прогон и не общий вердикт успеха всей карточки. Нижний предварительный протокол сохранён; его старое NOT_RUN не отменяет найденный run. Более поздняя новая рецептура не считается выполненной лишь из-за исторического отчёта. Повтор исходного сценария этим обновлением не назначен.

**Продолжение:** [единый индекс и новая очередь Q](00-execution-index.md#next-contract). Наличие открытых исправлений не означает отсутствия новых опытов. Перед любым повтором сверить exact ID/попытку с отчётом; не запускать уже завершённый run и не перезаписывать его output. При STOP/NEXT Любы немедленно прекратить предыдущую работу; не продолжать её отчёты, retries или проверки вместо нового ID.
<!-- END result-navigation-2026-10-27 -->



<!-- BEGIN mandatory-learning-and-recovery-2026-10-21 -->
## Обязательные правила для карточки и любого её промпта

**Найти tool и дождаться своей операции — обязанность исполнителя.** До выдачи команды прочитать [контракт31](../31-tool-discovery-and-wait.md) и указанные там реальные skills/references; по каталогу текущей среды найти launch и wait/resume/status, прочитать их схему и сохранить provider, handle, start time, H/R и deadline. Не выдумывать Kiro API по Codex-примеру, не просить пользователя выбирать инструмент или запускать команду вместо исполнителя.

**Ожидание не равно бесполезному polling.** Одна выдача entry; затем штатный bounded wait/resume того же session/cell/process ID до terminal exit или исходного experiment deadline. Нормальный wait можно повторять при silent/running. Два быстрых неблокирующих чтения без прогресса требуют перейти в wait; если он неисправен/ответы противоречат — независимое чтение H/R/claim/exit и своего PID+start. Не вводить Start-Sleep в занятый терминал. Предел одного ожидания≤60с — не предел эксперимента; yield/RPC timeout не доказывает exit/kill/нулевой расход. После диагностики живой процесс продолжают ждать. Отсутствие log не доказывает, что запуска не было. Не выдавать entry заново «синхронно на25минут», не подбирать results-NN, не удалять claim/partial, не сокращать TRAIN/steps до двух минут ради tool. При UNKNOWN сохранять неизвестный расход и разбирать существующий запуск. STOP/NEXT Любы немедленны.

**Не завершать ход на ожидании и не придумывать работу.** ISSUED/WAITING → штатный блокирующий/event wait исходного handle → terminal/exit → чтение назначенных raw → отчёт. Финальное «жду уведомления / доложу потом» до результата запрещено; обещание auto-notification не доказывает возобновление. Не создавать фоновый waiter/monitor/watchdog, циклы polling, progress-события каждые N шагов, summary scripts, manifests или другую работу «пока ждём»; не запускать агентов или следующую карточку. Обычное ожидание не требует заполнения паузы работой. Соблюдать provider no-poll/no-sleep. Проверить настоящий wait/continuation в каталоге до launch; при отсутствии доказать конкретный блокер, сохранить handle и INCOMPLETE, без выдуманного API или relaunch. STOP/NEXT Любы немедленны. Полный порядок и исключения — [контракт31](../31-tool-discovery-and-wait.md).

**Без действительного обучения задача не выполнена.** Для каждой новой экспериментальной программы, включая diagnostic/reader/restore, обязателен назначенный реальный учебный участок: original файл/метод, learner→изменённое состояние→обычный consumer, точные TRAIN/EXAM, before/after и конечный бюджет. Сохранённый обученный S0 является стартом, но не заменяет этот участок. Для retention/restore навык проверяется до вмешательства; для acquisition допустим cold start с приобретением поведения. Ноль обновлений допустим у отдельного контрольного плеча или reader после общего Train, но не у всей новой программы. Unit/BPE действительно обучают память/правила; neural updates=0 у них не означает отсутствие native Train. Updates, loss, hash и случайный верный токен сами по себе не доказывают навык; 0→0 не является учебным результатом, частичные токены не объявляются целым ответом. Исторический безучебный receipt сохранить; его повтор получает отдельный назначенный рецепт по оригиналу и новый output, а не считается завершённым новым опытом. Уже выполненный учебный участок этой же попытки не повторять ради счётчика после сбоя отчёта; продолжить его сохранённые stages. Отдельная unit-проверка готового receipt не является новым экспериментом обучения. Полный охват карточек и адресные рецепты — [37](../37-full-catalogue-learning-coverage.md).

**0 → 0, отсутствие обучения или неполное A/B — не завершение эксперимента.** Раздельно записать execution_status, learning_validity и hypothesis_status; при невалидности учебная цель OPEN. Сохранить сырые свидетельства и продолжить разрешённое исправление постановки, а не только написать «невалидно» и отчитаться об успехе. Не искать другой checkpoint/модель/host, не менять seed/фактор, не увеличивать steps до PASS: точный исправленный рецепт назначает автор карточки до новой попытки. Старые опыты не удалять. Поздняя ошибка сохранения не обнуляет состоявшиеся updates; неизвестное число — null, невыполненное плечо — «не измерено».

Любое полезное измеренное улучшение в **валидном учебном сравнении** — успех своей метрики/домена; рост accuracy необязателен, регрессии рядом. Диагностика невалидной постановки не закрывает учебную цель. Default-модель исполнителя без override; точные H/R/entry ниже. Отчёт — русский R/report.md. Текущая правка документов не разрешает runtime. Прямые STOP/NEXT Любы немедленны и при открытой цели.

Полный конечный порядок диагностики и критерии: [контракт30](../30-failure-recovery-and-learning-validity.md). При конфликте со старым текстом ниже действует этот блок.
**Состав данных ограничивает вывод.** До запуска автор задаёт точные TRAIN/EXAM/CONTROL, покрытие классов, роли отрицательных примеров, семьи/пересечения и конкретный контроль постоянного ответа. Одноклассовый TRAIN и выигрыш на его единственной метке не доказывают различение маршрутов; updates/confidence не заменяют этот контроль. Разные опыты не устанавливают причину регрессии состава данных. Измерять любой исход, не предписывать ухудшение; набор и budget исполнитель не меняет сам. Полное обязательное правило — [состав данных и допустимый вывод](../32-training-set-and-inference-contract.md).

**Сохранённый учебный результат используется без повторного выбора.** В уже разрешённой фазе исполнения пригодные сохранённые веса/обученное состояние имеют приоритет: восстановить их и продолжить назначенный опыт, не спрашивая «A: использовать результат или B: обучить заново». Поздняя ошибка проверки, сериализации или отчёта сама по себе не основание повторять Train. Сначала прочитать актуальные receipts и установить, не выполнены ли recovery/плечи уже; состоявшуюся работу не переиздавать. Проверять только необходимую для следующего шага совместимость и учебную квалификацию, используя имеющиеся данные; недостающие показания получать лишь по явно ограниченному recovery-рецепту, без optimizer updates и без притворного восстановления отсутствующих moments/RNG. Если пригодность доказана, выбрать reuse автоматически. Если объективно отсутствует необходимое состояние, назвать конкретный технический пробел; не предлагать повторное обучение «для чистоты», не просить пользователя снять guard и не выдавать размер файла/падение loss за доказательство выученного навыка. Текущая документная фаза запусков не разрешает.

**Реализация не отменяет контракт.** «Код не читает RNG/optimizer/проверку» означает отклонение, а не разрешение его игнорировать. До следующего model call исправить реализацию в уже разрешённом scope либо применить только явно назначенный в карточке совместимый режим восстановления; не придумывать исключение, не просить снять guard и не переносить weights-only DPO-исключение на SFT continuation.

<!-- END mandatory-learning-and-recovery-2026-10-21 -->

**Единственное назначение результата BPE-REALIZED-MERGE-COUNT-01:** R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/r/0001; report.md и candidate.md в R (WIKI-FEATURES использует report.ru.md, AST — candidate-fix.md). Все старые output-only CLI/bridge/guard/DTO/checkpoint-output ссылки на собственный прежний results-root заменены тем же R; read-only исходный B0/tokenizer/source не переносить. Host-owned execution.claim/study/train/source/dependency/build provenance остаются в назначенном host, до R. R резервирует только один штатный execution/prepare owner. При старом claim/result/partial сначала сохранить его и записать EXISTS_OR_UNKNOWN; новый путь не обход ранее issued попытки. Модель по умолчанию, STOP/NEXT выше оформления.


**Маршрут по ID:** [BPE-REALIZED-MERGE-COUNT-01](BPE-REALIZED-MERGE-COUNT-01.md) → [GRAD-ACCUM-COPY-01](GRAD-ACCUM-COPY-01.md). Числовые позиции и текущие статусы — в [едином индексе](00-execution-index.md). Переход следует [правилу перехода](00-execution-index.md#next-contract); одиночный ID заканчивается END. Consumed означает чтение сохранённого статуса без повторного entry, а OPEN не становится completed от перехода. Исторические source labels не определяют очередь.


**Только документный дизайн, НЕ запуск.** В конфликте со snapshot planning9d311/53cb действуют назначенные здесь внутренние пути, индекс40, модель сессии по умолчанию без override. Source pins сохраняются, первоначально прочитанный HEAD669477 не подставляется вместо них (поздний read-only snapshot — 6e3e886). Старые метки номеров — только source labels. Report/candidate в R; полезный результат также кратко регистрируется в разрешённом required-changes.md. Внешний запрет править protected относился к другому поручению и не отменяет нынешние правки Markdown. Запрет product/runtime изменений сейчас сохраняется. Claim/legacy attempt нельзя обойти сменой root. Ни host, ни runner не материализованы этой сессией.

> **Согласование:** S04 — source label BPE-REALIZED-MERGE-COUNT-01, SKNN — alias AST без второго запуска. Действующий переход: [BPE-REALIZED-MERGE-COUNT-01](BPE-REALIZED-MERGE-COUNT-01.md) → [GRAD-ACCUM-COPY-01](GRAD-ACCUM-COPY-01.md). Дальнейшие переходы — [текущий индекс](00-execution-index.md#next-contract), без сокращённой копии очереди. Budgets/source/host сохранены; сессия документная.

# BPE-REALIZED-MERGE-COUNT-01: выбирать слияние по числу исполнимых замен

**Статус: DESIGNED_NOT_RUN.** Карточка описывает будущий малый опыт. Сейчас разрешено только документирование в назначенной папке; продукт, данные, веса и старые результаты не меняются. Новый runner ещё не создан, команд обучения не выполняли, измеренного успеха этой гипотезы нет.

**Действующий next_id — [GRAD-ACCUM-COPY-01](GRAD-ACCUM-COPY-01.md), позиция — [индекс](00-execution-index.md).** Одиночный ID — END; вход программы PARENT-ADOPTION-01, учебная цель№08 OPEN.

## 1. Вопрос, тема и получатель обучения

Тема — язык программирования и документация кода; малый материал — символы C# lambda-выражений и строк комментариев. Получатель изменения — настоящий `Tokenizer.BpeTokenizer`: в `Train` меняются vocabulary и таблица merges, затем `Encode` использует эту выученную таблицу. Два профиля EXAM — lambda и комментарии — не объявляются двумя новыми доменами. Нейтральный профиль нужен для контроля изменений на строке без выбранной пары.

В исходном алгоритме `CountPairs` учитывает все соседние позиции, включая перекрывающиеся одинаковые пары. `MergeChunk` заменяет найденную пару слева направо с шагом два. Для серии из L одинаковых token IDs число пар равно L−1, а исполнимых замен — floor(L/2). При конкуренции разных пар эти критерии могут выбрать разное первое слияние.

**Гипотеза:** выбор по числу реально исполнимых замен может эффективнее расходовать ограниченный бюджет merges для части учебных материалов. Возможна обратная сторона: ухудшение токенизации повторяющихся символов и иной предметной темы. Это альтернативная цель обучения BPE, а не установленная ошибка стандартного BPE. Ухудшение RP01, улучшение нейросетевой точности или KNN-ответов этой гипотезой пока не доказаны.

## 2. Код, запись состояния и настоящий потребитель

Все core-ссылки закреплены на `883496bca18113bf37e0afe683c75b05a0075048`, ветка `codex/dpo-implementation`.

| Звено | Первичный источник | Что следует из кода |
|---|---|---|
| Учебный выбор и запись | [BpeTokenizer.Train,42–68](<C:/Projects/llm_mvp_core/01.Tokenizer/BpeTokenizer.cs:42>) | CountPairs → MaxBy → запись `_merges` и `_vocab` → MergeAll. Фактор меняет решение непосредственно внутри обучения. |
| Исходная частота | [CountPairs,526–538](<C:/Projects/llm_mvp_core/01.Tokenizer/BpeTokenizer.cs:526>) | Каждая соседняя позиция увеличивает счётчик. |
| Исполнение слияния | [MergeChunk,548–565](<C:/Projects/llm_mvp_core/01.Tokenizer/BpeTokenizer.cs:548>) | После совпадения `i += 2`, поэтому два соседних экземпляра одной self-pair нельзя исполнить одновременно. |
| Использование выученного состояния | [Encode,73–82](<C:/Projects/llm_mvp_core/01.Tokenizer/BpeTokenizer.cs:73>); [ApplyMerges,568–574](<C:/Projects/llm_mvp_core/01.Tokenizer/BpeTokenizer.cs:568>) | Encode применяет записанные merges в выученном порядке. |
| Применение в обучении единицы | [KnnLogicalUnit.Train,136–153](<C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs:136>); [Vectorize,266–282](<C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs:266>) | Caller обучает BPE с vocab320; затем Encode участвует в векторах, добавляемых в HNSW. Это реальный downstream consumer, а не только счётчик отчёта. |

В малом опыте используется vocab257, чтобы изолировать **один выбор** и не смешивать его с накоплением последующих расхождений. Это диагностическая граница. Она не доказывает пользу при vocab320, не оценивает всю KNN-цепочку и не подменяет её результат компрессией. После отдельного решения Любы потенциальное место изменения продукта — только `01.Tokenizer/BpeTokenizer.cs::CountPairs`; способ изменения задан ниже. Автоматического переноса нет.

## 3. Дедуп: новый фактор внутри известного семейства

На planning `6bc4b6fba8f14870f5dc4966cf5a344754962d66` проверены поиском 747 Markdown документов possibleDifs; на core `883496b` — 172 доступных Markdown файла защищённого корпуса, включая 153 карточки. Поиск включал `CountPairs`, realized/realisable merges, фактическое сокращение, исполнимые слияния, overlap/non-overlap и семантически ближайшие BPE-карточки. Проверка не означает ручного прочтения каждой строки всех файлов.

Факт перекрывающегося счёта **уже известен**. [BPE-WEIGHTED-CHUNKS-01 и source-copy specification](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/neural-bpe/bpe-source-copy-design.md:25>) требуют его сохранять и называют non-overlap CountPairs другим алгоритмом. [Защищённая карточка BPE-WEIGHTED-CHUNKS-01](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/BPE-WEIGHTED-CHUNKS-01.md>) повторяет это ограничение. BPE-NO-MATCH-COPY меняет лишние копии и сохраняет выбор merges.

Новая часть здесь — отдельный причинный A/B **другой цели выбора**, с конкурирующими парами, сохранением остального алгоритма и заранее показанной ценой для разных профилей. Такой отдельный опыт в проверенном корпусе не найден. Не считать карточку повторным «открытием overlap bug» и не складывать её с weighted/no-match как независимые доказанные причины неудачного обучения. Неизвестные незагруженные ветки этим выводом не покрываются.

## 4. Ровно одно различие A/B

**A:** неизменённые `BpeTokenizer.cs` и `RegexSplitter.cs` указанного commit.

**B:** те же файлы; заменить только тело `CountPairs` приведённым алгоритмом. Self-pair означает пару с одинаковыми левым и правым token IDs. Для иных пар перекрывающиеся вхождения невозможны, поэтому исходный счёт сохраняется.

```csharp
private static Dictionary<(int, int), int> CountPairs(List<List<int>> chunks)
{
    var counts = new Dictionary<(int, int), int>();
    foreach (var chunk in chunks)
    {
        int selfEdges = 0;
        for (int i = 0; i < chunk.Count - 1; i++)
        {
            var pair = (chunk[i], chunk[i + 1]);
            if (pair.Item1 == pair.Item2)
            {
                selfEdges++;
                if ((selfEdges & 1) == 0) continue;
            }
            else
            {
                selfEdges = 0;
            }
            counts[pair] = counts.GetValueOrDefault(pair) + 1;
        }
    }
    return counts;
}
```

Первое появление каждого ключа и tie policy MaxBy сохраняются. Regex, UTF-8, порядок чанков, корпус, target vocabulary, `_merges`, MergeChunk, Save/Load, Encode и остановка Train не меняются. Не добавлять weighted chunks, no-match fast path, сортировку пар, нормализацию текста, EOS-токен, dropout, другую tokenizer-библиотеку, дополнительный KNN/DPO/SFT запуск. Host вызывает native методы; он не реализует второй tokenizer на Python/JS или через формулу.

## 5. Полностью фиксированные данные

Будущий `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/in/fixture.json` содержит ровно этот JSON, UTF-8 без BOM. TRAIN собирается соединением `train_lines` через LF (`\n`), без заключительного LF. Данные EXAM фиксированы сейчас и не подбираются после результата. Совпадение общих символов с TRAIN необходимо для проверки переноса merges; точного совпадения EXAM-строк с TRAIN-строками нет.

```json
{
  "id": "BPE-REALIZED-MERGE-COUNT-01",
  "domain": "code-documentation",
  "target_vocab": 257,
  "train_lines": ["///////", "x=>x", "y=>y", "z=>z", "w=>w"],
  "exam": [
    {"id": "E1", "profile": "lambda", "text": "a=>a"},
    {"id": "E2", "profile": "lambda", "text": "b=>b"},
    {"id": "E3", "profile": "lambda", "text": "c=>c"},
    {"id": "E4", "profile": "comment", "text": "/////////"},
    {"id": "E5", "profile": "neutral", "text": "q+r"}
  ]
}
```

Заранее выведенный из кода диагностический результат ниже — **не измерение**. Если native результат отличается, сохранить расхождение и завершить опыт; не менять fixture до совпадения с прогнозом.

| Объект | Before, byte IDs | Ожидаемый A после | Ожидаемый B после | Ожидаемая цена B относительно A |
|---|---:|---:|---:|---|
| Выбранная единственная merge | 0 merges | `//` (байты47,47) | `=>` (байты61,62) | Семантика намеренно различается |
| Весь TRAIN, token_count | 27 | 24 | 23 | На1 токен меньше |
| E1–E3, сумма lambda | 12 | 12 | 9 | На3 токена меньше |
| E4, comment | 9 | 5 | 9 | **На4 токена больше** |
| E5, neutral | 3 | 3 | 3 | Нет изменения |
| Все пять EXAM | 24 | 20 | 21 | **На1 токен больше** |

У A в TRAIN `//` имеет6 соседних вхождений, но только3 исполнимые замены; `=>` имеет4 и4. У B эти оценки3 и4. Это выбор ограниченного merge-budget, а не доказательство «B всегда лучше». В частности, заранее ожидается ухудшение общего EXAM-token_count при выигрыше lambda-профиля.

## 6. Реальное обучение, before/after и no-update контроль

Два независимых child-процесса, порядок A → B, без warmup и повторов. В каждом процессе:

1. Создать новый native BpeTokenizer и вызвать `Train("",256)` один раз для одинакового byte vocabulary. Сохранить canonical state и hash. Это initialization, отдельно от содержательного обучения.
2. Получить no-update контроль `C` через native сохранение и загрузку того же начального состояния: `using var stream = new MemoryStream(); tokenizer.Save(stream); stream.Position = 0; var control = BpeTokenizer.Load(stream);`. Явный rewind обязателен: native Save оставляет Position в конце, Load сам его не перематывает. У `C` не вызывать Train вообще.
3. Выполнить один before-readout пяти EXAM на обучаемом tokenizer и на `C`: actual IDs, token_count, Decode, round-trip equality. Дополнительно один Encode всего TRAIN на обучаемом tokenizer, только для baseline token_count.
4. Снять TRAIN counters, вызвать ровно один `Train(corpus,257)`, немедленно снять counters. Никакие Encode, Save, JSON parsing, report writing, hash/readout не входят в timed Train interval.
5. Сохранить состояние после, actual merge `(A,B,Result)`, vocab и hash. Повторить те же пять EXAM на обучаемом tokenizer и неизменённом `C`, плюс один Encode TRAIN на обучаемом tokenizer. Контрольные hashes и IDs должны оставаться прежними.
6. Записать raw JSON и завершить child. Controller только читает два уже полученных JSON и формирует сравнение; он не обучает, не вызывает Encode и не запускает детей повторно.

**Жёсткий бюджет:** 2 содержательных `Train(corpus,257)` +2 byte-vocabulary initialization, **всего4 вызова native Train API**; максимум2 learned merges суммарно; 40 пар EXAM Encode/Decode (2 процесса ×2 момента ×2 состояния ×5 строк), плюс4 TRAIN Encode; 0 NN forward/backward, 0 DPO/SFT/KNN fit, 0 LLM API calls. Один содержательный Train на плечо, без выбора лучшего seed/запуска. На один child — максимум30 секунд, на controller с двумя children —90 секунд. При timeout остановить собственный child, сохранить уже записанное, отметить неполные значения `UNKNOWN`, не повторять.

До сравнения подтвердить равенство corpus bytes/hash, fixture bytes/hash, A0/B0 canonical state, target vocabulary и исходных source hashes. A1/B1 **должны иметь разные merges при данном прогнозе**: различие не является ошибкой оптимизации, поскольку меняется учебный objective. Идентичные A1/B1 при выбранных данных означают `FACTOR_NOT_DISTINGUISHED`: гипотеза другого выбора merge не подтверждена. Это не отменяет отдельно наблюдаемых полезных ресурсных изменений; их и все регрессии сохранить с границей причинности. Изменение no-update контроля или разные входы дают `INVALID_COMPARISON`: raw сохраняется, но несопоставимую разницу не приписывать фактору. Успешный разбор JSON без native Train не является экспериментом.

## 7. Что измерить и как назвать успех

| Метрика | Как фиксируется | Граница интерпретации |
|---|---|---|
| Выученное состояние | A0/A1/B0/B1: canonical hash, vocab, actual merges | Доказывает наличие или отсутствие учебного перехода, само по себе не доказывает полезность. |
| Компрессия | Для каждого EXAM и TRAIN: фактические token counts до/после; суммы по profile и всей панели | Меньше tokens при точном Decode — полезно по этой оси. Не является точностью программы, перевода или KNN. |
| Сохранение текста | Actual IDs, Decode, exact ordinal equality исходной строки; no-update readouts | Потерю текста нельзя скрыть под выигрышем компрессии. |
| Выделенные байты Train | ΔGC.GetAllocatedBytesForCurrentThread вокруг синхронного Train | Это allocation, не живая RAM. |
| CPU и wall time Train | Process.Refresh + TotalProcessorTime до/после; Stopwatch | Одна маленькая A→B пара не отделяет общий шум/JIT/порядок процессов. Это наблюдение, не устойчивый speedup. |
| GC | ΔGC.CollectionCount для Gen0, Gen1 и Gen2 | Все три счётчика показываются, включая ухудшения. |

Для численных before/after показателей показать ΔA=A1−A0, ΔB=B1−B0 и ΔB−ΔA, с направлением «меньше/больше лучше». Для стоимости законченного Train показать абсолютные A/B и B−A; выдуманного «до-стоимости обучения» нет. Не сводить разные оси в произвольный суммарный score.

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и этому профилю/домену; рост точности не обязателен.** Рядом обязательно показываются остальные изменения, в том числе comment и общий EXAM regressions. Никаких требований «все показатели должны улучшиться». Например, measured lambda-compression gain сохраняется как кандидат даже при худшей суммарной компрессии; карточка не превращает это в общий выигрыш для всего домена. Если ни одна полезная ось не улучшилась — `NO_MEASURED_GAIN`, без подбора нового corpus или дополнительного запуска.

## 8. Фиксированная площадка, сборка и конфликты

Источник только для чтения: `C:/Projects/llm_mvp_core`, объект commit `883496bca18113bf37e0afe683c75b05a0075048`. Единственный будущий пакет опыта: **`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01`**. Host: **`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/Runner.csproj`**. Эти короткие пути заранее выбраны, чтобы не воспроизводить известный Windows path-limit failure. Ничего не создавать в старых attempt folders и не удалять их.

| Файл будущего пакета | Точное назначение |
|---|---|
| `A/BpeTokenizer.cs` | Неизменённый файл указанного commit; сохранить copyright/license header. |
| `B/BpeTokenizer.cs` | Та же копия с единственной заменой CountPairs из §4. |
| `common/RegexSplitter.cs` | Неизменённый source указанного commit. |
| `Program.cs` | Native Train/Encode/Save/Load, finite CLI, контроль времени, raw measurements; другого BPE нет. |
| `Runner.csproj` | .NET9 console, AssemblyName `BpeRealizedRunner`, ImplicitUsings/Nullable enabled; Disable default Compile items. Include Program.cs, common/RegexSplitter.cs и только `$(Variant)/BpeTokenizer.cs`. NuGet/ProjectReference на продукт не требуются. |
| `in/fixture.json` | Literal fixture §5. |
| `source-copy-manifest.json` | Repo/commit и Git blob IDs, SHA256 originals и A/B копий, точный diff, fixture hash, .NET version. |
| `r/0001/` | Append-only intent/status, A.json, B.json, stdout/stderr, initial/final canonical states, comparison.json, report.md, candidate.md при полезном выигрыше. |

Материализация после назначения этого опыта извлекает только два файла через `git show <fixed SHA>:<fixed path>` из существующего checkout. Содержимое писать как UTF-8 без BOM, без PowerShell default-encoding преобразования; например, читать stdout в .NET Process и записывать `new UTF8Encoding(false)`. Не использовать текущий HEAD вместо указанного объекта. Отсутствие объекта, .NET9 или исходного checkout — `PREPARATION_FAILED` с конкретной ошибкой, без смены ветки, скачивания SDK или поиска иной модели/проекта.

Экспериментальная оболочка может использовать `throw`, `try/catch`, `void`, простые DTO и собственный JSON-журнал. Не переносить на неё продуктовый запрет исключений или полный code-slice workflow. Исключение сохраняется с этапом и ненулевым exit code, не превращается в нулевую точность. Это послабление не разрешает менять данные, фактор или бюджет.

`ImportDirectoryBuildProps=false` и `ImportDirectoryBuildTargets=false` передаются обеими build-командами как global MSBuild properties, чтобы действовать до SDK imports; одно объявление внутри тела implicit-SDK csproj недостаточно. До SDK imports задать `BaseIntermediateOutputPath` и `MSBuildProjectExtensionsPath` как `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/obj/$(Variant)/`. OutputPath — `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/bin/$(Variant)/Release/net9.0/`, `AppendTargetFrameworkToOutputPath=false`, `AppendRuntimeIdentifierToOutputPath=false`. Только Variant A или B, Release, пустой RID. Это фиксирует соответствие команд путям и раздельные generated files.

`run-pair --peer ... --fixture ... --attempt 0001` эксклюзивно создаёт intent в новом `r/0001`, один раз запускает A и один раз B через `run-arm --arm A|B --fixture ... --output ...`; Arm A не включает B через runtime toggle. Существующий пакет `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01` допустим только как та же уже подготовленная source-copy с совпадающим manifest; иной пакет — `PREPARATION_CONFLICT`. Существующий `r/0001` **всегда** означает `ATTEMPT_EXISTS_NO_REPLAY`, даже при совпадающем manifest и отсутствии completed записи. Проверить это до обеих build-команд; controller повторяет тот же guard перед эксклюзивным созданием attempt. Записать состояние в ответе, не перезаписывать чужие файлы, не выбирать `0002`, не удалять каталоги. В пределах отдельного поручения материализации допускается одна сборка каждого варианта. Сбой этой сборки завершает попытку подготовки; не запускать full solution, не чинить продуктовые тесты и не создавать цикл ретраев.

Точные команды **будущего**, ещё не реализованного host:

Literal C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/Run-Assigned.ps1:

```powershell
$ErrorActionPreference = 'Stop'
if (Test-Path -LiteralPath C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/r/0001) { throw 'ATTEMPT_EXISTS_NO_REPLAY: no build; no training' }
$Claim = [System.IO.File]::Open('C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/execution.claim', [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
$Claim.Dispose()

$Root = 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01'
function Invoke-AssignedBuild([string]$Arm, [string[]]$BuildArgs) {
    $Build = Start-Process -FilePath 'dotnet' -ArgumentList $BuildArgs -WorkingDirectory $Root -WindowStyle Hidden -PassThru -RedirectStandardOutput "$Root/build-$Arm.stdout.log" -RedirectStandardError "$Root/build-$Arm.stderr.log"
    if (-not $Build.WaitForExit(60000)) {
        & taskkill.exe /PID $Build.Id /T /F | Out-Null
        throw "PREPARATION_FAILED: build $Arm timeout60s; preserve claim/logs; no training or retry"
    }
    $Build.WaitForExit()
    $Build.Refresh()
    if ($Build.ExitCode -ne 0) { throw "PREPARATION_FAILED: build $Arm exit $($Build.ExitCode); no training or retry" }
}
Invoke-AssignedBuild 'A' @('build', 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/Runner.csproj', '-c', 'Release', '-p:Variant=A', '-p:BaseIntermediateOutputPath=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/obj/A/', '-p:MSBuildProjectExtensionsPath=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/obj/A/', '-p:ImportDirectoryBuildProps=false', '-p:ImportDirectoryBuildTargets=false')
Invoke-AssignedBuild 'B' @('build', 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/Runner.csproj', '-c', 'Release', '-p:Variant=B', '-p:BaseIntermediateOutputPath=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/obj/B/', '-p:MSBuildProjectExtensionsPath=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/obj/B/', '-p:ImportDirectoryBuildProps=false', '-p:ImportDirectoryBuildTargets=false')

dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/bin/A/Release/net9.0/BpeRealizedRunner.dll run-pair --peer C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/bin/B/Release/net9.0/BpeRealizedRunner.dll --fixture C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/in/fixture.json --attempt 0001
if ($LASTEXITCODE -ne 0) { throw 'ATTEMPT_FAILED: preserve partial artifacts; no retry' }
```

Единственная внешняя команда:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/Run-Assigned.ps1
```

После единственной попытки разрешено прочесть raw для одного отчёта; это не повод повторять runtime. Literal Invoke-AssignedBuild ограничивает каждую сборку60 секундами, сохраняет stdout/stderr в host и при превышении завершает только дерево своего процесса через taskkill /PID; claim сохраняется, Train и повтор не начинаются. Это назначенный дизайн скрипта, его исполнение здесь не проверялось. Никаких commit/push, CI и product promotion. Полезный кандидат регистрируется в разрешённом required-changes.md; это документация, не перенос кода.

## 9. Исполнитель, отчёт и правило немедленного перехода

Исполнитель и последовательный проверяющий используют **модель текущей сессии по умолчанию**, без поиска другой модели и без самостоятельной смены режима. Обучаемая модель отдельно фиксирована: native BpeTokenizer с byte-vocabulary initialization, не LLM. Применить навыки из `C:/Projects/llm_mvp_core/docs/skills/`: `architect-behavior-classifier-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md` и `human-communication-GLOSSARY.md`. Перед новым аудитом или заданием заново и полностью прочитать стек договора120 и навыки применимой роли. Внутри того же задания ожидание и оформление отчёта сами по себе не требуют повторного полного чтения. Требования навыков к полной продуктовой цепочке не расширяют документационное поручение или ограниченный эксперимент.

Отчёт **на русском языке**, Markdown, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/r/0001/report.md`: до350 слов пояснений плюс таблица всех raw метрик/профилей и ссылки на JSON. Разделы: «Что меняли», «На чём проверяли», «Что получилось», «Выигрыши и ухудшения», «Граница вывода и кандидат исправления». Числа из §5 пометить как прогноз и не копировать в measured columns. Отсутствующее измерение — «не измерено», не0. При любом полезном measured improvement добавить рядом `candidate.md` до120 слов: ось/профиль выигрыша, все регрессии, исходный метод CountPairs, точное изменение, ограничение vocab257 и решение Любы о дальнейшем переносе. Сейчас candidate success не создаётся: результатов ещё нет.

**STOP/NEXT — обязательный запрет на продолжение прежней задачи.** При прямом «стоп/закончить немедленно» сразу прекратить действия: не делать новый tool call, запись памяти, checksum, исправление отчёта или commit. Единственное допустимое завершение — остановить уже работающий собственный процесс, если иначе он продолжит обучение. Затем кратко сообщить только известное состояние. При «следующий эксперимент» немедленно прекратить прежний опыт и перейти к уже назначенному следующему ID/единому реестру; запрещено дописывать прежний report, candidate, review, исправлять CI, проверять hashes или перезапускать его. Если следующего назначения нет — сообщить известный статус, не возобновлять прежний опыт. При обычном завершении допустим один readback фактических ошибок и ссылок, затем карточка закончена. Существующие данные не удалять. Повторно запрашивать уже предоставленные GitHub permissions не требуется.


## Внутренний контракт запуска

Полная карточка: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/BPE-REALIZED-MERGE-COUNT-01.md. Host=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01; R=Host/r/0001. Действующий next_id — [GRAD-ACCUM-COPY-01](GRAD-ACCUM-COPY-01.md); дальнейший порядок — [текущий индекс](00-execution-index.md#next-contract). S04 — source label, SKNN не отдельный запуск.

Только Run-Assigned.ps1 захватывает host execution.claim до build. R и intent резервирует только run-pair. Каждый child до byte initialization создаёт R/training-A.claim или training-B.claim через CreateNew. При неизвестном исходе сохранять claims и partial; не выбирать 0002 и не повторять child. Старый C:/xg/br01, если он существует, — только проверяемое read-only свидетельство прежней попытки; новый root не разрешает обход повтора.

Manifest фиксируется после B-factor и до build. Два global MSBuild пути заданы в literal командах wrapper отдельно для A/B; OutputPath=Host/bin/$(Variant)/Release/net9.0/, AppendTargetFrameworkToOutputPath=false, AppendRuntimeIdentifierToOutputPath=false. Compile только Program.cs, common/RegexSplitter.cs и $(Variant)/BpeTokenizer.cs; дополнительных packages нет. Точная DTO-секция raw: id, arm, source/fixture/corpus hashes; init/content Train issued/completed; initial/final canonical state, merges и vocab; все EXAM IDs/Decode/roundtrip/token_count; четыре TRAIN token_count; Train elapsed/CPU/allocated/GC0/1/2; stage/error/timeout. Контроллер сравнивает JSON без новых native вызовов.

Бюджет: 4 Train (2 initialization +2 content), 40 EXAM Encode/Decode и 4 TRAIN Encode. Контроль C не получает Train; Save→Position=0→Load обязателен. Возможные потери comment/общего EXAM показываются рядом с gain lambda; прогноз не измерение. Русский report и candidate в R; при фактической пользе одна запись до120 слов в required-changes.md без product transfer. Модель сессии по умолчанию. STOP/NEXT выше любого оформления.

По «остановить всё» немедленно прекратить всю работу; допустима только необходимая остановка собственного живого процесса. По «заканчивай и переходи» сразу NEXT без прошлого отчёта, hashes, review, retry или Git. Незавершённые файлы сохраняются.
