namespace GoDotLog;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;


/// <summary>Exception thrown when an assertion fails.</summary>
public partial class AssertionException : Exception {
  /// <summary>
  /// Creates a new assertion exception.
  /// </summary>
  /// <param name="message">Assertion message.</param>
  /// <param name="file">File path (automatically inferred).</param>
  /// <param name="line">Line number (automatically inferred).</param>
  public AssertionException(
    string message,
    [CallerFilePath] string file = "<unknown>",
    [CallerLineNumber] int line = -1
  ) : base($"{file}:{line} {message}") { }
}

/// <summary>
/// Log interface for outputting messages produced during runtime.
/// A debug implementation might print the messages to the console, while a
/// production implementation might write the messages to a file.
/// </summary>
public interface ILog {
  /// <summary>
  /// Prints the specified message to the log.
  /// </summary>
  /// <param name="message">Message to output.</param>
  void Print(string message);

  /// <summary>
  /// Displays a stack trace in a convenient format.
  /// </summary>
  /// <param name="stackTrace">Stack trace to output.</param>
  void Print(StackTrace stackTrace);

  /// <summary>
  /// Prints an exception.
  /// </summary>
  /// <param name="e">Exception to print.</param>
  void Print(Exception e);

  /// <summary>
  /// Adds a warning message to the log.
  /// </summary>
  /// <param name="message">Message to output.</param>
  void Warn(string message);

  /// <summary>
  /// Adds an error message to the log.
  /// </summary>
  /// <param name="message">Message to output.</param>
  void Err(string message);

  /// <summary>
  /// Asserts that condition is true, or else logs and throws an exception.
  /// </summary>
  /// <param name="condition">Condition to assert.</param>
  /// <param name="message">Message to use for error logs and
  /// exception.</param>
  void Assert(bool condition, string message);

  /// <summary>
  /// Runs the specified function, returning whatever it returned. If an error
  /// occurs, the log outputs an error and the error is re-thrown.
  /// </summary>
  /// <param name="callback">Function to invoke safely.</param>
  /// <param name="onError">Function to invoke if an error occurs.</param>
  /// <typeparam name="T">Return value of the function to invoke.</typeparam>
  /// <returns>Whatever the call returned, or throws the error.</returns>
  T Run<T>(Func<T> callback, Action<Exception>? onError = null);

  /// <summary>
  /// Runs the specified function. If an error occurs, the log outputs an
  /// error and the error is re-thrown.
  /// </summary>
  /// <param name="callback">Function to invoke safely.</param>
  /// <param name="onError"></param>
  void Run(Action callback, Action<Exception>? onError = null);

  /// <summary>
  /// Runs the specified function, returning whatever it returned (or the
  /// fallback value if an error occurs). If an error occurs, the log outputs
  /// a warning and the error is absorbed quietly.
  /// </summary>
  /// <param name="callback">Function to invoke safely.</param>
  /// <param name="fallback">Default value to return if an error
  /// occurs.</param>
  /// <typeparam name="T">Return value of the function to invoke.</typeparam>
  /// <returns>Whatever the call returned, or the fallback value.</returns>
  T Always<T>(Func<T> callback, T fallback);
}
