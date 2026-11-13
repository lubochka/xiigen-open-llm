# P24 - redacted runtime holdout rows для SkillSelection

Вердикт: создан малый легитимный holdout ledger. Принятых строк: 4.

Что принято:
- строки взяты только из локальных skill/guide source families, которых нет в common checkpoint и candidate checkpoint;
- raw private text, exact prompt, path, filename, source id, pair id и candidate order не используются как authority;
- каждая строка содержит redacted summary, hashes, available skills, expected selected skills и явный not-used-for-training флаг;
- каждая строка помечена как runtimeInvocationReady, потому что в ней есть все поля для будущего SkillSelection runtime request.

Что отклонено:
- кандидатов с уже представленными skill families: 3;
- эти кандидаты записаны в gap ledger, а не превращены в accepted holdout rows.

Граница доказательства:
P24 не запускал side-by-side comparison, не обучал модель, не менял checkpoints и не выполнял promotion. Это только подготовка честного eval/holdout ledger.

Следующее действие по плану:
P25 может сравнивать common и candidate checkpoints только на accepted P24 rows. Даже после этого promotion останется запрещен, пока отдельный runtime gate не пройдет.
