namespace GoDotLog {
  using System;
  using System.Diagnostics;
  using System.Runtime.CompilerServices;
  using Godot;

  /// <summary>
  /// Default log which outputs to Godot using GD.Print, GD.PushWarning,
  /// and GD.PushError. Warnings and errors also print the message since
  /// `GD.PushWarning` and `GD.PushError` don't always show up in the output
  /// when debugging.
  /// </summary>
  public partial class GDLog : ILog {
    /// <summary>Default print action (GD.Print).</summary>
    public static readonly Action<string> DefaultPrint
      = (message) => GD.Print(message);

    /// <summary>Print action (defaults to GD.Print).</summary>
    public static Action<string> PrintAction = DefaultPrint;

    /// <summary>Default push warning action (GD.PushWarning).</summary>
    public static readonly Action<string> DefaultPushWarning
      = (message) => GD.PushWarning(message);

    /// <summary>Push warning action (defaults to GD.PushWarning).</summary>
    public static Action<string> PushWarningAction = DefaultPushWarning;

    /// <summary>Default push error action (GD.PushError).</summary>
    public static readonly Action<string> DefaultPushError
      = (message) => GD.PushError(message);
    /// <summary>Push error action (defaults to GD.PushError).</summary>
    public static Action<string> PushErrorAction = DefaultPushError;

    private string _prefix { get; set; }

    /// <summary>
    /// Creates a new GDLog with the given prefix.
    /// </summary>
    /// <param name="prefix">Log prefix, displayed at the start of each message.
    /// </param>
    public GDLog(string prefix) => _prefix = prefix;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Print(string message) => PrintAction(_prefix + ": " + message);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Print(StackTrace stackTrace) {
      foreach (var frame in stackTrace.GetFrames()) {
        var fileName = frame.GetFileName() ?? "**";
        var lineNumber = frame.GetFileLineNumber();
        var colNumber = frame.GetFileColumnNumber();
        var method = frame.GetMethod();
        var className = method.DeclaringType?.Name ?? "UnknownClass";
        var methodName = method.Name;
        Print(
          $"{className}.{methodName} in " +
          $"{fileName}({lineNumber},{colNumber})"
        );
      }
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Print(Exception e) {
      Error("An error ocurred.");
      Error(e.ToString());
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Warn(string message) {
      PrintAction(_prefix + ": " + message);
      PushWarningAction(_prefix + ": " + message);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Error(string message) {
      PrintAction(_prefix + ": " + message);
      PushErrorAction(_prefix + ": " + message);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Assert(bool condition, string message) {
      if (!condition) {
        Error(message);
        throw new AssertionException(message);
      }
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Run<T>(Func<T> call, Action<Exception>? onError = null) {
      try {
        return call();
      }
      catch (Exception e) {
        Print(e);
        onError?.Invoke(e);
        throw;
      }
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(Action call, Action<Exception>? onError = null) {
      try {
        call();
      }
      catch (Exception e) {
        Print(e);
        onError?.Invoke(e);
        throw;
      }
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Always<T>(Func<T> call, T fallback) {
      try {
        return call();
      }
      catch (Exception e) {
        Warn($"An error ocurred. Using fallback value `{fallback}`.");
        Warn(e.ToString());
        return fallback;
      }
    }
  }
}
