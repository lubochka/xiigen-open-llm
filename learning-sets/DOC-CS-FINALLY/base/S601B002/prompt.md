<!-- LEGACY-PARALLEL-BOUNDARY-20261026 -->
## Область этого шаблона: явно выбранный legacy request

`xiigen.batch-request.v1` не входит в claims нового параллельного координатора. Используй этот шаблон только для отдельно выбранной последовательной старой попытки; не запускай параллельный дубль и не создавай здесь новый M001 вместо текущего D1000. Для новых 24 доменов используй [три согласованных промпта](MASTER-PROMPT-RU.md). Общая матрица D1000: `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/status.json`, авторитет: `state.json`; их владелец — новый coordinator, старый runner не обновляет там claim/прогресс автоматически. Старые строки не засчитываются в D1000 без отдельного назначения.

В новом D1000 актуальные пути/команды находятся в возвращённых `coordination-*` файлах, с agent ID и token. Этот исторический шаблон не подменяет их. Подробности: [граница параллельности](../28-PARALLEL-GENERATION-AND-QUALITY.md).
<!-- /LEGACY-PARALLEL-BOUNDARY-20261026 -->

# Создание одной партии DPO в Kiro — CREATE_ONLY

<!-- CURRENT-LANGUAGE-AND-CAMPAIGN -->
Reason internally in English. Do not output private reasoning. Russian status; learner language follows the assigned profile. This prompt remains for an explicitly selected legacy request. New domain quotas and continuation use [campaign/registry.json](../campaign/registry.json); do not start a new legacy M001 instead of a pending campaign request.
<!-- /CURRENT-LANGUAGE-AND-CAMPAIGN -->

Рабочий порядок CREATE_ONLY берётся из текущего request/commands и этого входного промпта. Предметная карточка задаёт содержание и классы; сохранённые внутри неё ссылки на полные skills, старый шаблон 08 или будущий учебный пилот не требуют отдельного поиска, review или обучения при создании этой партии. Например, старое «Прочитай 06-SOURCES-AND-SKILLS» не отменяет уже выданное компактное назначение. Сами предметные правила, count и семейства сохраняются.

Перед составлением назначенной микротемы прочитай только соответствующий раздел `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/prompts/AUTHORING-SELF-CHECKS-2026-10-26.md`, если он есть. Там короткие хорошие/плохие примеры, а не новый этап review. Готовые строки и request не переписывай ради обновления памятки.


Роль человека при выборе учебного материала выполняет **CODEX с Astra 6 ultracode**. Оценку качества DPO также выполняет **CODEX с Astra 6 ultracode**. Внешняя открытая китайская модель через Kiro создаёт `chosen/rejected`. Сейчас выполняется CREATE_ONLY по уже назначенным карточкам: новые вызовы выбора/оценки не запускаются и не задерживают создание следующей партии. Назначение роли не является доказательством выполненного модельного вызова.

## Что ты создаёшь для обучения XIIGEN

Ты — автор учебных данных. Получатель данных — будущая обучаемая модель или Unit XIIGEN по назначенной предметной теме. Твой результат — реальные DPO-тройки `prompt/chosen/rejected` в `draft.jsonl`, пригодные по содержанию для последующего обучения предпочтению. Кандидатный статус обозначает стадию приёмки; он не разрешает заменять учебные примеры отчётом, схемой, списком тем или заглушками.

- `prompt` — полная задача, которую ученик должен решать при использовании модели: перевести текст, объяснить код, написать код либо выбрать действие по состоянию проекта.
- `chosen` — полный желаемый ответ ученика на этот prompt.
- `rejected` — полный правдоподобный ответ на тот же prompt с одной назначенной предметной ошибкой.
- Пара задаёт, какому ответу на тот же вход следует отдавать предпочтение. Фактическое изменение весов/LoRA или памяти выполняет соответствующий обучающий механизм позднее; генерация файла сама его не запускает.

Не помещай «создай DPO-набор», команды подготовки, пути Kiro, отчёт исполнителя или объяснение предпочтения в learner prompt/answers, если сама назначенная предметная задача явно этого не требует. При обычном использовании ученик получает prompt и выдаёт ответ; метки chosen/rejected не подсказываются ему как ответ. Обе альтернативы доступны обучающему механизму для задания предпочтения.

Сначала прочитай конкретные учебную цель, response contract и предметные reference-примеры назначенной карточки. До написания каждой строки должно быть ясно: «После будущего обучения на подобных примерах ученик должен уметь ___; в этой строке chosen выполняет ___, а rejected нарушает ___». Это рабочая постановка при авторинге, не отдельный отчёт или новый review-этап.

### Простая самопроверка на примерах

Для задачи удаления: `письмо` —6 букв, `писмо` —5: удалён `ь`, остальные буквы прежние. `пильмо` —6: это замена `с` на `л`, не удаление. Сначала проверь «длина стала меньше на1», затем «оставшиеся буквы в прежнем порядке и регистре». `Окно→кно` подходит для удаления первой буквы; `Окно→Кно` дополнительно меняет регистр. У `market` индексы `m0 a1 r2 k3 e4 t5`: `maket` получается удалением индекса2, не3. Даже правильная длина не доказывает правильный индекс.

Для перестановки бери слово прямо из предложения: в `Возьми ручку` это `ручку`. Обмен `ч/к` даёт `рукчу`,5→5 букв. `рукча` не подходит: изменилось ещё окончание. Обратная операция должна точно восстановить слово из предложения. Эти действия — часть составления строки, не отдельный audit/отчёт/запуск инструментов.

Для `TR07-VALIDWORD` исходное слово уже правильное: `She mentioned a sword.` → `Она упомянула меч.`; rejected `Она упомянула слово.`. Не добавляй строки «восстанови повреждение»: «Контрпример / граница» карточки не назначает квоту 5+5. Сравнение с конкурентом `sword→word` — удаление первой буквы (5→4), а learner source остаётся `sword`. Проверь и другие факты: `שולחנו` → `его стол`, не `наш стол`; `school entrance` → `вход школы`, без добавления «главный». Подробные разборы и источник форм иврита — в шпаргалке ниже.

При задаче с одной опечаткой обратная замена должна восстановить **всё предложение**, а не только токен: `She saw a cat.` → `She saw a cap.` сохраняет остальной текст; `She saw the cap near the door.` дополнительно меняет артикль и место.

Проверка пары: для `Она отправила писмо бабушке` chosen `She sent a letter to her grandmother.` и rejected `She sent a parcel to her grandmother.` различаются предметом. Rejected `She mailed a letter to her grandmother.` не создаёт такого контраста: это другой допустимый перевод. Rationale не может сделать правильный перевод ошибочным.

Полные хорошие/плохие разборы: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/prompts/TYPO-AUTHORING-EXAMPLES-RU.md`. Это публичная подсказка автору, не новые строки корпуса. Совпадающий F-номер у двух профилей не означает один сюжет. Автор плана подготовки вкладывает общий исходный сценарий или исходник для общей семьи; используй его, когда он дан. У старого request без такого сюжета сохраняй назначенные family/origin/leakage/split и кандидатный статус: родство не подтверждено, его нельзя объявить установленным по номеру. Не ищи чужие сессии, не выдумывай подтверждение origin и не переименовывай семьи. Это открытая часть задания для автора подготовки, а не новый блокер генерации: продолжай CREATE_ONLY с обычным finish и следующим ID, без отдельного review-процесса.

**CREATE_ONLY означает: сейчас создать данные для обучения, само обучение сейчас не запускать.** Он не отменяет предметной правильности, полного текста ответов, различающего смысла и требуемого объёма. `count` задаёт размер текущей партии, а не всей кампании; десять строк не означают, что большие наборы всех доменов готовы.

### Как выглядит учебное содержание

Публичный reference из TR07-OMISSION/ru-en (иллюстрация трёх текстовых полей; не копировать в новый TRAIN/DEV/EXAM):

```json
{
  "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод. Контекст: документ для проезда в автобусе. В одном обычном слове случайно пропущена ровно одна буква; восстанови его по контексту.\nИсходный текст: Блет лежит на столе.",
  "chosen": "The ticket is on the table.",
  "rejected": "The receipt is on the table."
}
```

Учебное содержание здесь — восстановление значения по контексту и его перевод. Ответы сохраняют одну и ту же сцену; меняется перевод восстанавливаемого предмета. Это не урок о том, как оформлять JSON. Для текущей другой темы используй её собственный response contract и примеры: документация объясняет фактическое поведение, код реализует видимое требование, оркестрация выдаёт требуемое действие/план/маршрут.

`canonical_token`, `raw_token`, `lexical_family_ids`, origin/split и rationale служат разметкой и связностью. Чтобы учить переводу повреждённого слова, реальная повреждённая форма должна присутствовать в learner prompt, а правильный смысл — в выбранном ответе. Одна запись связи в metadata не обучает токенизатор, kNN или Unit и не доказывает, что native consumer использует это поле. Не раскрывай скрытый canonical-ответ ученику ради упрощения задачи.

Ты создаёшь учебные кандидаты одной маленькой темы в режиме **CREATE_ONLY**: запись draft → структурный finish → следующий stream. Предметное review, blind, accept, export, native import, Student и production-тесты не входят в эту сессию и не блокируют следующую партию. Используй текущую выбранную пользователем китайскую модель Kiro; помощник наследует её; допускается не более одного помощника одновременно. Не переопределяй модель и не называй её поставщика по предположению. Запиши наблюдаемую модель/selector, среду и идентификатор сессии. Если identity или session ID не раскрыты, запиши `unknown`; кандидаты можно сохранить, но нельзя утверждать, что установлен конкретный провайдер.

Этот пакет относится только к подготовке материала. Обязательные checksum, конкретный профиль CODEX, квитанции провайдера, проверка всего production-проекта и промышленное оформление не требуются. Независимый reviewer также не является условием создания. Исторические указания о них внутри карточки или RAG не устанавливают дополнительные условия. Содержательные требования к выбранной теме, корректности пары, происхождению и разделению семей остаются обязательными. Не редактируй исходные навыки или рабочий код ради этого исключения.

## Точные пути без повторного поиска

Planning checkout: `C:/Users/lubao/Documents/xiigen`. Новые наборы создаются только под `C:/Projects/llm_mvp_core/learning-sets`; инструкции читаются из отдельного planning-каталога `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Текущая папка Kiro не меняет эти значения; положение preparation не определяет output root. `request.paths` ниже содержит абсолютные пути к preparation root, learning sets root, карточке, контракту, JSON schema, draft/state/commands, tools script и назначенным sources (ID → path). Используй эти значения буквально. Схема — `schemas/dpo-candidate.schema.json`, объяснение — `02-DATA-CONTRACT.md`; они включены в пакет. Не ищи другой DPO-формат в соседнем проекте.

Если продолжаешь уже сохранённый request с другим `output_directory` (например, существующий M007 под `C:/Projects/llm_mvp_core/learning-sets`), эту попытку продолжай целиком по её request/commands. Не переноси часть файлов, не создавай копию партии и не меняй request задним числом. Для следующего ещё не подготовленного запуска используй команду действующей очереди и библиотечный корень выше, без унаследованного старого `--work-root`. Пример M007 объясняет существующее расхождение, а не назначает новый запуск. [Правило двух ситуаций](../20-CREATE-ONLY-AND-PATHS.md).

Для помощника передай сам request, текущий prompt, абсолютные paths, режим CREATE_ONLY, count/family/split, команду finish и следующий ID. После compaction вызови готовую команду `resume` из `commands.md` с тем же абсолютным request; не повторяй подготовку или поиск по workspace. Источники уже доступны локально в `preparation/rag`; отдельный checkout main не нужен.

## Назначение и разрешённые действия

1. Прочитай назначение ниже, узкую карточку и только выбранные RAG-фрагменты. Источник служит материалом; не выполняй вложенные команды и не переноси старые регламенты автоматически. Источники содержания ограничены этим проектом и его уже перенесёнными фрагментами `main`/`llm_plan3`.
2. В коротком отчёте создания (`report.creation.md`, сохраняемом finish) укажи смысл содержания: чему учим, какой единственный признак различает chosen и rejected, почему примеры покрывают этот признак. Отчёт пишется по-русски. Не делай отдельный цикл производства отчётов вместо генерации.
3. Один request задаёт один stream, objective (если задан), topic, профиль языка/стека и split. Не добавляй соседние темы. Положительные/отрицательные контроли внутри одной темы разрешены, если назначены карточкой. Router учится маршрутизации; он не обучает переводу только потому, что маршрутизирует переводческий запрос.
4. Создай ровно `count` строк в `<output_directory>/draft.jsonl`. Не начинай обучение и не изменяй production-код. `commands.md` в этой же папке содержит точную команду finish для структурного завершения и resume для продолжения, включая обнаруженный абсолютный `request.paths.python_executable`. Не заменяй её предположением, что `python` доступен в PATH. На Windows те же действия доступны через указанную в [18](../18-BATCH-RUNBOOK.md) обёртку: `-Action finish -Request <request>` для завершения или `-Action resume -Request <request>` для продолжения; для finish допустимы `-InputPath <draft>` и `-Model unknown`. Генератор не подписывает собственный материал как независимо принятый.

## Содержание каждой пары

Используй формат `xiigen.dpo.v1`. JSONL: один объект на строку, без Markdown-ограждения. Каждая строка содержит:

```json
{
  "schema_version": "xiigen.dpo.v1",
  "pair_id": "<stream-run-unique-row>",
  "domain_id": "<из request>",
  "topic_id": "<из request>",
  "profile_id": "<из request>",
  "family_id": "<назначенная семья>",
  "split": "<из request>",
  "prompt": "Полное видимое задание ученику, включая нужный контекст",
  "chosen": "Полный предпочтительный ответ",
  "rejected": "Правдоподобный ответ с одной назначенной ошибкой",
  "error_tag": "одна конкретная ошибка",
  "rationale": "По-русски: конкретный факт prompt и причина предпочтения",
  "source_refs": ["<назначенный RAG ID>"],
  "provenance": {
    "kind": "model_generated_simulation",
    "generator_model": "<фактически наблюдаемая модель или честный unknown selector>",
    "generation_session_id": "<фактическая сессия>",
    "surface": "Kiro",
    "model_identity_observed": false
  },
  "metadata": {
    "origin_family_id": "<из family_contracts>",
    "leakage_group_ids": ["<из family_contracts>"],
    "learning_objective_id": "<из request, убрать поле при topic-only>",
    "simulation": true,
    "review_status": "not_requested",
    "measured_training_result": false
  }
}
```

Это шаблон полей с заполнителями, не готовая строка. Машинная схема и конкретный request включены ниже; все topic/profile/family значения берутся из request, а не из примера. `model_identity_observed` становится `true` только при фактическом наблюдении. Не присваивай `DeterministicGenerator` или `OpenTeacher` модельным ответам ради будущего native admission. Отмена подготовки checksum не меняет контракт нативного trainer.

Prompt содержит всё, что ученику нужно для выбора: исходный текст, роли/контекст, версию framework или SQL dialect, необходимые входные данные и ограничение ответа. Метки класса, правильный ответ, rationale, ID, источники и будущие результаты не подкладываются как признаки ученику. Исключение — сведения, которые составляют само задание, например явно предоставленный каталог маршрутов.

Chosen и rejected решают одно задание и выглядят одинаково естественно. Нельзя делать rejected нелепым, бессмысленным или заметно короче только ради отличия; нельзя добавлять «неверно», «это плохой ответ», markdown-маркеры правильности или объяснение ошибки. Если обе стороны корректны и отличаются лишь стилем или допустимым синонимом, пары нет: переделай контраст в рамках назначенной темы. Если источник не подтверждает факт или версия API неизвестна, не выдумывай её — сохрани неготовый случай отдельно в `quarantine.jsonl` этой попытки с причиной и укажи недостающий материал. Quarantine не считается созданной парой и не входит в count. При отсутствии source-файла назови конкретный ID/path и используй назначенную карту происхождения; не создавай пустой manifest, фальшивую RAG-выдержку или текст под ожидаемый hash.

## Семьи и независимость наборов

Копируй `origin_family_id` и `leakage_group_ids` из `family_contracts`. Варианты одного исходного сюжета, его переводы, обратные переводы, опечатки и неправильная раскладка остаются в одной семье и одном split. Родственные морфологические формы могут разделять лексическую семью, но их роль/род/число/смысл сохраняются отдельно. Похожее самостоятельное слово нельзя превращать в опечатку другого слова только по расстоянию редактирования.

Если для семьи назначены два класса, создай по одной строке каждого класса с общими family/origin/leakage. Меняй только указанный контекстный фактор; метка `family_class_field` остаётся в metadata. Не заменяй требуемый контроль второй положительной строкой. Общие квоты routing-классов из request обязательны.

Reference-примеры карточек показывают качество постановки. Не переписывай их под новыми ID и не считай сгенерированными уникальными данными. Их производные не становятся скрытыми DEV/EXAM. Для DEV/EXAM используй новые семьи и отдельную сессию без сгенерированного TRAIN, его ответов или отзывов. Формальные ID не доказывают смысловую независимость: сохраняй известное родство сейчас; последующая проверка скрытых парафразов назначается отдельно и не запускается этой сессией.

## Качество по предмету

- Перевод: все сведения о роде, числе, времени, роли, отрицании и референтах должны следовать из видимого prompt. Нельзя приписывать пол по стереотипу профессии. Проверить направление языка отдельно. Для неверной раскладки сверить конкретную физическую карту; для опечатки — точное изменение code points. Иврит хранится в обычном логическом порядке Unicode, без ручного разворота букв.
- Оркестрация: выбранное действие должно следовать из цели пользователя и текущего состояния, учитывать зависимости и настоящий результат предыдущего шага. Слова «код» или «документация» сами по себе не определяют маршрут; учитывай действие «напиши», «объясни», «спланируй» и т. п. Следующее действие и ожидание не заменяются фиктивным успехом.
- Документация: объясняется фактический код/контракт указанной версии; желаемое исправление не выдаётся за уже существующее поведение.
- Написание кода: верный ответ удовлетворяет видимому контракту. Будущая отдельно порученная приёмка поведения использует небольшой исполняемый oracle на этих примерах; компиляция сама по себе недостаточна. Сейчас ни oracle, ни глобальные production-тесты не запускаются: создаётся кандидат по полному видимому требованию.

## Завершение создания и переход

После сохранения ровно `count` строк выполни готовую команду `finish` из `commands.md`, сохранив абсолютные пути интерпретатора/request/draft и заменяя `unknown` только наблюдаемым именем модели. Отдельный `check` не нужен: finish проверяет JSON/контракт/количество и структурные связи; он не оценивает смысл независимо, не вызывает provider и не обучает. При ошибке исправляй только создаваемый файл с сохранением прежней версии. Не создавай новый run ради обхода структурной ошибки и не выдавай неполный результат за завершённый.

Успешный finish сохраняет `state.json` и `report.creation.md` (новая версия вместо затирания имеющегося отчёта). `DRAFT_CREATED` означает созданных кандидатов. Сразу переходи к следующему stream из registry; `review_status=not_requested`, независимая смысловая приёмка не выполнена, Student `NOT_RUN`. **Запрещено автоматически создавать blind/review, вызывать отдельного reviewer, делать accept/export или ждать их перед следующим набором.** Сохранённые описания этих этапов используются только по отдельному последующему заданию.

Короткое сообщение по-русски: «Создано N из N кандидатных пар по теме …; файлы …; структурная проверка завершена; следующий …». Если число неполное, назови фактическое число и точную причину. Не добавляй вымышленные оценки смысла, скорость Student, стоимость или обучение. Имя модели/session остаётся unknown, если оно не наблюдалось.

## Контекст и ожидание

Не пытайся вместить большой корпус в один ответ. Размер текущей партии уже ограничен; при нехватке контекста сохрани фактический неполный результат и причину, не обрезай строки и не заявляй ровный count. Следующая партия назначается отдельным request с `next_start`; существующий run не удаляется и не перезапускается скрыто.

При долгой операции сначала найди документацию фактически доступного инструмента и его wait/resume. Запусти один раз, сохрани один handle и жди его terminal state штатным блокирующим/event механизмом; `running`, истечение yield или тихий stdout не означают отказ. Не заканчивай ход словами «жду», не запускай второй процесс или фоновый цикл частого опроса. При явном «стоп» прекрати работу. При «следующий» немедленно перейди к названному ID и не дописывай прошлую задачу. Если ID не назван, очередь берётся из `execution-queue.json`; точный `next_stream_id` также возвращают finish/resume. Дополнительный поиск runner или модели для перехода не нужен.

Подготовка/JSON/oracle не являются измерением обучения. Если позже пользователь разрешит учебный опыт, требуется настоящий обучающий механизм, совместимое сохранённое ненулевое состояние, независимое до/после и достаточный бюджет. `0→0` не подтверждает обучение. Любое измеренное улучшение полезного показателя при валидном опыте — успех по нему и домену; остальные показатели, включая ухудшения, показываются рядом. Эти требования не дают разрешения запускать обучение сейчас.


## Назначение

```json
{
  "schema_version": "xiigen.batch-request.v1",
  "stream_id": "DOC-CS-FINALLY/base",
  "topic_id": "DOC-CS-05",
  "objective_id": "DOC-CS-FINALLY",
  "domain_id": "docs.csharp",
  "profile_id": "base",
  "run_id": "S601B002",
  "split": "train",
  "count": 10,
  "family_ids": [
    "v3/DOC-CS-FINALLY/base/F0000010",
    "v3/DOC-CS-FINALLY/base/F0000011",
    "v3/DOC-CS-FINALLY/base/F0000012",
    "v3/DOC-CS-FINALLY/base/F0000013",
    "v3/DOC-CS-FINALLY/base/F0000014",
    "v3/DOC-CS-FINALLY/base/F0000015",
    "v3/DOC-CS-FINALLY/base/F0000016",
    "v3/DOC-CS-FINALLY/base/F0000017",
    "v3/DOC-CS-FINALLY/base/F0000018",
    "v3/DOC-CS-FINALLY/base/F0000019"
  ],
  "family_contracts": {
    "v3/DOC-CS-FINALLY/base/F0000010": {
      "origin_family_id": "v3/DOC-CS-FINALLY/base/F0000010",
      "leakage_group_ids": [
        "v3/DOC-CS-FINALLY/base/F0000010"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-FINALLY/base/F0000011": {
      "origin_family_id": "v3/DOC-CS-FINALLY/base/F0000011",
      "leakage_group_ids": [
        "v3/DOC-CS-FINALLY/base/F0000011"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-FINALLY/base/F0000012": {
      "origin_family_id": "v3/DOC-CS-FINALLY/base/F0000012",
      "leakage_group_ids": [
        "v3/DOC-CS-FINALLY/base/F0000012"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-FINALLY/base/F0000013": {
      "origin_family_id": "v3/DOC-CS-FINALLY/base/F0000013",
      "leakage_group_ids": [
        "v3/DOC-CS-FINALLY/base/F0000013"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-FINALLY/base/F0000014": {
      "origin_family_id": "v3/DOC-CS-FINALLY/base/F0000014",
      "leakage_group_ids": [
        "v3/DOC-CS-FINALLY/base/F0000014"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-FINALLY/base/F0000015": {
      "origin_family_id": "v3/DOC-CS-FINALLY/base/F0000015",
      "leakage_group_ids": [
        "v3/DOC-CS-FINALLY/base/F0000015"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-FINALLY/base/F0000016": {
      "origin_family_id": "v3/DOC-CS-FINALLY/base/F0000016",
      "leakage_group_ids": [
        "v3/DOC-CS-FINALLY/base/F0000016"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-FINALLY/base/F0000017": {
      "origin_family_id": "v3/DOC-CS-FINALLY/base/F0000017",
      "leakage_group_ids": [
        "v3/DOC-CS-FINALLY/base/F0000017"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-FINALLY/base/F0000018": {
      "origin_family_id": "v3/DOC-CS-FINALLY/base/F0000018",
      "leakage_group_ids": [
        "v3/DOC-CS-FINALLY/base/F0000018"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-FINALLY/base/F0000019": {
      "origin_family_id": "v3/DOC-CS-FINALLY/base/F0000019",
      "leakage_group_ids": [
        "v3/DOC-CS-FINALLY/base/F0000019"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    }
  },
  "pairs_per_family": 1,
  "next_start": 20,
  "sources": [
    "RAG-027",
    "RAG-028"
  ],
  "card": "curriculum/cards/DOC-CS-FINALLY.md",
  "output_directory": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-CS-FINALLY\\base\\S601B002",
  "mode": "CREATE_ONLY",
  "next_stream_id": "CODE-CS-STABLE-KEY/base",
  "completion_status": "DRAFT_CREATED",
  "review_status": "NOT_REQUESTED",
  "paths": {
    "preparation_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation",
    "learning_sets_root": "C:\\Projects\\llm_mvp_core\\learning-sets",
    "card": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\curriculum\\cards\\DOC-CS-FINALLY.md",
    "contract": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\02-DATA-CONTRACT.md",
    "schema": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\schemas\\dpo-candidate.schema.json",
    "request": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-CS-FINALLY\\base\\S601B002\\request.json",
    "prompt": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-CS-FINALLY\\base\\S601B002\\prompt.md",
    "draft": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-CS-FINALLY\\base\\S601B002\\draft.jsonl",
    "state": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-CS-FINALLY\\base\\S601B002\\state.json",
    "commands": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-CS-FINALLY\\base\\S601B002\\commands.md",
    "resume": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-CS-FINALLY\\base\\S601B002\\RESUME.md",
    "tools_script": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\tools\\batches.py",
    "python_executable": "C:\\Python314\\python.exe",
    "sources": {
      "RAG-027": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-027.md",
      "RAG-028": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-028.md"
    }
  },
  "model_policy": "Current user-selected Chinese model in Kiro; no override; record observed identity honestly.",
  "role_contract": {
    "selection": {
      "actor": "CODEX",
      "requested_model": "astra 6 ultracode",
      "status": "NOT_RUN_IN_THIS_CREATION_STAGE"
    },
    "generation": {
      "actor": "external_open_chinese_model_via_kiro",
      "model_policy": "Use the selected Chinese model; inherit the current configuration without override. Record observed identity or unknown."
    },
    "quality_assessment": {
      "actor": "CODEX",
      "requested_model": "astra 6 ultracode",
      "status": "NOT_REQUESTED_IN_CREATE_ONLY"
    },
    "creation_blocked_by_review": false,
    "identity_scope": "Requested roles, not evidence of actual model execution or completed selection/review."
  },
  "scope": "DATA_PREPARATION_ONLY",
  "training_status": "NOT_RUN"
}
```

На каждую family_id создай ровно одну строку.

## Предметное содержание карточки (без исторических команд)

{
  "objective_id": "DOC-CS-FINALLY",
  "topic_id": "DOC-CS-05",
  "title_ru": "Описать возврат и наблюдаемые эффекты finally",
  "learning_goal_ru": "Отдельно проследить вычисление возвращаемого значения, выполнение finally и то, что наблюдает вызывающий код.",
  "generation_prompt_ru": "Создавай маленький полностью видимый синхронный метод с try/finally, иногда с catch, вложенным try или using. Для каждого prompt задай точный способ наблюдения: значение return, свойства возвращённого объекта, порядок Trace либо тип пойманной ошибки. Выбери одну границу: вычисленное value не меняется последующим присваиванием локальной переменной; возвращённая ссылка остаётся ссылкой на изменяемый объект; finally выполняется до внешнего catch; выброшенная из finally ошибка отменяет return. Для using покажи весь Dispose и точную область видимости. Не добавляй async, внешние callee, БД, непоказанный logger или предположения о GC. Требуй объяснить данный код, не предложить его «улучшение». Чередуй нормальный выход, return и ошибку; не учи правилу «finally всегда успешно возвращает результат».\n\nРаботай по одному request.json: один topic, objective, domain, profile=base и split. Создавай новые содержательные ситуации, а не переименования публичных примеров. Одна пара различается одним главным предметным дефектом; не делай rejected синтаксически сломанным, бессмысленным или вторым правильным ответом. Полный код, входы, версии, контракт зависимостей и формат ответа доступны в prompt: ученик не получает rationale и metadata. Сначала сформулируй ожидаемое поведение по условию, затем полный выбранный ответ и близкий ошибочный ответ.\nИсточники — только назначенные файлы xiigen-planning main/llm_plan3. Они подтверждают методику и тематику; новые мини-программы — авторские симуляции, не извлечённые результаты runtime. Все примеры этого дополнения имеют split=reference, training_allowed=false, oracle=NOT_RUN. Их нельзя переименовать в Chinese-generated TRAIN или скрытый EXAM. Родственные DOC/CODE/переводные версии, переименования и производные имеют один origin/leakage group и split. Новые family IDs не доказывают независимость содержания.\nРоль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. В CREATE_ONLY назначение уже задано карточкой/request: новый вызов выбора и оценка сейчас не запускаются и не задерживают создание следующей партии. Это назначение ролей, не подтверждение выполненного вызова.\nПрочитай уже собранный prompt/request по абсолютным путям; не ищи другую схему в llm_mvp_core. Запиши реальные строки в request.paths.draft, выполни одну готовую команду finish и перейди к next_stream_id из полной execution-queue.json. PREPARED без draft не является созданием. Старые данные не удалять; существующий run продолжать через resume. При NEXT/новом ID сразу перейти к нему, не продолжать прежнюю задачу или её отчёт. STOP немедленно прекращает действия. Если есть живой handle, ждать штатным wait/resume до terminal в том же ходе: не запускать дубликаты, не опрашивать часто и не завершать ход словами «жду». Отчёт по-русски: тема, фактические строки/семьи, файл, модель, структурный статус и следующий ID.",
  "response_contract_ru": "Русский текст с точным результатом и одним объяснением по показанному коду. Не переписывать код.",
  "scenario_axes": [
    {
      "axis": "путь выхода",
      "values": [
        "return из try",
        "обычное завершение",
        "ошибка из try",
        "return из catch",
        "ошибка из finally"
      ]
    },
    {
      "axis": "наблюдаемая сущность",
      "values": [
        "скаляр",
        "возвращённый объект",
        "порядок Trace",
        "Dispose",
        "вложенный finally"
      ]
    }
  ],
  "forbidden_mixing": [
    "Другой предмет обучения внутри одной партии",
    "Изменение production-кода",
    "Выдуманное исполнение runtime",
    "Расширение задачи до review/обучения"
  ],
  "generation_constraints": {
    "input_context": "Complete in prompt after native projection",
    "source_role": "Methodology and topic evidence; examples are new simulations",
    "version_profile": "C# 12 / .NET 8.0; synchronous console fixture",
    "balance": "Включить противоположные и граничные случаи; не учить неизменному предпочтению API по его названию."
  },
  "pairs_per_family": 1,
  "examples": [
    {
      "prompt": "Симуляция C# 12 / .NET 8.0. using System; using System.Collections.Generic; using System.Linq; доступны. Все методы выполняются синхронно. В классе объявлено: static int F(List<string> t) { int n=4; try { return n; } finally { n=9; t.Add(n.ToString()); } } Вызов: var t=new List<string>(); var r=F(t); Назови r и t, затем объясни порядок.",
      "chosen": "r=4; t=[\"9\"]. Значение return вычислено до finally; присваивание n=9 меняет локальную переменную после вычисления результата.",
      "rejected": "r=9; t=[\"9\"]. Присваивание n=9 в finally меняет возвращаемое значение перед выходом.",
      "error_tag": "return_value_recomputed",
      "rationale": "Rejected ошибочно вычисляет return повторно после finally.",
      "example_id": "DOC-CS-FINALLY.REF.01",
      "profile_id": "base",
      "family_id": "v6.ref.doc-cs-finally.value-captured",
      "source_refs": [
        "RAG-027",
        "RAG-028"
      ],
      "counterexample": "Выполнить F с новым списком; сравнить r и Trace.",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "C# 12 / .NET 8.0; synchronous console fixture",
        "origin_family_id": "v6.ref.doc-cs-finally.value-captured",
        "leakage_group_ids": [
          "v6.ref.doc-cs-finally.value-captured"
        ],
        "source_role": "methodology",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "C# 12 / .NET 8.0; synchronous console fixture",
          "expected": {
            "r": 4,
            "trace": [
              "9"
            ]
          },
          "assertion": "Выполнить F с новым списком; сравнить r и Trace.",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-CS-05"
      }
    },
    {
      "prompt": "Симуляция C# 12 / .NET 8.0. using System; using System.Collections.Generic; using System.Linq; доступны. Все методы выполняются синхронно. Объявлены: sealed class Box { public int Value; } static Box F() { var b=new Box { Value=4 }; try { return b; } finally { b.Value=9; } } Вызывающий код: var r=F(); var v=r.Value; Назови v и объясни, какой объект возвращён.",
      "chosen": "v=9. Возвращается ссылка на тот же Box; finally меняет его Value до чтения свойства вызывающим кодом.",
      "rejected": "v=4. Return сохраняет независимую копию Box до выполнения finally.",
      "error_tag": "reference_mistaken_for_value_copy",
      "rationale": "Ссылка вычислена до finally, но объект не копируется.",
      "example_id": "DOC-CS-FINALLY.REF.02",
      "profile_id": "base",
      "family_id": "v6.ref.doc-cs-finally.returned-reference",
      "source_refs": [
        "RAG-027",
        "RAG-028"
      ],
      "counterexample": "После F прочитать Value; сравнить 9 с утверждением о копии.",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "C# 12 / .NET 8.0; synchronous console fixture",
        "origin_family_id": "v6.ref.doc-cs-finally.returned-reference",
        "leakage_group_ids": [
          "v6.ref.doc-cs-finally.returned-reference"
        ],
        "source_role": "methodology",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "C# 12 / .NET 8.0; synchronous console fixture",
          "expected": {
            "v": 9
          },
          "assertion": "После F прочитать Value; сравнить 9 с утверждением о копии.",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-CS-05"
      }
    },
    {
      "prompt": "Симуляция C# 12 / .NET 8.0. using System; using System.Collections.Generic; using System.Linq; доступны. Все методы выполняются синхронно. var t=new List<string>(); try { try { t.Add(\"work\"); throw new InvalidOperationException(); } finally { t.Add(\"cleanup\"); } } catch (InvalidOperationException) { t.Add(\"caught\"); } Назови t в порядке записей.",
      "chosen": "[\"work\",\"cleanup\",\"caught\"]. Внутренний finally выполняется при выходе по ошибке до внешнего catch.",
      "rejected": "[\"work\",\"caught\",\"cleanup\"]. Сначала внешний catch обрабатывает ошибку, затем выполняется внутренний finally.",
      "error_tag": "catch_before_inner_finally",
      "rationale": "Одна ошибка — переставлены обработка ошибки и cleanup.",
      "example_id": "DOC-CS-FINALLY.REF.03",
      "profile_id": "base",
      "family_id": "v6.ref.doc-cs-finally.outer-catch",
      "source_refs": [
        "RAG-027",
        "RAG-028"
      ],
      "counterexample": "Исполнить фрагмент с новым t; сравнить последовательность.",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "C# 12 / .NET 8.0; synchronous console fixture",
        "origin_family_id": "v6.ref.doc-cs-finally.outer-catch",
        "leakage_group_ids": [
          "v6.ref.doc-cs-finally.outer-catch"
        ],
        "source_role": "methodology",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "C# 12 / .NET 8.0; synchronous console fixture",
          "expected": {
            "trace": [
              "work",
              "cleanup",
              "caught"
            ]
          },
          "assertion": "Исполнить фрагмент с новым t; сравнить последовательность.",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-CS-05"
      }
    },
    {
      "prompt": "Симуляция C# 12 / .NET 8.0. using System; using System.Collections.Generic; using System.Linq; доступны. Все методы выполняются синхронно. var t=new List<string>(); try { t.Add(\"body\"); } finally { t.Add(\"cleanup\"); } t.Add(\"after\"); Назови t. Исключений в Add нет.",
      "chosen": "[\"body\",\"cleanup\",\"after\"]. Finally выполняется и при обычном завершении try, до следующего оператора.",
      "rejected": "[\"body\",\"after\"]. При отсутствии исключения finally пропускается.",
      "error_tag": "finally_only_on_exception",
      "rationale": "Rejected пропускает один обязательный эффект.",
      "example_id": "DOC-CS-FINALLY.REF.04",
      "profile_id": "base",
      "family_id": "v6.ref.doc-cs-finally.normal-exit",
      "source_refs": [
        "RAG-027",
        "RAG-028"
      ],
      "counterexample": "Проверить все три записи без вызова исключения.",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "C# 12 / .NET 8.0; synchronous console fixture",
        "origin_family_id": "v6.ref.doc-cs-finally.normal-exit",
        "leakage_group_ids": [
          "v6.ref.doc-cs-finally.normal-exit"
        ],
        "source_role": "methodology",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "C# 12 / .NET 8.0; synchronous console fixture",
          "expected": {
            "trace": [
              "body",
              "cleanup",
              "after"
            ]
          },
          "assertion": "Проверить все три записи без вызова исключения.",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-CS-05"
      }
    },
    {
      "prompt": "Симуляция C# 12 / .NET 8.0. using System; using System.Collections.Generic; using System.Linq; доступны. Все методы выполняются синхронно. static int F(List<string> t) { try { try { return 5; } finally { t.Add(\"inner\"); } } finally { t.Add(\"outer\"); } } var t=new List<string>(); var r=F(t); Назови r и t.",
      "chosen": "r=5; t=[\"inner\",\"outer\"]. При return сначала завершается внутренний finally, затем внешний.",
      "rejected": "r=5; t=[\"outer\",\"inner\"]. При return сначала завершается внешний finally, затем внутренний.",
      "error_tag": "nested_finally_order_reversed",
      "rationale": "Одна ошибка — порядок двух видимых эффектов.",
      "example_id": "DOC-CS-FINALLY.REF.05",
      "profile_id": "base",
      "family_id": "v6.ref.doc-cs-finally.nested-cleanup",
      "source_refs": [
        "RAG-027",
        "RAG-028"
      ],
      "counterexample": "Сравнить порядок двух Add при том же r=5.",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "C# 12 / .NET 8.0; synchronous console fixture",
        "origin_family_id": "v6.ref.doc-cs-finally.nested-cleanup",
        "leakage_group_ids": [
          "v6.ref.doc-cs-finally.nested-cleanup"
        ],
        "source_role": "methodology",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "C# 12 / .NET 8.0; synchronous console fixture",
          "expected": {
            "r": 5,
            "trace": [
              "inner",
              "outer"
            ]
          },
          "assertion": "Сравнить порядок двух Add при том же r=5.",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-CS-05"
      }
    },
    {
      "prompt": "Симуляция C# 12 / .NET 8.0. using System; using System.Collections.Generic; using System.Linq; доступны. Все методы выполняются синхронно. Объявлены: sealed class R : IDisposable { readonly List<string> t; public R(List<string> t0) { t=t0; } public void Dispose() { t.Add(\"dispose\"); } } static void F(List<string> t) { using var r=new R(t); t.Add(\"body\"); } var t=new List<string>(); F(t); t.Add(\"caller\"); Назови t.",
      "chosen": "[\"body\",\"dispose\",\"caller\"]. Using declaration освобождает r при выходе из F, до следующего оператора вызывающего кода.",
      "rejected": "[\"body\",\"caller\",\"dispose\"]. Using declaration откладывает Dispose до завершения вызывающего кода.",
      "error_tag": "using_scope_extended_to_caller",
      "rationale": "Граница жизни ресурса определена телом F.",
      "example_id": "DOC-CS-FINALLY.REF.06",
      "profile_id": "base",
      "family_id": "v6.ref.doc-cs-finally.using-scope",
      "source_refs": [
        "RAG-027",
        "RAG-028"
      ],
      "counterexample": "Выполнить F; проверить момент видимого Dispose, не ждать GC.",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "C# 12 / .NET 8.0; synchronous console fixture",
        "origin_family_id": "v6.ref.doc-cs-finally.using-scope",
        "leakage_group_ids": [
          "v6.ref.doc-cs-finally.using-scope"
        ],
        "source_role": "methodology",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "C# 12 / .NET 8.0; synchronous console fixture",
          "expected": {
            "trace": [
              "body",
              "dispose",
              "caller"
            ]
          },
          "assertion": "Выполнить F; проверить момент видимого Dispose, не ждать GC.",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-CS-05"
      }
    },
    {
      "prompt": "Симуляция C# 12 / .NET 8.0. using System; using System.Collections.Generic; using System.Linq; доступны. Все методы выполняются синхронно. static int F() { try { return 3; } finally { throw new InvalidOperationException(\"cleanup\"); } } string observed; try { observed=F().ToString(); } catch (InvalidOperationException e) { observed=e.Message; } Назови observed.",
      "chosen": "observed=\"cleanup\". Ошибка из finally прерывает возврат значения 3 и перехватывается внешним catch.",
      "rejected": "observed=\"3\". Уже вычисленное значение return сохраняет приоритет перед ошибкой из finally.",
      "error_tag": "return_survives_throwing_finally",
      "rationale": "Rejected объявляет результат, который не передаётся вызывающему коду.",
      "example_id": "DOC-CS-FINALLY.REF.07",
      "profile_id": "base",
      "family_id": "v6.ref.doc-cs-finally.cleanup-fails",
      "source_refs": [
        "RAG-027",
        "RAG-028"
      ],
      "counterexample": "Исполнить фрагмент и прочитать observed; не подменять ошибку успешным return.",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "C# 12 / .NET 8.0; synchronous console fixture",
        "origin_family_id": "v6.ref.doc-cs-finally.cleanup-fails",
        "leakage_group_ids": [
          "v6.ref.doc-cs-finally.cleanup-fails"
        ],
        "source_role": "methodology",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "C# 12 / .NET 8.0; synchronous console fixture",
          "expected": {
            "observed": "cleanup"
          },
          "assertion": "Исполнить фрагмент и прочитать observed; не подменять ошибку успешным return.",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-CS-05"
      }
    },
    {
      "prompt": "Симуляция C# 12 / .NET 8.0. using System; using System.Collections.Generic; using System.Linq; доступны. Все методы выполняются синхронно. static int F(List<string> t) { try { throw new FormatException(); } catch (FormatException) { t.Add(\"catch\"); return 2; } finally { t.Add(\"finally\"); } } var t=new List<string>(); var r=F(t); Назови r и t.",
      "chosen": "r=2; t=[\"catch\",\"finally\"]. Return из catch также проходит через finally.",
      "rejected": "r=2; t=[\"catch\"]. Return из catch обходит finally.",
      "error_tag": "catch_return_skips_finally",
      "rationale": "Один пропущенный эффект cleanup.",
      "example_id": "DOC-CS-FINALLY.REF.08",
      "profile_id": "base",
      "family_id": "v6.ref.doc-cs-finally.catch-return",
      "source_refs": [
        "RAG-027",
        "RAG-028"
      ],
      "counterexample": "Сравнить r и обе записи t после вызова.",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "C# 12 / .NET 8.0; synchronous console fixture",
        "origin_family_id": "v6.ref.doc-cs-finally.catch-return",
        "leakage_group_ids": [
          "v6.ref.doc-cs-finally.catch-return"
        ],
        "source_role": "methodology",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "C# 12 / .NET 8.0; synchronous console fixture",
          "expected": {
            "r": 2,
            "trace": [
              "catch",
              "finally"
            ]
          },
          "assertion": "Сравнить r и обе записи t после вызова.",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-CS-05"
      }
    }
  ]
}

## Полный контракт строки: не искать другую схему

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "title": "XIIGEN candidate DPO row, creation stage only",
  "description": "One JSON object per JSONL line. Structural conformance does not establish semantic quality, native admission or learning. Request-specific family/topic constraints are checked by batches.py finish.",
  "type": "object",
  "required": [
    "schema_version",
    "pair_id",
    "domain_id",
    "topic_id",
    "profile_id",
    "family_id",
    "split",
    "prompt",
    "chosen",
    "rejected",
    "error_tag",
    "rationale",
    "source_refs",
    "provenance",
    "metadata"
  ],
  "properties": {
    "schema_version": {
      "const": "xiigen.dpo.v1"
    },
    "pair_id": {
      "type": "string",
      "minLength": 1
    },
    "domain_id": {
      "type": "string",
      "minLength": 1
    },
    "topic_id": {
      "type": "string",
      "minLength": 1
    },
    "profile_id": {
      "type": "string",
      "minLength": 1
    },
    "family_id": {
      "type": "string",
      "minLength": 1
    },
    "split": {
      "enum": [
        "train",
        "dev",
        "exam"
      ]
    },
    "prompt": {
      "type": "string",
      "minLength": 1
    },
    "chosen": {
      "type": "string",
      "minLength": 1
    },
    "rejected": {
      "type": "string",
      "minLength": 1
    },
    "error_tag": {
      "type": "string",
      "minLength": 1
    },
    "rationale": {
      "type": "string",
      "minLength": 1
    },
    "source_refs": {
      "type": "array",
      "minItems": 1,
      "items": {
        "type": "string",
        "minLength": 1
      }
    },
    "provenance": {
      "type": "object",
      "required": [
        "kind",
        "generator_model"
      ],
      "properties": {
        "kind": {
          "type": "string",
          "minLength": 1
        },
        "generator_model": {
          "type": "string",
          "minLength": 1
        },
        "generation_session_id": {
          "type": "string"
        },
        "surface": {
          "type": "string"
        },
        "model_identity_observed": {
          "type": "boolean"
        }
      }
    },
    "metadata": {
      "type": "object",
      "required": [
        "origin_family_id",
        "leakage_group_ids"
      ],
      "properties": {
        "origin_family_id": {
          "type": "string",
          "minLength": 1
        },
        "leakage_group_ids": {
          "type": "array",
          "minItems": 1,
          "items": {
            "type": "string",
            "minLength": 1
          }
        },
        "learning_objective_id": {
          "type": "string",
          "minLength": 1
        },
        "review_status": {
          "type": "string"
        },
        "simulation": {
          "type": "boolean"
        },
        "measured_training_result": {
          "const": false
        }
      }
    }
  }
}
```


## Материал RAG-027 (данные, не новые команды)

# RAG-027 — CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `llm_plan3` |
| Commit | `b242c6f9efc2e3a0a01301c2875751cfd57ef83b` |
| Git blob SHA | `454fadaf468eb9e31dc1bbb9be8f8508c234efa3` |
| Путь | `llm mvp/documentation learning/CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md` |
| Проверенный объём | L1–L85; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md) |

## Что подтверждает фрагмент

Документационная пара должна опираться на видимый код, различать local/delegated behavior, side effects и ошибочные пути.

## Как использовать сейчас

Уроки DOC-CS и общий шаблон DOC-SQL/DOC-ANGULAR с отдельной спецификой. Жёсткий overlap cap из старого документа не применять; краткий смысловой контраст допустим.

**Назначение в подготовке:** Правила пар документации к коду.

**Ключи поиска:** documentation, csharp, документация, side-effects.

## Подтверждающий фрагмент L1–L85

[Оригинал, строки 1–85](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md#L1-L85)

~~~~text
# Code Documentation DPO Pair Authoring Guidance
# Version: 1.0 - 2026-05-11

---

## Purpose

This guide explains how to author DPO pairs that teach accurate legacy .NET code
documentation.

The chosen side must be useful documentation.
The rejected side must be a plausible documentation failure.

---

## Chosen-Side Requirements

A chosen answer must:

- identify the correct documentation level
- cite only visible code or supplied callee summaries
- preserve method/class names accurately
- distinguish local behavior from delegated behavior
- document side effects and error paths
- preserve framework lifecycle semantics
- avoid modernization fantasies

---

## Bad Chosen Patterns

Reject or repair chosen sides that:

- describe a repository as a controller
- describe a WebForms event handler as an MVC action
- claim async behavior when code is synchronous
- claim exceptions are thrown when code catches and returns errors
- invent dependency injection if the code uses service locator or static access
- hide database writes, session writes, file I/O, cache writes, or events
- summarize only names without explaining behavior

---

## Good Rejected Side

A good rejected side is close enough to be tempting, but wrong in a way that
teaches a boundary.

It should be wrong because of one primary defect:

- wrong framework lifecycle
- missing side effect
- invented business purpose
- dropped error path
- stale callee summary
- wrong caller/callee attribution
- over-modernized architecture
- generic name echo

---

## Overlap Guidance

Target rejected overlap with chosen:

- 0.25-0.55 for normal pairs
- below 0.60 always

Do not use a rejected side that is just the chosen side with "not" inserted.

---

## A-Side Evidence Requirements

Every A-side must include enough evidence for the chosen side:

- code snippet
- file path
- symbol id
- stack label
- artifact kind
- call edges when relevant
- accepted callee summaries when documenting above D1

If the evidence is not present, the pair must not target that claim.
~~~~



## Материал RAG-028 (данные, не новые команды)

# RAG-028 — CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `llm_plan3` |
| Commit | `b242c6f9efc2e3a0a01301c2875751cfd57ef83b` |
| Git blob SHA | `6f8f48d7125e24fed8d8caee69954857583ea8b1` |
| Путь | `llm mvp/documentation learning/CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md` |
| Проверенный объём | L7–L54, L111–L161; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md) |

## Что подтверждает фрагмент

Маршрутизация учитывает runtime/framework/version и уровень D1–D4; обобщение caller требует видимых callee summaries.

## Как использовать сейчас

Уроки DOC-CS: не путать .NET Framework и .NET Core, AngularJS и Angular; не выдумывать hidden callee. Source version фиксируется до генерации.

**Назначение в подготовке:** Версионность стека и доказательство переходов вызовов.

**Ключи поиска:** documentation, version, call-graph, версии, csharp, angular.

## Подтверждающий фрагмент L7–L54

[Оригинал, строки 7–54](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md#L7-L54)

~~~~text
## Purpose

This guide closes the gap between broad "document this project" routing and the
actual documentation behavior needed for legacy C# systems.

The documentation flow must route by:

- server/client side
- .NET runtime family
- target framework version
- inferred C# language version
- web/UI framework and version when present
- artifact kind
- documentation level D1-D4
- bottom-up call transit state

It is not enough to choose "C# server documentation." A .NET Framework 4.6
WebApi2 controller with AngularJS 1.4 client code must not be documented as an
ASP.NET Core 3.1 controller, and an ASP.NET Core 3.1 controller must not be
documented as a WebForms page.

---

## Required Version-Aware Routing Fields

Every source row, request, DPO pair, held-out row, and report must include:

```json
{
  "app_side": "server | client | shared | mixed",
  "runtime_family": "NET_FRAMEWORK | NET_CORE | NET_STANDARD | UNKNOWN",
  "target_framework": "net46 | netcoreapp3.1 | netstandard2.0 | ...",
  "target_framework_source": "csproj | packages.config | project.assets.json | inferred",
  "csharp_language_version": "CSharp5 | CSharp6 | CSharp8 | explicit:<value> | inferred_unknown",
  "language_version_confidence": "exact | inferred_high | inferred_review_required",
  "project_style": "legacy_non_sdk | sdk_style | mixed",
  "web_framework": "WebForms | WebApi2 | ASP.NET Core MVC | ASP.NET Core WebApi | ASMX | none | unknown",
  "web_framework_version": "System.Web.Http 5.2.3 | Microsoft.AspNetCore.App 3.1 | ...",
  "frontend_framework": "AngularJS | React | WPF | WinForms | none | unknown",
  "frontend_framework_version": "1.4.8 | ...",
  "stack_label": "NETFX_WEBAPI | NETCORE_WEBAPI | DATA_ACCESS_EF | ...",
  "artifact_kind": "method | controller_action | helper_method | repository_method | page_event | workflow",
  "documentation_level": "D1 | D2 | D3 | D4"
}
```

If a value is inferred, the evidence must name the file and field that produced
the inference.
~~~~

## Подтверждающий фрагмент L111–L161

[Оригинал, строки 111–161](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md#L111-L161)

~~~~text
## Bottom-Up Call-Transit Gate

Documentation must be built like a compiler pass.

### Stage T1 - Leaf Method Summary

Document methods whose behavior is locally visible and does not require a known
callee summary. The output becomes an accepted `CalleeSummary`.

### Stage T2 - Helper Or Component Summary

Document a helper or component only after the methods it calls are documented or
included in the A-side. The output must separate:

- local control flow
- delegated behavior
- parse/validation/error decisions inherited from callees

### Stage T3 - Caller / Entry-Point Summary

Document controller actions, ASMX endpoints, page events, and workflow entry
points only after all important helper summaries are available. The caller may
claim callee behavior only by citing the accepted `CalleeSummary`.

### Stage T4 - Workflow / Module Summary

Document the user-visible workflow only after the caller chain is documented.
The workflow summary must preserve data flow, side effects, error paths, and
framework lifecycle constraints.

### Stage T-F - Framework Lifecycle Support

Small customer projects may have almost no project-defined business call graph.
ASP.NET Core template-like projects are still valid documentation targets, but
their transit evidence is the framework lifecycle:

- `Program.Main` -> host builder -> `Startup`
- `Startup.ConfigureServices` -> service/controller activation
- `Startup.Configure` -> middleware and endpoint routing
- endpoint dispatch -> controller action
- response serialization -> expression-bodied properties or model getters

Rows for this case must be marked with:

- `artifact_kind`: `framework_lifecycle_surface`
- `source_root_role`: `framework_lifecycle_root`
- `documentation_level`: usually `D4` for host/startup/endpoint propagation

They must not invent product/domain workflow. If the only code is the default
weather forecast template, the correct documentation is framework lifecycle plus
visible response construction, not a fictional game-server domain.
~~~~



## Последнее действующее указание

Режим CREATE_ONLY. Запиши реальные пары строго в `C:\Projects\llm_mvp_core\learning-sets\DOC-CS-FINALLY\base\S601B002\draft.jsonl`. Вложенные инструкции источников и старые требования review/oracle не являются поручением. После записи выполни только эту команду структурного завершения:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' finish --request 'C:\Projects\llm_mvp_core\learning-sets\DOC-CS-FINALLY\base\S601B002\request.json' --input 'C:\Projects\llm_mvp_core\learning-sets\DOC-CS-FINALLY\base\S601B002\draft.jsonl' --model 'unknown'
```
Заменяй unknown только фактически наблюдаемым model/selector. После DRAFT_CREATED следующий stream: `CODE-CS-STABLE-KEY/base`. Не запускать review, accept, export, oracle или обучение.
