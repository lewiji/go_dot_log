# GoDotLog

[![Chickensoft Badge][chickensoft-badge]][chickensoft-website] [![Discord](https://img.shields.io/badge/Chickensoft%20Discord-%237289DA.svg?style=flat&logo=discord&logoColor=white)][discord] ![line coverage][line-coverage] ![branch coverage][branch-coverage]

Opinionated Godot logging interface and console implementation.

```csharp
public class MyEntity {
  // Create a log which outputs messages prefixed with the name of the class.
  private ILog _log = new GDLog(nameof(MyClass));
}
```

Logs can perform a variety of common actions (and output messages if they fail automatically):

```csharp
_log.Print("My message");

_log.Warn("My message");

_log.Err("My message");

// Run a potentially unsafe action. Any errors thrown from the action will
// be output by the log. An optional error handler callback can be provided
// which will be invoked before the exception is rethrown.
_log.Run(
  () => { _log.Print("Potentially unsafe action"); },
  (e) => {
    _log.Err("Better clean up after myself...whatever I did failed.");
  }
);

// Throw an assertion exception with the given message if the assertion fails.
_log.Assert(node.Name == "MyNode", "Must be valid node name.");

// Return the value of a function or a fallback value.
var result = _log.Always<T>(
  () => new Random().Next(0, 10) > 5 ? "valid value" : null,
  "fallback value"
);

// Print a decently formatted stack trace.
_log.Print(new StackTrace());
```

## Contributing

For information on contributing, see [CONTRIBUTING.md](CONTRIBUTING.md).

<!-- Links -->

[chickensoft-badge]: https://chickensoft.games/images/chickensoft/chickensoft_badge.svg
[chickensoft-website]: https://chickensoft.games
[discord]: https://discord.gg/gSjaPgMmYW
[line-coverage]: https://raw.githubusercontent.com/chickensoft-games/go_dot_log/main/test/reports/line_coverage.svg
[branch-coverage]: https://raw.githubusercontent.com/chickensoft-games/go_dot_log/main/test/reports/branch_coverage.svg
