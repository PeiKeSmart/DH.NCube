﻿namespace NewLife.Cube.Charts;

/// <summary>漏斗图</summary>
public class SeriesFunnel : Series
{
    //public String Type { get; set; } = "funnel";

    ///// <summary>组件 ID</summary>
    ///// <remark>默认不指定。指定则可用于在 option 或者 API 中引用组件。</remark>
    //public String Id { get; set; }

    ///// <summary></summary>
    ///// <remark>系列名称，用于tooltip的显示，legend 的图例筛选，在 setOption 更新数据和配置项时用于指定对应的系列。</remark>
    //public String Name { get; set; }

    ///// <summary></summary>
    ///// <remark>
    ///// 从 v5.2.0 开始支持
    ///// 从调色盘 option.color 中取色的策略，可取值为：
    ///// 'series'：按照系列分配调色盘中的颜色，同一系列中的所有数据都是用相同的颜色；
    ///// 'data'：按照数据项分配调色盘中的颜色，每个数据项都使用不同的颜色。
    ///// </remark>
    //public String ColorBy { get; set; }

    /// <summary>指定的数据最小值</summary>
    public Double? Min { get; set; }

    /// <summary>指定的数据最大值</summary>
    public Double? Max { get; set; }

    /// <summary>数据最小值 min 映射的宽度</summary>
    /// <remark>可以是绝对的像素大小，也可以是相对布局宽度的百分比，如果需要最小值的图形并不是尖端三角，可通过设置该属性实现。</remark>
    public Object MinSize { get; set; }

    /// <summary>数据最大值 max 映射的宽度</summary>
    /// <remark>可以是绝对的像素大小，也可以是相对布局宽度的百分比。</remark>
    public Object MaxSize { get; set; }

    /// <summary></summary>
    /// <remark>
    /// 漏斗图朝向，支持配置为'vertical'或者'horizontal'。
    /// 从 v4.9.0 开始支持
    /// </remark>
    public String Orient { get; set; }

    /// <summary></summary>
    /// <remark>数据排序， 可以取 'ascending'，'descending'，'none'（表示按 data 顺序），或者一个函数（即 Array.prototype.sort(function (a, b) { ... })）。</remark>
    public String Sort { get; set; }

    /// <summary>数据图形间距</summary>
    public Double? Gap { get; set; }

    /// <summary>是否启用图例 hover 时的联动高亮</summary>
    public Boolean? LegendHoverLink { get; set; }

    /// <summary>水平方向对齐布局类型</summary>
    /// <remark>默认居中对齐，可用选项还有：'left' | 'right' | 'center'</remark>
    public String FunnelAlign { get; set; }

    /// <summary>漏斗图图形上的文本标签，可用于说明图形的一些数据信息，比如值，名称等。</summary>
    public Object Label { get; set; }

    /// <summary></summary>
    /// <remark>标签的视觉引导线样式，在 label 位置 设置为'left'或者'right'的时候会显示视觉引导线。</remark>
    public Object LabelLine { get; set; }

    /// <summary>图形样式</summary>
    public Object ItemStyle { get; set; }

    /// <summary></summary>
    /// <remark>
    /// 从 v5.0.0 开始支持
    /// 标签的统一布局配置。
    /// 该配置项是在每个系列默认的标签布局基础上，统一调整标签的(x, y)位置，标签对齐等属性以实现想要的标签布局效果。
    /// 该配置项也可以是一个有如下参数的回调函数
    /// // 标签对应数据的 dataIndex
    /// dataIndex: number
    /// // 标签对应的数据类型，只在关系图中会有 node 和 edge 数据类型的区分
    /// dataType?: string
    /// // 标签对应的系列的 index
    /// seriesIndex: number
    /// // 标签显示的文本
    /// text: string
    /// // 默认的标签的包围盒，由系列默认的标签布局决定
    /// labelRect: {x: number, y: number, width: number, height: number}
    /// // 默认的标签水平对齐
    /// align: 'left' | 'center' | 'right'
    /// // 默认的标签垂直对齐
    /// verticalAlign: 'top' | 'middle' | 'bottom'
    /// // 标签所对应的数据图形的包围盒，可用于定位标签位置
    /// rect: {x: number, y: number, width: number, height: number}
    /// // 默认引导线的位置，目前只有饼图(pie)和漏斗图(funnel)有默认标签位置
    /// // 如果没有该值则为 null
    /// labelLinePoints?: number[][]
    /// 示例：
    /// 将标签显示在图形右侧 10px 的位置，并且垂直居中：
    /// labelLayout(params) {
    ///     return {
    ///         x: params.rect.x + 10,
    ///         y: params.rect.y + params.rect.height / 2,
    ///         verticalAlign: 'middle',
    ///         align: 'left'
    ///     }
    /// }
    /// 根据图形的包围盒尺寸决定文本尺寸
    /// labelLayout(params) {
    ///     return {
    ///         fontSize: Math.max(params.rect.width / 10, 5)
    ///     };
    /// }
    /// </remark>
    public Object LabelLayout { get; set; }

    /// <summary>高亮的标签和图形样式</summary>
    public Object Emphasis { get; set; }

    /// <summary></summary>
    /// <remark>
    /// 从 v5.0.0 开始支持
    /// 淡出时的图形样式和标签样式。开启 emphasis.focus 后有效
    /// </remark>
    public Object Blur { get; set; }

    /// <summary></summary>
    /// <remark>
    /// 从 v5.0.0 开始支持
    /// 数据选中时的图形样式和标签样式。开启 selectedMode 后有效。
    /// </remark>
    public Object Select { get; set; }

    /// <summary></summary>
    /// <remark>
    /// 从 v5.0.0 开始支持
    /// 选中模式的配置，表示是否支持多个选中，默认关闭，支持布尔值和字符串，字符串取值可选'single'，'multiple'，'series' 分别表示单选，多选以及选择整个系列。
    /// 从 v5.3.0 开始支持 'series'。
    /// </remark>
    public Object SelectedMode { get; set; }

    /// <summary>所有图形的 zlevel 值</summary>
    /// <remark>
    /// zlevel用于 Canvas 分层，不同zlevel值的图形会放置在不同的 Canvas 中，Canvas 分层是一种常见的优化手段。我们可以把一些图形变化频繁（例如有动画）的组件设置成一个单独的zlevel。需要注意的是过多的 Canvas 会引起内存开销的增大，在手机端上需要谨慎使用以防崩溃。
    /// zlevel 大的 Canvas 会放在 zlevel 小的 Canvas 的上面。
    /// </remark>
    public Double? Zlevel { get; set; }

    /// <summary>组件的所有图形的z值</summary>
    /// <remark>
    /// 控制图形的前后顺序。z值小的图形会被z值大的图形覆盖。
    /// z相比zlevel优先级更低，而且不会创建新的 Canvas。
    /// </remark>
    public Double? Z { get; set; }

    /// <summary>漏斗图组件离容器左侧的距离</summary>
    /// <remark>
    /// left 的值可以是像 20 这样的具体像素值，可以是像 '20%' 这样相对于容器高宽的百分比，也可以是 'left', 'center', 'right'。
    /// 如果 left 的值为 'left', 'center', 'right'，组件会根据相应的位置自动对齐。
    /// </remark>
    public Object Left { get; set; }

    /// <summary>漏斗图组件离容器上侧的距离</summary>
    /// <remark>
    /// top 的值可以是像 20 这样的具体像素值，可以是像 '20%' 这样相对于容器高宽的百分比，也可以是 'top', 'middle', 'bottom'。
    /// 如果 top 的值为 'top', 'middle', 'bottom'，组件会根据相应的位置自动对齐。
    /// </remark>
    public Object Top { get; set; }

    /// <summary>漏斗图组件离容器右侧的距离</summary>
    /// <remark>right 的值可以是像 20 这样的具体像素值，可以是像 '20%' 这样相对于容器高宽的百分比。</remark>
    public Object Right { get; set; }

    /// <summary>漏斗图组件离容器下侧的距离</summary>
    /// <remark>bottom 的值可以是像 20 这样的具体像素值，可以是像 '20%' 这样相对于容器高宽的百分比。</remark>
    public Object Bottom { get; set; }

    /// <summary>漏斗图组件的宽度</summary>
    /// <remark>默认自适应。</remark>
    public Object Width { get; set; }

    /// <summary>漏斗图组件的高度</summary>
    /// <remark>默认自适应。</remark>
    public Object Height { get; set; }

    /// <summary></summary>
    /// <remark>
    /// 当使用 dataset 时，seriesLayoutBy 指定了 dataset 中用行还是列对应到系列上，也就是说，系列“排布”到 dataset 的行还是列上。可取值：
    /// 'column'：默认，dataset 的列对应于系列，从而 dataset 中每一列是一个维度（dimension）。
    /// 'row'：dataset 的行对应于系列，从而 dataset 中每一行是一个维度（dimension）。
    /// 参见这个 示例
    /// </remark>
    public String SeriesLayoutBy { get; set; }

    /// <summary></summary>
    /// <remark>如果 series.data 没有指定，并且 dataset 存在，那么就会使用 dataset。datasetIndex 指定本系列使用哪个 dataset。</remark>
    public Double? DatasetIndex { get; set; }

    /// <summary></summary>
    /// <remark>
    /// 使用 dimensions 定义 series.data 或者 dataset.source 的每个维度的信息。
    /// 注意：如果使用了 dataset，那么可以在 dataset.dimensions 中定义 dimension ，或者在 dataset.source 的第一行/列中给出 dimension 名称。于是就不用在这里指定 dimension。但如果在这里指定了 dimensions，那么优先使用这里的。
    /// 例如：
    /// option = {
    ///     dataset: {
    ///         source: [
    ///             // 有了上面 dimensions 定义后，下面这五个维度的名称分别为：
    ///             // 'date', 'open', 'close', 'highest', 'lowest'
    ///             [12, 44, 55, 66, 2],
    ///             [23, 6, 16, 23, 1],
    ///             ...
    ///         ]
    ///     },
    ///     series: {
    ///         type: 'xxx',
    ///         // 定义了每个维度的名称。这个名称会被显示到默认的 tooltip 中。
    ///         dimensions: ['date', 'open', 'close', 'highest', 'lowest']
    ///     }
    /// }
    /// series: {
    ///     type: 'xxx',
    ///     dimensions: [
    ///         null,                // 如果此维度不想给出定义，则使用 null 即可
    ///         {type: 'ordinal'},   // 只定义此维度的类型。
    ///                              // 'ordinal' 表示离散型，一般文本使用这种类型。
    ///                              // 如果类型没有被定义，会自动猜测类型。
    ///         {name: 'good', type: 'number'},
    ///         'bad'                // 等同于 {name: 'bad'}
    ///     ]
    /// }
    /// dimensions 数组中的每一项可以是：
    /// string，如 'someName'，等同于 {name: 'someName'}
    /// Object，属性可以有：
    /// name: string。
    /// type: string，支持
    /// number，默认，表示普通数据。
    /// ordinal，对于类目、文本这些 string 类型的数据，如果需要能在数轴上使用，须是 'ordinal' 类型。ECharts 默认会自动判断这个类型。但是自动判断也是不可能很完备的，所以使用者也可以手动强制指定。
    /// float，即 Float64Array。
    /// int，即 Int32Array。
    /// time，表示时间类型。设置成 'time' 则能支持自动解析数据成时间戳（timestamp），比如该维度的数据是 '2017-05-10'，会自动被解析。时间类型的支持参见 data。
    /// displayName: 一般用于 tooltip 中维度名的展示。string 如果没有指定，默认使用 name 来展示。
    /// 值得一提的是，当定义了 dimensions 后，默认 tooltip 中对个维度的显示，会变为『竖排』，从而方便显示每个维度的名称。如果没有定义 dimensions，则默认 tooltip 会横排显示，且只显示数值没有维度名称可显示。
    /// </remark>
    public Double[] Dimensions { get; set; }

    /// <summary>可以定义 data 的哪个维度被编码成什么</summary>
    /// <remark>
    /// 比如：
    /// option = {
    ///     dataset: {
    ///         source: [
    ///             // 每一列称为一个『维度』。
    ///             // 这里分别是维度 0、1、2、3、4。
    ///             [12, 44, 55, 66, 2],
    ///             [23, 6, 16, 23, 1],
    ///             ...
    ///         ]
    ///     },
    ///     series: {
    ///         type: 'xxx',
    ///         encode: {
    ///             x: [3, 1, 5],      // 表示维度 3、1、5 映射到 x 轴。
    ///             y: 2,              // 表示维度 2 映射到 y 轴。
    ///             tooltip: [3, 2, 4] // 表示维度 3、2、4 会在 tooltip 中显示。
    ///         }
    ///     }
    /// }
    /// 当使用 dimensions 给维度定义名称后，encode 中可直接引用名称，例如：
    /// series: {
    ///     type: 'xxx',
    ///     dimensions: ['date', 'open', 'close', 'highest', 'lowest'],
    ///     encode: {
    ///         x: 'date',
    ///         y: ['open', 'close', 'highest', 'lowest']
    ///     }
    /// }
    /// encode 声明的基本结构如下，其中冒号左边是坐标系、标签等特定名称，如 'x', 'y', 'tooltip' 等，冒号右边是数据中的维度名（string 格式）或者维度的序号（number 格式，从 0 开始计数），可以指定一个或多个维度（使用数组）。通常情况下，下面各种信息不需要所有的都写，按需写即可。
    /// 下面是 encode 支持的属性：
    /// // 在任何坐标系和系列中，都支持：
    /// encode: {
    ///     // 使用 “名为 product 的维度” 和 “名为 score 的维度” 的值在 tooltip 中显示
    ///     tooltip: ['product', 'score']
    ///     // 使用第一个维度和第三个维度的维度名连起来作为系列名。（有时候名字比较长，这可以避免在 series.name 重复输入这些名字）
    ///     seriesName: [1, 3],
    ///     // 表示使用第二个维度中的值作为 id。这在使用 setOption 动态更新数据时有用处，可以使新老数据用 id 对应起来，从而能够产生合适的数据更新动画。
    ///     itemId: 2,
    ///     // 指定数据项的名称使用第三个维度在饼图等图表中有用，可以使这个名字显示在图例（legend）中。
    ///     itemName: 3,
    ///     // 指定数据项的组 ID (groupId)。当全局过渡动画功能开启时，setOption 前后拥有相同 groupId 的数据项会进行动画过渡。
    ///     itemGroupId: 4,
    ///     // 指定数据项对应的子数据组 ID (childGroupId)，用于实现多层下钻和聚合。详见 childGroupId。
    ///     // 从 v5.5.0 开始支持
    ///     itemChildGroupId: 5
    /// }
    /// // 直角坐标系（grid/cartesian）特有的属性：
    /// encode: {
    ///     // 把 “维度1”、“维度5”、“名为 score 的维度” 映射到 X 轴：
    ///     x: [1, 5, 'score'],
    ///     // 把“维度0”映射到 Y 轴。
    ///     y: 0
    /// }
    /// // 单轴（singleAxis）特有的属性：
    /// encode: {
    ///     single: 3
    /// }
    /// // 极坐标系（polar）特有的属性：
    /// encode: {
    ///     radius: 3,
    ///     angle: 2
    /// }
    /// // 地理坐标系（geo）特有的属性：
    /// encode: {
    ///     lng: 3,
    ///     lat: 2
    /// }
    /// // 对于一些没有坐标系的图表，例如饼图、漏斗图等，可以是：
    /// encode: {
    ///     value: 3
    /// }
    /// 这是个更丰富的 encode 的示例：
    /// 特殊地，在 自定义系列（custom series） 中，encode 中轴可以不指定或设置为 null/undefined，从而使系列免于受这个轴控制，也就是说，轴的范围（extent）不会受此系列数值的影响，轴被 dataZoom 控制时也不会过滤掉这个系列：
    /// var option = {
    ///     xAxis: {},
    ///     yAxis: {},
    ///     dataZoom: [{
    ///         xAxisIndex: 0
    ///     }, {
    ///         yAxisIndex: 0
    ///     }],
    ///     series: {
    ///         type: 'custom',
    ///         renderItem: function (params, api) {
    ///             return {
    ///                 type: 'circle',
    ///                 shape: {
    ///                     cx: 100, // x 位置永远为 100
    ///                     cy: api.coord([0, api.value(0)])[1],
    ///                     r: 30
    ///                 },
    ///                 style: {
    ///                     fill: 'blue'
    ///                 }
    ///             };
    ///         },
    ///         encode: {
    ///             // 这样这个系列就不会被 x 轴以及 x
    ///             // 轴上的 dataZoom 控制了。
    ///             x: -1,
    ///             y: 1
    ///         },
    ///         data: [ ... ]
    ///     }
    /// };
    /// </remark>
    public Object Encode { get; set; }

    /// <summary>该系列所有数据项的组 ID，优先级低于groupId</summary>
    /// <remark>详见series.data.groupId。</remark>
    public String DataGroupId { get; set; }

    /// <summary>系列中的数据内容数组</summary>
    /// <remark>
    /// 数组项可以为单个数值，如：
    /// [12, 34, 56, 10, 23]
    /// 如果需要在数据中加入其它维度给 visualMap 组件用来映射到颜色等其它图形属性。每个数据项也可以是数组，如：
    /// [[12, 14], [34, 50], [56, 30], [10, 15], [23, 10]]
    /// 这时候可以将每项数组中的第二个值指定给 visualMap 组件。
    /// 更多时候我们需要指定每个数据项的名称，这时候需要每个项为一个对象：
    /// [{
    ///     // 数据项的名称
    ///     name: '数据1',
    ///     // 数据项值8
    ///     value: 10
    /// }, {
    ///     name: '数据2',
    ///     value: 20
    /// }]
    /// 需要对个别内容指定进行个性化定义时：
    /// [{
    ///     name: '数据1',
    ///     value: 10
    /// }, {
    ///     // 数据项名称
    ///     name: '数据2',
    ///     value : 56,
    ///     //自定义特殊 tooltip，仅对该数据项有效
    ///     tooltip:{},
    ///     //自定义特殊itemStyle，仅对该item有效
    ///     itemStyle:{}
    /// }]
    /// </remark>
    public override Object[] Data { get; set; }

    ///// <summary>图表标注</summary>
    //public Object MarkPoint { get; set; }

    ///// <summary>图表标线</summary>
    //public Object MarkLine { get; set; }

    /// <summary>图表标域，常用于标记图表中某个范围的数据，例如标出某段时间投放了广告。</summary>
    public Object MarkArea { get; set; }

    /// <summary>图形是否不响应和触发鼠标事件，默认为 false，即响应和触发鼠标事件。</summary>
    public Boolean? Silent { get; set; }

    /// <summary>是否开启动画</summary>
    public Boolean? Animation { get; set; }

    /// <summary>是否开启动画的阈值，当单个系列显示的图形数量大于这个阈值时会关闭动画。</summary>
    public Double? AnimationThreshold { get; set; }

    /// <summary>初始动画的时长</summary>
    /// <remark>
    /// 支持回调函数，可以通过每个数据返回不同的时长实现更戏剧的初始动画效果：
    /// animationDuration: function (idx) {
    ///     // 越往后的数据时长越大
    ///     return idx * 100;
    /// }
    /// </remark>
    public Object AnimationDuration { get; set; }

    /// <summary>初始动画的缓动效果</summary>
    /// <remark>不同的缓动效果可以参考 缓动示例。</remark>
    public String AnimationEasing { get; set; }

    /// <summary></summary>
    /// <remark>
    /// 初始动画的延迟，支持回调函数，可以通过每个数据返回不同的 delay 时间实现更戏剧的初始动画效果。
    /// 如下示例：
    /// animationDelay: function (idx) {
    ///     // 越往后的数据延迟越大
    ///     return idx * 100;
    /// }
    /// 也可以看该示例
    /// </remark>
    public Object AnimationDelay { get; set; }

    /// <summary>数据更新动画的时长</summary>
    /// <remark>
    /// 支持回调函数，可以通过每个数据返回不同的时长实现更戏剧的更新动画效果：
    /// animationDurationUpdate: function (idx) {
    ///     // 越往后的数据时长越大
    ///     return idx * 100;
    /// }
    /// </remark>
    public Object AnimationDurationUpdate { get; set; }

    /// <summary>数据更新动画的缓动效果</summary>
    public String AnimationEasingUpdate { get; set; }

    /// <summary></summary>
    /// <remark>
    /// 数据更新动画的延迟，支持回调函数，可以通过每个数据返回不同的 delay 时间实现更戏剧的更新动画效果。
    /// 如下示例：
    /// animationDelayUpdate: function (idx) {
    ///     // 越往后的数据延迟越大
    ///     return idx * 100;
    /// }
    /// 也可以看该示例
    /// </remark>
    public Object AnimationDelayUpdate { get; set; }

    /// <summary></summary>
    /// <remark>
    /// 从 v5.2.0 开始支持
    /// 全局过渡动画相关的配置。
    /// 全局过渡动画（Universal Transition）提供了任意系列之间进行变形动画的功能。开启该功能后，每次setOption，相同id的系列之间会自动关联进行动画的过渡，更细粒度的关联配置见universalTransition.seriesKey配置。
    /// 通过配置数据项的groupId和childGroupId，还可以实现诸如下钻，聚合等一对多或者多对一的动画。
    /// 可以直接在系列中配置 universalTransition: true 开启该功能。也可以提供一个对象进行更多属性的配置。
    /// </remark>
    public Object UniversalTransition { get; set; }

    /// <summary>本系列特定的 tooltip 设定</summary>
    public Object Tooltip { get; set; }

}
