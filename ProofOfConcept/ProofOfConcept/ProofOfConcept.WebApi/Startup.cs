using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using ProofOfConcept.Infrastructure;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ProofOfConcept.WebApi
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
            services.AddInfrastructure(Configuration);
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "ProofOfConcept - WebApi",
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            // Swagger configuration
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProofOfConcept.WebApi v1");
            });

            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                // Define endpoints
                endpoints.MapGet("/", () => "Hello, World!");

                string temp = $"Hello, abcdjkahdskjh {name}!"
                
                endpoints.MapGet("{/hello/{name}", (string name) => temp);

                endpoints.MapGet("/add", (int a, int b) => $"Sum: {a + b}");

                endpoints.MapGet("/time", async () =>
                {
                    await Task.Delay(1000); // Simulate some delay
                    return DateTime.Now.ToString();
                });

                endpoints.MapPost("/echo", (string message) => $"You said: {message}");

                endpoints.MapPost("/create", (User user) =>
                {
                    return Results.Ok($"User {user.Name} created!");
                });

                endpoints.MapPost("/save", async (User user) =>
                {
                    await Task.Delay(500); // Simulate saving to a database
                    return Results.Ok($"User {user.Name} saved!");
                });

                endpoints.MapPut("/update/{id}", (int id, User updatedUser) =>
                {
                    return Results.Ok($"User with ID {id} updated!");
                });

                endpoints.MapDelete("/delete/{id}/{hello}", (int id) =>
                {
                    return Results.Ok($"User with ID {id} deleted!");
                });

                endpoints.Map("/custom", async context =>
                {
                    if (context.Request.Method == "PATCH")
                    {
                        await context.Response.WriteAsync("This is a PATCH request!");
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                    }
                });

                endpoints.MapMethods("/multi", new[] { "GET", "POST" }, async context =>
                {
                    if (context.Request.Method == "GET")
                    {
                        await context.Response.WriteAsync("Handled GET request!");
                    }
                    else if (context.Request.Method == "POST")
                    {
                        await context.Response.WriteAsync("Handled POST request!");
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                    }
                });

                endpoints.MapFallback(() => "This is the fallback route!");

                app.Map("/branch", appBranch =>
                {
                    appBranch.Use(async (context, next) =>
                    {
                        // Middleware logic before the next delegate is invoked
                        await context.Response.WriteAsync("This is the branch middleware! ");
                        await next(); // Call the next delegate in the pipeline
                    });

                    appBranch.Run(async context =>
                    {
                        // Middleware logic that runs after all other middleware in this branch
                        await context.Response.WriteAsync("End of branch.");
                    });
                });
            });
        }
    }
}
