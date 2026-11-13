# Короткие доменные эксперименты: восстановление прежде новых исправлений

## Что уже исполнено

В этом пакете исполнены две формульные Python-пробы и пересчёт первичных архивных JSON/JSONL. **Ни один эксперимент с обучением/генерацией ниже ещё не запускался.** .NET в окружении отсутствует; доступность настоящих checkpoint payload и внешней teacher-конфигурации не предполагается. Скрипты и полные результаты находятся в [scripts](scripts) и [evidence](evidence). Эти пробы удостоверяют конкретный механизм, но не его полезность для продукта.

## 1. Одна причинная панель для всех получателей

**P** — замороженное исходное состояние; **C0** — тот же parent, прошедший тот же load/eval цикл без update; **T** — состояние с одним проверяемым изменением. P и C0 различают дрейф/побочный эффект восстановления от learning. В опыт с representation A/B входят те же frozen material и target bytes, а индексы строятся соответствующим вариантом encoder. Иначе измеряется несовместимость старых ключей с новым запросом.

Три разных вопроса фиксируются раздельно:

1. **Механизм активен?** Нужный сигнал пережил DPO projection/encoding и достиг state/consumer; tracer показывает изменение выбранного объекта.
2. **Обычный ответ полезнее?** Истинный caller даёт более правильный ответ на заранее отделённых source families. Teacher-forced margin, local feature-fit и accuracy classifier записываются отдельно.
3. **Результат сохраняется?** Новый процесс загружает тот же accepted state и воспроизводит решение; старые задачи не ухудшились при заданном пороге.

Если первый вопрос отрицательный, увеличение числа DPO не проверяет заявленную гипотезу. Если первый положительный, а второй нет, результат — «механизм воспроизведён, влияние на качество не установлено». Если P и C0 различаются, сначала исправляется контроль; learning verdict становится INVALID_EVIDENCE.

Для пилота достаточно заранее заморозить 8–20 независимых evaluation cases и 4–8 контрпримеров/старых задач. Это **размер диагностического пилота**, не доказательство общего 95% качества. Показать каждый changed correct/wrong answer и paired win/loss/tie; при малом n не объявлять статистическую уверенность по одному красивому проценту. Первый neural pilot — 1–4 update на 4–8 новых разрешённых pairs, а не новый DPO500. Повтор другого seed или больше данных нужен только для конкретной оставшейся неопределённости.

## 2. Сначала вернуть подтверждённые механизмы clean

Полные первичные ссылки — [глава 00](00-DESIGNTOCODE-CLEAN-RESTORATION-FIRST.md). Восстановительные R0–R4 — **KNOWN, REVALIDATED_ON_CLEAN**, не новые открытия этого пакета.

| Приоритет / область | Один фактор и материал | Контроль и конечная мера | Фальсификатор / решение |
|---|---|---|---|
| **R0-SNAPSHOT — CodeTrace scorer / состояние** | Получить policy019 и reference из настоящих LFS payload, exact config и hash-word input contract; без обучения пересчитать старые 50 heldout | До/после cold load: selected logits, relative/absolute ordering, same state; отдельно 20 новых source-grounded задач | Pointer вместо payload или другой tokenizer/recipient: restore NOT_PROVEN. Совпавший pair score без обычного consumer ограничивает claim scorer-ом |
| **DC-01b-R — совместимость clean improver state** | Пять otherwise-valid checkpoint cases: known, underscore, hyphen, оба alias, запрещённый key; без обучения, только узкая canonical migration | Exact import verdict, отсутствие частичной mutation, сохранность остальных weights и ordinary load; не ослаблять whitelist | Native/реальный архив пока NOT_RUN. Если alias создаёт duplicate или полезное state не принимается, migration неполна |
| **R0-INPUT — доставка старой способности в consumer** | Один сохранённый state, новый input trace через реальный caller; менять только подтверждённую потерю поля/выбора | 8–12 запросов, которым нужна эта способность; learned-disabled control; native handler и состояние в trace | Helper отвечает, но ordinary caller его не использует: сначала маршрут. Новый fallback нельзя считать эффектом исходной модели |
| **R0-LEARN — continuation против несовместимого начала** | После установления identity — те же 4–8 новых DPO, один neural recipient; восстановленный совместимый обученный parent и fresh start как отдельный contrast | Dropout0, objective/masks/reference policy/бюджет фиксированы; новые code-law ответы и старые задачи после load | Улучшилось только relative ranking или поменялись одновременно representation и objective: не восстановлена semantic способность |
| **R1 / E25 — порядок ролей в RU→EN** | 16 разрешённых train случаев и 16 heldout, где те же слова имеют другой порядок; менять только order feature в одной памяти | Pattern classification отдельно; ordinary «кто сделал что кому», отрицание; мешок слов — контроль | Рост POS accuracy без исправления перевода ограничивается classifier. Синтетические POS templates не доказывают язык |
| **R2 / E26 — значение слова по двустороннему контексту** | 12 новых lessons и 12 unseen форм из двух предметных страт; при тех же targets добавить только правый POS/context | Пустая/обученная память раздельно от BuildDefault; unknown-context и противоположное значение как controls | Ответ существовал в fixed dictionary; либо wrong-specific answers выросли. Старые 15/15 — compatibility, не learning proof |
| **R3 / E09 — AST внутри KNN для C#** | 8–12 source files; один codec, file-disjoint split и одинаковые eval transitions; один KNN ± AST | Те же target IDs, k и search budget. Paired selected neighbors, затем source-grounded объяснение/trace | Выигрыш исчез после выравнивания denominator/labels; helper-only retrieval не улучшает ответ. Не переносить на прозу |
| **R4 / E43 — сохранение старой памяти при новых знаниях** | Малый old/new corpus из C# и переводческой лексики; сохранение old memory vs замена только при том же new material | Old retention и new acquisition отдельно, равный объём/шаги; cold load и ordinary answers | Старый результат был bigram rebuild. Он не оправдывает neural replay. Повтор старых DPO требует отдельного допустимого режима, не новых ID |

R0-LEARN не означает обязательное обучение tiny hash-word wiki scorer свободной генерации: его исходный contract может позволять только ранжирование. Для проверки neural Student задаётся отдельный совместимый model/tokenizer и измерение его собственного ответа. Случайная B0, восстановленный archive scorer и trained KNN не являются тремя взаимозаменяемыми checkpoint одного потребителя.

### Если надо проверить именно «в матрице не помогало, в KNN помогло»

Исторические E07/E09 сохраняются как два разных опыта. Для причинного ответа нужен 2×2: table/n-gram без и с одним структурным признаком; KNN без и с **тем же** признаком. Во всех четырёх — общие labels, train/source split, eval transitions и downstream metric. Если бюджет только на один малый запуск, сначала KNN ± AST для C#: это проверяет актуальную полезность функции, но не реконструирует причину неудачи старой матрицы. Порядок предложения в RU→EN проверяется отдельным R1, а не копированием C# SyntaxKind в текстовый encoder.

## 3. Затем новые механизмы DC/KU

KU-01…03, DC-01a и selection DC-02/03 **INHERITED**; DC-01b включает current whitelist/import несовместимость. DC-02/03 уже описаны параллельным пакетом54 и здесь не считаются новыми. До их ремонта нужна демонстрация, что соответствующий доменный consumer участвует в проваленном сценарии. Для raw neural RP01 без такого consumer они могут быть вообще не активны.

| Опыт | Минимальный контраст | Размер пилота и обязательный контроль | Измеряемый эффект |
|---|---|---|---|
| **DC-01-A / дизайн** | 2×2 skip-absent × canonical-preservation, без других parser изменений | 8–12 schema cases, затем 4–6 admitted lessons. Полностью заполненные known fields; exact canonical; metadata отдельно | Source categories→allowed deltas→local fit→restored weights. Render только при изменившемся ordinary selection |
| **DC-02-A / дизайн** | Только all-negative fallback учитывает отрицательное наблюдение в том же контексте | 6 harmful-spacing cases + 4 positive/unknown controls; тот же QA и renderer | Исправленные/новые defects, selected rules, два viewport; global negative veto не предполагается |
| **DC-03-A / дизайн** | Только cardinality заранее совместимых co-required corrections | 6 двухдефектных layouts + 3 single + 3 conflicting alternatives | Оба дефекта за один проход и за одинаковый полный бюджет; время/число render; не применять все rules вслепую |
| **KU-01-A / технический RU→EN** | Concept lookup по исходной cleaned surface до потери suffix; остальные POS/tags фиксированы | 6–9 train rows и 12 source-disjoint запросов; frozen chosen/WordMappings/PhraseMappings; early paths не отключать | Token-concept, KNN-gate, полезный neighbor, semantic correct/wrong accepted/Bootstrap |
| **KU-02-A / бытовой и технический RU→EN** | Убрать только ошибочный PAST у независимо размеченных nonpast, без новых future/present features | 6 train, 12 unseen tense cases + 4 infinitive/imperative controls; past controls неизменны | Ложный PAST и bucket purity; правильное время в реальном переводе. Gold tense — диагностическая верхняя граница, не готовый runtime classifier |
| **KU-03-A / сообщения RU→EN** | Для gender использовать ту же cleaned surface, что для POS | 8 новых source families × без/с final period; train исключает эти строки; true она→он control | Gender/signature invariance, путь ответа и semantic correctness обеих половин, wrong accepts отдельно |

Полные source paths, вмешательства и фальсификаторы — [01](01-NEW-DESIGN-CORRECTION-SEMANTICS.md) и [02](02-NEW-TRANSLATION-REPRESENTATION-GAPS.md). Переобучение новых WordMappings одновременно с новым encoder превращает KU-01 в составное вмешательство. Если это нужно для итогового продукта, сначала раздельно измерить KNN-address и word-map mediators, а затем совместное изменение; выигрыш не присваивать одному KNN без трассы.

## 4. Правило материала, оценки и стоимости

Внешняя открытая китайская модель создаёт обе финальные стороны настоящей DPO. CODEX на **Astra 6 ultracode** выбирает учебный случай в human-role и отдельно оценивает exact final pair; он не переписывает chosen/rejected под чужим provenance. При запуске записываются фактический model ID/version, request/response receipt, доступный режим и hashes, а не предполагается, что имя роли равно уже сделанному вызову. Проектный источник: [RESEARCH-03, current conformance](../../../09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md).

Разделение train/eval делается до teacher authoring по source family. Heldout ответы не попадают в обучение. One-pair application и source identity сохраняются: нельзя повтор старого урока скрыть новым ID. Исторический multi-epoch replay и E43 bigram память описывают старые режимы, а не отменяют правила нынешнего pipeline. Для воспроизведения source формул механические fixtures разрешены как диагностика и не выдаются за teacher-authored материал.

Независимый оценщик получает исходную задачу, reference/rubric и ответ без метки P/C0/T. В коде проверяется смысл операторов/веток и отсутствие вымышленных calls; в переводе — полнота, роли, отрицание и время; в дизайне — реальный render/поведение на согласованных viewport. Процент evaluator-а не заменяет сохранённые raw ответы и объяснение спорных случаев.

Для каждого плеча записать wall time, update count, tokens и teacher/judge attempts, RAM/VRAM peak если измеримы, размер state/index и время load. Приведённые размеры пилотов — план; новые временные измерения этого пакета не выдумываются. Если выполнение требует дорогого расширения, сначала вернуть результат малой панели и точную оставшуюся неопределённость. Общий Windows CI/coverage не является мерой полезности обучения.

## 5. Критерий решения

**IMPROVE** — заранее определённая предметная мера улучшилась на новых families, контроль C0 стабилен, старые задачи не превысили допустимую регрессию, эффект использован обычным consumer и сохранился после load. Для пилота допустим ограниченный вывод о конкретных случаях, без общего product-ready.

**REGRESS** — вмешательство добавило смысловые/визуальные ошибки или потеряло старый навык; сохраняется parent. **INCONCLUSIVE** — механизм изменился, но полезность неотличима или затенена другим путём; сохраняется parent и описывается следующий один различающий случай. **INVALID_EVIDENCE** — changed target, leakage, несовместимый tokenizer/checkpoint, не тот consumer, self-grading или отсутствующий payload сделали сравнение недостоверным; это не доказательство плохого обучения.

Содержимое квитанции: run/ref/model/tokenizer/config identity; frozen dataset and source-family split; P/C0/T state hashes; фактический recipient/handler/consumer trace; ровно один changed factor; raw outputs и все independent grades; old/new results; cold-load observations; время/ресурсы; verdict и непроверенная граница. Результат без этих конкретных наблюдений не переименовывается в PASS по количеству rows или изменённых тензоров.
