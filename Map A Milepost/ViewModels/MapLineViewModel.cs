using Map_A_Milepost.Models;
using Map_A_Milepost.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Map_A_Milepost.ViewModels
{
    public class MapLineViewModel:ViewModelBase
    {
        private SOEResponseModel _soeStartResponse;
        private SOEResponseModel _soeEndResponse;
        private SOEArgsModel _soeStartArgs;
        private SOEArgsModel _soeEndArgs;
        private ICommand _updateSOEStartEndResponse;
        private ICommand _updateSOEStartEndArgs;
        private ICommand _saveLineResultCommand;
        private List<List<SOEResponseModel>> _soeLineResponses;
        public MapLineViewModel()//constructor
        {
            _soeStartResponse = new SOEResponseModel();
            _soeEndResponse = new SOEResponseModel();
            _soeStartArgs = new SOEArgsModel(1);
            _soeEndArgs = new SOEArgsModel(2);
            _soeLineResponses = new List<List<SOEResponseModel>>();
        }
        public List<List<SOEResponseModel>> SoeLineResponses
        {
            get { return _soeLineResponses; }
            set { _soeLineResponses = value; OnPropertyChanged("SoeLineResponses"); }
        }
        public SOEResponseModel SOEStartResponse
        {
            get { return _soeStartResponse; }
            set { _soeStartResponse = value; OnPropertyChanged("SOEStartResponse"); }
        }
        public SOEResponseModel SOEEndResponse
        {
            get { return _soeEndResponse; }
            set { _soeEndResponse = value; }
        }
        public SOEArgsModel SOEStartArgs
        {
            get { return _soeStartArgs; }
            set { _soeStartArgs = value; }
        }
        public SOEArgsModel SOEEndArgs
        {
            get { return _soeEndArgs; }
            set { _soeEndArgs = value; }
        }
        public ICommand UpdateSOEStartEndResponseCommand
        {
            get
            {
                if (_updateSOEStartEndResponse == null)
                    _updateSOEStartEndResponse = new Commands.RelayCommand(param => this.SubmitStartEnd(param),
                        null);
                return _updateSOEStartEndResponse;
            }
            set
            {
                _updateSOEStartEndResponse = value;
            }
        }
        public ICommand SaveLineResultCommand
        {
            get
            {
                if (_saveLineResultCommand == null)
                    _saveLineResultCommand = new Commands.RelayCommand(SaveLineResult,
                        null);
                return _saveLineResultCommand;
            }
            set
            {
                _saveLineResultCommand = value;
            }
        }
        public async void SubmitStartEnd(object param)
        {
            if (param.ToString() == "start")
            {
                SOEResponseModel response = await Utils.HTTPRequest.QuerySOE(SOEStartArgs);
                if (response != null)
                {
                    if (Utils.CheckObject.HasBeenUpdated(SOEEndResponse))
                    {
                        if (SOEStartResponse.Route == SOEEndResponse.Route)
                        {
                            CopyProps.CopyProperties(response, SOEStartResponse);
                            //createLine
                        }
                        else
                        {
                            MessageBox.Show("Start and end points must be on the same route.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        } 
                    }
                    else
                    {
                        CopyProps.CopyProperties(response, SOEStartResponse);
                    }
                }
                else
                {
                    MessageBox.Show("Could not find nearest route.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            else if (param.ToString() == "end")
            {
                SOEResponseModel response = await Utils.HTTPRequest.QuerySOE(SOEEndArgs);
                if (response != null)
                {
                    if (Utils.CheckObject.HasBeenUpdated(SOEStartResponse))
                    {
                        if (SOEStartResponse.Route == SOEEndResponse.Route)
                        {
                            CopyProps.CopyProperties(response, SOEEndResponse);
                            //create line
                        }
                        else
                        {
                            MessageBox.Show("Start and end points must be on the same route.","Error",MessageBoxButton.OK,MessageBoxImage.Exclamation);
                        }
                    }
                    else
                    {
                        CopyProps.CopyProperties(response, SOEEndResponse);
                    }
                }
                else
                {
                    MessageBox.Show("Could not find nearest route.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                } 
            }
        }
        public void SaveLineResult(object state)
        {
            if (Utils.CheckObject.HasBeenUpdated(SOEStartResponse)&& Utils.CheckObject.HasBeenUpdated(SOEEndResponse)) {
               // SoeLineResponses.Add({ SOEStartResponse, SOEEndResponse});
            }
        }
    }
}
