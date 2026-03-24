using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TechSolutions_IPS_HW.Models.Api;
using TechSolutions_IPS_HW.Models.User;
using TechSolutions_IPS_HW.Services.AuthenticationServices;

namespace TechSolutions_IPS_HW.Controllers.Api
{

    //This class has not been extensively expanded as we for now just want to allow 
    //alternative (authorised) clients to interact with the system but unsure what
    //functionality should be granted
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RsaCryptoService _rsaCrypto;

        public TokenController(
            UserManager<ApplicationUser> userManager, 
            IConfiguration configuration, 
            RsaCryptoService rsaCrypto)
        {
            _userManager = userManager;
            _configuration = configuration;
            _rsaCrypto = rsaCrypto;
        }      
      
        /// <summary>
        /// Method used for encrypted login, we dont want to receive cleartext usernames and passwords, 
        /// a login is then performed and a JWT token is returned to the user
        /// </summary>
        /// <param name="request"></param>
        [HttpPost("encrypted")]
        public async Task<IActionResult> Post([FromBody] EncryptedLoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Data)) 
                return BadRequest();

            string json;
            try
            {
                json = _rsaCrypto.DecryptBase64(request.Data);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to decrypt payload: {ex.Message}");
            }

            var login = System.Text.Json.JsonSerializer.Deserialize<LoginRequest>(json);
            if (login == null) return BadRequest();

            return await PostWithClear(login);
        }


        /// <summary>
        /// Gets and send the public RSA key to the client, used to encrypt the username and password that the user 
        /// will supply when authenticating, to avoid sending cleartext credentials
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAuthKey()
        {
            var publicKey = _configuration["RSA:PublicKeyPem"];
            if (string.IsNullOrEmpty(publicKey))
            {
                return StatusCode(500, "Public key not configured. Set PublicKeyPem in configuration.");
            }
            return Ok(new { auth = publicKey });
        }


        #region Helpers    

        ///<summary>
        ///Gets the decrypted credentials from the Post method and performs the login
        ///</summary>
        /// <returns>JWT token in the case that the user:
        /// - Does exist
        /// - Supplied the correct credentials.
        /// </returns>
        public async Task<IActionResult> PostWithClear([FromBody] LoginRequest request)
        {
            if (request == null) return BadRequest();

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return Unauthorized();

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
                return Unauthorized();

            // Optional: enforce approval flag on user
            if (user is ApplicationUser appUser && !appUser.IsApproved)
                return Forbid();

            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || 
                string.IsNullOrEmpty(jwtAudience))            
                return StatusCode(500, "JWT configuration is missing. Configure Jwt:Key, Jwt:Issuer and Jwt:Audience.");
            

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // include roles
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r));

            var allClaims = claims.Concat(roleClaims).ToArray();

            var expires = DateTime.UtcNow.AddMinutes(60);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: allClaims,
                expires: expires,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { token = tokenString, expires });
        }

        #endregion
    }
}
