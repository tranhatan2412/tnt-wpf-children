using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using tnt_wpf_children.Models;

namespace tnt_wpf_children.ViewModels
{
    public class RelativeViewModel : BaseViewModel
    {
        public RelativeViewModel()
        {
            Items1 = new ObservableCollection<SelectableViewModel>(
                CreateData().Select(x => new SelectableViewModel(x))
            );

            foreach (var item in Items1)
            {
                item.PropertyChanged += Item_PropertyChanged;
            }
        }

        public ObservableCollection<SelectableViewModel> Items1 { get; }

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
            if (e.PropertyName == nameof(SelectableViewModel.IsSelected))
                OnPropertyChanged(nameof(IsAllItems1Selected));
        }

        #endregion

        #region Sample Data (Model layer)

        private static IEnumerable<Relatives> CreateData()
        {
            return new List<Relatives>
            {
                new Relatives
                {
                    FullName = "Material Design",
                    PhoneNumber = "123-456-7890",
                    CreatedAt = DateTime.Now.AddDays(-5),
                    UpdatedAt = DateTime.Now
                },
                new Relatives
                {
                    FullName = "Dragablz",
                    PhoneNumber = "098-765-4321",
                    CreatedAt = DateTime.Now.AddDays(-3),
                    UpdatedAt = DateTime.Now
                },
                new Relatives
                {
                    FullName = "Predator",
                    PhoneNumber = "555-555-5555",
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
    }

    // =====================================================
    // Row ViewModel (UI state ONLY)
    // =====================================================
    public class SelectableViewModel : BaseViewModel
    {
        private bool _isSelected;

        public SelectableViewModel(Relatives model)
        {
            Model = model;
        }

        public Relatives Model { get; }

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

        public string PhoneNumber
        {
            get => Model.PhoneNumber;
            set
            {
                if (Model.PhoneNumber == value) return;
                Model.PhoneNumber = value;
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
    }
}
