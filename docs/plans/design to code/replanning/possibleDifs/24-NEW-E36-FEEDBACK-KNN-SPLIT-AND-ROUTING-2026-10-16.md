# NEW-24 — E36: «неудачный» kNN feedback routing мог быть неверно прочитан из-за split по повторным шаблонам и подмены метрики обучения

**16.10.2026 · SOURCE-TRACED / TWO HISTORICAL RUNS VERIFIED / CURRENT USER-QUALITY NOT RUN.** Core snapshot: [0eccbdc3c4efb90adf1d699806cbfa9f5610998d](https://github.com/lubochka/llm_mvp_core/tree/0eccbdc3c4efb90adf1d699806cbfa9f5610998d). **Ни новую модель, ни исправление кода, ни independent CODEX/Astra review не запускали.**

## Почему E36 важен в расследовании «неудачная матрица → успешная функция KNN»

E36 строит *память исправлений пользователя*, чтобы отправить исправление нужному получателю — Decomposer (32), Stop Signal (33), Query Match (34), Dependency (35), Merger (37). Это потенциально нужное условие **continual learning, корректного credit assignment и DPO→Unit/Flow**, а не просто вспомогательный текстовый классификатор. Провал или PASS старой экспериментальной метрики **нельзя** автоматически считать доказательством непригодности или успешной миграции такого механизма в живой runtime.

### Свидетельства из исходного эксперимента

1. [E36 Run 20–31](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/35.ExperimentsV2/Experiments/E36FeedbackCorrector.cs#L20-L31): создаёт **n=tier×40** feedback rows через GenerateFeedbackExamples; затем один псевдослучайный row-level split 80/20. Train/validation не разделены по **исходному шаблону, смысловой семье или истинному пользовательскому обращению**.
2. [GenerateFeedbackExamples 77–107](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/35.ExperimentsV2/Experiments/E36FeedbackCorrector.cs#L77-L107): в коде ровно **15 фиксированных текстов** (3–4 на component), а n samples строятся через многократный выбор одного из них **с возвращением**. Поэтому одинаковая фраза может присутствовать и в train, и в val. Это доказанный риск утечки; *реальное число пересечений двух сохранённых runs пока не вычислено*.
3. [KNN scoring 29–52](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/35.ExperimentsV2/Experiments/E36FeedbackCorrector.cs#L29-L52): FamilyCollapser/TagDist → top3 → голосование классов, против majority-class baseline. **В E36 действительно используется 3 соседей**, в отличие от E26 с объявленным, но игнорируемым _k.
4. [Сохранённый tier1, May 1](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/35.ExperimentsV2/results/E36-FEEDBACK-CORRECTOR/results/run-2026-05-01-001.json): train32, val8, majority=0/8, kNN=2/8 (25%), PASSED, val class coverage=**0.40**.
5. [Сохранённый tier2, May 1](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/35.ExperimentsV2/results/E36-FEEDBACK-CORRECTOR/results/run-2026-05-01-002.json): train64, val16, majority=0/16, kNN=0/16 (0%), FAILED, val class coverage=**0.40**.
6. Если написать «E36 улучшил KNN на 25%» **нужна точность**: речь об абсолютных **+25 процентных пунктах на восьми примерах tier1**, где базовая точность нулевая. Относительный прирост к нулевому baseline математически не определён. «FAIL tier2» тоже не показывает, что learned KNN потерял полезный навык на независимом распределении.

### Новое отличие от уже существующих DIF

- [E09/E34 comparison drift](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md) фиксирует **разные eval subsets** в других экспериментах; здесь более специфический риск: **повторяемые тексты, произведённые одним и тем же шаблоном, пересекают границу train/val E36**.
- [17-E13 boundary](17-HISTORICAL-E13-TRAIN-VALIDATION-BOUNDARY.md) касается реконструкции соседних методов и возможного train overlap; здесь это **модель коррекции человеком** с 5 возможными получателями, а не next-method prediction.
- [DIF-26/DIF-38/DIF-43](01-PRIORITIZED-DIFFERENCES.md) требуют обоснованного recipient/feedback learning; E36 здесь важный конкретный исторический precursor, *но не доказанный существующий product caller*.

## Гипотезы H-24

- **H24-A / оценка:** статус PASSED/FAILED меняется главным образом из-за случайного состава короткой панели и несогласованности семантических шаблонов. Если оценивать на **полностью новых формулировках** равномерно для всех 5 components, качество обученной памяти может отличаться от обоих отчётов и от majority-class.
- **H24-B / ошибочный перенос:** при переносе из эксперимента в runtime могла быть сохранена только жёсткая классификация «по ключевым словам», а **learned feedback→correct Unit recipient** не подключен или вычисляет другой набор признаков. **Это пока поиск потребителя, не установленный missing call**.
- **H24-C / ошибочный gold:** label некоторых correction-текстов зависит от контекста текущего Graph/Flow, а E36 хранит только фразу. Два разных истинных recipient могут соответствовать одинаковой или очень похожей фразе. Требуется контекстный oracle, а не подгонка к синтетическому label.

## Быстрый причинный опыт «E36-SPLIT-01»

**L0 (без нового обучения):** использовать исходный генератор и seed, посчитать для tiers1/2: train/val пересечения точных фраз, частоты 15 шаблонов, class coverage, distribution, число независимых шаблонов, дубликаты и bootstrap CI. Не менять записи истории.

**A — historical:** исходный E36 TagDist/top3/FamilyCollapser, row split; пересчитать на одной замороженной панели.

**B — корректная независимая оценка:** обучить ту же функцию с идентичными параметрами/сопоставимым train budget; валидировать по **оставленным целиком вне обучения шаблонным семьям**, 5-stratified leave-one-template-per-class-out folds. Evaluation original phrases не пересекает train. Не менять одновременно kernel, k и family collapse.

**C — контроль:** majority-class на том же fold и **перестановка train labels** при сохранении векторов, class ratios и fold; если C≈B, сигнал коррекционного обучения не установлен. **D — контекстный вариант** только как отдельный следующий эксперимент после B (один additional context recipient hint, без oracle leakage).

**Реальный критерий:** измерить macro-F1 пяти классов, per-class recall, correct recipient@1/3, abstention/unknown и downstream *фактически исправленную Unit* на 10–20 blind новых correction cases. Качественное улучшение имеет место только если работающий ordinary feedback consumer направляет исправление нужному Unit/Flow и следующий независимый запрос становится верным; route metric одна недостаточна.

**Фальсификация H24-A:** если при template-disjoint B всё ещё существенно выше matched majority/shuffled control и downstream работает — есть дополнительный learned сигнал; если все методы около chance на новых фразах, архивный PASS не переносится. Любой вариант без actual consumer остаётся **механическим**. Условие остановки: если источники истины для 5 recipient отсутствуют, STATE=INSUFFICIENT_ORACLE; не заменять независимый gold историческими labels.

## Требования к публикации и людям

Малый local/native diagnostic только после разрешения исполнения; не нужен Windows GitHub workflow, merge, полная QA батарея. CODEX/Astra в человеческой роли выбирает задачу обучения и проверяет истинный recipient у реального исправления; авторство финальных chosen/rejected в *новом DPO* — только у разрешённой внешней открытой китайской модели, если нужны именно DPO пары. Если эксперимент только классифицирует утверждённые labeled feedback rows, он **не генерирует DPO**. Независимая оценка качества Student/Unit обязательна до вывода об улучшении. Исторический matrix→KNN donor commit ещё не доказан; не приписывать E36 происхождение чужому переносу без git lineage.