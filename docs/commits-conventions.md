# Commits conventions

## Reasons for these conventions:

* Easier generation of the changelog.
* Easier determination of the semantic version bump (based on the types of commits landed).
* Simple navigation through git history (e.g. ignoring style changes).

## Format of the commit message:

```
<type>: <subject>

<body>

<footer>
```

## Type and subject (first line)

* Keep the first line shorter than 50 characters as much as possible.
* Use the imperative, present tense: "change", not "changed" nor "changes".
* The `<type>` should always be lowercase as shown below, as well as the first word of the `<subject>`.
* Don't end the `<subject>` line with a period.

### Allowed `<type>` values:

Type     | Release | Description | Changelog
---------|---------|-------------|----------
feat     | `minor` | New feature or enhancement (for the user - not a new feature for a build script). | Visible
fix      | `patch` | Bug fix (for the user - not a fix to a build script). | Visible
docs     | `patch` | A change to the documentation. | Hidden
refactor | `patch` | Refactoring production code, eg. renaming a variable, formatting, etc | Hidden
test     | `patch` | Adding tests, refactoring tests; no production code change | Hidden
chore    | `patch` | Updating grunt tasks, updating a library, etc | Conditional

## Body

* Keep it wrapped at 72 characters.
* Use the imperative, present tense: "change", not "changed" nor "changes".
* Include motivation for the change and contrasts with previous behavior.
* Start the body with a blank line.

## Footer

Closed issues should be listed on a separate line in the footer prefixed with "Closes" or "Fixes" keyword like this:

```
Closes #234
```

Or in the case of multiple issues, the keyword must be repeated in front of each issue (see [GitHub documentation](https://help.github.com/articles/closing-issues-using-keywords/)):

```
Fixes #123, Fixes #245, Fixes #992
```

## Commit message example:

```
fix: ensure Range headers adhere to RFC 2616

Add one new dependency, use `range-parser` (Express dependency) to 
compute range. It is more well-tested in the wild.

Fixes #2310
```

## Tips

If it seems difficult to summarize what your commit does, it may be because it includes several logical changes or bug fixes. Try to split up your commit into several commits.

## Credits

Inspired from:
* [Conventional commits](https://www.conventionalcommits.org/en/v1.0.0-beta.2/)
* [Karma commit conventions](https://github.com/karma-runner/karma/blob/master/docs/dev/06-git-commit-msg.md)
* [CKEditor commit conventions](https://ckeditor.com/docs/ckeditor5/latest/framework/guides/contributing/git-commit-message-convention.html)
* [Erlang OTP commit conventions](https://github.com/erlang/otp/wiki/writing-good-commit-messages)
