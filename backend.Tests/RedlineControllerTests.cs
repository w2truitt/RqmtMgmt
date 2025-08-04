using backend.Controllers;
using backend.Data;
using RqmtMgmtShared;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Tests
{
    public class RedlineControllerTests
    {
        private static RqmtMgmtDbContext GetDbContextWithData(
            List<RequirementVersion>? reqVersions = null,
            List<TestCaseVersion>? tcVersions = null)
        {
            var options = new DbContextOptionsBuilder<RqmtMgmtDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            var db = new RqmtMgmtDbContext(options);
            if (reqVersions != null)
            {
                db.RequirementVersions.AddRange(reqVersions);
                db.SaveChanges();
            }
            if (tcVersions != null)
            {
                db.TestCaseVersions.AddRange(tcVersions);
                db.SaveChanges();
            }
            return db;
        }

        [Fact]
        public async Task GetRequirementVersions_ReturnsVersionsOfRequirement()
        {
            var reqId = 1;
            var db = GetDbContextWithData(new List<RequirementVersion> {
                new RequirementVersion { Id=1, RequirementId=reqId, Version=1, Title="A", Type=0, Status=0, ModifiedBy=1, ModifiedAt=System.DateTime.Now },
                new RequirementVersion { Id=2, RequirementId=reqId, Version=2, Title="B", Type=0, Status=0, ModifiedBy=1, ModifiedAt=System.DateTime.Now }
            });
            var controller = new RedlineController(db);
            var result = await controller.GetRequirementVersions(reqId);
            var ok = Assert.IsType<OkObjectResult>(result);
            var versions = Assert.IsAssignableFrom<List<RequirementVersion>>(ok.Value);
            Assert.Equal(2, versions.Count);
        }

        [Fact]
        public async Task GetRequirementVersion_ReturnsNotFoundIfMissing()
        {
            var db = GetDbContextWithData();
            var controller = new RedlineController(db);
            var result = await controller.GetRequirementVersion(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task RedlineRequirement_ReturnsNotFoundIfAnyVersionMissing()
        {
            var db = GetDbContextWithData();
            var controller = new RedlineController(db);
            var result = await controller.RedlineRequirement(1, 2);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task RedlineRequirement_ReturnsDiffResult()
        {
            var db = GetDbContextWithData(new List<RequirementVersion> {
                new RequirementVersion { Id=1, RequirementId=1, Version=1, Title="A", Type=0, Status=0, ModifiedBy=1, ModifiedAt=System.DateTime.Now },
                new RequirementVersion { Id=2, RequirementId=1, Version=2, Title="B", Type=0, Status=0, ModifiedBy=1, ModifiedAt=System.DateTime.Now }
            });
            var controller = new RedlineController(db);
            var result = await controller.RedlineRequirement(1, 2);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<RedlineResultDto>(ok.Value);
            Assert.Equal(1, dto.Changes.Count); // Title changed
        }

        [Fact]
        public async Task GetTestCaseVersions_ReturnsVersionsOfTestCase()
        {
            var tcId = 1;
            var db = GetDbContextWithData(null, new List<TestCaseVersion> {
                new TestCaseVersion { Id=1, TestCaseId=tcId, Version=1, Title="A", ModifiedBy=1, ModifiedAt=System.DateTime.Now },
                new TestCaseVersion { Id=2, TestCaseId=tcId, Version=2, Title="B", ModifiedBy=1, ModifiedAt=System.DateTime.Now }
            });
            var controller = new RedlineController(db);
            var result = await controller.GetTestCaseVersions(tcId);
            var ok = Assert.IsType<OkObjectResult>(result);
            var versions = Assert.IsAssignableFrom<List<TestCaseVersion>>(ok.Value);
            Assert.Equal(2, versions.Count);
        }

        [Fact]
        public async Task GetTestCaseVersion_ReturnsNotFoundIfMissing()
        {
            var db = GetDbContextWithData();
            var controller = new RedlineController(db);
            var result = await controller.GetTestCaseVersion(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task RedlineTestCase_ReturnsNotFoundIfAnyVersionMissing()
        {
            var db = GetDbContextWithData();
            var controller = new RedlineController(db);
            var result = await controller.RedlineTestCase(1, 2);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task RedlineTestCase_ReturnsDiffResult()
        {
            var db = GetDbContextWithData(null, new List<TestCaseVersion> {
                new TestCaseVersion { Id=1, TestCaseId=1, Version=1, Title="A", ModifiedBy=1, ModifiedAt=System.DateTime.Now },
                new TestCaseVersion { Id=2, TestCaseId=1, Version=2, Title="B", ModifiedBy=1, ModifiedAt=System.DateTime.Now }
            });
            var controller = new RedlineController(db);
            var result = await controller.RedlineTestCase(1, 2);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<RedlineResultDto>(ok.Value);
            Assert.Equal(1, dto.Changes.Count); // Title changed
        }
    }
}
