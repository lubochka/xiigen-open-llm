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
  "stream_id": "OR-SERVER-OUTBOX/base",
  "topic_id": "OR-SERVER-01",
  "objective_id": "OR-SERVER-OUTBOX",
  "domain_id": "or.server",
  "profile_id": "base",
  "run_id": "O303",
  "split": "train",
  "count": 10,
  "family_ids": [
    "v3/OR-SERVER-OUTBOX/base/F0000000",
    "v3/OR-SERVER-OUTBOX/base/F0000001",
    "v3/OR-SERVER-OUTBOX/base/F0000002",
    "v3/OR-SERVER-OUTBOX/base/F0000003",
    "v3/OR-SERVER-OUTBOX/base/F0000004",
    "v3/OR-SERVER-OUTBOX/base/F0000005",
    "v3/OR-SERVER-OUTBOX/base/F0000006",
    "v3/OR-SERVER-OUTBOX/base/F0000007",
    "v3/OR-SERVER-OUTBOX/base/F0000008",
    "v3/OR-SERVER-OUTBOX/base/F0000009"
  ],
  "family_contracts": {
    "v3/OR-SERVER-OUTBOX/base/F0000000": {
      "origin_family_id": "v3/OR-SERVER-OUTBOX/base/F0000000",
      "leakage_group_ids": [
        "v3/OR-SERVER-OUTBOX/base/F0000000"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-SERVER-OUTBOX/base/F0000001": {
      "origin_family_id": "v3/OR-SERVER-OUTBOX/base/F0000001",
      "leakage_group_ids": [
        "v3/OR-SERVER-OUTBOX/base/F0000001"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-SERVER-OUTBOX/base/F0000002": {
      "origin_family_id": "v3/OR-SERVER-OUTBOX/base/F0000002",
      "leakage_group_ids": [
        "v3/OR-SERVER-OUTBOX/base/F0000002"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-SERVER-OUTBOX/base/F0000003": {
      "origin_family_id": "v3/OR-SERVER-OUTBOX/base/F0000003",
      "leakage_group_ids": [
        "v3/OR-SERVER-OUTBOX/base/F0000003"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-SERVER-OUTBOX/base/F0000004": {
      "origin_family_id": "v3/OR-SERVER-OUTBOX/base/F0000004",
      "leakage_group_ids": [
        "v3/OR-SERVER-OUTBOX/base/F0000004"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-SERVER-OUTBOX/base/F0000005": {
      "origin_family_id": "v3/OR-SERVER-OUTBOX/base/F0000005",
      "leakage_group_ids": [
        "v3/OR-SERVER-OUTBOX/base/F0000005"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-SERVER-OUTBOX/base/F0000006": {
      "origin_family_id": "v3/OR-SERVER-OUTBOX/base/F0000006",
      "leakage_group_ids": [
        "v3/OR-SERVER-OUTBOX/base/F0000006"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-SERVER-OUTBOX/base/F0000007": {
      "origin_family_id": "v3/OR-SERVER-OUTBOX/base/F0000007",
      "leakage_group_ids": [
        "v3/OR-SERVER-OUTBOX/base/F0000007"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-SERVER-OUTBOX/base/F0000008": {
      "origin_family_id": "v3/OR-SERVER-OUTBOX/base/F0000008",
      "leakage_group_ids": [
        "v3/OR-SERVER-OUTBOX/base/F0000008"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-SERVER-OUTBOX/base/F0000009": {
      "origin_family_id": "v3/OR-SERVER-OUTBOX/base/F0000009",
      "leakage_group_ids": [
        "v3/OR-SERVER-OUTBOX/base/F0000009"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    }
  },
  "pairs_per_family": 1,
  "next_start": 10,
  "sources": [
    "RAG-006",
    "RAG-OR-001"
  ],
  "card": "curriculum/cards/OR-SERVER-OUTBOX.md",
  "output_directory": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-SERVER-OUTBOX\\base\\O303",
  "mode": "CREATE_ONLY",
  "next_stream_id": "OR-MOBILE-ACK/base",
  "completion_status": "DRAFT_CREATED",
  "review_status": "NOT_REQUESTED",
  "paths": {
    "preparation_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation",
    "learning_sets_root": "C:\\Projects\\llm_mvp_core\\learning-sets",
    "card": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\curriculum\\cards\\OR-SERVER-OUTBOX.md",
    "contract": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\02-DATA-CONTRACT.md",
    "schema": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\schemas\\dpo-candidate.schema.json",
    "request": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-SERVER-OUTBOX\\base\\O303\\request.json",
    "prompt": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-SERVER-OUTBOX\\base\\O303\\prompt.md",
    "draft": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-SERVER-OUTBOX\\base\\O303\\draft.jsonl",
    "state": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-SERVER-OUTBOX\\base\\O303\\state.json",
    "commands": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-SERVER-OUTBOX\\base\\O303\\commands.md",
    "resume": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-SERVER-OUTBOX\\base\\O303\\RESUME.md",
    "tools_script": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\tools\\batches.py",
    "python_executable": "C:\\Python314\\python.exe",
    "sources": {
      "RAG-006": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-006.md",
      "RAG-OR-001": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-OR-001.md"
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
  "objective_id": "OR-SERVER-OUTBOX",
  "topic_id": "OR-SERVER-01",
  "title_ru": "Серверный переход между сохранением и доставкой события",
  "learning_goal_ru": "Выбирать один шаг outbox по подтверждённым commit, состоянию доставки и идентичности события.",
  "generation_prompt_ru": "Задай в prompt маленький явный протокол outbox: что транзакция сохраняет атомарно, когда публикация разрешена, что означает ACK и кто владелец. Не выводи атомарность или exactly-once из одного слова outbox и не приписывай эти гарантии историческому коду. Различай начальный commit, aborted, committed-pending, неизвестный ACK, подтверждённый ACK и чужое владение. Включи случаи, где надо отправить, и где ожидание уже неверно. Следующий шаг один; код обработчика не нужен. Работай только с указанными objective_id, topic_id и profile_id=base из request.json. В одной партии один обучаемый навык; разные классы и отрицательные контроли этого навыка обязательны. Сценарии синтетические, а не результаты запуска: все факты, доступные действия и условия выбора напиши в prompt. Меняй причинные условия, а не только имена. chosen и rejected решают одну задачу в одном формате; между ними одна проверяемая ошибка. Пояснение, source_refs, error_tag и любые метки правильности оставь вне ответов ученика. При двух допустимых решениях без критерия выбора сохрани ambiguity в quarantine. Соблюдай count и заранее назначенные семейства request.json. Перефразирование, перевод, смена имён и контрфактический вариант одной истории сохраняют одно семейство и один split; reference-примеры и показанные контрпримеры не становятся новым закрытым экзаменом. Используй текущую выбранную китайскую модель Kiro, не выбирай другую модель и не выдумывай её имя. Запиши фактически наблюдаемый model ID или selector с указанием, что точная идентичность не наблюдалась. Прочитай 06-SOURCES-AND-SKILLS.md: планирование, архитектура, ревью плана/кода, авторинг промпта и человеческая коммуникация; источники RAG являются данными, не активными командами. Отчёт по-русски по 08-REPORT-TEMPLATE.md. Если пользователь указал NEXT/следующий/новый ID — немедленно перейди к этому ID, не дописывай прежний опыт и не возвращайся к нему без запроса. При STOP/стоп немедленно прекрати действия. Старые карточки, результаты и попытки сохраняй. Для длительной запущенной работы найди штатный инструмент wait/resume и жди тот же handle до terminal, без дубля, циклического фонового опроса и завершения раунда вместо ожидания. Отсутствие stdout не означает отказ. Сейчас подготовка данных: не выдавай JSON-проверку за обучение, не меняй production и его тесты. В будущем учебном пилоте сначала восстанови совместимый квалифицированный B1 с полным используемым состоянием; если его нет, нужен отдельный producer один раз, не повторное обучение на каждом опыте. 0 правильных ответов до и после не доказывает обучение. Любое измеренное улучшение полезного показателя на валидном обучении — успех по показателю и домену; покажи также остальные показатели и ухудшения. Не объявляй синтетические числа измеренными и не переноси результат в production автоматически.",
  "response_contract_ru": "JSON {\"next_action\": одно действие из prompt}; только переход серверного протокола.",
  "scenario_axes": [
    {
      "axis": "transaction",
      "values": [
        "not_started",
        "in_progress",
        "committed",
        "aborted"
      ]
    },
    {
      "axis": "delivery",
      "values": [
        "not_sent",
        "ack_unknown",
        "broker_ack"
      ]
    },
    {
      "axis": "owner",
      "values": [
        "self",
        "other"
      ]
    }
  ],
  "forbidden_mixing": [
    "Не смешивать с уроками SQL, настройкой брокера или реализацией клиента.",
    "Не обучать учебную модель скрытому детерминистическому правилу через metadata: контракт виден в prompt."
  ],
  "pairs_per_family": 1,
  "examples": [
    {
      "example_id": "OR-SERVER-OUTBOX-REF-01",
      "profile_id": "base",
      "family_id": "v3-ref.or-server-outbox.f01",
      "prompt": "Симуляция сервера. Нужно сохранить заказ D и сообщить событие E. Контракт: транзакция commit_document_and_outbox атомарно сохраняет D и pending-запись E; relay может публиковать E только после commit. Ни D, ни E ещё не сохранены. publish_first отправляет E до commit. Выбери JSON next_action из commit_document_and_outbox, publish_first.",
      "chosen": "{\"next_action\":\"commit_document_and_outbox\"}",
      "rejected": "{\"next_action\":\"publish_first\"}",
      "error_tag": "publish_before_durable_state",
      "rationale": "Условие требует наличия устойчивого состояния до внешнего события; выбранная транзакция обеспечивает именно явно заданную атомарность.",
      "counterexample": "Если D и pending E уже подтверждённо committed, следующим действием может быть dispatch_outbox, а повторный commit не нужен.",
      "source_refs": [
        "RAG-006",
        "RAG-OR-001"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false,
        "reference_only": true,
        "training_allowed": false,
        "review_status": "semantic_review_required",
        "source_role": "methodology_not_runtime_contract",
        "simulation": true
      }
    },
    {
      "example_id": "OR-SERVER-OUTBOX-REF-02",
      "profile_id": "base",
      "family_id": "v3-ref.or-server-outbox.f02",
      "prompt": "Симуляция сервера. Транзакция D+outbox E вернула aborted; по контракту обе записи отсутствуют. Публикация допустима только по committed outbox. record_failure сохраняет terminal-ошибку попытки без публикации; dispatch_event публикует E сейчас. Повторный запуск этой попытки не разрешён. Верни JSON next_action.",
      "chosen": "{\"next_action\":\"record_failure\"}",
      "rejected": "{\"next_action\":\"dispatch_event\"}",
      "error_tag": "event_after_transaction_abort",
      "rationale": "Aborted не оставляет committed-основания для события; событие не должно сообщать о несохранённом заказе.",
      "counterexample": "При подтверждённом commit и pending E публикация становится разрешённой.",
      "source_refs": [
        "RAG-006",
        "RAG-OR-001"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false,
        "reference_only": true,
        "training_allowed": false,
        "review_status": "semantic_review_required",
        "source_role": "methodology_not_runtime_contract",
        "simulation": true
      }
    },
    {
      "example_id": "OR-SERVER-OUTBOX-REF-03",
      "profile_id": "base",
      "family_id": "v3-ref.or-server-outbox.f03",
      "prompt": "Симуляция сервера. D и pending E подтверждённо committed. Relay владеет записью E, других блокировок нет. Контракт требует доставить pending E; dispatch_outbox публикует сохранённое событие, wait_for_document ждёт сохранения D. Верни JSON next_action.",
      "chosen": "{\"next_action\":\"dispatch_outbox\"}",
      "rejected": "{\"next_action\":\"wait_for_document\"}",
      "error_tag": "waiting_after_durable_precondition",
      "rationale": "Нужная предпосылка уже доказана, поэтому повторное ожидание сохранения не выполняет требуемую доставку.",
      "counterexample": "Если commit ещё in_progress, ожидание его результата нужно до публикации.",
      "source_refs": [
        "RAG-006",
        "RAG-OR-001"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false,
        "reference_only": true,
        "training_allowed": false,
        "review_status": "semantic_review_required",
        "source_role": "methodology_not_runtime_contract",
        "simulation": true
      }
    },
    {
      "example_id": "OR-SERVER-OUTBOX-REF-04",
      "profile_id": "base",
      "family_id": "v3-ref.or-server-outbox.f04",
      "prompt": "Симуляция сервера. Relay отправил E, но транспорт оборвался до broker ACK. D+E committed, outbox ещё pending. Контракт at-least-once: retry_same_event повторяет тот же event_id=E, получатель дедуплицирует по E; create_new_event создаёт E2 и лишает retry прежней идентичности. Нужно продолжить доставку. Верни JSON next_action.",
      "chosen": "{\"next_action\":\"retry_same_event\"}",
      "rejected": "{\"next_action\":\"create_new_event\"}",
      "error_tag": "retry_changes_event_identity",
      "rationale": "При неизвестном ACK повтор сохраняет идентичность события; exactly-once из наличия outbox не заявляется.",
      "counterexample": "Если появляется отдельное новое бизнес-событие, его собственный новый ID корректен; это уже не повтор E.",
      "source_refs": [
        "RAG-006",
        "RAG-OR-001"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false,
        "reference_only": true,
        "training_allowed": false,
        "review_status": "semantic_review_required",
        "source_role": "methodology_not_runtime_contract",
        "simulation": true
      }
    },
    {
      "example_id": "OR-SERVER-OUTBOX-REF-05",
      "profile_id": "base",
      "family_id": "v3-ref.or-server-outbox.f05",
      "prompt": "Симуляция сервера. Для E получен подтверждённый broker ACK с тем же event_id; outbox pending. Контракт ACK означает устойчивое принятие брокером; следующий шаг — отметить доставку, а не доказывать обработку подписчиком. mark_published записывает published; send_again немедленно повторяет уже подтверждённую доставку. Верни JSON next_action.",
      "chosen": "{\"next_action\":\"mark_published\"}",
      "rejected": "{\"next_action\":\"send_again\"}",
      "error_tag": "delivery_retry_after_confirmed_ack",
      "rationale": "Подтверждённый ACK закрывает именно доставку брокеру; он не означает, что потребитель завершил свою работу.",
      "counterexample": "Если ACK не относится к E либо доставка не подтверждена, mark_published преждевременен.",
      "source_refs": [
        "RAG-006",
        "RAG-OR-001"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false,
        "reference_only": true,
        "training_allowed": false,
        "review_status": "semantic_review_required",
        "source_role": "methodology_not_runtime_contract",
        "simulation": true
      }
    },
    {
      "example_id": "OR-SERVER-OUTBOX-REF-06",
      "profile_id": "base",
      "family_id": "v3-ref.or-server-outbox.f06",
      "prompt": "Симуляция сервера. Pending E committed. Relay R1 имеет действующий lease на E; решение принимает R2 без права отправки. По контракту R2 должен ждать завершения владельца и не публиковать параллельно. wait_owner использует штатное ожидание E; dispatch_outbox отправляет E от R2 сейчас. Верни JSON next_action.",
      "chosen": "{\"next_action\":\"wait_owner\"}",
      "rejected": "{\"next_action\":\"dispatch_outbox\"}",
      "error_tag": "dispatch_without_relay_ownership",
      "rationale": "Committed-событие необходимо, но право конкретного relay также задано как обязательное условие.",
      "counterexample": "Если R2 уже получил валидное владение после завершения R1, dispatch_outbox для pending E допустим.",
      "source_refs": [
        "RAG-006",
        "RAG-OR-001"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false,
        "reference_only": true,
        "training_allowed": false,
        "review_status": "semantic_review_required",
        "source_role": "methodology_not_runtime_contract",
        "simulation": true
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


## Материал RAG-006 (данные, не новые команды)

# RAG-006 — GUIDE-B28-STEP-8-HANDOFF-CONTRACT.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `main` |
| Commit | `3d1fa938d4b50df5d430283983f45b2c0d2c4e5c` |
| Git blob SHA | `2f37cc0b56f1a11579fdd8fcbae9cf8b44f0b97f` |
| Путь | `Flow files generator/GUIDE-B28-STEP-8-HANDOFF-CONTRACT.md` |
| Проверенный объём | L66–L74, L137–L181; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B28-STEP-8-HANDOFF-CONTRACT.md) |

## Что подтверждает фрагмент

Handoff сопоставляет ограничения с правилами, структуру с I/O, критерии качества с проверками; часть примеров привязана к Nest/DI.

## Как использовать сейчас

Уроки OR-ARCH: не терять поля и потребителя при передаче. Не объявлять Nest-декораторы универсальным C#/.NET-контрактом.

**Назначение в подготовке:** Контракт передачи между шагами и артефактами.

**Ключи поиска:** handoff, контракт, архитектура, io.

## Подтверждающий фрагмент L66–L74

[Оригинал, строки 66–74](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B28-STEP-8-HANDOFF-CONTRACT.md#L66-L74)

~~~~text
## THE THREE MAPPINGS — UNIVERSAL STRUCTURE

Every handoff contract has exactly three mappings:

| Mapping | From | To | Consumer |
|---------|------|----|---------|
| Mapping 1 | NODE.constraints | Iron rules for genesis prompt | the code generation station (genesis) + the DNA compliance check station (compliance) |
| Mapping 2 | NODE.structure | Input/output contract | the code generation station (signature) + Phase C (tests) |
| Mapping 3 | NODE.quality.scoringCriteria | Arbiter checklist | Phase E (arbiter panel) |
~~~~

## Подтверждающий фрагмент L137–L181

[Оригинал, строки 137–181](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B28-STEP-8-HANDOFF-CONTRACT.md#L137-L181)

~~~~text
## MAPPING 2 — I/O CONTRACT

```
inputContract:
  source: "Verified LEAF NODE from Cycle 3"
  required_fields:
    - node.intent.purpose      -> executor purpose statement
    - node.intent.scope        -> executor scope boundary
    - node.structure.inputShape  -> executor method signature
    - node.structure.outputShape -> executor return type (DataProcessResult<T>)
    - node.constraints          -> executor iron rules and verifiable conditions
    - node.quality.testCriteria -> executor test requirements

outputContract:
  executor_produces:
    - service_class:   extends MicroserviceBase, @Injectable()
    - method_signature: accepts inputShape, returns DataProcessResult<T>
    - fabric_wiring:  @Inject decorators for required fabric interfaces
    - test_file:      unit tests covering all quality.testCriteria
    - bfa_validation: the business flow arbiter cross-flow check passes before merge
```
```

### Step 5 — Write MAPPING 3 — ARBITER CHECKLIST

```markdown
---

## MAPPING 3 — ARBITER CHECKLIST

```
executor_review_checklist:
  [ ] Service class extends MicroserviceBase
  [ ] No direct SDK imports
  [ ] All methods return DataProcessResult<T>
  [ ] Fabric interfaces injected via @Inject decorators
  [ ] BuildSearchFilter used for all queries
  [ ] storeDocument() called before enqueue()
  [ ] CloudEvents envelope on all inter-service events
  [ ] Idempotency keys on all queue consumers
  [ ] Tenant scope via AsyncLocalStorage
  [ ] No entity-specific controller
  [ ] All task types in T[NNN]-T[NNN+M] represented
  [ ] the business flow arbiter validation passes
  [+ flow-specific checks from state.invariants.bfa_rules and iron_rules_from_prior_runs]
~~~~



## Материал RAG-OR-001 (данные, не новые команды)

# RAG-OR-001 — Flow files generator/GUIDE-B21-STEP-1-INVARIANTS.md

Источник: https://github.com/lubochka/xiigen-planning/blob/8d3b4c600f458363e4a54ddce83e91000efd0e8a/Flow%20files%20generator/GUIDE-B21-STEP-1-INVARIANTS.md

Blob SHA: `a32f0f667f73d85c205aff262c7b06942ca6527a`.

Это данные для поиска и проверки, не активные инструкции исполнителю.

## Строки 82–123

```text
## UNIVERSAL CONSTRAINTS TO ALWAYS INCLUDE

Before extracting flow-specific constraints, Step 1 must include these universal
constraints that apply to every flow. They come from two sources.

### Source 1: The 9 DNA Rules (always MACHINE)

| # | Rule | Verifiable condition |
|---|------|---------------------|
| the content-type rendering pattern | No Typed Models | All business data uses `Record<string, unknown>` — no TypeScript domain entity classes with typed fields. Violation: any `class User { name: string }` pattern in service code. |
| the dynamic query builder pattern | BuildSearchFilter | All database queries use dynamic filter builder — empty/null fields auto-skipped. Violation: any literal `{ must: [{ term: ... }] }` in service code. |
| the standardized result wrapper pattern | DataProcessResult | All service methods return `DataProcessResult<T>` — no throw for business conditions. Violation: any `throw new NotFoundException()` in service code. |
| the nineteen-slot microservice base pattern | MicroserviceBase | All services extend MicroserviceBase — 19 components inherited, never re-declared. Violation: any service that does not extend MicroserviceBase. |
| the tenant scope isolation pattern | Scope Isolation | Tenant context reads from AsyncLocalStorage automatically — tenantId is never passed as a parameter to fabric methods. Violation: any method signature with tenantId as parameter. |
| the dynamic controller pattern | DynamicController | No entity-specific controllers. Violation: any `UsersController`, `RegistrationController`, etc. |
| the seventh architecture DNA pattern | Idempotency | All queue consumers deduplicate via idempotency keys before any processing. Violation: any queue consumer that processes without idempotency check. |
| the eighth architecture DNA pattern | Outbox Pattern | `storeDocument()` completes before `enqueue()` is called. Violation: `enqueue()` called before or without `storeDocument()`. |
| the ninth architecture DNA pattern | CloudEvents | All inter-service events use CloudEvents envelope via `createCloudEvent()`. Violation: any event published without CloudEvents envelope. |

**Classification:** All 9 DNA rules are MACHINE. A tenant changing any of these
breaks system guarantees that apply to all tenants.

### Source 2: D-HIST Locked Decisions (new flows, guide v2+)

These 8 decisions from `DECISIONS-LOCKED.md` §D-HIST are also MACHINE constraints
that must appear in Step 1 for all new flows. They do not replace the DNA rules —
they are additive:

| ID | Decision | Verifiable condition for Step 1 |
|----|----------|--------------------------------|
| a locked architecture decision | Fabric-First | Services import only interface tokens (`DATABASE_SERVICE`, etc.) via `@Inject()`. No SDK imports (`@elastic/elasticsearch`, `@anthropic-ai/sdk`) in service code. |
| a locked architecture decision | the atomic first-writer-wins lock at ORDER 1 | Queue consumers: `setIfAbsent()` idempotency check at ORDER 1 before any business logic. The idempotency key format is fixed. |
| a locked architecture decision | V39 (4 artifacts per external system) | Every external system dependency produces exactly: Interface + Factory + Base Skeleton + Generated Implementation. No shortcuts. |
| a locked architecture decision | broken object-level authorization/the request-scoped context storage tenant scoping | Tenant scoping via AsyncLocalStorage. `tenantId` never passed as parameter to fabric methods. `TenantContextMiddleware` sets context automatically. |
| a locked architecture decision | REGISTRATION archetype atomic | If the flow has REGISTRATION archetypes: `registerAtomically()` — no separate check-then-write sequence. `cycleBudget=3` for REGISTRATION tasks. |
| a locked architecture decision | Best-effort observer | Observer task types (analytics, notifications, audit logging) wrap entire handler in try/catch and return `DataProcessResult.success()` from catch — never `failure()`. |
| a locked architecture decision | SILENT_FAILURE → score-0 | Named checks that prevent SILENT_FAILURE violations use `score-0` severity, not `BUILD_FAILURE`. `machineConstants[]` values with `neverFromConfig: true` may never use `config.get()`. |
| a locked architecture decision | Read-path extension | For guarded services: read-path extension (not dual-write) when extending. Writer guard invariants stay single-sourced. |

Include all D-HIST entries in Step 1 even if the flow doesn't yet have task types
that exercise them — the goal is to make every subsequent cycle aware of them so
the context packages encode them into genesis prompts.
```

## Строки 180–195

```text
### Step 5 — Write USER INTENT (verbatim)

Extract verbatim from the flow spec. This is the user's original goal statement
for this flow — not rephrased. Format: a single "When ... [trigger], [action]."
sentence or short paragraph:

```markdown
---

## USER INTENT (verbatim)

> [Verbatim user intent from flow spec]
```

**For standard XIIGen flows, the pattern is:**
> "When [actor] [trigger event] on the XIIGen [platform area], [what the engine does] [for what purpose]."
```

## Строки 257–297

```text
### Step 8 — Write FREEDOM / MACHINE CLASSIFICATION

```markdown
---

## FREEDOM / MACHINE CLASSIFICATION

| Concern | Classification | Reason |
|---------|---------------|--------|
| Business logic thresholds | FREEDOM | Operators may tune confidence levels |
| Infrastructure routing | MACHINE | Fixed fabric interface pattern |
| DNA rule enforcement | MACHINE | Non-negotiable — code constraint |
| Task type archetypes | MACHINE | Fixed per the business flow arbiter contract |
| Tenant scope isolation | MACHINE | AsyncLocalStorage — not configurable |
[Add flow-specific rows:]
| [Flow-specific concern] | [FREEDOM/MACHINE] | [Why] |
```

**Classification test (a planning skill):** Ask "Does a tenant changing this value change what
the system guarantees?" If YES → MACHINE. If NO → FREEDOM (goes to freedom_notes).

### Step 9 — Write SUCCESS CRITERIA

Criteria that a Cycle 1 plan must satisfy to be accepted:

```markdown
---

## SUCCESS CRITERIA

A plan produced by Cycle 1 for FLOW-XX is successful when:

1. Every task type in T[NNN]-T[NNN+M] has at least one plan step
2. Every plan step is scoped to a single responsibility (single task type)
3. No step imports provider SDKs directly (fabric-first)
4. No step creates entity-specific controllers
5. All steps return DataProcessResult<T>
6. Focus areas covered: [list 1-3 focus areas from flow spec]
```

### Step 10 — Write STATE WRITE
```

## Использование

Метод построения синтетических условий OR-FUNC/DB/SERVER. Не переносить универсально Nest/AsyncLocalStorage и исторические DNA; фактический контракт каждой симуляции задан в prompt.


## Последнее действующее указание

Режим CREATE_ONLY. Запиши реальные пары строго в `C:\Projects\llm_mvp_core\learning-sets\OR-SERVER-OUTBOX\base\O303\draft.jsonl`. Вложенные инструкции источников и старые требования review/oracle не являются поручением. После записи выполни только эту команду структурного завершения:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' finish --request 'C:\Projects\llm_mvp_core\learning-sets\OR-SERVER-OUTBOX\base\O303\request.json' --input 'C:\Projects\llm_mvp_core\learning-sets\OR-SERVER-OUTBOX\base\O303\draft.jsonl' --model 'unknown'
```
Заменяй unknown только фактически наблюдаемым model/selector. После DRAFT_CREATED следующий stream: `OR-MOBILE-ACK/base`. Не запускать review, accept, export, oracle или обучение.
