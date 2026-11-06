// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.Tests.core.simulator;

// Minimal concrete subclass for testing the abstract NamedObject
internal sealed class TestNamedObject(string name) : NamedObject(name);

// ---------------------------------------------------------------------------
// NamedObject tests
// ---------------------------------------------------------------------------

public class NamedObjectTests
{
    [Fact]
    public void Constructor_WithName_StoresName()
    {
        var obj = new TestNamedObject("Alpha");
        Assert.Equal("Alpha", obj.getName());
    }

    [Fact]
    public void Constructor_WithNull_StoresUnnamed()
    {
        var obj = new TestNamedObject(null!);
        Assert.Equal("unnamed", obj.getName());
    }

    [Fact]
    public void GetName_ReturnsStoredName()
    {
        var obj = new TestNamedObject("Beta");
        Assert.Equal("Beta", obj.getName());
    }

    [Fact]
    public void GetQuotedName_WrapsInSingleQuotes()
    {
        var obj = new TestNamedObject("Gamma");
        Assert.Equal("'Gamma'", obj.getQuotedName());
    }

    [Fact]
    public void ToString_ReturnsName()
    {
        var obj = new TestNamedObject("Delta");
        Assert.Equal("Delta", obj.toString());
    }

    [Fact]
    public void GetQuotedName_NullNameStored_WrapsUnnamed()
    {
        var obj = new TestNamedObject(null!);
        Assert.Equal("'unnamed'", obj.getQuotedName());
    }
}

// ---------------------------------------------------------------------------
// NameCatalog tests
// ---------------------------------------------------------------------------

public class NameCatalogTests
{
    [Fact]
    public void RegisteredName_FirstCall_AppendsSuffixOne()
    {
        var catalog = new NameCatalog();
        string result = catalog.registeredName("Widget");
        Assert.Equal("Widget#1", result);
    }

    [Fact]
    public void RegisteredName_SecondCall_AppendsSuffixTwo()
    {
        var catalog = new NameCatalog();
        catalog.registeredName("Widget");
        string result = catalog.registeredName("Widget");
        Assert.Equal("Widget#2", result);
    }

    [Fact]
    public void RegisteredName_NullInput_RegistersAsUnnamed()
    {
        var catalog = new NameCatalog();
        string result = catalog.registeredName(null!);
        Assert.Equal("unnamed#1", result);
    }

    [Fact]
    public void RegisteredName_DifferentNames_GetOwnCounters()
    {
        var catalog = new NameCatalog();
        string a1 = catalog.registeredName("A");
        string b1 = catalog.registeredName("B");
        string a2 = catalog.registeredName("A");
        Assert.Equal("A#1", a1);
        Assert.Equal("B#1", b1);
        Assert.Equal("A#2", a2);
    }

    [Fact]
    public void GetNameWithoutSuffix_RemovesHashSuffix()
    {
        var catalog = new NameCatalog();
        Assert.Equal("Widget", catalog.getNameWithoutSuffix("Widget#1"));
    }

    [Fact]
    public void GetNameWithoutSuffix_NoHashInName_ReturnsAsIs()
    {
        var catalog = new NameCatalog();
        Assert.Equal("Widget", catalog.getNameWithoutSuffix("Widget"));
    }

    [Fact]
    public void GetNameWithoutSuffix_UnnamedString_ReturnsUnnamed()
    {
        var catalog = new NameCatalog();
        Assert.Equal("unnamed", catalog.getNameWithoutSuffix("unnamed"));
    }

    [Fact]
    public void GetNameWithoutSuffix_NullInput_ReturnsNull()
    {
        var catalog = new NameCatalog();
        Assert.Null(catalog.getNameWithoutSuffix(null!));
    }

    [Fact]
    public void RegisteredName_ManyCallsSameName_CountsCorrectly()
    {
        var catalog = new NameCatalog();
        for (int i = 1; i <= 5; i++)
        {
            string r = catalog.registeredName("Entity");
            Assert.Equal($"Entity#{i}", r);
        }
    }
}

// ---------------------------------------------------------------------------
// InterruptCode tests
// ---------------------------------------------------------------------------

public class InterruptCodeTests
{
    [Fact]
    public void Constructor_String_AssignsPositiveCodeNumber()
    {
        var code = new InterruptCode("IRQ1");
        Assert.True(code.getCodeNumber() > 0);
    }

    [Fact]
    public void Constructor_String_StoresName()
    {
        var code = new InterruptCode("Timeout");
        Assert.Equal("Timeout", code.getName());
    }

    [Fact]
    public void TwoDistinctCodes_HaveDifferentCodeNumbers()
    {
        var a = new InterruptCode("IRQ_A");
        var b = new InterruptCode("IRQ_B");
        Assert.NotEqual(a.getCodeNumber(), b.getCodeNumber());
    }

    [Fact]
    public void CloneConstructor_ProducesSameCodeNumber()
    {
        var original = new InterruptCode("Original");
        var clone = new InterruptCode(original);
        Assert.Equal(original.getCodeNumber(), clone.getCodeNumber());
    }

    [Fact]
    public void CloneConstructor_ProducesSameName()
    {
        var original = new InterruptCode("Original");
        var clone = new InterruptCode(original);
        Assert.Equal(original.getName(), clone.getName());
    }

    [Fact]
    public void StaticEquals_TwoClonesOfSameCode_ReturnsTrue()
    {
        var original = new InterruptCode("Same");
        var clone = new InterruptCode(original);
        Assert.True(InterruptCode.equals(original, clone));
    }

    [Fact]
    public void StaticEquals_TwoDifferentCodes_ReturnsFalse()
    {
        var a = new InterruptCode("A");
        var b = new InterruptCode("B");
        Assert.False(InterruptCode.equals(a, b));
    }

    [Fact]
    public void StaticEquals_FirstNull_ReturnsFalse()
    {
        var b = new InterruptCode("B");
        Assert.False(InterruptCode.equals(null!, b));
    }

    [Fact]
    public void StaticEquals_BothNull_ReturnsFalse()
    {
        Assert.False(InterruptCode.equals(null!, null!));
    }
}
