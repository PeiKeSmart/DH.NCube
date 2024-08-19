﻿using System.ComponentModel;
using NewLife.Web;
using XCode.Membership;

namespace NewLife.Cube.Areas.Admin.Controllers;

/// <summary>租户管理</summary>
[AdminArea]
[Menu(75, true, Icon = "fa-user-circle", Mode = MenuModes.Admin | MenuModes.Tenant)]
public class TenantController : EntityController<Tenant>
{
    static TenantController()
    {
        LogOnChange = true;

        //ListFields.RemoveField("Secret", "Logo", "AuthUrl", "AccessUrl", "UserUrl", "Remark");
        ListFields.RemoveField("ID", "Remark")
            .RemoveField("CreateUserId", "CreateTime", "CreateIP", "UpdateUserId", "UpdateTime", "UpdateIP");

        {
            var df = ListFields.AddListField("Users", null, "ManagerName");
            df.DisplayName = "用户";
            df.Url = "/Admin/TenantUser?tenantId={Id}";
        }

        {
            var df = AddFormFields.AddDataField("RoleIds", "RoleNames");
            df.DataSource = entity => Role.FindAllWithCache().OrderByDescending(e => e.Sort).ToDictionary(e => e.ID, e => e.Name);
            AddFormFields.RemoveField("RoleNames");
        }
        {
            var df = EditFormFields.AddDataField("RoleIds", "RoleNames");
            df.DataSource = entity => Role.FindAllWithCache().OrderByDescending(e => e.Sort).ToDictionary(e => e.ID, e => e.Name);
            EditFormFields.RemoveField("RoleNames");
        }

        {
            AddFormFields.GroupVisible = (entity, group) => (entity as Tenant).Id == 0 && group != "扩展";
        }
    }

    /// <summary>搜索数据集</summary>
    /// <param name="p"></param>
    /// <returns></returns>
    protected override IEnumerable<Tenant> Search(Pager p)
    {
        var id = p["id"].ToInt(-1);
        if (id > 0)
        {
            var entity = Tenant.FindById(id);
            if (entity != null) return new[] { entity };
        }

        if (TenantContext.CurrentId > 0) PageSetting.EnableAdd = false;

        var managerId = p["managerId"].ToInt(-1);
        //var roleIds = p["roleIds"].SplitAsInt();
        var enable = p["enable"]?.ToBoolean();
        var start = p["dtStart"].ToDateTime();
        var end = p["dtEnd"].ToDateTime();

        return Tenant.Search(null, managerId, enable, start, end, p["q"], p);
    }

    /// <summary>验证数据</summary>
    /// <param name="entity"></param>
    /// <param name="type"></param>
    /// <param name="post"></param>
    /// <returns></returns>
    protected override Boolean Valid(Tenant entity, DataObjectMethodType type, Boolean post)
    {
        if (/*!post &&*/ type == DataObjectMethodType.Insert)
        {
            if (entity.ManagerId == 0) entity.ManagerId = ManageProvider.Provider.Current.ID;
        }

        return base.Valid(entity, type, post);
    }

    /// <summary></summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected override Int32 OnInsert(Tenant entity)
    {
        var result = base.OnInsert(entity);

        var tuEntity = TenantUser.FindByTenantIdAndUserId(entity.Id, entity.ManagerId);
        tuEntity ??= new TenantUser()
        {
            TenantId = entity.Id,
            UserId = entity.ManagerId
        };

        tuEntity.Enable = true;
        tuEntity.RoleIds = entity.RoleIds;

        tuEntity.Save();

        return result;
    }

    /// <summary></summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected override Int32 OnUpdate(Tenant entity)
    {
        var oldTenantEntity = Tenant.FindById(entity.Id);
        var tuEntity = TenantUser.FindByTenantIdAndUserId(oldTenantEntity.Id, oldTenantEntity.ManagerId);

        if (entity.ManagerId != oldTenantEntity.ManagerId)
        {
            tuEntity.Enable = false;
            tuEntity.Save();
        }

        var newTuEntity = TenantUser.FindByTenantIdAndUserId(entity.Id, entity.ManagerId);
        newTuEntity ??= new TenantUser()
        {
            TenantId = entity.Id,
            UserId = entity.ManagerId
        };

        newTuEntity.Enable = entity.Enable;
        newTuEntity.RoleIds = entity.RoleIds;

        newTuEntity.Save();

        return base.OnUpdate(entity);
    }
}