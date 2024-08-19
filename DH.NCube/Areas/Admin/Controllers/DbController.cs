﻿using System.ComponentModel;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NewLife.Cube.Areas.Admin.Models;
using NewLife.Reflection;
using XCode;
using XCode.DataAccessLayer;
using XCode.Membership;

namespace NewLife.Cube.Areas.Admin.Controllers;

/// <summary>数据库管理</summary>
[DisplayName("数据库")]
[EntityAuthorize(PermissionFlags.Detail)]
[AdminArea]
[Menu(26, true, Icon = "fa-database")]
public class DbController : ControllerBaseX
{
    /// <summary>数据库列表</summary>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Detail)]
    [HttpGet("/[area]/[controller]")]
    public ActionResult Index()
    {
        var list = new List<DbItem>();
        var dir = NewLife.Setting.Current.BackupPath.GetBasePath().AsDirectory();

        // 读取配置文件
        foreach (var item in DAL.ConnStrs.ToArray())
        {
            var di = new DbItem
            {
                Name = item.Key,
                ConnStr = item.Value
            };

            var dal = DAL.Create(item.Key);
            di.Type = dal.DbType;

            var t = Task.Run(() =>
            {
                try
                {
                    return dal.Db.ServerVersion;
                }
                catch { return null; }
            });
            if (t.Wait(300)) di.Version = t.Result;

            if (dir.Exists) di.Backups = dir.GetFiles($"{dal.ConnName}_*", SearchOption.TopDirectoryOnly).Length;

            list.Add(di);
        }

        return Json(0, null, list);
    }

    /// <summary>备份数据库</summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Insert)]
    [HttpPost]
    public ActionResult Backup(String name)
    {
        var sw = Stopwatch.StartNew();

        var dal = DAL.Create(name);
        //var bak = dal.Db.CreateMetaData().SetSchema(DDLSchema.BackupDatabase, dal.ConnName, null, false);
        var bak = dal.Db.CreateMetaData().Invoke("Backup", dal.ConnName, null, false);

        sw.Stop();
        WriteLog("备份", true, $"备份数据库 {name} 到 {bak}，耗时 {sw.Elapsed}");

        return Index();
    }

    /// <summary>备份并压缩数据库</summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Insert)]
    [HttpPost]
    public ActionResult BackupAndCompress(String name)
    {
        var sw = Stopwatch.StartNew();

        var dal = DAL.Create(name);
        //var bak = dal.Db.CreateMetaData().SetSchema(DDLSchema.BackupDatabase, dal.ConnName, null, true);
        //var bak = dal.Db.CreateMetaData().Invoke("Backup", dal.ConnName, null, true);
        var bak = $"{name}_{DateTime.Now:yyyyMMddHHmmss}.zip";
        bak = NewLife.Setting.Current.BackupPath.CombinePath(bak);
        //var tables = dal.Tables;
        var tables = EntityFactory.GetTables(name, false);
        dal.BackupAll(tables, bak);

        sw.Stop();
        WriteLog("备份", true, $"备份数据库 {name} 并压缩到 {bak}，耗时 {sw.Elapsed}");

        return Index();
    }

    /// <summary>下载数据库备份</summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Detail)]
    [HttpGet]
    public ActionResult Download(String name)
    {
        var dal = DAL.Create(name);
        var xml = DAL.Export(dal.Tables);

        WriteLog("下载", true, "下载数据库架构 " + name);

        return File(xml.GetBytes(), "application/xml", name + ".xml");
    }
}