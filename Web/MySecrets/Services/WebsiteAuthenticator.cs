using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MySecrets.DTO;
using MySecrets.Model;
using Newtonsoft.Json;
using ScuolaRegionale.Services;

namespace MySecrets.Services
{
    public class WebsiteAuthenticator
        : AuthenticationStateProvider
    {
        private readonly ProtectedSessionStorage _protectedLocalStorage;
        private readonly UserService _userService;

        public WebsiteAuthenticator(ProtectedSessionStorage protectedLocalStorage, UserService userService)
        {
            _protectedLocalStorage = protectedLocalStorage;
            _protectedLocalStorage.DeleteAsync("identity");
            _userService = userService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var principal = new ClaimsPrincipal();

            try
            {
                var storedPrincipal = await _protectedLocalStorage.GetAsync<string>("identity");

                if (storedPrincipal.Success)
                {
                    var user = JsonConvert.DeserializeObject<User>(storedPrincipal.Value);
                    var (_, isLookUpSuccess) = LookUpUser(user.UserName, user.Password);

                    if (isLookUpSuccess)
                    {
                        var identity = CreateIdentityFromUser(user);
                        principal = new(identity);
                    }
                }
            }
            catch (Exception e)
            {
            }

            return new AuthenticationState(principal);
        }

        public async Task<int> LoginAsync(LoginFormModel loginFormModel)
        {
            int status = 0;
            var principal = new ClaimsPrincipal();

            var (userInDatabase, isSuccess) = LookUpUser(loginFormModel.UserName, loginFormModel.Password);
            UserDTO userDto = new UserDTO(loginFormModel.UserName, loginFormModel.Password);

            if (userInDatabase != null)
            {
                if (isSuccess)
                {
                    var identity = CreateIdentityFromUser(userInDatabase);
                    principal = new ClaimsPrincipal(identity);
                    await _protectedLocalStorage.SetAsync("identity", JsonConvert.SerializeObject(userDto,
                        new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
                }
                else
                {
                    status = -1;
                }
            }
            else
            {
                status = -2;
            }


            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
            return status;
        }

        public async Task LogoutAsync()
        {
            await _protectedLocalStorage.DeleteAsync("identity");

            var principal = new ClaimsPrincipal();
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        }

        private static ClaimsIdentity CreateIdentityFromUser(User user)
        {
            return new ClaimsIdentity(new Claim[]
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Hash, user.Password),
            }, "BlazorSchool");
        }

        private (User?, bool) LookUpUser(string username, string password)
        {
            bool success = false;
            var istruttore = _userService.FindByUserName(username);
            success = istruttore.Password == password;

            return (istruttore, success);
        }
    }
}