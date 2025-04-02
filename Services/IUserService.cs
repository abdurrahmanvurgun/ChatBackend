using System.Threading.Tasks;
using ChatApp.Backend.Models;

namespace ChatApp.Backend.Services
{
    public interface IUserService
    {
        Task<User> Register(UserRegistrationDto registrationDto);
        Task<string> Login(UserLoginDto loginDto);
    }
}