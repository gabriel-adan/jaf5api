using System;
using System.Xml;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using WebApi.Extentions;

namespace WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddJsonOptions(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            services.AddCors(options =>
            {
                options.AddPolicy("corsAllowAllPolicy",
                builder =>
                {
                    builder.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
                });
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            string xmlString = Configuration.GetSection("JwtPublicKey").Value;
            RSAParameters parameters = new RSAParameters();
            RSA rsa = RSA.Create();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);
            if (xmlDoc.DocumentElement.Name.Equals("RSAKeyValue"))
            {
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "Modulus": parameters.Modulus = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "Exponent": parameters.Exponent = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "P": parameters.P = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "Q": parameters.Q = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "DP": parameters.DP = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "DQ": parameters.DQ = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "InverseQ": parameters.InverseQ = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "D": parameters.D = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                    }
                }
            }
            else
            {
                throw new Exception("Invalid XML RSA key.");
            }

            rsa.ImportParameters(parameters);
            RsaSecurityKey signingKey = new RsaSecurityKey(rsa);

            string authority = Configuration.GetSection("Authority").Value;
            string authorityUrl = Configuration.GetSection("AuthorityUrl").Value;
            string audience = Configuration.GetSection("Audience").Value;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(token =>
            {
                token.RequireHttpsMetadata = false;
                token.Audience = audience;
                token.Authority = authorityUrl;
                token.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = signingKey,
                    ValidIssuer = authority,
                    ValidAudience = authorityUrl,
                    ValidateIssuerSigningKey = true,
                    RequireSignedTokens = true,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            string connectionString = Configuration.GetConnectionString("Default");
            string infrastructureAssemblyNamespace = Configuration.GetSection("AppConfig:InfrastructureAssemblyNamespace").Value;
            string logicAssemblyNamespace = Configuration.GetSection("AppConfig:LogicAssemblyNamespace").Value;
            var infrastructureAssembly = Assembly.Load(new AssemblyName(infrastructureAssemblyNamespace));
            var logicAssembly = Assembly.Load(new AssemblyName(logicAssemblyNamespace));

            RegisterScopedServices(services, infrastructureAssembly, "Repository");
            RegisterScopedServices(services, logicAssembly, "Logic");
            services.AddNHibernate(connectionString);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors("corsAllowAllPolicy");
            //app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMvc();
        }

        void RegisterScopedServices(IServiceCollection services, Assembly assembly, string subfix)
        {
            foreach (var type in assembly.ExportedTypes.Where(e => e.Name.EndsWith(subfix)))
            {
                var interfaces = type.GetInterfaces().Where(i => i.Name.EndsWith(subfix)).ToList();
                foreach (var f in interfaces)
                {
                    services.AddScoped(f, type);
                }
            }
        }
    }
}
