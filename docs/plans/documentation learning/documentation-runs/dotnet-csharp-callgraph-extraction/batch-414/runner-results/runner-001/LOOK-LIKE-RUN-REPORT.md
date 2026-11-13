# Code Documentation Batch Run

Status: PASS
Metric mode: runner_verified

| Metric | Value |
|---|---:|
| train rows | 50 |
| held-out rows | 50 |
| routing accuracy | 1.000 |
| source gate accuracy | 1.000 |
| version gate accuracy | 1.000 |
| transit gate accuracy | 1.000 |
| bottom-up order accuracy | 1.000 |
| callee-summary transit accuracy | 1.000 |
| local-vs-delegated accuracy | 1.000 |
| CPU ms | 359.375 |
| peak RAM MB | 36.25 |
| elapsed ms | 358.808 |

This run verifies the trainable documentation router path. It does not claim a full LoRA checkpoint unless a separate DPO model runner is attached.