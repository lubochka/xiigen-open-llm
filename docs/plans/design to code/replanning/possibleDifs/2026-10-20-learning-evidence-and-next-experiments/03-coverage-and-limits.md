# Покрытие: что есть в экспериментах и где остаются точные пробелы

**Все найденные различия нельзя объявить уже корректно измеренными.** Два новых tensor-фактора ещё не проверены опытами, phase-only E-METRIC-PHASE не закрыт P78/P89, P54 требует конкретных inputs. На позднем core669 прежний P54 запрет уже снят, NG01 получил полную protected карточку, R1 report исправлен, R2 завершён. Эти устранённые документные gaps больше не называются текущими дефектами. [Сводка669](07-core669-status-and-results.md). Защищённая папка этой работой не менялась.

## 1. Инвентаризация без подмены семантической проверки

| Проверяемый объект | Результат | Что этот результат доказывает |
|---|---:|---|
| Файлы possibleDifs на planning `6bc4b6fba8f14870f5dc4966cf5a344754962d66` |1313 | Полный полученный tree данной папки |
| Markdown possibleDifs |747/747 Git blobs совпали | Дедупликация опирается на текущие тексты, не устаревший кэш |
| Файлы protected на core `883496bca18113bf37e0afe683c75b05a0075048` |1253 | Полный tree защищённой папки, включая сохранённые результаты/LFS |
| Markdown protected |334/334 Git blobs совпали | Полный текстовый корпус для поиска и адресных сопоставлений |
| Cards на883 |154 файла =153 карточки+индекс |125P-семейств и 28 конкретных карточек |
| Матрица P-подслучаев |470/470 ссылок и explicit anchors существуют | Навигация полна; build/runtime/семантический success этим не доказан |
| Source execution index |32 назначения на883;36 на827,01–06 завершены | Следующий07=LF-03; объединённая программа добавляет2 наших ID |
| Дополнение planning53cb к исходному snapshot |87 новых файлов,52 новых Markdown+1 изменённый README | 1400 файлов /799 Markdown;53 поздних Markdown blobs сверены, адресный semantic review и проверка дублей |
| Согласованная проекция очереди |38 строк,32 pending;6 completed | Все 36 опубликованных ID и их относительный порядок сохранены; наши 2 фактора перед двумя dependent readers |

[Полный file manifest](coverage/repository-content-manifest.json), [747 Markdown и их identities](coverage/planning-source-index.json), [470 переходов](coverage/subcase-links.json), [153 карточки](experiments/CARD-BY-CARD-COVERAGE.md), [правила и source lines](experiments/card-policy-matrix-current.json), [текущие 32 строки](experiments/current-queue.json).

Все 153 карточки исходного883 уже имеют default-модель, NEXT/STOP, экспериментальные исключения, русский отчёт и before/after. Прежние заявления об отсутствии этих правил во всём каталоге устарели. Пять конкретных карточек без собственного точного skill-списка получили добавочный список в [overlay](experiments/EXECUTION-CONFLICT-OVERLAY.md). Наличие формулировки не означает, что любая ветвь автоматически выполнима или любой measurement верен.

## 2. Поиск по имени — навигатор, не доказательство покрытия

Из 747 Markdown:593 необщих имени встречаются в protected,116 не встречаются,38 общих README/report-подобных имён не оценивались таким способом. **593≠593 семантически закрытых гипотез;116≠116 пропущенных опытов.**

Для 116 выполнена отдельная содержательная классификация: заголовки и consequential абзацы всех документов, адресная сверка конкретных факторов с current recipes/code. Итог:79 метадокументов,13 исторических/кандидатных документов,10 широких карт с неаттестованной построчной полнотой,8 документов с названными текущими рецептами,2 точных gap,1P54 conflict,1 условный caller,1no-delta accounting,1 альтернативный E2 contract. [Объяснение и доказательства](history/04-unmentioned-source-crosswalk.md), [все 116 строк с confidence и scope](history/evidence/unmentioned-source-crosswalk-116.json).

Для десяти широких карт `U006/U012/U017/U024/U028/U037/U042/U046/U059/U089` не выставлен фиктивный общий `covered`. Они содержат много механизмов; текущая проверка не сертифицирует каждую встроенную гипотезу. Их точные пути доступны в JSON. Это явная граница проверки, не отбрасывание источников и не отрицание содержащихся в них результатов.

## 3. Точная карта новых и оставшихся различий

| ID / различие | Новое в этом аудите? | Что есть в protected | Что сделано здесь / статус |
|---|---|---|---|
| **GRAD-ACCUM-COPY-01** | Новый узкий factor относительно проверенного корпуса | LF-03 меняет другую копию в Tensor.Backward; exact AccumulateGrad A/B не найден | Полный [card+prompt](learning/GRAD-ACCUM-COPY-01.md), native source-chain и fixed execution contract; не запускался |
| **FROZEN-BROADCAST-MAP-01** | Новое уточнение RMS-семьи | RMS-COORD-ALLOC-01 меняет FlatToIndices и сохраняет mappedBIndices | Полный [card+prompt](learning/FROZEN-BROADCAST-MAP-01.md), только no-grad map-factor; не запускался |
| **HIST-E07-GLOBAL-01** | Новое условное свойство repeated Train | E07 alignment/tier/lineage не покрывают last-block fallback | [Условная спецификация](history/02-conditional-ngram-continuation.md); нет established cumulative caller, вне активной очереди |
| **N62-R1 measurement interpretation** | Независимый исторический readback | На669 report уже исправлен; новый R2 также показывает отсутствие after-panel | [Corrigendum](experiments/N62-R1-RESULT-CORRIGENDUM.md) и [кандидат](candidates/N62-R1.md); no rerun |
| **NG01 int-relational-pattern** | Уже найден другим автором | На669 полный exact NG01 card+prompt, DESIGNED_NOT_RUN | Документный gap закрыт; новый internal host указан в root05; результат не измерен |
| **E-METRIC-PHASE** | Уже найден другим автором | P78 меняет окно, P89 нормировку; точный phase-only monitor factor не найден | [Exact gap](history/04-unmentioned-source-crosswalk.md); current learning caller подтверждён, паспорт с literal inputs/host ещё не готов |
| **P54.1–P54.7** | Старые семь механизмов | На669 все7 запретов сняты; NEEDS_INPUT_FOR_RECIPE | [Уточнена применимость](history/03-p54-ng01-semantic-verdict.md); нужны конкретные learned checkpoints/inputs/host, после прямого обучения |
| **N60-E2 representation/reader boundary** | Известный альтернативный дизайн | SourceSpanFrames перед успешным phrase return | OrderedPhraseSupport/v1 до всех routes не эквивалентен; [граница сохранена](history/04-unmentioned-source-crosswalk.md), не смешивать в два фактора |
| **Native no-delta vs readback invalid** | Уже найденное account-свойство | Один current guard объединяет несколько причин | Наблюдать в уже назначенном update; не создавать fake/no-op run ради статуса |
| **Positive verifier correction** | Ранее условная гипотеза | Общий prior не устанавливает producer/admission положительного урока | Вне активной очереди до real caller; не тратить обучение на вручную подставленный недоказанный путь |

Два новых tensor-паспорта имеют все назначенные данные, Student/default-модель, source-copy/host/команду после подготовки, бюджет, измерение до/после, отчёт и STOP/NEXT. Они **не физически перенесены в protected**, поскольку пользователь запретил менять эту папку. Точный gap отмечен, а материал для такого опыта сохранён в разрешённом месте.

## 4. Почему нельзя написать «все before/after корректны»

- N62-R1 не записал final learning-readout; его возвращённые step metrics относятся к pre-update. Новый протокол не добавляет несуществующий after задним числом.
- В отдельных исторических E09 разные denominator/panels baseline и enhanced; inference размера панели явно отделена от отсутствующего per-case historical manifest.
- Исторические E07/E22 таймеры охватывают разные этапы. Их полезные quality-deltas сохраняются, но из них нельзя вывести цену одинакового полного обучения.
- Existing LFS pointers не являются payload: comparison.json первых трёх опытов и бинарные checkpoints N62 не декодировались как отсутствующие данные. Проверка чисел использовала доступные arm raw/receipts.
- P-семейство может содержать допустимый типовой вопрос и всё ещё не иметь конкретного исполняемого micro на каждый domain profile.

[Пять актуальных результатов и original RP01](02-results-and-candidates.md), [историческая сопоставимость](history/01-history-evidence.md). Новые измерения и повтор завершённых опытов для исправления этих формулировок не проводились.

## 5. Что уже разрешено документно

Общий [договор](05-executor-contract.md) задаёт пользовательский критерий любого полезного улучшения, все 32 pending output assignments вне protected, без дублирующих host claims, default model/skills/русский report и абсолютный NEXT/STOP. [Реестр](04-execution-registry.md) устраняет конкурирующие next IDs, сохраняет историю и ставит два dependent readers после прямого обучения. [N62 overlay](experiments/EXECUTION-CONFLICT-OVERLAY.md) сохраняет историю старого R1/R2 literal; оба фактора теперь completed, их повтор не разрешён.

Эти дополнения снимают конкретные конфликтующие инструкции для назначенного будущего исполнения. Они не выдают невыполненный experiment за результат, не меняют production и не требуют новых разрешений GitHub для уже порученного документирования.


## 6. Поздняя проверка перед публикацией

[Согласование 53cb](06-concurrent-reconciliation.md) сохраняет оба параллельных пакета, их primary readback и все 32 ранее назначенных pending outputs. Точные GRAD/FROZEN вмешательства не совпадают с ROPE, AST/SKNN и BPE-REALIZED. N62-R1 corrigendum более не называется уникальной находкой относительно нового HEAD. Первоначальные inventory/review receipts 747/35 сохранены с исторической границей; активный реестр —38, completed5, outputs33 после [позднего669](07-core669-status-and-results.md).

Original RP01 primary summaries подтверждают реальные 50+50 updates, изменение 16 LoRA при 39 неизменных base и положительную mean-margin дельту на каждом домене. Свободные ответы не улучшились. Это отдельное свидетельство полезной метрики и ограничения результата; повторного декодирования checkpoints или реконструкции индивидуальных grades не было.

**Финальный827:** [срез08](08-final-core827.md) добавляет E-OPT raw factor mismatch и stale future E-OPT prompt при completed статусе; источник и измерение не смешиваются. Оставшиеся corpus-wide выводы по827 не сертифицируются адресным delta readback.
