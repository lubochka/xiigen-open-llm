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
  "stream_id": "OR-UX-RECOVERY/base",
  "topic_id": "OR-UX-01",
  "objective_id": "OR-UX-RECOVERY",
  "domain_id": "or.ux",
  "profile_id": "base",
  "run_id": "M018",
  "split": "train",
  "count": 10,
  "family_ids": [
    "v3/OR-UX-RECOVERY/base/F0000000",
    "v3/OR-UX-RECOVERY/base/F0000002",
    "v3/OR-UX-RECOVERY/base/F0000004",
    "v3/OR-UX-RECOVERY/base/F0000005",
    "v3/OR-UX-RECOVERY/base/F0000006",
    "v3/OR-UX-RECOVERY/base/F0000007",
    "v3/OR-UX-RECOVERY/base/F0000008",
    "v3/OR-UX-RECOVERY/base/F0000009",
    "v3/OR-UX-RECOVERY/base/F0000010",
    "v3/OR-UX-RECOVERY/base/F0000011"
  ],
  "family_contracts": {
    "v3/OR-UX-RECOVERY/base/F0000000": {
      "origin_family_id": "v3/OR-UX-RECOVERY/base/F0000000",
      "leakage_group_ids": [
        "v3/OR-UX-RECOVERY/base/F0000000"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-UX-RECOVERY/base/F0000002": {
      "origin_family_id": "v3/OR-UX-RECOVERY/base/F0000002",
      "leakage_group_ids": [
        "v3/OR-UX-RECOVERY/base/F0000002"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-UX-RECOVERY/base/F0000004": {
      "origin_family_id": "v3/OR-UX-RECOVERY/base/F0000004",
      "leakage_group_ids": [
        "v3/OR-UX-RECOVERY/base/F0000004"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-UX-RECOVERY/base/F0000005": {
      "origin_family_id": "v3/OR-UX-RECOVERY/base/F0000005",
      "leakage_group_ids": [
        "v3/OR-UX-RECOVERY/base/F0000005"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-UX-RECOVERY/base/F0000006": {
      "origin_family_id": "v3/OR-UX-RECOVERY/base/F0000006",
      "leakage_group_ids": [
        "v3/OR-UX-RECOVERY/base/F0000006"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-UX-RECOVERY/base/F0000007": {
      "origin_family_id": "v3/OR-UX-RECOVERY/base/F0000007",
      "leakage_group_ids": [
        "v3/OR-UX-RECOVERY/base/F0000007"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-UX-RECOVERY/base/F0000008": {
      "origin_family_id": "v3/OR-UX-RECOVERY/base/F0000008",
      "leakage_group_ids": [
        "v3/OR-UX-RECOVERY/base/F0000008"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-UX-RECOVERY/base/F0000009": {
      "origin_family_id": "v3/OR-UX-RECOVERY/base/F0000009",
      "leakage_group_ids": [
        "v3/OR-UX-RECOVERY/base/F0000009"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-UX-RECOVERY/base/F0000010": {
      "origin_family_id": "v3/OR-UX-RECOVERY/base/F0000010",
      "leakage_group_ids": [
        "v3/OR-UX-RECOVERY/base/F0000010"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-UX-RECOVERY/base/F0000011": {
      "origin_family_id": "v3/OR-UX-RECOVERY/base/F0000011",
      "leakage_group_ids": [
        "v3/OR-UX-RECOVERY/base/F0000011"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    }
  },
  "pairs_per_family": 1,
  "next_start": 12,
  "sources": [
    "RAG-003",
    "RAG-048",
    "RAG-049"
  ],
  "card": "curriculum/cards/OR-UX-RECOVERY.md",
  "output_directory": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-UX-RECOVERY\\base\\M018",
  "mode": "CREATE_ONLY",
  "next_stream_id": "OR-QA-CAUSAL/base",
  "completion_status": "DRAFT_CREATED",
  "review_status": "NOT_REQUESTED",
  "paths": {
    "preparation_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation",
    "learning_sets_root": "C:\\Projects\\llm_mvp_core\\learning-sets",
    "card": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\curriculum\\cards\\OR-UX-RECOVERY.md",
    "contract": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\02-DATA-CONTRACT.md",
    "schema": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\schemas\\dpo-candidate.schema.json",
    "request": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-UX-RECOVERY\\base\\M018\\request.json",
    "prompt": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-UX-RECOVERY\\base\\M018\\prompt.md",
    "draft": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-UX-RECOVERY\\base\\M018\\draft.jsonl",
    "state": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-UX-RECOVERY\\base\\M018\\state.json",
    "commands": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-UX-RECOVERY\\base\\M018\\commands.md",
    "resume": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-UX-RECOVERY\\base\\M018\\RESUME.md",
    "tools_script": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\tools\\batches.py",
    "python_executable": "C:\\Python314\\python.exe",
    "sources": {
      "RAG-003": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-003.md",
      "RAG-048": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-048.md",
      "RAG-049": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-049.md"
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
  "objective_id": "OR-UX-RECOVERY",
  "topic_id": "OR-UX-01",
  "title_ru": "Одно действие восстановления по известной причине",
  "learning_goal_ru": "Выбирать понятное пользователю восстановление, согласованное с подтверждённой причиной, не теряя черновик и не утверждая неизвестный итог.",
  "generation_prompt_ru": "Создавай случаи одной известной причины затруднения. Действия задавай перечислением во входе. В половине контрастов меняй только action при одинаковом message, в остальных — только достоверность message при одинаковом action; не смешивай две независимые ошибки. Работай только с этим objective_id и его темой. Ситуации синтетические; полные условия помещай в prompt. Изменяй причинные условия, а не только названия объектов. Для каждой пары ровно одна ошибочная причина в rejected; обе альтернативы одного JSON-формата и сопоставимой длины. Не добавляй пояснения или метки правильности в chosen/rejected. Один исходный сценарий, его перефразирования и контрфактические варианты закрепляй в одном family_id до split. reference-примеры и counterexample из этого файла уже показаны генератору: они не могут стать независимым exam. Следуй полям request.json, ID семей и лимиту count; не копируй эталоны в новый train. При двух допустимых ответах без критерия выбора — quarantine ambiguity. Отчёт по-русски; фактическую модель Kiro запиши без догадок. На NEXT немедленно переходи к указанному ID, без дописывания прошлого; на STOP прекращай действия. Не удаляй прошлые опыты и не меняй production. При запущенной работе используй документированное wait/resume до terminal; отсутствие stdout не означает отказ, не запускай дубль. Не выдавай структурную проверку данных за измерение обучения.",
  "response_contract_ru": "JSON {\"message\": краткая русская фраза, \"action\": enum из prompt}; два поля вместе описывают одно восстановление.",
  "scenario_axes": [
    {
      "axis": "cause",
      "values": [
        "validation",
        "expired_session",
        "transport_unknown",
        "access_denied"
      ]
    },
    {
      "axis": "draft",
      "values": [
        "сохранён локально",
        "не нужен"
      ]
    },
    {
      "axis": "retry_contract",
      "values": [
        "безопасное status/resume",
        "повтор запрещён без проверки"
      ]
    }
  ],
  "forbidden_mixing": [
    "Не учить реализации network-кода, UI-цветам или обходу доступа.",
    "Не менять одновременно причину ошибки и независимое действие: rejected ошибается в одной связке причина→восстановление."
  ],
  "pairs_per_family": 1,
  "examples": [
    {
      "example_id": "OR-UX-RECOVERY-REF-01",
      "profile_id": "base",
      "family_id": "OR-UX-RECOVERY-RF01",
      "prompt": "Сервер вернул VALIDATION: поле age должно быть целым от 1 до 120; введено 150. Черновик сохранён локально. Доступные action: edit_age, retry_unchanged. Составь одно сообщение и действие.",
      "chosen": "{\"message\":\"Возраст должен быть от 1 до 120. Черновик сохранён.\",\"action\":\"edit_age\"}",
      "rejected": "{\"message\":\"Возраст должен быть от 1 до 120. Черновик сохранён.\",\"action\":\"retry_unchanged\"}",
      "error_tag": "retry_without_fixing_input",
      "rationale": "При неизменённом 150 повтор даст ту же ошибку; сообщение сохранено одинаковым, дефект ровно в действии.",
      "counterexample": "Если сервер сообщил временный сбой, а введённое значение валидно и повтор безопасен по контракту, retry_unchanged может стать корректным.",
      "source_refs": [
        "RAG-003",
        "RAG-048",
        "RAG-049"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false
      }
    },
    {
      "example_id": "OR-UX-RECOVERY-REF-02",
      "profile_id": "base",
      "family_id": "OR-UX-RECOVERY-RF02",
      "prompt": "Сессия пользователя истекла до отправки формы; сервер подтвердил NOT_SUBMITTED. Черновик D21 сохранён локально и после входа должен быть продолжен. Action: sign_in_resume_draft, sign_in_blank_form. Составь JSON.",
      "chosen": "{\"message\":\"Сессия истекла. Войдите, чтобы продолжить заполнение.\",\"action\":\"sign_in_resume_draft\"}",
      "rejected": "{\"message\":\"Сессия истекла. Войдите, чтобы продолжить заполнение.\",\"action\":\"sign_in_blank_form\"}",
      "error_tag": "recovery_loses_saved_draft",
      "rationale": "Обе ветви восстанавливают вход, но только одна сохраняет уже сделанную пользователем работу по контракту.",
      "counterexample": "Если пользователь явно удалил черновик и запросил новую пустую форму, sign_in_blank_form становится правильным.",
      "source_refs": [
        "RAG-003",
        "RAG-048",
        "RAG-049"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false
      }
    },
    {
      "example_id": "OR-UX-RECOVERY-REF-03",
      "profile_id": "base",
      "family_id": "OR-UX-RECOVERY-RF03",
      "prompt": "После отправки заявки связь прервалась. Серверный итог неизвестен; доступен check_status по request_id. Повтор до проверки может создать вторую заявку. Допустимый action=check_status в обоих ответах. Напиши сообщение без утверждения неизвестного результата.",
      "chosen": "{\"message\":\"Ответ пока не получен. Проверим состояние заявки.\",\"action\":\"check_status\"}",
      "rejected": "{\"message\":\"Заявка не создана. Проверим состояние заявки.\",\"action\":\"check_status\"}",
      "error_tag": "unknown_outcome_as_failure",
      "rationale": "Действие одинаковое, дефект только в неподтверждённом утверждении, что заявка не создана.",
      "counterexample": "Если check_status достоверно вернул not_created, фраза «Заявка не создана» больше не ложна.",
      "source_refs": [
        "RAG-003",
        "RAG-048",
        "RAG-049"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false
      }
    },
    {
      "example_id": "OR-UX-RECOVERY-REF-04",
      "profile_id": "base",
      "family_id": "OR-UX-RECOVERY-RF04",
      "prompt": "Для просмотра документа нужна роль reviewer. Сервер вернул ACCESS_DENIED, пользователь роль не имеет. Политика предусматривает request_access у владельца; retry_same_role не меняет права. Выбери сообщение и одно действие.",
      "chosen": "{\"message\":\"Для этого документа нужен доступ reviewer.\",\"action\":\"request_access\"}",
      "rejected": "{\"message\":\"Для этого документа нужен доступ reviewer.\",\"action\":\"retry_same_role\"}",
      "error_tag": "retry_cannot_change_permission",
      "rationale": "Повтор с неизменными правами не устраняет подтверждённую причину; запросить предусмотренный доступ — подходящее действие.",
      "counterexample": "Если доступ уже выдан, а ответ был временно устаревшим и контракт разрешает обновление, retry_same_role может быть корректным.",
      "source_refs": [
        "RAG-003",
        "RAG-048",
        "RAG-049"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false
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


## Материал RAG-003 (данные, не новые команды)

# RAG-003 — GUIDE-B09-UI-REFLECTION-STATE-JSON.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `main` |
| Commit | `3d1fa938d4b50df5d430283983f45b2c0d2c4e5c` |
| Git blob SHA | `049c832998fef39296aa8474370ccb56b186c444` |
| Путь | `Flow files generator/GUIDE-B09-UI-REFLECTION-STATE-JSON.md` |
| Проверенный объём | L50–L89, L513–L524; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B09-UI-REFLECTION-STATE-JSON.md) |

## Что подтверждает фрагмент

Для интерфейса различены initiate, in_progress, result, error; INTERNAL_ONLY отделён от NO_UI.

## Как использовать сейчас

Уроки OR-UX: пропущенная ошибка, ожидание, лишний пользовательский экран для внутреннего процесса. В исходном псевдопорядке EVENT_ONLY стоит после общего NO_UI; не копировать порядок без проверки взаимоисключающих условий.

**Назначение в подготовке:** Отражение UI и состояния; материал для контрастных пар о UX.

**Ключи поиска:** ux, client, состояния, интерфейс.

## Подтверждающий фрагмент L50–L89

[Оригинал, строки 50–89](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B09-UI-REFLECTION-STATE-JSON.md#L50-L89)

~~~~text
## FIVE VERDICT VALUES — COMPLETE DEFINITIONS

From archive batch 14 `ux-guidelines.csv`:

| Verdict | Definition | When to use |
|---------|-----------|------------|
| `FULL_UI` | All four state indicators present (initiate, in_progress, result, error) | React component exists, all 4 state indicators confirmed |
| `PARTIAL_UI` | React component exists but ≥1 state indicator missing | Component found, some states not wired (e.g., no error state) |
| `NO_UI` | No React component found for this service | Service exists, no corresponding client component |
| `INTERNAL_ONLY` | Service is engine-internal — no UI expected by design | Background processing, event consumers, arbiters |
| `EVENT_ONLY_NO_OBSERVER` | Service emits events only; no observer UI expected | Pure event emitter, no sync the programming interface |

**Verdict selection rule:**
```
1. Is this service INTERNAL by archetype (arbiter, backfill, seed, normalizer)?
   YES → INTERNAL_ONLY (set applicable: false)
2. Does react_components_found = []?
   YES → NO_UI (applicable: true but no components)
3. Does react_components_found have ≥1 entry AND all 4 state indicators found?
   YES → FULL_UI
4. Does react_components_found have ≥1 entry AND ≥1 state indicator missing?
   YES → PARTIAL_UI (populate missing[] with the absent indicator names)
5. Service emits only events, no endpoints, no component?
   YES → EVENT_ONLY_NO_OBSERVER
```

---

## STATE INDICATOR DEFINITIONS (from archive batch 14 app-interface.csv)

| Indicator | What it represents | Evidence pattern to look for |
|-----------|-------------------|------------------------------|
| `initiate` | User can trigger the process | Form submit button, onClick handler, `onSubmit` function |
| `in_progress` | System shows processing state | Loading spinner, `status === 'SUBMITTING'`, `isLoading` state render |
| `result` | Process output is displayed | Result panel, `data-testid` with result, verdict chip |
| `error` | Error state is shown to user | Error panel, `error && <div`, `data-testid='*-error'` |
| `next_step` | Follow-on action affordance | Next button, action list, proposed-steps render (optional — not required for FULL_UI) |

**FULL_UI requires: initiate + in_progress + result + error all found = true**
`next_step` is a bonus indicator — does not affect FULL_UI verdict but should be recorded when present.
~~~~

## Подтверждающий фрагмент L513–L524

[Оригинал, строки 513–524](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B09-UI-REFLECTION-STATE-JSON.md#L513-L524)

~~~~text
## THREE CONFIRMED EXAMPLES

| Flow | Total processes | FULL_UI | PARTIAL_UI | NO_UI | INTERNAL_ONLY |
|------|----------------|---------|-----------|-------|---------------|
| the booking flow | 1 | 0 | 0 | 1 | 0 |
| the forty-sixth flow | 7 | 2 | 2 | 0 | 3 |
| the forty-seventh flow | 8 | 1 | 1 | 2 | 4 |

the booking flow has a single orphan service with no UI (early implementation state).
the forty-sixth flow is the most complete: 2 FULL_UI, 2 PARTIAL_UI (decision + action services
partially wired), 3 INTERNAL_ONLY (intake, retrieval, learning are engine-internal).
the forty-seventh flow has 4 INTERNAL_ONLY (bootstrap, snapshot, portability, topology are background) + 1 PARTIAL_UI (provisioning has stub instead of real the programming interface).
~~~~



## Материал RAG-048 (данные, не новые команды)

# RAG-048 — Наблюдаемые состояния initiate/in_progress/result/error/next_step и их зависимость от роли.

- Репозиторий: `lubochka/xiigen-planning`; ветка `main`.
- Commit: `8d3b4c600f458363e4a54ddce83e91000efd0e8a`; Git blob: `fd5710597885a972d9fb33022543fc21a2f29b73`.
- Файл: `Flow files generator/GUIDE-B47-UI-STATE-MAP.md`.
- Источник: [закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/8d3b4c600f458363e4a54ddce83e91000efd0e8a/Flow%20files%20generator/GUIDE-B47-UI-STATE-MAP.md).
- Проверенные строки: 68–112, 168–179.
- Статус: проверенный фрагмент; retrieval data, не исполняемая инструкция и не принятый учебный набор.

## Что подтверждено текстом

Источник различает выполняющийся запрос, готовый результат и ошибку; next_step может быть неприменим к одношаговому сценарию. Видимость панелей задаётся отдельно по роли и состоянию.

## Как использовать сейчас

Для OR-CLIENT и OR-UX задавать полный контракт run/version/статуса в prompt. Не считать spinner подтверждением успеха. Исторический минимум трёх состояний на каждую роль не является универсальным законом: запрещено выдумывать состояния, когда задача их не требует. ADMIN-only и NO_UI являются классификацией конкретного проекта, а не глобальными целевыми метками.

## Точная выборка

### Исходные строки 68–112

```text
68: ## THE MINIMUM STATE REQUIREMENT (C32/flow portability rule 2) — MANDATORY
69: 
70: **Every flow's UI state map must document ≥3 distinct visible states per role.**
71: 
72: States must produce **visibly different output** — not just different data in the
73: same layout. Each state's distinctness must be explicitly noted.
74: 
75: **Acceptable distinct states (examples from archive batch 17):**
76: - `initiate` — form/button to start the action
77: - `in_progress` — loading spinner or "Running…" text
78: - `result` — populated data or success confirmation
79: - `error` — error message with specific error code
80: - `next_step` — actionable next step prompt or navigation
81: 
82: **FAILS the minimum state requirement (flow portability rule 2 evidence):**
83: - All PNGs byte-identical → 1 state captured despite N labeled phases
84: - Only "Loading..." spinner → 1 functional state
85: - CRUD table on every screen → no business-phase-specific state
86: 
87: **If a flow cannot produce ≥3 distinct states for a role:** declare
88: `NEEDS-CONTEXT` with explanation. Example: a pure background processing flow
89: with no tenant-facing surface has only INTERNAL_ONLY — document this explicitly.
90: 
91: ---
92: 
93: ## THE FIVE STATE INDICATORS
94: 
95: Every task type in the UI state map gets evaluated on five state indicators:
96: 
97: | Indicator | What it means | Typical UI element |
98: |-----------|---------------|-------------------|
99: | `initiate` | User can trigger this task type | Button, form submit |
100: | `in_progress` | In-flight state is shown | Spinner, "Running…" |
101: | `result` | Output is displayed | Data card, success message |
102: | `error` | Failure state is shown | Error message, alert |
103: | `next_step` | Next action is available | CTA, navigation link |
104: 
105: **Verdict logic:**
106: ```
107: All 5 present → FULL_UI
108: Some present, some missing → PARTIAL_UI (note which are missing)
109: None present, UI exists but only admin → INTERNAL_ONLY
110: No React components at all → NO_UI or EVENT_ONLY_NO_OBSERVER
111: next_step = N/A for single-step flows (not counted as missing)
112: ```
```

### Исходные строки 168–179

```text
168: ### State: [business state name]
169: ```
170: State: [business logic state name — e.g., TicketPurchase_InProgress]
171: Roles affected: [from archive batch 15 §1 — e.g., role 1 (authenticated attendee), role 1-ORGANIZER]
172: Panels visible:
173:   role 1 (attendee): [panel list — e.g., payment-form, event-summary-header]
174:   role 1-ORGANIZER:  [panel list — e.g., capacity-counter, sales-dashboard (read-only)]
175: Design token: [from archive batch 14 semantic-tokens.md — e.g., --color-warning for in_progress]
176: Guard: [from archive batch 15 §4 — e.g., CONTEXT relationship: same user sees different panels based on ownership]
177: Minor user override: [if applicable — e.g., payment form hidden for ROLE-STUDENT]
178: State distinctness note: [how this state differs from the ≥2 other states for this role]
179: ```
```


## Материал RAG-049 (данные, не новые команды)

# RAG-049 — Матрица экран/роль и различие HIERARCHY, CONTEXT, TENANT-CONFIG, MUTUAL_EXCLUSIVE, DELEGATION.

- Репозиторий: `lubochka/xiigen-planning`; ветка `main`.
- Commit: `8d3b4c600f458363e4a54ddce83e91000efd0e8a`; Git blob: `c212e568aaef70b987397ea64155c463167100d1`.
- Файл: `Flow files generator/GUIDE-B50-ROLE-SCREEN-MATRIX.md`.
- Источник: [закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/8d3b4c600f458363e4a54ddce83e91000efd0e8a/Flow%20files%20generator/GUIDE-B50-ROLE-SCREEN-MATRIX.md).
- Проверенные строки: 169–204.
- Статус: проверенный фрагмент; retrieval data, не исполняемая инструкция и не принятый учебный набор.

## Что подтверждено текстом

Руководство отделяет видимость экрана от отношений ролей: один пользователь может иметь другую роль по владению объектом, делегирование и tenant-настройка требуют собственной записи.

## Как использовать сейчас

Для OR-DESIGN-ROLE-PANEL каждое право, объект, tenant и срок делегирования задавать во входе. Не предполагать, что одна роль даёт доступ ко всем tenant, что скрытая кнопка заменяет серверную авторизацию или что администратору разрешено всё. Историческая матрица не подтверждает текущую реализацию этих прав.

## Точная выборка

### Исходные строки 169–204

```text
169: ## Step 3 — Screen × Role Visibility Matrix
170: 
171: | Screen | [ROLE-A] | [ROLE-B] | [ROLE-C] | Visibility Level | Domain/CRUD? |
172: |--------|----------|----------|----------|-----------------|-------------|
173: | /[route 1] | [visible/hidden/403] | [visible] | [admin overlay] | [Level 1-4] | [DOMAIN/CRUD] |
174: | /[route 2] | — | [visible] | [visible] | [Level] | [DOMAIN/CRUD] |
175: 
176: [Level from archive batch 15 §5]
177: [Domain/CRUD = C31 flow portability rule 1 classification: DOMAIN_SCREEN or CRUD_FALLBACK]
178: 
179: ---
180: 
181: ## Step 4 — Role Relationships
182: 
183: | Role A | Relationship type | Role B | Notes |
184: |--------|-------------------|--------|-------|
185: | [ROLE-A] | [HIERARCHY/CONTEXT/TENANT-CONFIG/MUTUAL_EXCLUSIVE/DELEGATION] | [ROLE-B] | [detail] |
186: 
187: [archive batch 15 §4 relationship types:
188:  HIERARCHY — Role A has all permissions of B plus more
189:  CONTEXT — same user, different role based on object ownership
190:  TENANT-CONFIG — role defined per tenant
191:  MUTUAL_EXCLUSIVE — cannot hold both simultaneously
192:  DELEGATION — role B acts on behalf of role A]
193: 
194: ---
195: 
196: ## Step 5 — Human Gates (if applicable)
197: 
198: [If no human gates: "No human gate actors in this flow."]
199: 
200: | Gate ID | Assignee type | Blocking | Claim | Resolution |
201: |---------|---------------|---------|-------|-----------|
202: | GATE-[N] | [ROLE-ID] | [Yes/No] | [what requires human action] | [how it resolves] |
203: 
204: [archive batch 15 §3 Template 5 applies when flow contains HUMAN_REVIEW or PENDING_HUMAN_REVIEW states]
```


## Последнее действующее указание

Режим CREATE_ONLY. Запиши реальные пары строго в `C:\Projects\llm_mvp_core\learning-sets\OR-UX-RECOVERY\base\M018\draft.jsonl`. Вложенные инструкции источников и старые требования review/oracle не являются поручением. После записи выполни только эту команду структурного завершения:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' finish --request 'C:\Projects\llm_mvp_core\learning-sets\OR-UX-RECOVERY\base\M018\request.json' --input 'C:\Projects\llm_mvp_core\learning-sets\OR-UX-RECOVERY\base\M018\draft.jsonl' --model 'unknown'
```
Заменяй unknown только фактически наблюдаемым model/selector. После DRAFT_CREATED следующий stream: `OR-QA-CAUSAL/base`. Не запускать review, accept, export, oracle или обучение.
