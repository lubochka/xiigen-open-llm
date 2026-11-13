# Восстановление DesignToCode-clean и дополнительные причины потери качества

19 октября 2026. **Сначала проверяем возврат полезного поведения из DesignToCode-clean.** Начальная очередь также вынесена [в отдельную главу общего списка](../00-DESIGNTOCODE-CLEAN-RECOVERY-FIRST-2026-10-19.md).

Это дополнение к существующему possibleDifs: исторические успехи привязаны к конкретным алгоритмам, проверены изменения neural core и добавлены две конкретные причинные цепочки. Для них подтверждён механизм в исходниках; влияние на предметное качество ещё предстоит измерить. Связь этих двух DesignConversion-механизмов с неудачей RP01 C#/EN→RU пока не установлена.

## 1. В начале — опыты, возвращающие успешное поведение CLEAN

| Очередь | Что проверять | Быстрый опыт | Домен и граница вывода |
|---|---|---|---|
| **R0: исходное обученное состояние и его получатель** | Точный исполнявшийся commit, checkpoint/index, tokenizer, learned entries, Unit/Flow и фактический caller | Один контрольный input до/после cold load; trace должен дойти до того же обученного получателя | Обязательно для каждого механизма. Наличие отчёта в ветке не устанавливает runtime SHA старого запуска |
| **R1: структурный KNN E09** | Действительно ли AST-признаки участвуют в запросе и выборе соседа | 12–20 C# файлов, общий holdout, baseline/KNN/абляция структуры, затем штатный consumer | Задачи о коде, чувствительные к дереву; не переносить автоматически на обычный текст |
| **R2: обученная память порядка E25** | Доходит ли обученный SentencePatternClassifier до ответа после восстановления | 16–24 предложения, rules/trained memory/cold restore/POS-абляция | Классификация шаблона и качество полного перевода измеряются отдельно; EN→RU и RU→EN — направления внутри перевода |
| **R3: сохранение прежней памяти E43** | Не потерялись ли старые записи при обновлении или загрузке | Old/new навыки внутри одного scope, память new против old+new и cold restore | Исторически обучалась таблица переходов. Для neural DPO нужна собственная разрешённая политика rehearsal |
| **R4: CLEAN-CP01 — импорт старого feature checkpoint** | Один retired feature вызывает отказ всей совокупности weights | Current import против явной versioned migration; сохранённые допустимые weights, Unit IDs, history и output сравниваются после cold load | Локальное feature state DesignConversion; не LoRA tensor checkpoint |
| **R5: CLEAN-CTX01 — одинаковое условие обучения и runtime** | Policy обучают на source signal, но вызывают с root/custody IDs | Один checkpoint, те же HTML/CSS стороны; нынешний prompt против общего source condition; 12–24 разных исходных макета | Преобразование UI-дизайна в HTML/CSS; каждой ветви A/B нужна отдельно выданная валидная runtime authority |

Исторические raw receipts CLEAN: **E09 .2413 → .3507; E25 .3438 → .875; E43 .3226 → .3978**. Это три разных метрики, а не общий показатель модели. У E09 baseline и KNN оценены на неодинаковом объёме validation, E25 — синтетическая POS-классификация, E43 — count-table retention. В [01](01-HISTORICAL-BASELINE-AND-RECOVERY.md) приведены исходники, ограничения и проверка соседних веток. Подробные короткие протоколы R1–R3 находятся в [04](04-DOMAIN-MICRO-EXPERIMENTS.md), R4/R5 — в [02](02-NEW-CHECKPOINT-AND-CONTEXT-GAPS.md).

Этот порядок задаёт приоритет доказательств. Если R0 обнаруживает, что состояние вообще нельзя загрузить, техническая проверка R4 проводится до измерения последующих эффектов.

**Для действующего UI/DesignConversion пути порядок уточняется после R0:** первым диагностировать известный peer [GDC-A0/A1](../54-DESIGNTOCODE-CLEAN-CAUSAL-MICROEXPERIMENTS-2026-10-19.md) — доходит ли предпочтение/категория до local feature learner и меняет ли его weights. Старый margin был также отчётным полем; возврат одного score-sidecar не гарантирует signed update. Затем проверять блокирующий restore R4 и condition R5. Эти peer-находки не присваиваются как новые; [поправка по исходникам](../54-DESIGNTOCODE-CLEAN-EVIDENCE-REVIEW-2026-10-19.md). R1 и R2 нужны соответствующим задачам о коде и переводе, а не обязательны для любого UI-запуска.

## 2. Дополнительные находки после сверки с другими агентами

| ID | Конкретная цепочка | Что нового относительно просмотренного реестра | Что ещё не доказано |
|---|---|---|---|
| **CLEAN-CP01** | CLEAN сохраняет source_manifest:hash_diversity; current StageLearnedWeights отклоняет весь checkpoint, если хотя бы один key не проходит whitelist | Само удаление feature уже описал peer. Здесь дополнительно прослежен отказ всего импорта и предложена явная миграция с сохранением допустимого состояния | Полный checkpoint успешной пользовательской сессии с этим key не найден; native отказ и прирост качества не измерены |
| **CLEAN-CTX01** | Training/heldout получает srcenc с vector/structure; обычный runtime передаёт design-conversion-runtime-pair с root/custody identities в реальные log-prob вычисления | Точный переход source encoder → coordinator → RuntimePair → scorer отличается от известных canonical prefix16 и local-weight проблем | Часть расхождения уже существовала в CLEAN; оно не объявляется новой source-регрессией или доказанной причиной RP01 |
| **HIST-R01, дополнительный диагностический случай** | Пересечение current и replay SourceId может вызвать duplicate error при подготовке mixed batch | Учтён владелец AUT-REPLAY-02; уточнение не увеличивает число новых находок | Live consumer и право на соответствующий update не доказаны; отказ для уже consumed pair может быть правильным |

Полные доказательства, контраргументы и варианты исправления — в [02](02-NEW-CHECKPOINT-AND-CONTEXT-GAPS.md); attribution и свежая дедупликация — в [05](05-EVIDENCE-NOVELTY-AND-REVIEW.md). Наличие похожего ключа, совпавшего текста или различия в коде по отдельности не считается доказательством причины ухудшения.

## 3. Что показала сверка DPO, LoRA и токенизации

Точный CLEAN уже имел подключённый LoRA и правильный DPO backward. BpeTokenizer, ModelConfig, RmsNorm, RotaryEmbedding, TransformerBlock и supervised Trainer имеют одинаковые Git blobs в двух снимках. Формула LoRA, инициализация A/B и её участие в attention сохранились. Поэтому возврат к старым файлам этих механизмов сам по себе не восстанавливает утраченный источник знаний, checkpoint или вызов нужного Unit. [Сравнение исходников и ссылки](03-MECHANISM-COVERAGE-AND-NEURAL-PARITY.md#2-что-точно-сохранилось-а-что-изменилось).

Реальное изменение neural objective — exact material, response masks, response sum и stable softplus при сохранении legacy-пути. Его следует проверять отдельными факторами на одинаковых IDs, исходных весах и данных. Для DesignConversion evaluated bridge предположение «train sum, runtime mean» **не подтвердилось**: проверенный caller остаётся legacy. Исправления fractional masks, embedding gradients, broadcast bias и точного восстановления optimizer/RNG требуется сохранить. [03](03-MECHANISM-COVERAGE-AND-NEURAL-PARITY.md), [контраргумент о DTC objective](02-NEW-CHECKPOINT-AND-CONTEXT-GAPS.md#3-проверки-которые-не-позволили-записать-старые-или-неверные-гипотезы-как-новые).

Полная карта запрошенных осей — DPO, LoRA, векторная геометрия, токенизация, KNN, скрытые фиксированные решения, преобразование урока в под-Units, восстановление и численная устойчивость — сведена [в таблицу 03](03-MECHANISM-COVERAGE-AND-NEURAL-PARITY.md#4-покрытие-всех-запрошенных-осей-и-предыдущие-владельцы). Связь с исходными RAG-требованиями Units/Flow и ролью арбитров дана в [05](05-EVIDENCE-NOVELTY-AND-REVIEW.md#3-связь-с-исходным-планом-и-rag-требованиями).

Последняя параллельная публикация также учтена: [U-DTC-1b/U-DTC-3, numerical N-ADJOINT-01 и границы пересечения](evidence/PUBLICATION-RECONCILIATION.md). Основной корпус 743 версии дополнен 15 версиями: итог 758 версий / 747 разных repo-paths. Новые peer-файлы сохранены.

## 4. Состав пакета и степень проверки

| Документ | Назначение |
|---|---|
| [01-HISTORICAL-BASELINE-AND-RECOVERY.md](01-HISTORICAL-BASELINE-AND-RECOVERY.md) | Что именно успешно обучалось в CLEAN и соседних ветках; как восстановить честный baseline |
| [02-NEW-CHECKPOINT-AND-CONTEXT-GAPS.md](02-NEW-CHECKPOINT-AND-CONTEXT-GAPS.md) | Две новые конкретные цепочки, прямые source anchors, варианты исправлений и малые A/B |
| [03-MECHANISM-COVERAGE-AND-NEURAL-PARITY.md](03-MECHANISM-COVERAGE-AND-NEURAL-PARITY.md) | Сохранённые/изменённые neural механизмы, покрытие остальных осей, владельцы прежних findings |
| [04-DOMAIN-MICRO-EXPERIMENTS.md](04-DOMAIN-MICRO-EXPERIMENTS.md) | Проверки KNN, порядка, retention и дополнительного replay diagnostic |
| [05-EVIDENCE-NOVELTY-AND-REVIEW.md](05-EVIDENCE-NOVELTY-AND-REVIEW.md) | Версии, область поиска, атрибуция новизны, RAG crosswalk, применение skills и независимая проверка |
| [evidence/CORPUS-MANIFEST.json](evidence/CORPUS-MANIFEST.json) | 702 исходные текстовые версии двух реестров и 41 дополнительная версия planning |
| [evidence/SOURCE-SPANS.json](evidence/SOURCE-SPANS.json) | Девять повторно полученных ключевых source spans с commit, blob SHA и строками |
| [evidence/HISTORICAL-SOURCES.md](evidence/HISTORICAL-SOURCES.md) | Источники исторической проверки, включая адресную сверку соседних веток |

**Review: [PASS_WITH_LIMITS — SOURCE/DOCUMENT REVIEW](evidence/REVIEW.md).**

**Новые native эксперименты: NOT_RUN. Измеренное восстановление качества: NOT_MEASURED.** Выполнены исследование исходников, разбор архивных результатов, поиск прежних владельцев, проектирование опытов и review документов. Фактически выполненные локальные механические диагностики отдельно раскрыты в 05 и не используются как доказательство native обучения.

При будущей подготовке DPO внешняя открытая китайская модель создаёт обе стороны. CODEX Astra 6 ultracode выполняет делегированный человеческий выбор и отдельно независимую оценку качества. В этой сессии такие модельные вызовы не заявляются. Запрещённый к изменению каталог llm_mvp_core/docs/plans/replanning-possible-diffs использован только для чтения; публикация ограничена planning/possibleDifs.

