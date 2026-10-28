using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;
using SolicitudesApi.Authorization;
using SolicitudesApi.Data;
using SolicitudesApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

if (builder.Environment.IsDevelopment())
{
    builder.Services.Configure<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>(
        JwtBearerDefaults.AuthenticationScheme,
        opts =>
        {
            opts.IncludeErrorDetails = true;
            opts.Events ??= new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents();
            var prev = opts.Events.OnAuthenticationFailed;
            opts.Events.OnAuthenticationFailed = async ctx =>
            {
                Console.WriteLine($"[JWT FAIL] {ctx.Exception.GetType().Name}: {ctx.Exception.Message}");
                if (prev is not null) await prev(ctx);
            };
            var prevChallenge = opts.Events.OnChallenge;
            opts.Events.OnChallenge = async ctx =>
            {
                Console.WriteLine($"[JWT CHALLENGE] error='{ctx.Error}' description='{ctx.ErrorDescription}'");
                if (prevChallenge is not null) await prevChallenge(ctx);
            };
        });
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppRoles.Administrador, p => p.RequireRole(AppRoles.Administrador));
    options.AddPolicy(AppRoles.Gestor, p => p.RequireRole(AppRoles.Gestor, AppRoles.Administrador));
    options.AddPolicy(AppRoles.AsignadorEjecutor, p => p.RequireRole(AppRoles.AsignadorEjecutor, AppRoles.Administrador));
    options.AddPolicy(AppRoles.Ejecutor, p => p.RequireRole(AppRoles.Ejecutor, AppRoles.Administrador));
    options.AddPolicy(AppRoles.Usuario, p => p.RequireRole(
        AppRoles.Usuario, AppRoles.Ejecutor, AppRoles.AsignadorEjecutor, AppRoles.Gestor, AppRoles.Administrador));
});

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddHttpContextAccessor();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        opt.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Solicitudes API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pega aquí un access token JWT de Entra ID (sin el prefijo 'Bearer ')."
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex,
            "No se pudieron aplicar migraciones. La API seguirá iniciando " +
            "para que puedas ver Swagger. Revisa la cadena de conexión y los permisos RBAC en SQL.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex,
            "Excepción no manejada en {Method} {Path}", context.Request.Method, context.Request.Path);
        throw;
    }
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseCors();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger")).AllowAnonymous();

app.MapControllers();

app.Logger.LogInformation("API arrancando. CORS permite: {origins}",
    app.Environment.IsDevelopment() ? "* (dev)" : string.Join(", ", allowedOrigins));

app.Run();
