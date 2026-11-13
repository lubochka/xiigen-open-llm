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
| bottom-up order accuracy | 0.480 |
| callee-summary transit accuracy | 0.480 |
| local-vs-delegated accuracy | 0.480 |
| CPU ms | 359.375 |
| peak RAM MB | 37.906 |
| elapsed ms | 347.432 |

This run verifies the trainable documentation router path. It does not claim a full LoRA checkpoint unless a separate DPO model runner is attached.