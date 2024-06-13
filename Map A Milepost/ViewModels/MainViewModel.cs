using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Map_A_Milepost.Commands;
using Map_A_Milepost.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Map_A_Milepost.ViewModels
{
    class MainViewModel: ViewModelBase
    {
        public ICommand SelectPageCommand => new Commands.RelayCommand(SelectPage);
        private MapLineViewModel _mapLineViewModel;
        private MapPointViewModel _mapPointViewModel;
        private ResultsViewModel _resultsViewModel;
        private object _selectedViewModel;
        private string _testString = "test";
        public MapPointViewModel MapPointViewModel
        {
            get => _mapPointViewModel;
            set {
                if (_mapPointViewModel != value)
                {
                    _mapPointViewModel = value;
                    OnPropertyChanged("MapPointViewModel");
                }
            }
        }
        public MapLineViewModel MapLineViewModel
        {
            get => _mapLineViewModel;
            set
            {
                if (_mapLineViewModel != value)
                {
                    _mapLineViewModel = value;
                    OnPropertyChanged("MapLineViewModel");
                }
            }
        }
        public ResultsViewModel ResultsViewModel
        {
            get => _resultsViewModel;
            set
            {
                if (_resultsViewModel != value)
                {
                    _resultsViewModel = value;
                    OnPropertyChanged("ResultsViewModel");
                }
            }
        }
        public object SelectedViewModel
        {
            get => _selectedViewModel;
            set
            {
                this._selectedViewModel = value;
                OnPropertyChanged("SelectedViewModel");
            }
        }

        public MainViewModel()
        {
            this.MapPointViewModel = new MapPointViewModel();
            this.MapLineViewModel = new MapLineViewModel();
            this.ResultsViewModel = new ResultsViewModel();
            this.SelectedViewModel = this.MapPointViewModel;
           
        }
        public void SelectPage(object param)
        {
            TabControl control = ((TabControl)param);
            TabItem tabItem = (TabItem)control.SelectedItem;
            string Name = tabItem.Name;
            switch (Name){
                case "MapPointButton":
                    SelectedViewModel = _mapPointViewModel;
                    break;
                case "MapLineButton":
                    SelectedViewModel = _mapLineViewModel;
                    break;
                case "ResultsButton":
                    SelectedViewModel = _resultsViewModel;
                    break;
                default: throw new ArgumentException("Control name not found");
            }
            
        }
    }
}
