using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySecrets.Model;

namespace MySecrets.Services.Controller
{
    [Route("api/controller/app")]
    [ApiController]
    [AllowAnonymous]
    public class AppController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly RsaService _rsaService;
        private readonly AesService _aesService;


        public AppController(UserService userService, RsaService rsaService, AesService aesService)
        {
            _userService = userService;
            _rsaService = rsaService;
            _aesService = aesService;

            Console.WriteLine("AppController---->AppController::::");
        }

        [Route("/Error")]
        public IActionResult HandleError() =>
            Problem();

        [HttpPost("init")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<byte[]> Init([FromBody] InitDTO initDto)
        {
            byte[] publicKey = _rsaService.LocalPublicKeyArray;

            Console.WriteLine("ListSecrets---->PublicKeyArray::::" + Helper.ByteArrayToStringHex(publicKey));
            return publicKey;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<byte[]> Login([FromBody] byte[] loginDtoArray)
        {
            LoginDTO loginDto = _rsaService.Decrypt<LoginDTO>(loginDtoArray);
            LoginResultDTO loginResultDto = new LoginResultDTO();
            /*
             User? istruttore = _userService.FindByUserName(customer.Username);
             
             if (istruttore == null)
             {
                 loginResultDto.resultCode = -3;
                 loginResultDto.publicKeyArray = null;
                 
             }else
     */
            //bool passwordMatch = istruttore.Password == customer.Password;
            bool passwordMatch = true;
            if (passwordMatch)
            {
                Console.WriteLine(this.GetHashCode());

                Console.WriteLine("Login---->PublicKeyArray::::" + Helper.ByteArrayToStringHex(loginDto.PublicKey));
                _rsaService.CreateRsaData(loginDto.Username);
                _rsaService.GetRsaData(loginDto.Username).RemotePublicKeyArray = loginDto.PublicKey;
                loginResultDto.ResultCode = 0;
                loginResultDto.SymmetricKey = "b14ca5898a4e4133bbce2ea2315a1916";
                string? agentId = HttpContext.Request.Cookies["AgentID"];
                _rsaService.CreateRsaData(agentId);
                _rsaService.GetRsaData(agentId).SymmetricKey = loginResultDto.SymmetricKey;
            }
            else
            {
                loginResultDto.ResultCode = -1;
            }

            byte[] publicKey = loginDto.PublicKey;
            byte[] encrypted = _rsaService.Encrypt(loginResultDto, publicKey);
            return encrypted;
        }


        [HttpPost("listsecrets")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<byte[]> ListSecrets([FromBody] byte[] tokenDtoArray)
        { 
            string? agentId = HttpContext.Request.Cookies["AgentID"];
            string symmetricKey = _rsaService.GetRsaData(agentId).SymmetricKey;
            TokenDto tokenDto = _aesService.DecryptSymmetric<TokenDto>(tokenDtoArray,symmetricKey);

            User? istruttore = _userService.FindByUserName(tokenDto.Username);

            SecretsWrapper wrapper = new SecretsWrapper();
            foreach (var secret in istruttore.Secrets)
            {
                MySecretDto mySecretDto = new MySecretDto();
                mySecretDto.Name = secret.Name;
                mySecretDto.Value = secret.Value;
                wrapper.Secrets.Add(mySecretDto);
            }

            var rsaData = _rsaService.GetRsaData(tokenDto.Username);
            byte[] publicKey = rsaData.RemotePublicKeyArray;
            byte[] encrypted = _aesService.EncryptSymmetric(wrapper, symmetricKey);
            return encrypted;
        }
    }

    public class InitDTO
    {
        public string Id { get; set; }
    }

    public class LoginResultDTO
    {
        public int ResultCode { get; set; }
        public string SymmetricKey { get; set; }
    }

    public class TokenDto
    {
        public string Username { get; set; }
    }

    public class SecretsWrapper
    {
        public List<MySecretDto> Secrets { get; set; } = new List<MySecretDto>();
    }

    public class MySecretDto
    {
        public string Value { get; set; }
        public string Name { get; set; }
    }

    public class LoginDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public byte[] PublicKey { get; set; }
    }
}