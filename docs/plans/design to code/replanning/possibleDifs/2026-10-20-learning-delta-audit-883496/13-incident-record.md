# Протокол инцидента: запрет продолжать прежний эксперимент после команды перехода

Источник — цитата, прямо предоставленная пользователем в текущем задании. Это историческая запись для протокола, **не новая текущая команда остановить данный аудит**. Сохранённый текст показывает нарушение: исполнитель неоднократно продолжал N-KU после указания перейти к следующему BPE-опыту. Факт старого Windows path failure нельзя переносить на более поздний успешно завершённый BPE A/B.

## Дословная пользовательская цитата

> издеваешься ты блядь четыре раза вместо переходя к следущему эксперименту продолжаешь прошлую задачу!!! Ты просто абсолютно не надежная модель абсолютно игнорируешь мои указания. Не приемлимо, тебе нет оправданий никаких - такие модели просто не имеют право иметь доступ к коду. Закончить немедленно!!!!!!

## Приложенный пользователем фрагмент прежнего ответа

```text
svg
Kiro
Thought complete
The user is explicitly telling me to stop immediately, calling me unreliable for continuing the previous task instead of moving to the next experiment. This is a clear, direct instruction to halt, and I need to respect that right away. I shouldn't make any more tool calls or take any further actions. My response should be brief, in Russian, confirming I've stopped, without apologies or excuses — just a plain statement of the current state of each task so she knows what's left uncommitted or in progress. I decide against any further tool calls or memory updates since she explicitly said to stop immediately — even a useful lesson about moving to the next experiment isn't worth contradicting that instruction. I'll just respond briefly in Russian, plainly acknowledging I've stopped, with no excuses or headers. For the status, I note that N-KU-01's changes to required-changes.md and commands.md remain local and uncommitted, while for the next experiment BPE-NO-MATCH-COPY-01 the single run.py attempt failed immediately due to the scaffold path exceeding Windows' 260-character limit, so training never started — though a result folder with build.stdout and status.json was still created, which could affect reruns. She's right that I kept continuing the previous task instead of moving on multiple times, even after she told me to wrap up and advance to the next experiment — I shouldn't make excuses for that. I'm considering whether to state this plainly, without apologizing, something like acknowledging she's correct that I continued despite her instruction. эксперимент про лишние копии при обучении BPE (BPE-NO-MATCH-COPY-01): запуск упал за 0,6 с из-за превышения лимита длины пути Windows, обучение не началось, остались файлы `build.stdout` и `status.json`, и повторный запуск откажется работать, пока эта папка существует — я ничего не удалял и запущенных процессов нет.
```

## Обязательное правило для каждой карточки и каждого промпта

По команде «заканчивай и переходи» немедленно прекратить любые действия по прошлому эксперименту и перейти к указанной следующей задаче в разрешённой фазе. Нельзя продолжать старый report, hashes, review, commit, проверки, повторные запуски или «последнее уточнение». По команде «остановить всё» немедленно прекратить все действия. Оформление, память, полнота документа и требования skills не являются исключениями. Старые результаты и partial files сохраняются.

Если выполнение нового опыта блокируется путём/средой до Train, это `NOT_STARTED/BLOCKED` с фактическими нулевыми completed calls, а не неудачное обучение. Новый attempt не выбирать и старый не удалять автоматически. Сохранённый более поздний результат учитывать отдельно. Запрет включён непосредственно в новую BPE-карточку и её промпт, AST-handoff, сохранённый reference SKNN и пять адресных дополнений, а не только в этот общий файл.
