using System;
using backend.Models;
using Xunit;

namespace backend.Tests
{
    public class AuditLogTests
    {
        [Fact]
        public void CanSetAndGet_AllProperties()
        {
            var user = new User { Id = 5, UserName = "tester", Email = "tester@test.com" };
            var now = DateTime.UtcNow;
            var log = new AuditLog
            {
                Id = 1,
                UserId = 2,
                Action = "Created",
                Entity = "Requirement",
                EntityId = 3,
                Timestamp = now,
                Details = "Some details",
                User = user
            };
            Assert.Equal(1, log.Id);
            Assert.Equal(2, log.UserId);
            Assert.Equal("Created", log.Action);
            Assert.Equal("Requirement", log.Entity);
            Assert.Equal(3, log.EntityId);
            Assert.Equal(now, log.Timestamp);
            Assert.Equal("Some details", log.Details);
            Assert.Equal(user, log.User);
        }

        [Fact]
        public void EntityAndDetails_AcceptNull()
        {
            var log = new AuditLog { Action = "Updated", Timestamp = DateTime.UtcNow };
            Assert.Null(log.Entity);
            Assert.Null(log.Details);
        }
    }
}
