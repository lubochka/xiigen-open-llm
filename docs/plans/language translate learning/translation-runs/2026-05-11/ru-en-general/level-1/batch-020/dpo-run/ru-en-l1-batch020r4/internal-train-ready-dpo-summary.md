# Internal Train Ready DPO Run — BPE tokenizer

- Run id:        `ru-en-l1-batch020r4`
- Source:        `training-pairs-train-ready.jsonl`
- Source SHA256: `FBEB61838DC99DEF47D3AFB9EBF936E328C13E543A20ED7D980B69D5158C4D98`
- Pairs:         50
- Batches:       5
- Batch size:    10
- Context len:   128
- BPE vocab:     2048
- Result root:   `dpo-run\ru-en-l1-batch020r4`
- Total elapsed: 19427.0 ms
- Avg loss:      0.706942
- Final loss:    0.711446
- Avg accuracy:  0.120
- Final accuracy:0.000
- Avg margin:    -0.027253
- Final margin:  -0.036166

## Per-batch metrics
| Batch | Pairs | Loss | Accuracy | Margin | ms | First | Last |
|---:|---:|---:|---:|---:|---:|---|---|
| 1 | 10 | 0.704854 | 0.100 | -0.023213 | 2762.9 | ru-en-l1-020-001 | ru-en-l1-020-010 |
| 2 | 10 | 0.706468 | 0.100 | -0.026323 | 3704.2 | ru-en-l1-020-011 | ru-en-l1-020-020 |
| 3 | 10 | 0.709171 | 0.300 | -0.031464 | 6868.8 | ru-en-l1-020-021 | ru-en-l1-020-030 |
| 4 | 10 | 0.702771 | 0.100 | -0.019100 | 3760.1 | ru-en-l1-020-031 | ru-en-l1-020-040 |
| 5 | 10 | 0.711446 | 0.000 | -0.036166 | 2035.9 | ru-en-l1-020-041 | ru-en-l1-020-050 |
