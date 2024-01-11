## Release 1.13

### Tentative Rules

|  Rule ID  | Category | Severity | Notes                                                                                                                     |
|-----------|----------|----------|---------------------------------------------------------------------------------------------------------------------------|
| DAPR0500  | Usage    | Warning  | Workflow functions must call deterministic APIs.                                                                          |
| DAPR0501  | Usage    | Warning  | Workflow functions must only interact indirectly with external state.                                                     |
| DAPR0502  | Usage    | Warning  | Workflow functions must execute only on the workflow dispatch thread.                                                     |