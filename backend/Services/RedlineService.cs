using RqmtMgmtShared;
using backend.Models;
using System.Collections.Generic;

namespace backend.Services
{
    /// <summary>
    /// Interface for redline/diff comparison services between different versions of entities.
    /// Provides methods to compare requirement and test case versions for change tracking.
    /// </summary>
    public interface IRedlineService
    {
        /// <summary>
        /// Compares two requirement versions and returns the differences.
        /// </summary>
        /// <param name="oldV">The older requirement version.</param>
        /// <param name="newV">The newer requirement version.</param>
        /// <returns>A RedlineResultDto containing all field-level changes.</returns>
        RedlineResultDto CompareRequirements(RequirementVersion oldV, RequirementVersion newV);

        /// <summary>
        /// Compares two test case versions and returns the differences.
        /// </summary>
        /// <param name="oldV">The older test case version.</param>
        /// <param name="newV">The newer test case version.</param>
        /// <returns>A RedlineResultDto containing all field-level changes.</returns>
        RedlineResultDto CompareTestCases(TestCaseVersion oldV, TestCaseVersion newV);
    }

    /// <summary>
    /// Service implementation for redline/diff comparison between entity versions.
    /// Provides detailed field-by-field comparison logic for requirements and test cases with change type detection.
    /// </summary>
    public class RedlineService : IRedlineService
    {
        /// <summary>
        /// Compares two requirement versions and identifies all field-level changes.
        /// Analyzes title, description, type, status, and parent relationship changes.
        /// </summary>
        /// <param name="oldV">The older requirement version to compare from.</param>
        /// <param name="newV">The newer requirement version to compare to.</param>
        /// <returns>A RedlineResultDto containing detailed change information for each modified field.</returns>
        public RedlineResultDto CompareRequirements(RequirementVersion oldV, RequirementVersion newV)
        {
            var result = new RedlineResultDto
            {
                OldVersion = oldV.Version,
                NewVersion = newV.Version,
                Changes = new List<RedlineFieldChangeDto>()
            };

            // Compare each field individually to detect specific changes
            CompareField(result, "Title", oldV.Title, newV.Title);
            CompareField(result, "Description", oldV.Description, newV.Description);
            CompareField(result, "Type", oldV.Type.ToString(), newV.Type.ToString());
            CompareField(result, "Status", oldV.Status.ToString(), newV.Status.ToString());
            CompareField(result, "ParentId", oldV.ParentId?.ToString(), newV.ParentId?.ToString());
            
            return result;
        }

        /// <summary>
        /// Compares two test case versions and identifies all field-level changes.
        /// Analyzes title, description, steps, and expected result changes.
        /// </summary>
        /// <param name="oldV">The older test case version to compare from.</param>
        /// <param name="newV">The newer test case version to compare to.</param>
        /// <returns>A RedlineResultDto containing detailed change information for each modified field.</returns>
        public RedlineResultDto CompareTestCases(TestCaseVersion oldV, TestCaseVersion newV)
        {
            var result = new RedlineResultDto
            {
                OldVersion = oldV.Version,
                NewVersion = newV.Version,
                Changes = new List<RedlineFieldChangeDto>()
            };

            // Compare each field individually to detect specific changes
            CompareField(result, "Title", oldV.Title, newV.Title);
            CompareField(result, "Description", oldV.Description, newV.Description);
            CompareField(result, "Steps", oldV.Steps, newV.Steps);
            CompareField(result, "ExpectedResult", oldV.ExpectedResult, newV.ExpectedResult);
            
            return result;
        }

        /// <summary>
        /// Compares two field values and adds a change record if they differ.
        /// Determines change type (Added, Removed, Modified) based on null states of old and new values.
        /// </summary>
        /// <param name="result">The RedlineResultDto to add changes to.</param>
        /// <param name="field">The name of the field being compared.</param>
        /// <param name="oldValue">The previous value of the field (can be null).</param>
        /// <param name="newValue">The current value of the field (can be null).</param>
        private void CompareField(RedlineResultDto result, string field, string? oldValue, string? newValue)
        {
            // Skip if values are identical
            if (oldValue == newValue) return;
            
            // Determine change type based on null states
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