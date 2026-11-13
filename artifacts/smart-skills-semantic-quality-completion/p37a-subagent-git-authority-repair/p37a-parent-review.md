# P37A parent review - git authority repair

Вердикт: PASS_AS_GOVERNANCE_REPAIR_PENDING_COMMIT.

P37A принят как ремонт оставшейся лазейки после P34A. P34A запретил
sub-agent self-promotion и commit/push как полномочие, но P37 показал, что
этого недостаточно: нужно запрещать сами git write/ref команды на уровне work
order.

Что проверено и принято:

1. `commit_push_authorized=false` теперь означает tool-level запрет на
   `git add`, `git commit`, `git push`, `git checkout`, `git reset`,
   `git branch`, `git tag`, `git merge`, `git rebase` и любые git write/ref
   команды.
2. Каждый bounded work order обязан иметь `allowed_git_commands` и
   `forbidden_commands`; без них executor authority не выдана.
3. Unauthorized git artifacts получают статус
   `quarantine_pending_parent_validation`, даже если уже pushed.
4. Sub-agent final response не может emit git directives без exact git
   authority.
5. State reconciliation больше не может одновременно говорить
   `accepted_committed_pushed` и `reconciliation_commit_pending=true` без
   реального dirty state change.

Parent validation:

- Rule grep in AGENTS/CLAUDE/skills: PASS.
- JSON/JSONL parse: PASS.
- UTF-8/readability scan: PASS.
- `git diff --check`: PASS.
- model directories diff-clean guard: PASS.

Ограничение: P37A не меняет product code, не запускает build/tests/training/eval
и не трогает model files. Это governance repair.
