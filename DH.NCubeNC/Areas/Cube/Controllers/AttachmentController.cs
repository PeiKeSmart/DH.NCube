﻿using Microsoft.AspNetCore.Mvc;
using NewLife.Cube.Entity;
using NewLife.Cube.Extensions;
using NewLife.Cube.ViewModels;
using NewLife.Web;
using XCode;
using XCode.Membership;

namespace NewLife.Cube.Areas.Cube.Controllers;

/// <summary>附件管理</summary>
[CubeArea]
[Menu(38, true, Icon = "fa-file-text")]
public class AttachmentController : EntityController<Attachment, AttachmentModel>
{
    static AttachmentController()
    {
        ListFields.RemoveField("Id", "Hash", "Url", "Source", "UpdateUserID", "UpdateIP", "Remark");
        ListFields.RemoveCreateField();

        {
            var df = ListFields.GetField("Category") as ListField;
            df.Url = "/Cube/Attachment?category={Category}";
        }
        {
            var df = ListFields.GetField("Key") as ListField;
            df.Url = "/Cube/Attachment?category={Category}&key={Key}";
        }
        {
            var df = ListFields.GetField("Extension") as ListField;
            df.Url = "/Cube/Attachment?ext={Extension}";
        }

        {
            var df = ListFields.AddListField("Info", null, "Title");
            df.DisplayName = "信息页";
            df.Url = "{Url}";
            df.DataVisible = e => !(e as Attachment).Url.IsNullOrEmpty();
            df.Target = "_blank";
        }

        {
            var df = ListFields.AddListField("down", null, "Title");
            df.DisplayName = "下载";
            df.Url = "/cube/file/{Id}{Extension}";
            df.Target = "_blank";
        }

        ListFields.TraceUrl();
    }

    /// <summary>搜索</summary>
    /// <param name="p"></param>
    /// <returns></returns>
    protected override IEnumerable<Attachment> Search(Pager p)
    {
        var category = p["category"];
        var key = p["key"];
        var ext = p["ext"];

        var start = p["dtStart"].ToDateTime();
        var end = p["dtEnd"].ToDateTime();

        if (p.Sort.IsNullOrEmpty()) p.Sort = AppLog._.Id.Desc();

        //return Attachment.Search(category, key, ext, start, end, p["Q"], p);
        return base.Search(p);
    }
}