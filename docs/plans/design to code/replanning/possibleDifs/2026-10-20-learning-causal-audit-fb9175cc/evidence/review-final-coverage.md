# Bounded independent review — пользовательские контракты

**Пакет:** `planning/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-causal-audit-fb9175cc`.

**Результат:** содержательная структура, очередность и границы доказательства выдержаны. Перед публикацией нужны две конкретные поправки SiLU и явное раннее резервирование единственной попытки; ниже точные адреса. Это документные изменения нового пакета, без пересмотра защищённых карточек, запуска или нового исследования.

## Scope

Прочитаны только назначенные README,01,03,04,05,06, две новые карточки; JSON dispatcher разобран целиком структурно, его очередь/переходы проверены. Выполнена проверка35 относительных Markdown ссылок из этих8файлов и двух относительных card paths JSON. Полный corpus повторно не сканировался. Native API/сборка новой source-copy не исполнялись и повторно не аттестовались. Saved raw не пересчитывались в этом bounded review.

## Исправления

### FC-01 — SiLU EXAM используется как admission, хотя это запрещено собственным контрактом

**Приоритет: material method/learning-unblock conflict.**

- `cards/SILU-SIGMOID-REUSE-01.md:65`: EXAM не входит в checkpoint selection.
- `:103` и embedded prompt `:251`: S0 принимается только при хотя бы одном correct TRAIN **и одном correct EXAM**.

Даже при одном заранее фиксированном W400 EXAM становится условием допуска A/B. Это также может заблокировать полезное resource A/B при действительном ненулевом TRAIN и нулевом EXAM — без необходимости для проверяемой гипотезы SiLU. Оно не требуется пользовательским правилом «любое полезное изменение — успех своей метрики».

**Минимальное исправление:** S0 пригоден при ненулевом correct TRAIN, конечных values, действительных изменениях trainable weights и optimizer step400. EXAM измеряется по прежней панели и бюджету, целиком сохраняется, но не участвует в admission/checkpoint selection и не меняет400steps. Исправить одновременно основной пункт и prompt. Исторические1/2 EXAM остаются описанием evidence, а не gate. Никаких новых readouts или запуска для этого не нужно.

### FC-02 — два несовместимых назначения result/S0 paths в SiLU

**Приоритет: material executor ambiguity.**

- `cards/SILU-SIGMOID-REUSE-01.md:174`: run ID `SILU01-R1`, `results/SILU01-R1`, `s0/SILU01-R1`.
- `:237`: `s0/weights.bin`, `s0/optimizer.bin`, `s0/manifest.json`; `results/status.json`, `raw.json`, `per-row.csv`, `costs.csv`, `report.md`.
- `:255`: prompt снова ссылается на неопределённое сокращённое `results`.

Исполнитель должен сам решить, являются ли последние пути сокращением или альтернативным назначением. Это нарушает требование одного точного места без самостоятельного выбора.

**Минимальное исправление:** задать один раз `R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01/results/SILU01-R1`, `S=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01/s0/SILU01-R1`; все result-файлы писать как `R/<name>`, donor-файлы как `S/<name>`. Согласовать174/237/255, не менять run ID и бюджет.

### FC-03 — явно указать резервирование единственной попытки до common preparation

**Приоритет: bounded execution-contract gap.**

SiLU `:172–180` запрещает overwrite и второй run ID, однако не определяет, когда единственный owner резервирует попытку относительно400common preparation updates. `:168` начинает Program с W0 и400updates. Позднее создание result-файла не защищает уже потраченный учебный бюджет при повторном запуске после partial failure.

**Минимальное дополнение к Run.ps1 contract:** до выдачи Program/любой W0/model/Train работы проверить существующие R/S/claim, атомарно создать постоянный `SILU01/execution.claim` через CreateNew для `SILU01-R1`; marker сохраняется при failure/STOP. Один script владеет всей последовательностью, дочерний Program не резервирует тот же claim вторично. Existing claim/partial → известный terminal status без новой попытки. Это одна ясная операция защиты бюджета, не новые hash-review/CI/production gates.

## Проверенные части, не требующие нового цикла

1. **Dispatcher корректен:**42 core +5 proposals =47 уникальных ID;9 consumed и38 remaining. Объединение completed и remaining равно всем47, пересечение пусто. Все38 transitions соответствуют единственной указанной последовательности, начинаются N63C и завершаются END. A1–A3 сохранены, A4/A5 не выданы за core43/44. Consumers41/42 в конце. Single-ID END и live STOP/NEXT заданы.
2. **N63C report anchor разрешается:** `03-card-contract-audit.md#n63c-report-table` существует. Таблица содержит C/T/E/N/M и NOT_MEASURED; прямо указаны0дополнительных forwards и36 прежних score calls.
3. **169/171 описано честно:**06 содержит самостоятельные правила для №08/09, не меняет historical marker count и не объявляет169semantic PASS. Нулевые response результаты не обнуляют полезные admission/NLL/retention observations.
4. **Learning paths присутствуют в обоих новых опытах:** SiLU внутри настоящих SFT updates, HNSW внутри native Unit.Train/Add. HNSW явно отличает Train4→Train8 от полного checkpoint continuation; gold не подменяет model output. Экспериментальные адаптации перечислены, production не меняется.
5. **Бюджеты согласованы арифметически:** SiLU400+4+4=408updates и24readouts,8568shiftedtargets; HNSW4Train/24presentations/96Execute. Никакие сохранённые400/4600steps не выдаются за бесплатный новый budget. No loop-until-PASS.
6. **Model/roles/report/STOP:** обе карточки и embedded prompts используют default текущей сессии, назначенный native learner, русский отчёт, конкретные skills, все useful gains с регрессиями, запрет product transfer до решения пользователя. DPO teacher/quality роли обозначены, фактические external calls0. Запрет продолжать прошлую задачу после NEXT/STOP присутствует локально.
7. **Нет вымышленного RUN_READY:** оба новых host обозначены AUTHOR_SETUP_REQUIRED и команды даны как будущий контракт. Отсутствие сейчас исполняемых файлов не скрыто как успешная сборка. Этот review не превращает их в runtime-certified.
8. **Private raw restore отделён от native parent adoption:**03 не блокирует все научные копии ожиданием production SFT-parent schema. `Step1/parent=null` не используется как достаточное доказательство необученности.

## Ссылки и мелкие замечания

- Из35 relative Markdown ссылок33 разрешаются. Две ссылки пока ведут в отсутствующий `09-publication-review.md`: README:47 и05:62. Это ожидаемый финальный файл; создать его перед публикацией или убрать ссылки, если он не планируется. Ссылки на обе новые card paths из JSON разрешаются.
- В03:21 сохранён условный расчёт45/36 и фраза «основной автор пересчитает»; при готовом04=47/38 её лучше заменить ссылкой на окончательный реестр. Сейчас это условное историческое пояснение, не ошибка actual JSON counts.
- В03:45 упомянут scratch-only `audit-cache/coverage/build-review-inventory.py`, отсутствующий в опубликованном пакете. Если обещается доступная воспроизводимость через этот script, скопировать его в data и поправить путь; иначе явно обозначить внутренний инструмент и оставить воспроизводимые данные/метод. Это не broken clickable link и не причина запускать повторный inventory.

## Закрытие

Достаточен один адресный readback изменённых строк FC-01/02/03 и наличие09. Повторный corpus scan, новые source hypotheses, runtime/tests и новый полный review не назначаются. Все старые experiments/raw/protected files остаются неизменными.
