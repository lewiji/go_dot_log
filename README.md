# GoDotLog

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

_log.Error("My message");

// Run a potentially unsafe action. Any errors thrown from the action will
// be output by the log. An optional error handler callback can be provided
// which will be invoked before the exception is rethrown.
_log.Run(
  () => { _log.Print("Potentially unsafe action"); },
  (e) => {
    _log.Error("Better clean up after myself...whatever I did failed.");
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
