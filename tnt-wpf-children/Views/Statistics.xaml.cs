using System.Windows.Controls;
using System.Windows.Data;
using LiveChartsCore.SkiaSharpView.WPF;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using System.Collections.Generic;

namespace tnt_wpf_children.Views
{
    public partial class Statistics : UserControl
    {
        public Statistics()
        {
            InitializeComponent();

            var chart = new CartesianChart();
            
            BindingOperations.SetBinding(chart, CartesianChart.SeriesProperty, new Binding("Series"));
            BindingOperations.SetBinding(chart, CartesianChart.XAxesProperty, new Binding("XAxes"));
            BindingOperations.SetBinding(chart, CartesianChart.YAxesProperty, new Binding("YAxes"));

            BindingOperations.SetBinding(chart, CartesianChart.DataPointerDownCommandProperty, new Binding("ChartPointCommand"));

            ChartBorder.Child = chart;
        }
    }
}
