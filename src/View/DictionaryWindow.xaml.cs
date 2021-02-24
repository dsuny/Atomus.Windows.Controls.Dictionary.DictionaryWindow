using Atomus.Control;
using Atomus.Control.Dictionary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Atomus.Windows.Controls.Dictionary
{
    /// <summary>
    /// DictionaryWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DictionaryWindow : Window, IDictionary, IAction
    {
        private AtomusControlEventHandler beforeActionEventHandler;
        private AtomusControlEventHandler afterActionEventHandler;

        string IDictionary.Code { get; set; }
        string IDictionary.SearchText { get; set; }
        int IDictionary.SearchIndex { get; set; }
        string IBeforeEventArgs.Where { get; set; }
        bool IBeforeEventArgs.SearchAll { get; set; }
        bool IBeforeEventArgs.StartsWith { get; set; }
        DataRow IAfterEventArgs.DataRow { get; set; }
        DataTable IAfterEventArgs.DataTable { get; set; }
        BeforeAction IDictionary.BeforeAction { get; set; }
        AfterAction IDictionary.AfterAction { get; set; }

        #region Init
        public DictionaryWindow()
        {
            InitializeComponent();

            this.DataContext = new ViewModel.DictionaryWindowViewModel(this);
        }
        #endregion

        #region IO
        object IAction.ControlAction(ICore sender, AtomusControlArgs e)
        {
            try
            {
                this.beforeActionEventHandler?.Invoke(this, e);

                switch (e.Action)
                {
                    case "Selected":
                        this.DictionaryWindow_Deactivated(this, null);
                        return true;

                    case "ADD_COMBO_ITEM":
                        if(e.Value == null)
                        { 
                            (this.DataContext as ViewModel.DictionaryWindowViewModel).SearchSyncCommand.Execute(null);
                            if ((this as IDictionary).AfterAction != null)
                            {
                                (this as IDictionary).DataRow = (this as IDictionary).DataTable.Rows[0];
                                (this as IDictionary).AfterAction.Invoke(null, (this as IDictionary));
                            }
                        }
                        else if(e.Value is List<object>)
                        {
                            // [0]: Code                (string)
                            // [1]: SearchIndex         (int)
                            // [2]: SearchText          (string)
                            // [3]: Target              (ComboBox)
                            // [4]: DisplayMemberPath   (string)
                            // [5]: SelectedValuePath   (string)

                            List<object> data = e.Value as List<object>;
                            (this as IDictionary).Code = data[0].ToString();
                            (this as IDictionary).SearchIndex = data[1].ToString().ToInt();
                            (this as IDictionary).SearchText = data[2].ToString();
                            ComboBox combo = data[3] as ComboBox;
                            combo.DisplayMemberPath = data[4].ToString();
                            combo.SelectedValuePath = data[5].ToString();

                            (this.DataContext as ViewModel.DictionaryWindowViewModel).SearchSyncCommand.Execute(null);                            
                            combo.ItemsSource = (this as IDictionary).DataTable.DefaultView;                            
                        }
                        return true;

                    default:
                        throw new AtomusException("'{0}'은 처리할 수 없는 Action 입니다.".Translate(e.Action));
                }
            }
            finally
            {
                this.afterActionEventHandler?.Invoke(this, e);
            }
        }
        #endregion

        #region Event
        event AtomusControlEventHandler IAction.BeforeActionEventHandler
        {
            add
            {
                this.beforeActionEventHandler += value;
            }
            remove
            {
                this.beforeActionEventHandler -= value;
            }
        }
        event AtomusControlEventHandler IAction.AfterActionEventHandler
        {
            add
            {
                this.afterActionEventHandler += value;
            }
            remove
            {
                this.afterActionEventHandler -= value;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            (this.DataContext as ViewModel.DictionaryWindowViewModel).SearchCommand.Execute(null);
            this.SearchText.Focus();
        }

        public void AdjustPosition()
        {
            try
            {
                int cnt = (this as IDictionary).DataTable.Rows.Count;
                int height = (cnt * 25) + 90 > 590 ? 610 : (cnt * 25) + 110;

                if (this.Top + height > Application.Current.MainWindow.ActualHeight)
                {
                    this.Top = this.Top - height;
                    if (this.Top < 0)
                        this.Top = 0;
                }

                if (this.Left + (this.DataContext as ViewModel.DictionaryWindowViewModel).WidthCalculated > Application.Current.MainWindow.ActualWidth)
                {
                    this.Left = Application.Current.MainWindow.ActualWidth - (this.DataContext as ViewModel.DictionaryWindowViewModel).WidthCalculated - 20;
                    if (this.Left < 0)
                        this.Left = 0;
                }
            }
            catch (Exception ex)
            {
                Diagnostics.DiagnosticsTool.MyTrace(ex);
            }
        }

        private void DictionaryWindow_Deactivated(object sender, EventArgs e)
        {
            if (this.IsVisible)
                this.Close();
        }
        #endregion

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            DataTable dataTable;

            if (sender is DataGrid && (sender as DataGrid).ItemsSource != null && (sender as DataGrid).ItemsSource is DataView)
            {
                dataTable = ((sender as DataGrid).ItemsSource as DataView).Table;

                foreach (DataColumn dataColumn in dataTable.Columns)
                {
                    if (dataColumn.Caption.Contains('^') && dataColumn.ColumnName == (e.Column.Header as string))
                        e.Column.Width = new DataGridLength(dataColumn.Caption.Split('^')[1].ToDouble());

                    // 헤더에 부적합 문자
                    if (dataColumn.ColumnName.Contains('/'))
                        dataColumn.ColumnName = dataColumn.ColumnName.Replace('/', '_');
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
