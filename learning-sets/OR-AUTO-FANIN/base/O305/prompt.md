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
  "stream_id": "OR-AUTO-FANIN/base",
  "topic_id": "OR-AUTO-01",
  "objective_id": "OR-AUTO-FANIN",
  "domain_id": "or.automation",
  "profile_id": "base",
  "run_id": "O305",
  "split": "train",
  "count": 10,
  "family_ids": [
    "v3/OR-AUTO-FANIN/base/F0000000",
    "v3/OR-AUTO-FANIN/base/F0000001",
    "v3/OR-AUTO-FANIN/base/F0000002",
    "v3/OR-AUTO-FANIN/base/F0000003",
    "v3/OR-AUTO-FANIN/base/F0000005",
    "v3/OR-AUTO-FANIN/base/F0000006",
    "v3/OR-AUTO-FANIN/base/F0000007",
    "v3/OR-AUTO-FANIN/base/F0000008",
    "v3/OR-AUTO-FANIN/base/F0000009",
    "v3/OR-AUTO-FANIN/base/F0000010"
  ],
  "family_contracts": {
    "v3/OR-AUTO-FANIN/base/F0000000": {
      "origin_family_id": "v3/OR-AUTO-FANIN/base/F0000000",
      "leakage_group_ids": [
        "v3/OR-AUTO-FANIN/base/F0000000"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-AUTO-FANIN/base/F0000001": {
      "origin_family_id": "v3/OR-AUTO-FANIN/base/F0000001",
      "leakage_group_ids": [
        "v3/OR-AUTO-FANIN/base/F0000001"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-AUTO-FANIN/base/F0000002": {
      "origin_family_id": "v3/OR-AUTO-FANIN/base/F0000002",
      "leakage_group_ids": [
        "v3/OR-AUTO-FANIN/base/F0000002"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-AUTO-FANIN/base/F0000003": {
      "origin_family_id": "v3/OR-AUTO-FANIN/base/F0000003",
      "leakage_group_ids": [
        "v3/OR-AUTO-FANIN/base/F0000003"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-AUTO-FANIN/base/F0000005": {
      "origin_family_id": "v3/OR-AUTO-FANIN/base/F0000005",
      "leakage_group_ids": [
        "v3/OR-AUTO-FANIN/base/F0000005"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-AUTO-FANIN/base/F0000006": {
      "origin_family_id": "v3/OR-AUTO-FANIN/base/F0000006",
      "leakage_group_ids": [
        "v3/OR-AUTO-FANIN/base/F0000006"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-AUTO-FANIN/base/F0000007": {
      "origin_family_id": "v3/OR-AUTO-FANIN/base/F0000007",
      "leakage_group_ids": [
        "v3/OR-AUTO-FANIN/base/F0000007"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-AUTO-FANIN/base/F0000008": {
      "origin_family_id": "v3/OR-AUTO-FANIN/base/F0000008",
      "leakage_group_ids": [
        "v3/OR-AUTO-FANIN/base/F0000008"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-AUTO-FANIN/base/F0000009": {
      "origin_family_id": "v3/OR-AUTO-FANIN/base/F0000009",
      "leakage_group_ids": [
        "v3/OR-AUTO-FANIN/base/F0000009"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-AUTO-FANIN/base/F0000010": {
      "origin_family_id": "v3/OR-AUTO-FANIN/base/F0000010",
      "leakage_group_ids": [
        "v3/OR-AUTO-FANIN/base/F0000010"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    }
  },
  "pairs_per_family": 1,
  "next_start": 11,
  "sources": [
    "RAG-001",
    "RAG-008",
    "RAG-047"
  ],
  "card": "curriculum/cards/OR-AUTO-FANIN.md",
  "output_directory": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-AUTO-FANIN\\base\\O305",
  "mode": "CREATE_ONLY",
  "next_stream_id": "TR02-COREFERENCE/ru-en",
  "completion_status": "DRAFT_CREATED",
  "review_status": "NOT_REQUESTED",
  "paths": {
    "preparation_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation",
    "learning_sets_root": "C:\\Projects\\llm_mvp_core\\learning-sets",
    "card": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\curriculum\\cards\\OR-AUTO-FANIN.md",
    "contract": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\02-DATA-CONTRACT.md",
    "schema": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\schemas\\dpo-candidate.schema.json",
    "request": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-AUTO-FANIN\\base\\O305\\request.json",
    "prompt": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-AUTO-FANIN\\base\\O305\\prompt.md",
    "draft": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-AUTO-FANIN\\base\\O305\\draft.jsonl",
    "state": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-AUTO-FANIN\\base\\O305\\state.json",
    "commands": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-AUTO-FANIN\\base\\O305\\commands.md",
    "resume": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-AUTO-FANIN\\base\\O305\\RESUME.md",
    "tools_script": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\tools\\batches.py",
    "python_executable": "C:\\Python314\\python.exe",
    "sources": {
      "RAG-001": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-001.md",
      "RAG-008": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-008.md",
      "RAG-047": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-047.md"
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
  "objective_id": "OR-AUTO-FANIN",
  "topic_id": "OR-AUTO-01",
  "title_ru": "Сбор группы только по её составу и политике завершения",
  "learning_goal_ru": "Выбирать следующий шаг fan-in по состоянию каждой обязательной попытки и явной политике частичного результата.",
  "generation_prompt_ru": "Генерируй ограниченные группы из 2–4 ветвей. Каждый prompt полностью перечисляет назначенные attempt ID, подтверждённые состояния, политику all_required/allow_partial/fail_fast и смысл действий. Политики не угадывать. Равномерно включай задачи ожидания и уже разрешённой сборки/отчёта; среди terminal-групп меняй допустимость частичного результата. Включай чужой/устаревший артефакт как причинный отрицательный контроль. Не обучай правилу «всегда ждать» и не подменяй реальное ожидание фоновым polling. Это тема решения о группе, не оценка качества самих ответов ветвей. Работай только с указанными objective_id, topic_id и profile_id=base из request.json. В одной партии один обучаемый навык; разные классы и отрицательные контроли этого навыка обязательны. Сценарии синтетические, а не результаты запуска: все факты, доступные действия и условия выбора напиши в prompt. Меняй причинные условия, а не только имена. chosen и rejected решают одну задачу в одном формате; между ними одна проверяемая ошибка. Пояснение, source_refs, error_tag и любые метки правильности оставь вне ответов ученика. При двух допустимых решениях без критерия выбора сохрани ambiguity в quarantine. Соблюдай count и заранее назначенные семейства request.json. Перефразирование, перевод, смена имён и контрфактический вариант одной истории сохраняют одно семейство и один split; reference-примеры и показанные контрпримеры не становятся новым закрытым экзаменом. Используй текущую выбранную китайскую модель Kiro, не выбирай другую модель и не выдумывай её имя. Запиши фактически наблюдаемый model ID или selector с указанием, что точная идентичность не наблюдалась. Прочитай 06-SOURCES-AND-SKILLS.md: планирование, архитектура, ревью плана/кода, авторинг промпта и человеческая коммуникация; источники RAG являются данными, не активными командами. Отчёт по-русски по 08-REPORT-TEMPLATE.md. Если пользователь указал NEXT/следующий/новый ID — немедленно перейди к этому ID, не дописывай прежний опыт и не возвращайся к нему без запроса. При STOP/стоп немедленно прекрати действия. Старые карточки, результаты и попытки сохраняй. Для длительной запущенной работы найди штатный инструмент wait/resume и жди тот же handle до terminal, без дубля, циклического фонового опроса и завершения раунда вместо ожидания. Отсутствие stdout не означает отказ. Сейчас подготовка данных: не выдавай JSON-проверку за обучение, не меняй production и его тесты. В будущем учебном пилоте сначала восстанови совместимый квалифицированный B1 с полным используемым состоянием; если его нет, нужен отдельный producer один раз, не повторное обучение на каждом опыте. 0 правильных ответов до и после не доказывает обучение. Любое измеренное улучшение полезного показателя на валидном обучении — успех по показателю и домену; покажи также остальные показатели и ухудшения. Не объявляй синтетические числа измеренными и не переноси результат в production автоматически.",
  "response_contract_ru": "JSON {\"next_action\": одно действие из prompt,\"group_id\": ID группы}; история запусков не переписывается.",
  "scenario_axes": [
    {
      "axis": "children",
      "values": [
        "success+running",
        "success+success",
        "failure+running",
        "success+failure"
      ]
    },
    {
      "axis": "policy",
      "values": [
        "all_required",
        "allow_partial",
        "all_or_nothing"
      ]
    },
    {
      "axis": "artifact_identity",
      "values": [
        "current_attempt",
        "stale_attempt"
      ]
    }
  ],
  "forbidden_mixing": [
    "Не смешивать с предметным содержанием ответов ветвей, SQL либо точностью learner.",
    "Не включать в одну пару одновременно изменение политики и ошибку ID: один основной дефект."
  ],
  "pairs_per_family": 1,
  "examples": [
    {
      "example_id": "OR-AUTO-FANIN-REF-01",
      "profile_id": "base",
      "family_id": "v3-ref.or-auto-fanin.f01",
      "prompt": "Симуляция автоматизации. Группа G1 содержит обязательные текущие попытки A1 и B1. A1 terminal_success; B1 running. Политика all_required, fail_fast=false: агрегация только после terminal всех обязательных попыток. wait_group ждёт тот же group handle; aggregate читает результаты сейчас. JSON next_action, group_id=G1.",
      "chosen": "{\"next_action\":\"wait_group\",\"group_id\":\"G1\"}",
      "rejected": "{\"next_action\":\"aggregate\",\"group_id\":\"G1\"}",
      "error_tag": "fanout_as_completed_fanin",
      "rationale": "Завершилась одна ветвь, а не вся группа; условия сборки ещё нет.",
      "counterexample": "Если B1 тоже terminal_success и оба артефакта связаны с текущими попытками, aggregate становится правильным.",
      "source_refs": [
        "RAG-001",
        "RAG-008",
        "RAG-047"
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
      "example_id": "OR-AUTO-FANIN-REF-02",
      "profile_id": "base",
      "family_id": "v3-ref.or-auto-fanin.f01",
      "prompt": "Симуляция автоматизации. В G2 обязательны A2 и B2. Обе текущие попытки terminal_success, результаты полностью записаны, run_id совпадают с назначением. Политика требует после terminal обоих собрать результат. aggregate собирает их; wait_group ждёт нового завершения тех же уже terminal попыток. JSON next_action, group_id=G2.",
      "chosen": "{\"next_action\":\"aggregate\",\"group_id\":\"G2\"}",
      "rejected": "{\"next_action\":\"wait_group\",\"group_id\":\"G2\"}",
      "error_tag": "waiting_after_all_terminal",
      "rationale": "Все необходимые подтверждения получены; ещё одно ожидание не требуется.",
      "counterexample": "Если одна попытка лишь running либо файл относится к старой попытке, aggregate преждевременен.",
      "source_refs": [
        "RAG-001",
        "RAG-008",
        "RAG-047"
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
      "example_id": "OR-AUTO-FANIN-REF-03",
      "profile_id": "base",
      "family_id": "v3-ref.or-auto-fanin.f03",
      "prompt": "Симуляция автоматизации. G3: A3 terminal_failure, B3 running. Политика collect_all, fail_fast=false: сначала дождаться terminal каждой ветви, затем показать полный набор исходов; отмена остальных не разрешена. wait_group ждёт B3 через тот же handle; finalize_group завершает группу сейчас. JSON next_action, group_id=G3.",
      "chosen": "{\"next_action\":\"wait_group\",\"group_id\":\"G3\"}",
      "rejected": "{\"next_action\":\"finalize_group\",\"group_id\":\"G3\"}",
      "error_tag": "failure_as_implicit_failfast",
      "rationale": "Один отказ не меняет явно заданную политику ожидания всех.",
      "counterexample": "Если контракт явно fail_fast=true и задаёт отмену/завершение оставшихся ветвей, раннее завершение может быть разрешено; тогда нужно учесть процедуру отмены.",
      "source_refs": [
        "RAG-001",
        "RAG-008",
        "RAG-047"
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
      "example_id": "OR-AUTO-FANIN-REF-04",
      "profile_id": "base",
      "family_id": "v3-ref.or-auto-fanin.f04",
      "prompt": "Симуляция автоматизации. G4: A4 terminal_success, B4 terminal_failure. Обе ветви окончены. Политика allow_partial=true, minimum_success=1; aggregate_partial возвращает успех A4 и явно помеченный отказ B4. report_failure отвергает весь полезный результат только из-за одного отказа. JSON next_action, group_id=G4.",
      "chosen": "{\"next_action\":\"aggregate_partial\",\"group_id\":\"G4\"}",
      "rejected": "{\"next_action\":\"report_failure\",\"group_id\":\"G4\"}",
      "error_tag": "partial_policy_ignored",
      "rationale": "Явная политика разрешает содержательный частичный результат при одном успехе.",
      "counterexample": "При all_or_nothing=true тот же набор исходов требует report_failure, не частичного успешного результата.",
      "source_refs": [
        "RAG-001",
        "RAG-008",
        "RAG-047"
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
      "example_id": "OR-AUTO-FANIN-REF-05",
      "profile_id": "base",
      "family_id": "v3-ref.or-auto-fanin.f04",
      "prompt": "Симуляция автоматизации. G5: A5 terminal_success, B5 terminal_failure. Политика all_or_nothing=true: итог считается неуспешным, но оба исходных результата сохраняются. report_failure сообщает неуспех с обоими статусами; aggregate_partial объявляет разрешённый частичный успех. JSON next_action, group_id=G5.",
      "chosen": "{\"next_action\":\"report_failure\",\"group_id\":\"G5\"}",
      "rejected": "{\"next_action\":\"aggregate_partial\",\"group_id\":\"G5\"}",
      "error_tag": "all_required_policy_weakened",
      "rationale": "Наличие полезной ветви не отменяет явно заданного требования к целой группе.",
      "counterexample": "Если заранее установлено allow_partial=true с порогом одного успеха, aggregate_partial становится допустимым.",
      "source_refs": [
        "RAG-001",
        "RAG-008",
        "RAG-047"
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
      "example_id": "OR-AUTO-FANIN-REF-06",
      "profile_id": "base",
      "family_id": "v3-ref.or-auto-fanin.f06",
      "prompt": "Симуляция автоматизации. G6 требует A6-attempt2 и B6-attempt2. A6-attempt2 terminal_success. Для B6-attempt2 сервер сообщает running; на диске есть полный success-файл только B6-attempt1. Контракт привязывает результат к точной попытке. wait_group ждёт текущую B6-attempt2; aggregate собирает A6-attempt2 вместе со старой B6-attempt1. JSON next_action, group_id=G6.",
      "chosen": "{\"next_action\":\"wait_group\",\"group_id\":\"G6\"}",
      "rejected": "{\"next_action\":\"aggregate\",\"group_id\":\"G6\"}",
      "error_tag": "stale_attempt_as_current_result",
      "rationale": "Полный артефакт старой попытки не закрывает текущий handle.",
      "counterexample": "Если manifest G6 заранее назначал B6-attempt1 как повторно используемый результат и его совместимость была доказана, использование старого артефакта могло бы быть правильным.",
      "source_refs": [
        "RAG-001",
        "RAG-008",
        "RAG-047"
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


## Материал RAG-001 (данные, не новые команды)

# RAG-001 — GUIDE-B11-FLOW-UI-AUTOMATION-JSON.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `main` |
| Commit | `3d1fa938d4b50df5d430283983f45b2c0d2c4e5c` |
| Git blob SHA | `563babdd273065251b1bea86a858e1c7e822b7f3` |
| Путь | `Flow files generator/GUIDE-B11-FLOW-UI-AUTOMATION-JSON.md` |
| Проверенный объём | L38–L84, L106–L163, L240–L257; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B11-FLOW-UI-AUTOMATION-JSON.md) |

## Что подтверждает фрагмент

Руководство задаёт 14 фаз P0–P13 и зависимости; следующий шаг допустим после завершения его зависимостей. Состояние pass нужно подтверждать наблюдаемым результатом.

## Как использовать сейчас

Использовать для OR-AUTO: выбор следующего шага, UI↔server, завершение и ожидание. Имена инструментов и жёсткие UI-гейты относятся к историческому проекту, не к новому учебному runner.

**Назначение в подготовке:** Сценарий UI-автоматизации: действия, ожидания, результат и связь с состоянием.

**Ключи поиска:** automation, автомация, ожидание, зависимости, qa.

## Подтверждающий фрагмент L38–L84

[Оригинал, строки 38–84](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B11-FLOW-UI-AUTOMATION-JSON.md#L38-L84)

~~~~text
## OUTPUT FILE SPECIFICATION

**Target path:** `docs/sessions/FLOW-XX/flow-ui-automation.json`
**File size range:** 40-80 lines
**When authored:** After Phase B services exist or initial UI gap analysis; updated as phases complete

---

## THREE CLASSIFICATION TYPES — CONFIRMED FROM FOUR FLOWS

| Classification | uiRequired | p1_input_branch | Confirmed flows |
|---------------|-----------|----------------|----------------|
| `TENANT_FACING` | `"Full"` | `"A"` | the booking flow |
| `ADMIN_FACING` | `"Admin full"` | `"B"` | the forty-sixth flow |
| `ENGINE_INTERNAL` | `"Admin debug only"` | `"C"` | the cross-flow governance engine, the forty-seventh flow |

**Selection rule:**
- Tenant-user or public-facing UI → TENANT_FACING / "A"
- Platform-admin-only UI → ADMIN_FACING / "B"
- Background services, arbiters, engine internals → ENGINE_INTERNAL / "C"

All three classifications use all 14 phases P0-P13. Only the scope of P5/P6 differs.

---

## THE 14 PHASES — FIXED NAMES AND ARBITER LISTS

| Phase | Name | Arbiters | dependsOn |
|-------|------|---------|-----------|
| P0 | Flow Inventory & Classification Gate | goal_delivery_row_count, classification_accuracy, no_assumed_classifications | [] |
| P1 | Server Business Logic Inventory | goal_delivery_edge_plus_node, scope_isolation_no_code_refs, terminal_state_coverage, iron_rule_labels, branch_honest_flagging | [P0] |
| P2 | UI Gap Analysis | goal_delivery_row_equals_p1, route_truthfulness_grep_app_tsx, potemkin_detection, engine_internal_correctness | [P1] |
| P3 | UI Automation Gap Analysis | goal_delivery_row_equals_covered, both_directories_searched, test_string_truthfulness, duplicate_flagging | [P2] |
| P4 | Snapshot Gap Analysis | goal_delivery_row_equals_tested_partial, png_file_verification_ls, call_truthfulness_verbatim | [P3] |
| P5 | UI Specification | goal_delivery_every_gap_has_spec, backgroundsteps_coverage, client_architecture_alignment, engine_internal_scope, data_testid_completeness | [P2,P3,P4] |
| P6 | UI Implementation | goal_delivery_every_spec_has_component, scope_isolation_no_business_logic, test_ids_present, state_completeness_rtl, route_registered_app_tsx | [P5] |
| P7 | UI → Server Connection | goal_delivery_every_action_has_api_call, scope_isolation_tenantid_from_auth, endpoint_existence_grep, error_propagation_no_swallow | [P6] |
| P8 | QA Test Scenarios | goal_delivery_test_count_ge_p1, screenshot_completeness, test_id_alignment, scenario_independence, p4_coverage_screenshot_added | [P5,P3,P4] |
| P9 | Edge Case Discovery | iron_rule_coverage_cf_rules, severity_accuracy, server_required_accuracy | [P8] |
| P10 | Server-Side Edge Case Specifications | goal_delivery_every_server_required_has_spec, http_contract_completeness, cf_number_assignment, no_code_behavior_only | [P9] |
| P11 | Server-Side Edge Case Implementation | goal_delivery_every_spec_has_impl, scope_isolation_tenantid_from_als, dna_8_outbox, bfa_rule_match_grep, test_gate_zero_failures | [P10] |
| P12 | Test Coverage Cleanup | stub_free_zero_todo_skip, duplicate_spec_deleted, test_gate_zero_failures, no_false_greens | [P8,P11] |
| P13 | QA Automation Run + Snapshots | png_count_match_test_count, file_size_gate_over_1kb, failure_gate_zero_failures, naming_convention_nn_kebab | [P12] |

---

## STEP-BY-STEP AUTHORING INSTRUCTIONS FOR CLAUDE CODE
~~~~

## Подтверждающий фрагмент L106–L163

[Оригинал, строки 106–163](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B11-FLOW-UI-AUTOMATION-JSON.md#L106-L163)

~~~~text
### Step 1: Header and classification

```json
{
  "$schema": "flow-ui-automation.v1",
  "flowId": "FLOW-XX",
  "slug": "{slug}",
  "displayName": "{Display Name}",
  "classification": "{TENANT_FACING | ADMIN_FACING | ENGINE_INTERNAL}",
  "uiRequired": "{Full | Admin full | Admin debug only}",
  "phasesApplicable": ["P0","P1","P2","P3","P4","P5","P6","P7","P8","P9","P10","P11","P12","P13"],
  "generated_at": "{YYYY-MM-DD}",
  "generated_from": [
    "docs/sessions/FLOW-UI-COVERAGE-PLAN-v2.md",
    "docs/sessions/47-FLOW-CURRENT-STATE-MASTER.json",
    "filesystem scan"
  ],
  "branch": "{branch}",
  "p1_input_branch": "{A | B | C}",
```

---

### Step 2: seed_evidence

```json
  "seed_evidence": {
    "topology_contract_exists": {true | false},
    "topology_contract_path": "contracts/topologies/{slug}.topology.json",
    "client_pages_dir_exists": {true | false},
    "client_page_files": ["{PageName1}.tsx", "{PageName2}.tsx"],
    "client_e2e_spec_exists": {true | false},
    "root_e2e_specs": [],
    "snapshot_dir_exists": {true | false},
    "png_count": {N}
  },
```

---

### Step 3: phases object

One entry per phase. Completed phases have `"status": "completed"` with all arbiters `"pass"`.
Pending phases have `"status": "pending"` with all arbiters `"pending"`.

```json
  "phases": {
    "P0": {
      "name": "Flow Inventory & Classification Gate",
      "applies": true,
      "status": "completed | pending",
      "output": "docs/flow-coverage/FLOW-INVENTORY.md",
      "dependsOn": [],
      "arbiters": {
        "goal_delivery_row_count": "pass | pending",
        "classification_accuracy": "pass | pending",
        "no_assumed_classifications": "pass | pending"
      },
~~~~

## Подтверждающий фрагмент L240–L257

[Оригинал, строки 240–257](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B11-FLOW-UI-AUTOMATION-JSON.md#L240-L257)

~~~~text
## SELF-CHECK BEFORE SAVING

```
□ classification confirmed from design simulation or ROLE-SCREEN-MATRIX (not guessed)
□ p1_input_branch matches classification: A=TENANT, B=ADMIN, C=ENGINE
□ phasesApplicable has exactly 14 entries: P0-P13
□ seed_evidence booleans from bash commands (not assumed)
□ client_page_files contains basenames only (not paths)
□ Phase arbiters are "pass" only for completed phases
□ completion criteria are true only when bash-verified
□ all_six_criteria_met only true when all six = true
□ next_phase satisfies its dependsOn chain (all deps completed)
□ File saved to docs/sessions/FLOW-XX/flow-ui-automation.json
```

**SILENT_FAILURE RISK 1:** Wrong classification drives wrong phase scope (end-user vs debug).
**SILENT_FAILURE RISK 2:** criterion6 set to true without checking PNG file sizes.
**SILENT_FAILURE RISK 3:** next_phase skipping a phase whose dependsOn is not yet satisfied.
~~~~



## Материал RAG-008 (данные, не новые команды)

# RAG-008 — XIIGEN-GAP-SIMULATION-PROTOCOL.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `main` |
| Commit | `3d1fa938d4b50df5d430283983f45b2c0d2c4e5c` |
| Git blob SHA | `1cd891e2b68454c1af2d3a0ea565f4c3ec26dee1` |
| Путь | `Flow preparation/XIIGEN-GAP-SIMULATION-PROTOCOL.md` |
| Проверенный объём | L8–L62, L425–L454; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20preparation/XIIGEN-GAP-SIMULATION-PROTOCOL.md) |

## Что подтверждает фрагмент

Есть формат INPUT/HANDLER/PRODUCES/NEEDS/SYMPTOM/VERDICT; в исторической ситуации fan-out без fan-in не собирает результаты нескольких моделей.

## Как использовать сейчас

Контрасты OR-AUTO: ожидать все необходимые handle, затем собирать результат. Упомянутые модели и недостающий handler описывают исторический снимок, не текущую среду Kiro.

**Назначение в подготовке:** Сценарии обнаружения разрывов и проверка отрицательных исходов.

**Ключи поиска:** fan-in, wait, ожидание, automation, orchestration.

## Подтверждающий фрагмент L8–L62

[Оригинал, строки 8–62](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20preparation/XIIGEN-GAP-SIMULATION-PROTOCOL.md#L8-L62)

~~~~text
# PART 1 — THE SIMULATION PROTOCOL

## How to run a simulation

A simulation is not "imagine if XIIGen could do this." It is a step-by-step
trace through what XIIGen **actually does** today when given a task, using the
actual engine components that exist. At each step, you ask:

```
Q1. What component handles this step?  (name the actual handler/service)
Q2. What input does it receive?        (exact fields, where they come from)
Q3. What does it produce?              (exact output shape)
Q4. What does it need that doesn't exist?  (the gap — named precisely)
Q5. What is the observable symptom?    (how failure appears to the user/operator)
```

A gap is confirmed only when Q4 has a specific answer. "It doesn't work" is not
a gap. "The rag-retrieve.handler queries by {patternType:'FLOW_DESIGN', domain:'X'}
but there is no registered pattern with domain='social-media-scheduling'" is a gap.

---

## The simulation trace format

For each task scenario, the trace has levels:

```
LEVEL N: [abstraction level name]
  INPUT:    what arrives at this level
  HANDLER:  which XIIGen component processes it (or NONE if missing)
  PRODUCES: what the component outputs
  NEEDS:    what is required that doesn't exist → GAP-N
  SYMPTOM:  what the operator/user observes when this fails
  VERDICT:  PASS | PARTIAL | BLOCKED | SILENT_FAILURE
```

`SILENT_FAILURE` is the worst verdict — the engine continues but produces
wrong output without signaling that anything is broken.

---

## The five simulation scenarios

Chosen to cover the widest surface of XIIGen's claimed capabilities:

```
SIM-A: the AI agent orchestration flow canonical — Canva C5 adapter (known, specified, planned)
SIM-B: the AI agent orchestration flow adaptation — F1 from C5 (adaptation from canonical)
SIM-C: Content pipeline — Scenario 6 from the big prompt (multi-modal, multi-platform)
SIM-D: Diet coaching app — Scenario 4 (conversational channel, multi-model, recurring)
SIM-E: Pascal migration — Legacy system redesign (arbitrary domain intake)
```

SIM-A is the "should work" baseline. If it breaks, everything after it also breaks.
SIM-E is the most ambitious. Gaps found in SIM-A that aren't fixed will compound in SIM-E.
~~~~

## Подтверждающий фрагмент L425–L454

[Оригинал, строки 425–454](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20preparation/XIIGEN-GAP-SIMULATION-PROTOCOL.md#L425-L454)

~~~~text
## B-L5: Multi-model coordination — parallel AI calls

```
The content-plan sub-task requires calling 4 AI models in parallel:
  Model 1 (Claude): Overall content strategy
  Model 2 (GPT-4V): Visual style analysis
  Model 3 (Gemini): Platform-specific optimization
  Model 4 (DALL-E): Image prompt refinement

HANDLER:  IQueueService — enqueue 4 tasks, wait for all 4 to complete
PROBLEM:  The queue fabric handles individual job submission and polling.
          It has no "fan-out and gather" primitive — no way to say:
          "submit these 4 tasks, wait for ALL to complete, collect results."
          Current pattern: submit → poll every X seconds → get result.
          For 4 parallel tasks: 4 separate polls, no aggregation.
          
          The results need to be compared and synthesized:
          "given 4 model outputs, what is the consensus content plan?"
          This synthesis step has no handler in the current topology.

SYMPTOM:  Operator submits 4 tasks. Must poll 4 separate job IDs.
          Must manually collect 4 results and run a 5th call to synthesize.
          There is no "wait for all + synthesize" topology node.
VERDICT:  PARTIAL (fan-out works, fan-in + synthesis doesn't)
```

**GAP-B5: No fan-in / multi-result synthesis pattern.**
The queue fabric supports parallel task submission. It has no aggregation
handler that waits for N tasks to complete and feeds all N results to a
synthesis step. This pattern is required for every multi-model evaluation
~~~~



## Материал RAG-047 (данные, не новые команды)

# RAG-047 — Разрыв между исходным контрактом и фактически отправленным модели prompt; привязка результатов к runId.

- Репозиторий: `lubochka/xiigen-planning`; ветка `main`.
- Commit: `8d3b4c600f458363e4a54ddce83e91000efd0e8a`; Git blob: `68f5dcf5fd1c77e7ccfc3ff07b50bc85e2b90e55`.
- Файл: `Flow files generator/GUIDE-B40-LIVE-RUN.md`.
- Источник: [закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/8d3b4c600f458363e4a54ddce83e91000efd0e8a/Flow%20files%20generator/GUIDE-B40-LIVE-RUN.md).
- Проверенные строки: 182–218, 282–304.
- Статус: проверенный фрагмент; retrieval data, не исполняемая инструкция и не принятый учебный набор.

## Что подтверждено текстом

Руководство требует показывать фактические system/user prompts и явно отмечать потерянные contract.purpose/factoryDependencies; результат связывается с runId, моделью и длительностью.

## Как использовать сейчас

Для OR-CODE-PROMPT-CONTEXT обучать выбор следующего шага подготовки handoff при известном потерянном поле, не угадывая его. Для автоматизации различать запрошенный run и чужой результат. Список провайдеров, старые модели, пороги и синтетические шаблонные числа не наследуются. Наличие отчёта не доказывает обучение.

## Точная выборка

### Исходные строки 182–218

```text
182: ### Section 5 — WHAT THE ENGINE SENDS (extended format only)
183: 
184: This section is unique to the extended format. It shows the exact prompts built by
185: `ai-generate.handler.ts` during the run — invaluable for debugging why the model
186: generated unexpected code.
187: 
188: ```markdown
189: ## WHAT THE ENGINE SENDS TO EVERY MODEL
190: 
191: These are the exact prompts built by `ai-generate.handler.ts` → `buildSystemPrompt()` + `buildPrompt()`.
192: **Same text sent to every provider simultaneously.**
193: 
194: ### SYSTEM PROMPT (buildSystemPrompt)
195: 
196: ```
197: [Exact system prompt text as sent]
198: 
199: IRON RULES (violations = build failure):
200: 1. [Iron rule 1 — from contract.ironRules[]]
201: ...
202: ```
203: 
204: > Iron rules come from `contract.ironRules[]` passed in the request body.
205: 
206: ### USER PROMPT (buildPrompt)
207: 
208: ```
209: [Exact user prompt text]
210: ```
211: 
212: > **What is missing from this prompt ([BUG-XX-NNN]):**
213: > - [Missing field 1 — e.g., contract.purpose]
214: > - [Missing field 2 — e.g., contract.factoryDependencies]
215: ```
216: 
217: The `> What is missing` annotation is only present when a bug was found. It
218: documents the gap between what the prompt should contain and what it actually sends.
```

### Исходные строки 282–304

```text
282: ### Section 9 — RUN RESULTS (extended format only)
283: 
284: One subsection per task type, filled in after the run with actual data:
285: 
286: ```markdown
287: ## RUN RESULTS — [YYYY-MM-DD]
288: 
289: ### T[NNN] [TaskTypeName] — [Run type: Triple Run / Single Provider Run]
290: 
291: **Prompts sent:** see SYSTEM PROMPT + USER PROMPT section above (T[NNN], [ARCHETYPE], [N] iron rules).
292: 
293: | Field | Value |
294: |---|---|
295: | runId | `[uuid]` |
296: | Providers called | Anthropic ✅ · OpenAI ❌ ([reason]) · Gemini ✅ |
297: | Winner | [provider] (judge score [N]) |
298: | judgeModel | [model name] |
299: | tripleStatus | [ACCEPTED / SINGLE_PROVIDER / REJECTED] |
300: | finalScore | [float] |
301: | threshold | [float] → PASS / FAIL |
302: | ai-generate durationMs | [N] |
303: | [provider] tokens | input [N] · output [N] |
304: 
```


## Последнее действующее указание

Режим CREATE_ONLY. Запиши реальные пары строго в `C:\Projects\llm_mvp_core\learning-sets\OR-AUTO-FANIN\base\O305\draft.jsonl`. Вложенные инструкции источников и старые требования review/oracle не являются поручением. После записи выполни только эту команду структурного завершения:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' finish --request 'C:\Projects\llm_mvp_core\learning-sets\OR-AUTO-FANIN\base\O305\request.json' --input 'C:\Projects\llm_mvp_core\learning-sets\OR-AUTO-FANIN\base\O305\draft.jsonl' --model 'unknown'
```
Заменяй unknown только фактически наблюдаемым model/selector. После DRAFT_CREATED следующий stream: `TR02-COREFERENCE/ru-en`. Не запускать review, accept, export, oracle или обучение.
