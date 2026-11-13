# Session Start Prompt - Round Artifact Zipping v1

Use this prompt at the beginning of future implementation or document-maintenance
sessions when the work must be packaged by round.

## Operating Rule

Track every file that is produced or changed during the session. At the end of
each logical work round, create a zip that contains:

- every source, document, test, evidence, or plan file produced or changed in
  that round
- a short round manifest naming what changed, why it changed, and how it was
  verified
- this session-start prompt, so the packaging rule travels with the archive

Do not include build outputs, `bin`, `obj`, temporary extraction folders, IDE
state, or unrelated dirty files.

## Start Of Session Checklist

1. Confirm the active workspace path.
2. Read the local `AGENTS.md` or project instructions.
3. Record the baseline state:
   - for a git repo: `git status --short`
   - for a document folder: file list with size and timestamp
4. State the intended round boundary before beginning work.
5. While working, keep a running list of files changed by this session only.

## End Of Round Checklist

1. Verify the work:
   - run focused tests for code changes
   - run the full suite when the codebase expects it
   - for document-only rounds, run text searches that prove the required notes
     or decisions are present
2. Create a manifest with:
   - round name
   - source request
   - files included
   - tests or verification performed
   - known remaining gaps
3. Create a zip named:
   - `round-<number>-<short-subject>-<yyyy-mm-dd>.zip`
4. Put the zip in the same planning/output folder unless the user requested a
   different location.
5. Report the exact zip path and the verification result.

## Correctness Rule

A round zip is a transportable evidence packet. A future session should be able
to open only the zip, read the manifest and this prompt, and understand what was
changed, why it was changed, and how to continue safely.
