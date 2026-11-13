# HNSW-QUERY-NORM-REUSE-01 — повторно использовать норму query в настоящем обучении KNN Unit

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница STATE:** Проверить полный typed learned payload и применимые RNG/tokenizer/index/maps, состояние сразу после вмешательства и отдельный fresh-load только по назначенному API. Положительный старый ответ сохраняется; внешняя мутация/чужая identity должна обнаруживаться. Count/hash сами не доказывают восстановление.

**Исходный адрес из паспорта:** `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs`; роль этого файла и фактически достигнутый native/copy путь сверить по собственному runner карточки. Примеры120 не добавляют чужие fixtures/пороги к её набору.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HNQ01/results/report.md>).
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

**21.10.2026: AUTHOR_SETUP_REQUIRED / NOT_MATERIALIZED / NOT_RUN.** Incoming8346 адаптирован только в документах. Source contract прочитан; host/source copies не создавались, build/Train/Execute не запускались. Default модель будущего исполнителя без override; Astra этого исследовательского поручения runtime не исполняет. Один worker, без дочернего AI. Student здесь — native `KnnLogicalUnit<string,string>`, не языковая модель исполнителя.

**Маршрут:** после [SILU-SIGMOID-REUSE-01](SILU-SIGMOID-REUSE-01.md); next_id — [IDENTITY-M1-01](N61-mode1-retention-and-identity.md#queue-identity). Остальные переходы и позиции — [текущий индекс](00-execution-index.md#next-contract). Активный вход [PARENT-ADOPTION-01](PARENT-ADOPTION-01.md) сохраняется. Внешние A5/№46 — исторические labels. Прямое поручение только этого ID имеет NEXT=END. Невалидный результат не закрывает учебную цель и не назначает автоматического перехода.

## 1. Фактор, обучение и границы

Native [KnnLogicalUnit.Train:103–192](C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs:103) обучает BPE320 по input corpus, создаёт HNSW m8/efConstruction64/efSearch32 и вызывает Add для каждого урока. [Add:49–97](C:/Projects/llm_mvp_core/06.VectorSearch/HnswIndex.cs:49) использует GreedySearch/SearchLayer/SelectNeighbours. Их пять cosine sites повторно вычисляют сумму квадратов query. B считает её один раз на **один вызов метода**, сохраняя dot/норму stored vector/FP32/epsilon/порядок кандидатов. Это единственный фактор.

Reader — настоящий [ExecuteAsync:202–256](C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs:202): input→обученный BPE→Vectorize→HNSW.Search→`bestExample.Output`. Domain — EN→RU короткие технические инструкции через generic Unit. Это не TranslationMachine/TranslationKnnStore, не StructuralKnn и не neural DPO. Изолированный cosine benchmark, готовый JSON и подстановка gold не заменяют Train/learned state/reader.

Во входном аудите заявлен novelty search planning929MD/23таблицы и core372MD при pins fc0dd/fb9175; это чужой поисковый охват, не новые наши reviews. Ближайшие P18 norm/shape, P21 aliases, P22 RNG, P20 reachability, P107 aggregation отличаются от локального reuse пяти cosine sites. Текущий адресный поиск этих имён в live cards не выявил другого такого опыта; глобальная уникальность не утверждается. KH-COST/AST/IDENTITY имеют другие learner/factors.

## 2. Назначенные paths, ownership и один entry

| Обозначение | Единственный абсолютный путь |
|---|---|
| K | C:/Projects/llm_mvp_core |
| H | C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HNQ01 |
| W | C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HNQ01/work |
| R | C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HNQ01/results |
| Внешний entry | C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HNQ01/Run-HNQ01.ps1 |
| Итоги | C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HNQ01/results/report.md и candidate.md |

Внешний C:/xgi root из incoming заменён внутренним W; состав10sourcefiles/3fixtures сохранён. W находится вне35.ExperimentsV2 default Compile glob. Только после прямого разрешения будущего исполнения автор создаёт H/Run-HNQ01.ps1 и H/harness/{prepare.py,run.py,report.py,Program.cs.txt,LabProbe.cs.txt,Hnq.csproj.txt,manifest.json}. Сейчас это документный контракт отсутствующей CLI; никаких файлов host не создаётся. Прямое поручение всего ID разрешает его конечную цепь без повторного вопроса по каждой фазе; поручение только именованной фазы не расширяется на остальные.

До первого copy/build/log/native constructor Run проверяет **любой** existing H/execution.claim или R directory/file, включая partial/study/arm/raw без host claim: отказ EXISTS_OR_UNKNOWN, старое не перезаписывается, новый root не выбирается. Одинаковые невыданные H/harness templates допустимы; чужой/несовпадающий template/source → PATH_CONFLICT. Run атомарно FileMode.CreateNew резервирует H/execution.claim с owner nonce/PID/start/H/R/run01; FileAccess.ReadWrite, FileShare.Read, чтобы children могли читать claim, но не менять его. Только этот owner создаёт R и R/study.claim до Prepare. Child получает HNQ_OWNER_NONCE и сверяет живого owner по PID/start и claims; direct Prepare/build/arm не альтернативный entry. Перед первым native Train каждой руки Program атомарно создаёт R/A|B/issued.marker; любой existing arm file/marker → refusal. Поздний raw CreateNew не заменяет эти guards.

Разрешённый future host — минимальная экспериментальная оболочка: обычные DTO/throw/try-catch/void, reflection только для чтения `_index`. Product/tests/CI/solution/AGENTS/skills не меняются. Incoming protected-scope относился к чужому аудитору; он не запрещает текущие разрешённые Markdown/required-changes. Native learning guards сохраняются; перенос в product решает Люба.

## 3. Student, материал и настоящее before→after

`unitId="hnq01-enru"`, `unitRole="translation-executor"`, `domainTag="TranslationDomain"`, dim64,k3,confidenceFloor.70, `textSelector:s=>s`, PredicateIds пусты. Default LogicalExecutionFlags, пустой Metadata. В обеих копиях HNSW только общая seeded adaptation: `private static readonly Random _rng = new(7331);`. Каждый arm — свежий процесс; внутри него RNG расходуется Train4→Train8 последовательно, без reset между ними.

| State | Реальный learner | Readout |
|---|---|---|
| A0/B0 | Native Train первых tr01..tr04 | Все24Execute, manifest, index snapshot |
| A/B | Новый native Train всех tr01..tr08 | Те же24Execute и snapshot |

Это rebuild на расширенной памяти4→8, не continuation из checkpoint. Static Train заново обучает BPE и создаёт HNSW. Несуществующие LoadUnit/TrainOnExample не вводить. Warm Train включён в бюджет и естественно прогревает частьJIT; дополнительного warmup нет.

Source pin всех источников/fixtures — `fb9175cc752d41e1cb525e0609b7c96140940217`. Полный fixture prefix **C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/**, только чтение:

| Файл | Git blob SHA | Проекция |
|---|---|---|
| training-batch-B.json | 0c2a3d9a0da31b7277c0d215fb79ddebda49aa6c | 8examples в сохранённом порядке; case-sensitive `inputJson`/`chosenOutputJson` — строки вложенногоJSON; JsonDocument.Parse каждой даёт SourceText/Text |
| eval-inputs.json | 585bf30cad84b57b2e1f3fe8c00f171f2348c313 | Все24cases с id/source/bucket |
| panel-gold.json | af73d89cfd5b42f954380437bc5596b46dae9498 | accepted только scorer после сохраненияraw; никогда Train/query metadata |

| ID | Input | Output |
|---|---|---|
| tr01 | validate the file | проверь файл |
| tr02 | delete the cache | удали кэш |
| tr03 | copy the report | скопируй отчёт |
| tr04 | move the config | перемести конфигурацию |
| tr05 | upload the image | загрузи изображение |
| tr06 | download the package | скачай пакет |
| tr07 | save the draft | сохрани черновик |
| tr08 | archive the log | архивируй журнал |

Rows=`new LogicalKnnTrainingExample<string,string>(id,source,target,[],["translation"],["P125-enru-memory-01:"+id])`. Проверить8unique IDs, sourceId=simulation, heldOut=false, буквальные пары таблицы; JSON Unicode escapes декодируются стандартным вложенным parse, не передаются как буквальные `\uNNNN`. Rejected не используется: это supervised Unit, не DPO objective. Exact x01..x04 — старые, x05..x08 — новыеTRAIN readbacks; near3/composition8/edge5 отдельно, всего24. Exact не held-out, остальные16 не доказательство общего перевода. Исторические19/24P125 другимscorer/learner не сравнивать с этим результатом.

## 4. Точный source-copy closure

Prepare получает bytes `git -C C:/Projects/llm_mvp_core show fb9175cc752d41e1cb525e0609b7c96140940217:<path>` через argv subprocess безshell; проверяет Git blob до любой BOM/CRLF-нормализации, сохраняет original bytes в W/original/<path>, transformed copies — W/A/src и W/B/src. Source shape/pin mismatch → отказ, не changedHEAD fallback. SHA256 host/templates/source/input фиксируется автоматически до первой руки. Source links ниже — local working files; pin обязателен и не подменяется текущим HEAD.

| Absolute original | Git blob | Copy |
|---|---|---|
| C:/Projects/llm_mvp_core/00.Core/OperationResult.cs | 2bfde76ca7d99f63eb657e736330da177838c644 | OperationResult.cs, полный |
| C:/Projects/llm_mvp_core/00.Core/LogicalContracts.cs | fbba6f97f226d96d0d36ed08d6befa8338e86434 | LogicalContracts.cs, точный prefix1–111 до единственного marker `// This record carries a request to execute one logical flow.` |
| C:/Projects/llm_mvp_core/29.SemanticCompiler/Core/Core.cs | 0dc09344f15816c11f23e3166bfe8e24ff9fada9 | SemanticCore.cs, полный |
| C:/Projects/llm_mvp_core/01.Tokenizer/BpeTokenizer.cs | d7c5e69308a32bf832ec4b087097a35b26f51d64 | BpeTokenizer.cs, полный |
| C:/Projects/llm_mvp_core/01.Tokenizer/RegexSplitter.cs | ad3eed613fb90777af20ed07b6f8340b8c16bfd9 | RegexSplitter.cs, полный |
| C:/Projects/llm_mvp_core/01.Tokenizer/VocabEntry.cs | 396257edb67bf0c409362f487c3d0b6d10b4225d | VocabEntry.cs, полный |
| C:/Projects/llm_mvp_core/06.VectorSearch/IVectorIndex.cs | e952a8679d098b9e511f2a656b98f7c55b044504 | IVectorIndex.cs, полный |
| C:/Projects/llm_mvp_core/06.VectorSearch/Distances.cs | 3bef456735d33aa23228c6aee72dde308a09bcac | Distances.cs, полный, неизменныйA/B |
| C:/Projects/llm_mvp_core/06.VectorSearch/HnswIndex.cs | 03287c05bcd6c4422d679a3b2e37a03f418918d6 | HnswIndex.cs, полный с объявленнымиadaptations |
| C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs | 2957816a88aa9cdcd3020bb81be56b5dfff165e0 | KnnLogicalUnit.cs, полный, learner неизменён |

W/A иW/B содержат Hnq.csproj,Program.cs,LabProbe.cs и10explicit sourcecopies. LabProbe — `public static` class в global namespace, доступный VectorSearch без dependency. Project: Microsoft.NET.Sdk, OutputTypeExe,TargetFrameworknet9.0,Nullable/ImplicitUsingsenable,EnableDefaultCompileItemsfalse,AssemblyNameHnq,ImportDirectoryBuildProps/Targetsfalse; Compile Include **явно** Program.cs,LabProbe.cs и десять filenames таблицы с `src/`, безwildcard. Никаких ProjectReference,PackageReference,IVT,внешних DLL. Два OperationResult живут в разных native namespaces; их не объединять. LogicalContracts prefix содержит flags/request/result/PhaseTransitionSignal/ILogicalUnit; Domain даёт SemanticCore.

Все paths source/output/bin/obj≤200символов; превышение — PATH_CONTRACT_FAILURE, не новыйroot/настройка ОС. W/input содержит только3исходныхJSON. Results толькоR/A,R/B,R/report.md,candidate.md,metrics.json,provenance.json; setup logs H/logs отдельно. Python3/.NET9/pwsh найти Get-Command, записать фактический executable path/version, не hard-code чужой Python. Missingdependency — точный setup status, безinstall/fullrepo build/заменыопыта.

## 5. Единственный B factor и общая telemetry

Original [Distances.CosineSimilarity:10–20](C:/Projects/llm_mvp_core/06.VectorSearch/Distances.cs:10) остаётся без изменений. В B HnswIndex добавить:

```csharp
private static float QuerySquaredNorm(float[] query)
{
    float na = 0f;
    for (int i = 0; i < query.Length; i++) na += query[i] * query[i];
    return na;
}
private static float CosineWithQueryNorm(float[] a, float[] b, float na)
{
    float dot = 0f, nb = 0f;
    for (int i = 0; i < a.Length; i++)
    {
        dot += a[i] * b[i];
        nb += b[i] * b[i];
    }
    float denom = MathF.Sqrt(na) * MathF.Sqrt(nb);
    return denom < 1e-9f ? 0f : dot / denom;
}
```

В начале каждого GreedySearch/SearchLayer/SelectNeighbours ровно один `float querySquaredNorm=QuerySquaredNorm(query);`. Пять replacements:[167/175/198/215/228](C:/Projects/llm_mvp_core/06.VectorSearch/HnswIndex.cs:164): query,entry ×1;query,nb ×2;query,e ×1;query,c ×1. Sort/comparer/earlytermination/m/ef/k/state unchanged. Не double/SIMD/dot-only/different epsilon/renormalization. Норма локальна вызову, не dictionary/поле/checkpointcache. Concurrent mutable aliases не испытываются; Native Unit синхронно создаёт свои vectors, API ownership не чинится. В8-nodegraph reversepruning maxM16/8 не активируется, его runtime coverage не заявлять.

Общие A/B adaptations: seed7331; одинаковая Add telemetry; explicit sourceclosure; одниfixtures. В начале Add `using var scope=LabProbe.MeasureAdd();`: struct IDisposable с ticks/threadallocated и ordinal, безheapallocation внутриtightloop. Per-Add record добавляется после остановки измеряемогоAddinterval; внешнийTrain всё равно включаетегоцену. A five sites → OriginalCosineWithProbe(a,b): active-Add CosineCalls++ → неизменныйnativeCosine. B → PatchedCosineWithProbe(a,b,na): тотжеcounter++ →helper. В обеих копиях начало трёх querymethods вызывает одинаковый LabProbe.QueryMethod(query.Length). Counters внеAddнерастут.

Сохранять actual AddCalls,CosineCalls,QueryMethodCalls,QueryMethodLengthSum,perAddelapsed/allocated. Derived norm-builds A=CosineCalls,B=QueryMethodCalls; norm-elements A=CosineCalls×64,B=QueryMethodLengthSum. Это арифметическийproxy по actual path, не измеренныйCPUtime/speedup. ПервыйAdd каждогоTrain естественно0cosine — negativecontrol безновогоquery. Для attribution графы и counts сравнений совпадают; divergence сохраняется. Instrumentation overhead — ограничение timing.

## 6. Program, readout и квалификация обучения

Program принимает ровно4аргумента: A|B, absolute W/input, absolute R/A|B, run01. Проверяет заранее назначенные paths, fixtures, owner HNQ_OWNER_NONCE; создаёт issued.marker до nativecalls. Native call:

```csharp
KnnLogicalUnit<string,string>.Train("hnq01-enru", "translation-executor",
    "TranslationDomain", rows, s => s, 64, 3, 0.70);
```

Никаких Tasks/benchmark loops. Native !IsSuccess сохраняет ErrorCode/Message; fakeUnit не создаётся. Перед Train4 иTrain8 сброситьprobe. Измерять полный Train Stopwatch.GetTimestamp/GetElapsedTime, GC.GetAllocatedBytesForCurrentThread и processTotalProcessorTime с явно записанными границами; CPUснимать отдельно от synchronous allocation/time region, noforcedGC. Native Manifestсохранятьвесь. ПослекаждогоTrain ровно24:

```csharp
unit.ExecuteAsync(new LogicalUnitRequest<string>(runId, "hnq01-enru",
    "translation-executor", "TranslationDomain", source,
    new LogicalExecutionFlags(), new Dictionary<string,object>())).GetAwaiter().GetResult();
```

Сначала durable raw output/confidence/evidence/warnings/error каждогоcase иDoubleToInt64Bits(confidence); только затем scorerчитаетgold. Scorer `HNQ_ACCEPTED_EXACT_V1`: lowerInvariant→ё/е→trim whitespace иUnicodepunctuation/symbols по границам→collapseUnicodewhitespace в одинпробел; exact iff равен одному normalizedaccepted. Сохранить normalizedoutput и acceptedvariant index. Action/object regex не использовать. Scorer не меняет Unit/query и не вызывает learner.

**Назначенная автором qualification, не обещание PASS:** warm x01..x04 должны дать ≥2 разных правильных **полных** labels, что превышает constant1/4. Иначе BEFORE_UNQUALIFIED: main Train этой руки не выдаётся, partial/OPEN сохраняется, seed search запрещён. После полного назначенного A/B каждая рука должна иметь ≥1 новый correct exact из x05..x08. До расширения этих четырёх labels нет в native exampleMap, поэтому их появление — чтение нового выученного материала. Отрицательный after quality не отменяет технически исправное B ради выгодного отбора: конечную пару завершить без дополнительных calls, сохранить регрессии/OPEN. Это qualification обучения, **не** минимальный порог полезного ресурсного выигрыша. Точность всех8, near/composition/edge показывается отдельно.

После каждой панели reflection читает `_index`, вызывает настоящий HnswIndex.Save один раз в новый R/arm/index-before.json или index-after.json. Save выполняется вне Train interval; полный restore Unit не обещается. Сохранять A0.json/A.json либо B0.json/B.json, before-/after-predictions.json, status.json, manifest, input row hashes. Monotonic issued/returned counts и actual Add/Execute не обнулять при позднем exception; unknown≠0. Все outputs пишет один owner, existing raw не перезаписывается. В status.json раздельно: execution_status, learning_validity, hypothesis_status; неполное/невалидное обучение сохраняет hypothesis_status=OPEN.

## 7. Бюджет, сравнение и выход

| Native операция | На arm | Максимум A+B |
|---|---:|---:|
| Train4 + Train8 | 2 | 4 |
| Presentations / HNSW Add | 12 | 24 |
| ExecuteAsync24до +24после | 48 | 96 |
| Result HNSW Save | 2 | 4 |
| Extra warmup/reverseorder/timingrepeat/teacher/DPO/LoRA/новыеqueries | 0 | 0 |

A→B, два свежих процесса. Максимум120секунд на arm и300секунд A→B→report после build — заданные аварийные ceilings, **не** ограничения tool. Не сокращать8уроков для двух минут. Timeout=INCOMPLETE_BUDGET с actual counters, неизвестный exit=UNKNOWN; no rerun/new root. Before fail или technical failure A прекращает выдачу B, неполный A/B не PASS; отрицательный after quality при technical success A не блокирует назначенное B.

Метрики A0/B0 и A/B: полный Train wall/CPU/allocated; вложенный Add elapsed/allocated; actual calls и derived norm work; old exact4, new exact4, near3, composition8, edge5 correct/total; coverage/errors/warnings/confidence; Count4→8 и ordered index vectors/ids/graph. Сравнить A0↔B0 и A↔B: byte/hash index, substantive output/confidence bits/evidence, semantic manifest identity/config/count/tokenizer/index/ingredients. **TrainingTimeMs и ManagedAllocationBytes исключены из semantic parity**, показываются как расходы. Не суммировать вложенные Train/Add времена как независимые.

Любое полезное измеренное улучшение в валидном учебном сравнении — успех оси/EN_RU generic KNN, без accuracy/5% порога; регрессии всегда рядом. «Та же выученная память дешевле» требует parity; иначе INVALID_EQUIVALENCE при сохранённых observations. Одно A→B timing не доказывает устойчивый причинный speedup, derived norm work не CPU time,8nodes не large graph/full RP01. Полный checkpoint restore Unit здесь не измеряется.

## 8. Единственная внешняя команда и внутренние стадии

Это будущая author-owned CLI, сейчас NOT_MATERIALIZED/NOT_RUN:

```powershell
pwsh -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HNQ01/Run-HNQ01.ps1
```

Весь script/host contract готовится до единственного launch. Run после early claims сам последовательно вызывает:

```text
python C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HNQ01/harness/prepare.py --repo C:/Projects/llm_mvp_core --work C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HNQ01/work --source fb9175cc752d41e1cb525e0609b7c96140940217
dotnet build C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HNQ01/work/A/Hnq.csproj -c Release --nologo -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false
dotnet build C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HNQ01/work/B/Hnq.csproj -c Release --nologo -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false
python C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HNQ01/harness/run.py --work C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HNQ01/work --results C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HNQ01/results --run-id run01
python C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HNQ01/harness/report.py --work C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HNQ01/work --output C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HNQ01/results
```

Не запускать эти подкоманды самостоятельно. MSBuild flags — global properties, чтобы отключение parent imports действовало до Sdk.props, не только поздно внутри csproj. Каждый build: Start-Process -WindowStyle Hidden -PassThru, отдельные stdout/stderr в H/logs, сохранённые PID/start; bounded WaitForExit≤60000ms, общий deadline120000ms. Wait того же PID повторять допустимо. При deadline завершить только собственное PID дерево, сохранить exit/UNKNOWN, без retry. Setup fix до first issued допустим только в собственной копии без factor/data/metric changes; эта документная сессия его не выполняет.

run.py: shell=false, argv subprocess, timeout120. Сначала `dotnet W/A/bin/Release/net9.0/Hnq.dll A W/input R/A run01`, затем аналогично B; все W/R развёрнуты из §2. Owner nonce унаследован, прямой child без owner отказывает. Run получает terminal exit; исходный deadline не перезапускается wait вызовом. Unknown не разрешает новый launch. report.py читает только raw, без Train/Execute/Load.

При обычном завершении: R/report.md≤350слов по-русски плюс таблица, R/candidate.md, metrics.json, provenance.json. Полезный кандидат обязательно записать≤120слов в [required-changes.md](../../required-changes.md): source fix, measured axis, domain, regressions, raw link. Ручные checksum rituals/extra Train ради review не назначаются; native/source automatic receipts сохраняются. Перенос product решает Люба. По «заканчивай и переходи»/«следующий эксперимент» немедленно NEXT в разрешённой фазе без дописывания report/hash/review/retry; по «остановить всё» немедленно остановиться. Разрешено лишь необходимое прекращение собственного процесса, если он продолжает работать. Не продолжать старую задачу после перехода.

## 9. Навыки, source readback и промпт

Применимые repo skills читаются по точным путям: C:/Projects/llm_mvp_core/docs/skills/architect-behavior-classifier-SKILL.md; C:/Projects/llm_mvp_core/docs/skills/phase-planning-SKILL.md; C:/Projects/llm_mvp_core/docs/skills/plan-review-SKILL.md; C:/Projects/llm_mvp_core/docs/skills/generated-code-review-SKILL.md; C:/Projects/llm_mvp_core/docs/skills/experiment-design-SKILL.md; C:/Projects/llm_mvp_core/docs/skills/prompt-patch-authoring-SKILL.md; C:/Projects/llm_mvp_core/docs/skills/human-readable-architecture-audit-SKILL.md; C:/Projects/llm_mvp_core/docs/skills/human-communication-GLOSSARY.md. Роли: причинность→малый план→review фактора/бюджета→review source copy→человеческий отчёт; это не расширяет runtime phase.

Исходная incoming карточка239строк прочитана полностью;13source/fixture blob hashes проверены readonly и совпали с pin/current text. Полностью прочитаны HnswIndex/Distances/KnnLogicalUnit и короткие зависимости; BPE Train/Encode/helpers и LogicalContracts1–112 адресно. Native learning/build не вызывались; byte closure не compile proof. [Evidence](../../2026-10-21-8346-training-evidence.md); [полный standalone промпт](../prompts/HNSW-QUERY-NORM-REUSE-01-executor.md). Это новый cost factor из incoming, не результат старого P125 и не доказательство поломки общего обучения.
