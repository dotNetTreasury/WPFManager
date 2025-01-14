﻿using CorePlugin.Windows;
using Common;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CorePlugin.Pages.Manager
{
    /// <summary>
    /// AdminIndex.xaml 的交互逻辑
    /// </summary>
    public partial class Index : Page
    {
        public Index()
        {
            InitializeComponent();
            this.StartPageInAnimation();

            PointLabel = chartPoint =>
    string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);

            DataContext = this;
        }

        public Func<ChartPoint, string> PointLabel { get; set; }

        private void Chart_OnDataClick(object sender, ChartPoint chartpoint)
        {
            var chart = (LiveCharts.Wpf.PieChart)chartpoint.ChartView;

            //clear selected slice.
            foreach (PieSeries series in chart.Series)
                series.PushOut = 0;

            var selectedSeries = (PieSeries)chartpoint.SeriesView;
            selectedSeries.PushOut = 8;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDataCount();
        }

        /// <summary>
        /// 更新数量
        /// </summary>
        private void UpdateDataCount()
        {
            using (CoreDBContext context = new CoreDBContext())
            {
                lblUserCount.Content = context.User.Any() ? context.User.Where(c => !c.IsDel).Count() : 0;
                lblRoleCount.Content = context.Role.Any() ? context.Role.Count() : 0;
                lblPluginsCount.Content = context.Plugins.Count();
                lblPositionCount.Content = context.DepartmentPosition.Any(c => !c.IsDel) ? context.DepartmentPosition.Count(c => !c.IsDel) : 0;
            }
        }
    }
}
