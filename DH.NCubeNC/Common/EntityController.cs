﻿using System.ComponentModel;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NewLife.Cube.Common;
using NewLife.Cube.Entity;
using NewLife.Cube.Extensions;
using NewLife.Cube.ViewModels;
using NewLife.Data;
using NewLife.Log;
using NewLife.Reflection;
using NewLife.Remoting;
using NewLife.Serialization;
using NewLife.Web;
using XCode;
using XCode.Configuration;
using XCode.Membership;

namespace NewLife.Cube;

/// <summary>实体控制器基类</summary>
/// <typeparam name="TEntity"></typeparam>
public class EntityController<TEntity> : EntityController<TEntity, TEntity> where TEntity : Entity<TEntity>, new() { }

/// <summary>实体控制器基类</summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TModel"></typeparam>
public class EntityController<TEntity, TModel> : ReadOnlyEntityController<TEntity> where TEntity : Entity<TEntity>, new()
{
    #region 属性
    private String CacheKey => $"CubeView_{typeof(TEntity).FullName}";
    #endregion

    #region 构造
    /// <summary>实例化</summary>
    public EntityController() => PageSetting.IsReadOnly = false;
    #endregion

    #region 默认Action
    /// <summary>删除</summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Delete)]
    [DisplayName("删除{type}")]
    public virtual ActionResult Delete(String id)
    {
        var url = Request.GetReferer();

        var act = "删除";
        var entity = FindData(id);
        var rs = false;
        var err = "";
        try
        {
            // 假删除与还原
            var fi = GetDeleteField();
            if (fi != null)
            {
                var restore = GetRequest("restore").ToBoolean();
                entity.SetItem(fi.Name, !restore);
                if (restore) act = "恢复";

                if (Valid(entity, DataObjectMethodType.Update, true))
                    OnUpdate(entity);
                else
                    err = "验证失败";
            }
            else
            {
                if (Valid(entity, DataObjectMethodType.Delete, true))
                    OnDelete(entity);
                else
                    err = "验证失败";
            }

            rs = true;
        }
        catch (Exception ex)
        {
            err = ex.GetTrue().Message;
            WriteLog("Delete", false, err);
            //if (LogOnChange) LogProvider.Provider.WriteLog("Delete", entity, err);

            if (Request.IsAjaxRequest())
                return JsonRefresh($"{act}失败！{err}");

            throw;
        }

        if (Request.IsAjaxRequest())
            return JsonRefresh(rs ? $"{act}成功！" : $"{act}失败！{err}");
        else if (!url.IsNullOrEmpty())
            return Redirect(url);
        else
            return RedirectToAction("Index");
    }

    private static FieldItem GetDeleteField() => Factory.Fields.FirstOrDefault(e => e.Name.EqualIgnoreCase("Deleted", "IsDelete", "IsDeleted") && e.Type == typeof(Boolean));

    /// <summary>表单，添加/修改</summary>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Insert)]
    [DisplayName("添加{type}")]
    public virtual ActionResult Add()
    {
        var entity = Factory.Create(true) as TEntity;

        // 填充QueryString参数
        var qs = Request.Query;
        foreach (var item in Entity<TEntity>.Meta.Fields)
        {
            var v = qs[item.Name];
            if (v.Count > 0) entity[item.Name] = v[0];
        }

        // 验证数据权限
        Valid(entity, DataObjectMethodType.Insert, false);

        // 记下添加前的来源页，待会添加成功以后跳转
        // 如果列表页有查询条件，优先使用
        var key = $"Cube_Add_{typeof(TEntity).FullName}";
        if (Session[CacheKey] is Pager p)
        {
            var sb = p.GetBaseUrl(true, true, true);
            if (sb.Length > 0)
                Session[key] = "Index?" + sb;
            else
                Session[key] = Request.GetReferer();
        }
        else
            Session[key] = Request.GetReferer();

        // 用于显示的列
        ViewBag.Fields = OnGetFields(ViewKinds.AddForm, entity);

        return View("AddForm", entity);
    }

    /// <summary>保存</summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Insert)]
    [HttpPost]
    public virtual async Task<ActionResult> Add(TModel model)
    {
        // 实例化实体对象，然后拷贝
        if (model is not TEntity entity)
        {
            entity = Factory.Create(true) as TEntity;

            if (model is IModel src)
                entity.CopyFrom(src, true);
            else
                entity.Copy(model);
        }

        // 检测避免乱用Add/id
        if (Factory.Unique.IsIdentity && entity[Factory.Unique.Name].ToInt() != 0)
            throw new Exception("我们约定添加数据时路由id部分默认没有数据，以免模型绑定器错误识别！");

        var rs = false;
        var err = "";
        try
        {
            if (Valid(entity, DataObjectMethodType.Insert, true))
            {
                //SaveFiles(entity);

                OnInsert(entity);

                var fs = await SaveFiles(entity);
                if (fs.Count > 0) OnUpdate(entity);

                if (LogOnChange) LogProvider.Provider.WriteLog("Insert", entity);

                rs = true;
            }
            else
                err = "验证失败";
        }
        catch (Exception ex)
        {
            err = ex.Message;
            ModelState.AddModelError((ex as ArgumentException)?.ParamName ?? "", ex.Message);
        }

        if (!rs)
        {
            WriteLog("Add", false, err);

            ViewBag.StatusMessage = SysConfig.Develop ? ("添加失败！" + err) : "添加失败！";

            // 添加失败，ID清零，否则会显示保存按钮
            entity[Entity<TEntity>.Meta.Unique.Name] = 0;

            if (IsJsonRequest) return Json(500, ViewBag.StatusMessage);

            ViewBag.Fields = OnGetFields(ViewKinds.AddForm, entity);

            return View("AddForm", entity);
        }

        ViewBag.StatusMessage = "添加成功！";

        if (IsJsonRequest) return Json(0, ViewBag.StatusMessage);

        var key = $"Cube_Add_{typeof(TEntity).FullName}";
        var url = Session[key] as String;
        if (!url.IsNullOrEmpty()) return Redirect(url);

        // 新增完成跳到列表页，更新完成保持本页
        return RedirectToAction("Index");
    }

    /// <summary>表单，添加/修改</summary>
    /// <param name="id">主键。可能为空（表示添加），所以用字符串而不是整数</param>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Update)]
    [DisplayName("更新{type}")]
    public virtual ActionResult Edit(String id)
    {
        var entity = FindData(id);
        if (entity == null || (entity as IEntity).IsNullKey) throw new XException("要编辑的数据[{0}]不存在！", id);

        // 验证数据权限
        Valid(entity, DataObjectMethodType.Update, false);

        // 如果列表页有查询条件，优先使用
        var key = $"Cube_Edit_{typeof(TEntity).FullName}-{id}";
        if (Session[CacheKey] is Pager p)
        {
            var sb = p.GetBaseUrl(true, true, true);
            if (sb.Length > 0)
                Session[key] = "../Index?" + sb;
            else
                Session[key] = Request.GetReferer();
        }
        else
            Session[key] = Request.GetReferer();

        // Json输出
        if (IsJsonRequest) return Json(0, null, entity);

        ViewBag.Fields = OnGetFields(ViewKinds.EditForm, entity);

        return View("EditForm", entity);
    }

    /// <summary>保存</summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Update)]
    [HttpPost]
    public virtual async Task<ActionResult> Edit(TModel model)
    {
        // 实例化实体对象，然后拷贝
        if (model is not TEntity entity)
        {
            var uk = Factory.Unique;
            var key = model is IModel ext ? ext[uk.Name] : model.GetValue(uk.Name);

            // 先查出来，再拷贝。这里没有考虑脏数据的问题，有可能拷贝后并没有脏数据
            entity = FindData(key);

            if (model is IModel src)
                entity.CopyFrom(src, true);
            else
                entity.Copy(model, false, uk.Name);
        }

        var rs = false;
        var err = "";
        try
        {
            if (Valid(entity, DataObjectMethodType.Update, true))
            {
                await SaveFiles(entity);

                OnUpdate(entity);

                rs = true;
            }
            else
                err = "验证失败";
        }
        catch (Exception ex)
        {
            err = ex.Message;
            ModelState.AddModelError((ex as ArgumentException)?.ParamName ?? "", ex.Message);
        }

        Object id = null;
        if (Factory.Unique != null) id = entity[Factory.Unique.Name];

        if (!rs)
        {
            WriteLog("Edit", false, err);

            ViewBag.StatusMessage = SysConfig.Develop ? ("保存失败！" + err) : "保存失败！";

            if (IsJsonRequest) return Json(500, ViewBag.StatusMessage);
        }
        else
        {
            ViewBag.StatusMessage = "保存成功！";

            if (IsJsonRequest) return Json(0, ViewBag.StatusMessage);

            // 实体对象保存成功后直接重定向到列表页，减少用户操作提高操作体验
            var key = $"Cube_Edit_{typeof(TEntity).FullName}-{id}";
            var url = Session[key] as String;
            if (!url.IsNullOrEmpty()) return Redirect(url);
        }

        // 重新查找对象数据，以确保取得最新值
        if (id != null) entity = FindData(id);

        ViewBag.Fields = OnGetFields(ViewKinds.EditForm, entity);

        return View("EditForm", entity);
    }

    /// <summary>保存所有上传文件</summary>
    /// <param name="entity">实体对象</param>
    /// <param name="uploadPath">上传目录。为空时默认UploadPath配置</param>
    /// <returns></returns>
    protected virtual async Task<IList<String>> SaveFiles(TEntity entity, String uploadPath = null)
    {
        var rs = new List<String>();
        var list = new List<String>();

        if (!Request.HasFormContentType) return list;

        var files = Request.Form.Files;
        var fields = Factory.Fields;
        foreach (var fi in fields)
        {
            var dc = fi.Field;
            if (dc.IsAttachment())
            {
                // 允许一次性上传多个文件到服务端
                foreach (var file in files)
                {
                    if (file.Name.EqualIgnoreCase(fi.Name, fi.Name + "_attachment"))
                    {
                        var att = await SaveFile(entity, file, uploadPath, null);
                        if (att != null)
                        {
                            var url = ViewHelper.GetAttachmentUrl(att);
                            list.Add(url);
                            rs.Add(url);
                        }
                    }
                }

                if (list.Count > 0)
                {
                    entity.SetItem(fi.Name, list.Join(";"));
                    list.Clear();
                }
            }
        }

        return rs;
    }

    /// <summary>保存单个文件</summary>
    /// <param name="entity">实体对象</param>
    /// <param name="file">文件</param>
    /// <param name="uploadPath">上传目录，默认使用UploadPath配置</param>
    /// <param name="fileName">文件名，如若指定则忽略前面的目录</param>
    /// <returns></returns>
    protected virtual async Task<Attachment> SaveFile(TEntity entity, IFormFile file, String uploadPath, String fileName)
    {
        if (fileName.IsNullOrEmpty()) fileName = file.FileName;

        using var span = DefaultTracer.Instance?.NewSpan(nameof(SaveFile), new { name = file.Name, fileName, uploadPath });

        var id = Factory.Unique != null ? entity[Factory.Unique] : null;
        var att = new Attachment
        {
            Category = typeof(TEntity).Name,
            Key = id + "",
            Title = entity + "",
            //FileName = fileName ?? file.FileName,
            ContentType = file.ContentType,
            Size = file.Length,
            Enable = true,
            UploadTime = DateTime.Now,
        };

        if (id != null)
        {
            var ss = GetControllerAction();
            att.Url = $"/{ss[0]}/{ss[1]}/Detail/{id}";
        }

        var rs = false;
        var msg = "";
        try
        {
            rs = await att.SaveFile(file.OpenReadStream(), uploadPath, fileName);
        }
        catch (Exception ex)
        {
            rs = false;
            msg = ex.Message;
            span?.SetError(ex, att);

            throw;
        }
        finally
        {
            // 写日志
            var type = entity.GetType();
            var log = LogProvider.Provider.CreateLog(type, "上传", rs, $"上传 {file.FileName} ，目录 {uploadPath} ，保存为 {att.FilePath} " + msg, 0, null, UserHost);
            log.LinkID = id.ToLong();
            log.SaveAsync();
        }

        return att;
    }

    /// <summary>批量启用</summary>
    /// <param name="keys">主键集合</param>
    /// <param name="reason">操作原因</param>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Update)]
    public virtual ActionResult EnableSelect(String keys, String reason) => EnableOrDisableSelect(true, reason);

    /// <summary>批量禁用</summary>
    /// <param name="keys">主键集合</param>
    /// <param name="reason">操作原因</param>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Update)]
    public virtual ActionResult DisableSelect(String keys, String reason) => EnableOrDisableSelect(false, reason);

    /// <summary>
    /// 批量启用或禁用
    /// </summary>
    /// <param name="isEnable">启用/禁用</param>
    /// <param name="reason">操作原因</param>
    /// <returns></returns>
    protected virtual ActionResult EnableOrDisableSelect(Boolean isEnable, String reason)
    {
        var count = 0;
        var ids = GetRequest("keys").SplitAsInt();
        var fields = Factory.AllFields;
        if (ids.Length > 0 && fields.Any(f => f.Name.EqualIgnoreCase("enable")))
        {
            var log = LogProvider.Provider;
            foreach (var id in ids)
            {
                var entity = Factory.Find("ID", id);
                if (entity != null && entity["Enable"].ToBoolean() != isEnable)
                {
                    entity.SetItem("Enable", isEnable);

                    log.WriteLog("Update", entity);
                    log.WriteLog(entity.GetType(), isEnable ? "Enable" : "Disable", true, reason);

                    entity.Update();

                    Interlocked.Increment(ref count);
                }
            }
        }

        return JsonRefresh($"共{(isEnable ? "启用" : "禁用")}[{count}]个");
    }
    #endregion

    #region 高级Action
    /// <summary>导入Xml</summary>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Insert)]
    [DisplayName("导入")]
    [HttpPost]
    public virtual ActionResult ImportXml() => throw new NotImplementedException();

    /// <summary>导入Json</summary>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Insert)]
    [DisplayName("导入")]
    [HttpPost]
    public virtual ActionResult ImportJson() => throw new NotImplementedException();

    /// <summary>导入Excel</summary>
    /// 当前采用前端解析的excel，表头第一行数据无效，从第二行开始处理
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Insert)]
    [DisplayName("导入Excel")]
    public virtual ActionResult ImportExcel(String data)
    {
        if (String.IsNullOrWhiteSpace(data)) return Json(500, null, $"“{nameof(data)}”不能为 null 或空白。");
        try
        {
            var fact = Factory;
            var dal = fact.Session.Dal;
            var type = Activator.CreateInstance(fact.EntityType);
            var json = new JsonParser(data);
            var dataList = json.Decode() as IList<Object>;


            //解析json
            //var dataList = JArray.Parse(data);
            var errorString = String.Empty;
            Int32 okSum = 0, fiSum = 0;

            //using var tran = Entity<TEntity>.Meta.CreateTrans();
            foreach (var itemD in dataList)
            {
                var item = itemD.ToDictionary();
                if (item[fact.Fields[1].Name].ToString() == fact.Fields[1].DisplayName) //判断首行是否为标体列
                {
                    continue;
                }
                else
                {
                    //检查主字段是否重复
                    if (Entity<TEntity>.Find(fact.Master.Name, item[fact.Master.Name].ToString()) == null)
                    {
                        //var entity = item.ToJson().ToJsonEntity(fact.EntityType);
                        var entity = fact.Create();

                        foreach (var fieldsItem in fact.Fields)
                        {
                            if (!item.ContainsKey(fieldsItem.Name))
                            {
                                if (!fieldsItem.IsNullable)
                                    fieldsItem.FromExcelToEntity(item, entity);

                                continue;
                            }

                            fieldsItem.FromExcelToEntity(item, entity);
                        }

                        if (fact.FieldNames.Contains("CreateTime"))
                            entity["CreateTime"] = DateTime.Now;

                        if (fact.FieldNames.Contains("CreateIP"))
                            entity["CreateIP"] = "--";

                        okSum += fact.Session.Insert(entity);
                    }
                    else
                    {
                        errorString += $"<br>{item[fact.Master.Name]}重复";
                        fiSum++;
                    }
                }
            }

            //tran.Commit();

            WriteLog("导入Excel", true, $"导入Excel[{data}]（{dataList.Count()}行）成功！");

            return Json(0, $"导入成功:({okSum}行)，失败({fiSum}行)！{errorString}");
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);

            WriteLog("导入Excel", false, ex.GetMessage());

            return Json(500, ex.GetMessage(), ex);
        }
    }


    /// <summary>修改bool值</summary>
    /// <param name="id"></param>
    /// <param name="valName"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Update)]
    public ActionResult SetBool(Int32 id = 0, String valName = "", Boolean val = true)
    {
        var fi = Factory.Fields.FirstOrDefault(e => e.Name.EqualIgnoreCase(valName));
        if (fi == null) throw new InvalidOperationException($"未找到{valName}字段。");

        var rs = 0;
        if (id > 0)
        {
            var entity = FindData(id);
            if (entity == null) throw new ArgumentNullException(nameof(id), "找不到任务 " + id);

            //entity.Enable = enable;
            entity.SetItem(fi.Name, val);
            if (Valid(entity, DataObjectMethodType.Update, true))
                rs += OnUpdate(entity);
        }
        else
        {
            var ids = GetRequest("keys").SplitAsInt();

            foreach (var item in ids)
            {
                var entity = FindData(item);
                if (entity != null)
                {
                    //entity.Enable = enable;
                    entity.SetItem(fi.Name, val);
                    if (Valid(entity, DataObjectMethodType.Update, true))
                        rs += OnUpdate(entity);
                }
            }
        }
        return JsonRefresh($"操作成功！共更新[{rs}]行！");
    }

    /// <summary>启用 或 禁用</summary>
    /// <param name="id"></param>
    /// <param name="enable"></param>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Update)]
    public virtual ActionResult SetEnable(Int64 id = 0, Boolean enable = true)
    {
        var fi = Factory.Fields.FirstOrDefault(e => e.Name.EqualIgnoreCase("Enable"));
        if (fi == null) throw new InvalidOperationException($"启用/禁用仅支持Enable字段。");

        var rs = 0;
        if (id > 0)
        {
            var entity = FindData(id);
            if (entity == null) throw new ArgumentNullException(nameof(id), "找不到任务 " + id);

            //entity.Enable = enable;
            entity.SetItem(fi.Name, enable);
            if (Valid(entity, DataObjectMethodType.Update, true))
                rs += OnUpdate(entity);
        }
        else
        {
            var ids = GetRequest("keys").SplitAsInt();

            foreach (var item in ids)
            {
                var entity = FindData(item);
                if (entity != null)
                {
                    //entity.Enable = enable;
                    entity.SetItem(fi.Name, enable);
                    if (Valid(entity, DataObjectMethodType.Update, true))
                        rs += OnUpdate(entity);
                }
            }
        }
        return JsonRefresh($"操作成功！共更新[{rs}]行！");
    }
    #endregion

    #region 批量删除
    /// <summary>删除选中</summary>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Delete)]
    [DisplayName("删除选中")]
    public virtual ActionResult DeleteSelect()
    {
        var total = 0;
        var success = 0;
        var keys = SelectKeys;
        if (keys != null && keys.Length > 0)
        {
            // 假删除
            var fi = GetDeleteField();

            using var tran = Entity<TEntity>.Meta.CreateTrans();
            var list = new List<IEntity>();
            foreach (var item in keys)
            {
                var entity = Entity<TEntity>.FindByKey(item);
                if (entity != null)
                {
                    // 验证数据权限
                    if (fi != null)
                    {
                        entity.SetItem(fi.Name, true);
                        if (Valid(entity, DataObjectMethodType.Update, true)) list.Add(entity);
                    }
                    else
                    {
                        if (Valid(entity, DataObjectMethodType.Delete, true)) list.Add(entity);
                    }
                }
            }

            total = list.Count;
            if (fi != null)
                success = list.Update();
            else
                success = list.Delete();

            tran.Commit();
        }

        return JsonRefresh($"共删除{total}行数据，成功{success}行");
    }

    /// <summary>删除全部</summary>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Delete)]
    [DisplayName("删除全部")]
    public virtual ActionResult DeleteAll()
    {
        var url = Request.GetReferer();

        // 假删除
        var fi = GetDeleteField();

        var total = 0;
        var success = 0;
        var p = Session[CacheKey] as Pager;
        p = new Pager(p);
        if (p != null)
        {
            // 循环多次删除
            for (var i = 0; i < 10; i++)
            {
                p.PageIndex = i + 1;
                p.PageSize = 100_000;
                // 不要查记录数
                p.RetrieveTotalCount = false;

                var data = SearchData(p).ToList();
                if (data.Count == 0) break;

                total += data.Count;

                using var tran = Entity<TEntity>.Meta.CreateTrans();
                var list = new List<IEntity>();
                foreach (var entity in data)
                {
                    // 验证数据权限
                    if (fi != null)
                    {
                        entity.SetItem(fi.Name, true);
                        if (Valid(entity, DataObjectMethodType.Update, true)) list.Add(entity);
                    }
                    else
                    {
                        if (Valid(entity, DataObjectMethodType.Delete, true)) list.Add(entity);
                    }
                }

                if (fi != null)
                    success += list.Update();
                else
                    success += list.Delete();
                tran.Commit();
            }
        }

        if (Request.IsAjaxRequest())
            return JsonRefresh($"共删除{total}行数据，成功{success}行");
        else if (!url.IsNullOrEmpty())
            return Redirect(url);
        else
            return RedirectToAction("Index");
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
    protected virtual Int32 OnUpdate(TEntity entity)
    {
        if (Request.HasFormContentType)
        {
            // 遍历表单字段，部分字段可能有扩展
            foreach (var item in EditFormFields)
            {
                if (item is FormField ef && ef.GetExpand != null)
                {
                    // 获取参数对象，展开参数，从表单字段接收参数
                    var p = ef.GetExpand(entity);
                    if (p != null && p is not String && !(entity as IEntity).Dirtys[ef.Name])
                    {
                        // 保存参数对象
                        if (FieldCollection.ReadForm(p, Request.Form, ef.Name + "_"))
                            entity.SetItem(ef.Name, p.ToJson(true));
                    }
                }
            }
        }

        return entity.Update();
    }

    /// <summary>删除实体对象</summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected virtual Int32 OnDelete(TEntity entity) => entity.Delete();
    #endregion

    #region 同步/还原
    /// <summary>同步数据</summary>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Insert)]
    [DisplayName("同步{type}")]
    public async Task<ActionResult> Sync()
    {
        //if (id.IsNullOrEmpty()) return RedirectToAction(nameof(Index));

        // 读取系统配置
        var ps = Parameter.FindAllByUserID(ManageProvider.User.ID); // UserID=0 && Category=Sync
        ps = ps.Where(e => e.Category == "Sync").ToList();
        var server = ps.FirstOrDefault(e => e.Name == "Server")?.Value;
        var token = ps.FirstOrDefault(e => e.Name == "Token")?.Value;
        var models = ps.FirstOrDefault(e => e.Name == "Models")?.Value;

        if (server.IsNullOrEmpty()) throw new ArgumentNullException("未配置 Sync:Server");
        if (token.IsNullOrEmpty()) throw new ArgumentNullException("未配置 Sync:Token");
        if (models.IsNullOrEmpty()) throw new ArgumentNullException("未配置 Sync:Models");

        var mds = models.Split(",");

        //// 创建实体工厂
        //var etype = mds.FirstOrDefault(e => e.Replace(".", "_") == id);
        //var fact = etype.GetTypeEx()?.AsFactory();
        //if (fact == null) throw new ArgumentNullException(nameof(id), "未找到模型 " + id);

        // 找到控制器，以识别动作地址
        var cs = GetControllerAction();
        var ctrl = cs[0].IsNullOrEmpty() ? cs[1] : $"{cs[0]}/{cs[1]}";
        if (!mds.Contains(ctrl)) throw new InvalidOperationException($"[{ctrl}]未配置为允许同步 Sync:Models");

        // 创建客户端，准备发起请求
        var url = server.EnsureEnd("/") + $"{ctrl}/Json/{token}?PageSize=100000";

        var http = new HttpClient
        {
            BaseAddress = new Uri(url)
        };

        var sw = Stopwatch.StartNew();

        var list = await http.InvokeAsync<TEntity[]>(HttpMethod.Get, null);

        sw.Stop();

        var fact = Factory;
        XTrace.WriteLine("[{0}]共同步数据[{1:n0}]行，耗时{2:n0}ms，数据源：{3}", fact.EntityType.FullName, list.Length, sw.ElapsedMilliseconds, url);

        var arrType = fact.EntityType.MakeArrayType();
        if (list.Length > 0)
        {
            XTrace.WriteLine("[{0}]准备覆盖写入[{1}]行数据", fact.EntityType.FullName, list.Length);
            using var tran = fact.Session.CreateTrans();

            // 清空
            try
            {
                fact.Session.Truncate();
            }
            catch (Exception ex) { XTrace.WriteException(ex); }

            // 插入
            //ms.All(e => { e.AllChilds = new List<Menu>(); return true; });
            fact.AllowInsertIdentity = true;
            //ms.Insert();
            //var empty = typeof(List<>).MakeGenericType(fact.EntityType).CreateInstance();
            foreach (IEntity entity in list)
            {
                if (entity is IEntityTree tree) tree.AllChilds.Clear();

                entity.Insert();
            }
            fact.AllowInsertIdentity = false;

            tran.Commit();
        }

        return Index();
    }
    #endregion
}