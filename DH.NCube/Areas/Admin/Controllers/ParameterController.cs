﻿using System.ComponentModel;
using XCode.Membership;

namespace NewLife.Cube.Areas.Admin.Controllers;

/// <summary>字典参数</summary>
[DisplayName("字典参数")]
[AdminArea]
[Menu(30, false, Icon = "fa-wrench")]
public class ParameterController : EntityController<Parameter, ParameterModel>
{
    static ParameterController()
    {
        LogOnChange = true;

        ListFields.RemoveField("Ex1", "Ex2", "Ex3", "Ex4", "Ex5", "Ex6", "UpdateUserID", "UpdateIP");
        ListFields.RemoveCreateField();
    }
}