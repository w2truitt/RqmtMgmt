using backend.DTOs;
using backend.Models;
using System.Collections.Generic;

namespace backend.Services
{
    /// <summary>
    /// Provides redline/diff logic for comparing requirement and test case versions.
    /// </summary>
    public class RedlineService
    {
        public RedlineResultDto CompareRequirements(RequirementVersion oldV, RequirementVersion newV)
        {
            var result = new RedlineResultDto
            {
                OldVersion = oldV.Version,
                NewVersion = newV.Version,
                Changes = new List<RedlineFieldChangeDto>()
            };
            CompareField(result, "Title", oldV.Title, newV.Title);
            CompareField(result, "Description", oldV.Description, newV.Description);
            CompareField(result, "Type", oldV.Type.ToString(), newV.Type.ToString());
            CompareField(result, "Status", oldV.Status.ToString(), newV.Status.ToString());
            CompareField(result, "ParentId", oldV.ParentId?.ToString(), newV.ParentId?.ToString());
            return result;
        }

        public RedlineResultDto CompareTestCases(TestCaseVersion oldV, TestCaseVersion newV)
        {
            var result = new RedlineResultDto
            {
                OldVersion = oldV.Version,
                NewVersion = newV.Version,
                Changes = new List<RedlineFieldChangeDto>()
            };
            CompareField(result, "Title", oldV.Title, newV.Title);
            CompareField(result, "Description", oldV.Description, newV.Description);
            CompareField(result, "Steps", oldV.Steps, newV.Steps);
            CompareField(result, "ExpectedResult", oldV.ExpectedResult, newV.ExpectedResult);
            return result;
        }

        private void CompareField(RedlineResultDto result, string field, string? oldValue, string? newValue)
        {
            if (oldValue == newValue) return;
            var changeType = oldValue == null ? "Added" : newValue == null ? "Removed" : "Modified";
            result.Changes.Add(new RedlineFieldChangeDto
            {
                Field = field,
                OldValue = oldValue,
                NewValue = newValue,
                ChangeType = changeType
            });
        }
    }
}
