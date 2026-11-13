# Историческое обучение, kNN и преобразование уроков: проверка первичных доказательств

Дата: 20.10.2026. Текущий core: `66947785d1318f7cbbc88118f3156cdc75001e16`. Planning для проверки новизны: `53cbcabd3926cda714bab428d294eb666301e9e4`. Ни новые эксперименты, ни build/test, ни исправления production в этом аудите не запускались. Защищённый каталог `docs/plans/replanning-possible-diffs` прочитан и не изменён.

## 1. Главные выводы и сила доказательств

1. **Успешное обучение памяти существует и подтверждается опубликованными native outputs.** P125 изменяет actual память Executor после восьми уроков и обычные ответы EN→RU: 0/24→19/24. Контроль с переставленными учебными действиями даёт 13/24. Это не «только намерение эксперимента»; это опубликованный результат исполнения, повторно проверенный здесь по raw144 и frozen gold. Успех ограничен переводом коротких команд и новыми сочетаниями знакомых частей. Три ошибочно принятых ответа и neural updates0 показаны рядом. [P125 report][p125-report], [raw144][p125-raw], [gold][p125-gold].
2. **Потеря `phrase_mappings` при загрузке действительно ухудшала обучение на тех же уроках.** Matched before/after:11/24→19/24, композиции 0/8→8/8, но false accepts1→3 и precision11/12→19/22. Это полезный измеренный успех по качеству перевода при ухудшении другой метрики. Исправление reader→builder **уже присутствует** на текущем core; создавать ещё один ремонт этого же ingress не нужно. [Ingress report,3–18,51–59][ingress-report], [reader][tools-reader], [builder][tools-builder], [DTO][tools-dto].
3. **Cold restore сохранил приобретённое поведение:** прежние output/route/confidence/verdict24/24 и state совпали, correct19/24 сохранилось без новых уроков. Это валидная проверка зависимости результата обучения от загрузки; не новое обучение и не улучшение смысла. [P25 report,3–41][cold-report], [full comparison][cold-comparison].
4. В соседней `external_domains` сохранились positive receipts E09/E25/E43 и полезный context readout E26. Однако **E09 tier2 сравнивает разные размеры EXAM**, а таймер E25 вообще не включает Train. Нельзя смешивать эти записи в одно доказательство «ускорилось нейронное обучение» или приписывать любому `passed` универсальное качество. [E09 source][e09-source], [E25 source][e25-source], [E26 source][e26-source], [E43 source][e43-source].
5. Найден отдельный ещё не описанный в 799 checked planning MD и 154 protected cards writer gap: native builder требует `brother's`, а обучаемый verifier запоминает `brother` из-за delimiter splitting. Он вынесен в [VT-APOSTROPHE-01](cards/VT-APOSTROPHE-01.md) с полным source→Train→Verify путём и изолированным A/B. Это **новая гипотеза, не измеренный успех**; её возможные ухудшения на paraphrase/U+2019 включены заранее.

## 2. Что именно проверено сейчас

Здесь различаются три уровня: сохранённый результат прошлого исполнения; исходник соответствующей ветки; новый статический вывод аудита. Исходник на tip ветки не заменяет run-level source manifest. Исторические E-json не содержат полного хеша исполнения/перечня EXAM IDs, поэтому связь каждого старого числа с данным tip не объявляется побайтно воспроизведённой. Отчёты P125/ingress/P25 содержат source/DLL/PDB guards; в этой сессии guards не запускались заново, но actual опубликованные raw и state заново прочитаны и сопоставлены.

Источники закреплены:

| Материал | Ref |
|---|---|
| Текущая реализация и protected receipts | `codex/dpo-implementation@66947785d1318f7cbbc88118f3156cdc75001e16` |
| Исторические E07/E09/E25/E26/E43 source и результаты | `external_domains@3036a01acec938f0b83fa4ffbda26202cc35d639` |
| Сохранённые StructuralKnn/ContextualDictionary | `language_translate@f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` |
| Дополнительно проверенные деревья веток | `local_wiki_planning@219d7ba89dc3ef43e03e17f1461ad96154d3dd7a`; `DesignToCode@8f20ceb8a9bfbadab2ad309226c47dd76e714548` |

Чтение деревьев последних двух веток не означает, что всё их содержимое или все успешные прогоны исследованы. Этот документ не заменяет уже существующие 28+ branch findings: повторно описанные codec, boundary, ignored-k, sibling-cache и routing темы не выданы за новые.

Свежая проверка: [JSON пересчёта primary outputs](evidence/historical-primary-readback.json), [manifest12 первичных файлов](evidence/historical-primary-source-manifest.json), [проверка их Git blob SHA](evidence/historical-primary-snapshot-validation.json). Во всех 12 полученных snapshot bytes восстановлен и проверен authoritative Git blob hash; это проверка чтения source artifacts, не новая тренировка. Один файл comparison оказался LFS pointer; ниже показано, какие выводы независимо восстановлены из raw без него.

## 3. E09 и вопрос «неуспешная матрица → успешная функция kNN»

### 3.1. Что действительно хранится в соседней ветке

| Историческая запись | Primary metric A→B | Что участвует в learning/readout | Ограничение |
|---|---:|---|---|
| E07, tier1,1 May | top1 .1531→.1462 | BPE+AST, затем count-based MultidimensionalNGram | Отрицательная accuracy; таймер A включает BPE и AST preparation, B — нет |
| E09, tier1,30 Apr18:50 | top1 .3368→.3472 | Train memory из 773 пары structural vector→next label, затем Predict | Положительное accuracy, общий 193-transition EXAM выводится по source/notes; time .5→54 ms |
| E09, tier1,30 Apr18:52 | top1 .3368→.3472 | Та же обозначенная конфигурация | Отдельный сохранённый run, time .4→54.6 ms; не усреднять как достаточную статистику |
| E09, tier2,1 May | top1 .2413→.3507 | Structural kNN,4492 TRAIN pairs | A имеет 1123 next-transition оценки, B только 499; эффект+10.94 pp не является matched estimate |
| E25, tier1 | top1 .3438→.875 | native classifier.Train128 synthetic POS sequences, then32val | +53.12 pp по округлённым fields; точные fractions согласуются 11/32→28/32. Таймеры только Predict |
| E26, tier1 | disambiguation .4→1 | fixed default dictionary,15 cases,context distance | Это 6/15→15/15 в fixed reader; Train update в этом run нет |
| E43, tier2 | retention accuracy .3226→.3978 | реальные count updates: new414pairs vs new414+old177replay | matched oldVal93 по source/notes;+7.52 pp округлённо, время .1→5.8 ms вместе с evaluation |

Primary receipts: [E07][e07-result], [E09 tier1a][e09-1a], [tier1b][e09-1b], [tier2][e09-2], [E25][e25-result], [E26][e26-result], [E43][e43-result]. Denominator, не записанный отдельно в JSON, явно помечен выводом из source+notes; сырых per-case verdict для этих E здесь нет. Числа времени — сохранённые наблюдения с разными границами; это не измеренная экономия обучения.

E09 tier2: [baseline loop46–53][e09-denominator] оценивает весь val, а [enhanced65–72][e09-denominator] применяет `val.Take(500)`. При 5617 tokens и 4492 trainPairs получается 4493 TRAIN tokens/1124VAL tokens: baseline1123next transitions, enhanced499. Одинаковая `top1_accuracy` не уравнивает панели. E09 tier1 при 968 tokens/773trainPairs даёт 194 VAL tokens/193transitions, ограничение 500 не срабатывает. Поэтому допустимо сохранить узкий positive tier1 receipt, а больший tier2 uplift обозначить несопоставимым до корректного matched readout. Это не отменяет существование успешного kNN.

Есть также **другой, более ранний** [Markdown E09 run][e09-early], время 30 Apr17:27, с той же округлённой accuracy .3368→.3472 и label `FAILED` по прежнему порогу 5%. Его время 1→74 ms и avgdistance2.1920 отличаются от поздних JSON. Это не тот же raw run. По текущему правилу Любы такой accuracy gain считается успехом этой оси, а замедление остаётся видимым; старую запись и её label не переписывать и не удалять.

### 3.2. Почему lineage не следует выдумывать

В [E07 source15–24,35–95][e07-source] enhanced — `MultidimensionalNGram`, baseline flat bigram; это не эксперимент с Transformer matrix/LoRA. E09 — nearest-neighbor prediction по AST structural vectors. Их соседство и разные результаты подтверждают, что структуру нельзя объявлять бесполезной по одному неудачному механизму. Но **это ещё не exact matched доказательство того самого переноса функции из матрицы в kNN, о котором сообщила Люба**: разные representation/labels/datasets/timing. Для такого утверждения нужен конкретный donor commit/функция и сопоставленные before/after результаты. В этом ограниченном чтении они не установлены; пользовательское свидетельство не объявлено ошибкой.

Код structural kNN действительно не исчез из репозитория: [language_translate StructuralKnn][old-knn] и [current StructuralKnn][current-knn] отличаются только добавленным 5-line license header; то же для [старого dictionary][old-dict] и [текущего dictionary][current-dict]. Это проверено прямым diff двух source файлов. Наличие их в `12.SemanticCompiler` не доказывает, что каждый новый Unit их вызывает; current `27.TranslationDomain` имеет собственные контракты, tokenizer и learned state.

Не выдаём за новую гипотезу уже описанные per-file compact label codec E09, `%200` node kinds, file-boundary next pairs, игнорируемые поля distance, `k` fixed dictionary, sibling traversal или отсутствие wiring. Их будущий перенос зависит от темы: AST role/depth естественны для кода и документации кода; POS/word-order признаки уместны для перевода; наличие файла/узла не универсальная семантика любой задачи.

### 3.3. E25/E26/E43 — разные утверждения

**E25** учит kNN на POS bigrams и классифицирует sentence pattern. `clf.Train(train)` стоит [строкой 23 до обоих таймеров 26 и 33][e25-source]. Поэтому .4→15.4 ms нельзя называть временем обучения; это readout latency. Высокая synthetic classifier accuracy не равна end-to-end переводческой точности. Предметный профиль — порядок слов/тип предложения, и проверять его нужно в языках и конструкциях, где порядок меняет правильный ответ.

**E26** вызывает [BuildDefault17 и два перевода 22–33][e26-source]; примеры 65–92 относятся к русским техническим омонимам (`поток`, `запрос`, `записать`). Это полезное первичное доказательство контекстной функции reader, но не evidence newly acquired state. Новый аналогичный опыт допустим по требованию пользователя только с реальным learned-memory writer и consumer до/после либо с доказанной зависимостью результата уже обученной модели. Тест fixed dictionary сам по себе не занимает очередь learning экспериментов.

**E43** [строит count map из новых данных и из смеси new+replay38–49][e43-source]; обучение реально меняет счётчики и предсказания. Но это не AdamW/LoRA forgetting. `ExtractPairs` [73–81][e43-source] объединяет извлечённые predicates разных файлов до `Zip`; этот известный boundary риск ограничивает перенос результата на code behavior. Metric name retention_accuracy — accuracy на oldVal; `scaling_signal=1.1562` — ratio к прежнему baseOnOld, а не 115.62% абсолютной точности. Старый printed `forgot=+21%` не следует брать как корректный процент без формулы: отчёт source вычисляет абсолютную разность accuracy; raw rounded .344−.3226≈.0214, то есть около 2.14 pp. Нового replay-прогона здесь нет.

## 4. P125: настоящее приобретение переводческой способности

Исходный [report9–13][p125-report] закрепляет run HEAD `7be28a0d24937e3100af8ffa7701518e1022745c`, DLL SHA256 `395995130c266d23a2f6ac1b2da9e90fc4544d64524b5eb8ed38221e85182683`, три fresh machine и initial equivalence. A не обучается; B получает один 8-row batch; C получает те же 8 с переставленными validate/delete targets. Опубликовано **ровно 144** Flow outputs —3 arms×2 phases×24,16 actual lessons всего, neural updates0. Никаких дополнительных скрытых вызовов из пересчёта аудита не добавляется.

| Arm | Before correct/24 | After correct/24 | After answered/24 | After accepted wrong |
|---|---:|---:|---:|---:|
| A no Train |0|0|0|0|
| B8correct lessons |0|19|22|3|
| C8swapped lessons |0|13|22|9|

Пересчёт из raw144 по [frozen accepted phrases][p125-gold] совпал со всеми 6`bucket=all` rows опубликованного [score-summary][p125-summary]. Поля internal trainer `accuracyBefore=accuracyAfter=0` относятся к native unit coverage, не к external19/24; смешивать эти метрики нельзя. Источник успешных составных примеров — synthetic action/object fixture, не новая независимая предметная выборка. [Report19–29,45–56][p125-report].

Реальное состояние заново прочитано из [A_initial][p125-initial] и [B_trained][p125-state]: exact0→8, phrase0→16, sharedKNN0→24, word/surface0→0, training identities0→8, history0→1, version0.1.0→0.1.1, readinessfalse→true. Это реальные serialized learner artifacts; не заглушка returned accuracy. Intervention одновременно меняет memory/readiness/version/history, поэтому чистый вклад каждого KNN scalar не разделён.

Обычный consumer после Train отвечает не только учебным 8: near3/3 и новые комбинации 8/8. Edge5/5 правильными не стали: cc01 теряет отрицание, cc04 превращает вопрос в приказ; cc03 `проверь файл кэш` сохраняет оба объекта, но неоднозначен и не проходит fixed strict rubric. Нельзя описывать cc03 как доказанную потерю второго объекта. Quantity и unknown-object остаются bootstrap. [Все edge ответы 31–41][p125-report].

Опубликованное [независимое review][p125-review] допускает эти выводы с ограничениями и описывает отдельный прошлый repeat. Этот audit проверил уже сохранённое, а не исполнил ещё один repeat. Известные успешные результаты можно документировать как кандидаты в улучшения; не требуется запускать их снова ради подтверждения уже видимого успеха.

## 5. Ingress: одно dropped field объясняет конкретные 8 дополнительных правильных ответов

В сравнении оба свежих экземпляра получили одинаковые 8 уроков и одинаковые 24 source inputs. До исправления reader не сохранял заданные 16 phrase mappings, builder не передавал их в ExecutorOutput. После исправления меняется доставка учебных данных, а native Domain DLL, executor lookup, verifier и Flow сохраняются. Реальная цепь: `ReadTranslationCorpusRowsAsync` → `BuildTranslationContinueExample` через reflection → `TrainStructuredBatchAsync` → ordinary `RunAsync`. Полный CLI registry/manifest не выполнялся; RP01 ResearchCheckpointEnRuAdapter — отдельный путь. [Report51–59][ingress-report].

Свежий пересчёт [before raw24][ingress-before] и [after raw24][ingress-after] дал:

| Метрика | До после тех же 8 уроков | После после тех же 8 уроков | Delta |
|---|---:|---:|---:|
| Correct / all |11/24|19/24|+8 ответов / +33.33 pp|
| Novel action/object combinations |0/8|8/8|+8|
| Exact TRAIN recall |8/8|8/8|0|
| Near article variants |3/3|3/3|0|
| Strict edge correct |0/5|0/5|0|
| Answered |12/24|22/24|+10|
| Accepted wrong |1|3|+2, ухудшение|
| Correct / answered |11/12=91.67%|19/22=86.36%|−5.30 pp|

Идентификаторы и source inputs во всех 24 соответствующих строках совпали. `comparison.json` в GitHub snapshot — **Git LFS pointer**, oid `sha256:d739be03eea3c79985898af3e1134200ec2e0fa348c25aec690d53346bc8dbd2`, size8632. Его payload не получен; приведённые counts независимо восстановлены из полностью доступных before/after raw и frozen P125 gold, а не взяты на доверии из недоступного файла. Это ограничение provenance честно записано в [receipt](evidence/historical-primary-readback.json).

На текущем core reader [4465][tools-reader] читает `phrase_mappings`, builder [4687][tools-builder] передаёт `row.PhraseMappings`, DTO [5146][tools-dto] содержит поле. Следовательно, новый action — документировать successful existing fix/границы и использовать его как baseline, а не вновь предлагать тот же dropped-field repair.

Измерения batch38.5685→34.5068 ms, flow291.0535→52.9254 ms, RAM и bytes состояния в старом report не превращаются здесь в causal speed/peak claim: по одному A→B, JIT и cold effects, sampler5/2samples, разная полнота процесса. [Report61–77][ingress-report]. Успех уже есть по правильным переводам; обосновывать его недоказанным ускорением не нужно.

## 6. Cold reload: результат обучения зависит от reader state

P25 сравнивает сохранённые P125 B_after outputs с новым native process после `TranslationMachine.LoadCheckpointAsync`, а не с заново обученной моделью. Load1,ordinary Flow24,Train0. В [full output-comparison][cold-comparison] присутствуют оба payload каждого 24 case; здесь проверено не только поле `exactFieldsEqual`, но и сами product fields после исключения arm/phase, elapsedMs, new allocatedBytes и summaryId. Все 24 equal; retained payload совпал с оригинальным P125 raw. [Report9–29][cold-report].

Дополнительно сравнили реальные [B_trained][p125-state] и [cold after state][cold-state], исключив **только exported_at_utc**: структуры, массивы, все values равны. Exact8/phrase16/KNN24/identities8/history1 сохранены; correct19/24,answered22/24 и те же 3 wrong сохраняются. Это положительное восстановление learned ability, не дополнительное смысловое улучшение. Cold означает новый process/owner, не гарантированно пустой OS file cache.

Ограничения не скрыты: word/surface maps пусты, поэтому их nonempty import filtering не испытан; schema hash-rejection не встроен и не проверен; neural weights, другие языки, other snapshots и другие предметные семьи не доказаны. [Report31–41][cold-report], [independent review][cold-review]. Проверка валидна по требованию пользователя, поскольку reader восстанавливает actual acquired memory, непосредственно используемую обычным Flow. Generic roundtrip произвольного DTO без такого consumer не был бы достаточен.

## 7. Новый causal candidate: VT-APOSTROPHE-01

Проверка полных 799 planning MD и 155 MD cards/index вернула только близкие source-contraction/marker/conceptParser темы. [Новизна](evidence/verifier-apostrophe-novelty.json). Новый фактор отличается от них: `BuildGeneralBatch` сохраняет в issue полный target token с ASCII апострофом, `TryParseRequiredTerm` теряет суффикс при actual learning update, `Verify` читает обрезанное значение. В [карточке](cards/VT-APOSTROPHE-01.md) закреплены source refs,2 TRAIN,8 matching EXAM, disjoint native markers и единственная patch.

Ширина результата заранее ограничена: B может правильно отвергать `brother book`, но ошибочно отклонять `book of your brother` и Unicode `brother’s`. Предсказанная из кода общая accuracy может остаться 6/8 при улучшении falseaccept2/3→0/3 и ухудшении falsereject0/5→2/5. **Это не измерения.** Исполнитель не должен менять gold или подбирать новый corpus для достижения net accuracy gain. Любое реально измеренное улучшение полезной оси признаётся успехом этой оси, рядом остаются потери.

Карточка и [prompt](prompts/VT-APOSTROPHE-01.md) определяют fixed Windows source/host/output paths, current default executor model, навыки и русский report, no-overwrite guard, native closure, one build/A/B, exact counts, жёсткую остановку/переход и запрет переносить в production до решения Любы. Source closure проверена чтением, не компиляцией; host ещё не материализован. Приоритет — active training очередь, затем другие задачи, не новая серия инфраструктурных проверок ради самих проверок.

## 8. Что не считается найденным или выполненным

- Не утверждается exact failed-matrix→successful-KNN donor lineage, если нет соответствующего matched receipt; полезные branch proofs при этом сохранены.
- Не утверждается, что все исторические learning успехи потеряны или что все E-коды означают native neural training.
- Новый apostrophe repair не выполнен, его anticipated counts не внесены в measured results.
- No new learning/build/tests, no protected-folder edits, no production changes; этот пакет добавляет evidence и кандидат, не удаляет и не переписывает прежние исследования.

[p125-report]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md
[p125-raw]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/raw-outputs.json
[p125-gold]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/panel-gold.json
[p125-summary]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/score-summary.json
[p125-initial]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/states/A_initial.json
[p125-state]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/states/B_trained.json
[p125-review]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/independent-review/review.md
[ingress-report]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md
[ingress-before]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/before/raw-outputs.json
[ingress-after]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/after/raw-outputs.json
[tools-reader]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain.Tools/Program.cs#L4465
[tools-builder]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain.Tools/Program.cs#L4687
[tools-dto]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain.Tools/Program.cs#L5146
[cold-report]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md
[cold-comparison]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/output-comparison.json
[cold-state]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/states/after.json
[cold-review]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/independent-review/review.md
[e07-source]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/Experiments/E07StructuralContextToken.cs#L15-L95
[e09-source]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L26-L119
[e09-denominator]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L45-L73
[e25-source]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/Experiments/E25PatternClassifier.cs#L15-L43
[e26-source]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/Experiments/E26ContextualDictionary.cs#L15-L92
[e43-source]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs#L18-L104
[e07-result]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/results/E07-STRUCTURAL-CONTEXT-TOKEN/results/run-2026-05-01-001.json
[e09-1a]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-04-30-001.json
[e09-1b]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-04-30-002.json
[e09-2]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json
[e09-early]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/experiments-v2/E09-STRUCTURAL-KNN/results/run-2026-04-30-001.md
[e25-result]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json
[e26-result]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-001.json
[e43-result]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json
[old-knn]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Prediction/StructuralKnn.cs
[current-knn]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/12.SemanticCompiler/Prediction/StructuralKnn.cs
[old-dict]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Translation/ContextualDictionary.cs
[current-dict]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/12.SemanticCompiler/Translation/ContextualDictionary.cs
