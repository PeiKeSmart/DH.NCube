﻿using System.Web.Script.Serialization;
using NewLife.Collections;
using NewLife.Data;

namespace NewLife.Cube.Charts;

/// <summary>文字样式</summary>
public class TextStyle : IExtend
{
    /// <summary>
    /// 颜色
    /// </summary>
    public String Color { get; set; }

    /// <summary>
    /// 默认值：'none' 
    /// 修饰，仅对tooltip.textStyle生效 
    /// </summary>
    public String Decoration { get; set; }

    /// <summary>
    /// 默认值：各异 
    /// 水平对齐方式，可选为：'left' | 'right' | 'center' 
    /// </summary>
    public String Align { get; set; }

    /// <summary>
    /// 默认值：各异
    /// 垂直对齐方式，可选为：'top' | 'bottom' | 'middle' 
    /// </summary>
    public String Baseline { get; set; }

    /// <summary>
    /// 默认值：'Arial, Verdana, sans-serif'
    /// 字体系列  
    /// </summary>
    public String FontFamily { get; set; }

    /// <summary>
    /// 默认值：12
    /// 字号，单位px  
    /// </summary>
    public Int32 FontSize { get; set; }

    /// <summary>
    /// 字体系列  
    /// </summary>
    public String FontStyle { get; set; }

    /// <summary>
    /// 粗细，可选为：'normal' | 'bold' | 'bolder' | 'lighter' | 100 | 200 |... | 900  
    /// </summary>
    public String FontWeight { get; set; }

    /// <summary>扩展字典</summary>
    [ScriptIgnore]
    public IDictionary<String, Object> Items { get; set; } = new NullableDictionary<String, Object>(StringComparer.OrdinalIgnoreCase);

    /// <summary>扩展数据</summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Object this[String key] { get => Items[key]; set => Items[key] = value; }
}