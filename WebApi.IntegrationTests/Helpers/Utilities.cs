using System.Runtime.Intrinsics.Arm;
using DataModel.Model;
using DataModel.Repository;
using Domain.Model;

namespace WebApi.IntegrationTests.Helpers;

public class Utilities
{
    public static void InitializeDbForTests(AbsanteeContext db)
    {
        db.Projects.AddRange(GetSeedingProjectsDataModel());
        db.SaveChanges();
    }

    public static void ReinitializeDbForTests(AbsanteeContext db)
    {
        db.Projects.RemoveRange(db.Projects);
        // db.Projects.
        InitializeDbForTests(db);
    }

    public static List<ProjectDataModel> GetSeedingProjectsDataModel()
    {
        return new List<ProjectDataModel>()
        {
            new ProjectDataModel(new Project("Project 1", new DateOnly(2022, 1, 1), new DateOnly(2022, 1, 2))),
            new ProjectDataModel(new Project("Project 2", new DateOnly(2022, 1, 3), new DateOnly(2022, 1, 4))),
            new ProjectDataModel(new Project("Project 3", new DateOnly(2022, 1, 5), new DateOnly(2022, 1, 6)))
        };
    }
}
