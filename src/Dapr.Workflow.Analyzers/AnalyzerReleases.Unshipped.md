## Release 1.13

### New Rules

| Rule ID | Category | Severity | Notes                                                                                                                     |
|---------|----------|----------|---------------------------------------------------------------------------------------------------------------------------|
| WF0010  | Usage    | Warning  | Workflow functions must call deterministic APIs.                                                                          |
| WF0011  | Usage    | Warning  | Workflow functions must only interact indirectly with external state.                                                     |
| WF0012  | Usage    | Warning  | Workflow functions must execute only on the workflow dispatch thread.                                                     |
| WF0013  | Usage    | Warning  | Care should be taken when retry policies and resiliency policies are both used as they can result in unexpected behavior. |