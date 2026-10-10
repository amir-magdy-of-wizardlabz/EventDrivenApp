using Microsoft.AspNetCore.Mvc;
using UserService.Api.Contracts;
using UserService.Api.Controllers;
using UserService.Core.Entities;

namespace UserService.Tests
{
    public class UsersControllerTests
    {
        private readonly UsersController _controller =
            new(new Core.Services.UserService(new InMemoryUserRepository(), new RecordingEventPublisher()));

        [Fact]
        public async Task Creating_a_user_returns_201_with_the_generated_id()
        {
            var result = await _controller.AddUser(new UserRequest("Ann", "ann@example.com"));

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(1, Assert.IsType<User>(created.Value).Id);
        }

        [Fact]
        public async Task Creating_a_user_with_a_taken_email_returns_409()
        {
            await _controller.AddUser(new UserRequest("Ann", "ann@example.com"));

            var result = await _controller.AddUser(new UserRequest("Other", "ann@example.com"));

            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task Updating_a_user_to_a_taken_email_returns_409()
        {
            await _controller.AddUser(new UserRequest("Ann", "ann@example.com"));
            await _controller.AddUser(new UserRequest("Bob", "bob@example.com"));

            var result = await _controller.UpdateUser(2, new UserRequest("Bob", "ann@example.com"));

            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task A_missing_user_returns_404_for_get_update_and_delete()
        {
            Assert.IsType<NotFoundResult>(await _controller.GetUser(42));
            Assert.IsType<NotFoundResult>(await _controller.UpdateUser(42, new UserRequest("Nobody", "nobody@example.com")));
            Assert.IsType<NotFoundResult>(await _controller.DeleteUser(42));
        }
    }
}
