# 32. Разделить ключи unigram и bigram в обучаемом Wiki-маршрутизаторе

<!-- transfer-entry-reload -->
**Вход исполнителя.** [Q59](01-registry.md#work-fix-wk). Для общего включения действует измеренное ограничение этой строки. Возврат к работе — по записанному условию для её домена. Перед каждым переносом и после восстановления контекста заново прочтите полностью агентские файлы, назначенные навыки, исправленный список Любы, текущий индекс и карточку. [Как применить прочитанное к этой работе](03-test-and-execution-contract.md#read-and-apply) · [Что взять сейчас](01-registry.md#executor-next).

<!-- XIIGEN-AUDIT-FOLLOWUP-20261111-WIKI32:START -->
## Ключи, качество и ресурсный допуск — уточнение 11 ноября 2026

Это профилактическая применимость F02/F11 к FIX-WK, а не новая найденная регрессия. **Запрет общего переноса из уточнения 3 ноября сохраняется.** FIX-WC (извлечение признаков один раз) и FIX-WK (разделение unigram/bigram) — разные вмешательства; приёмка одного не разрешает другое.

В собственном A/B сохранить source rows, порядок/multiplicity, стартовое состояние, дозу и ordinary consumer. Раздельно измерять collision/key isolation, TRAIN acquisition, held-out ответы, public gate, allocations/time/heap и checkpoint size. Рост confidence без правильного маршрута не является ростом точности.

Updates и active epochs считать раздельно по поэпоховому ledger. На отдельной WC01 панели — 7 updates, 4 активные и 14 пустых эпох; это не назначает FIX-WK четыре или семь эпох. При early-stop первая полная эпоха без изменений тоже входит в расход. [Проверяемый replay](audits/2026-11-11/followup/product-recomputation.json) не является native замером скорости.

Ресурсный threshold соответствует договору этой карточки. Медиана, более широкий катастрофический guard или окно другого переноса не заменяют её 5% границу. Сохранить каждую парную A/B строку и сценарий, signed delta и разброс; редкое ухудшение не скрывается медианой. Мутация отмены оптимизации должна различаться ресурсным контролем при сохранённой семантике; зелёный semantic-equivalence test сам по себе не охраняет экономию.

Новое обучение и переключение product default этим уточнением не назначены.
<!-- XIIGEN-AUDIT-FOLLOWUP-20261111-WIKI32:END -->


## Уточнение допуска по показателям — 3 ноября 2026

**FIX-WK: Не допускать общий перенос.** Вошёл по времени и изменению оценки. Это не улучшение точности; рост памяти >5% нарушает правило.

- Точность: В A и B 0/6 → 3/6; после обучения 3/6 → 3/6 между вариантами (0 п.п.).
- Полезный результат: Время 6,0871 → 4,0095 мс (−34,13%); у одного отложенного запроса уверенность 0,271 → 0,462, но верный маршрут не выбран.
- Цена и ограничения: Выделения 227 648 → 325 280 Б (+42,89%); снимок 9 737 → 10 135 Б (+4,09%); куча после +6,73%.
- [Полное сравнение и статус кода](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md#benefit-fix-wk).

Этот вывод относится к названным ID, а не ко всем механизмам документа. Старый текст плана и опыта сохранён как доказательство, но его общий статус «к переносу / разрешён» не переопределяет это уточнение. Ближайшее действие — разбор уже сохранённых данных и конкретного регресса; новое обучение, повтор PRIMARY и переключение product default этим дополнением не назначаются. Для отдельного логического ремонта назвать нарушенный контракт и сохранить отрицательные контроли, а не переносить весь эксперимент под словом «bugfix».


<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски ROUTE/FEATURE:** На названном маршрутизаторе/каталоге проверить writer→обученная память→ordinary route/payload и собственные positive/negative строки; label без выбранного payload не закрывает маршрут. Exact recall, простой baseline и предметная тема раздельно. Дополнительный dynamic same-functions фактор допустим только если назначен паспортом и проверяет скорость native Train. Сверить именованное преобразование входных признаков до настоящего learner/consumer; различающий положительный признак и теряющий его отрицательный контроль берутся из собственного корпуса. Формат/энергия/кодировка сами не дают новый навык; feature equality не заменяет независимый gold.

**Ожидания от теста.** Используйте данные этого ID и обычный путь, указанный в его действующей строке. Исходные доказательства сохраняются. Дополнительную диагностику из прежнего плана открывайте при конкретном расхождении. Для учебного результата запишите, что было до урока и что стало после. Техническую проверку формата или маршрута так и назовите.

**Проверка исполнителя.** Соберите затронутый проект и выполните назначенный малый сценарий на своей сборке. Укажите вызванный метод, вход и фактический результат. Для обученного поведения сравните смысл ответа и выбранного получателя. Для ресурса измерьте названную величину за тот же вызов. Если проверка упала, сохраните вход, ожидание и ошибку. Исполнитель может исправить дефект теста по независимому критерию и повторить эту проверку. Отпечаток нужен при разборе происхождения файла или сборки. Дождитесь завершения запущенной проверки. Затем переходите по текущей очереди. Право выполнить перенос определяется действующим назначением.
<!-- EXECUTOR-CONTEXT-120:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

Проверять один фактически исполненный путь: prompt → обученный выбор → выбранные model/profile → actual результат получателя → адрес записи нового урока → чтение после restore. Подстановка сохранённых logits/ответа вместо результата выбранной модели не доказывает этот путь: именно так ограничен новый P08 product test. Исторический oracle читается только для сравнения, а не подаётся как ответ следующему звену.

Для FIX-WK исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Доказывать вызов реального получателя после выбора, а не наличие созданного constructor/bridge. Контрпример P08: A/B TextGenerator создан, но sampled ответы получены вне него. Для этой карточки записать выбранную модель/профиль, реально вызванный метод потребителя, данные ответа и получателя нового урока; альтернативный маршрут и restore проверяются отдельно. Подмена ответа ближайшей строкой/таблицей gold не проходит E2E даже при правильном выборе домена.

Применимость: FIX-WK. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Проверять фактическую конфигурацию модели после маршрутизации: domain, checkpoint, tokenizer, функции KNN, trainable recipient и параметры обучения. Контрпример P12: direct Trainer Warmup4 отказывает, но фабрика IncrementalTrainingSession уже задаёт Warmup0; дефект одного вызова не доказывает отказ всего маршрута. После урока проверять сохранение выбранной и неизменность чужих моделей; hash без полного snapshot не является restore.

Применимость: FIX-WK. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: FIX-WK

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** ROUTE/FEATURE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**K — владение допущенным состоянием и восстановление.** Показать, какой объект хранит допущенные параметры, примеры, конфигурацию или ключ кэша; интерфейс IReadOnlyList сам по себе не доказывает неизменность. Контроль после создания/допуска: изменение исходного caller-owned массива или внешнего представления не меняет захваченную запись незаметно. Законное обновление/смена model identity вызывает предусмотренное обновление или инвалидирование кэша, а не запрет обучения. Если reader сохраняется/восстанавливается, сравнить поведение обычного consumer по сохранённым данным и настройкам: reconstruction с default не является эквивалентным restore opt-in режима. Ошибка из KDP: catalogue был проверен, затем изменён через возвращённый массив; schema не содержала catalogue. Для этой карточки проверять её собственный carrier, не добавлять чужой KNN-механизм ради теста. Если изменяемого carrier/сохранения нет, отметить это явно в evidence и не выдумывать лишний runtime.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

**Ограничение ресурса и reader, 27 октября 2026.** Allocation+42.89% при typed keys нельзя скрыть за wall−34.13%; общий профиль не проходит условие отсутствия ухудшения>5%. Исправление collision учебных признаков само по себе не является исключением P1-L. Plan остаётся ограниченным кандидатом, а legacy/v2 codec compatibility — обязательным контрактом корректности.

Уже назначенный learned legacy parity должен различать потерю lookup при целых labels/biases: равный размер snapshot и тот же отказ public gate этого не выявляют. Actual state включает codec и learned coordinates; restore не вызывает Featurize/Train по fixture для ремонта пропавшего key. Internal acquired outputs и public отказ gate сохраняются раздельно; snapshot с correctness labels не даёт права выставить CanStartTargetRunner. Новых fits сверх десяти из §6 нет.

**Уточнение исполнения, 27 октября 2026.** Не строить expected snapshot через новый codec: ошибка общего encoder иначе даст одинаковую неправильную A/B проверку. Legacy и v2 читаются из закреплённых независимых payload; сравниваются значения признаков и уже назначенные ответы по label names. После load запрещён fit «для согласования ключей». Public gate, internal evaluation и реальный маршрут остаются разными проверками: положительный internal readout не включает автоматически product receiver.

Общие примеры правильной и ошибочной проверки — [контракт03§24](03-test-and-execution-contract.md#transfer-audit-20261027). Это дополнение плана; новые проверки сейчас не запускались.

ID: **FIX-WK**. Статус: **ограниченный кандидат переноса WIKI-FEATURE-KEYS-01 / WK01**, только документация. Источники закреплены на `32d8887983c347f3bf3d674a5e9414d57eaec75b`. Порядок и следующий ID — только [реестр](01-registry.md): непосредственно после FIX-WC, перед FIX-WT. Тема — **оркестрация, профиль D07**: выбор действия Wiki по промпту. Идентификаторы исходного кода являются аргументами запроса, а не отдельной темой обучения.

## 1. Основание и точная граница успеха

Первичные доказательства: [A/report.json](https://github.com/lubochka/llm_mvp_core/blob/32d8887983c347f3bf3d674a5e9414d57eaec75b/experiments-v2/RP01-MICRO/WK01/results/attempt-001/A/report.json), [B/report.json](https://github.com/lubochka/llm_mvp_core/blob/32d8887983c347f3bf3d674a5e9414d57eaec75b/experiments-v2/RP01-MICRO/WK01/results/attempt-001/B/report.json), [Program](https://github.com/lubochka/llm_mvp_core/blob/32d8887983c347f3bf3d674a5e9414d57eaec75b/experiments-v2/RP01-MICRO/WK01/Program.cs#L62-L129), [one-factor.diff](https://github.com/lubochka/llm_mvp_core/blob/32d8887983c347f3bf3d674a5e9414d57eaec75b/experiments-v2/RP01-MICRO/WK01/one-factor.diff). Завершённый опыт обучал source copies native lexical perceptron. Его вход называется `DpoPair`, однако здесь нет neural DPO, LoRA или генерации: `Train` использует `Prompt` и `ExpectedRouteKind`.

Разные provenance не смешиваются:32d888 — commit чтения опубликованного результата; raw сообщает execution `coreCommit=ab583032562d6656de9dd5ca6fa0a325fd4b1f30`; [source-provenance.json](https://github.com/lubochka/llm_mvp_core/blob/32d8887983c347f3bf3d674a5e9414d57eaec75b/experiments-v2/RP01-MICRO/WK01/source-provenance.json) закрепляет более ранний source origin`62bcc8f2368840e22d2ea98ef16d9bd526ad9a39`. Native model blob на32d888 — `3672bd2c9dd813eb2a31835c2e6b06071de37869`, совпадает с source provenance. Чтением сопоставлено математическое ядро baseline copy с native; это не выдуманная проверка historical loaded DLL или единый SHA для всех стадий.

| Показатель непустого обучения / после него | A | B | Вывод |
|---|---:|---:|---|
| Wall непустого `Train` | 6,0871 ms | 4,0095 ms | **Измеренное улучшение −2,0776 ms / −34,1312%** |
| CPU непустого `Train` | 15,625 ms | 15,625 ms | Одинаково, короткий интервал |
| Allocated bytes текущего потока | 227 648 | 325 280 | **Ухудшение +97 632 / +42,8873%** |
| Размер after-snapshot | 9 737 bytes | 10 135 bytes | **Ухудшение +398 / +4,0875%** |
| Peak working set процесса | 33 910 784 bytes | 33 951 744 bytes | +40 960 bytes |
| Правильные выбранные маршруты | 0/6→3/6 | 0/6→3/6 | Настоящее приобретение результата в каждой руке, без роста A→B |
| Ошибочные / отложенные после | 1/6 / 2/6 | 1/6 / 2/6 | Не улучшились |
| Признаки / параметры / ненулевые параметры | 38 / 585 / 94 | 39 / 600 / 95 | Изменение механизма и памяти, не само по себе качество |

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и панели; рост точности не обязателен.** Поэтому исторический headline `NO_GAIN` из [report.ru.md](https://github.com/lubochka/llm_mvp_core/blob/32d8887983c347f3bf3d674a5e9414d57eaec75b/experiments-v2/RP01-MICRO/WK01/results/attempt-001/report.ru.md) не является основанием отбросить измеренное уменьшение времени обучения. Старый отчёт не переписывается: здесь уточняется область его вывода. Это один короткий холодный процесс на плечо, включая helper/JIT costs; причинная устойчивость ускорения не установлена. −34,13% не становится обещанием production или обязательным порогом CI. Повторить consumed WK01 для признания уже измеренного выигрыша не требуется.

Дополнительные изменения решений фиксируются по строкам, без подмены diagnostic confidence точностью:

| EXAM | Изменение A→B | Что осталось прежним |
|---|---|---|
| K-Q1, правильный CodeTraceTraining | Margin 0,2285504344496469→0,23643854082711305; confidence 0,8928919562236168→0,8943473227822809 | Верный Selected |
| K-Q2, ожидается WikiQuestionAnswer | Отрыв правильного скрытого winner 0,011068693944939381→0,018858502321984577; low-confidence diagnostic 0,27118300165101483→0,4620333068886221 | Оба margin ниже 0,02; **NeedsSecondPass**, правильный target не запущен |
| K-Q4, ошибочный CodeTraceTraining вместо WikiQuestionAnswer | Отрыв ошибочного winner 0,07481917147269898→0,060000000000000026; confidence 0,8622675323525363→0,8591908567265432 | Ошибочный Selected сохранился |
| K-Q3 / K-Q5 / K-Q6 | Readout не изменился | Два правильных Selected / один deferred |

Для K-Q2 hidden winner восстановлен **арифметически из опубликованных весов**, не новым fit: score WikiQuestionAnswer одинаков `0,08332415534951491`; конкурирующий CodeTraceTraining `0,07225546140457553→0,06446565302753034`. Это вывод из [A snapshot](https://github.com/lubochka/llm_mvp_core/blob/32d8887983c347f3bf3d674a5e9414d57eaec75b/experiments-v2/RP01-MICRO/WK01/results/attempt-001/A/after-snapshot.json), [B snapshot](https://github.com/lubochka/llm_mvp_core/blob/32d8887983c347f3bf3d674a5e9414d57eaec75b/experiments-v2/RP01-MICRO/WK01/results/attempt-001/B/after-snapshot.json) и native Score/Featurize. Raw `actualRouteKind=NotProjectWiki` здесь — placeholder отложенного решения, не имя скрытого победителя. Margin и полученная из него confidence не считаются двумя независимыми выигрышами. Constant CodeTraceTraining также получает3/6; устойчивое различение классов или готовность роутера к выпуску не доказаны.

## 2. Точное место переноса и участие в обучении

Алгоритмический получатель — [42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs, Featurize:341–356](https://github.com/lubochka/llm_mvp_core/blob/32d8887983c347f3bf3d674a5e9414d57eaec75b/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L341-L356). Старый unigram `load_customer` сталкивается с bigram `load`+`customer` в ключе `load_customer`. Измеренный candidate задаёт:

~~~text
unigram = u:<token.Length>:<token>
bigram  = b:<left.Length>:<left>:<right.Length>:<right>

u:13:load_customer
b:4:load:8:customer
~~~

Использовать именно UTF-16 `string.Length`, десятичную запись через `CultureInfo.InvariantCulture`. Сохранить `RegexLikeWords`, `char.ToLowerInvariant`, `_` и `-` внутри token, `StringComparer.Ordinal`, частоты, порядок циклов, формулу L2-нормировки, 18 эпох, LR0,14, mistake-only updates, tie-breaking, Labels и все пороги. После разделения координат значение нормирователя может измениться; это следствие устранения коллизии при прежней формуле. Не добавлять WM margin-update rule, taxonomy, retention union/max или новый lexer.

Путь участия — [Train:58–83](https://github.com/lubochka/llm_mvp_core/blob/32d8887983c347f3bf3d674a5e9414d57eaec75b/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L58-L83) → признаки `row.Prompt` → prediction → updates weights/biases; при чтении [DecideOrDefer/Score:155–253](https://github.com/lubochka/llm_mvp_core/blob/32d8887983c347f3bf3d674a5e9414d57eaec75b/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L155-L253) обязан использовать тот же codec. Реальный [TrainingRunner:43–53](https://github.com/lubochka/llm_mvp_core/blob/32d8887983c347f3bf3d674a5e9414d57eaec75b/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingRunner.cs#L43-L53) вызывает этот learner. Runtime [ProjectWikiRequestRoutingUnit:418–426](https://github.com/lubochka/llm_mvp_core/blob/32d8887983c347f3bf3d674a5e9414d57eaec75b/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L418-L426) восстанавливает snapshot и вызывает gated `Route`. Поэтому формат сохранения ниже непосредственно определяет результат обучения при использовании.

### Версия состояния и восстановление без повторного Train

`FromSnapshot` сейчас не знает семантики keys, а `ToSnapshot` пишет прежний ModelKind. Нельзя заменить только `Featurize`: old weights перестанут находиться. Старую координату `load_customer` невозможно однозначно разложить на два выученных веса.

Назначенные product files — model выше и [ProjectWikiDomainModels.cs, snapshot record:575–615](https://github.com/lubochka/llm_mvp_core/blob/32d8887983c347f3bf3d674a5e9414d57eaec75b/42.ProjectWikiDomain/ProjectWikiDomainModels.cs#L575-L615). Точный контракт:

1. В тело record добавить init-property `string? FeatureKeyCodecVersion`; positional constructor не менять. Отсутствующее/null поле означает **`legacy-joined-v1`**. Другой признанный literal — **`namespaced-length-v2`**. Пустая строка и неизвестное значение вызывают `InvalidDataException` в `FromSnapshot` до чтения маршрута; не угадывать codec по keys.
2. В model хранить readonly `_featureKeyCodecVersion`. Новый `Train`, включая empty init, создаёт v2. `FromSnapshot` выбирает legacy/v2 по явной версии. Версию передавать в `Featurize` и из Train, и из DecideOrDefer; legacy ветвь сохраняет исходные keys/нормировку.
3. `ToSnapshot` записывает разрешённую версию; `WithGateEvidence` копирует её вместе с weights/biases. `ModelKind` остаётся прежним: новая ось относится к входному codec, не к составу выходных labels FIX-WT. Никакой второй tokenizer.json не создаётся; snapshot уже сериализуется внутри [training result publisher:71–82](https://github.com/lubochka/llm_mvp_core/blob/32d8887983c347f3bf3d674a5e9414d57eaec75b/42.ProjectWikiDomain/ProjectWikiRequestRoutingManifestPublisher.cs#L71-L82).
4. Legacy snapshot читает legacy learner с неизменными weights. Не переименовывать/размножать неоднозначные координаты и не делать автоматический Train при загрузке. Новое v2 состояние создаётся только назначенным обучением и сохраняется отдельно. Прошедший gate старой интерпретации не переносить к преобразованной модели; в этой карточке преобразования legacy weights вообще нет.
5. Исторический B имеет v2 keys, но старый ModelKind и **не имеет нового поля**. Он закреплён как независимый числовой oracle, не production donor для `FromSnapshot`. Его нельзя выдать за legacy либо автоматически «исправить» источник. Cold tests используют новый native B snapshot с правильной версией. Исторический A используется для действительного legacy restore.

FIX-WT отдельно меняет labels15→17 и named-coordinate migration; FIX-RET относится к `PromptFlowGroundingModel`, а не этому classifier. Общая база A/B обязана иметь один label contract. Не внедрять WT ради WK и не откатывать уже принятый WT. Если на общей базе уже17labels, historical numerical oracle сопоставляется **по имени прежних15labels**, две добавленные координаты требуют нулевых значений для этой панели; actual17label readouts фиксируются отдельно и не выдаются за историческое измерение. Сам мигратор FIX-WT и его правила gate сохраняются. Любое другое несовпадение baseline с закреплённой математикой — явная несовместимость, без замены gold, корпуса или скрытого переобучения.

Ограниченное применение этого oracle к17labels имеет конкретное основание: в historical WK ненулевые только heads ProjectWikiRequestRouterTraining, CodeTraceTraining и WikiQuestionAnswer; остальные12heads нулевые. FIX-WT сохраняет позиции прежних15 и добавляет два класса в конец. При этих нулевых конкурентах и неизменном strict-`>` tie-breaking добавление ещё двух нулевых heads не меняет argmax/top-two данной панели; её TRAIN targets также прежние. Preflight проверяет именно этот список/порядок, нулевые добавленные coordinates и неизменный алгоритм. Это рассуждение для фиксированной WK панели, не обещание parity произвольной15→17миграции с отрицательными scores; предупреждение FIX-WT сохраняется.

## 3. Замороженные данные и состояния

Источник — [fixture.json](https://github.com/lubochka/llm_mvp_core/blob/32d8887983c347f3bf3d674a5e9414d57eaec75b/experiments-v2/RP01-MICRO/WK01/fixture.json), SHA256 original bytes **`90d70b9c6998d921465720ac743e095aade7d3f5126b937ae7dd2ee6305ce63d`**. Обе цели относятся к одной теме оркестрации; данные не обучают ответам о коде или документам Wiki. Gold видит только harness; native reader получает prompt без target/expected metadata.

| TRAIN ID, фиксированный порядок | Полный prompt | ExpectedRouteKind |
|---|---|---|
| K-T1 | `Train the code trace builder on calls inside load_customer.` | CodeTraceTraining |
| K-T2 | `Answer the project wiki question about how services load customer records.` | WikiQuestionAnswer |
| K-T3 | `Train the code trace builder on calls inside save_order.` | CodeTraceTraining |
| K-T4 | `Answer the project wiki question about how services save order records.` | WikiQuestionAnswer |

| EXAM ID | Полный prompt | ExpectedRouteKind |
|---|---|---|
| K-Q1 | `Train code trace using source calls from load_customer.` | CodeTraceTraining |
| K-Q2 | `Answer the wiki question about which components load customer records.` | WikiQuestionAnswer |
| K-Q3 | `Learn source call edges in save_order for code trace training.` | CodeTraceTraining |
| K-Q4 | `Use project wiki evidence to answer how components save order records.` | WikiQuestionAnswer |
| K-Q5 | `Train code trace on source call edges.` | CodeTraceTraining |
| K-Q6 | `Answer the project wiki question about service ownership.` | WikiQuestionAnswer |

Targets: CodeTraceTraining → `code-trace-builder-unit`, WikiQuestionAnswer → `project-local-wiki-orchestrator`. Для каждого TRAIN создать ровно один `ProjectWikiRequestRoutingDpoPair` по конструктору Program: Id из таблицы; domain `isolated-project-wiki-route`; Prompt и A равны prompt; ExpectedRouteKind из таблицы; Chosen — `route_kind: <kind>\ntarget_unit_id: <target>`; Rejected — `[WRONG: wrong_training_unit]\nroute_kind: NotProjectWiki`; RejectedType — `wrong_training_unit`. Здесь `\n` обозначает один LF. Никакой генератор новых строк или внешняя модель не вызываются.

Будущий малый паспорт — `Tests/Fixtures/ExperimentFixes/v1/WikiFeatureKeys01/manifest.json`; рядом exact `fixture.json`, `historical-A.json`, `historical-B.json` как извлечённые read-only oracle с исходными ссылками/хэшами. Не менять original bytes ради нормализации LF. Полные исходные snapshots находятся под `experiments-v2/RP01-MICRO/WK01/results/attempt-001/`:

| Файл относительно attempt-001 | Bytes оригинала | SHA256 original bytes | Использование |
|---|---:|---|---|
| `A/before-snapshot.json` | 1 256 | `7d227e2810b8505477c1c60ff74e4e2972e978f58bbd507f36c6730487a77c9c` | Oracle нулевого старта |
| `A/after-snapshot.json` | 9 737 | `427f89669a21c368ab886455b2961b36103dad8889dc97df91b386e260507bd0` | Настоящий сохранённый legacy donor, без Train |
| `B/after-snapshot.json` | 10 135 | `cd7cdfa46c787564cdbcd729367b71e6aed9faa4dac49ec8153a815b2dcfff3f` | Только parameter/readout oracle, без native загрузки untagged B |

Original JSON содержит CRLF. Это file hashes, **не** raw `stateSha256` canonical projection Labels/Biases/отсортированные TokenWeights из [Program:238–244](https://github.com/lubochka/llm_mvp_core/blob/32d8887983c347f3bf3d674a5e9414d57eaec75b/experiments-v2/RP01-MICRO/WK01/Program.cs#L238-L244). Canonical state before `68d955181059b3b4962ecbdfbb5a11b726a85cb003bfac34bc5022a3652952aa`, after A `0a04573bab0266ce08910836c68c75774fdb73afd0765ddf0ac63f23207fe6a0`, after B `ea84c2a99add3634ce94770218ac3cb326542134e3bada73f53db9a54eea24bd`.

Новый native writer проверяется одним приобретением навыка на4уроках: empty `Train([])` задаёт нулевое состояние без presentations, затем один `Train(4 rows)` даёт ненулевые правильные решения. Здесь не нужен отдельный bootstrap/почти обученный neural checkpoint. Финальные native состояния один раз сохраняются в `artifacts/fbe/FIX-WK/attempt-0001/learning/{A,B}/state/after.snapshot.json`. Все проверки restore/codec/gate ниже используют эти bytes или historical A; **не обучают заново**. В snapshot `TrainingRowCount=0` отражает missing gate, не отсутствие реальных4уроков; фактические rows/presentations указываются отдельно в receipt.

## 4. Исполнимый native test seam и два SHA

Тесты находятся в `Tests/ExperimentFixes/`, namespace `LlmFromScratch.Tests.ExperimentFixes`, существующий проект `Tests/LlmFromScratch.Tests.csproj`, Release/net9.0. Historical public evaluation wrapper не переносится. Native `RouteForEvaluation` — internal; заранее назначен узкий доступ: в `42.ProjectWikiDomain/42.ProjectWikiDomain.csproj` добавить `<InternalsVisibleTo Include="LlmFromScratch.Tests" />` в ItemGroup. Он одинаков в A/B, evaluation method остаётся internal, public Route/gates не меняются.

Public TrainingRunner требует ровно50 TRAIN/50 held-out; полный pipeline ещё добавляет DPO. **Они не запускаются для этой панели и их пороги не снижаются.** Native тест вызывает настоящий public `Train` и internal evaluation reader через указанный friend. Измеряемое обучение не исполняется в archived BaselineModel/CandidateModel.

Общий runner — планируемый `Tests.Support/ExperimentTransfer/Run-NativeAB.ps1`, с явной ветвью `-PlanId FIX-WK`; сравнение — `Compare-NativeAB.ps1`. [03 §9](03-test-and-execution-contract.md#native-ab-transfer) задаёт последовательные Baseline/Candidate в одном canonical root, SHA/intent/receipts/waits; **WK имеет собственную конечную матрицу ниже**, а не автоматически запускает GB/RMS/BW. Common harness commit содержит tests/helpers/fixtures, одинаковый narrow friend и при необходимости [build prerequisite02§10](02-learning-state-and-fixtures.md#native-build-prerequisite). Сначала на нём завершается Baseline с сохранением A receipts; только затем человек или назначенный исполнитель меняет два product files из§2, проводит review и фиксирует candidate commit. Test/helper/fixture bytes не меняются. Сами snapshot property/codec появляются в candidate, не в baseline.

Чтобы один и тот же harness компилировался в A, test code читает/задаёт новое поле через JSON `featureKeyCodecVersion` с camelCase/case-insensitive options и десериализует существующий snapshot type. Он не ссылается на отсутствующую в A C# property и не выбирает алгоритм по ARM. Исправление старого equal-score helper ниже также работает через JSON. Не вводить conditional compilation, другой learner в A или copied B.

Единственный SourceRoot — `C:/Projects/llm_mvp_core`, ветка `codex/dpo-implementation`; здесь находятся read-only pinned donors и output `artifacts/fbe/FIX-WK/attempt-0001`. Worktree, sibling/copy, checkout/reset и переключение ветки запрещены; runner не создаёт Linux/sibling fallback. Missing/неверные donor bytes — конкретный blocked input, без полного LFS download, поиска другого старта или bootstrap. Каждая стадия заново выполняет `Rebuild` назначенного Tests проекта; loaded native model/snapshot assembly Location/SHA-256/MVID и actual source SHA записываются для своей руки. Готовые A receipts сохраняются до product edit и читаются при сравнении B, без повторного A или использования старой DLL после изменения кода.

## 5. Постоянные тесты и ремонт существующих

### Один учебный Fact на плечо

Будущий `WikiFeatureKeyLearningTests.cs`, exact method **`FrozenPanel_AcquiresRoutes`**:

1. Проверить fixture/assembly identity до вызовов модели: loaded `ProjectWikiRequestRoutingTrainableModel` и snapshot type принадлежат настоящему project42, сохранить DLL path/hash/AssemblyName из evaluated native csproj. Source diff/codec данные не являются отдельным успехом обучения.
2. Создать native empty state одним `Train([])`; сохранить snapshot; вызвать `RouteForEvaluation` на6EXAM. Должно быть0correct/0wrong/6deferred, а state digest не измениться от readout.
3. Один `Train` на4строках в заданном порядке, native18эпох. Измерить wall, CPU, allocated bytes, heap и peak **вокруг только непустого Train**, отдельно от fixture IO, empty init, serialization и evaluation. Не добавлять warmup/повторы/другой порядок ради времени.
4. Сохранить native after;6evaluation calls. Требуется приобретённый результат3correct/1wrong/2deferred и изменённые weights; поштучные decisions/margins/confidence и learned coordinates сверяются с закреплёнными oracle соответствующего фактора. Числовые float readouts/weights — absolute tolerance1e-12; raw values сохраняются. A и B states **не** должны быть одинаковы между собой. Новая версия metadata не входит в исторический canonical parameter digest.
5. Snapshot по-прежнему `GatePassed=false`, `SemanticQualityClaimed=false`, `PublishAllowed=false`. Честное приобретение навыка по evaluation reader не даёт public target authority. Сохранить before/after/readouts/стоимость/путь DLL в CreateNew receipt, включая failure receipt в finally; не терять actual бюджет при assertion fail.

Historical copies имели counters72presentations/3mistake updates. Native сейчас не экспортирует таких счётчиков. В новом receipt `Train` calls фиксируются harness;72presentations — `source_derived =18×4`; actual mistake-update count — `not_exposed`. Historical3 не выдаётся за новое измерение. Дополнительная production instrumentation или повтор обучения ради счётчика не назначены.

### Пять контрактных Facts только B, после завершённых learning receipts

Будущий `WikiFeatureKeyCompatibilityTests.cs`. Все tests читают immutable historical A либо `learning/B/state/after.snapshot.json`, **0Train**, без зависимости от порядка xUnit. Pair root задаёт runner через `XIIGEN_FBE_PAIR_ROOT`; обычные SourceRoot/Output/ARM — как03. Ничего не переобучать, если нужный learning receipt не завершён.

| Exact method | Действие и неизменяемый oracle | Максимум reader calls |
|---|---|---:|
| `LegacySnapshot_PreservesLearnedReadout` | Native FromSnapshot historical A без поля → legacy;6evaluation decisions равны A raw,6public Route остаются без target authority; weights по именам не изменены. Это проверяет действительное learned lookup, а не только synthetic equal biases | 12 |
| `NamespacedSnapshot_RoundTripsWithoutTraining` | Native B after → serialize/restore →6evaluation; ещё один JSON round-trip→6evaluation, сравнение с уже сохранённым B readout; WithGateEvidence **исходным missing gate** сохраняет codec/weights и false flags, без дополнительных Route | 12 |
| `LearnedCoordinates_SeparateIdentifierAndBigram` | В native B learned snapshot есть разные `u:13:load_customer` и `b:4:load:8:customer`, нет общего raw `load_customer`. По именам labels: CodeTrace weights соответственно +0,03395498750508662 и −0,030550504633038936; WikiQuestionAnswer 0 и +0,030550504633038936. Это координаты настоящего Train, не отдельно реализованный featurizer. Не требовать веса для bigram `save order`: его урок мог не вызвать mistake update | 0 |
| `UnknownCodec_IsRejectedBeforeReadout` | Копии B JSON с неизвестным literal и с пустой строкой дают InvalidDataException при FromSnapshot. Исходник/weights не изменять; отсутствующее поле проверено на настоящем legacy A в первом Fact | 0 |
| `MissingGate_DoesNotStartTarget` | Native B6public Route и native ProjectWikiRequestRoutingUnit для K-Q1/K-Q4 с B snapshot:8решений deferred, CanStartTargetRunner=false, отсутствует product publication. Unit result читается как есть; fake passed gate/ручной expected target запрещены | 8 |

Contract total32reader calls; проверки ключей/версии поддерживают сохранение и использование обученного результата, не выдаются за отдельные эксперименты качества. Для `WithGateEvidence` нельзя использовать synthetic passed gate, чтобы получить хороший ответ.

### Существующие regressions: точный список и ремонт

Существование подтверждено чтением [Tests/ProjectWikiDomainTests.cs:1206–1347](https://github.com/lubochka/llm_mvp_core/blob/32d8887983c347f3bf3d674a5e9414d57eaec75b/Tests/ProjectWikiDomainTests.cs#L1206-L1347). Новых фактических падений не заявляется: тесты в документной сессии не запускались.

- `ProjectWikiRequestRoutingTrainableModel_UntrainedModelDoesNotEmitHighConfidenceRoute`.
- `ProjectWikiRequestRoutingTrainableModel_ZeroMarginReturnsNeedsSecondPass`.
- `ProjectWikiRequestRoutingUnit_EqualScoreEvidenceDoesNotStartTargetRunnerOrPublishManifest`.

Первый делает empty Train напрямую; остальные вызывают [CreateEqualScoreRequestRoutingSnapshot:2512–2524](https://github.com/lubochka/llm_mvp_core/blob/32d8887983c347f3bf3d674a5e9414d57eaec75b/Tests/ProjectWikiDomainTests.cs#L2512-L2524), который тоже делает empty Train. Поэтому это **3empty Train/arm**, не один. Existing методы запускаются один раз в A и один раз в B.

Предусмотренный ремонт helper в общем harness: сохранить один empty Train и synthetic legacy key `ambiguous`, dimension брать из `snapshot.Labels.Length`, а JSON codec явно установить в `legacy-joined-v1` до Deserialize/FromSnapshot. Это тот же legacy zero-margin test, не случайный v2 snapshot с legacy key. Все assertions о deferred, margin0, confidence<0,5, CanStartTargetRunner=false и отсутствии manifest сохраняются. Одни equal biases могут маскировать потерю lookup; поэтому дополнительно назначен настоящий learned legacy parity Fact, а не ослабление guard.

Не назначать50/50 tests `SnapshotCarriesEvalGateAndSemanticQualityFalse`, `MeasuresHeldOutRouteMetrics` или полный pipeline только ради нового поля: они расширили бы учебный бюджет. Их исходники/гейты не удаляются; конкретное новое падение при отдельно обязательном gate разбирается адресно, не превращается в автоматический широкий прогон. Неправильный key writer чинится в Featurize; потеря codec — в snapshot/FromSnapshot/WithGateEvidence; неверный fixture — в его конкретной версии, без изменения gold. Unknown codec не лечится fallback на v2. Нельзя Skip/xfail, снижать product thresholds или менять состав тем ради зелёного результата.

## 6. Две будущие стадии, конечный бюджет и отчёт

После назначения внедрения и фиксации common harness на неизменённом native A:

~~~powershell
# baselineCommit — записанный SHA common harness из§4; candidate ещё не создавался.
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-WK -Stage Baseline -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit
~~~

Дождаться terminal A receipts. Затем назначенный исполнитель в том же корне вносит только product changes§2, проводит review и фиксирует `$candidateCommit`, сохранив прежние harness/test/fixture bytes. Вторая команда:

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-WK -Stage Candidate -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit -CandidateCommit $candidateCommit
~~~

Предпосылка02§10 одинакова в A/B. Всего **2project Rebuild**, по одному в каждой стадии перед её `--no-build --list-tests` и test calls. Discovery exact union: A3existing+1learning=4, B3existing+1learning+5compatibility=9. Отсутствующий test/другой case count — ошибка, не PASS. Runner не выбирает SHA и не подставляет branch HEAD; Candidate без завершённых A receipts останавливается без Train и не повторяет Baseline.

Ровно **5процессов dotnet test**: Baseline `existing A→learning A`; после product edit/review/commit Candidate `existing B→learning B→compatibility B`. Existing filter — `|` трёх полных `FullyQualifiedName=LlmFromScratch.Tests.ProjectWikiDomainTests.<method>` из§5; learning — `FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.WikiFeatureKeyLearningTests.FrozenPanel_AcquiresRoutes`; compatibility — `FullyQualifiedName~LlmFromScratch.Tests.ExperimentFixes.WikiFeatureKeyCompatibilityTests.`. Использовать форму `dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --filter $exactFilter --logger "trx;LogFileName=selected.trx" --results-directory $stageOutput`. Stage paths: `baseline/A`, `after/B`, `learning/A`, `learning/B`, `contracts/B`. Не запускать compatibility A: codec rejection является новым контрактом B. Одинаковый test source должен компилироваться в обеих сборках; Candidate сравнивает сохранённые A/B без повторного обучения.

| Расход всей назначенной проверки A+B | Максимум / точный состав |
|---|---|
| Native Train calls | **10:8empty +2непустых**;6empty existing guards,2empty learning-before,2fit |
| Presentations | **144=2×18×4**;0 в empty и compatibility |
| Main evaluation | 24=6before+6after на каждое плечо |
| Compatibility readers | 32 толькоB по таблице§5 |
| Existing guard readers | 6=3на плечо |
| Все reader calls | **62**:42evaluation +20public Route, включая2native unit calls |
| Neural SFT/DPO/LoRA/Generate, teacher, bootstrap | **0** |
| Новая production publication / full pipeline | **0** |

После learning ни contracts, ни formatter, ни cold check не получают второй fit. Для параметров oracle берётся historical соответствующего фактора по label names; новый фактический wall/alloc записывается со всеми изменениями, но не обязан воспроизвести −34,13%. Критерии корректного переноса: real native acquisition0→3/6, сохранённые поштучные outcomes и численные learned coordinates, совместимое legacy/v2 restore, неизменный public gate. Отдельно отчёт указывает фактические ресурсные улучшения/ухудшения, не маскируя их общим PASS.

Runner и formatter сохраняют `intent.json`, source/fixture/assembly identities, per-stage TRX/receipt, `comparison.json`, `report.md`, `report-for-user.md`, terminal `status.json`. Технический и человеческий отчёты — **русский**, структура из[03§8](03-test-and-execution-contract.md). До/после обучения, A→B, positive/negative metrics и `not_measured` показаны раздельно. Historical3updates не называется actual новым счётчиком, новый17label run не называется measured historical15label result. `source-provenance.runtime_status=NOT_COMPILED_NOT_RUN` — старая подготовительная метка; completed raw приоритетнее её, raw не исправляется задним числом.

Each process deadline5min, Rebuild15min/stage. Один issued handle, штатное ожидание блоками≤60секунд в том же ходе до terminal/deadline. Не объявлять «процесс пропал» по раннему чтению файлов, не создавать sleep/poll loop, дубликат запуска, automatic retry/cleanup или новый attempt. При существующем COMPLETED читать готовые receipts; formatter failure исправляется чтением тех же raw с отдельной ревизией, не fit. Старые эксперименты и результаты не удалять.

## 7. E2E, модель исполнителя и немедленное переключение

Дополнение к существующему [prompt→domain E2E20](20-prompt-domain-e2e.md#wiki-codec-e2e) сохраняет три темы, прежние Q1–Q6 и learned FlowRef→registered handler. При cold restore используемого Wiki snapshot codec сохраняется вместе с weights/label contract, handler вызывает native gated Route. Исторический untagged B не становится runtime donor; missing gate остаётся deferred без target runner. Локальная панель4TRAIN/6EXAM не заменяет outer selector training и не доказывают полный prompt→domain маршрут. E2E использует ранее сохранённый state, добавляет0новых запросов/тем/Train; положительный локальный fit выполняется только здесь один раз.

Исполнитель — **фактическая модель сессии по умолчанию**, без самовольного override. Обязательные project skills по точным путям `docs/skills/`: `architect-behavior-classifier-SKILL.md`, `intent-to-plan-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `test-failure-triage-SKILL.md`, `bug-to-tests-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-communication-GLOSSARY.md`, `output-readability-gate-SKILL.md`; общая роль/приоритет инструкций —[03](03-test-and-execution-contract.md). Уже подготовленный4/6 набор не требует генерации DPO внешней моделью. Запреты production публикации не переносятся на допустимый micro evaluation доступ, но public gate не обходится.

Назначение исполнителю: только этот FIX-WK и два вызова§6 в canonical root/branch. Сначала Baseline и terminal A receipts, затем назначенный product diff/review/commit, затем Candidate; каждый stage делает Rebuild и проверяет native DLL SHA/MVID. Не создавать worktree/copy, не выполнять checkout/reset/branch switch и не повторять A при сравнении. Common03§9 остаётся владельцем orchestration контракта; собственная матрица WK не запускает другие PlanId.

**STOP/NEXT исполняется немедленно.** При STOP никаких дальнейших действий; при NEXT перейти к точному назначенному ID, не заканчивать прежние code, отчёт, commit, cleanup или «ещё одну проверку». Коротко сообщить только подтверждённое состояние. Запрещён описанный пользователем цикл продолжения прошлой задачи после прямого перехода. Следующий ID читать из[реестра](01-registry.md), а не восстанавливать по старому номеру. Эта карточка не разрешает реализацию невыбранного кандидата или полную цепочку production после одного локального успеха.
