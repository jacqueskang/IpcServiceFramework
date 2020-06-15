#Contributing

## Git workflow

Follow [Gitflow](https://datasift.github.io/gitflow/IntroducingGitFlow.html).

*Notes:*
 - Use **Rebase and merge** to complete PR merging to develop branch too have a clean and linear history.
 - Use **Create a merge commit** to complete PR merging to master branch so that "first-parent" commits matches the versioning history.

## Commit syntax

Follow [Conventional Commits 1.0](https://www.conventionalcommits.org/en/v1.0.0/)

## Versioning

Follow [Semantic Versioning 2.0](https://semver.org/).

Currently all JKang.IpcServiceFramework.* packages share a same version fixed in [version.yml](/build/version.yml). You should thus update this file when starting working on a new milestone.

## CI/CD

- A PR build is triggered when any PR is created, which checks the changes included by executing all tests.
- A CI build is triggered when any change is commited in `develop` branch, which generates CI packages (e.g., *.3.0.0-ci-20200612.1.nupkg)
- A preview release build is triggered when any change is commited in `master` branch, which generates and publishes preview packages (e.g., *.3.0.0-preview-20200612.1.nupkg) to nuget.org
- To publish a stable release repository owner manually trigger a stable release build in Azure DevOps which generates stable packages and publishes to nuget.org (e.g., *.3.0.0.nupkg)
