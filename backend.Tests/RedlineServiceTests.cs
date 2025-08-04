using System;
using RqmtMgmtShared;
using backend.Models;
using backend.Services;
using Xunit;

namespace backend.Tests
{
    public class RedlineServiceTests
    {
        private readonly RedlineService _service = new RedlineService();

        [Fact]
        public void CompareRequirements_DetectsTitleChange()
        {
            var oldV = new RequirementVersion { Version = 1, Title = "A", Status = RequirementStatus.Draft, Type = RequirementType.CRS };
            var newV = new RequirementVersion { Version = 2, Title = "B", Status = RequirementStatus.Draft, Type = RequirementType.CRS };
            var result = _service.CompareRequirements(oldV, newV);
            Assert.Contains(result.Changes, c => c.Field == "Title" && c.OldValue == "A" && c.NewValue == "B" && c.ChangeType == "Modified");
        }

        [Fact]
        public void CompareRequirements_DetectsNoChange()
        {
            var oldV = new RequirementVersion { Version = 1, Title = "A", Status = RequirementStatus.Draft, Type = RequirementType.CRS };
            var newV = new RequirementVersion { Version = 2, Title = "A", Status = RequirementStatus.Draft, Type = RequirementType.CRS };
            var result = _service.CompareRequirements(oldV, newV);
            Assert.DoesNotContain(result.Changes, c => c.Field == "Title");
        }

        [Fact]
        public void CompareRequirements_DetectsStatusChange()
        {
            var oldV = new RequirementVersion { Version = 1, Title = "A", Status = RequirementStatus.Draft, Type = RequirementType.CRS };
            var newV = new RequirementVersion { Version = 2, Title = "A", Status = RequirementStatus.Approved, Type = RequirementType.CRS };
            var result = _service.CompareRequirements(oldV, newV);
            Assert.Contains(result.Changes, c => c.Field == "Status" && c.OldValue == "Draft" && c.NewValue == "Approved" && c.ChangeType == "Modified");
        }

        [Fact]
        public void CompareRequirements_DetectsAddedField()
        {
            var oldV = new RequirementVersion { Version = 1, Title = "A", Status = RequirementStatus.Draft, Type = RequirementType.CRS, Description = null };
            var newV = new RequirementVersion { Version = 2, Title = "A", Status = RequirementStatus.Draft, Type = RequirementType.CRS, Description = "desc" };
            var result = _service.CompareRequirements(oldV, newV);
            Assert.Contains(result.Changes, c => c.Field == "Description" && c.OldValue == null && c.NewValue == "desc" && c.ChangeType == "Added");
        }

        [Fact]
        public void CompareRequirements_DetectsRemovedField()
        {
            var oldV = new RequirementVersion { Version = 1, Title = "A", Status = RequirementStatus.Draft, Type = RequirementType.CRS, Description = "desc" };
            var newV = new RequirementVersion { Version = 2, Title = "A", Status = RequirementStatus.Draft, Type = RequirementType.CRS, Description = null };
            var result = _service.CompareRequirements(oldV, newV);
            Assert.Contains(result.Changes, c => c.Field == "Description" && c.OldValue == "desc" && c.NewValue == null && c.ChangeType == "Removed");
        }

        [Fact]
        public void CompareTestCases_DetectsAllFieldChanges()
        {
            var oldV = new TestCaseVersion { Version = 1, Title = "A", Description = "desc", Steps = "step1", ExpectedResult = "pass" };
            var newV = new TestCaseVersion { Version = 2, Title = "B", Description = "desc2", Steps = "step2", ExpectedResult = "fail" };
            var result = _service.CompareTestCases(oldV, newV);
            Assert.Contains(result.Changes, c => c.Field == "Title" && c.OldValue == "A" && c.NewValue == "B");
            Assert.Contains(result.Changes, c => c.Field == "Description" && c.OldValue == "desc" && c.NewValue == "desc2");
            Assert.Contains(result.Changes, c => c.Field == "Steps" && c.OldValue == "step1" && c.NewValue == "step2");
            Assert.Contains(result.Changes, c => c.Field == "ExpectedResult" && c.OldValue == "pass" && c.NewValue == "fail");
        }
    }
}
