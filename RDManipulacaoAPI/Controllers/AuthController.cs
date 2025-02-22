using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RDManipulacaoAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RDManipulacaoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Realiza a autenticação do usuário e retorna um token JWT válido por 30 minutos.
        /// </summary>
        /// <param name="login">Dados de login (usuário e senha).</param>
        /// <returns>Token JWT caso as credenciais estejam corretas.</returns>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel login)
        {
            // Validação simplificada: em produção, não seria dessa forma e sim acoplado a um serviço
            if (login.Username != "admin" || login.Password != "password")
            {
                return Unauthorized("Credenciais inválidas.");
            }

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, login.Username)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new { token = tokenString });
        }
    }
}
