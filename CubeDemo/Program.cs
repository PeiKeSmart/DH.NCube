﻿using CubeDemo;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using NewLife;
using NewLife.Cube;
using NewLife.Cube.WebMiddleware;
using NewLife.Log;
using Swashbuckle.AspNetCore.SwaggerGen;

XTrace.UseConsole();

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// 引入星尘，设置监控中间件
var star = services.AddStardust(null);
TracerMiddleware.Tracer = star?.Tracer;

//services.AddHttpContextAccessor();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerConfigureOptions>();
builder.Services.AddSwaggerGen(options =>
{
    // 解决 NewLife.Setting 与 XCode.Setting 冲突的问题
    options.CustomSchemaIds(type => type.FullName);
    options.IncludeXmlComments("NewLife.Cube.xml".GetFullPath());

    //options.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "第三代魔方", Description = "第三代魔方WebApi接口，用于前后端分离。" });
    //options.SwaggerDoc("Basic", new OpenApiInfo { Version = "basic", Title = "基础模块" });
    //options.SwaggerDoc("Admin", new OpenApiInfo { Version = "admin", Title = "系统管理" });
    //options.SwaggerDoc("Cube", new OpenApiInfo { Version = "cube", Title = "魔方管理" });

    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (apiDesc.ActionDescriptor is not ControllerActionDescriptor controller) return false;

        var groups = controller.ControllerTypeInfo.GetCustomAttributes(true).OfType<IApiDescriptionGroupNameProvider>().Select(e => e.GroupName).ToList();

        if (docName == "v1" && (groups == null || groups.Count == 0)) return true;

        return groups != null && groups.Any(e => e == docName);
    });
});

services.AddCube();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //app.UseSwaggerUI();
    app.UseSwaggerUI(options =>
    {
        //options.SwaggerEndpoint("/swagger/Basic/swagger.json", "Basic");
        //options.SwaggerEndpoint("/swagger/Admin/swagger.json", "Admin");
        //options.SwaggerEndpoint("/swagger/Cube/swagger.json", "Cube");
        //options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = String.Empty;
        var groups = app.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>().ApiDescriptionGroups.Items;
        foreach (var description in groups)
        {
            var group = description.GroupName;
            if(group.IsNullOrEmpty()) continue;
            options.SwaggerEndpoint($"/swagger/{group}/swagger.json", group);
        }
    });
}

app.UseCube(builder.Environment);

app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Index}/{action=Index}/{id?}");
app.MapControllers();

app.RegisterService("CubeDemo", null, builder.Environment.EnvironmentName, "/cube/info");

app.Run();
