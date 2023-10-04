

namespace MySecrets.Services
{
    public class DatabaseInitializatorService
    {
        private UserService _userService;

        public DatabaseInitializatorService(UserService userService)
        {
            _userService = userService;
        }

        public void Initialize( )
        {
            _userService.CreateAdmin();


        }
    }
}