using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Factory;
using Domain.Model;

namespace Domain.Tests
{
    public class ProjectFactoryTest
    {
        // public Project NewProject(string strName, DateOnly dateStart, DateOnly? dateEnd)
        [Fact]
        public void IfPassingValidDates_ShouldReturnANewProject()
        {
            // Arrange
            string pStrName = "Project";
            DateOnly startDate = new DateOnly(DateTime.Now.Year, 1, 1);
            DateOnly endDate = new DateOnly(DateTime.Now.Year, 1, 5);
            ProjectFactory projectFactory = new ProjectFactory();
            // act
            var result = projectFactory.NewProject(pStrName, startDate, endDate);
            // assert
            Assert.IsType<Project>(result);
        }
    }
}
