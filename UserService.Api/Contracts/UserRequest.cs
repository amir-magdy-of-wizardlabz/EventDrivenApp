using System.ComponentModel.DataAnnotations;

namespace UserService.Api.Contracts
{
    // Clients send only what they are allowed to set; the id comes from the database or the route.
    public record UserRequest(
        [Required, MaxLength(100)] string Name,
        [Required, EmailAddress] string Email);
}
