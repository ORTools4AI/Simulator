# Engine provenance for licensing review

ORTools4AI.Simulator is based on a C# port of DESMO-J. The current engine combines inherited code with later changes. The entries below identify places to start an ownership and licensing review within the existing files.

| Original revision | Evidence to review |
| --- | --- |
| `42748f6` | The commit labeled “Porting code” provides a comparison baseline. Review the baseline itself for any material that predates the port. |
| `edf179c` | The event-list performance commit changes the event tree and sorted event list, adds event-list logging and reading, and changes scheduler integration. |
| `4cb4cae` | The usability refactor changes process and scheduler code and adds resource lease support. |

The following comparison uses `42748f6` as the baseline and `4723b94` as the source snapshot imported into this solution. The counts include implementation changes, comments, and formatting.

| Current file | Lines added | Lines removed |
| --- | ---: | ---: |
| [EventTreeList.cs](../src/ORTools4AI.Simulator/core/engine/EventTreeList.cs) | 355 | 162 |
| [Scheduler.cs](../src/ORTools4AI.Simulator/core/engine/Scheduler.cs) | 48 | 25 |
| [SimProcess.cs](../src/ORTools4AI.Simulator/core/engine/SimProcess.cs) | 68 | 14 |

## Release review

For each engine change proposed for commercial licensing, record its file and method, the revision or diff, any code adapted from upstream or third parties, and the basis for the licensor's rights. Review changes after the imported snapshot against the specific release named in the [commercial order form](COMMERCIAL-LICENSE.md#commercial-order-form). Commit history documents when changes appeared. Ownership and applicable terms require separate verification.

Before adopting commercial terms, reconcile the covered release's file notices and package metadata with the documented rights. Include the applicable upstream [license](../LICENSE) and [notices](../NOTICE) with the release.
