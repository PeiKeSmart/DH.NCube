﻿using System.ComponentModel;

namespace NewLife.Cube.Areas.Admin;

/// <summary>权限管理区域注册</summary>
[DisplayName("系统管理")]
[Menu(-1, true, Icon = "fa-desktop", LastUpdate = "20240118")]
public class AdminArea : AreaBase
{
    /// <inheritdoc />
    public AdminArea() : base(nameof(AdminArea).TrimEnd("Area")) { }
}