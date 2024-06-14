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
        /// <summary>
        /// Private variables with associated public variables, granting access to the INotifyPropertyChanged command via ViewModelBase.
        /// </summary>
        public ICommand SelectPageCommand => new Commands.RelayCommand(SelectPage);
        private MapLineViewModel _mapLineViewModel;
        private MapPointViewModel _mapPointViewModel;
        private ResultsViewModel _resultsViewModel;
        private object _selectedViewModel;

        /// <summary>
        /// -   Instance of MapPointViewModel used by MapPointView.xaml and ResultsView.xaml.
        /// </summary>
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

        /// <summary>
        /// -   Instance of MapLineViewModel used by MapLineView.xaml and ResultsView.xaml.
        /// </summary>
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

        /// <summary>
        /// -   Instance of ResultsViewModel used by ResultsView.xaml.
        /// </summary>
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

        /// <summary>
        /// -   The currently selected viewmodel, used when a tab is selected in the controlsGrid in MilepostDockpane.xaml
        ///     via data binding.
        /// </summary>
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
            //sets the initial view displayed by the ContentPresenter in MilepostDockpane.xaml via data binding.
            this.SelectedViewModel = this.MapPointViewModel;
        }

        /// <summary>
        /// -   Select Page method called when a tab is clicked using a data-bound relationship between the ICommand SelectPageCommand
        ///     and the controlsGrid element in milepostdockpane.xaml.
        /// -   Switches the SelectedViewModel, which is data-bound to the ContentPresenter element in milepostdockpane.xaml.
        /// </summary>
        /// <param name="param"></param>
        /// <exception cref="ArgumentException"></exception>
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
