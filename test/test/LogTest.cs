using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using Godot;
using GoDotLog;
using GoDotTest;
using Moq;
using Shouldly;

// Since log is used by GoDotTest, it creates a circular dependency warning
// we can safely ignore.
#pragma warning disable CS0436
public class LogTest : TestClass {

  private StringBuilder _print = null!;
  private StringBuilder _warn = null!;
  private StringBuilder _error = null!;

  public LogTest(Node testScene) : base(testScene) { }

  [Setup]
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

  [Test]
  public void Prints() {
    var log = new GDLog("Prefix");
    log.Print("Hello, world!");
    _print.ToString().ShouldBe("Prefix: Hello, world!\n");
  }

  [Test]
  public void PrintsStackTrace() {
    var log = new GDLog("Prefix");
    var st = new Mock<StackTrace>();
    log.Print(new FakeStackTrace(true));
    _print.ToString().ShouldBe(
      "Prefix: ClassName.MethodName in File.cs(1,2)\n"
    );
  }

  [Test]
  public void PrintsStackTraceWithDefaults() {
    var log = new GDLog("Prefix");
    var st = new Mock<StackTrace>();
    log.Print(new FakeStackTrace(false));
    _print.ToString().ShouldBe(
      "Prefix: UnknownClass.MethodName in **(1,2)\n"
    );
  }

  [Test]
  public void PrintsException() {
    var e = new Exception("message");
    var log = new GDLog("Prefix");
    log.Print(e);
    var output = string.Join("\n", new string[] {
      "Prefix: An error ocurred.",
      "Prefix: System.Exception: message\n"
    });
    _print.ToString().ShouldBe(output);
    _error.ToString().ShouldBe(output);
  }

  [Test]
  public void Warns() {
    var log = new GDLog("Prefix");
    log.Warn("Hello, world!");
    _print.ToString().ShouldBe("Prefix: Hello, world!\n");
    _warn.ToString().ShouldBe("Prefix: Hello, world!\n");
  }

  [Test]
  public void Errors() {
    var log = new GDLog("Prefix");
    log.Error("Hello, world!");
    _print.ToString().ShouldBe("Prefix: Hello, world!\n");
    _error.ToString().ShouldBe("Prefix: Hello, world!\n");
  }

  [Test]
  public void AssertsSuccessfully() {
    var log = new GDLog("Prefix");
    log.Assert(true, "message");
    _print.ToString().ShouldBeEmpty();
    _warn.ToString().ShouldBeEmpty();
    _error.ToString().ShouldBeEmpty();
  }

  [Test]
  public void AssertsError() {
    var log = new GDLog("Prefix");
    var e = Should.Throw<AssertionException>(
      () => log.Assert(false, "error message")
    );
    e.Message.ShouldContain("error message");
    var output = "Prefix: error message\n";
    _print.ToString().ShouldBe(output);
    _error.ToString().ShouldBe(output);
  }

  [Test]
  public void RunsSuccessfully() {
    var log = new GDLog("Prefix");
    log.Run(() => { });
    _print.ToString().ShouldBeEmpty();
    _warn.ToString().ShouldBeEmpty();
    _error.ToString().ShouldBeEmpty();
  }

  [Test]
  public void RunsError() {
    var log = new GDLog("Prefix");
    var called = false;
    var exception = new Exception("error message");
    var e = Should.Throw<Exception>(
      () => log.Run(
        () => throw exception,
        (e) => { e.ShouldBe(exception); called = true; }
      )
    );
    _print.ToString().ShouldNotBeEmpty();
    called.ShouldBeTrue();
    e.Message.ShouldContain("error message");
  }

  [Test]
  public void RunReturnsSuccessfully() {
    var log = new GDLog("Prefix");
    var result = log.Run(() => "value");
    result.ShouldBe("value");
  }

  [Test]
  public void RunThrowsOnReturn() {
    var log = new GDLog("Prefix");
    var called = false;
    var exception = new Exception("error message");
    var e = Should.Throw<Exception>(
      () => log.Run<string>(
        () => throw exception,
        (e) => { e.ShouldBe(exception); called = true; }
      )
    );
    called.ShouldBe(true);
    _print.ToString().ShouldNotBeEmpty();
    e.Message.ShouldContain("error message");
  }

  [Test]
  public void AlwaysReturns() {
    var log = new GDLog("Prefix");
    var result = log.Always(() => "value", "fallback");
    result.ShouldBe("value");
  }

  [Test]
  public void AlwaysReturnsFallback() {
    var log = new GDLog("Prefix");
    var result = log.Always(
      () => throw new Exception("error message"), "fallback"
    );
    _warn.ToString().ShouldNotBeEmpty();
  }

  [CleanupAll]
  public void CleanupAll() {
    GDLog.PrintAction = GDLog.DefaultPrint;
    GDLog.PushWarningAction = GDLog.DefaultPushWarning;
    GDLog.PushErrorAction = GDLog.DefaultPushError;
  }
}
#pragma warning restore CS0436

internal class FakeMethodBase : MethodBase {
  private readonly bool _valid = false;
  public FakeMethodBase(bool valid) => _valid = valid;

  public override Type DeclaringType => _valid ? new FakeType() : null!;
  public override string Name => "MethodName";

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
  public override object Invoke(
    object obj,
    BindingFlags invokeAttr,
    Binder binder,
    object[] parameters,
    CultureInfo culture
  ) => throw new NotImplementedException();
  public override bool IsDefined(Type attributeType, bool inherit)
    => throw new NotImplementedException();
}

internal class FakeStackFrame : StackFrame {
  private readonly bool _valid = false;
  public FakeStackFrame(bool valid) => _valid = valid;

  public override string GetFileName() => _valid ? "File.cs" : null!;
  public override int GetFileLineNumber() => 1;
  public override int GetFileColumnNumber() => 2;
  public override MethodBase GetMethod() => new FakeMethodBase(_valid);
}

internal class FakeStackTrace : StackTrace {
  private readonly bool _valid = false;
  public FakeStackTrace(bool valid) => _valid = valid;

  public override StackFrame GetFrame(int index)
    => new FakeStackFrame(_valid);

  public override StackFrame[] GetFrames() => new StackFrame[] {
    new FakeStackFrame(_valid),
  };
}

internal class FakeType : Type {
  public override string Name => "ClassName";

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
  public override object InvokeMember(
    string name,
    BindingFlags invokeAttr,
    Binder binder,
    object target,
    object[] args,
    ParameterModifier[] modifiers,
    CultureInfo culture,
    string[] namedParameters
  ) => throw new NotImplementedException();
  public override bool IsDefined(Type attributeType, bool inherit)
    => throw new NotImplementedException();
  protected override TypeAttributes GetAttributeFlagsImpl()
    => throw new NotImplementedException();
  protected override ConstructorInfo GetConstructorImpl(
    BindingFlags bindingAttr,
    Binder binder,
    CallingConventions callConvention,
    Type[] types,
    ParameterModifier[] modifiers
  ) => throw new NotImplementedException();
  protected override MethodInfo GetMethodImpl(
    string name,
    BindingFlags bindingAttr,
    Binder binder,
    CallingConventions callConvention,
    Type[] types,
    ParameterModifier[] modifiers
  ) => throw new NotImplementedException();
  protected override PropertyInfo GetPropertyImpl(
    string name,
    BindingFlags bindingAttr,
    Binder binder,
    Type returnType,
    Type[] types,
    ParameterModifier[] modifiers
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
