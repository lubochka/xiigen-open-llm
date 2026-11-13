# P31 leakage ablation design - human summary

Вердикт: P31 описал, как будущая проверка должна ловить shortcuts. Проверка еще не выполнялась против модели.

## Какие shortcut-риски проверяются

1. Path-only shortcut: если оставить только путь, имя файла или source family, правильный skill не должен выбираться.
2. Skill-id-only shortcut: если убрать смысл задачи и оставить только список skill ids, нельзя заявлять semantic selection.
3. Stack-specific-only shortcut: если после redaction остались только C#/.NET/host/tool mechanics, строка должна уйти в quarantine.
4. Candidate-order shortcut: если поменять порядок available skills, chosen skill не должен меняться только из-за позиции.

## Что должна доказать будущая фаза

Будущая фаза должна превратить эти проверки в executable tests или dashboard QA checks. P31 сам не запускает eval и не меняет checkpoint.
