# KH-COST-01 — одно дерево C# при подготовке учебной строки

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница COMPILER/LEARN:** Сверить назначенный уровень compiler: входные features, target packing и ordinary consumer не взаимозаменяемы. Отрицательный/положительный witness берётся из собственного корпуса этой карточки; copied distances/accuracy не обещать native пути без parity. Если вызывается Compile, учесть его реальные вложенные Grammar/Ngram Train. Сохранить назначенные lesson/RowId и before→factor→after по независимому gold; margin/hash/update не заменяют правильный ответ. Положительный контроль удерживает прежний правильный ответ, отрицательный показывает потерянную/неусвоенную строку; gained/lost TRAIN и EXAM раздельно.

**Исходный адрес из паспорта:** `12.SemanticCompiler/SemanticCompilerFacade.cs`; роль этого файла и фактически достигнутый native/copy путь сверить по собственному runner карточки. Примеры120 не добавляют чужие fixtures/пороги к её набору.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/KH-COST-01/report.md>).
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

**Единственное назначение результата KH-COST-01:** R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KHC/results; report.md и candidate.md в R (WIKI-FEATURES использует report.ru.md, AST — candidate-fix.md). Все старые output-only CLI/bridge/guard/DTO/checkpoint-output ссылки на собственный прежний results-root заменены тем же R; read-only исходный B0/tokenizer/source не переносить. Host-owned execution.claim/study/train/source/dependency/build provenance остаются в назначенном host, до R. R резервирует только один штатный execution/prepare owner. При старом claim/result/partial сначала сохранить его и записать EXISTS_OR_UNKNOWN; новый путь не обход ранее issued попытки. Модель по умолчанию, STOP/NEXT выше оформления.


**Маршрут по ID:** [KH-COST-01](KH-COST-01.md) → [KH-EXT-01](KH-EXT-01.md). Числовые позиции и текущие статусы — в [едином индексе](00-execution-index.md). Переход следует [правилу перехода](00-execution-index.md#next-contract); одиночный ID заканчивается END. Consumed означает чтение сохранённого статуса без повторного entry, а OPEN не становится completed от перехода. Исторические source labels не определяют очередь.


**Обязательное уточнение запуска и выводов (новый журнал 20.10).** Сначала полностью записать/закрыть script и входы, затем один последовательный запуск; не запускать одновременно с записью. До build/Train проверить существующий результат и раннюю защиту от повтора. Поздний FileMode.CreateNew после обучения защищает файл, а не бюджет. При timeout/неизвестном исходе наблюдать тот же process/session ID; новая копия, удаление partial/raw/claim или третий каталог запрещены. Существующий JSON и пустой log не доказывают успешный exit или единственный запуск. Равные первые loss не доказывают тождество states; 4 steps/full-sequence/frozen head остаются раздельными гипотезами. Это правило входит в каждый промпт ниже; текущая документная сессия runtime не запускает. [Подробности](../00-execution-policy-2026-10-20.md).

**Обязательно для этой карточки и каждого её промпта.** Модель исполнителя — модель сессии по умолчанию, без override. В назначенной изолированной копии допустимы throw/try-catch/void, простые DTO и минимальный host; переделка в продуктовый OperationResult, комментарии к каждой строке, product tests/CI/Git не требуются. Runtime scope и один назначенный фактор сохраняются. При обычном завершении: один русский report.md в указанном ниже result-root; при полезном измеренном выигрыше — одна запись до 120 слов в [required-changes.md](../../required-changes.md) с точным source fix, ухудшениями и ссылкой. Это обязательно без отдельного поручения; немедленный переход/stop выше оформления. Ручные checksums и новые запуски ради отчёта запрещены.

**Достоверность осей.** При одном процессе на плечо и фиксированном порядке время/CPU/process peak сохраняются как измеренные наблюдения с неустановленной причинностью; полезное измеренное изменение сохраняется как успех своей оси, причинный speedup не установлен. Для эквивалентной оптимизации сравнить уже предусмотренные конечные learned/optimizer state, loss и logits/readouts; различие нельзя скрыть ресурсным выигрышем. При изменении качества равенство конечных ответов не требуется. Числа 0→0 не доказывают сохранность навыка. Эта вставка не добавляет training/evaluation calls к указанному ниже бюджету.

**Учебный сигнал и предел вывода.** На назначенных запросах до/после настоящего FileActionKnnPredictor.Train показать native category и correct count. Уменьшение parse-cost — отдельная ось; совпавшие строки подготовки без правильных ответов не доказывают освоение темы. Нулевой positive query result оставить видимым.



Меняем повторный синтаксический разбор урока; учится настоящая таблица `FileActionKnnPredictor`; проверяем стоимость подготовки и `Train`, сохраняя её собственные советы. Это отдельный ресурсный опыт, а не повтор результата N-KU-01.

**Состояние документа:** рецепт записан; host и копии этим проходом не создавались, сборка и исполнение не проверялись. `NOT_RUN` — отсутствие результата, не запрет. Номер и следующий опыт задаёт [единый индекс](00-execution-index.md), не ID файла.

## Поручение, модель и разрешение конфликтов

Исходное поручение: «рассмотри последние обновления помощников и выстрой эксперименты, как можно более простые и маленькие для каждого домейна». Исполнитель и отдельный последовательный reviewer используют модель своей текущей сессии по умолчанию; фактическое имя записывают. Один worker, без вложенных. Самопроверку не называть независимым review.

Точное экспериментальное копирование разрешено: старый запрет `81-validation-program.md` здесь исторический. Оригинал — источник и кандидат будущего переноса, не edit target. Никаких product tests, full 35 build, CI, S00–S04, commit/push. Простая оболочка с `throw`, `try/catch`, `void`, DTO допустима; ошибку сохранить с ненулевым exit, не скрыть. Перенос в продукт решает Люба. Нет повторного личного допуска ID/SHA и порога 5%.

Параллельная документная сессия меняет только карточки. Исполнитель читает назначенную версию один раз; движение документного HEAD не вызывает повторное чтение, проверки или перезапуск. Ручные hashes паспорта/отчёта не нужны. Существующие raw, bin/obj и чужие файлы сохраняются.

**Остановка:** прямое «переходи к следующему» немедленно прекращает дополнительные действия над этим опытом, включая отчёт, review, hashes и правки; следующий берётся из индекса в разрешённой фазе. «Остановить всё» прекращает всю работу; для собственного живого процесса допустима необходимая остановка. Незавершённые данные оставить. Документная карточка сама не поручает runtime, а исполнитель не берёт работу документной сессии.

| Навык, точный путь внутри `C:/Projects/llm_mvp_core` | Применяемое требование |
|---|---|
| `docs/skills/architect-behavior-classifier-SKILL.md` | Сначала назвать получателя, изменение и границу вывода |
| `docs/skills/planning-session-startup-SKILL.md` | Сохранять назначенную фазу и пути |
| `docs/skills/plan-review-SKILL.md` | Один фактор; повтор знакомых признаков не объявлять обобщением |
| `docs/skills/generated-code-review-SKILL.md` | Проверить только копию и назначенные вызовы, не расширять работу |
| `docs/skills/output-readability-gate-SKILL.md` | Русский результат, факты отдельно от ожиданий |
| `docs/skills/human-communication-GLOSSARY.md` | Домен — программирование на C#, механизм — обучаемая kNN-таблица |

## Источник и один фактор

[Facade.TrainFileActionKnn:194–203](C:/Projects/llm_mvp_core/12.SemanticCompiler/SemanticCompilerFacade.cs:194) вызывает `FeatureVector(source)` и `Label(source)`, затем `Train`. В [FileActionKnn.cs:22–85](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/FileActionKnn.cs:22) оба метода строят AST. [Train:122–125](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/FileActionKnn.cs:122) заменяет память строками; [reader facade:327–333](C:/Projects/llm_mvp_core/12.SemanticCompiler/SemanticCompilerFacade.cs:327) проверяет минимум четыре строки и читает `Predict`.

A — две исходные parse-точки. B — одно дерево на учебный source, переданное двум root-overloads. В B сохранить string-overloads и все остальные тела. Не добавлять `DoStatement`, признаки, histogram, cache, иной `k` или голосование. Ошибка Do-меток остаётся вне этого фактора; новый набор ниже не использует do-while. Значения признаков, labels и ответы ожидаются одинаковыми; это проверяемое ожидание.

Цепочка: C# source → AST → признаки + учебная метка → `Train` → `_train` → портированный guard → настоящий `Predict`. Это настоящий учебный вход, но не полный `SemanticCompilerFacade.Compile` и не нейронное обучение.

## Точные уроки и четыре контрольных запроса

Авторская документная замена невыданного рецепта от 21.10.2026: прежние четыре строки с native labels3:1 и k3 давали постоянный ADD_LOOP; такой контроль не показывал различения входов. Новый TRAIN содержит шесть строк в двух структурных семействах, по три каждой метки; EXAM — четыре другие строки тех же двух семейств, по две каждой метки. Это family-recall для проверки стоимости настоящего обучения и чтения памяти, не перенос на независимые семейства. При проверке перед правкой каталог KHC отсутствовал; это не разрешение будущего запуска и не отмена повторной проверки claims перед ним. Исторического результата старой фикстуры здесь не заявлено.

Каждая строка — отдельный source, порядок K1→K6 фиксирован. Входы передаются как данные Roslyn, не компилируются и не исполняются как C#-программы. Имена/числа внутри семейства меняются; AST-структура одинакова. Member access, nullable и null-guards отсутствуют. TRAIN и EXAM не пересекаются точными строками; feature-эквивалентность внутри семейства намеренная.

```csharp
// K1
class L1 { static int Sum(int[] xs,int n){int s=0;for(int i=0;i<n;i++){s+=xs[i];}return s;} }
// K2
class L2 { static int Total(int[] items,int count){int total=1;for(int j=0;j<count;j++){total+=items[j];}return total;} }
// K3
class L3 { static int Accumulate(int[] values,int size){int result=2;for(int k=0;k<size;k++){result+=values[k];}return result;} }
// K4
class N1 { static int Sum(int[] xs,int n){int s=0;return s;} }
// K5
class N2 { static int Total(int[] items,int count){int total=1;return total;} }
// K6
class N3 { static int Accumulate(int[] values,int size){int result=2;return result;} }
```

```csharp
// KQ1
class QL1 { static int Collect(int[] input,int length){int accumulator=3;for(int p=0;p<length;p++){accumulator+=input[p];}return accumulator;} }
// KQ2
class QL2 { static int Combine(int[] data,int limit){int value=4;for(int q=0;q<limit;q++){value+=data[q];}return value;} }
// KQ3
class QN1 { static int Collect(int[] input,int length){int accumulator=3;return accumulator;} }
// KQ4
class QN2 { static int Combine(int[] data,int limit){int value=4;return value;} }
```

Рубрика: K1–K3 и KQ1–KQ2 имеют native `ADD_METHOD`, обозначающий здесь `NO_EXTRA_LOOP`; полезность добавления метода не заявляется. K4–K6 и KQ3–KQ4 имеют `ADD_LOOP`: задача — обработать массив, а цикла нет. `null` и другие метки не засчитываются. Gold заранее указан здесь, `Label` на экзамене не вызывать и не подавать expected в learner/reader.

Обоснование по [native source](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/FileActionKnn.cs:22): у всех строк нет SimpleMemberAccessExpression; у loop-семейства есть ForStatement и не более шести разных Statement kinds, поэтому Label доходит до ADD_METHOD; у no-loop семейства array parameter и нет циклов, поэтому ADD_LOOP. FeatureVector считает только частоты SyntaxKind: внутри каждого семейства все три TRAIN-вектора и два EXAM-вектора совпадают; между семействами различается как минимум координата ForStatement. При k3 три соседа своего семейства имеют нулевую cosine distance, другая семья — положительную. Ожидаются два разных правильных ответа и4/4 после обучения при0/4 guard-null до него; это статическое ожидание, не измеренный результат и не обещание PASS.

Постоянный ответ ADD_LOOP даёт2/4; постоянный ADD_METHOD также2/4, отказ0/4. Показывать эти аналитические контроли, правильность loop2/no-loop2 и ошибки каждой группы рядом с общим correct/4. Контроли считаются из rubric, не добавляют Predict. Если реальные ответы не различают обе группы или не превосходят constant2/4, полученное затратное наблюдение и rows сохранить, но приобретённое различение не подтверждено, learning objective=OPEN; не менять данные, k, seed или бюджет и не повторять entry. Положительное различение в обеих руках не обещает прироста accuracy именно от parse-фактора: его цель — эквивалентность подготовленных строк/ответов и стоимость.

## Назначенные копии, сборка и host

Root `K = C:/Projects/llm_mvp_core`. Только при поручении выполнить выбранный опыт создать:

| Назначение | Путь относительно K |
|---|---|
| Исходная копия A | `35.ExperimentsV2/Experiments/Rp01Micro/KHC/A/FileActionKnn.cs` |
| Копия B | `35.ExperimentsV2/Experiments/Rp01Micro/KHC/B/FileActionKnn.cs` |
| Host | `experiments-v2/RP01-MICRO/KHC/Runner.csproj`, `Program.cs` |
| Выход | `docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/KH-COST-01/` |

A копирует **весь** `12.SemanticCompiler/Prediction/FileActionKnn.cs`, меняя только namespace на `ExperimentsV2.Rp01Micro.KHC.A`. B — то же с namespace `.B` и следующими двумя overload-преобразованиями. Copyright сохраняется.

```csharp
public static int Label(string csharpSource)
    => Label(CSharpSyntaxTree.ParseText(csharpSource).GetRoot());
public static int Label(Microsoft.CodeAnalysis.SyntaxNode root)
{
    // Исходное тело Label ПОСЛЕ строки var root = ParseText(...).
}
public static float[] FeatureVector(string csharpSource)
    => FeatureVector(CSharpSyntaxTree.ParseText(csharpSource).GetRoot());
public static float[] FeatureVector(Microsoft.CodeAnalysis.SyntaxNode root)
{
    // Исходное тело FeatureVector ПОСЛЕ строки var root = ParseText(...).
}
```

Содержание `Runner.csproj` полностью определено:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType><TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <AssemblyName>KhCostRunner</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.12.0" />
    <Compile Include="Program.cs" />
    <Compile Include="../../../35.ExperimentsV2/Experiments/Rp01Micro/KHC/A/FileActionKnn.cs" />
    <Compile Include="../../../35.ExperimentsV2/Experiments/Rp01Micro/KHC/B/FileActionKnn.cs" />
  </ItemGroup>
</Project>
```

Без ProjectReference и OperationResult. `Program.cs` — синхронный exe с `System.Text.Json`, `System.Diagnostics`, Roslyn и aliases:

```csharp
using AGenerator = ExperimentsV2.Rp01Micro.KHC.A.ActionLabelGenerator;
using APredictor = ExperimentsV2.Rp01Micro.KHC.A.FileActionKnnPredictor;
using BGenerator = ExperimentsV2.Rp01Micro.KHC.B.ActionLabelGenerator;
using BPredictor = ExperimentsV2.Rp01Micro.KHC.B.FileActionKnnPredictor;
```

Функциональный контракт Program полностью фиксирован:

1. Аргументы ровно `--arm A|B <absolute-result-directory>`. Допустим только заданный result-root. Создать `A.started.json`/`B.started.json` через `FileMode.CreateNew`; имеющийся маркер/raw не перезаписывать. Ошибку сохранить и вернуть exit 1; partial run не повторять.
2. Массивы `train`/`exam` содержат точные строки выше без комментариев-ID. Создать один выбранный predictor `new(k:3)`, feature/predict/train delegates привязать к тому же namespace. `UseFileActionKnn=true`, `StackLabel=CSharp` фиксированы адаптером без facade.
3. `Query(source)` увеличивает logicalQueries; если `TrainingSize<4`, возвращает `null` и увеличивает guardNulls. Иначе ровно один `FeatureVector(source)` и `Predict`, перевод ID через неизменную `Labels`. Выполнить четыре Q0 и сохранить исходный size.
4. Начать фазовый Stopwatch, CPU `Process.TotalProcessorTime`, `GC.GetAllocatedBytesForCurrentThread`. Подготовить ровно шесть `(float[] Features,int Label)`, как `.Where(nonempty).Select(...).ToArray()` facade. A вызывает string-методы, B один `CSharpSyntaxTree.ParseText(source).GetRoot()` и два root-overloads. Исходный guard `Length>=4` сохраняется; при шести строках ровно один native `Train(rows)`. Закончить измеритель **до** сериализации, чтения EXAM и сохранения rows.
5. Выполнить четыре Q1. Сохранить один `arm-A.json`/`arm-B.json`: полные входы, подготовленные features/labels, sizes, все ответы, рубрика, actual counters, фазовые wall/CPU/allocated bytes, whole-process peak working set, timestamp/exit и фактическая модель исполнителя. Никакого повторного Label/FeatureVector TRAIN ради отчёта.
6. Сравнение A/B читает только сохранённые JSON; сравнивает каждый feature/label/ответ, не вызывает native код. Значение `12→6 parse-sites` при подготовке шести TRAIN строк на плечо подписать **ожидание по исходнику**, не инструментальный счётчик Roslyn; parse EXAM здесь не включён.

## Команды и неизменный бюджет

```powershell
$khHost='C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KHC'
$khOut='C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KHC/results'
dotnet build "$khHost/Runner.csproj" -c Release --nologo -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false
if($LASTEXITCODE -ne 0){throw 'Host build failed; no learning started'}
dotnet "$khHost/bin/Release/net9.0/KhCostRunner.dll" --arm A $khOut
if($LASTEXITCODE -ne 0){throw 'A failed; preserve partial data; no retry'}
dotnet "$khHost/bin/Release/net9.0/KhCostRunner.dll" --arm B $khOut
if($LASTEXITCODE -ne 0){throw 'B failed; preserve partial data; no retry'}
```

Итого два свежих процесса A→B; **2 Train, 12 предъявлений строк (6 общих уникальных), 16 logical queries = 8 guard-null + 8 Predict; optimizer/Generate/Compile/EXAM Label/warmup/repeats = 0**. В каждой руке шесть уроков, исходный порог facade остаётся4; k3 не меняется. Сборка только назначенного host; локальная починка его до runtime не меняет budget и не требует нового личного разрешения. Не добавлять B→A, 20+200 повторов, reviewer rerun, facade crosscheck.

## Метрики, ожидаемый результат и отчёт

Любое фактически измеренное полезное улучшение — успех по этой метрике и C#-профилю; рост accuracy не обязателен. Все ухудшения рядом. Один замер даёт локальную разницу: не приписывать её JIT/шуму как установленной причине и не обещать устойчивое ускорение.

Фазовые wall/CPU/allocated bytes — подготовка шести строк плюс Train. Allocated bytes не RAM; whole-process peak working set не training-only peak. Ответная точность, null coverage, false ADD_LOOP на KQ1–KQ2 и ADD_LOOP recall на KQ3–KQ4 — отдельные оси. Состояние 0→6 rows, 30 features/row, k3; нейронных слоёв/optimizer нет, одна таблица, полного Unit/Flow нет.

`report.md`: ≤350 слов объяснения плюс таблицы/raw-ссылки. «Что меняли» → «На чём проверяли» → «Что получилось» → «Выигрыши и ухудшения» → «Граница вывода». Заполнить:

| Метрика | A0 | A1 | B0 | B1 / B−A |
|---|---|---|---|---|
| Четыре ответа и выполнение рубрики /4 | не измерено | не измерено | не измерено | не измерено |
| Размер памяти / actual Train | не измерено | не измерено | не измерено | не измерено |
| Фаза Train: wall ms / CPU ms / allocated bytes | N/A | не измерено | N/A | не измерено |
| Process peak working set; цена остальных осей | — | не измерено | — | не измерено |

GOOD, только образец: «Меняли повторный parse в реальной подготовке Train. Выделения уменьшились; ответы совпали; CPU вырос. Это локальный успех allocations с указанной ценой». BAD: «Убрали parse по тексту, значит память точно ускорилась» или «accuracy не выросла, опыт неудачен». Нулевой измеренный эффект валиден. При различии подготовленных rows нельзя назвать результат эквивалентной оптимизацией.

Один readback отчёта: факты, ссылки, кодировка. Без Mermaid/render/ручных hashes/Git/новых запусков. Кандидат ≤120 слов со ссылкой на `ActionLabelGenerator` root-overloads и `TrainFileActionKnn`; продукт не менять. При редакционной правке: «уточнён отчёт, данные и опыт не менялись».

> **Промпт исполнителю.** Выполни только назначенный индексом KH-COST-01 по этому телу. Создай заданные KHC-копии и standalone host; A два parse, B общий AST, остальной механизм неизменен. Ровно один Train на шесть назначенных source и восемь logical queries до/после на плечо: всего2Train,12предъявлений,16logical=8guard-null+8Predict. TRAIN3+3 и EXAM2+2 — близкие структурные семьи, не независимое обобщение; k3/guard4 сохраняются. Покажи каждый ответ, обе группы и constant2/4; если различение не подтверждено, learning objective=OPEN, без добора или повторов. Ожидаемые12→6TRAIN parse-sites не выдавай за инструментальный счётчик. При валидной эквивалентности сохраняй любой полезный ресурсный эффект, без требования роста accuracy от parse-фактора; все ухудшения рядом. Модель текущей сессии по умолчанию. Сохрани ошибки/partial; production/CI не трогай. Отчёт короткий русский, ручные hashes не нужны. При прямой остановке/переходе прекрати прежние действия немедленно.

Источник: [полная helper-карточка](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-candidates-and-causal-gaps/knn-cost-card.md>). Её ручные SHA, fetch, protected-output запреты не перенесены. 20.10.2026 прочитаны полный helper и указанные current-source окна; это source review, не native run.
