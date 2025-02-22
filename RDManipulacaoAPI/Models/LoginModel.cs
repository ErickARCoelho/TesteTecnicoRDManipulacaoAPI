namespace RDManipulacaoAPI.Models
{
    /// <summary>
    /// Representa os dados necessários para realizar o login.
    /// </summary>
    public class LoginModel
    {
        /// <summary>
        /// Nome do usuário.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Senha do usuário.
        /// </summary>
        public string Password { get; set; }
    }
}
