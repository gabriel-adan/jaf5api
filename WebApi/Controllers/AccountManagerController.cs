using System;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using WebApi.Models;
using Domain;
using Logic;
using Logic.Contracts;

namespace WebApi.Controllers
{
    public class AccountManagerController : ApiBaseController
    {
        private readonly IConfiguration Configuration;
        private readonly IUserLogic userLogic;
        private readonly Random RandomCodeGenerator;
        private readonly int MinValueVerifyCode, MaxValueVerifyCode;
        private readonly string AUTH_SERVICE_NOT_AVAILABLE = "Servicio de autenticación no disponible";

        public AccountManagerController(ILogger<ApiBaseController> logger, IConfiguration configuration, IUserLogic userLogic) : base(logger)
        {
            Configuration = configuration;
            this.userLogic = userLogic;
            RandomCodeGenerator = new Random();
            MinValueVerifyCode = configuration.GetValue<int>("MinValueVerifyCode");
            MaxValueVerifyCode = configuration.GetValue<int>("MaxValueVerifyCode");
        }

        [HttpPost("LogIn")]
        public async Task<IActionResult> LogIn(UserForm userForm)
        {
            try
            {
                string baseAuthUrl = Configuration.GetSection("AuthorityUrl").Value;
                string applicationName = Configuration.GetSection("AppConfig:Name").Value;
                string domainName = Configuration.GetSection("Audience").Value;
                string authField = Configuration.GetSection("AuthenticationField").Value;
                string authUri = Configuration.GetSection("AuthUri").Value;

                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(baseAuthUrl);
                var obj = new
                {
                    UserName = userForm.UserName,
                    Password = userForm.Password,
                    ApplicationName = applicationName,
                    DomainName = domainName,
                    AuthenticationField = authField
                };
                string json = JsonConvert.SerializeObject(obj);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage result = await httpClient.PostAsync(authUri + "/LogIn", content);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    LogInResult logInResult = await result.Content.ReadAsAsync(typeof(LogInResult)) as LogInResult;
                    return Ok(logInResult);
                }
                string message = await result.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(message) && result.StatusCode == HttpStatusCode.NotFound)
                    throw new Exception(AUTH_SERVICE_NOT_AVAILABLE);
                return NotFound(new { Message = message });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "LogIn method");
                return BadRequest(new { Message = "Ocurrió un error" });
            }
        }

        [HttpPost("PlayerSignIn")]
        public async Task<IActionResult> PlayerSignIn(PlayerSignInForm playerSignInForm)
        {
            try
            {
                Helper.ThrowIfIsNullOrEmpty(playerSignInForm.Email, "Debe ingresar un correo electrónico");

                string baseAuthUrl = Configuration.GetSection("Authority").Value;
                string applicationName = Configuration.GetSection("AppConfig:Name").Value;
                string authUri = Configuration.GetSection("AuthUri").Value;
                
                int verifyCode = RandomCodeGenerator.Next(MinValueVerifyCode, MaxValueVerifyCode);
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(baseAuthUrl);
                    var obj = new
                    {
                        FirstName = playerSignInForm.FirstName,
                        LastName = playerSignInForm.LastName,
                        Email = playerSignInForm.Email,
                        Password = playerSignInForm.Password,
                        IsEnabled = false,
                        VerifyCode = verifyCode,
                        Roles = new List<string>() { "Player" },
                        ApplicationName = applicationName,
                        AuthenticationField = 0
                    };

                    string json = JsonConvert.SerializeObject(obj);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage result = await httpClient.PostAsync(authUri + "/SignIn", content);
                    string message = await result.Content.ReadAsStringAsync();
                    if (result.StatusCode == HttpStatusCode.OK)
                        return Ok(message);
                    if (string.IsNullOrEmpty(message) && result.StatusCode == HttpStatusCode.NotFound)
                        throw new Exception(AUTH_SERVICE_NOT_AVAILABLE);
                    return NotFound(new { Message = message });
                }
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "PlayerSignIn method");
                return BadRequest(new { Message = "Ocurrió un error" });
            }
        }

        [HttpPost("PlayerAccountConfirm")]
        public async Task<IActionResult> PlayerAccountConfirm(UserConfirmAccountForm form)
        {
            try
            {
                Helper.ThrowIfIsNullOrEmpty(form.UserName, "Usuario inválido");
                Helper.ThrowIfIsNullOrEmpty(form.VerifyCode, "Código de verificación inválido");

                string baseAuthUrl = Configuration.GetSection("Authority").Value;
                string applicationName = Configuration.GetSection("AppConfig:Name").Value;
                string domainName = Configuration.GetSection("Audience").Value;
                string authUri = Configuration.GetSection("AuthUri").Value;

                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(baseAuthUrl);
                    var obj = new
                    {
                        UserName = form.UserName,
                        VerifyCode = form.VerifyCode,
                        ApplicationName = applicationName,
                        DomainName = domainName,
                        AuthenticationField = 0
                    };

                    string json = JsonConvert.SerializeObject(obj);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage result = await httpClient.PostAsync(authUri + "/ConfirmAccount", content);
                    if (result.StatusCode == HttpStatusCode.OK)
                    {
                        LogInResult logInResult = await result.Content.ReadAsAsync(typeof(LogInResult)) as LogInResult;
                        return Ok(logInResult);
                    }
                    else
                    {
                        string message = await result.Content.ReadAsStringAsync();
                        if (string.IsNullOrEmpty(message) && result.StatusCode == HttpStatusCode.NotFound)
                            throw new Exception(AUTH_SERVICE_NOT_AVAILABLE);
                        return NotFound(new { Message = message });
                    }
                }
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "PlayerAccountConfirm method");
                return NotFound(new { Message = "Ocurrió un error." });
            }
        }

        [HttpPost("CustomerSigIn")]
        public async Task<IActionResult> CustomerSigIn(CustomerSigInForm form)
        {
            try
            {
                Helper.ThrowIfIsNullOrEmpty(form.Email, "Debe ingresar un correo electrónico");

                string baseAuthUrl = Configuration.GetSection("Authority").Value;
                string applicationName = Configuration.GetSection("AppConfig:Name").Value;
                string authUri = Configuration.GetSection("AuthUri").Value;

                int verifyCode = RandomCodeGenerator.Next(MinValueVerifyCode, MaxValueVerifyCode);

                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(baseAuthUrl);
                    var obj = new
                    {
                        FirstName = form.FirstName,
                        LastName = form.LastName,
                        Email = form.Email,
                        Password = form.Password,
                        IsEnabled = false,
                        VerifyCode = verifyCode,
                        Roles = new List<string>() { "Owner" },
                        ApplicationName = applicationName,
                        AuthenticationField = 0
                    };

                    string json = JsonConvert.SerializeObject(obj);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage result = await httpClient.PostAsync(authUri + "/SignIn", content);
                    string message = await result.Content.ReadAsStringAsync();
                    if (result.StatusCode == HttpStatusCode.OK)
                    {
                        Customer customer = userLogic.CustomerExists(form.Email);
                        if (customer == null)
                            customer = userLogic.CreateCustomer(form.FirstName, form.LastName, form.Email);
                        return Ok(new { Message = message });
                    }
                    if (result.StatusCode == HttpStatusCode.BadRequest)
                        return NotFound(new { Message = message });
                    if (string.IsNullOrEmpty(message) && result.StatusCode == HttpStatusCode.NotFound)
                        throw new Exception(AUTH_SERVICE_NOT_AVAILABLE);
                    return NotFound(new { Message = message });
                }
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "CustomerSigIn method");
                return NotFound(new { Message = "Ocurrió un error." });
            }
        }

        [HttpPost("CustomerAccountConfirm")]
        public async Task<IActionResult> CustomerAccountConfirm(UserConfirmAccountForm form)
        {
            try
            {
                Helper.ThrowIfIsNullOrEmpty(form.UserName, "Usuario inválido");
                Helper.ThrowIfIsNullOrEmpty(form.VerifyCode, "Código de verificación inválido");

                string baseAuthUrl = Configuration.GetSection("Authority").Value;
                string applicationName = Configuration.GetSection("AppConfig:Name").Value;
                string domainName = Configuration.GetSection("Audience").Value;
                string authUri = Configuration.GetSection("AuthUri").Value;

                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(baseAuthUrl);
                    var obj = new
                    {
                        UserName = form.UserName,
                        VerifyCode = form.VerifyCode,
                        ApplicationName = applicationName,
                        DomainName = domainName,
                        AuthenticationField = 0
                    };

                    string json = JsonConvert.SerializeObject(obj);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage result = await httpClient.PostAsync(authUri + "/ConfirmAccount", content);
                    if (result.StatusCode == HttpStatusCode.OK)
                    {
                        Customer customer = userLogic.CustomerExists(form.UserName);
                        Helper.ThrowIf(customer == null, "No se encontró la cuenta para el email especificado");
                        User user = userLogic.CreateUser(customer.FirstName, customer.LastName, customer.Email);
                        LogInResult logInResult = await result.Content.ReadAsAsync(typeof(LogInResult)) as LogInResult;
                        return Ok(logInResult);
                    }
                    else
                    {
                        string message = await result.Content.ReadAsStringAsync();
                        if (string.IsNullOrEmpty(message) && result.StatusCode == HttpStatusCode.NotFound)
                            throw new Exception(AUTH_SERVICE_NOT_AVAILABLE);
                        return NotFound(new { Message = message });
                    }
                }
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "CustomerAccountConfirm method");
                return NotFound(new { Message = "Ocurrió un error." });
            }
        }

        [HttpGet]
        public IActionResult Get()
        {
            string appName = Configuration.GetSection("AppConfig:Name").Value;
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string response = string.Format("{0} Versión {1}", appName, version);
            return Ok(response);
        }
    }
}
