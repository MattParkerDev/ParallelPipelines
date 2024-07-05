# ParallelPipelines

⭐ Write your CICD orchestration with C#!


| ParallelPipelines                                                                      | Github Actions                         |
|----------------------------------------------------------------------------------------|----------------------------------------|
| ✅ Parallel steps                                                                       | ❌ Sequential steps                     |
| ✅ Run locally to rapidly prototype and debug                                           | ❌ Difficult to run locally, accurately |
| ✅ Interactive progress running locally                                                 | ❌ -                                    |
| ✅ Outputs a summary table and gantt chart of deployment to Github Actions Step Summary | 🟨 Run summary                         |
| ❌ Cannot rerun failed jobs (restart in progress run)                                   | ✅ Rerun failed jobs                    |
| ✅ C# and strong types                                                                  | ❌ YAML                                 |

Despite the comparison, ParallelPipelines is built primarily to be used in Github Actions. Support for Bitbucket Pipelines/Azure Pipelines may be officially supported in the future if there is interest.

## Getting Started

- Copy and update as needed the [Github workflow examples](./samples/Github%20Workflows) folder to your repository
- Create a new C# Project for CICD, and install the [ParallelPipelines Nuget package](https://www.nuget.org/packages/ParallelPipelines/)
- Create some steps! See the [examplesrc/ParallelPipelines.Console/Modules](./examplesrc/ParallelPipelines.Console/Modules) folder for examples
- Test your CICD locally with `dotnet run`
- Ensure the path to your `Deploy` project is correct in the Github workflow
- Done!

### Notes Regarding Examples

- The examples are written with a C#/.NET/Azure ecosystem in mind, noting example steps to install dotnet wasm-tools, install the Azure Static Web Apps CLI to deploy a Blazor WebAssembly app, and provision an Azure resource group/deploy a Bicep template.
- These are just examples - you can write whatever steps you would like, as steps simply run shell commands.


## Example Summary Table and Gantt Chart:
### Run Summary
| Module | Status | Start | End | Duration |
| --- | --- | --- | --- | --- |
| InstallSwaCliModule | ${\textsf{\color{lightgreen}Success}}$ | 00s:005ms | 10s:118ms | 10s:113ms |
| CreateResourceGroupModule | ${\textsf{\color{lightgreen}Success}}$ | 10s:119ms | 15s:735ms | 05s:616ms |
| InstallDotnetWasmToolsModule | ${\textsf{\color{lightgreen}Success}}$ | 00s:005ms | 25s:225ms | 25s:220ms |
| DeployBicepModule | ${\textsf{\color{lightgreen}Success}}$ | 15s:737ms | 01m:08s | 52s:861ms |
| RestoreAndBuildModule | ${\textsf{\color{lightgreen}Success}}$ | 25s:228ms | 02m:25s | 02m:00s |
| PublishWebApiModule | ${\textsf{\color{lightgreen}Success}}$ | 02m:25s | 02m:31s | 05s:862ms |
| DeployWebApiModule | ${\textsf{\color{lightgreen}Success}}$ | 02m:31s | 03m:15s | 44s:776ms |
| PublishWebUiModule | ${\textsf{\color{lightgreen}Success}}$ | 02m:25s | 03m:32s | 01m:06s |
| DeployWebUiModule | ${\textsf{\color{lightgreen}Success}}$ | 03m:32s | 04m:11s | 39s:304ms |
| **Total** | **${\textsf{\color{lightgreen}Success}}$** | **00s:000ms** | **04m:11s** | **04m:11s** |

```mermaid
---
config:
  theme: base
  themeVariables:
    primaryColor: "#007d15"
    primaryTextColor: "#fff"
    primaryBorderColor: "#02ad1e"
    lineColor: "#F8B229"
    secondaryColor: "#006100"
    tertiaryColor: "#fff"
    darkmode: "true"
    titleColor: "#fff"
  gantt:
    leftPadding: 40
    rightPadding: 120
---

gantt
	dateFormat  mm:ss:SSS
	title       Run Summary
	axisFormat %M:%S

InstallSwaCliModule : 00:00:005, 00:10:118
CreateResourceGroupModule : 00:10:119, 00:15:735
InstallDotnetWasmToolsModule : 00:00:005, 00:25:225
DeployBicepModule : 00:15:737, 01:08:598
RestoreAndBuildModule : 00:25:228, 02:25:285
PublishWebApiModule : 02:25:285, 02:31:148
DeployWebApiModule : 02:31:148, 03:15:925
PublishWebUiModule : 02:25:287, 03:32:122
DeployWebUiModule : 03:32:122, 04:11:427
```
