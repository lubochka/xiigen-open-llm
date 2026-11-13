# N50 — дешёвые причинные эксперименты для external_domains, по предметным темам

**Дата:** 2026-10-19. **Тип результата:** `DESIGN_ONLY`; исходный read-only аудит 3 000 строк `DONE`, **новых native .NET DPO / LoRA / KNN learning runs НЕ было**. Donor [external_domains](https://github.com/lubochka/llm_mvp_core/tree/external_domains), current [codex/dpo-implementation](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation), RP01 [learning-first pack](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/). Строго не изменять `llm_mvp_core/docs/plans/replanning-possible-diffs/**`.

## Сначала различить две цели

1. **Repair original learning:** восстановить измеренные E09 (C# AST/kNN), E26 (context dictionary в переводе), E43 (replay) в оригинальном **типе получателя** и вызвать обычный consumer. Эталон и ограничение сопоставимости: [Глава 0](00-EXTERNAL-DOMAINS-POSITIVE-RESTORE-P0-2026-10-19.md).
2. **Repair anomalous external CODE_TO_DOC DPO:** доказать, что дефекты в [N50](50-NEW-EXTERNAL-DOMAINS-PAIR-IDENTITY-AND-INVERTED-PREFERENCES-2026-10-19.md) влияют на **ту же** learning path, а не посторонний архив. `external domains/documentation/learning sets` **не следует выдавать за RP01 C#500** без `source_digest + original IDs + actual call site`.

Принцип каждого опыта: **один исследуемый фактор, одинаковые исходные запросы, одинаковый recipient, одинаковые данные остальных частей, один и тот же blind judge; негативный контроль обязателен**.

## MICRO-N50-00 — без обучения, немедленное подтверждение исходного дефекта

**Гипотеза:** в исторических пятнадцати JSONL парные контрасты, overlap-score, семейства запросов не соответствуют паспорту.

**Точные входы:** branch `external_domains@3036a01acec938f0b83fa4ffbda26202cc35d639`; для пяти директорий `external domains/documentation/learning sets/{dotnet-csharp,sql-tsql,angular-typescript,mongodb-csharp,elasticsearch-csharp}/` прочитать `*-run-001..003.jsonl`. Команда запуска локальной воспроизводимой проверки после сохранения следующего скрипта:
```bash
python "llm mvp/design to code session/replanning/possibleDifs/50-EXTERNAL-DOMAINS-CORPUS-PROBE.py" --data-root "/path/to/llm_mvp_core/external domains/documentation/learning sets" --runs 3
```

**Сравнения / контроль:** `Chosen == Rejected` побайтно по Unicode строке; `tier==3 AND rejected_type==missing-edge-case`; уникальные `Prompt`; `compiled_prompt_in_training`; одинаковая тройка с разным `overlap_ratio`. Никакого переименования `sourceId` в independent sample.

**Ожидаемый SOURCE результат:** 3 000 строк / 55 prompt / 270 ties / 219 suspects; по доменам и их исходным runs — [N50](50-NEW-EXTERNAL-DOMAINS-PAIR-IDENTITY-AND-INVERTED-PREFERENCES-2026-10-19.md). Если файл отсутствует, ответ `ERROR/MISSING_SOURCE`, не заполнять нулями. Результат подтвердит **корпус**, не прирост точности.

**Исполнитель:** независимый ревьюер запускает на read-only checkout и сохраняет `command + runtime + source SHA + JSON counts + errors`, не изменяя исходники.

## MICRO-N50-01 — семантическая разметка: ties / reverse / actual margins (C#, SQL, Angular, MongoDB, Elastic)

**Предрегистрация:** один базовый тематический `CODE_TO_DOC` semantic rubric для задачи «написать документацию к исходному коду»: 
- правильные *intent*, *design reason*, *edge/failure cases*, 
- нет ложного утверждения о поведении,
- ясность и отсутствие copied syntax без смысла,
- никакой оценки по маркеру `[WRONG:]`.

**Вход:** из каждого домена выбрать 6 равных, 6 потенциально обратных и 6 иных paired cases (три источниковые группы, 18 на домен, 90 всего; при меньшей доступности сообщить фактический `N`). Два слепых независимых эксперта **не знают** полей `chosen/rejected`, `tier`, `annotation`; сохраняют `pair_id`, `source file/line`, agreement и три категории `same_quality / A_better / B_better`. 

**Контрасты:** настоящая сторона A vs B и копия с переставленными labels; не интерпретировать дополнительную длину как автоматически более высокое качество. Отдельно `actual_model_token_overlap` по конкретному tokenizer, а не сохранённому random `overlap_ratio`.

**Гипотезы и falsifiers:** для ties — `semantic_same`, ожидается 100% exact text ties; для 219 подозрительных — слепая оценка либо фиксирует `rejected_better`, либо отвергает гипотезу. Если expert agreement низкий, НЕЛЬЗЯ экспериментально считать repaired labels ground truth.

**Outputs:** `blind-review.jsonl`, `conflicts.md`, per-domain proportions, `actual vs archived overlap`, accepted/refused pairs. Веса модели **не трогать**.

## MICRO-N50-02 — действительно меняется ли качество обучения (два изолированных A/B, не смешивать факторы)

**PRECONDITION:** выяснены исходный потребитель, разрешённый легальный источник, ID и конвертер (линия `raw pair → admission → training material`); прав/разрешений нет = `BLOCKED_LEGAL_PROVENANCE`, **не** тренировать на архивных raw rows. Нельзя обойти current `PaidExternalModel` checks, `Admission` и tenant isolation.

**Первичная минимальная реализация:** предпочесть малый уже существующий локальный trainer или read-only preparer + проверку градиента на **одной паре**; не запускать полную 500-парную эпоху/Windows CI ради одного causality probe.

**Фактор A — качество предпочтений**, в **одном домене за раз**:
- A0: legal-admitted original **нетождественные** пары; если оригинал содержит tie, он должен быть `REJECTED_BY_GATE`, не насильственно допущен. Записать original admission count.
- A1: та же принятая группа, но только blind-expert-approved исправления спорных `tier=3/missing-edge`; одинаковые `prompt IDs`, `number of presentations`, `batch size`, `optimizer steps`, `B0 SHA`, `tokenizer SHA`, `LoRA modules, rank, alpha, seeds`, `reference SHA`. Для сохранения одинаковой длины/масок логировать токены обеих сторон и делать stratification по длине, не обрезать произвольно.
- Negative A2: те же данные, но **перевёрнутые случайно** labels / random preference; должно стать не лучше A1 (если стало — метрика/утечка/learnability под вопросом).
- **Основная метрика:** семантическая правильность финальной документации на 8–12 **не встречавшихся** source-template families (лучше 15–20 при наличии), blind judge, macro average по независимому файлу, + critical-law errors. **Вторичная:** training reward margin и log-probs на same accepted pairs. Весовое изменение не засчитывается как качество.

**Фактор B — разнообразие запросов** (НЕ одновременно с A):
- B0: исходные повторяющиеся формы (сохранить фактическое количество уникальных `Prompt`).
- B1: такое же `N` и те же subject laws, но дополненные реально новыми семействами исходного кода, экспертно проверенными, **не простая перестановка JSON keys**; одинаковый split по source-family и тренировочный бюджет.
- Negative B2: только косметическое переименование / сохранение той же семантики. Если B1 и B2 одинаковы, «добавление домейнов» могло быть лишь дублированием.

**Матрица:** по умолчанию ровно A0/A1/A2 или B0/B1/B2, *не* весь декартов продукт. После явного эффекта добавить второе измерение. Начальный размер 24–48 законно подготовленных пар/домен и 8–12 действительно новых тестовых случаев; результат малоразмерной пробы **exploratory**, не статистическая гарантия. Повторить 3 seeds, показать paired differences и bootstrap по исходным семьям, не по текстовым перефразировкам. No-op checkpoint и old skill retention обязательны. Если тренер велик для машины — начать с пары gradient/sign consistency, дальше переходить к recipient memory, а не подменять результат фиктивным proxy.

## MICRO-N50-03 — positive donor parity: C# структурный KNN (E09)

**Гипотеза:** синтаксические признаки AST, ошибочно сочтённые бесполезными для общей matrix, полезны **при retrieval** и реально улучшают C# решение.

**Точный donor:** [E09 исходный receipt](https://github.com/lubochka/llm_mvp_core/blob/external_domains/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json) + [E09 code](https://github.com/lubochka/llm_mvp_core/blob/external_domains/35.ExperimentsV2/Experiments/E09StructuralKnn.cs); найти его реальный `StructuralVectorExtractor` и текущий production `StructuralKnn` consumer.

**A/B:** A — lexical/vector-only on same C# heldout file IDs; B — *тот же* индекс/метки/`k=5`, но структурные признаки включены. C — AST признаки shuffled между файлами (negative). Запрет leakage: source-family/file-disjoint и нет cross-file transitions. **Важный контрфакт:** средняя длина/латентность retrieval; историческая `14.5→763.5 ms` показывает существенную цену.

**Acceptance:** B выигрывает у A по **matched** macro top-1 на одинаковых node IDs и/или реальные semantic laws документации улучшаются на тех же 8–12 unseen C# methods. Даже подтверждённый gain `top-1` — **только E09 задача**, не `RP01 trained Student`.

**Не переносить AST KNN по умолчанию:** SQL требует собственного parser + проверки; обычное повествовательное EN→RU не является AST задачей.

## MICRO-N50-04 — contextual translation word order (E26), два направления

**Источник:** [E26 receipt](https://github.com/lubochka/llm_mvp_core/blob/external_domains/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-003.json); соответствующая реализация `TranslationTrainedUnits`, POS/phrase consumer и их фактический save/load.

**Ситуации:** минимум по 4 независимых случая: неоднозначное существительное/глагол, отрицание, перестановка порядка слов, субъект/объект; у каждого matched paraphrase + adversarial same-bag-of-words (слова те же, порядок/роль меняются). Два **направления** EN→RU и RU→EN — это профили одного предметного домена «перевод».

**A/B/C:** A=exact/word map без контекста, B=контекст POS/order в learned kNN или learned phrase lookup (тот же corpus), C=перемешанная последовательность POS/времени или выключенный feature. Обычный application call после cold restore. Скорить meaning/negation/polarity/direction, source match, latency; не считать prefix exact lookup «обобщением». **Acceptance:** B>А на frozen unseen с устойчивостью старых примеров; иначе признак не нужен/нужен другой representation.

## MICRO-N50-05 — replay old/new + restore на одном получателе (E43)

**Исходная точка:** [E43 historical](https://github.com/lubochka/llm_mvp_core/blob/external_domains/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json). Выбрать конкретный обучаемый получатель, не смешивать bigram и LoRA. Записать 12 frozen старых и 12 новых independent example families (минимум) со `train/test`-разделением.

**A/B/C:** A=новые данные без replay; B=тот же budget, но bounded replay old examples; C=shuffled unrelated replay. После каждого сделать cold restart, проверить веса, `optimizer step`, tokenizer/version, kNN/phrase store identity, и **ordinary consumer**. Показать old retention + new adaptation + wall seconds/RAM. **Acceptance:** B сохраняет старое без критического ухудшения новых на том же recipient; если restore A/B неэквивалентен — дефект restore прежде вывода о forgetting.

## Пакеты исполнителей: ясность, разрешения, отчёт

**Граница задачи:** только работа с этой главой и измерительной копией; source repo защищён, `core/docs/plans/replanning-possible-diffs/**` **read-only**. Реальная модификация модели допускается только в separately authorized execution и с проверенным license/provenance. Текущее поручение пользователя — аудит и документация, не автоматически разрешённое переобучение продукта.

**Навыки:** `docs/skills/architect-behavior-classifier-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `generated-code-review-SKILL.md`, `experiment-design-SKILL.md`, `learning-path-audit-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-communication-GLOSSARY.md`.

**Output каждого опыта:** `subject/domain/profile; donor_branch_sha; recipient_type; experiment_ids; source IDs; admission counts; tokenizer/LoRA/checkpoint sha; random seeds; A/B exact one-factor diff; frozen test IDs; control; metrics+denominator; absolute mistakes; cost; source/quality evidence; verdict (VERIFIED/REJECTED/INCONCLUSIVE/NOT_RUN); review author`.

**Готово** только после отчёта на реальном обычном потребителе. Формулировка «3 000 пар проверено на равенство» — source audit, а не «модель обучилась на 3 000 пар». Формулировка «E09 passed» — historical top-1; не публикация нового обученного релиза.
