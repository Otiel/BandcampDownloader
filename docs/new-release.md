# How to create a new release

## Workflow

This repository follows [git flow](https://nvie.com/posts/a-successful-git-branching-model) as a git branching and release management workflow. The following content describes how to create a new release by following this workflow and using GitHub resources.

## Prepare the release

1. Merge all `fix` and `feature` branches that are to be incorporated in the new release on the `develop` branch.
2. Update `CHANGELOG.md` based on the git history. Determine the new version number from the changes. Do not commit yet.

## Create a new release branch

1. Create a new branch named `release-X.Y.Z` from `develop` and switch to this new branch.
2. Bump the version number:
    * Commit the changes on `CHANGELOG.md`.
    * Update the assembly number on `AssemblyInfo.cs` and commit.

## Finish the release branch

1. Merge `release-X.Y.Z` into `master`.
2. Create a new tag called `vX.Y.Z`.
3. Merge `release-X.Y.Z` into `develop`.
4. Delete the `release-X.Y.Z` branch.
5. Push `master`, `develop` and the new tag.

## Create the GitHub release

1. On Visual Studio, set the Solution Configuration to "Release".
2. Build the solution.
3. Create a new _zip_ archive containing the necessary files created under `src\BandcampDownloader\bin\Release`.
4. Compute checksums (MD5, SHA-1...) for the files.
5. Draft a new [release](https://github.com/Otiel/BandcampDownloader/releases) on GitHub:
    * Choose the newly created tag (if you forgot to push it, now's the time to do it).
    * Set the title equal to `X.Y.Z`.
    * Copy-paste the changes from `CHANGELOG.md`.
    * Add the checksums to the description of the release.
    * Attach the _zip_ file.
6. Publish the release!
