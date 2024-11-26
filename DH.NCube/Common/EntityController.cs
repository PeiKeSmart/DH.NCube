﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using NewLife.Cube.Entity;
using NewLife.Data;
using NewLife.Log;
using NewLife.Reflection;
using NewLife.Remoting;
using XCode;
using XCode.Membership;

namespace NewLife.Cube;

/// <summary>实体控制器基类</summary>
/// <typeparam name="TEntity">实体类型</typeparam>
/// <typeparam name="TModel">数据模型，用于接口数据传输</typeparam>
public partial class EntityController<TEntity, TModel> : ReadOnlyEntityController<TEntity> where TEntity : Entity<TEntity>, new()
{
    #region 默认Action
    /// <summary>删除数据</summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Delete)]
    [DisplayName("删除{type}")]
    [HttpDelete("/[area]/[controller]")]
    public virtual ApiResponse<TEntity> Delete([Required] String id)
    {
        var act = "删除";
        var entity = FindData(id);
        try
        {
            act = ProcessDelete(entity);

            return new ApiResponse<TEntity>(0, $"{act}成功！", entity);
        }
        catch (Exception ex)
        {
            var code = ex is ApiException ae ? ae.Code : 500;
            var err = ex.GetTrue().Message;
            WriteLog("Delete", false, err);

            return new ApiResponse<TEntity>(code, $"{act}失败！" + err, entity);
        }
    }

    /// <summary>添加数据</summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [DisplayName("添加{type}")]
    [EntityAuthorize(PermissionFlags.Insert)]
    [HttpPost("/[area]/[controller]")]
    public virtual async Task<ApiResponse<TEntity>> Insert(TModel model)
    {
        // 实例化实体对象，然后拷贝
        if (model is TEntity entity) return await Insert(entity);

        entity = Factory.Create(false) as TEntity;

        if (model is IModel src)
            entity.CopyFrom(src, true);
        else
            entity.Copy(model);

        return await Insert(entity);
    }

    /// <summary>添加数据</summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    [NonAction]
    public virtual async Task<ApiResponse<TEntity>> Insert(TEntity entity)
    {
        // 检测避免乱用Add/id
        if (Factory.Unique.IsIdentity && entity[Factory.Unique.Name].ToInt() != 0)
            throw new Exception("我们约定添加数据时路由id部分默认没有数据，以免模型绑定器错误识别！");

        try
        {
            if (!Valid(entity, DataObjectMethodType.Insert, true))
                throw new Exception("验证失败");

            OnInsert(entity);

            // 先插入再保存附件，主要是为了在附件表关联业务对象主键
            var fs = await SaveFiles(entity);
            if (fs.Count > 0) OnUpdate(entity);

            if (LogOnChange) LogProvider.Provider.WriteLog("Insert", entity);

            return new ApiResponse<TEntity>(0, "添加成功！", entity);
        }
        catch (Exception ex)
        {
            var code = ex is ApiException ae ? ae.Code : 500;
            var msg = ex.Message;

            WriteLog("Add", false, msg);

            msg = SysConfig.Develop ? ("添加失败！" + msg) : "添加失败！";

            // 添加失败，ID清零，否则会显示保存按钮
            entity[Factory.Unique.Name] = 0;

            return new ApiResponse<TEntity>(code, msg, entity);
        }
    }

    /// <summary>更新数据</summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Update)]
    [DisplayName("更新{type}")]
    [HttpPut("/[area]/[controller]")]
    public virtual async Task<ApiResponse<TEntity>> Update(TModel model)
    {
        // 实例化实体对象，然后拷贝
        if (model is TEntity entity) return await Update(entity);

        var uk = Factory.Unique;
        var key = model is IModel ext ? ext[uk.Name] : model.GetValue(uk.Name);

        // 先查出来，再拷贝。这里没有考虑脏数据的问题，有可能拷贝后并没有脏数据
        entity = FindData(key);

        if (model is IModel src)
            entity.CopyFrom(src, true);
        else
            entity.Copy(model, false, uk.Name);

        return await Update(entity);
    }

    /// <summary>更新数据</summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    [NonAction]
    public virtual async Task<ApiResponse<TEntity>> Update(TEntity entity)
    {
        try
        {
            if (!Valid(entity, DataObjectMethodType.Update, true))
                throw new Exception("验证失败");

            await SaveFiles(entity);

            OnUpdate(entity);

            return new ApiResponse<TEntity>(0, "保存成功！", entity);
        }
        catch (Exception ex)
        {
            var code = ex is ApiException ae ? ae.Code : 500;
            var err = ex.Message;
            ModelState.AddModelError((ex as ArgumentException)?.ParamName ?? "", ex.Message);

            WriteLog("Edit", false, err);

            err = SysConfig.Develop ? ("保存失败！" + err) : "保存失败！";

            return new ApiResponse<TEntity>(code, err, null);
        }
    }
    #endregion

    #region 实体操作重载
    /// <summary>添加实体对象</summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected virtual Int32 OnInsert(TEntity entity) => entity.Insert();

    /// <summary>更新实体对象</summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected virtual Int32 OnUpdate(TEntity entity) => entity.Update();

    /// <summary>删除实体对象</summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected virtual Int32 OnDelete(TEntity entity) => entity.Delete();
    #endregion
}