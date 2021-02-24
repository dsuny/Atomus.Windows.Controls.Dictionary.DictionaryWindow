using Atomus.Control;
using Atomus.Diagnostics;
using Atomus.Service;
using Atomus.Windows.Controls.Dictionary.Controllers;
using Atomus.Windows.Controls.Dictionary.Models;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Atomus.Windows.Controls.Dictionary.ViewModel
{
    public class DictionaryWindowViewModel : Atomus.MVVM.ViewModel
    {
        #region Declare
        private double _WIdthCalculated;

        public double WidthCalculated
        {
            get { return _WIdthCalculated; }
            set
            {
                if (this._WIdthCalculated != value)
                {
                    this._WIdthCalculated = value; this.NotifyPropertyChanged();
                }
            }
        }

        private string _TitleText;

        public string TitleText
        {
            get { return _TitleText; }
            set
            {
                if (this._TitleText != value)
                {
                    this._TitleText = value; this.NotifyPropertyChanged();
                }
            }
        }

        private bool _IsSearchAll;

        public bool IsSearchAll
        {
            get { return _IsSearchAll; }
            set
            {
                if (this._IsSearchAll != value)
                {
                    this._IsSearchAll = value; this.NotifyPropertyChanged();

                    // 체크시 전체 검색
                    if (IsSearchAll)
                    {
                        SearchText = "";
                        SearchCommand.Execute(null);
                    }
                }
            }
        }

        private bool isEnabledControl;

        public bool IsEnabledControl
        {
            get
            {
                return this.isEnabledControl;
            }
            set
            {
                if (this.isEnabledControl != value)
                {
                    this.isEnabledControl = value; this.NotifyPropertyChanged();
                }
            }
        }

        private DataView _CurrentDataView;

        public DataView CurrentDataView
        {
            get { return _CurrentDataView; }
            set
            {
                if (this._CurrentDataView != value)
                {
                    this._CurrentDataView = value; this.NotifyPropertyChanged();
                }
            }
        }

        private DataRowView _SelectedDataRowView;

        public DataRowView SelectedDataRowView
        {
            get { return _SelectedDataRowView; }
            set
            {
                if (this._SelectedDataRowView != value)
                {
                    this._SelectedDataRowView = value; this.NotifyPropertyChanged();
                }
            }
        }
        #endregion

        #region Property
        public ICore Core { get; set; }
        public ICore ParentCore { get; set; }

        public string SearchText
        {
            get
            {
                if (this.Core != null)
                    return (this.Core as IDictionary).SearchText;
                else
                    return null;
            }
            set
            {
                if ((this.Core as IDictionary).SearchText != value)
                {
                    (this.Core as IDictionary).SearchText = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public ICommand SearchCommand { get; set; }
        public ICommand LeftDoubleClickCommand { get; set; }
        public ICommand SearchSyncCommand { get; set; }
        #endregion

        #region INIT
        public DictionaryWindowViewModel()
        {
            this.isEnabledControl = true;

            this.SearchCommand = new MVVM.DelegateCommand(async () => { await this.SearchProcess(); }
                                                            , () => { return this.isEnabledControl; });

            this.LeftDoubleClickCommand = new MVVM.DelegateCommand(() => { this.LeftDoubleClickProcess(); }
                                                            , () => { return this.isEnabledControl; });

            this.SearchSyncCommand = new MVVM.DelegateCommand(() => { this.SearchSyncProcess(); }
                                                            , () => { return this.isEnabledControl; });

        }
        public DictionaryWindowViewModel(ICore core) : this()
        {
            this.Core = core;
        }
        #endregion

        #region IO
        private async Task SearchProcess()
        {
            //System.Diagnostics.Trace.WriteLine("LEFT: " +(this.Core as Window).Left + " TOP: " + (this.Core as Window).Top);

            IDictionary dictionary;
            IResponse result;
            string[] tmps;
            double tmpDouble;

            dictionary = null;

            try
            {
                this.IsEnabledControl = false;
                (this.SearchCommand as Atomus.MVVM.DelegateCommand).RaiseCanExecuteChanged();
                (this.Core as DictionaryWindow).Cursor = Cursors.Wait;

                dictionary = (this.Core as IDictionary);

                if (dictionary.BeforeAction != null && !dictionary.BeforeAction.Invoke(null, dictionary))
                    return;

                result = await this.Core.SearchAsync(new WindowsControlSearchModel()
                {
                    CODE = dictionary.Code,
                    SEARCH_TEXT = dictionary.SearchText,
                    SEARCH_INDEX = dictionary.SearchIndex + 1,
                    COND_ETC = dictionary.Where,
                    SEARCH_ALL = dictionary.SearchAll ? "Y" : "N",
                    STARTS_WITH = dictionary.StartsWith ? "Y" : "N"
                }, this.ParentCore);

                if (result.Status == Status.OK)
                {
                    if (result.DataSet.Tables.Count >= 1)
                    {
                        tmpDouble = 30;
                        foreach (DataColumn _DataColumn in result.DataSet.Tables[0].Columns)
                        {
                            if (_DataColumn.Caption.Contains('^'))
                            {
                                tmps = _DataColumn.Caption.Split('^');
                                tmpDouble += tmps[1].ToDouble();
                            }
                        }

                        if (tmpDouble < 70)
                            tmpDouble = 70;

                        this.CurrentDataView = result.DataSet.Tables[0].DefaultView;

                        dictionary.DataTable = this.CurrentDataView.Table;

                        this.SetForm(tmpDouble);

                        if ((this.Core as Window).Tag == null)
                        {
                            this.TitleText = dictionary.Code.ToString() +
                            " (" + CurrentDataView.Table.Rows.Count + "/" + CurrentDataView.Table.Rows.Count + ")";
                        }
                        else
                        {
                            this.TitleText = (this.Core as Window).Tag.ToString() +
                            " (" + CurrentDataView.Table.Rows.Count + "/" + CurrentDataView.Table.Rows.Count + ")";
                        }

                        (this.Core as DictionaryWindow).AdjustPosition();
                    }

                    return;
                }
                else
                    throw new AtomusException(result.Message);

            }
            catch (Exception ex)
            {
                this.WindowsMessageBoxShow(Application.Current.Windows[0], ex);
            }
            finally
            {
                this.IsEnabledControl = true;
                (this.SearchCommand as Atomus.MVVM.DelegateCommand).RaiseCanExecuteChanged();
                (this.Core as DictionaryWindow).Cursor = Cursors.Arrow;
            }
        }

        private void LeftDoubleClickProcess()
        {
            IDictionary dictionary;

            try
            {
                (this.LeftDoubleClickCommand as Atomus.MVVM.DelegateCommand).RaiseCanExecuteChanged();

                if (this.SelectedDataRowView != null)
                {
                    dictionary = (this.Core as IDictionary);

                    dictionary.DataRow = this.SelectedDataRowView.Row;

                    if (dictionary.AfterAction != null && dictionary.AfterAction.Invoke(null, dictionary))
                    {
                    }
                    else
                    {
                        dictionary.DataRow = null;
                    }

                    (this.Core as IAction).ControlAction(this.Core, "Selected", null);
                }
            }
            catch (Exception ex)
            {
                this.WindowsMessageBoxShow(Application.Current.Windows[0], ex);
            }
            finally
            {
                this.IsEnabledControl = true;
                (this.LeftDoubleClickCommand as Atomus.MVVM.DelegateCommand).RaiseCanExecuteChanged();
            }
        }

        private void SearchSyncProcess()
        {
            IDictionary dictionary;
            IResponse result;
            string[] tmps;
            double tmpDouble;
            dictionary = null;

            try
            {
                this.IsEnabledControl = false;
                (this.SearchCommand as Atomus.MVVM.DelegateCommand).RaiseCanExecuteChanged();

                dictionary = (this.Core as IDictionary);

                if (dictionary.BeforeAction != null && !dictionary.BeforeAction.Invoke(null, dictionary))
                    return;

                result = this.Core.Search(new WindowsControlSearchModel()
                {
                    CODE = dictionary.Code,
                    SEARCH_TEXT = dictionary.SearchText,
                    SEARCH_INDEX = dictionary.SearchIndex + 1,
                    COND_ETC = dictionary.Where,
                    SEARCH_ALL = dictionary.SearchAll ? "Y" : "N",
                    STARTS_WITH = dictionary.StartsWith ? "Y" : "N"
                }, this.ParentCore);

                if (result.Status == Status.OK)
                {
                    if (result.DataSet.Tables.Count >= 1)
                    {
                        tmpDouble = 30;
                        foreach (DataColumn _DataColumn in result.DataSet.Tables[0].Columns)
                        {
                            if (_DataColumn.Caption.Contains('^'))
                            {
                                tmps = _DataColumn.Caption.Split('^');
                                tmpDouble += tmps[1].ToDouble();
                            }
                        }

                        this.CurrentDataView = result.DataSet.Tables[0].DefaultView;

                        dictionary.DataTable = this.CurrentDataView.Table;
                    }

                    return;
                }
                else
                    throw new AtomusException(result.Message);

            }
            catch (Exception ex)
            {
                this.WindowsMessageBoxShow(Application.Current.Windows[0], ex);
            }
            finally
            {
                this.IsEnabledControl = true;
                (this.SearchCommand as Atomus.MVVM.DelegateCommand).RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region ETC
        private void SetForm(double tmpWidth)
        {
            IDictionary dictionary;

            try
            {
                dictionary = (this as IDictionary);

                WidthCalculated = tmpWidth;
            }
            catch (Exception exception)
            {
                DiagnosticsTool.MyTrace(exception);
            }
        }
        #endregion
    }
}