namespace GoDotLogTest;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using Godot;
using GoDotLog;
using GoDotTest;
using LightMock.Generator;
using Shouldly;

// Since log is used by GoDotTest, it creates a circular dependency warning
// we can safely ignore.
#pragma warning disable CS0436
public class LogTest : TestClass {

  private StringBuilder _print = null!;
  private StringBuilder _warn = null!;
  private StringBuilder _error = null!;

  public LogTest(Node testScene) : base(testScene) { }

  public void Setup() {
    _print = new StringBuilder();
    _warn = new StringBuilder();
    _error = new StringBuilder();

    GDLog.PrintAction = (message) => _print.AppendLine(message);
    GDLog.PushWarningAction = (message) => _warn.AppendLine(message);
    GDLog.PushErrorAction = (message) => _error.AppendLine(message);
  }

  [Test]
  public void Initializes() {
    var log = new GDLog(nameof(LogTest));
    log.ShouldBeAssignableTo<ILog>();
  }

  // Note: Setup() and Cleanup() have to be called inside each test instead of
  // using [SetupAll] and [CleanupAll] from GoDotTest since we are overriding
  // GDLog's static methods and GoDotTest has a project reference to us.
  // These packages are very intertwined, making it difficult to test them.

  [Test]
  public void Prints() {
    Setup();
    var log = new GDLog("Prefix");
    log.Print("Hello, world!");
    _print.ToString().ShouldBe("Prefix: Hello, world!\n");
    Cleanup();
  }

  [Test]
  public void PrintsStackTrace() {
    Setup();
    var log = new GDLog("Prefix");
    var st = new Mock<StackTrace>();
    log.Print(new FakeStackTrace("File.cs", "ClassName", "MethodName"));
    _print.ToString().ShouldBe(
      "Prefix: ClassName.MethodName in File.cs(1,2)\n"
    );
    Cleanup();
  }

  [Test]
  public void PrintsStackTraceWithoutFile() {
    Setup();
    var log = new GDLog("Prefix");
    var st = new Mock<StackTrace>();
    log.Print(new FakeStackTrace(null, "ClassName", "MethodName"));
    _print.ToString().ShouldBe(
      "Prefix: ClassName.MethodName in **(1,2)\n"
    );
    Cleanup();
  }

  [Test]
  public void PrintsStackTraceWithoutClass() {
    Setup();
    var log = new GDLog("Prefix");
    var st = new Mock<StackTrace>();
    log.Print(new FakeStackTrace("File.cs", null, "MethodName"));
    _print.ToString().ShouldBe(
      "Prefix: UnknownClass.MethodName in File.cs(1,2)\n"
    );
    Cleanup();
  }

  [Test]
  public void PrintsStackTraceWithoutMethod() {
    Setup();
    var log = new GDLog("Prefix");
    var st = new Mock<StackTrace>();
    log.Print(new FakeStackTrace("File.cs", "ClassName", null));
    // No method also results in an unknown class.
    _print.ToString().ShouldBe(
      "Prefix: UnknownClass.UnknownMethod in File.cs(1,2)\n"
    );
    Cleanup();
  }

  [Test]
  public void PrintsException() {
    Setup();
    var e = new InvalidOperationException("message");
    var log = new GDLog("Prefix");
    log.Print(e);
    var output = string.Join("\n", new string[] {
      "Prefix: An error ocurred.",
      "Prefix: System.InvalidOperationException: message\n"
    });
    _print.ToString().ShouldBe(output);
    _error.ToString().ShouldBe(output);
    Cleanup();
  }

  [Test]
  public void Warns() {
    Setup();
    var log = new GDLog("Prefix");
    log.Warn("Hello, world!");
    _print.ToString().ShouldBe("Prefix: Hello, world!\n");
    _warn.ToString().ShouldBe("Prefix: Hello, world!\n");
    Cleanup();
  }

  [Test]
  public void Errors() {
    Setup();
    var log = new GDLog("Prefix");
    log.Err("Hello, world!");
    _print.ToString().ShouldBe("Prefix: Hello, world!\n");
    _error.ToString().ShouldBe("Prefix: Hello, world!\n");
    Cleanup();
  }

  [Test]
  public void AssertsSuccessfully() {
    Setup();
    var log = new GDLog("Prefix");
    log.Assert(true, "message");
    _print.ToString().ShouldBeEmpty();
    _warn.ToString().ShouldBeEmpty();
    _error.ToString().ShouldBeEmpty();
    Cleanup();
  }

  [Test]
  public void AssertsError() {
    Setup();
    var log = new GDLog("Prefix");
    var e = Should.Throw<AssertionException>(
      () => log.Assert(false, "error message")
    );
    e.Message.ShouldContain("error message");
    var output = "Prefix: error message\n";
    _print.ToString().ShouldBe(output);
    _error.ToString().ShouldBe(output);
    Cleanup();
  }

  [Test]
  public void RunsSuccessfully() {
    Setup();
    var log = new GDLog("Prefix");
    log.Run(() => { });
    _print.ToString().ShouldBeEmpty();
    _warn.ToString().ShouldBeEmpty();
    _error.ToString().ShouldBeEmpty();
    Cleanup();
  }

  [Test]
  public void RunsError() {
    Setup();
    var log = new GDLog("Prefix");
    var called = false;
    var exception = new InvalidOperationException("error message");
    var e = Should.Throw<Exception>(
      () => log.Run(
        () => throw exception,
        (e) => { e.ShouldBe(exception); called = true; }
      )
    );
    _print.ToString().ShouldNotBeEmpty();
    called.ShouldBeTrue();
    e.Message.ShouldContain("error message");
    Cleanup();
  }

  [Test]
  public void RunReturnsSuccessfully() {
    Setup();
    var log = new GDLog("Prefix");
    var result = log.Run(() => "value");
    result.ShouldBe("value");
    Cleanup();
  }

  [Test]
  public void RunThrowsOnReturn() {
    Setup();
    var log = new GDLog("Prefix");
    var called = false;
    var exception = new InvalidOperationException("error message");
    var e = Should.Throw<Exception>(
      () => log.Run<string>(
        () => throw exception,
        (e) => { e.ShouldBe(exception); called = true; }
      )
    );
    called.ShouldBe(true);
    _print.ToString().ShouldNotBeEmpty();
    e.Message.ShouldContain("error message");
    Cleanup();
  }

  [Test]
  public void AlwaysReturns() {
    Setup();
    var log = new GDLog("Prefix");
    var result = log.Always(() => "value", "fallback");
    result.ShouldBe("value");
    Cleanup();
  }

  [Test]
  public void AlwaysReturnsFallback() {
    Setup();
    var log = new GDLog("Prefix");
    var result = log.Always(
      () => throw new InvalidOperationException("error message"), "fallback"
    );
    _warn.ToString().ShouldNotBeEmpty();
    Cleanup();
  }

  public void Cleanup() {
    GDLog.PrintAction = GDLog.DefaultPrint;
    GDLog.PushWarningAction = GDLog.DefaultPushWarning;
    GDLog.PushErrorAction = GDLog.DefaultPushError;
  }
}
#pragma warning restore CS0436

internal class FakeMethodBase : MethodBase {
  private readonly string? _fileName;
  private readonly string? _methodName;
  private readonly string? _className;

  public FakeMethodBase(
    string? fileName, string? className, string? methodName
  ) {
    _fileName = fileName;
    _methodName = methodName;
    _className = className;
  }

  public override Type? DeclaringType
    => _className != null ? new FakeType(_className) : null;
  public override string Name => _methodName ?? null!;

  public override MethodAttributes Attributes
    => throw new NotImplementedException();
  public override RuntimeMethodHandle MethodHandle
    => throw new NotImplementedException();
  public override MemberTypes MemberType
    => throw new NotImplementedException();
  public override Type ReflectedType
    => throw new NotImplementedException();
  public override object[] GetCustomAttributes(bool inherit)
    => throw new NotImplementedException();
  public override object[] GetCustomAttributes(Type attributeType, bool inherit)
    => throw new NotImplementedException();
  public override MethodImplAttributes GetMethodImplementationFlags()
    => throw new NotImplementedException();
  public override ParameterInfo[] GetParameters()
    => throw new NotImplementedException();
  public override object? Invoke(
    object? obj,
    BindingFlags invokeAttr,
    Binder? binder,
    object?[]? parameters,
    CultureInfo? culture
  ) => throw new NotImplementedException();
  public override bool IsDefined(Type attributeType, bool inherit)
    => throw new NotImplementedException();
}

internal class FakeStackFrame : StackFrame {
  private readonly string? _fileName;
  private readonly string? _className;
  private readonly string? _methodName;

  public FakeStackFrame(
    string? fileName, string? className, string? methodName
  ) {
    _fileName = fileName;
    _methodName = methodName;
    _className = className;
  }

  public override string? GetFileName() => _fileName;
  public override int GetFileLineNumber() => 1;
  public override int GetFileColumnNumber() => 2;
  public override MethodBase? GetMethod()
    => (_methodName != null)
      ? new FakeMethodBase(_fileName, _className, _methodName)
      : null;
}

internal class FakeStackTrace : StackTrace {
  private readonly string? _fileName;
  private readonly string? _className;
  private readonly string? _methodName;

  public FakeStackTrace(
    string? fileName, string? className, string? methodName
  ) {
    _fileName = fileName;
    _className = className;
    _methodName = methodName;
  }

  public override StackFrame GetFrame(int index)
    => new FakeStackFrame(_fileName, _className, _methodName);

  public override StackFrame[] GetFrames() => new StackFrame[] {
    new FakeStackFrame(_fileName, _className, _methodName),
  };
}

internal class FakeType : Type {
  private readonly string? _className;

  public FakeType(string? className) => _className = className;

#pragma warning disable CS8764
  public override string? Name => _className ?? null!;
#pragma warning restore CS8764

  public override Assembly Assembly => throw new NotImplementedException();
  public override string AssemblyQualifiedName
    => throw new NotImplementedException();
  public override Type BaseType => throw new NotImplementedException();
  public override string FullName => throw new NotImplementedException();
  public override Guid GUID => throw new NotImplementedException();
  public override Module Module => throw new NotImplementedException();
  public override string Namespace => throw new NotImplementedException();
  public override Type UnderlyingSystemType
    => throw new NotImplementedException();
  public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
    => throw new NotImplementedException();
  public override object[] GetCustomAttributes(bool inherit)
    => throw new NotImplementedException();
  public override object[] GetCustomAttributes(Type attributeType, bool inherit)
    => throw new NotImplementedException();
  public override Type GetElementType() => throw new NotImplementedException();
  public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
    => throw new NotImplementedException();
  public override EventInfo[] GetEvents(BindingFlags bindingAttr)
    => throw new NotImplementedException();
  public override FieldInfo GetField(string name, BindingFlags bindingAttr)
    => throw new NotImplementedException();
  public override FieldInfo[] GetFields(BindingFlags bindingAttr)
    => throw new NotImplementedException();
  public override Type GetInterface(string name, bool ignoreCase)
    => throw new NotImplementedException();
  public override Type[] GetInterfaces() => throw new NotImplementedException();
  public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
    => throw new NotImplementedException();
  public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
    => throw new NotImplementedException();
  public override Type GetNestedType(string name, BindingFlags bindingAttr)
    => throw new NotImplementedException();
  public override Type[] GetNestedTypes(BindingFlags bindingAttr)
    => throw new NotImplementedException();
  public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
    => throw new NotImplementedException();
  public override object? InvokeMember(
    string name,
    BindingFlags invokeAttr,
    Binder? binder,
    object? target,
    object?[]? args,
    ParameterModifier[]? modifiers,
    CultureInfo? culture,
    string[]? namedParameters
  ) => throw new NotImplementedException();
  public override bool IsDefined(Type attributeType, bool inherit)
    => throw new NotImplementedException();
  protected override TypeAttributes GetAttributeFlagsImpl()
    => throw new NotImplementedException();
  protected override ConstructorInfo? GetConstructorImpl(
    BindingFlags bindingAttr,
    Binder? binder,
    CallingConventions callConvention,
    Type[] types,
    ParameterModifier[]? modifiers
  ) => throw new NotImplementedException();
  protected override MethodInfo? GetMethodImpl(
    string name,
    int genericParameterCount,
    BindingFlags bindingAttr,
    Binder? binder,
    CallingConventions callConvention,
    Type[]? types,
    ParameterModifier[]? modifiers
  ) => throw new NotImplementedException();
  protected override MethodInfo? GetMethodImpl(
    string name,
    BindingFlags bindingAttr,
    Binder? binder,
    CallingConventions callConvention,
    Type[]? types,
    ParameterModifier[]? modifiers
  ) => throw new NotImplementedException();
  protected override PropertyInfo? GetPropertyImpl(
    string name,
    BindingFlags bindingAttr,
    Binder? binder,
    Type? returnType,
    Type[]? types,
    ParameterModifier[]? modifiers
  ) => throw new NotImplementedException();
  protected override bool HasElementTypeImpl()
    => throw new NotImplementedException();
  protected override bool IsArrayImpl() => throw new NotImplementedException();
  protected override bool IsByRefImpl() => throw new NotImplementedException();
  protected override bool IsCOMObjectImpl()
    => throw new NotImplementedException();
  protected override bool IsPointerImpl()
    => throw new NotImplementedException();
  protected override bool IsPrimitiveImpl()
    => throw new NotImplementedException();
}
