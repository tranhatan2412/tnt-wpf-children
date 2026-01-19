using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Kernel.Sketches; // For IChartPoint
using SkiaSharp;
using tnt_wpf_children.Data;
using tnt_wpf_children.Models;
using LiveChartsCore.Kernel;

namespace tnt_wpf_children.ViewModels
{
    public class StatisticsViewModel : BaseViewModel
    {
        private AppDbContext _context;
        
        public ICommand NextCommand { get; }
        public ICommand PrevCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ChartPointCommand { get; }

        public StatisticsViewModel()
        {
            _context = new AppDbContext();
            
            _currentDate = DateTime.Now;
            _isMonthView = false; // Start in Year View
            
            _totalSessions = 0;
            _totalChildren = 0;

            AvailableMonths = Enumerable.Range(1, 12).ToList();
            
            var minYear = _context.Sessions.Select(s => s.CheckinTime.Year).OrderBy(y => y).FirstOrDefault();
            if (minYear == 0) minYear = DateTime.Now.Year; 
            
            var currentYear = DateTime.Now.Year;
            if (minYear > currentYear) minYear = currentYear;

            AvailableYears = Enumerable.Range(minYear, currentYear - minYear + 1).OrderByDescending(y => y).ToList();

            NextCommand = new RelayCommand<object>(p => true, p => MoveTime(1));
            PrevCommand = new RelayCommand<object>(p => true, p => MoveTime(-1));
            BackCommand = new RelayCommand<object>(p => IsMonthView, p => GoBack());
            
            ChartPointCommand = new RelayCommand<IEnumerable<ChartPoint>>(p => true, HandleChartClick);

            LoadData();
        }

        private DateTime _currentDate;
        public DateTime CurrentDate
        {
            get => _currentDate;
            set 
            { 
                _currentDate = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(SelectedYear));
                OnPropertyChanged(nameof(SelectedMonth));
            }
        }

        public List<int> AvailableYears { get; }
        public List<int> AvailableMonths { get; }

        public int SelectedYear
        {
            get => CurrentDate.Year;
            set
            {
                if (CurrentDate.Year != value)
                {
                    CurrentDate = new DateTime(value, CurrentDate.Month, 1);
                    LoadData();
                }
            }
        }

        public int SelectedMonth
        {
            get => CurrentDate.Month;
            set
            {
                if (CurrentDate.Month != value)
                {
                    CurrentDate = new DateTime(CurrentDate.Year, value, 1);
                    LoadData();
                }
            }
        }

        private bool _isMonthView;
        public bool IsMonthView
        {
            get => _isMonthView;
            set 
            { 
                _isMonthView = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(IsYearView)); 
                OnPropertyChanged(nameof(Title)); 
                CommandManager.InvalidateRequerySuggested();
            }
        }
        
        public bool IsYearView => !IsMonthView;

        public string Title
        {
            get
            {
                if (IsMonthView)
                    return $"Tháng {CurrentDate.Month}/{CurrentDate.Year}";
                else
                    return $"Năm {CurrentDate.Year}";
            }
        }

        private ISeries[] _series;
        public ISeries[] Series
        {
            get => _series;
            set { _series = value; OnPropertyChanged(); }
        }

        private Axis[] _xAxes;
        public Axis[] XAxes
        {
            get => _xAxes;
            set { _xAxes = value; OnPropertyChanged(); }
        }

        private Axis[] _yAxes;
        public Axis[] YAxes
        {
            get => _yAxes;
            set { _yAxes = value; OnPropertyChanged(); }
        }

        private int _totalSessions;
        public int TotalSessions
        {
            get => _totalSessions;
            set { _totalSessions = value; OnPropertyChanged(); }
        }

        private int _totalChildren;
        public int TotalChildren
        {
            get => _totalChildren;
            set { _totalChildren = value; OnPropertyChanged(); }
        }

        private void MoveTime(int step)
        {
            if (IsMonthView)
                CurrentDate = CurrentDate.AddMonths(step);
            else
                CurrentDate = CurrentDate.AddYears(step);

            LoadData();
        }

        private void GoBack()
        {
            IsMonthView = false;
            LoadData();
        }

        private void HandleChartClick(IEnumerable<ChartPoint> points)
        {
            if (IsMonthView) return; 

            var point = points?.FirstOrDefault();
            if (point == null) return;

            int monthIndex = (int)point.Index + 1; 
            
            CurrentDate = new DateTime(CurrentDate.Year, monthIndex, 1);
            IsMonthView = true;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var data = _context.Sessions.AsQueryable();
                
                DateTime start, end;
                List<string> labels = new List<string>();
                List<int> sessionCounts = new List<int>();
                List<int> childCounts = new List<int>();

                if (IsMonthView)
                {
                    start = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
                    end = start.AddMonths(1);
                    
                    data = data.Where(s => s.CheckinTime >= start && s.CheckinTime < end);
                    var list = data.ToList();

                    TotalSessions = list.Count;
                    TotalChildren = list.Sum(s => s.NumberOfChildren ?? 0);

                    var daysInMonth = DateTime.DaysInMonth(CurrentDate.Year, CurrentDate.Month);
                    var grouped = list.GroupBy(s => s.CheckinTime.Day).ToDictionary(g => g.Key, g => g.ToList());

                    for (int i = 1; i <= daysInMonth; i++)
                    {
                        labels.Add($"{i}/{CurrentDate.Month}");
                        if (grouped.ContainsKey(i))
                        {
                            sessionCounts.Add(grouped[i].Count);
                            childCounts.Add(grouped[i].Sum(s => s.NumberOfChildren ?? 0));
                        }
                        else
                        {
                            sessionCounts.Add(0);
                            childCounts.Add(0);
                        }
                    }
                }
                else
                {
                    start = new DateTime(CurrentDate.Year, 1, 1);
                    end = start.AddYears(1);

                    data = data.Where(s => s.CheckinTime >= start && s.CheckinTime < end);
                    var list = data.ToList();

                    TotalSessions = list.Count;
                    TotalChildren = list.Sum(s => s.NumberOfChildren ?? 0);

                    var grouped = list.GroupBy(s => s.CheckinTime.Month).ToDictionary(g => g.Key, g => g.ToList());

                    for (int i = 1; i <= 12; i++)
                    {
                        labels.Add($"T{i}");
                        if (grouped.ContainsKey(i))
                        {
                            sessionCounts.Add(grouped[i].Count);
                            childCounts.Add(grouped[i].Sum(s => s.NumberOfChildren ?? 0));
                        }
                        else
                        {
                            sessionCounts.Add(0);
                            childCounts.Add(0);
                        }
                    }
                }

                Series = new ISeries[]
                {
                    new ColumnSeries<int>
                    {
                        Name = "Số lượt gửi",
                        Values = sessionCounts.ToArray(),
                        Fill = new SolidColorPaint(SKColors.DodgerBlue)
                    },
                    new ColumnSeries<int>
                    {
                        Name = "Số trẻ",
                        Values = childCounts.ToArray(),
                        Fill = new SolidColorPaint(SKColors.Orange)
                    }
                };

                // Configure Axes
                XAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = labels,
                        LabelsRotation = 0,
                        SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)),
                        SeparatorsAtCenter = false,
                        TicksPaint = new SolidColorPaint(new SKColor(35, 35, 35)),
                        TicksAtCenter = true
                    }
                };

                YAxes = new Axis[]
                {
                    new Axis
                    {
                        MinStep = 1,
                        Labeler = (value) => value.ToString("N0")
                    }
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Stats Error: {ex.Message}");
            }
        }
    }
}
