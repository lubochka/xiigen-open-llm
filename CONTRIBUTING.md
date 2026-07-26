# Contributing to LlmFromScratch

Thank you for your interest. This project is an LLM and a full machine-learning
stack built from scratch in pure C# (.NET 9): a BPE tokenizer, a transformer, a
training loop, LoRA, a from-scratch DPO trainer, and an orchestration/agent
layer. It is a study/reference stack, not a finished product.

## Ground rule: no external ML frameworks

Every algorithm is implemented from scratch. Do not add PyTorch, TensorFlow,
ML.NET training, or any equivalent external ML framework (project Rule 10).
Roslyn is allowed for code analysis, and non-ML utility packages are allowed.

## Building and testing

    dotnet build        # build the whole solution
    dotnet test         # build and run the test suite

Please make sure the solution builds and the test suite is green before you
propose a change.

## Code rules

- Every public method returns `OperationResult<T>`: do not throw for business
  logic, do not return null, and do not return a bare bool without context.
- Resolve external dependencies through interfaces, not concrete types.
- Every line of production code carries a plain-language comment explaining what
  that line does in context.
- No external ML frameworks (see the ground rule above).

## Contributions and pull requests

<!-- PR policy: variant A (issues only) is the published default. To accept
     external pull requests instead, the maintainer swaps in variant B, which is
     kept in the HTML comment below. -->

Please use issues. Open an issue to report a bug or propose a change; issues
labelled `good first issue` are a good place to start. External pull requests are
not being merged yet.

<!--
Variant B (if pull requests are accepted): pull requests are welcome. By
submitting one you agree that your contribution is licensed under the same
license as this project (`AGPL-3.0-only`); inbound = outbound.
-->

## License

This project is licensed under the GNU Affero General Public License v3.0
(`AGPL-3.0-only`). See the `LICENSE` file.
