using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

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
      services.AddCors(option =>
      {
        option.AddPolicy("any", builder =>
        {
          builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        });
      });

      services.AddControllers();
      //swagger docs
      services.AddSwaggerGen(c =>
        {
          c.SwaggerDoc("v1", new OpenApiInfo
          {
            Title = "Web Api Template",
            Version = "v1",
            Description = "Web Api 模板 3.1版本",
            TermsOfService = new Uri("https://minskiter.github.io"),
            Contact = new OpenApiContact
            {
              Name = "minskiter",
              Email = "minskiter@gmail.com",
              Url = new Uri("https://blog.leavessoft.cn")
            },
            License = new OpenApiLicense
            {
              Name = "MIT",
            }
          });
          var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
          var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
          c.IncludeXmlComments(xmlPath);
        }
      );
      // Jwt Token
      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option =>
      {
        option.TokenValidationParameters = new TokenValidationParameters
        {
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"])),
          ValidIssuer = Configuration["Jwt:Issue"],
          ValidateIssuer = true,
          ValidateAudience = false,
          ValidateIssuerSigningKey = true, // validate signingkey
          ValidateLifetime = true, // validate datetime 
        };
      });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseSwagger();

      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web Api V1");
        c.RoutePrefix = string.Empty;
      });

      app.UseRouting();
      // Cors Middleware Must be between Routing And Endpoints.
      app.UseCors("any");
      // Authentication before authorization
      app.UseAuthentication();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });
    }
  }
}
