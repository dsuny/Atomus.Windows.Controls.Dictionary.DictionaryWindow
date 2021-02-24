using Atomus.Database;
using Atomus.Service;
using Atomus.Windows.Controls.Dictionary.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Atomus.Windows.Controls.Dictionary.Controllers
{
    internal static class DictionaryWindowController
    {
        internal static async Task<IResponse> SearchAsync(this ICore core, WindowsControlSearchModel search, ICore parentCore)
        {
            IServiceDataSet serviceDataSet;
            IResponse result;
            string tmp;

            serviceDataSet = new ServiceDataSet
            {
                ServiceName = core.GetAttribute("ServiceName"),
                TransactionScope = false
            };

            if (parentCore == null)
                serviceDataSet["Dictionary"].ConnectionName = core.GetAttribute("DatabaseName");
            else
            {
                serviceDataSet["Dictionary"].ConnectionName = parentCore.GetAttribute(core.GetAttribute("DictionaryConnectionName"));
                serviceDataSet["Dictionary"].ConnectionName = serviceDataSet["Dictionary"].ConnectionName.IsNullOrEmpty() ? core.GetAttribute("DatabaseName") : serviceDataSet["Dictionary"].ConnectionName;
            }

            serviceDataSet["Dictionary"].CommandText = core.GetAttribute("ProcedureExec");
            serviceDataSet["Dictionary"].AddParameter("@CODE", DbType.NVarChar, 50);
            serviceDataSet["Dictionary"].AddParameter("@SEARCH_TEXT", DbType.NVarChar, 4000);
            serviceDataSet["Dictionary"].AddParameter("@SEARCH_INDEX", DbType.Int, 0);
            serviceDataSet["Dictionary"].AddParameter("@COND_ETC", DbType.NVarChar, 4000);
            serviceDataSet["Dictionary"].AddParameter("@SEARCH_ALL", DbType.NVarChar, 1);
            serviceDataSet["Dictionary"].AddParameter("@STARTS_WITH", DbType.NVarChar, 1);

            serviceDataSet["Dictionary"].NewRow();
            serviceDataSet["Dictionary"].SetValue("@CODE", search.CODE);
            serviceDataSet["Dictionary"].SetValue("@SEARCH_TEXT", search.SEARCH_TEXT);
            serviceDataSet["Dictionary"].SetValue("@SEARCH_INDEX", search.SEARCH_INDEX);
            serviceDataSet["Dictionary"].SetValue("@COND_ETC", search.COND_ETC);
            serviceDataSet["Dictionary"].SetValue("@SEARCH_ALL", search.SEARCH_ALL);
            serviceDataSet["Dictionary"].SetValue("@STARTS_WITH", search.STARTS_WITH);

            result = await core.ServiceRequestAsync(serviceDataSet);

            if (result.Status == Status.OK)
            {
                if (result.DataSet.Tables.Count >= 1)
                {
                    foreach (System.Data.DataColumn _DataColumn in result.DataSet.Tables[0].Columns)
                    {
                        if (_DataColumn.ColumnName.Contains('^'))
                        {
                            tmp = _DataColumn.ColumnName;
                            _DataColumn.ColumnName = tmp.Split('^')[0];
                            _DataColumn.Caption = tmp;
                        }
                    }
                }
            }

            return result;
        }
        internal static IResponse Search(this ICore core, WindowsControlSearchModel search, ICore parentCore)
        {
            IServiceDataSet serviceDataSet;
            IResponse result;
            string tmp;

            serviceDataSet = new ServiceDataSet
            {
                ServiceName = core.GetAttribute("ServiceName"),
                TransactionScope = false
            };

            if (parentCore == null)
                serviceDataSet["Dictionary"].ConnectionName = core.GetAttribute("DatabaseName");
            else
            {
                serviceDataSet["Dictionary"].ConnectionName = parentCore.GetAttribute(core.GetAttribute("DictionaryConnectionName"));
                serviceDataSet["Dictionary"].ConnectionName = serviceDataSet["Dictionary"].ConnectionName.IsNullOrEmpty() ? core.GetAttribute("DatabaseName") : serviceDataSet["Dictionary"].ConnectionName;
            }

            serviceDataSet["Dictionary"].CommandText = core.GetAttribute("ProcedureExec");
            serviceDataSet["Dictionary"].AddParameter("@CODE", DbType.NVarChar, 50);
            serviceDataSet["Dictionary"].AddParameter("@SEARCH_TEXT", DbType.NVarChar, 4000);
            serviceDataSet["Dictionary"].AddParameter("@SEARCH_INDEX", DbType.Int, 0);
            serviceDataSet["Dictionary"].AddParameter("@COND_ETC", DbType.NVarChar, 4000);
            serviceDataSet["Dictionary"].AddParameter("@SEARCH_ALL", DbType.NVarChar, 1);
            serviceDataSet["Dictionary"].AddParameter("@STARTS_WITH", DbType.NVarChar, 1);

            serviceDataSet["Dictionary"].NewRow();
            serviceDataSet["Dictionary"].SetValue("@CODE", search.CODE);
            serviceDataSet["Dictionary"].SetValue("@SEARCH_TEXT", search.SEARCH_TEXT);
            serviceDataSet["Dictionary"].SetValue("@SEARCH_INDEX", search.SEARCH_INDEX);
            serviceDataSet["Dictionary"].SetValue("@COND_ETC", search.COND_ETC);
            serviceDataSet["Dictionary"].SetValue("@SEARCH_ALL", search.SEARCH_ALL);
            serviceDataSet["Dictionary"].SetValue("@STARTS_WITH", search.STARTS_WITH);

            result = core.ServiceRequest(serviceDataSet);

            if (result.Status == Status.OK)
            {
                if (result.DataSet.Tables.Count >= 1)
                {
                    foreach (System.Data.DataColumn _DataColumn in result.DataSet.Tables[0].Columns)
                    {
                        if (_DataColumn.ColumnName.Contains('^'))
                        {
                            tmp = _DataColumn.ColumnName;
                            _DataColumn.ColumnName = tmp.Split('^')[0];
                            _DataColumn.Caption = tmp;
                        }
                    }
                }
            }

            return result;
        }
    }
}