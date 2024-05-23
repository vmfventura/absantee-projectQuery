using Domain.Model;

namespace Domain.Tests;
public class ProjectTest
{
    public static readonly object[][] SuccessCasesWithProjects =
    [
        ["Teste", new DateOnly(DateTime.Now.Year, 01, 01), new DateOnly(DateTime.Now.Year, 12, 01)],
        ["Teste", new DateOnly(DateTime.Now.Year, 10, 01), null],
        ["Teste", new DateOnly(DateTime.Now.Year, 10, 01), new DateOnly(DateTime.Now.Year, 10, 01)],
        ["TesteTesteTesteTesteTeTesteTesteTestesteTesteTeste", new DateOnly(DateTime.Now.AddYears(1).Year, 01, 01), null]
    ];

    [Theory]
    [MemberData(nameof(SuccessCasesWithProjects))]
    public void WhenPassingCorrectListData_ThenProjectIsInstantiated(string strName, DateOnly dateStart, DateOnly? dateEnd)
    {
        var project = new Project(strName, dateStart, dateEnd);
    }

    public static readonly object[][] FailedNameCases =
    [
        ["", new DateOnly(DateTime.Now.Year,01,01), new DateOnly(DateTime.Now.Year,12,01)],
        ["                ", new DateOnly(DateTime.Now.Year,10,01), new DateOnly(DateTime.Now.Year,12,31)],
        [null, new DateOnly(DateTime.Now.Year,10,01), new DateOnly(DateTime.Now.Year,12,31)]
    ];


    [Theory, MemberData(nameof(FailedNameCases))]
    public void WhenPassingInvalidName_ThenThrowsException( string strName, DateOnly dataStart, DateOnly dataEnd)
    {
        // act
        var ex = Assert.Throws<ArgumentException>(() => new Project(strName, dataStart, dataEnd));
        // assert
        Assert.Equal("Invalid arguments.", ex.Message);
    }

    public static readonly object[][] FailedDatesCases =
    [
        ["Teste", new DateOnly(DateTime.Now.Year,12,01), new DateOnly(DateTime.Now.Year,10,31)],
        ["Teste", new DateOnly(DateTime.Now.Year,12,02), new DateOnly(DateTime.Now.Year,12,01)]
    ];

    [Theory, MemberData(nameof(FailedDatesCases))]
    public void WhenPassingInvalidDates_ThenThrowsException( string strName, DateOnly dataStart, DateOnly dataEnd)
    {
        // act
        var ex = Assert.Throws<ArgumentException>(() => new Project(strName, dataStart, dataEnd));
        // assert
        Assert.Equal("Invalid arguments.", ex.Message);
    }

}
