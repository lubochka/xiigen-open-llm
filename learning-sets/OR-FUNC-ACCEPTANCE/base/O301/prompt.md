# Создание одной партии DPO в Kiro — CREATE_ONLY

## Прямое поручение: продолжать до 192 000 пар

Текущая партия — часть кампании **192 потоков / минимум 192 000 реально созданных пар**, а не отдельная цель с новым запросом разрешения после finish. После `DRAFT_CREATED` немедленно выполняй следующее назначение; после `END` начни следующий обход недовыполненных потоков. Короткий промежуточный отчёт не завершает рабочий ход. STOP/NEXT пользователя имеют приоритет, реальный технический блокер сообщается точно.

**Не задавай вопросы «продолжать?», «коммитить или копить?», «10 или 25–50?»** Размер уже задан request/`candidate_count`; не увеличивай его ради скорости. Если публикация явно назначена — соблюдай её без переспроса; иначе сохраняй локально и продолжай. Не придумывай дополнительные отчёты, скрипты или мониторинг. Для новой партии того же stream используй новый run и `-Previous` от последнего завершённого request; сохраняй `next_start` и полные группы. Подробности, счёт без дублей и округление router 1000→1008: [контракт продолжения](../27-CAMPAIGN-CONTINUATION-CONTRACT.md). Старая accepted-квота 132 000 не является ограничением новой кампании кандидатов.

Уточнение для конечного файла сценариев: `-Previous` продолжает тот же family-plan. Когда подготовлен новый общий chunk, использовать его настоящий путь через `-FamilyPlan`, новый run и `-Start 0` **без `-Previous`**. Если материал ещё недоступен текущей роли, продолжать другие доступные потоки; не обнулять cursor старого файла и не выдавать повторные сюжеты за новые пары. Порядок подготовки следующих chunk сохранён в контракте продолжения.

Visible operational explanations and brief action rationales must remain in English. Do not switch them to Chinese. Reports for Luba and metadata rationale remain in Russian; learner examples follow their assigned language profile. Do not produce long thought monologues. Repetitive or incoherent output is a failed response, not progress or training data; recover from the existing request/state without restarting the campaign. This governs visible output, not hidden reasoning.

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

Planning checkout: `C:/Users/lubao/Documents/xiigen`. Наборы создаются только под `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets`; инструкции — в соседнем `preparation`. Текущая папка Kiro или `llm_mvp_core` не меняет эти значения. `request.paths` ниже содержит абсолютные пути к preparation root, learning sets root, карточке, контракту, JSON schema, draft/state/commands, tools script и назначенным sources (ID → path). Используй эти значения буквально. Схема — `schemas/dpo-candidate.schema.json`, объяснение — `02-DATA-CONTRACT.md`; они включены в пакет. Не ищи другой DPO-формат в соседнем проекте.

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
  "stream_id": "OR-FUNC-ACCEPTANCE/base",
  "topic_id": "OR-FUNC-01",
  "objective_id": "OR-FUNC-ACCEPTANCE",
  "domain_id": "or.functional",
  "profile_id": "base",
  "run_id": "O301",
  "split": "train",
  "count": 10,
  "family_ids": [
    "v3/OR-FUNC-ACCEPTANCE/base/F0000000",
    "v3/OR-FUNC-ACCEPTANCE/base/F0000001",
    "v3/OR-FUNC-ACCEPTANCE/base/F0000002",
    "v3/OR-FUNC-ACCEPTANCE/base/F0000003",
    "v3/OR-FUNC-ACCEPTANCE/base/F0000004",
    "v3/OR-FUNC-ACCEPTANCE/base/F0000006",
    "v3/OR-FUNC-ACCEPTANCE/base/F0000007",
    "v3/OR-FUNC-ACCEPTANCE/base/F0000008",
    "v3/OR-FUNC-ACCEPTANCE/base/F0000009",
    "v3/OR-FUNC-ACCEPTANCE/base/F0000010"
  ],
  "family_contracts": {
    "v3/OR-FUNC-ACCEPTANCE/base/F0000000": {
      "origin_family_id": "v3/OR-FUNC-ACCEPTANCE/base/F0000000",
      "leakage_group_ids": [
        "v3/OR-FUNC-ACCEPTANCE/base/F0000000"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-FUNC-ACCEPTANCE/base/F0000001": {
      "origin_family_id": "v3/OR-FUNC-ACCEPTANCE/base/F0000001",
      "leakage_group_ids": [
        "v3/OR-FUNC-ACCEPTANCE/base/F0000001"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-FUNC-ACCEPTANCE/base/F0000002": {
      "origin_family_id": "v3/OR-FUNC-ACCEPTANCE/base/F0000002",
      "leakage_group_ids": [
        "v3/OR-FUNC-ACCEPTANCE/base/F0000002"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-FUNC-ACCEPTANCE/base/F0000003": {
      "origin_family_id": "v3/OR-FUNC-ACCEPTANCE/base/F0000003",
      "leakage_group_ids": [
        "v3/OR-FUNC-ACCEPTANCE/base/F0000003"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-FUNC-ACCEPTANCE/base/F0000004": {
      "origin_family_id": "v3/OR-FUNC-ACCEPTANCE/base/F0000004",
      "leakage_group_ids": [
        "v3/OR-FUNC-ACCEPTANCE/base/F0000004"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-FUNC-ACCEPTANCE/base/F0000006": {
      "origin_family_id": "v3/OR-FUNC-ACCEPTANCE/base/F0000006",
      "leakage_group_ids": [
        "v3/OR-FUNC-ACCEPTANCE/base/F0000006"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-FUNC-ACCEPTANCE/base/F0000007": {
      "origin_family_id": "v3/OR-FUNC-ACCEPTANCE/base/F0000007",
      "leakage_group_ids": [
        "v3/OR-FUNC-ACCEPTANCE/base/F0000007"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-FUNC-ACCEPTANCE/base/F0000008": {
      "origin_family_id": "v3/OR-FUNC-ACCEPTANCE/base/F0000008",
      "leakage_group_ids": [
        "v3/OR-FUNC-ACCEPTANCE/base/F0000008"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-FUNC-ACCEPTANCE/base/F0000009": {
      "origin_family_id": "v3/OR-FUNC-ACCEPTANCE/base/F0000009",
      "leakage_group_ids": [
        "v3/OR-FUNC-ACCEPTANCE/base/F0000009"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-FUNC-ACCEPTANCE/base/F0000010": {
      "origin_family_id": "v3/OR-FUNC-ACCEPTANCE/base/F0000010",
      "leakage_group_ids": [
        "v3/OR-FUNC-ACCEPTANCE/base/F0000010"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    }
  },
  "pairs_per_family": 1,
  "next_start": 11,
  "sources": [
    "RAG-004",
    "RAG-005",
    "RAG-OR-001"
  ],
  "card": "curriculum/cards/OR-FUNC-ACCEPTANCE.md",
  "output_directory": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-FUNC-ACCEPTANCE\\base\\O301",
  "mode": "CREATE_ONLY",
  "next_stream_id": "OR-DB-ATOMIC-CLAIM/base",
  "completion_status": "DRAFT_CREATED",
  "review_status": "NOT_REQUESTED",
  "paths": {
    "preparation_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation",
    "learning_sets_root": "C:\\Projects\\llm_mvp_core\\learning-sets",
    "card": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\curriculum\\cards\\OR-FUNC-ACCEPTANCE.md",
    "contract": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\02-DATA-CONTRACT.md",
    "schema": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\schemas\\dpo-candidate.schema.json",
    "request": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-FUNC-ACCEPTANCE\\base\\O301\\request.json",
    "prompt": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-FUNC-ACCEPTANCE\\base\\O301\\prompt.md",
    "draft": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-FUNC-ACCEPTANCE\\base\\O301\\draft.jsonl",
    "state": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-FUNC-ACCEPTANCE\\base\\O301\\state.json",
    "commands": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-FUNC-ACCEPTANCE\\base\\O301\\commands.md",
    "resume": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-FUNC-ACCEPTANCE\\base\\O301\\RESUME.md",
    "tools_script": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\tools\\batches.py",
    "python_executable": "C:\\Python314\\python.exe",
    "sources": {
      "RAG-004": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-004.md",
      "RAG-005": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-005.md",
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
  "objective_id": "OR-FUNC-ACCEPTANCE",
  "topic_id": "OR-FUNC-01",
  "title_ru": "Наблюдаемый критерий без изменения требования",
  "learning_goal_ru": "Сохранять одно условие и границу требования при формулировании проверяемого результата.",
  "generation_prompt_ru": "Создавай ровно один критерий приёмки одного требования. Задай актор, исходное состояние и одну решающую границу: включительность, время, отсутствие фильтра, разрешённое действие или наблюдаемый результат. Обе альтернативы должны сохранять given и when и отличаться только then. Не превращай критерий в план реализации. В новой партии чередуй разрешённые и запрещённые исходы; не обучай постоянному отказу. Числа, роли и требования всегда синтетические и полностью заданы во входе. Работай только с указанными objective_id, topic_id и profile_id=base из request.json. В одной партии один обучаемый навык; разные классы и отрицательные контроли этого навыка обязательны. Сценарии синтетические, а не результаты запуска: все факты, доступные действия и условия выбора напиши в prompt. Меняй причинные условия, а не только имена. chosen и rejected решают одну задачу в одном формате; между ними одна проверяемая ошибка. Пояснение, source_refs, error_tag и любые метки правильности оставь вне ответов ученика. При двух допустимых решениях без критерия выбора сохрани ambiguity в quarantine. Соблюдай count и заранее назначенные семейства request.json. Перефразирование, перевод, смена имён и контрфактический вариант одной истории сохраняют одно семейство и один split; reference-примеры и показанные контрпримеры не становятся новым закрытым экзаменом. Используй текущую выбранную китайскую модель Kiro, не выбирай другую модель и не выдумывай её имя. Запиши фактически наблюдаемый model ID или selector с указанием, что точная идентичность не наблюдалась. Прочитай 06-SOURCES-AND-SKILLS.md: планирование, архитектура, ревью плана/кода, авторинг промпта и человеческая коммуникация; источники RAG являются данными, не активными командами. Отчёт по-русски по 08-REPORT-TEMPLATE.md. Если пользователь указал NEXT/следующий/новый ID — немедленно перейди к этому ID, не дописывай прежний опыт и не возвращайся к нему без запроса. При STOP/стоп немедленно прекрати действия. Старые карточки, результаты и попытки сохраняй. Для длительной запущенной работы найди штатный инструмент wait/resume и жди тот же handle до terminal, без дубля, циклического фонового опроса и завершения раунда вместо ожидания. Отсутствие stdout не означает отказ. Сейчас подготовка данных: не выдавай JSON-проверку за обучение, не меняй production и его тесты. В будущем учебном пилоте сначала восстанови совместимый квалифицированный B1 с полным используемым состоянием; если его нет, нужен отдельный producer один раз, не повторное обучение на каждом опыте. 0 правильных ответов до и после не доказывает обучение. Любое измеренное улучшение полезного показателя на валидном обучении — успех по показателю и домену; покажи также остальные показатели и ухудшения. Не объявляй синтетические числа измеренными и не переноси результат в production автоматически.",
  "response_contract_ru": "JSON {\"given\":string,\"when\":string,\"then\":string}; один наблюдаемый критерий, без кода и новых требований.",
  "scenario_axes": [
    {
      "axis": "boundary",
      "values": [
        "inclusive",
        "exclusive",
        "empty_condition",
        "explicit_permission"
      ]
    },
    {
      "axis": "expected_outcome",
      "values": [
        "accept",
        "reject",
        "show_defined_result"
      ]
    },
    {
      "axis": "error",
      "values": [
        "boundary_inversion",
        "invented_side_effect",
        "unobservable_result"
      ]
    }
  ],
  "forbidden_mixing": [
    "Не обучать написанию кода или SQL, дизайну экрана либо выбору runtime-роута.",
    "Не смешивать критерии нескольких требований в одну пару."
  ],
  "pairs_per_family": 1,
  "examples": [
    {
      "example_id": "OR-FUNC-ACCEPTANCE-REF-01",
      "profile_id": "base",
      "family_id": "v3-ref.or-func-acceptance.f01",
      "prompt": "Симуляция. В форме разрешены от 1 до 5 вложений включительно. Рассматривается только количество, все файлы допустимы. Запиши один критерий для пяти вложений в JSON given, when, then; не добавляй другие функции.",
      "chosen": "{\"given\":\"Выбрано ровно 5 допустимых вложений\",\"when\":\"Пользователь отправляет форму\",\"then\":\"Форма принимает 5 вложений\"}",
      "rejected": "{\"given\":\"Выбрано ровно 5 допустимых вложений\",\"when\":\"Пользователь отправляет форму\",\"then\":\"Форма отклоняет 5 вложений\"}",
      "error_tag": "inclusive_boundary_inverted",
      "rationale": "Пять входит в явно заданную включительную границу.",
      "counterexample": "Если верхняя граница исключительная или разрешено не более 4, отклонение ровно 5 становится правильным.",
      "source_refs": [
        "RAG-004",
        "RAG-005",
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
      "example_id": "OR-FUNC-ACCEPTANCE-REF-02",
      "profile_id": "base",
      "family_id": "v3-ref.or-func-acceptance.f02",
      "prompt": "Симуляция. Экспорт списка должен создавать файл только при наличии хотя бы одной строки. Пустой результат должен показывать сообщение «Нет строк для экспорта», не создавая файл. Составь один критерий для пустого списка: JSON given, when, then.",
      "chosen": "{\"given\":\"Список не содержит строк\",\"when\":\"Пользователь нажимает экспорт\",\"then\":\"Показано «Нет строк для экспорта»; файл не создан\"}",
      "rejected": "{\"given\":\"Список не содержит строк\",\"when\":\"Пользователь нажимает экспорт\",\"then\":\"Показано «Нет строк для экспорта»; пустой файл создан\"}",
      "error_tag": "invented_export_side_effect",
      "rationale": "Запрет создания файла относится именно к пустому результату; rejected добавляет один запрещённый эффект.",
      "counterexample": "Если требование прямо велит создавать файл с заголовками и без строк, создание такого файла становится правильным.",
      "source_refs": [
        "RAG-004",
        "RAG-005",
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
      "example_id": "OR-FUNC-ACCEPTANCE-REF-03",
      "profile_id": "base",
      "family_id": "v3-ref.or-func-acceptance.f03",
      "prompt": "Симуляция. Правило редактирования: только автор может изменять свой комментарий; модератор без авторства может скрывать, но не редактировать текст. Пользователь M — модератор, автор комментария U. Нужен критерий попытки M изменить текст; JSON given, when, then.",
      "chosen": "{\"given\":\"M не является автором комментария U\",\"when\":\"M пытается сохранить изменённый текст\",\"then\":\"Изменение отклонено; исходный текст сохранён\"}",
      "rejected": "{\"given\":\"M не является автором комментария U\",\"when\":\"M пытается сохранить изменённый текст\",\"then\":\"Изменение принято; новый текст сохранён\"}",
      "error_tag": "ungranted_role_capability",
      "rationale": "Роль модератора даёт ровно указанное право скрытия, а не редактирования.",
      "counterexample": "Если M является автором или правило явно выдаёт модератору право редактирования, принятие изменения допустимо.",
      "source_refs": [
        "RAG-004",
        "RAG-005",
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
      "example_id": "OR-FUNC-ACCEPTANCE-REF-04",
      "profile_id": "base",
      "family_id": "v3-ref.or-func-acceptance.f04",
      "prompt": "Симуляция. При изменении названия папки подтверждение происходит только после сохранения на сервере. Сервер ответил «сохранено», новое название видно при повторном открытии. Сформулируй наблюдаемый критерий завершения, JSON given, when, then. Не добавляй журнал и уведомления.",
      "chosen": "{\"given\":\"Сервер подтвердил сохранение нового названия\",\"when\":\"Пользователь повторно открывает папку\",\"then\":\"Показано новое название папки\"}",
      "rejected": "{\"given\":\"Сервер подтвердил сохранение нового названия\",\"when\":\"Пользователь повторно открывает папку\",\"then\":\"Функция переименования работает правильно\"}",
      "error_tag": "unobservable_acceptance",
      "rationale": "Правильный критерий называет наблюдаемое значение; общая оценка не задаёт проверку.",
      "counterexample": "Если требуется только текстовая оценка ревью, а не критерий приёмки, форма ответа может быть иной; это отдельная задача.",
      "source_refs": [
        "RAG-004",
        "RAG-005",
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
      "example_id": "OR-FUNC-ACCEPTANCE-REF-05",
      "profile_id": "base",
      "family_id": "v3-ref.or-func-acceptance.f05",
      "prompt": "Симуляция. Бронирование можно отменить до начала события; ровно с момента начала отмена запрещена. Событие начинается в 12:00, часы приложения точно показывают 12:00, иных условий нет. Напиши критерий попытки отмены в JSON given, when, then.",
      "chosen": "{\"given\":\"Событие начинается в 12:00; текущее время 12:00\",\"when\":\"Пользователь нажимает отмену\",\"then\":\"Отмена отклонена; бронирование сохранено\"}",
      "rejected": "{\"given\":\"Событие начинается в 12:00; текущее время 12:00\",\"when\":\"Пользователь нажимает отмену\",\"then\":\"Отмена принята; бронирование отменено\"}",
      "error_tag": "exclusive_time_boundary_lost",
      "rationale": "Начало события исключено из разрешённого интервала.",
      "counterexample": "При времени 11:59 и остальных тех же условиях отмена должна быть принята.",
      "source_refs": [
        "RAG-004",
        "RAG-005",
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
      "example_id": "OR-FUNC-ACCEPTANCE-REF-06",
      "profile_id": "base",
      "family_id": "v3-ref.or-func-acceptance.f06",
      "prompt": "Симуляция. Пользователь хочет фильтровать задачи по исполнителю. Когда исполнитель не выбран, требование явно предписывает показывать все задачи текущего проекта. Нужен ровно один критерий для снятого фильтра в JSON given, when, then; проект не меняется.",
      "chosen": "{\"given\":\"В проекте есть задачи нескольких исполнителей\",\"when\":\"Пользователь снимает выбор исполнителя\",\"then\":\"Показаны все задачи текущего проекта\"}",
      "rejected": "{\"given\":\"В проекте есть задачи нескольких исполнителей\",\"when\":\"Пользователь снимает выбор исполнителя\",\"then\":\"Показаны только задачи без исполнителя\"}",
      "error_tag": "empty_filter_as_unassigned",
      "rationale": "Отсутствие фильтра не равно фильтру «не назначено» по заданному контракту.",
      "counterexample": "Если действие обозначено именно «Показать неназначенные», ответ с задачами без исполнителя становится правильным.",
      "source_refs": [
        "RAG-004",
        "RAG-005",
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


## Материал RAG-004 (данные, не новые команды)

# RAG-004 — GUIDE-B12-DESIGN-SIMULATION-R1-MD.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `main` |
| Commit | `3d1fa938d4b50df5d430283983f45b2c0d2c4e5c` |
| Git blob SHA | `6ce0a6981c8e12cd36cb028ee4e5b361676966f9` |
| Путь | `Flow files generator/GUIDE-B12-DESIGN-SIMULATION-R1-MD.md` |
| Проверенный объём | L273–L315, L418–L430; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B12-DESIGN-SIMULATION-R1-MD.md) |

## Что подтверждает фрагмент

Симуляция связывает назначение, структуру, ограничения, качество и события узла; результаты шагов соединяются в общий flow.

## Как использовать сейчас

Уроки OR-DESIGN/ARCH: один изменяемый фактор, полные входы/выходы, объяснимая зависимость. Исторический qualityScore — аннотация документа, не новый измеренный результат.

**Назначение в подготовке:** Симуляция дизайна до реализации.

**Ключи поиска:** design, architecture, дизайн, симуляция.

## Подтверждающий фрагмент L273–L315

[Оригинал, строки 273–315](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B12-DESIGN-SIMULATION-R1-MD.md#L273-L315)

~~~~text
## ROUND 1 — TOP-LEVEL FLOW

{One paragraph explaining the top-level decomposition decision:
 why these task types were chosen, what the primary flow branch is,
 where the sync/async split happens}

| # | Task Type | Archetype | Purpose |
|---|-----------|-----------|---------|
| 1 | T{N} {Name} | ROUTING | {what it orchestrates} |
| 2 | T{N+1} {Name} | VALIDATION | {what it validates} |
...
```

---

### Step 7: Write ROUND 2 — SUBFLOW DECISION

```markdown
## ROUND 2 — SUBFLOW DECISION

{Explain the branching decisions: what triggers each subflow,
 which task types belong to which path, where time-decoupling happens}
```

---

### Step 8: Write COMPLETE FLOW ASSEMBLY and FIVE DIMENSIONS

```markdown
## COMPLETE FLOW ASSEMBLY

{The assembled flow with all branches shown — describes how
 the task types connect via events and queues}

## FIVE DIMENSIONS — EVERY NODE

For each node in the flow, five dimensions are verified:
1. Structure: archetype, input event/trigger, output event
2. Intent: what business goal this node achieves
3. Constraints: MACHINE rules (no override) + FREEDOM configs
4. Quality: the preference learning signal score threshold, pattern type
5. Events: completion event fired + consumed events list
```
~~~~

## Подтверждающий фрагмент L418–L430

[Оригинал, строки 418–430](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B12-DESIGN-SIMULATION-R1-MD.md#L418-L430)

~~~~text
## the preference learning signal TRIPLES — {N} NEW PATTERNS PRODUCED

\`\`\`
PATTERN-name 001
  teachingPoint: "{transferable principle applicable to ≥2 future flows}"
  qualityScore: 0.NN
  observedIn: [FLOW-XX (T{NNN})]
  applies_to: [FLOW-{XX+N}, FLOW-{XX+M}]
\`\`\`
```

Only include patterns that are NEW to this flow — not already in archive batch 08/09/10/11/12.
Each pattern must be applicable to ≥2 future flows.
~~~~



## Материал RAG-005 (данные, не новые команды)

# RAG-005 — GUIDE-B17-IMPLEMENTATION-PLAN.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `main` |
| Commit | `3d1fa938d4b50df5d430283983f45b2c0d2c4e5c` |
| Git blob SHA | `73e234e3df128e8d976af0ec15ded7bf7d15aba1` |
| Путь | `Flow files generator/GUIDE-B17-IMPLEMENTATION-PLAN.md` |
| Проверенный объём | L81–L105, L124–L143, L333–L349; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B17-IMPLEMENTATION-PLAN.md) |

## Что подтверждает фрагмент

План связывает пользовательскую цель с фазами, конкретным сервисом, контрактом и поведенческими проверками.

## Как использовать сейчас

Уроки OR-CODE: исполнимый ограниченный work order. Фазы полного внедрения, approval и глобальные тестовые гейты не запускать для подготовки данных или ещё не доказанного опыта.

**Назначение в подготовке:** План реализации с конкретными зависимостями и ожидаемыми изменениями.

**Ключи поиска:** plan, код, планирование, implementation.

## Подтверждающий фрагмент L81–L105

[Оригинал, строки 81–105](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B17-IMPLEMENTATION-PLAN.md#L81-L105)

~~~~text
## THE IMPLEMENTATION PLAN STRUCTURE

Every implementation plan has this skeleton:

```
1. File header
2. Goal advancement block (a planning skill Gate D)
3. Preamble (session type, base commit, prerequisites)
4. Execution model (phases listed)
5. Absolute gate (5-gate block — runs at every ⛔ STOP)
6. Found-issue protocol
7. Phase A — Corpus seed
8. Phase B — Service implementations (per task type)
9. Phase C — Client pages (if any React pages in this flow)
10. Phase D — Admin panel / secondary surfaces (if any)
11. Phase E — Integration + E2E tests + the preference learning signal verification
12. Phase F — the business flow arbiter cross-flow validation + Promote to ACTIVE
13. [C33] Phase 7 — UI/UX Compliance audit (if React pages produced)
14. Documentation artifacts shipped (Rule 17 checklist)
15. Issue inventory
16. Delivery summary
```

Phases A–F are the standard execution sequence. Phase 7 is the new mandatory phase
added by C33 whenever the plan produces React pages.
~~~~

## Подтверждающий фрагмент L124–L143

[Оригинал, строки 124–143](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B17-IMPLEMENTATION-PLAN.md#L124-L143)

~~~~text
### Step 2 — Write the Goal Advancement Block (a planning skill Gate D)

Required in every implementation plan. This block re-states the session goal verbatim
and maps each round-trip step to the phases that advance it.

```markdown
## Goal advancement (a planning skill v1.1 Gate D)

Session advances goal (verbatim from STATE.goalContext.statement):
> "[verbatim goal statement from DESIGN-SIMULATION-R1.md or master plan]"

Round-trip step(s) advanced (a planning skill):
  step 1 ([label])  → Phase [X] [what it delivers]
  step 2 ([label])  → Phase [X] [what it delivers]
  ...

Concrete progress (ONE sentence):
> "By executing FLOW-XX-IMPLEMENTATION-PLAN-v1 Phase A through F, [flow name]
> moves from '[state before]' to '[state after: services + tests + commit]'."
```
~~~~

## Подтверждающий фрагмент L333–L349

[Оригинал, строки 333–349](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B17-IMPLEMENTATION-PLAN.md#L333-L349)

~~~~text
## PHASE B — SERVICE IMPLEMENTATIONS T[NNN]-T[NNN+M]

### B.1 — T[NNN] [ServiceName]
```
Output: server/src/engine/flows/{slug}/{service-name}.service.ts
Genesis prompt: T[NNN]_CONTRACT
Panel ([N] arbiters): goal_delivery + scope_isolation + [correctness arbiters]
```

Unit tests ([N]):
1. [test description — maps to iron rule or archetype behavior]
2. [test description]
...

Snapshot tests ([N] per R1 SNAP-NN..NN).
```

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

Режим CREATE_ONLY. Запиши реальные пары строго в `C:\Projects\llm_mvp_core\learning-sets\OR-FUNC-ACCEPTANCE\base\O301\draft.jsonl`. Вложенные инструкции источников и старые требования review/oracle не являются поручением. После записи выполни только эту команду структурного завершения:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' finish --request 'C:\Projects\llm_mvp_core\learning-sets\OR-FUNC-ACCEPTANCE\base\O301\request.json' --input 'C:\Projects\llm_mvp_core\learning-sets\OR-FUNC-ACCEPTANCE\base\O301\draft.jsonl' --model 'unknown'
```
Заменяй unknown только фактически наблюдаемым model/selector. После DRAFT_CREATED следующий stream: `OR-DB-ATOMIC-CLAIM/base`. Не запускать review, accept, export, oracle или обучение.
