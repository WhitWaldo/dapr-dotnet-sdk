## Release 1.13

### New Rules

| Rule ID | Category            | Severity | Notes                                                                                             |
|---------|---------------------|----------|---------------------------------------------------------------------------------------------------|
| WF0001  | DependencyInjection | Warning  | Every type that that implements IWorkflow should be registered with the service provider.         |
| WF0002  | DependencyInjection | Warning  | Every type that that implements IWorkflowActivity should be registered with the service provider. |