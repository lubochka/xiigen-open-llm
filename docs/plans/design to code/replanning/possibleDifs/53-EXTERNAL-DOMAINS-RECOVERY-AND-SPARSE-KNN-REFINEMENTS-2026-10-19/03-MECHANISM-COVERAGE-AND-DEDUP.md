# Покрытие механизмов и отличие от предыдущих исследований

Дата: 19.10.2026. Метод: независимая инвентаризация и полнотекстовая сверка прежних исследований, затем предметное чтение ближайших совпадений. Core использован только для чтения; результаты старых proxy не перезапускались.

## 1. Главный результат

Три рассмотренных новых кандидата не следует представлять как три доказанные причины регрессии RP01.

- **NEW-AGE — новая условная гипотеза о механизме:** один global AdamW step при разрешённых sparse moments и поздней первой активации параметра. В полном просмотренном корпусе не найдено прежнего утверждения именно про возраст обновлений отдельного параметра.
- **Signed score mass в KnnSkillFilter — новое уточнение известного фильтра:** все IDs остаются eligible; меняется знак/сумма scores и решение confidence gate. Это отличается от уже известного global-topK→empty-intersection.
- **Насыщение используемого PositionInParent — новое уточнение известного AST codec:** прежние документы уже знают о насыщении SiblingCount 255, но это поле не использовалось distance. Отдельная потеря различия в реально используемой позиции требует собственного контрпримера и caller.

Условный механизм означает: исходный код и ограниченный численный пример могут быть подтверждены, но trigger и выигрыш на обычном получателе ещё надо установить. Унаследованное old/current поведение само по себе не объясняет более позднюю регрессию.

## 2. Что именно получено и проверено

| Источник | Точный Git SHA | Инвентаризация | Полностью скачанные Markdown | Проверка содержимого |
|---|---|---:|---:|---|
| xiigen-planning / possibleDifs, основной срез | a90350f33955f9c4b15707a4619eeeba5d42e863 | 428 верхних entries; 564 рекурсивных файла | 389; 6,423,126 bytes | 389/389 Git blob SHA совпали |
| llm_mvp_core / docs/plans/replanning-possible-diffs, включая experimets | 44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76 | 93 верхних entries; 1081 рекурсивный файл | 272; 6,850,527 bytes | 272/272 Git blob SHA совпали |
| Planning delta основного среза→актуальный срез | 0715844069a0c9b0242958c663464f552614ad84 | 9 commits; 9 изменённых путей: 8 добавлены, 1 изменён | 8 Markdown bodies: 7 новых, 1 новая версия старого | 9/9 файлов, включая один Python, Git blob SHA совпали |

До первой дельты 071 полнотекстовый поиск охватил **669 версий Markdown-тел**, представляющих **668 различных Markdown-путей**. Дополнительная дельта до `02532653562a5f1e27fd75b1b03d4ae23e930e2f` включает 7 Markdown-версий: 3 новых пути и 4 изменённых. Итог — **676 версий / 671 различный актуальный Markdown-путь**. Это количество документов, а не количество независимых гипотез. В core найдено **125 карточек P01–P125**. Non-Markdown пути инвентаризированы; они не скачивались массово и не запускались. Исключение — один новый Python-файл дельты52: скачан для точного контроля дельты, не выполнен.

Машинные доказательства: [реестр точных файлов](evidence/dedup-corpus-manifest.json), [результаты полнотекстовых запросов](evidence/dedup-search-receipt.json), [сводка проверки](evidence/dedup-verification.json). Реестр содержит исходные пути, commit, Git blob SHA и число байтов; повторные версии сохранены отдельно. Полные тексты прежних документов не дублируются в новом пакете.

Точные байты полученных копий сверены с Git blob SHA; техническая добавочная последняя LF при локальном сохранении удалялась только после совпадения ожидаемого хэша. Три core-файла со смешанными исходными переводами строки переписаны точно из полученного тела; итоговых несовпадений нет. Это проверка транспортного снимка, а не проверка корректности утверждений внутри файла.

### Предел прочтения

Все тела получены и доступны полнотекстовому поиску; основная matrix, claim ledger и именованные кандидатные разделы прочитаны содержательно. Это **не** независимое повторное исследование каждого первичного C# файла, каждого JSON receipt или каждого утверждения всех 676 версий текстов. Некоторые документы ссылаются на Windows paths, более ранние SHA и старые assistant conclusions; такие ссылки сохранены как указатели, а не автоматически приняты как доказательства.

Новизна ниже означает отсутствие совпавшего механизма/условия в названном проверенном корпусе после тематического поиска и чтения ближайших мест. Она не означает глобального доказательства «никто никогда не думал об этом» и не распространяется на новые commits после 02532653562a5f1e27fd75b1b03d4ae23e930e2f.

## 3. Нумерация и актуальные указатели

Основной README на a90350f33955f9c4b15707a4619eeeba5d42e863 начинает пакет50 feedback/BPE, затем содержит51 SQL/KNN/tensor и несколько приоритетных глав external_domains/local_wiki/language_translate. Этот README не менялся в просмотренной девятикоммитной дельте. Верхний номер основного среза — **51**; есть **три разных каталога49**, поэтому число в имени не является уникальным ID находки.

После дельты 0715844069a0c9b0242958c663464f552614ad84 максимальный опубликованный номер — **52**. Новый52 охватывает orchestrator-extensions: v1/v2 trained-state admission, IsLearned/default trainer, signed snapshot restore, первые10 records в continuation probe, ObservationCount вместо delta Weight и границы исторических DPO batches. В трёх наших условиях совпадающий trigger в этой дельте не найден. Номер следующего пакета перед публикацией надо сверить с GitHub ещё раз; этот отчёт ничего не резервирует.

Исходный core [00-README](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/00-README.md) содержит ссылку на84-check-source-coverage.md, которого нет в инвентаризации данного среза. Это факт полноты навигации, не новый дефект алгоритма обучения.

[Основная matrix P](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/02-finding-domain-matrix.md) и [реестр источников/claim ledger](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/04-source-claim-ledger.md) — ближайшие общие указатели дедупликации. Их слово «источник» не превращает пересказ helper в свежую первичную проверку.

Дополнительная заключительная дельта 071→025: 12 commits, 9 изменённых путей, 7 Markdown body versions и 2 probe files; 13 head/base blobs проверены точно. Все 7 новых/изменённых Markdown прочитаны, пять тематических запросов повторены. Прямых дублей трёх новых условий нет. README получил 13 строк в начале; остальной текст сохранён. [Отчёт](evidence/dedup-delta2-review.json), [точные объекты](evidence/dedup-delta2-integrity.json), [полнотекстовые запросы](evidence/dedup-delta2-search.json). Во вновь добавленных документах N52-01/N52-02 повторно используются для разных находок: нужен filename вместе с ID.

## 4. Критерий repeat / refined / new

Идентичность находки сравнивается по набору: **владелец состояния/функции → допустимый вход/trigger → вычислительная причина → наблюдаемый отказ → предметная задача → граница caller/доказательства**.

- **REPEAT:** те же owner, trigger и отказ; новая редакция или ещё один численный пример не создают новой общей причины.
- **REFINED:** известный механизм получил новый конкретный trigger, действующего потребителя либо более узкое различающее вмешательство. Ссылка на прежнего владельца обязательна.
- **NEW_CONDITIONAL_MECHANISM_IN_AUDITED_CORPUS:** ближайшие prior темы связаны, но именно этот механизм/trigger не найден. Новизна не присваивает статус causal quality proof.
- **COUNTEREVIDENCE / EXCLUDED:** нынешний путь исключает trigger либо old/current тела совпали; такой результат сохраняется, а не скрывается для увеличения числа находок.

## 5. Точные решения по трём кандидатам

### D-01. Sparse AdamW state + global update age

**Вердикт новизны: NEW_CONDITIONAL_MECHANISM_IN_AUDITED_CORPUS.**

Ближайшие владельцы:

- [N26 AdamW scale cancellation](https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/26-NEW-ADAMW-SCALE-CANCELLATION-2026-10-16.md) описывает m/v/bias correction и нечувствительность нормированного update к равномерному scale.
- [DLT-K1](https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/PREPARETOGPL-DPO-LORA-TOKENS-2026-10-18.md) и [N-NEURAL-02 / R06](https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/43-PREPARETOGPL-RESTORATION-AND-NEURAL-DIFFS-2026-10-18/06-EVIDENCE-AND-NOVELTY.md) уже рассматривают перенос C50 moments в другую тему и конфликт первого момента. Точное refinement R06 — m50→0 при прежних v50/step50.
- [N48-02](https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/02-NEW-HYPOTHESES.md) рассматривает Inf-v и его допустимость в portable continuation.
- [P12](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P12-optimizer-boundaries.md) рассматривает first LR0, finite clipping и partial SFT state.

**Отличие:** отсутствующие moments могут быть разрешённой sparse policy для ещё не использованного параметра. Если он получает первый Grad после множества global updates других параметров, bias correction опирается на global t, хотя его собственные m/v имеют возраст1. Это не тот же опыт, что сброс всех moments или только m50, и не тот же отказ, что нечисловой v.

**Граница:** sparse entries названы намеренными; потому формулировка «повреждённый checkpoint потерял параметры» неверна без отдельного факта. Сравнение per-parameter-age и global-age является выбором контракта оптимизации. Разница не доказывает универсальное нарушение AdamW и не требует автоматически менять все state schemas. В профиле RP01 ENRU, который требует все 16 moment entries и использует плотную активацию, причина не установлена.

**Рекомендуемая карточка:** прежние dense-active шаги как точный отрицательный контроль; один late-active параметр с одним новым gradient как trigger; отдельно обычное продолжение и valid sparse restore; затем предметный pilot только для подтверждённого редко активного recipient. Ни один prior текст не должен быть назван доказательством такого активного sparse RP01 run.

### D-02. Signed score mass в KnnSkillFilter при полностью eligible IDs

**Вердикт новизны: NEW_REFINEMENT, другой численный trigger известного owner.**

Ближайший prior — [WIKI51 NK-03](https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/51-LOCAL-WIKI-SQL-KNN-TENSOR-RECOVERY-2026-10-19/03-NEW-KNN-CONSUMER-GAPS.md), раздел Global topK может успешно удалить все допустимые навыки. Там input skills A/B, найденные global IDs чужие, scores .9/.1 дают confidence≈.9, а postintersection возвращает []. Это адресное уточнение общего P104.1 scope-before-topK. [P93](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P93-confidence-abstention.md) — другой owner, Generic KnnLogicalUnit.ExecuteAsync и score calibration.

**Отличие:** в новом опыте все возвращённые IDs с самого начала eligible, их порядок фиксирован. Положительный best score делится на signed sum scores. Ввод отрицательной similarity может уменьшить знаменатель и поднять confidence выше1, изменить confidence gate при прежнем лучшем соседе. Отсутствие output здесь не связано с чужими IDs. Для уверенной дедупликации опыт должен сохранять eligibility и rank, измеряя именно gate/result содержимого.

**Граница:** все эти компоненты присутствовали в успешном donor. Default ContextWindowManager использует PassThrough; установленный runtime KnnSkillFilter и provider со signed score должны быть доказаны отдельно. Указание класса и source-proxy не доказывает, что у текущего пользователя навыки реально терялись. До такого caller trace это условный downstream trigger, а не установленная причина качества RP01.

**Рекомендуемая карточка:** finite signed scores при одинаковых eligible IDs; all-positive scores как контроль; near-zero sum как edge; trace фактически переданных навыков и результата задачи при том же context budget. Изменение score normalization и eligibility не соединять в одном A/B.

### D-03. Used PositionInParent saturation 255

**Вердикт новизны: NEW_REFINEMENT известной структурной геометрии.**

Ближайшие владельцы:

- [N35-A](https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/35-NEW-E09-SIBLING-FEATURE-INVISIBLE-IN-STRUCTURAL-KNN-2026-10-16.md) прямо говорит о насыщении SiblingCount 255, но его основной дефект — отсутствие SiblingCount в distance.
- [core73](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/73-rare-class-evidence-and-structural-feature-scope.md) уточняет, что считают все дочерние SyntaxNodes; поле не означает punctuation siblings.
- [P26.4](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P26-structural-features.md) уже буквально фиксирует, что255/256 детей могут слиться до distance. Поэтому «впервые найдена byte saturation» — повтор.
- [P27](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P27-structural-codec.md) покрывает kind codec, boundary/flattening, csharpSource ownership и другие условия представления.

**Отличие:** PositionInParent реально входит в distance; если позиции255/256 и остальные используемые поля совпадают, этот уже работающий признак перестаёт различать контексты до KNN. Новое вмешательство — восстановить только представимость используемой позиции, сохранив остальные слагаемые, k и tie-break. Добавление штрафа неиспользуемого SiblingCount — другое вмешательство.

**Граница:** collision готовых вручную векторов доказывает только формулу. Для предметного эффекта нужны реальные Roslyn nodes длинного метода с одинаковыми остальными used features, различными целевыми next-kind и фактический predictor/consumer. Случаи до255 — обязательный retention control. Унаследованная граница становится кандидатом регрессии только при изменившихся длинах/структуре настоящего корпуса; один и тот же код old/current этого не доказывает.

## 6. Проверенные повторы, которые не вошли в список новых

| Кандидат | Ближайший существующий владелец | Решение |
|---|---|---|
| MatMul batch broadcast и Mean keepDims:false backward | [WIKI51 conditional tensor](https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/51-LOCAL-WIKI-SQL-KNN-TENSOR-RECOVERY-2026-10-19/04-TENSOR-CONDITIONAL-GAPS.md) | Точный повтор; actual problematic shape остаётся необходимым |
| E22 votes > configured k/2 при одном доступном уроке | [N37 one-pair cliff](https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/37-NEW-E22-ONE-PAIR-KNN-VOTE-CLIFF-AND-ONLINE-REPLACE-2026-10-16.md) | Точный повтор |
| Branch→registration training corpus grouping, первые64tokens | [PREPARETOGPL units](https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/PREPARETOGPL-UNITS-RESTORE-2026-10-18.md), [48 mechanisms](https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/01-MECHANISMS-AND-EVIDENCE.md) | U-N2/PG-07 уже существуют |
| Любой IsPatternExpression якобы защищает нужный объект | [core65](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/65-e22-positive-late-run-and-label-transfer.md), [N48-01](https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/02-NEW-HYPOTHESES.md) | Общий дефект уже отмечен; native receiver/predicate fixture допустим как уточнение |
| Dropout0 vs.1 | [P01.4](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P01-trainable-recipient.md) | Общий опыт уже есть. Fixed-p padding/RNG correlation — отдельное условное уточнение; не активно в dropout0 RP01 |
| Generic full restore/RNG/moments | [P25](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P25-cold-restore.md) | Нельзя заново считать общую проблему; нужен конкретный потерянный owner/field/caller |

## 7. Карта владельцев по всем пользовательским механизмам

Это маршрут к уже существующим исследованиям, а не повторная публикация их как новых фактов. P-ссылки — карточки ближайших проверок; применимость и статус отличаются по владельцу.

| Категория | Ближайшие владельцы | Содержательное различение |
|---|---|---|
| Реальный neural получатель и LoRA | [P01; core02/41](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P01-trainable-recipient.md) | q/v-only, trainable head/base, rank/alpha/dropout — разные вмешательства; discrete Unit не получает LoRA только потому, что он стоит рядом в Flow. |
| Рабочая база и reference | [P02/P105; recovery chapters](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P105-frozen-reference.md) | Random B0, подлинный обученный donor, policy continuation и frozen reference сохраняют разные состояния. До новых updates нужен один и тот же нулевой baseline. |
| Качество DPO и происхождение | [P04/P14/P109; N50](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P04-preference-contrast.md) | Обе стороны должны различаться и соответствовать выбранному предпочтению; 3000 архивных пар уже проверялись на equality/identity/потенциальные инверсии. Повторный подсчёт не новый механизм. |
| Model-visible материал и conditioning | [P07/P66/P81](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P07-materialized-token-contrast.md) | Текстовый contrast должен пережить материализацию tokens/masks; одинаковая строка prompt не доказывает, что learner зависит от её смысла. |
| DPO objective и mask/gradient | [P05/P86/P87](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P05-response-length-objective.md) | Sum/mean response likelihood, fractional mask и exact/legacy смешение уже имеют самостоятельные условия и опыты; изменение reported loss не равно улучшенному обучению. |
| Autograd и optimizer | [P06/P10–13; N48-02; WIKI51 tensor](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P12-optimizer-boundaries.md) | Teaching-vector scalar, reachability, finite clipping/partial mutation, Inf-v и reduced-rank/broadcast TensorOps известны. Sparse per-parameter age — отдельное условие [NEW-AGE](01-NEW-HYPOTHESES.md). |
| Tokenizer и embedding geometry | [P17/P18/P98/P99/P113; LW-BPE-02](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P17-tokenizer-refit.md) | Refit меняет ID/merges; state identity, vocabulary capacity и feature energy должны совпадать на записи и запросе. Numeric seed budget512 уже исследован пакетом50. |
| Обычная генерация и cache | [P08/P09/P75/P76/P102](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P09-cached-forward.md) | EOS framing, full/cache parity, float32 sampler tail, UTF-8 cutoff и teacher forcing уже известны. Сохранённый preference не заменяет собственный ответ. |
| Структурные признаки по предмету | [P16/P26/P27/P28/P43/P45](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P26-structural-features.md) | AST/parent/position для кода; POS/order/morphology для перевода. Поле в record ещё не значит слагаемое distance. FileId invariance может быть полезна. |
| KNN distance, соседи и confidence | [P20–23/P35–37/P90/P93/P100/P104/P107](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P93-confidence-abstention.md) | HNSW reachability, фактическое число голосов, scope до topK, lawful aggregation, tie order, score→acceptance уже покрыты. Signed mass другого фильтра — уточнение [NEW-SCORE](01-NEW-HYPOTHESES.md). |
| DPO→subunits и композиция | [P44/P46/P51–56/P81/P88/P94](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P51-translation-context.md) | Projection, context/topic/direction, identity, batch atomicity, phrase composition и typed many-results требуют отдельного настоящего потребителя. |
| Admission и исправления памяти | [P19/P59–65/P67/P108](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P64-tier-postfilter.md) | Mutable output, latest correction, signature duplicates, unknown-tail, containment/postfilter cascade и metadata carrier имеют разные владельцы; нельзя собрать их в одну причину KNN. |
| DPO/KNN→обычный Flow | [P92/P95–97/P125; U-N2/PG-07](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P92-dpo-memory-flow-bridge.md) | Нужно показать train→save→load→execute для именованного владельца. Branch→registration grouping и первые64tokens уже прямо разобраны. |
| Зависимости, credit и curriculum | [P40/P48/P50/P111; LW-FB-01](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P50-graph-dependencies.md) | Feedback, приписывание результата, typed dependencies и конец графа — разные состояния. Plan-only рекомендация следующего Unit не доказывает обучение всей цепи. |
| Replay и удержание | [P15/P24/P68/P74/P116](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P116-e43-file-boundary.md) | Selection/ranking/order, eviction, pretrain/file boundaries и действующий replay learner проверяются по старым и новым задачам отдельно. E43 является отдельным положительным donor. |
| GraphRAG и evidence | [P69/P70/P118; AC-EV02/NK-01–03](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P69-graphrag-context.md) | Prefix/context budget, seedless fallback, потеря trace/evidence и индексная статистика уже разнесены по потребителям. WIKI51 дополнил exact SQL и skill-filter cases. |
| Cold restore и продолжение | [P25/P18/P22/P105; N52](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P25-cold-restore.md) | Tokenizer/index/output map, RNG, optimizer и reference — отдельные части. Одинаковый ответ до update не гарантирует тот же следующий update. Orchestrator v1/v2 admission уже добавлен свежим52. |
| Oracle, детерминизм и честная метрика | [P03/P39/P57/P79/P80/P103/P112](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P39-oracle-output-separation.md) | Historical PASS, template lookup, hard-coded law, oracle features и данные настоящего learner нельзя объединять в один процент. Source-family split и denominator должны соответствовать вопросу. |
| Learning budget и stopping | [P41/P77/P78/P89/P101/P119](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P119-learning-dynamics.md) | Rare-class support, low delta/plateau, length/density и ресурсы уже имеют отдельные карточки. Малый отрицательный пример ограничивает свою панель, а не доказывает универсальную бесполезность механизма. |



## Полный навигационный индекс прежних карточек

[125 карточек P01–P125](evidence/prior-finding-index.md) позволяют открыть прежний механизм и его предметный A/B без пересказа всех карточек в новые гипотезы. Наличие пункта в индексе не присваивает ему статус доказанной причины: статус, consumer, контрдоказательства и ограничение домена читаются в самой карточке.

## Связь с полным требованием Unit → Flow → Orchestrator

Исходная постановка требует типизированное many→many поведение Unit, коррекцию входа в выход через Flow, асинхронный Orchestrator и граф следующего уровня. Новые математические пробы не удостоверяют эту цепь целиком. При восстановлении надо отдельно проследить точную Teacher-пару, её typed projection, получателя изменяемого состояния, права этого состояния влиять на output и тот же путь после загрузки. Для этих проверок сохранены владельцы строк «DPO→subunits», «DPO/KNN→обычный Flow», «Зависимости» и «Cold restore» таблицы, включая текущую дельту52. [Первичное архитектурное требование](https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/dpo%20legal%20build/active-codex-execution-original-task.md#L1-L16).

Свежие документы52 про orchestrator admission, trained-state и continuation прочитаны для дедупликации. Их первичные результаты этим пакетом заново не сертифицируются. Ни source-level исправление optimizer, ни улучшение отдельного KNN predictor не заменяют проверку исправляемого Flow и обычного ответа.
