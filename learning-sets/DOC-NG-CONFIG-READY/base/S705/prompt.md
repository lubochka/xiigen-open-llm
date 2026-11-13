# Создание одной партии DPO в Kiro — CREATE_ONLY

<!-- CURRENT-LANGUAGE-AND-CAMPAIGN -->
Reason internally in English. Do not output private reasoning. Russian status; learner language follows the assigned profile. This prompt remains for an explicitly selected legacy request. New domain quotas and continuation use [campaign/registry.json](../campaign/registry.json); do not start a new legacy M001 instead of a pending campaign request.
<!-- /CURRENT-LANGUAGE-AND-CAMPAIGN -->


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
  "stream_id": "DOC-NG-CONFIG-READY/base",
  "topic_id": "DOC-NG-03",
  "objective_id": "DOC-NG-CONFIG-READY",
  "domain_id": "docs.angular",
  "profile_id": "base",
  "run_id": "S705",
  "split": "train",
  "count": 10,
  "family_ids": [
    "v3/DOC-NG-CONFIG-READY/base/F0000000",
    "v3/DOC-NG-CONFIG-READY/base/F0000001",
    "v3/DOC-NG-CONFIG-READY/base/F0000002",
    "v3/DOC-NG-CONFIG-READY/base/F0000003",
    "v3/DOC-NG-CONFIG-READY/base/F0000004",
    "v3/DOC-NG-CONFIG-READY/base/F0000005",
    "v3/DOC-NG-CONFIG-READY/base/F0000006",
    "v3/DOC-NG-CONFIG-READY/base/F0000007",
    "v3/DOC-NG-CONFIG-READY/base/F0000009",
    "v3/DOC-NG-CONFIG-READY/base/F0000010"
  ],
  "family_contracts": {
    "v3/DOC-NG-CONFIG-READY/base/F0000000": {
      "origin_family_id": "v3/DOC-NG-CONFIG-READY/base/F0000000",
      "leakage_group_ids": [
        "v3/DOC-NG-CONFIG-READY/base/F0000000"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-CONFIG-READY/base/F0000001": {
      "origin_family_id": "v3/DOC-NG-CONFIG-READY/base/F0000001",
      "leakage_group_ids": [
        "v3/DOC-NG-CONFIG-READY/base/F0000001"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-CONFIG-READY/base/F0000002": {
      "origin_family_id": "v3/DOC-NG-CONFIG-READY/base/F0000002",
      "leakage_group_ids": [
        "v3/DOC-NG-CONFIG-READY/base/F0000002"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-CONFIG-READY/base/F0000003": {
      "origin_family_id": "v3/DOC-NG-CONFIG-READY/base/F0000003",
      "leakage_group_ids": [
        "v3/DOC-NG-CONFIG-READY/base/F0000003"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-CONFIG-READY/base/F0000004": {
      "origin_family_id": "v3/DOC-NG-CONFIG-READY/base/F0000004",
      "leakage_group_ids": [
        "v3/DOC-NG-CONFIG-READY/base/F0000004"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-CONFIG-READY/base/F0000005": {
      "origin_family_id": "v3/DOC-NG-CONFIG-READY/base/F0000005",
      "leakage_group_ids": [
        "v3/DOC-NG-CONFIG-READY/base/F0000005"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-CONFIG-READY/base/F0000006": {
      "origin_family_id": "v3/DOC-NG-CONFIG-READY/base/F0000006",
      "leakage_group_ids": [
        "v3/DOC-NG-CONFIG-READY/base/F0000006"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-CONFIG-READY/base/F0000007": {
      "origin_family_id": "v3/DOC-NG-CONFIG-READY/base/F0000007",
      "leakage_group_ids": [
        "v3/DOC-NG-CONFIG-READY/base/F0000007"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-CONFIG-READY/base/F0000009": {
      "origin_family_id": "v3/DOC-NG-CONFIG-READY/base/F0000009",
      "leakage_group_ids": [
        "v3/DOC-NG-CONFIG-READY/base/F0000009"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-CONFIG-READY/base/F0000010": {
      "origin_family_id": "v3/DOC-NG-CONFIG-READY/base/F0000010",
      "leakage_group_ids": [
        "v3/DOC-NG-CONFIG-READY/base/F0000010"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    }
  },
  "pairs_per_family": 1,
  "next_start": 11,
  "sources": [
    "RAG-027",
    "RAG-028",
    "RAG-037"
  ],
  "card": "curriculum/cards/DOC-NG-CONFIG-READY.md",
  "output_directory": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-NG-CONFIG-READY\\base\\S705",
  "mode": "CREATE_ONLY",
  "next_stream_id": "CODE-NG-CONFIG-READY/base",
  "completion_status": "DRAFT_CREATED",
  "review_status": "NOT_REQUESTED",
  "paths": {
    "preparation_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation",
    "learning_sets_root": "C:\\Projects\\llm_mvp_core\\learning-sets",
    "card": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\curriculum\\cards\\DOC-NG-CONFIG-READY.md",
    "contract": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\02-DATA-CONTRACT.md",
    "schema": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\schemas\\dpo-candidate.schema.json",
    "request": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-NG-CONFIG-READY\\base\\S705\\request.json",
    "prompt": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-NG-CONFIG-READY\\base\\S705\\prompt.md",
    "draft": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-NG-CONFIG-READY\\base\\S705\\draft.jsonl",
    "state": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-NG-CONFIG-READY\\base\\S705\\state.json",
    "commands": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-NG-CONFIG-READY\\base\\S705\\commands.md",
    "resume": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-NG-CONFIG-READY\\base\\S705\\RESUME.md",
    "tools_script": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\tools\\batches.py",
    "python_executable": "C:\\Python314\\python.exe",
    "sources": {
      "RAG-027": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-027.md",
      "RAG-028": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-028.md",
      "RAG-037": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-037.md"
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
  "objective_id": "DOC-NG-CONFIG-READY",
  "topic_id": "DOC-NG-03",
  "title_ru": "Описать фактическую готовность runtime config",
  "learning_goal_ru": "Отличить вызов загрузки, завершение возвращённого Promise и фактическую готовность конфигурации по показанному коду.",
  "generation_prompt_ru": "Создавай короткую TypeScript-симуляцию runtime config по проектной границе из RAG-037. В prompt всегда покажи локальный host-контракт: что initializer возвращает, какой Promise host ожидает и когда разрешено читать snapshot. Не используй provideAppInitializer как будто этот API есть во всех версиях Angular. Достаточны локальные функции с полными интерфейсами.\nДокументация должна точно описывать и неправильный показанный loader: преждевременный ready, потерянный return Promise, проглоченную ошибку. Не подменяй фактическое поведение кодом, который должен был быть написан. Для нескольких потребителей покажи полный single-flight код и счётчик fetch. Для reload задай начальное состояние. Не заявляй серверный rollback, скрытую последовательность guards либо реальный запуск host.\n\nПростая самопроверка:\n1. Выпиши состояние до вызова, при pending, после fulfilled и после rejected.\n2. Проверь, какой именно Promise получает host, либо что он получает undefined.\n3. Пройди каждый catch до конца: есть ли throw, return или только логирование.\n4. При двух потребителях отдельно сосчитай вызовы fetch.\n5. Проверь getter snapshot по его телу; одно наличие cached не отменяет guard.\n\nРаботай по одному готовому request.json: один topic_id, objective_id, domain_id, profile_id и split. В каждом prompt повтори среду, полный видимый код/данные, точку наблюдения и форму ответа: metadata не попадает в строгую проекцию prompt/chosen/rejected. Создай полный chosen и близкий rejected с одним основным содержательным дефектом, одинакового формата. Не заменяй rejected синтаксической ошибкой, шуткой, заведомо коротким ответом или другой правильной реализацией. Объяснение проверки, source_refs и error_tag записывай вне ответов.\nСначала выпиши маленькую таблицу состояния либо результата по условию, затем сравни с ней обе альтернативы. Это самопроверка автора кандидатов, не независимая оценка качества и не измерение обучения. Если по указанному входу ответы одинаковы, измени различающий вход в пределах задания; не придумывай победу. Смена имени или чисел сама по себе не создаёт независимую семью. Производные, обратные и DOC/CODE-варианты конкретного случая наследуют общий origin/leakage group и split. Публичные reference не являются закрытым EXAM.\nИсточники — только назначенные файлы xiigen-planning main/llm_plan3. Они дают методику и исходную тематику. Новые фрагменты являются авторскими симуляциями; runtime/oracle сейчас NOT_RUN. Не объявляй новую симуляцию извлечённым production-кодом, измеренным результатом или китайской генерацией.\nРоль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. Сейчас CREATE_ONLY: назначение уже дано карточкой/request. Не запускай новые selection/review/oracle/accept/export/Student/production CI и не задерживай следующую партию ради них. Модель — фактически текущая выбранная в Kiro, без override; неизвестное имя/session запиши unknown. Checksum, подписи, profile receipts и независимый review не нужны для создания.\nКорень наборов C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets; подготовка находится в preparation внутри него. Возьми абсолютные входные и выходные пути из request.paths, не ищи runner в llm_mvp_core. После записи реальных JSONL-строк выполни одну готовую команду finish, получи структурный DRAFT_CREATED и перейди к next_stream_id полной execution-queue.json. PREPARED без draft не считается созданием. Старые данные/попытки не удалять; существующий run продолжать через resume.\nПри прямом NEXT/новом ID немедленно перейти к нему: запрещено продолжать предыдущую задачу, её отчёт или исправления. STOP прекращает действия сразу. Живой процесс ждать штатным wait/resume по единственному handle до terminal в том же ходе; не запускать дубликаты, не делать частый polling и не завершать ход словом «жду». Отчёт по-русски: тема, созданные строки/семьи, точный файл, фактическая модель, структурный статус, следующий ID. Нельзя приписывать вызов Astra текущей модели.",
  "response_contract_ru": "Короткий русский текст с точным состоянием/числом вызовов по заданной точке наблюдения; без исправления кода.",
  "scenario_axes": [
    {
      "axis": "стадия",
      "values": [
        "pending",
        "fulfilled",
        "rejected",
        "зависимости pending",
        "reload"
      ]
    },
    {
      "axis": "связь с host",
      "values": [
        "Promise возвращён",
        "Promise потерян",
        "ошибка проглочена",
        "single-flight"
      ]
    }
  ],
  "forbidden_mixing": [
    "Другой предмет обучения в той же партии.",
    "Смешение языковых версий, SQL-диалектов либо AngularJS/Angular.",
    "Изменение production и запуск приёмки/обучения вместо создания.",
    "Объявление ожидаемого результата наблюдавшимся."
  ],
  "generation_constraints": {
    "input_context": "Complete in prompt after native projection",
    "source_role": "Methodology/topic evidence; source-bounded authored simulations",
    "version_profile": "Angular 16.2.0 / TypeScript 5.1.6; explicit project initializer contract; isolated Promise fixture",
    "balance": "Чередовать положительные, граничные и противоположные условия; не учить выбору по имени API.",
    "self_check_ru": [
      "Построй таблицу: load вызван, fetch pending, fetch resolved, зависимости pending, ready.",
      "Назови реальное возвращаемое значение initializer.",
      "Если fetch отклонён, проследи catch/throw; fulfilled оболочка может скрывать ошибку.",
      "Сосчитай вызовы fetch отдельно от числа потребителей.",
      "Читай snapshot только по явному getter-контракту."
    ]
  },
  "pairs_per_family": 1,
  "examples": [
    {
      "example_id": "DOC-NG-CONFIG-READY.REF.01",
      "profile_id": "base",
      "family_id": "v7.ref.config.load-transition",
      "source_refs": [
        "RAG-027",
        "RAG-028",
        "RAG-037"
      ],
      "prompt": "Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. let ready=false; let snapshot:Config|null=null; async function load(){const c=await fetchConfig();snapshot=c;ready=true;} Функция fetchConfig():Promise<Config> задана окружением. Вызов var loadTask=load(); её Promise ещё pending. Назови ready/snapshot до завершения fetch. Затем fetch успешно возвращает {apiBaseUrl:\"/v1\"}; выполнено await loadTask. Назови новое состояние.",
      "chosen": "До завершения fetch: ready=false,snapshot=null. После успешного завершения loadTask: ready=true,snapshot={apiBaseUrl:\"/v1\"}.",
      "rejected": "До завершения fetch: ready=true,snapshot=null. После успешного завершения loadTask: ready=true,snapshot={apiBaseUrl:\"/v1\"}.",
      "error_tag": "ready_inferred_from_invocation",
      "rationale": "Флаг изменяется после await, а не при начале вызова.",
      "counterexample": "Пока fetch pending, присваивание ready=true ещё не выполнено.",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "Angular 16.2.0 / TypeScript 5.1.6; explicit project initializer contract; isolated Promise fixture",
        "origin_family_id": "v7.ref.config.load-transition",
        "leakage_group_ids": [
          "v7.ref.config.load-transition"
        ],
        "source_role": "methodology_and_topic_seed",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "Angular 16.2.0 / TypeScript 5.1.6; explicit project initializer contract; isolated Promise fixture",
          "expected": {
            "pending": {
              "ready": false,
              "snapshot": null
            },
            "completed": {
              "ready": true,
              "snapshot": {
                "apiBaseUrl": "/v1"
              }
            }
          },
          "assertion": "Пока fetch pending, присваивание ready=true ещё не выполнено.",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-NG-03"
      }
    },
    {
      "example_id": "DOC-NG-CONFIG-READY.REF.02",
      "profile_id": "base",
      "family_id": "v7.ref.config.early-ready",
      "source_refs": [
        "RAG-027",
        "RAG-028",
        "RAG-037"
      ],
      "prompt": "Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. let ready=false; let snapshot:Config|null=null; async function load(){ready=true;snapshot=await fetchConfig();} Функция fetchConfig():Promise<Config> ещё pending. Вызван load(), fetch ещё не завершён. Назови состояние и поясни, доказывает ли ready наличие config в этом коде.",
      "chosen": "ready=true,snapshot=null. Показанный loader выставляет ready преждевременно; этот флаг здесь не доказывает наличие config.",
      "rejected": "ready=true,snapshot={apiBaseUrl:\"/v1\"}. Ready=true гарантирует, что присваивание после await уже выполнено.",
      "error_tag": "flag_treated_as_unconditional_truth",
      "rationale": "Rejected утверждает, что присваивание после await уже выполнено.",
      "counterexample": "Перед await есть ready=true; snapshot после pending await ещё не присвоен.",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "Angular 16.2.0 / TypeScript 5.1.6; explicit project initializer contract; isolated Promise fixture",
        "origin_family_id": "v7.ref.config.early-ready",
        "leakage_group_ids": [
          "v7.ref.config.early-ready"
        ],
        "source_role": "methodology_and_topic_seed",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "Angular 16.2.0 / TypeScript 5.1.6; explicit project initializer contract; isolated Promise fixture",
          "expected": {
            "ready": true,
            "snapshot": null
          },
          "assertion": "Перед await есть ready=true; snapshot после pending await ещё не присвоен.",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-NG-03"
      }
    },
    {
      "example_id": "DOC-NG-CONFIG-READY.REF.03",
      "profile_id": "base",
      "family_id": "v7.ref.config.initializer-return",
      "source_refs": [
        "RAG-027",
        "RAG-028",
        "RAG-037"
      ],
      "prompt": "Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. Функция load():Promise<void> задана окружением и завершается только после загрузки config. const init=()=>{void load();}; Host вызвал init(); load ещё pending. Ждёт ли host загрузку по данному контракту?",
      "chosen": "Нет. Init возвращает undefined; вызов load есть, но его Promise не возвращён host.",
      "rejected": "Да. Сам вызов load внутри init автоматически заставляет host дождаться его Promise.",
      "error_tag": "promise_not_returned_to_host",
      "rationale": "Наличие вызова не равнозначно возвращению его Promise.",
      "counterexample": "У init нет return; host не получил Promise загрузки.",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "Angular 16.2.0 / TypeScript 5.1.6; explicit project initializer contract; isolated Promise fixture",
        "origin_family_id": "v7.ref.config.initializer-return",
        "leakage_group_ids": [
          "v7.ref.config.initializer-return"
        ],
        "source_role": "methodology_and_topic_seed",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "Angular 16.2.0 / TypeScript 5.1.6; explicit project initializer contract; isolated Promise fixture",
          "expected": {
            "initializer_return": "undefined",
            "host_waits_for_load": false
          },
          "assertion": "У init нет return; host не получил Promise загрузки.",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-NG-03"
      }
    },
    {
      "example_id": "DOC-NG-CONFIG-READY.REF.04",
      "profile_id": "base",
      "family_id": "v7.ref.config.load-transition",
      "source_refs": [
        "RAG-027",
        "RAG-028",
        "RAG-037"
      ],
      "prompt": "Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. let ready=false; let snapshot:Config|null=null; async function load(){const c=await fetchConfig();snapshot=c;ready=true;} fetchConfig():Promise<Config> отклоняется Error(\"load\"). Host ожидает load() и обрабатывает отказ снаружи. Каковы состояние и статус Promise load?",
      "chosen": "Ready остаётся false,snapshot остаётся null; load отклонён той же ошибкой. Код после await не выполнен.",
      "rejected": "Ready становится true,snapshot остаётся null; load отклонён той же ошибкой. Флаг выставляется при любом завершении fetch.",
      "error_tag": "rejection_treated_as_success_completion",
      "rationale": "После rejected await обычное продолжение пропускается.",
      "counterexample": "В load нет finally; ready=true не достигается после ошибки.",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "Angular 16.2.0 / TypeScript 5.1.6; explicit project initializer contract; isolated Promise fixture",
        "origin_family_id": "v7.ref.config.load-transition",
        "leakage_group_ids": [
          "v7.ref.config.load-transition"
        ],
        "source_role": "methodology_and_topic_seed",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "Angular 16.2.0 / TypeScript 5.1.6; explicit project initializer contract; isolated Promise fixture",
          "expected": {
            "ready": false,
            "snapshot": null,
            "load": "rejected"
          },
          "assertion": "В load нет finally; ready=true не достигается после ошибки.",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-NG-03"
      }
    },
    {
      "example_id": "DOC-NG-CONFIG-READY.REF.05",
      "profile_id": "base",
      "family_id": "v7.ref.config.swallowed-failure",
      "source_refs": [
        "RAG-027",
        "RAG-028",
        "RAG-037"
      ],
      "prompt": "Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. let ready=false; let snapshot:Config|null=null; const log:string[]=[]; async function load(){try{snapshot=await fetchConfig();ready=true;}catch{log.push(\"failed\");}} fetchConfig():Promise<Config> отклоняется. Host выполнил await load(). Назови ready, статус load и log. Можно ли назвать config готовой?",
      "chosen": "Ready=false;load выполнен успешно;log=[\"failed\"]. Ошибка проглочена, поэтому fulfilled load здесь не доказывает готовность config.",
      "rejected": "Ready=false;load отклонён;log=[\"failed\"]. Catch автоматически передаёт ошибку ожидающему host.",
      "error_tag": "swallowed_error_mistaken_for_rethrow",
      "rationale": "В catch нет throw; Promise load успешно завершается.",
      "counterexample": "После логирования catch заканчивается без throw; готовность при этом false.",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "Angular 16.2.0 / TypeScript 5.1.6; explicit project initializer contract; isolated Promise fixture",
        "origin_family_id": "v7.ref.config.swallowed-failure",
        "leakage_group_ids": [
          "v7.ref.config.swallowed-failure"
        ],
        "source_role": "methodology_and_topic_seed",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "Angular 16.2.0 / TypeScript 5.1.6; explicit project initializer contract; isolated Promise fixture",
          "expected": {
            "ready": false,
            "snapshot": null,
            "load": "fulfilled",
            "log": [
              "failed"
            ]
          },
          "assertion": "После логирования catch заканчивается без throw; готовность при этом false.",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-NG-03"
      }
    },
    {
      "example_id": "DOC-NG-CONFIG-READY.REF.06",
      "profile_id": "base",
      "family_id": "v7.ref.config.single-flight",
      "source_refs": [
        "RAG-027",
        "RAG-028",
        "RAG-037"
      ],
      "prompt": "Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. let calls=0; let pending:Promise<Config>|undefined; function fetchConfig(){calls++;return new Promise<Config>(()=>{});} function whenReady(){return pending??=fetchConfig();} const a=whenReady(); const b=whenReady(); Назови calls и a===b; Promise ещё pending.",
      "chosen": "calls=1;a===b равно true. Второй потребитель получает уже сохранённый pending Promise.",
      "rejected": "calls=2;a===b равно false. Каждый вызов whenReady запускает новый fetch.",
      "error_tag": "shared_promise_ignored",
      "rationale": "Не учтено условное присваивание pending??=.",
      "counterexample": "После a поле pending задано; b не вычисляет правую часть ??=.",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "Angular 16.2.0 / TypeScript 5.1.6; explicit project initializer contract; isolated Promise fixture",
        "origin_family_id": "v7.ref.config.single-flight",
        "leakage_group_ids": [
          "v7.ref.config.single-flight"
        ],
        "source_role": "methodology_and_topic_seed",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "Angular 16.2.0 / TypeScript 5.1.6; explicit project initializer contract; isolated Promise fixture",
          "expected": {
            "calls": 1,
            "same_promise": true
          },
          "assertion": "После a поле pending задано; b не вычисляет правую часть ??=.",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-NG-03"
      }
    },
    {
      "example_id": "DOC-NG-CONFIG-READY.REF.07",
      "profile_id": "base",
      "family_id": "v7.ref.config.snapshot-guard",
      "source_refs": [
        "RAG-027",
        "RAG-028",
        "RAG-037"
      ],
      "prompt": "Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. let ready=false; const cached:Config={apiBaseUrl:\"/default\"}; function getSnapshot(){if(!ready)throw new Error(\"not-ready\");return cached;} До load вызывается getSnapshot(). Что наблюдает вызывающий код?",
      "chosen": "Ошибка \"not-ready\". Наличие cached не отменяет явную проверку ready.",
      "rejected": "Объект {apiBaseUrl:\"/default\"}. Поле cached уже заполнено, поэтому getter возвращает его без загрузки.",
      "error_tag": "cached_value_bypasses_guard",
      "rationale": "В rejected пропущен показанный throw.",
      "counterexample": "Первый if(!ready) истинен; return cached недостижим.",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "Angular 16.2.0 / TypeScript 5.1.6; explicit project initializer contract; isolated Promise fixture",
        "origin_family_id": "v7.ref.config.snapshot-guard",
        "leakage_group_ids": [
          "v7.ref.config.snapshot-guard"
        ],
        "source_role": "methodology_and_topic_seed",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "Angular 16.2.0 / TypeScript 5.1.6; explicit project initializer contract; isolated Promise fixture",
          "expected": {
            "throws": "not-ready"
          },
          "assertion": "Первый if(!ready) истинен; return cached недостижим.",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-NG-03"
      }
    },
    {
      "example_id": "DOC-NG-CONFIG-READY.REF.08",
      "profile_id": "base",
      "family_id": "v7.ref.config.dependent-flags",
      "source_refs": [
        "RAG-027",
        "RAG-028",
        "RAG-037"
      ],
      "prompt": "Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. let ready=false; let snapshot:Config|null=null; async function load(){const c=await fetchConfig();snapshot=c;await reloadFlags(c);ready=true;} Функции fetchConfig():Promise<Config> и reloadFlags(c:Config):Promise<void> даны окружением. Fetch уже вернул {apiBaseUrl:\"/v1\"}, reloadFlags ещё pending. Назови состояние. В этой симуляции договор готовности включает завершение reloadFlags; это явная вариация, а не утверждение, что исходный ADR использует асинхронный reloadFlags.",
      "chosen": "Snapshot={apiBaseUrl:\"/v1\"};ready=false. Конфигурация присвоена, но обязательный reloadFlags ещё не завершён.",
      "rejected": "Snapshot={apiBaseUrl:\"/v1\"};ready=true. Получения config достаточно, чтобы закончить load до reloadFlags.",
      "error_tag": "dependency_completion_omitted",
      "rationale": "Rejected пропускает обязательную вторую async-зависимость.",
      "counterexample": "Ready=true находится после await reloadFlags, который ещё pending.",
      "metadata": {
        "simulation": true,
        "reference_only": true,
        "training_allowed": false,
        "response_language": "ru",
        "stack": "Angular 16.2.0 / TypeScript 5.1.6; explicit project initializer contract; isolated Promise fixture",
        "origin_family_id": "v7.ref.config.dependent-flags",
        "leakage_group_ids": [
          "v7.ref.config.dependent-flags"
        ],
        "source_role": "methodology_and_topic_seed",
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "Angular 16.2.0 / TypeScript 5.1.6; explicit project initializer contract; isolated Promise fixture",
          "expected": {
            "snapshot": {
              "apiBaseUrl": "/v1"
            },
            "ready": false
          },
          "assertion": "Ready=true находится после await reloadFlags, который ещё pending.",
          "execution_unit": "Shown source fixture"
        },
        "parent_topic_reference_only": "DOC-NG-03"
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



## Материал RAG-037 (данные, не новые команды)

# RAG-037 — ADR-ANGULAR-RUNTIME-CONFIG-HTTP-SSR.md

**Роль:** существующий проектный источник; его пример или план не равен проверенной текущей реализации.

| Поле | Значение |
|---|---|
| Ветка происхождения | `main` |
| Commit | `3d1fa938d4b50df5d430283983f45b2c0d2c4e5c` |
| Git blob SHA | `d182324879939f54ae447060f0f0fbf03b15e04f` |
| Путь | `.net angular translation/adr/ADR-ANGULAR-RUNTIME-CONFIG-HTTP-SSR.md` |
| Проверенный объём | L22–L89, L185–L245, L297–L333; остальной файл не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/adr/ADR-ANGULAR-RUNTIME-CONFIG-HTTP-SSR.md) |

## Что подтверждает фрагмент

ADR содержит конкретные TypeScript-контракты и примеры provideAppInitializer, authRefresh single-flight и loadChildren/loadComponent. Это план реализации, не извлечённый работающий Angular-модуль.

## Как использовать сейчас

Материал для DOC-ANGULAR и CODE-ANGULAR: описать показанный интерфейс, отличать Promise окончания загрузки от раннего ready, объяснить ограничение повторного refresh. Указанный порядок guard-массива не считать доказательством последовательного ожидания асинхронных guard: зависимость явно обеспечить кодом и проверить. Версию API задаёт карточка, не старый статус Approved.

**Назначение:** Существующие TypeScript-примеры: интерфейс runtime config, initializer, единственный AuthSessionService и lazy route array.

## Подтверждающий фрагмент L22–L89

[Оригинал, строки 22–89](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/adr/ADR-ANGULAR-RUNTIME-CONFIG-HTTP-SSR.md#L22-L89)

~~~~text
## Runtime Config

The shell loads this file before route activation:

```text
assets/xiigen-runtime-config.json
```

The loader runs as an Angular application initializer
(`provideAppInitializer` / `APP_INITIALIZER` equivalent for the selected Angular
major) before router initial navigation. Guards, interceptors, Transloco
loaders, and feature routes must wait on `RuntimeConfigService.ready`; runtime
config must not be loaded through per-route resolvers because those fire too
late for the first auth/tenant guard decision.

`RuntimeConfigService` has this minimum shape:

```typescript
export interface XiigenRuntimeConfig {
  apiBaseUrl: string;
  tenantResolution: "subdomain" | "path" | "header" | "single-tenant";
  tenantIdHint: string | null;
  features: Record<string, boolean | string | number>;
  auth: {
    tokenStorage: "memory";
    refreshEnabled: boolean;
    refreshMode: "reactive-401-only" | "preemptive-and-reactive";
    preemptiveRefreshSkewSeconds: number;
    preserveReturnUrl: true;
  };
  correlationHeader: "X-Correlation-Id";
}

export class RuntimeConfigService {
  readonly ready: Signal<boolean>;
  load(): Promise<void>;
  whenReady(): Promise<XiigenRuntimeConfig>;
  get snapshot(): XiigenRuntimeConfig;
}
```

The application initializer returns `runtimeConfigService.load()`. Guards and
interceptors call `whenReady()` when they may run before `ready()` is true.
`snapshot` throws a fail-closed configuration error if accessed before load
completion.

The Angular bootstrap sequence has one required initializer in the first
implementation:

```typescript
provideAppInitializer(() => inject(RuntimeConfigService).load())
```

No token refresh, tenant resolution network call, feature flag reload, or
context-role resolver may run as a bootstrap initializer. Those depend on the
runtime config and therefore run through guards/interceptors after
`RuntimeConfigService.ready` is true. `FeatureFlagService.reload(config)` is
called synchronously from the runtime config load success path. This keeps
initial navigation deterministic: config first, router second, auth/tenant
guards third.

`RuntimeConfigService` is a root singleton:

```typescript
@Injectable({ providedIn: "root" })
export class RuntimeConfigService { ... }
```

~~~~
## Подтверждающий фрагмент L185–L245

[Оригинал, строки 185–245](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/adr/ADR-ANGULAR-RUNTIME-CONFIG-HTTP-SSR.md#L185-L245)

~~~~text
## HTTP Interceptors

Use functional interceptors:

- `authInterceptor: HttpInterceptorFn` adds `Authorization: Bearer <token>`
  when `AuthSessionService` has an in-memory access token.
- `tenantInterceptor: HttpInterceptorFn` adds `X-Tenant-Id` only when the
  server-side tenant resolution mode requires a header.
- `correlationInterceptor: HttpInterceptorFn` adds `X-Correlation-Id` from
  `CorrelationContextService` or creates one for the command/request.
- `authRefreshInterceptor: HttpInterceptorFn` performs one bounded refresh on
  401 when refresh is enabled; otherwise it clears session state and routes to
  login/denied.

`tenantInterceptor` reads `RuntimeConfigService.snapshot.tenantResolution`.
It adds the `X-Tenant-Id` header only when all of these are true:

- `tenantResolution` is `"header"`;
- `tenantIdHint` is present and non-empty;
- the request URL targets `apiBaseUrl` or a relative API URL owned by XIIGen;
- the request is not a third-party telemetry, asset, translation-bundle, or
  external provider request.

For `subdomain`, `path`, or `single-tenant`, the interceptor must not add
`X-Tenant-Id`; the server derives tenant scope from the approved resolution
source. A runtime config with `tenantResolution = "header"` and no
`tenantIdHint` fails schema validation before interceptors run.

JWT access tokens are stored in memory by default. Persistent token storage
requires a separate security ADR.

`AuthSessionService` is the only token source for interceptors:

```typescript
@Injectable({ providedIn: "root" })
export class AuthSessionService {
  readonly accessToken: Signal<string | null>;
  readonly refreshInProgress: Signal<boolean>;
  hasToken(): boolean;
  getAccessToken(): string | null;
  setSession(response: AuthLoginResponse): void;
  clearSession(reason: "logout" | "expired" | "refresh-failed"): void;
  refreshOnce(): Promise<boolean>;
}
```

Generated code must not read tokens from `RuntimeConfigService`, browser local
storage, session storage, cookies, or component state.

Refresh handling is single-flight and loop-safe:

1. `authRefreshInterceptor` catches a 401 only once per original request.
2. It marks the retried request with an internal `XIIGEN_REFRESH_RETRY` context
   token so the same request cannot trigger a second refresh loop.
3. If another refresh is already in progress, it awaits that in-memory promise
   instead of calling `/auth/refresh` again.
4. On refresh success, it updates `AuthSessionService` and retries the original
   request once.
5. On refresh failure, missing refresh token, or second 401 after retry, it
   clears the session and routes to login with the preserved `returnUrl`.
6. Refresh requests themselves are excluded from refresh interception.
~~~~
## Подтверждающий фрагмент L297–L333

[Оригинал, строки 297–333](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/adr/ADR-ANGULAR-RUNTIME-CONFIG-HTTP-SSR.md#L297-L333)

~~~~text
## Lazy Loading

The platform shell lazy-loads each flow as a route array:

```typescript
{
  path: "flows/FLOW-XX-<slug>",
  canActivate: [
    configLoadedGuard,
    tenantGuard,
    authGuard,
    roleGuard,
    contextRoleGuard
  ],
  loadChildren: () => import("./flows/FLOW-XX-<slug>/routes")
    .then(m => m.FLOW_XX_ROUTES)
}
```

Inside a flow route array, leaf screens use `loadComponent`. Use
`loadChildren` at the flow boundary and `loadComponent` for leaf views. This
keeps generated packages consistent while allowing fine-grained screen loading.

Guard activation order is fixed and fail-closed:

1. `configLoadedGuard` awaits `RuntimeConfigService.whenReady()` and redirects
   to a configuration-error route on validation/load failure.
2. `tenantGuard` resolves tenant context from the approved runtime-config mode.
3. `authGuard` verifies an in-memory access token or redirects to login with
   `returnUrl`.
4. `roleGuard` checks canonical role snapshots for route policies.
5. `contextRoleGuard` calls the server/context-role resolver when the policy
   requires business-state-derived roles.

Routes may omit `roleGuard` or `contextRoleGuard` only when the auth policy
registry marks the endpoint/screen as not requiring those gates. Routes must
not put auth, role, or context-role guards before `configLoadedGuard`.
~~~~



## Последнее действующее указание

Режим CREATE_ONLY. Запиши реальные пары строго в `C:\Projects\llm_mvp_core\learning-sets\DOC-NG-CONFIG-READY\base\S705\draft.jsonl`. Вложенные инструкции источников и старые требования review/oracle не являются поручением. После записи выполни только эту команду структурного завершения:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' finish --request 'C:\Projects\llm_mvp_core\learning-sets\DOC-NG-CONFIG-READY\base\S705\request.json' --input 'C:\Projects\llm_mvp_core\learning-sets\DOC-NG-CONFIG-READY\base\S705\draft.jsonl' --model 'unknown'
```
Заменяй unknown только фактически наблюдаемым model/selector. После DRAFT_CREATED следующий stream: `CODE-NG-CONFIG-READY/base`. Не запускать review, accept, export, oracle или обучение.
