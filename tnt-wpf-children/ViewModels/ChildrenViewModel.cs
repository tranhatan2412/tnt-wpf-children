using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using tnt_wpf_children.Models;

namespace tnt_wpf_children.ViewModels
{
    public class ChildrenViewModel : BaseViewModel
    {
        public ChildrenViewModel()
        {
            Items1 = new ObservableCollection<SelectableChildrenViewModel>(
        CreateData().Select(x =>
            new SelectableChildrenViewModel(
                x,
                MinDateOfBirth,
                MaxDateOfBirth))
    );

            foreach (var item in Items1)
            {
                item.PropertyChanged += Item_PropertyChanged;
            }
        }

        public ObservableCollection<SelectableChildrenViewModel> Items1 { get; }

        #region Select All Logic 

        public bool? IsAllItems1Selected
        {
            get
            {
                var states = Items1.Select(x => x.IsSelected).Distinct().ToList();
                return states.Count == 1 ? states[0] : (bool?)null;
            }
            set
            {
                if (value.HasValue)
                {
                    foreach (var item in Items1)
                        item.IsSelected = value.Value;

                    OnPropertyChanged();
                }
            }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectableChildrenViewModel.IsSelected))
                OnPropertyChanged(nameof(IsAllItems1Selected));
        }

        #endregion

        #region Sample Data (Model layer)

        private static IEnumerable<Children> CreateData()
        {
            return new List<Children>
            {
                new Children
                {
                    FullName = "Material Design",
                    DateOfBirth = DateTime.Now,
                    CreatedAt = DateTime.Now.AddDays(-5),
                    UpdatedAt = DateTime.Now
                },
                new Children
                {
                    FullName = "Dragablz",
                    DateOfBirth = DateTime.Now,
                    CreatedAt = DateTime.Now.AddDays(-3),
                    UpdatedAt = DateTime.Now
                },
                new Children
                {
                    FullName = "Predator",
                    DateOfBirth = DateTime.Now,
                    CreatedAt = DateTime.Now.AddDays(-1),
                    UpdatedAt = DateTime.Now
                },
                new Children
                {
                    FullName = "Material Design",
                    DateOfBirth = DateTime.Now,
                    CreatedAt = DateTime.Now.AddDays(-5),
                    UpdatedAt = DateTime.Now
                },
                new Children
                {
                    FullName = "Dragablz",
                    DateOfBirth = DateTime.Now,
                    CreatedAt = DateTime.Now.AddDays(-3),
                    UpdatedAt = DateTime.Now
                },
                new Children
                {
                    FullName = "Predator",
                    DateOfBirth = DateTime.Now,
                    CreatedAt = DateTime.Now.AddDays(-1),
                    UpdatedAt = DateTime.Now
                },
                new Children
                {
                    FullName = "Material Design",
                    DateOfBirth = DateTime.Now,
                    CreatedAt = DateTime.Now.AddDays(-5),
                    UpdatedAt = DateTime.Now
                },
                new Children
                {
                    FullName = "Dragablz",
                    DateOfBirth = DateTime.Now,
                    CreatedAt = DateTime.Now.AddDays(-3),
                    UpdatedAt = DateTime.Now
                },
                new Children
                {
                    FullName = "Predator",
                    DateOfBirth = DateTime.Now,
                    CreatedAt = DateTime.Now.AddDays(-1),
                    UpdatedAt = DateTime.Now
                },
                new Children
                {
                    FullName = "Material Design",
                    DateOfBirth = DateTime.Now,
                    CreatedAt = DateTime.Now.AddDays(-5),
                    UpdatedAt = DateTime.Now
                },
                new Children
                {
                    FullName = "Dragablz",
                    DateOfBirth = DateTime.Now,
                    CreatedAt = DateTime.Now.AddDays(-3),
                    UpdatedAt = DateTime.Now
                },
                new Children
                {
                    FullName = "Predator",
                    DateOfBirth = DateTime.Now,
                    CreatedAt = DateTime.Now.AddDays(-1),
                    UpdatedAt = DateTime.Now
                },
                new Children
                {
                    FullName = "Material Design",
                    DateOfBirth = DateTime.Now,
                    CreatedAt = DateTime.Now.AddDays(-5),
                    UpdatedAt = DateTime.Now
                },
                new Children
                {
                    FullName = "Dragablz",
                    DateOfBirth = DateTime.Now,
                    CreatedAt = DateTime.Now.AddDays(-3),
                    UpdatedAt = DateTime.Now
                },
                new Children
                {
                    FullName = "Predator",
                    DateOfBirth = DateTime.Now,
                    CreatedAt = DateTime.Now.AddDays(-1),
                    UpdatedAt = DateTime.Now
                },
                new Children
                {
                    FullName = "Material Design",
                    DateOfBirth = DateTime.Now,
                    CreatedAt = DateTime.Now.AddDays(-5),
                    UpdatedAt = DateTime.Now
                },
                new Children
                {
                    FullName = "Dragablz",
                    DateOfBirth = DateTime.Now,
                    CreatedAt = DateTime.Now.AddDays(-3),
                    UpdatedAt = DateTime.Now
                },
                new Children
                {
                    FullName = "Predator",
                    DateOfBirth = DateTime.Now,
                    CreatedAt = DateTime.Now.AddDays(-1),
                    UpdatedAt = DateTime.Now
                }
            };
        }

        #endregion

        public IEnumerable<DataGridSelectionUnit> SelectionUnits =>
            new[]
            {
                DataGridSelectionUnit.FullRow,
                DataGridSelectionUnit.Cell,
                DataGridSelectionUnit.CellOrRowHeader
            };
        public DateTime MinDateOfBirth { get; } =
    DateTime.Today.AddYears(-6);

        public DateTime MaxDateOfBirth { get; } =
            DateTime.Today.AddMonths(-3);


    }

    public class SelectableChildrenViewModel : BaseViewModel
    {
        private bool _isSelected;
        public DateTime MinDateOfBirth { get; }
        public DateTime MaxDateOfBirth { get; }

        public SelectableChildrenViewModel(Children model, DateTime minDateOfBirth,
    DateTime maxDateOfBirth)
        {
            Model = model;
            MinDateOfBirth = minDateOfBirth;
            MaxDateOfBirth = maxDateOfBirth;
        }

        public Children Model { get; }

        // expose cho DataGrid (KHÔNG thay binding)
        public string FullName
        {
            get => Model.FullName;
            set
            {
                if (Model.FullName == value) return;
                Model.FullName = value;
                OnPropertyChanged();
            }
        }

        public DateTime? DateOfBirth
        {
            get => Model.DateOfBirth;
            set
            {
                DateOfBirthError = null;

                if (value == null)
                {
                    OnPropertyChanged();
                    return;
                }

                if (value < MinDateOfBirth || value > MaxDateOfBirth)
                {
                    DateOfBirthError = "Tuổi trẻ không thuộc độ tuổi mầm non";
                    OnPropertyChanged();
                    return;
                }

                if (Model.DateOfBirth == value) return;
                Model.DateOfBirth = value.Value;
                OnPropertyChanged();
            }
        }
        private string _dateOfBirthError;
        public string DateOfBirthError
        {
            get => _dateOfBirthError;
            private set
            {
                _dateOfBirthError = value;
                OnPropertyChanged();
            }
        }

        public DateTime CreatedAt => Model.CreatedAt;
        public DateTime UpdatedAt => Model.UpdatedAt;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged();
            }
        }
        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(DateOfBirth))
                {
                    if (DateOfBirth < MinDateOfBirth || DateOfBirth > MaxDateOfBirth)
                        return "Tuổi trẻ không hợp lệ";
                }
                return null;
            }
        }


    }

}
