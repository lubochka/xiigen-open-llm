Before you spend time on a pull request, please read `CONTRIBUTING.md`. The
current policy is issues first: open an issue to report a bug or propose a
change. External pull requests are not being merged yet, and the maintainer may
close a pull request in favour of the issue discussion. This is a policy about
review capacity, not about the quality of your work.

## What this changes

A short description of the change and the issue it belongs to.

## Checklist

- [ ] `dotnet build` succeeds for the whole solution.
- [ ] `dotnet test` is green.
- [ ] No PyTorch, TensorFlow, ML.NET training, or any equivalent external ML
      framework is added (project Rule 10).
- [ ] No secrets, credentials, or personal data are included in the diff.
- [ ] The contribution is offered under the same license as this project,
      `AGPL-3.0-only` (inbound = outbound).

Security vulnerabilities are not reported through pull requests or public
issues: see `.github/SECURITY.md`.
