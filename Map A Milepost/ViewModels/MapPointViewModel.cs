using Map_A_Milepost.Models;
using Map_A_Milepost.Utils;
using Map_A_Milepost.Views;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Map_A_Milepost.ViewModels
{
    public class MapPointViewModel:ViewModelBase
    {
        //Map point view variables
        private SOEResponseModel _soeResponse;
        private SOEArgsModel _soeArgs;
        private ICommand _updateSOEResponse;
        private ICommand _updateSOEArgs;
        private ObservableCollection<SOEResponseModel> _soePointResponses;
        private ObservableCollection<string> _testString;
        private ICommand _savePointResultCommand;
        private bool _isSaved = false;
        public MapPointViewModel()//constructor
        {
            _soeResponse = new SOEResponseModel();
            _soeArgs = new SOEArgsModel(1);
            _soePointResponses = new ObservableCollection<SOEResponseModel>();
            _testString = new ObservableCollection<string>();
        }
        public bool IsSaved
        {
            get { return _isSaved; }
            set
            {
                _isSaved = value;
                OnPropertyChanged("IsSaved");
            }
        }
        public SOEResponseModel SOEResponse
        {
            get{ return _soeResponse;}
            set{ _soeResponse = value; OnPropertyChanged("SOEResponse"); }
        }
        public SOEArgsModel SOEArgs
        {
            get { return _soeArgs;}
            set { _soeArgs = value; OnPropertyChanged("SOEArgs"); }
        }
        public ObservableCollection<SOEResponseModel> SoePointResponses
        {
            get { return _soePointResponses; }
            set { _soePointResponses = value; OnPropertyChanged("SOEPointResponses"); }
        }
        public List<SOEResponseModel> SelectedItems { get; set; } = new List<SOEResponseModel>();
        public ICommand SelectedItemsCommand
        {
            get
            {
                return new Commands.RelayCommand(list =>
                {
                    SelectedItems.Clear();
                    SelectedItems = Commands.DataGridCommands.UpdateSelection(SelectedItems, list);
                });
            }
        }
        public ICommand DeleteItemsCommand
        {
            get
            {
                return new Commands.RelayCommand(list =>
                {
                    if (SoePointResponses.Count > 0 && SelectedItems.Count > 0)
                    {
                        if (MessageBox.Show(
                            $"Are you sure you wish to delete these {SelectedItems.Count} records?",
                            "Delete Rows",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.Yes
                        ){
                            for (int i=SoePointResponses.Count-1; i>=0; i--)
                            {
                                if (SelectedItems.Contains(SoePointResponses[i]))
                                {
                                    SoePointResponses.Remove(SoePointResponses[i]);
                                }
                            }
                        }
                    }
                    
                });
            }
        }
        public ICommand ClearItemsCommand
        {
            get
            {
                return new Commands.RelayCommand(list =>
                {
                    if (SoePointResponses.Count > 0)
                    {
                        if (MessageBox.Show(
                            $"Are you sure you wish to clear all {SoePointResponses.Count} point records?", 
                            "Clear Results",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.Yes
                        ){
                            SoePointResponses.Clear();
                        }
                    }
                });
            }
        }
        public ICommand UpdateSOEResponse
        {
            get
            {
                if (_updateSOEResponse == null)
                    _updateSOEResponse = new Commands.RelayCommand(Submit,
                        null);
                return _updateSOEResponse;
            }
            set
            {
                _updateSOEResponse = value;
            }
        }
        public ICommand SavePointResultCommand
        {
            get
            {
                if (_savePointResultCommand == null)
                    _savePointResultCommand = new Commands.RelayCommand(SavePointResult,
                        null);
                return _savePointResultCommand;
            }
            set
            {
                _savePointResultCommand = value;
            }
        }
        public async void Submit(object state)
        {
            SOEResponseModel response =  await Utils.HTTPRequest.QuerySOE(SOEArgs);
            if(response != null)
            {
                CopyProps.CopyProperties(response, SOEResponse);
                if (SoePointResponses.Contains(SOEResponse))
                {
                    IsSaved = true;
                }
                else
                {
                    IsSaved = false;
                }
            }
        }
        public void SavePointResult(object state)
        {
            if (Utils.CheckObject.HasBeenUpdated(SOEResponse))
            {
                if (SoePointResponses.Contains(SOEResponse))
                {
                    if(MessageBox.Show(
                        $"The selected point already exists in the results tab. Save a duplicate?",
                        "Clear Results",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.Yes
                    ){
                        SoePointResponses.Add(SOEResponse);
                    }
                }
                else
                {
                    SoePointResponses.Add(SOEResponse);
                    IsSaved = true;
                }
            }
            else
            {
                MessageBox.Show("Create a point to save it to the results tab.", "Save error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
