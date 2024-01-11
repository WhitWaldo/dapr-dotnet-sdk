## Release 1.13

### New Rules

| Rule ID | Category            | Severity | Notes                                                                                             |
|---------|---------------------|----------|---------------------------------------------------------------------------------------------------|
| DAPR1001  | DependencyInjection | Warning  | Every type that that implements IWorkflow should be registered with the service provider.         |
| DAPR1002  | DependencyInjection | Warning  | Every type that that implements IWorkflowActivity should be registered with the service provider. |