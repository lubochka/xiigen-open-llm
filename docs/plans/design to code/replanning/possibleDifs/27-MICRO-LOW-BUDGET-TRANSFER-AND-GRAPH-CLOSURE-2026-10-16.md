# MICRO-N27 — минимальные причинные опыты для E48 low-budget transfer и E50 false closure

<!-- XIIGEN-AUDIT-2026-11-11-INSERT -->
<!-- AUDIT-2026-11-11-EV-P50:START -->
## Уточнение допуска и завершения будущего runner — 11 ноября 2026

Проверка последующего P50 на core `009bf226174441fa0fbf0336e74ad2f2b7174d92` выявила переносимую ошибку измерителя: `P5001/attempt-0002/Program.cs` учитывает guardProblems в execution label, но допускает positive learning/hypothesis и exit0 при нарушенном бюджете. Сохранённый здоровый результат A2/4,B4/4 не отменяется. Это не доказательство ошибки E50-STATIC-01: здесь усиливается будущий runner contract, а механизмы остаются раздельными.

В «Measurements and stop» и «Minimal implementer contract» ниже обязательный guard должен управлять единым решением, сериализованным status и внешним exit. Negative: при прежних before2/4,after4/4,ceiling2/4 заменить только учёт дозы8 на9 — результат invalid/open и ненулевой exit. Здоровый отрицательный научный исход остаётся completed. Отдельный reader BPE/HNSW не удостоверяет native Unit restore без actual Import/Execute. Подробности — [дополнение11.11](2026-11-11-P48-P58-EXPERIMENT-MEASUREMENT-ADDENDUM.md); source review не является native mutation run и не назначает новый Train.
<!-- AUDIT-2026-11-11-EV-P50:END -->


**Дата:** 2026-10-16. **Артефакт:** исполнимый план, NOT_EXECUTED; никакой новый Student, optimizer, Windows CI или production patch этим документом не запускался. **Строгая изоляция:** E48 warm-state learning quality и E50 graph-shape shadow — разные гипотезы, не совместный A/B.

## Эксперимент A: E48-SMALL-01 — действительно ли prelearned checkpoint улучшает быстрый перенос?

### Проверяемая причинность и сравнимость

H: при небольшом target-budget реальное source pretraining с persist/reopen, а затем fine-tune, повышает качество обычного consumer против fresh base. Ни E48 archive, ни changed LoRA tensors не доказывают H. Площадка: выбрать **один** домен, в котором уже есть зарегистрированный обучаемый Student или typed learned Unit и runnable ordinary consumer. Для первого дешёвого опыта предпочтительнее 8–16 коротких независимых cases с очевидным law; если потребитель отсутствует, статус BLOCKED_CONSUMER, не имитация success через training CLI.

### Data freeze и распределение

- TRAIN: 8–16 исходных независимых задач/семейств из approved public domain corpus, source-tagged, одна задача может дать несколько paraphrases, но family count остаётся единым. Не брать SEALED gold из RP01, не корректировать historical E48 input.
- DEV: 4 независимые families для оценки формата/технических ограничений; никогда не использовать для подбора checkpoint после вскрытия holdout.
- HELDOUT: 12 иных source/family tasks; включить обычные, отрицательные/направленные/структурно сложные, метка ground truth от допустимого компилятора или заранее запечатанная semantic law rubric.
- Один dataset SHA/seed/tokenizer SHA/snapshot source+target/rubric; target outputs из TRAIN не входят в DEV/HELDOUT. Одинаковые текстовые input, fresh KV cache и генеративные параметры для всех arms.

### Arms, одна переменная

| Arm | Начальное состояние | Target обучение | Purpose |
|---|---|---|---|
| A | Замороженный B0, обычная инициализация | 2/4/8 target steps | fresh learning |
| B | ТОТ ЖЕ B0 → approved source pretrain → immutable saved checkpoint → reopen | те же 2/4/8 target steps | actual persisted warm transfer |
| C | B0 → предобучение на том же source-budget, но с явно перемешанными допустимыми source-side labels; сохранить/reopen | те же 2/4/8 target steps | отрицательный контроль «больше compute» |

У B/C source-update budget одинаков; у A нет source pretraining и это явно отразить как total-updates/wall-time difference. Для чистой равной стоимости добавить optional A2 с тем же числом extra train steps на neutral same-domain source, не переобозначать его обязательным baseline. Не выбирать source donor с помощью HELDOUT. **Сначала** проверить bytes/topology/tokenizer/LoRA/load/hash и reproducibility: logits before-save vs fresh reopen в предзаданной FP tolerance, optimizer/RNG continuation state отдельно. Провал parity → STOP_STATE_ERROR, quality arm не сравнивать.

### Measurements and stop

Primary — paired semantic-clause success Q_law обычного consumer на 12 independent families, одинаковые заранее определённые критические semantic clauses; case critical contradiction=0, считать critical count и abstention/coverage отдельно. Secondary — first-token logits, preference margin на отдельном DEV, wall-seconds per useful improvement, target sample-efficiency @2/@4/@8. Таблица case-by-case B−A, C−A, number wins/losses; не декларировать PASSED по одной разнице после вскрытия. FAIL/INCONCLUSIVE при нет consumer, invalid source/state, или gain исчезает против C. Осознанный режим исследования может остановиться после 2/4 target updates при бессвязном decode, не требуя 500 DPO. Не выдавать superiority на 12 задачах за статистическую значимость или покрытие всех доменов.

### Minimal implementer contract

Воспользоваться существующими lifecycle APIs, сделать изолированный read-only? Для запуска требуется отдельная явная execution-authority. Новые training artifacts должны создаваться исключительно в локальной экспериментальной папке, не изменяя исторический checkpoint. Нужен runner, который:
1. Закрепляет source SHA, executable version/commit, approved domains, tokenizer/model/base fingerprints, frozen suites.
2. Создаёт три arm и save/reopen для B/C, проверяет expected state identity; повторяет normal request через same consumer, не через teacher-forced score-only helper.
3. Записывает raw answers и grade evidence без пересоздания gold; строгие CODEX Astra 6 UltraCode judge/author роли ниже.
4. Публикует результат как EXPERIMENTAL_SIGNAL или NO_SIGNAL/INVALID, плюс rejected/failure receipts.

Не переписывать RP01 train500, не менять LoRA и KNN в том же опыте, не использовать Windows CI как prerequisite научной проверки.

## Эксперимент B: E50-STATIC-01 — доказательство только логической непригодности раннего closure

Вход **ровно сохранённый report**, не новая модель: [run E50 May01](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E50-DOMAIN-GRAPH-CLOSURE/results/run-2026-05-01-001.json). Предзарегистрировать критерий: finalClosure означает последний delta < 0.05; robustClosure означает последние 2 delta < 0.05; earlyObserved означает когда-либо delta <0.05. Historical sequence из notes: D3=0,D4=1.00,D5≈.33,D6≈.43,D7≈.09,D8=.20,D9=.18,D10≈.07. Вывод арифметической проверки: earlyObserved=true, finalClosure=false, robustClosure=false. Числа из округлённых notes не заменяют полный raw graph; любое совпадение на shadow — формальное, не native proof.

Контроль false-positive: последовательность [0.00,1.00,0.20] НЕ проходит final/robust. Контроль positive: [0.20,0.02,0.01] проходит final/robust. Фальсификатор N27-B1: найти действующий обычный call path, который реально использует E50 stop criterion, а затем native experiment с онлайновым stop и replay после следующих двух появлений новых nodes. Пока такой call path не найден, quality-causality status = NOT_INVOKED_OR_UNKNOWN.

## Дисциплина ролей / порядок проверки

**Human-in-the-loop роль выполняет CODEX (модель Astra 6 UltraCode)**: выбирает TRAIN, фиксирует eligibility, domain laws, отвечает за blind оценку правильности DPO пары и отдельно Student outputs. **Автор обеих сторон chosen и rejected реального DPO — ВНЕШНЯЯ разрешённая открытая китайская модель** с конкретной лицензией/version/генерационным provenance, а не CODEX, не локальный Student и не исторический deterministic simulation author. CODEX Astra judge не получает SEALED gold во время выбора TRAIN. Для эксперимента E48, где DPO не требуется, китайскую модель не запускать ради галочки. Никаких вымышленных вызовов CODEX, Chinese teacher и native runs: протокол является планом, а не выполненной научной проверкой.

## План проверки планирования и приёмки

Архитектура: 1 обучаемый state, 1 consumer, доменная функция; source→update→save→load→normal output. Code review: exact loader route и одни и те же параметры исполнения для arms; проверка градиента vs state change при необходимости. Plan review: контроль отрицательной ветки C, независимость family split, сохранность sealed, time cost, отсутствие замены quality на tests. Prompt engineering: заранее объявленные semantic clauses, тексты примеров и нейтральные grading instructions. Human communication: показ в отчёте «что выучилось на каких примерах», failed cases, ограничения. **Go/no-go:** изменение raw weights и даже heldout margin без полезного ordinary output — NO_VERIFIED_QUALITY_GAIN; успешный shadow E50 не объявлять восстановлением RP01.
