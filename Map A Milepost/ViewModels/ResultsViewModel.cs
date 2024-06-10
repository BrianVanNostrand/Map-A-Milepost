using Map_A_Milepost.Models;
using Map_A_Milepost.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Map_A_Milepost.ViewModels
{
    public class ResultsViewModel : Utils.ViewModelBase
    {
        private string _selectedDataGrid;
        public ResultsViewModel() { 
        }
        public String SelectedDataGrid
        {
            get { return _selectedDataGrid; }
            set { _selectedDataGrid = value; OnPropertyChanged(nameof(SelectedDataGrid)); }
        }
        public ICommand selectDataGrid
        {
            get
            {
                return new Commands.RelayCommand(obj =>
                {
                    setSelectedDataGrid("");
                });
            }
        }
        public void setSelectedDataGrid(string param)
        {

        }
    }
}
