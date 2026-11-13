# Финальная сверка перед публикацией: закрытые замечания и сохранённая новизна

**Главное:** на последнем проверенном core N-KU-01 уже зарегистрирован кандидатом, LF-03 fixture/бюджет описаны, а две наши новые гипотезы не дублируются поздними карточками. Исходная матрица 130 остаётся историческим доказательством своего pin; текущий адресный инвентарь — 135. Все сведения ниже получены чтением документов и исходных результатов. Новых экспериментов, сборок, Train/Predict/Generate и изменений protected или продукта нет.

## Последняя сверка перед публикацией: core ab583032

**Срез:** 20 октября 2026; `67abfabe6def8a002984be80b999824601ad273c → ab583032562d6656de9dd5ca6fa0a325fd4b1f30`. Это адресное документное дополнение к аудиту на `9622b575` и к [первому позднему обновлению](late-core-update.md). Оно **не заменяет** исходную матрицу 130 карточек и не объявляет повторную семантическую проверку всей библиотеки.

[Сравнение коммитов](https://github.com/lubochka/llm_mvp_core/compare/67abfabe6def8a002984be80b999824601ad273c...ab583032562d6656de9dd5ca6fa0a325fd4b1f30) содержит один коммит, 157 изменённых файлов, все внутри `docs/plans/replanning-possible-diffs/`. Пять карточек добавлены. В каталоге результатов изменены только N-KU-01 `report.md` и `commands.md`; сохранённые raw A/B, gold, reverse-order raw, код runner и production-файлы этим коммитом не изменены. Сборок, обучения, новых измерений и изменения remote-файлов в нашей сверке не было.

API compare не выводит полный patch каждого большого документа: нулевые patch-статистики отдельных поздних элементов не использованы как доказательство отсутствия изменений. Полные тексты десяти выбранных файлов прочитаны непосредственно через Git blobs, перечисленные ниже.

### 1. LF-03: ранее обнаруженный конфликт данных и бюджета закрыт в карточке

В последней N62 теперь есть самостоятельный раздел LF-03. Он задаёт прежнюю существующую fixture `fixtures/csharp-one-pair.jsonl`, требует копировать исходные bytes без пересериализации и запрещает добавлять общий subtraction-prefix. Приведены ID `simulation-csharp-doc-f06-008`, точные prompt/chosen/rejected и сохранение `simulation` / `csharp_runtime_verified=false`. Это тот же существующий опыт, для которого предыдущий аудит подготовил документное разрешение конфликта; новый самостоятельный эксперимент здесь не изобретён. [N62, строки 297–311](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L297-L311).

Бюджет теперь однозначен на уровне специального раздела: по одному `Inspect(..., expectedCount:1)`, одному trainer и одному настоящему `StepVector/ApplyOptimizer` в A и B. Всего — 2 предъявления одной общей пары, 2 optimizer calls, 4 policy forward, 4 reference forward и 4 policy backward; `Evaluate/Generate/teacher/warmup/retry=0`. A1/B1 относятся к первому committed update. Указана реальная цепь `StepVectorCore → BackwardLogProbs → logits.Backward → Tensor.Backward → autograd → ApplyOptimizer`; предполагаемые байты не выданы за измеренный RAM. [N62, строки 313–319](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L313-L319).

Общая таблица двух уроков теперь **прямо ограничена R1/R2/E-OPT-STEP**, а общий абзац о конечном состоянии отдельно уточняет LF-03 — после первого шага. Поэтому старое замечание «нет fixture; общий бюджет требует два updates и второй A1» больше не является текущим дефектом. Общие примеры про D-T1/D-T2, update2 и два updates читаются с явным специальным исключением LF-03; добавлять второй update из этих примеров нельзя. [N62, строки 321–337](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L321-L337).

**Что остаётся ограничением:** этот документ задаёт recipe, но не материализует и не компилирует host. В нём прямо сказано, что source-копии, JSONL, manifests и outputs не созданы; binary ABI, custody/checkpoints и runtime не проверены. Правильный статус — спецификация подготовлена, исполнение не выполнено, наличие всех физических входов не доказано. Снять прежний конфликт fixture не означает объявить опыт уже собранным или измеренным. [N62, строки 372–376](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L372-L376).

**Оставшийся конфликт с текущим поручением пользователя:** общий command-template N62 по-прежнему направляет `TaskResultRoot` внутрь запрещённой для наших изменений `docs/plans/replanning-possible-diffs`. Он не даёт разрешения писать туда в этой задаче. Сохраняется готовое документное разрешение из предыдущего дополнения: для будущего отдельного разрешённого LF-03 использовать назначенный экспериментальный output-root `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LF-03/results/2026-10-20/LF-03-01/`, отдельные A/B, literal allowlist и create-new в ещё не созданном экспериментальном host, все его native checkpoints/receipts там же. Это спецификация местоположения нового host; существующие проверки доступа не обходятся. В нынешнем документном проходе эти команды не запускаются. [Текущий шаблон N62, строки 220–236](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L220-L236).

### 2. N-KU-01: кандидат теперь зарегистрирован в самом core

На новом SHA уже существует раздел `required-changes.md#n-ku-01` со статусом **«кандидат переноса; основной код не изменён этим опытом; решение о переносе отдельно»**. Записано точное исправление: `ActionLabelGenerator.Label`, проверка `hasLoop`, добавить `SyntaxKind.DoStatement`. Изменение FeatureVector/TargetKinds, размерности и k в кандидат не включено. Поэтому старое указание «в защищённом реестре кандидат ещё отсутствует» теперь историческое, а не описание текущего HEAD. Регистрацию внёс полученный внешний коммит; наш аудит защищённую папку не менял. [Реестр, строки 21–32](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/required-changes.md#L21-L32).

| Показатель, C# file-action KNN | A | B | Вывод |
|---|---:|---:|---|
| Ответ удовлетворяет gold после обучения | 1/4 | 3/4 | +50 п.п. на этой панели |
| Ложный совет ADD_LOOP при уже имеющемся цикле | 3/3 | 0/3 | Измеренный полезный успех |
| Нужный ADD_LOOP на KQ4 без цикла | 1/1 | 0/1 | Регрессия рядом с успехом |
| До обучения | 0/4 | 0/4 | Оба начальных состояния без обученной памяти |

Эти числа сохранены в новом отчёте; речь о прежнем измерении, не о новом запуске. Формулировка gold «не добавлять лишний цикл» не означает, что любой другой совет семантически оптимален. В отчёте отдельно сохранено ограничение: при k=3 и распределении меток 3:1 итог задаётся большинством; структурное обобщение не доказано. Это не отменяет измеренного уменьшения ложных советов. [Отчёт, строки 13–23](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/report.md#L13-L23), [реестр, строки 34–46](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/required-changes.md#L34-L46).

Реестр также теперь показывает реальные дополнительные операции поверх основного бюджета: warmup, A-only equivalence, диагностический пересчёт расстояний и существующий обратный порядок. Однократное время 195,8 → 123,8 мс не объявлено доказанным эффектом исправления. Назначенный независимый reviewer в среде исходного исполнителя по-прежнему не выполнил review; эта запись не равна независимому повторному запуску нами. [Реестр, строки 44–55](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/required-changes.md#L44-L55).

`report.md` ссылается на зарегистрированного кандидата, а `commands.md` сохраняет журнал документного уточнения без повторной генерации данных. [Отчёт, строки 258–267](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/report.md#L258-L267), [журнал, строки 47–52](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/commands.md#L47-L52). Фраза в конце отчёта «локальные правки; исполнитель их не коммитил» относится к действию исходного исполнителя; при этой сверке текст уже находится в опубликованном Git-коммите ab583032.

**Оставшаяся причинная оговорка в реестре.** Фраза «Пропуск нужного цикла связан и с признаками» в [строке 46](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/required-changes.md#L46) не доказана этим A/B как отдельная причинная связь. Наблюдаемая близость файла без цикла к уроку с do-while описывает расстояния; при трёх одинаковых метках из четырёх и k=3 большинство остаётся тем же для любого набора трёх соседей. Поэтому вклад отсутствующего признака в ошибку KQ4 пока гипотеза для отдельного опыта, а не установленная причина данной ошибки. Это уточнение не отменяет измеренный успех исправленной учебной метки и зарегистрированный статус кандидата.

### 3. Текущий инвентарь: 135 карточек; исходный аудит 130 не переписан

Пять новых файлов повышают инвентарное количество `experimets/cards/*.md` с 130 до **135**. Это добавление пяти документов, а не пять доказанных успешных экспериментов. Прочитаны заголовки, объявленная область, состояния и структура новых карточек; полный независимый аудит всех их data/host/source/metrics/contracts в данном срочном дополнении **не выполнен**.

| Добавленная карточка | Заявленный учебный механизм | Граница этой сверки |
|---|---|---|
| [BPE-NO-MATCH-COPY-01](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/BPE-NO-MATCH-COPY-01.md#L1-L17) | Копии chunk в настоящем обучении BPE | Наличие и назначение; не runtime PASS |
| [N60-E1](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/N60-e1-verifier-admission.md#L1-L5) | Admission отрицательного урока в Units / QualityVerifierUnit; заявлены 2 batch, 11 Unit.TrainOnExample, из них 3 Verifier, 16 Verify | Заявленный бюджет прочитан, фактическое исполнение не проверено |
| [NEURAL-FROZEN-TRANSPOSE-01](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/NEURAL-FROZEN-TRANSPOSE-01.md#L1-L17) | Стоимость transpose frozen matrices при SFT/LoRA | Наличие и назначение; не runtime PASS |
| [WIKI-MARGIN-01](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-MARGIN-01.md#L1-L23) | Случай correct argmax без создания обученного состояния | Наличие и назначение; не runtime PASS |
| [WIKI-TAXONOMY-01](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-TAXONOMY-01.md#L1-L23) | Новые доменные маршруты, ошибочно получающие отрицательный учебный класс | Наличие и назначение; не runtime PASS |

Файл `coverage.json` и основная полная таблица остаются доказательством именно снимка `9622b575`: 130 карточек, 24 small-документа. Числа 470 под-сравнений и 993 профиля относятся к тому же исходному набору; для нынешних 135 они здесь не пересчитаны. Нельзя складывать присутствие нового ID с семантической полнотой или утверждать, что все недостающие механизмы закрыты пятью новыми файлами.

### 4. Прочитанные полные Git blobs

| Файл | Git blob SHA-1 |
|---|---|
| N62-neural-update-costs.md | `b3d4174c2348f2bd07dbb1912d7915d82725c340` |
| N-KU-01-do-loop-label.md | `b36fd81d84e0dbc640fd3bfd1407f93fec922ab0` |
| required-changes.md | `43076842894facdc70827b337966190d6d233e58` |
| N-KU-01/report.md | `f748005cc70563d4c64b044a138886491fa46272` |
| N-KU-01/commands.md | `c2d50fc3996c181b7d8562cb448967a9bd9a89de` |
| BPE-NO-MATCH-COPY-01.md | `e354191b4715aee0a088886bf5429853fa075426` |
| N60-e1-verifier-admission.md | `5aa9b4c267a289e580bcb61a00a1e11c0a8f4dfb` |
| NEURAL-FROZEN-TRANSPOSE-01.md | `12dbcca1c3a389bff57037fe750d0ba989335e28` |
| WIKI-MARGIN-01.md | `a537888b752376c9ac20076151abcabe73294a02` |
| WIKI-TAXONOMY-01.md | `d17ffc1634f036bb6b243009c460808b20af6598` |

Получены полные тексты указанных blobs. Затем 10/10 файлов сохранены с точными исходными bytes и проверены вычислением Git SHA-1 до записи и после чтения; суммарно 384 266 bytes. [Манифест последнего среза](final-source-byte-check.json) содержит SHA-256, Git SHA-1, размеры и число строк. Эта адресная проверка не меняет исходный byte-verified coverage на `9622b575`.



## Дополнительная сверка позднего planning-пакета

**Planning pin:** `f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9`. Проверен только новый каталог `possibleDifs/2026-10-20-learning-ingress-and-retention-review`: по Git tree в нём **30 файлов, из них 16 Markdown**. Все 16 Markdown получены и включены в адресный поиск. Другие 14 файлов и заново весь repository в этой сверке не исследовались. Прежние 662+296 ledgers остаются свидетельством своих immutable pins; они не переписываются.

**Итог: новые документы не дублируют KH-COST-01 или точный sequential сценарий KH-EXT-01.** KH-COST сохраняет ограниченную новизну по известному корпусу с этим поздним дополнением. KH-EXT по-прежнему является уточнением известного LT-N02, а не совершенно новой фундаментальной гипотезой.

### Что проверено поиском

- `FileActionKnn|ActionLabelGenerator|TrainFileActionKnn|KH-COST`: **0 совпавших строк**.
- `MethodKnn|TrainMethodKnn|_pairs.Clear|KH-EXT`: **1 строка** — [P83 expression-bodied input](https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-ingress-and-retention-review/experiments/03-CARD-BY-CARD-AUDIT.md#L102), другой механизм extractor/target.
- `ParseText`: **0**; MethodKnn рядом с Clear/retention/memory/память/стирание/потеря: **0**.
- File-level AND точного FileAction owner с reuse/double/duplicate/twice/повторный/двойной parse: **0 документов**. Этот дополнительный запрос учитывает описания на разных строках.

### Близкие темы — разные владельцы и вмешательства

| Поздняя тема | Что описано в новом документе | Соотношение с нашими KH |
|---|---|---|
| INGRESS-LENGTH-01 | Полный `.cs` source становится supervised response; одна overlength запись вызывает ранний возврат всего IncrementalTrainer до DPO/SFT. Предложено явно разделить допустимые и отложенные строки. Статус source-confirmed / эффект ещё не измерен. [Описание L9–12,14–32][ingress] | **Другой механизм.** KH-COST сохраняет все те же training rows и устраняет повторный AST parse перед FileAction memory Train. KH-EXT проверяет destructive replacement уже выученных method-pairs; здесь потеря возможности обновиться до mutation в neural pipeline |
| N63-AR-CAP | Общий FIFO ReplayBuffer плюс same-domain Sample может вытеснить историю A при A→B→A; затем меняется состав настоящих supervised updates. Не установлен общий нетестовый caller; это условный public API scenario. [L7–35][replay] | **Смежная retention тема, точного дубликата нет.** В KH-EXT уничтожается сама память next-method predictor при sparse replacement, а не replay-уроки для последующего LoRA/SFT. Наш фактор — eligible-pair admission до Clear; N63 меняет eviction/reservation при общей capacity |
| CORRECTION-COLLISION-01 | При точном domain/prompt/rejected=response совпадении исправленный feedback остаётся в cache, но последующий SFT берёт stale indexed target. Сохраняется DPO→SFT, меняется происхождение target; объявлено refinement ND-DPO-SFT/P87. [L3–11,17–24][collision] | **Другой механизм.** Он не строит два AST для FileAction и не очищает MethodKnn pairs. Общая связь с обучением не делает владельцев или причинные факторы одинаковыми |

Дополнение само сохраняет E26 как context-only (`BuildDefault`, без усвоения нового урока), а E25/E43 — с их настоящими владельцами и ограничениями. Это согласуется с нашим dossier; повторную находку этих исторических результатов новой не считаем. [Историческая таблица L43–50][replay]

**Граница проверки:** факты в таблице — содержание вновь полученных документов. Их ссылки на новый core `ce890a48...` здесь заново не открывались и не выдаются за наш независимый повторный source audit. Новых parser/Train/Predict/build действий нет. Точные карточки KH и существующие отчёты не изменялись. Поздний пакет добавляет самостоятельные направления покрытия; его нельзя считать ни доказанным новым успехом обучения, ни причиной автоматически снять KH-COST/KH-EXT с рассмотрения.

[ingress]: https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-ingress-and-retention-review/ingress/01-DATA-INGRESS-FINDINGS-AND-COVERAGE.md#L9-L32
[replay]: https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-ingress-and-retention-review/replay/01-NEW-REPLAY-CAPACITY-AND-HISTORICAL-EVIDENCE.md#L7-L50
[collision]: https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-ingress-and-retention-review/ingress/03-CORRECTION-COLLISION-REFINEMENT.md#L3-L24


## Публикационная сверка новизны GRADB-LOCALITY

**Вывод:** пять новых карточек и последнее дополнение N62 **не дублируют GRADB-LOCALITY-ENRU-01**. У них другие изменяемые функции и другие факторы. Статус GRADB остаётся **новая неподтверждённая гипотеза стоимости реального обучения**, без measured speedup и без заявленного объяснения предметной деградации.

### Область этой проверки

Проверен адресный документный delta на core HEAD `ab583032562d6656de9dd5ca6fa0a325fd4b1f30`: пять новых карточек плюс текущий `N62-neural-update-costs.md`. Это **дополнение** к прежнему full-text search по 662 planning / 296 protected Markdown на прежних pins; базовые corpus claims не переписаны. Полный manual semantic review всего нового HEAD не заявляется.

Все шесть файлов получены **полностью через Git blob URL**, а не search snippets. Декодированные UTF-8 bytes сохранены без `apply_patch`-перезаписи содержимого; для каждого вычислен Git SHA-1 `blob <length>\0<bytes>` и проверено совпадение с blob SHA API. Общий объём — 239 844 bytes. Идентификаторы этих шести исходников приведены в таблице полных Git blobs выше; локальные служебные кэши не входят в пользовательский пакет. Source implementation, binary ABI и runtime этим дополнением повторно не проверялись.

### Разграничение механизмов

| Новая/обновлённая карточка | Реальный фактор её рецепта | Почему это не GRADB-LOCALITY |
|---|---|---|
| [BPE-NO-MATCH-COPY-01:9–23][bpe] | В native `BpeTokenizer.Train → MergeChunk` откладывает создание/копирование списка до первого merge; если пары нет, возвращает существующий локальный chunk | Копирование токенов и обучение vocabulary, без MatMul backward |
| [N60-E1:30–42][verifier] | Допуск отрицательного verifier-урока: не добавлять совпавший с chosen rejected candidate в `TranslationStructuredBatchBuilder`; затем настоящий Unit Train/Verify | Меняет состав уроков дискретной памяти QualityVerifier, не TensorCore kernel |
| [NEURAL-FROZEN-TRANSPOSE-01:9–15][transpose] | Кэширует transpose **замороженного Linear.Weight** внутри `Linear.Forward`; trainable LoRA A/B продолжают пересчитываться | Forward-кэш frozen weights, а GRADB меняет только обход скалярных операций **правого trainable adjoint**. Общность native SFT/LoRA цепи не делает факторы одинаковыми |
| [WIKI-MARGIN-01:9–21][margin] | Изменяет условие native router update: правильный argmax с недостаточным margin тоже получает update против runner-up | Меняет правило обучения sparse feature classifier и его weights/bias; не MatMul loops |
| [WIKI-TAXONOMY-01:9–21][taxonomy] | Расширяет согласованное пространство output labels и decoder 15→17 | Изменение пространства учебных классов/чтения результата; не локальность численного backward |
| [N62 LF-03:297–319][n62-lf] | Удаляет только внешний `initialGradient.Data.ToArray()` в `Tensor.Backward(Tensor)`, сохраняя защитную копию в TensorStorage | Удаление дублирующего копирования начального градиента **перед** autograd. GRADB оставляет Tensor.Backward/TensorStorage неизменными и переставляет внутренние циклы `AᵀG` |
| [Другие N62:238–295][n62-other] | R1 — frozen state comparison; R2 — local softmax buffer; E-OPT-STEP — чтение step без копии m/v | Все три уже известны и исключены из GRADB-factor; ни один не меняет циклы правого MatMul gradient |

В целевых шести полных текстах запрос `gradB|AᵀG|right.operand|loop.interchange|row…inner…column|прав…операнд` дал **0 совпадений**. Вывод опирается также на чтение указанных причинных разделов: один нулевой keyword search не доказывал бы отсутствие смыслового дубля.

### Публикационное решение

Сохранить GRADB-карточку отдельно. Не переименовывать LF-03 или frozen-transpose в нашу находку и не соединять их оптимизации с GRADB в одном A/B. В будущей GRADB-проверке forward/cache, стартовое копирование gradient, softmax buffers, frozen validation и optimizer metadata остаются одинаковыми; меняется только `TensorOps.MatMul` gradB loop order `inner → column → row` на `row → inner → column` с reuse одного A scalar.

Новые карточки — рецепты, а не доказательство выполненных успешных запусков. Их появление не делает GRADB измеренным результатом и не разрешает смешать предметные домены или предъявить теоретические bytes как RAM. В этой сверке **0 Train/Step/Generate/compile**, защищённые документы и product code не менялись.

[bpe]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/BPE-NO-MATCH-COPY-01.md#L9-L23
[verifier]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/N60-e1-verifier-admission.md#L30-L42
[transpose]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/NEURAL-FROZEN-TRANSPOSE-01.md#L9-L15
[margin]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-MARGIN-01.md#L9-L21
[taxonomy]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-TAXONOMY-01.md#L9-L21
[n62-lf]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L297-L319
[n62-other]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L238-L295

