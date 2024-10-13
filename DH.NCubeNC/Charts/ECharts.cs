﻿using System.Web.Script.Serialization;
using NewLife.Collections;
using NewLife.Cube.ViewModels;
using NewLife.Data;
using NewLife.Security;
using NewLife.Serialization;
using XCode.Configuration;

namespace NewLife.Cube.Charts;

/// <summary>ECharts实例</summary>
public class ECharts : IExtend
{
    #region 属性
    /// <summary>名称</summary>
    public String Name { get; set; } = Rand.NextString(8);

    /// <summary>宽度。单位px，负数表示百分比，默认-100</summary>
    public Int32 Width { get; set; } = -100;

    /// <summary>高度。单位px，负数表示百分比，默认300px</summary>
    public Int32 Height { get; set; } = 300;

    /// <summary>网格</summary>
    public ChartGrid Grid { get; set; } = new();

    /// <summary>CSS样式</summary>
    public String Style { get; set; }

    /// <summary>CSS类</summary>
    public String Class { get; set; }

    /// <summary>标题。字符串或匿名对象</summary>
    public ChartTitle Title { get; set; }

    /// <summary>提示</summary>
    public Object Tooltip { get; set; } = new Object();

    /// <summary>图例组件。</summary>
    /// <remarks>图例组件展现了不同系列的标记(symbol)，颜色和名字。可以通过点击图例控制哪些系列不显示。</remarks>
    public Object Legend { get; set; }

    /// <summary>X轴</summary>
    public Object XAxis { get; set; }

    /// <summary>Y轴</summary>
    public Object YAxis { get; set; }

    /// <summary>数据缩放</summary>
    public DataZoom[] DataZoom { get; set; }

    /// <summary>系列数据</summary>
    public IList<Series> Series { get; set; } = [];

    /// <summary>标记的图形。设置后添加的图形都使用该值</summary>
    [ScriptIgnore]
    public String Symbol { get; set; }

    /// <summary>扩展字典</summary>
    [ScriptIgnore]
    public IDictionary<String, Object> Items { get; set; } = new NullableDictionary<String, Object>(StringComparer.OrdinalIgnoreCase);

    /// <summary>扩展数据</summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Object this[String key] { get => Items[key]; set => Items[key] = value; }

    Object _xField;
    String _timeField;
    Func<IModel, String> _timeSelector;
    #endregion

    #region 两轴设置
    /// <summary>设置X轴</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">数据列表，从中选择数据构建X轴</param>
    /// <param name="name">作为X轴的字段，支持time时间轴</param>
    /// <param name="selector">构建X轴的委托</param>
    public void SetX<T>(IList<T> list, String name, Func<T, String> selector = null) where T : class, IModel
    {
        XAxis = new
        {
            data = list.Select(e => selector == null ? e[name] + "" : selector(e)).ToArray()
        };
        _xField = name;
    }

    /// <summary>设置X轴</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">数据列表，从中选择数据构建X轴</param>
    /// <param name="name">作为X轴的字段，支持time时间轴</param>
    /// <param name="selector">构建X轴的委托</param>
    public void SetX4Time<T>(IList<T> list, String name, Func<T, String> selector = null) where T : class, IModel
    {
        XAxis = new
        {
            type = "time",
        };
        _timeField = name;
        _xField = name;

        if (selector != null)
            _timeSelector = e => selector(e as T) + "";

        if (Symbol.IsNullOrEmpty() && list.Count > 100) Symbol = "none";
    }

    /// <summary>设置X轴</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">数据列表，从中选择数据构建X轴</param>
    /// <param name="field">作为X轴的字段，支持time时间轴</param>
    /// <param name="selector">构建X轴的委托</param>
    public void SetX<T>(IList<T> list, DataField field, Func<T, String> selector = null) where T : class, IModel
    {
        if (field != null && field.Type == typeof(DateTime))
            SetX4Time(list, field.Name, selector);
        else
            SetX(list, field.Name, selector);

        _xField = field;
    }

    /// <summary>设置X轴</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">数据列表，从中选择数据构建X轴</param>
    /// <param name="field">作为X轴的字段，支持time时间轴</param>
    /// <param name="selector">构建X轴的委托</param>
    public void SetX<T>(IList<T> list, FieldItem field, Func<T, String> selector = null) where T : class, IModel
    {
        if (field != null && field.Type == typeof(DateTime))
            SetX4Time(list, field.Name, selector);
        else
            SetX(list, field.Name, selector);

        _xField = field;
    }

    Object GetTimeValue(IModel entity) => _timeSelector != null ? _timeSelector(entity) : entity[_timeField];

    /// <summary>设置Y轴</summary>
    /// <param name="name"></param>
    /// <param name="type">
    /// 坐标轴类型。
    /// value 数值轴，适用于连续数据。
    /// category 类目轴，适用于离散的类目数据，为该类型时必须通过 data 设置类目数据。
    /// time 时间轴，适用于连续的时序数据，与数值轴相比时间轴带有时间的格式化，在刻度计算上也有所不同，例如会根据跨度的范围来决定使用月，星期，日还是小时范围的刻度。
    /// log 对数轴。适用于对数数据。
    /// </param>
    public void SetY(String name, String type = "value") => YAxis = new { name, type };

    /// <summary>设置Y轴</summary>
    /// <param name="name"></param>
    /// <param name="type">
    /// 坐标轴类型。
    /// value 数值轴，适用于连续数据。
    /// category 类目轴，适用于离散的类目数据，为该类型时必须通过 data 设置类目数据。
    /// time 时间轴，适用于连续的时序数据，与数值轴相比时间轴带有时间的格式化，在刻度计算上也有所不同，例如会根据跨度的范围来决定使用月，星期，日还是小时范围的刻度。
    /// log 对数轴。适用于对数数据。
    /// </param>
    /// <param name="formatter"></param>
    public void SetY(String name, String type, String formatter) => YAxis = new { name, type, axisLabel = new { formatter } };

    /// <summary>设置多个Y轴</summary>
    /// <param name="names"></param>
    /// <param name="type">
    /// 坐标轴类型。
    /// value 数值轴，适用于连续数据。
    /// category 类目轴，适用于离散的类目数据，为该类型时必须通过 data 设置类目数据。
    /// time 时间轴，适用于连续的时序数据，与数值轴相比时间轴带有时间的格式化，在刻度计算上也有所不同，例如会根据跨度的范围来决定使用月，星期，日还是小时范围的刻度。
    /// log 对数轴。适用于对数数据。
    /// </param>
    /// <param name="formatters">Y轴格式化字符串。如：{value} °C</param>
    public void SetY(String[] names, String type = "value", String[] formatters = null)
    {
        //YAxis = names.Select(e => new { name = e, type }).ToArray();

        var list = new List<Object>();
        for (var i = 0; i < names.Length; i++)
        {
            var formatter = formatters != null && formatters.Length > i ? formatters[i] : null;
            if (i == 0)
                list.Add(new { name = names[i], type, axisLabel = new { formatter } });
            else if (i == 1)
                list.Add(new { name = names[i], type, position = "right", axisLabel = new { formatter } });
            else
                list.Add(new { name = names[i], type, position = "right", offset = 40 * (i - 1), axisLine = new { show = true }, axisLabel = new { formatter } });
        }

        // 多Y轴时，右边偏移加大
        var n = names.Length - 1;
        Grid.Right *= n;

        YAxis = list.ToArray();
    }

    /// <summary>设置工具栏</summary>
    /// <param name="trigger">
    /// 触发类型。
    /// item, 数据项图形触发，主要在散点图，饼图等无类目轴的图表中使用。
    /// axis, 坐标轴触发，主要在柱状图，折线图等会使用类目轴的图表中使用。
    /// none, 什么都不触发。
    /// </param>
    /// <param name="axisPointerType">坐标轴指示器配置项。cross，坐标系会自动选择显示哪个轴的 axisPointer</param>
    /// <param name="backgroundColor"></param>
    public void SetTooltip(String trigger = "axis", String axisPointerType = "cross", String backgroundColor = "#6a7985")
    {
        Tooltip = new
        {
            trigger,
            axisPointer = new
            {
                type = axisPointerType,
                label = new
                {
                    backgroundColor
                }
            },
        };
    }

    /// <summary>设置提示</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="field"></param>
    /// <param name="selector"></param>
    public void SetLegend<T>(IList<T> list, FieldItem field, Func<T, String> selector = null) where T : IModel
        => Legend = list.Select(e => selector == null ? e[field.Name] + "" : selector(e)).ToArray();

    /// <summary>添加缩放。默认X0轴，其它设置可直接修改返回对象</summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public DataZoom AddDataZoom(Int32 start = 0, Int32 end = 100)
    {
        var dz = new DataZoom
        {
            XAxiaIndex = [0],
            Start = start,
            End = end,
        };

        var list = DataZoom?.ToList() ?? [];
        list.Add(dz);

        DataZoom = list.ToArray();

        return dz;
    }
    #endregion

    #region 方法
    /// <summary>添加系列数据</summary>
    /// <param name="series"></param>
    public void Add(Series series)
    {
        Series.Add(series);
    }

    /// <summary>添加系列数据</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">实体列表</param>
    /// <param name="name">要使用数据的字段</param>
    /// <param name="type">图表类型，默认折线图line</param>
    /// <param name="selector">数据选择器，默认null时直接使用字段数据</param>
    /// <returns></returns>
    public Series Add<T>(IList<T> list, String name, String type = "line", Func<T, Object> selector = null) where T : IModel
    {
        if (type.IsNullOrEmpty()) type = "line";

        var data = _timeField != null ?
            list.Select(e => new Object[] { GetTimeValue(e), selector == null ? e[name] : selector(e) }).ToArray() :
            list.Select(e => selector == null ? e[name] : selector(e)).ToArray();

        var sr = new Series
        {
            Name = name,
            Type = type,
            Data = data,
        };
        if (!Symbol.IsNullOrEmpty()) sr.Symbol = Symbol;

        Add(sr);

        return sr;
    }

    /// <summary>添加系列数据</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">实体列表</param>
    /// <param name="field">要使用数据的字段</param>
    /// <param name="type">图表类型，默认折线图line</param>
    /// <param name="selector">数据选择器，默认null时直接使用字段数据</param>
    /// <returns></returns>
    public Series Add<T>(IList<T> list, FieldItem field, String type = "line", Func<T, Object> selector = null) where T : IModel
    {
        if (type.IsNullOrEmpty()) type = "line";

        var data = _timeField != null ?
            list.Select(e => new Object[] { GetTimeValue(e), selector == null ? e[field.Name] : selector(e) }).ToArray() :
            list.Select(e => selector == null ? e[field.Name] : selector(e)).ToArray();

        var sr = new Series
        {
            Name = field?.DisplayName ?? field.Name,
            Type = type,
            Data = data,
        };
        if (!Symbol.IsNullOrEmpty()) sr.Symbol = Symbol;

        Add(sr);
        return sr;
    }

    /// <summary>添加系列数据</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">实体列表</param>
    /// <param name="field">要使用数据的字段</param>
    /// <param name="type">图表类型，默认折线图line</param>
    /// <param name="selector">数据选择器，默认null时直接使用字段数据</param>
    /// <returns></returns>
    public Series Add<T>(IList<T> list, FieldItem field, SeriesTypes type, Func<T, Object> selector = null) where T : IModel
    {
        var data = _timeField != null ?
            list.Select(e => new Object[] { GetTimeValue(e), selector == null ? e[field.Name] : selector(e) }).ToArray() :
            list.Select(e => selector == null ? e[field.Name] : selector(e)).ToArray();

        var sr = new Series
        {
            Name = field?.DisplayName ?? field.Name,
            Type = type.ToString().ToLower(),
            Data = data,
        };
        if (!Symbol.IsNullOrEmpty()) sr.Symbol = Symbol;

        Add(sr);
        return sr;
    }

    /// <summary>批量添加系列数据</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">实体列表</param>
    /// <param name="fields">要使用数据的字段</param>
    /// <param name="type">图表类型，默认折线图line</param>
    /// <param name="selector">数据选择器，默认null时直接使用字段数据</param>
    /// <returns></returns>
    public IList<Series> Add<T>(IList<T> list, DataField[] fields, SeriesTypes type = SeriesTypes.Line, Func<T, Object> selector = null) where T : IModel
    {
        var rs = new List<Series>();
        foreach (var field in fields)
        {
            Series series = null;
            if (type == SeriesTypes.Line)
                series = AddLine(list, field, selector);
            else if (type == SeriesTypes.Bar)
                series = AddBar(list, field, selector);
            else if (type == SeriesTypes.Pie)
                series = AddPie(list, field);
            else
            {
                var data = _timeField != null ?
                list.Select(e => new Object[] { GetTimeValue(e), selector == null ? e[field.Name] : selector(e) }).ToArray() :
                list.Select(e => selector == null ? e[field.Name] : selector(e)).ToArray();

                series = new Series
                {
                    Name = field?.DisplayName ?? field.Name,
                    Type = type.ToString().ToLower(),
                    Data = data,
                };
                if (!Symbol.IsNullOrEmpty()) series.Symbol = Symbol;

                Add(series);
            }
            rs.Add(series);
        }

        return rs;
    }

    /// <summary>添加曲线系列数据</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">实体列表</param>
    /// <param name="name">要使用数据的字段</param>
    /// <param name="selector">数据选择器，默认null时直接使用字段数据</param>
    /// <param name="smooth">折线光滑</param>
    /// <returns></returns>
    public Series AddLine<T>(IList<T> list, String name, Func<T, Object> selector = null, Boolean smooth = false) where T : IModel => AddLine(list, new DataField { Name = name }, selector, smooth);

    /// <summary>添加曲线系列数据</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">实体列表</param>
    /// <param name="field">要使用数据的字段</param>
    /// <param name="selector">数据选择器，默认null时直接使用字段数据</param>
    /// <param name="smooth">折线光滑</param>
    /// <returns></returns>
    public Series AddLine<T>(IList<T> list, DataField field, Func<T, Object> selector = null, Boolean smooth = false) where T : IModel
    {
        var data = _timeField != null ?
            list.Select(e => new Object[] { GetTimeValue(e), selector == null ? e[field.Name] : selector(e) }).ToArray() :
            list.Select(e => selector == null ? e[field.Name] : selector(e)).ToArray();

        var sr = new Series
        {
            Name = field.DisplayName ?? field.Name,
            Type = "line",
            Data = data,
            Smooth = smooth,
        };
        if (!Symbol.IsNullOrEmpty()) sr.Symbol = Symbol;

        Add(sr);
        return sr;
    }

    /// <summary>添加曲线系列数据</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">实体列表</param>
    /// <param name="field">要使用数据的字段</param>
    /// <param name="selector">数据选择器，默认null时直接使用字段数据</param>
    /// <param name="smooth">折线光滑</param>
    /// <returns></returns>
    public Series AddLine<T>(IList<T> list, FieldItem field, Func<T, Object> selector = null, Boolean smooth = false) where T : IModel => AddLine(list, new DataField(field), selector, smooth);

    /// <summary>添加柱状图</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">实体列表</param>
    /// <param name="name">要使用数据的字段</param>
    /// <param name="selector">数据选择器，默认null时直接使用字段数据</param>
    /// <returns></returns>
    public Series AddBar<T>(IList<T> list, String name, Func<T, Object> selector = null) where T : IModel => AddBar(list, new DataField { Name = name }, selector);

    /// <summary>添加柱状图</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">实体列表</param>
    /// <param name="field">要使用数据的字段</param>
    /// <param name="selector">数据选择器，默认null时直接使用字段数据</param>
    /// <returns></returns>
    public Series AddBar<T>(IList<T> list, DataField field, Func<T, Object> selector = null) where T : IModel
    {
        var sr = new Series
        {
            Name = field.DisplayName ?? field.Name,
            Type = "bar",
            Data = list.Select(e => selector == null ? e[field.Name] : selector(e)).ToArray(),
        };

        Add(sr);
        return sr;
    }

    /// <summary>添加柱状图</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">实体列表</param>
    /// <param name="field">要使用数据的字段</param>
    /// <param name="selector">数据选择器，默认null时直接使用字段数据</param>
    /// <returns></returns>
    public Series AddBar<T>(IList<T> list, FieldItem field, Func<T, Object> selector = null) where T : IModel => AddBar(list, new DataField(field), selector);

    /// <summary>添加饼图</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">实体列表</param>
    /// <param name="keyName">要使用分类的字段</param>
    /// <param name="valueName">要使用数据的字段</param>
    /// <param name="displayName">显示名</param>
    /// <param name="selector">数据选择器，默认null时直接使用字段数据</param>
    /// <returns></returns>
    public Series AddPie<T>(IList<T> list, String keyName, String valueName, String displayName, Func<T, NameValue> selector = null) where T : IModel
    {
        var sr = new Series
        {
            Name = displayName ?? valueName,
            Type = "pie",
            Data = list.Select(e => selector == null ? new NameValue(e[keyName] + "", e[valueName]) : selector(e)).ToArray(),
        };

        Add(sr);
        return sr;
    }

    /// <summary>添加饼图</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">实体列表</param>
    /// <param name="field">数据字段</param>
    /// <param name="selector">数据选择器，默认null时直接使用字段数据</param>
    /// <returns></returns>
    public Series AddPie<T>(IList<T> list, DataField field, Func<T, NameValue> selector = null) where T : IModel
    {
        var keyName = field.Category;
        keyName ??= _xField as String;
        keyName ??= (_xField as DataField)?.Name;
        keyName ??= (_xField as FieldItem)?.Name;
        var sr = new Series
        {
            Name = field.DisplayName ?? field.Name,
            Type = "pie",
            Data = list.Select(e => selector == null ? new NameValue(e[keyName] + "", e[field.Name]) : selector(e)).ToArray(),
        };

        Add(sr);
        return sr;
    }

    /// <summary>添加饼图</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">实体列表</param>
    /// <param name="field">要使用数据的字段</param>
    /// <param name="selector">数据选择器，默认null时直接使用字段数据</param>
    /// <returns></returns>
    public Series AddPie<T>(IList<T> list, FieldItem field, Func<T, NameValue> selector = null) where T : IModel
    {
        var keyName = _xField as String;
        keyName ??= (_xField as DataField)?.Name;
        keyName ??= (_xField as FieldItem)?.Name;
        keyName ??= field.Table.Master?.Name ?? field.Table.PrimaryKeys.FirstOrDefault()?.Name;
        var sr = new Series
        {
            Name = field.DisplayName ?? field.Name,
            Type = "pie",
            Data = list.Select(e => selector == null ? new NameValue(e[keyName] + "", e[field.Name]) : selector(e)).ToArray(),
        };

        Add(sr);
        return sr;
    }
    #endregion

    #region 构建生成
    /// <summary>构建选项Json</summary>
    /// <returns></returns>
    public String Build()
    {
        var dic = new Dictionary<String, Object>();
        var series = Series;

        // 标题
        var title = Title;
        if (title != null) dic[nameof(title)] = title;

        // 提示
        var tooltip = Tooltip;
        if (tooltip != null) dic[nameof(tooltip)] = tooltip;

        // 提示
        var legend = Legend;
        if (legend == null && series != null && series.Count > 0)
        {
            if (series.Any(e => e.Type == "line" || e.Type == "bar"))
                legend = series.Select(e => e.Name).ToArray();
            else if (series.Any(e => e.Type == "pie"))
                legend = new { show = true, top = "5%", bottom = "5%", left = "center" };
        }
        if (legend != null)
        {
            if (legend is String str)
                legend = new { data = new[] { str } };
            else if (legend is String[] ss)
                legend = new { data = ss };

            dic[nameof(legend)] = legend;
        }

        // 网格
        var grid = Grid;
        if (grid != null)
        {
            dic[nameof(grid)] = grid;
        }

        // X轴
        var xAxis = XAxis;
        if (xAxis != null)
        {
            if (xAxis is String str)
                xAxis = new { data = new[] { str } };
            else if (xAxis is String[] ss)
                xAxis = new { data = ss };

            dic[nameof(xAxis)] = xAxis;
        }

        // Y轴
        var yAxis = YAxis;
        if (yAxis != null) dic[nameof(yAxis)] = yAxis;

        var dataZoom = DataZoom;
        if (dataZoom != null) dic[nameof(dataZoom)] = dataZoom;

        // 系列数据
        if (series != null) dic[nameof(series)] = series;

        // 合并Items
        foreach (var item in Items)
        {
            dic[item.Key] = item.Value;
        }

#if DEBUG
        return dic.ToJson(true, true, true);
#else
        return dic.ToJson(false, true, true);
#endif
    }
    #endregion
}